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

        // [HttpGet("{camId}")]
        // public async Task<IActionResult> Streaming(string camId)
        // {
        //     int camIndex = ActiveCameras.FindIndex(camInList => camInList.UniqueId==camId);
        //     if(camIndex == -1)
        //     {
        //         return NotFound();
        //     }

        //     var contentType = "multipart/x-mixed-replace;boundary=123456789000000000000987654321";
        //     Stream stream = ImageCaptureBackgroundTask.ActiveStreams[camIndex];
        //     var result = new FileStreamResult(stream, contentType) {
        //         EnableRangeProcessing = true
        //     };
        //     return result;

        //     // Response.StatusCode = 206;
        //     // Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
        //     // Response.Headers.Add("Connection", "Keep-Alive");

        //     // while(!Request.HttpContext.RequestAborted.IsCancellationRequested)
        //     // {

        //     //     byte[] image = cam.ImageBuffer[cam.ImageBufferHeadIndex];

        //     //     string header =
        //     //         "--frame" + "\r\n" +
        //     //         "Content-Type:image/jpeg\r\n" +
        //     //         "Content-Length:" + image.Length + "\r\n\r\n";

        //     //     string footer = "\r\n";

        //     //     await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(header));
        //     //     await Response.Body.WriteAsync(image);
        //     //     await Response.Body.WriteAsync(Encoding.ASCII.GetBytes(footer));
        //     //     await Response.Body.FlushAsync();

        //     // }

        //     // await Response.StartAsync();
        // }

    }
}