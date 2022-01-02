using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/cam")]
    public class CamController: ControllerBase
    {
        private readonly ICamSettingsRepository repository;
        public static List<Esp32Cam> ActiveCameras=new();

        public CamController(ICamSettingsRepository repo)
        {
            this.repository=repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<CamDto>>> GetActiveCameras()
        {
            List<Esp32Cam> camList = await Esp32Cam.FindCameras(repository);

            return ActiveCameras.Select(cam=>cam.AsDto()).ToList();
        }

        [HttpGet("{camId}")]
        public async Task Streaming(string camId)
        {
            Esp32Cam cam = ActiveCameras.Find(camInList => camInList.UniqueId==camId);
            if(cam is null)
            {
                return;
            }

            Response.StatusCode = 206;
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            Response.Headers.Add("Connection", "Keep-Alive");

            while(true)
            {
                if (Request.HttpContext.RequestAborted.IsCancellationRequested)
                {
                    break;
                }

                byte[] image = await cam.GetSingleShot();

                string header =
                    "--frame" + "\r\n" +
                    "Content-Type:image/jpeg\r\n" +
                    "Content-Length:" + image.Length + "\r\n\r\n";

                string footer = "\r\n";

                await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(header));
                await Response.Body.WriteAsync(image);
                await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(footer));
                await Response.Body.FlushAsync();

            }

            // await Response.StartAsync();
        }

    }
}