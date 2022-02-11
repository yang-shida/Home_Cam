using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home_Cam_Backend.BackgroundTasks;
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
            // Extensions.WriteToLogFile("Find cameras from web api.");
            List<Esp32Cam> camList = await Esp32Cam.FindCameras(repository);

            return ActiveCameras.Select(cam=>cam.AsDto()).ToList();
        }

        [HttpGet("{camId}")]
        public async Task Streaming(string camId)
        {
            int camIndex = ActiveCameras.FindIndex(camInList => camInList.UniqueId==camId);
            if(camIndex == -1)
            {
                Response.StatusCode = 404;
                return;
            }
            Esp32Cam cam = ActiveCameras[camIndex];

            Response.StatusCode = 206;
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            Response.Headers.Add("Connection", "Keep-Alive");

            while(!Request.HttpContext.RequestAborted.IsCancellationRequested)
            {

                int imageIndex = cam.ImageBufferHeadIndex;

                if(cam.ImageBuffer[imageIndex].valid == false)
                {
                    Response.StatusCode = 404;
                    return;
                }

                var delay = Task.Delay(100);

                byte[] image = cam.ImageBuffer[imageIndex].image;

                string header =
                    "--frame" + "\r\n" +
                    "Content-Type:image/jpeg\r\n" +
                    "Content-Length:" + image.Length + "\r\n\r\n";

                string footer = "\r\n";

                await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(header));
                await Response.Body.WriteAsync(image);
                await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(footer));
                await Response.Body.FlushAsync();

                await delay;

            }

            return;
        }

        [HttpGet("{camId}/preview")]
        public async Task<ActionResult<byte[]>> GetPreviewImage(string camId)
        {
            
            int camIndex = ActiveCameras.FindIndex(camInList => camInList.UniqueId==camId);
            
            // this cam is not active
            if(camIndex==-1)
            {
                return NotFound();
            }

            // no valid image
            if(!ActiveCameras[camIndex].ImageBuffer[ActiveCameras[camIndex].ImageBufferHeadIndex].valid)
            {
                return NotFound();
            }

            Response.ContentType="image/jpeg";
            await Response.Body.WriteAsync(ActiveCameras[camIndex].ImageBuffer[ActiveCameras[camIndex].ImageBufferHeadIndex].image);

            return Ok();
        }
    }
}