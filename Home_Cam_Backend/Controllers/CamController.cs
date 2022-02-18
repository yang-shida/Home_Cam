using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Home_Cam_Backend.BackgroundTasks;
using Home_Cam_Backend.Dtos;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Home_Cam_Backend.Controllers
{
    [ApiController]
    [Route("api/cam")]
    public class CamController : ControllerBase
    {
        private readonly ICamSettingsRepository repository;
        private readonly ICapturedImagesRepository capturedImagesRepository;
        private readonly IConfiguration configuration;
        public static List<Esp32Cam> ActiveCameras = new();


        public CamController(ICamSettingsRepository repo, ICapturedImagesRepository capturedImagesRepository, IConfiguration configuration)
        {
            this.repository = repo;
            this.capturedImagesRepository = capturedImagesRepository;
            this.configuration=configuration;
        }

        [HttpGet]
        public async Task<ActionResult<List<CamDto>>> GetActiveCameras(bool needRescan = false)
        {
            // Extensions.WriteToLogFile("Find cameras from web api.");
            if (needRescan)
            {
                List<Esp32Cam> camList = await Esp32Cam.FindCameras(repository);
            }


            return ActiveCameras.Select(cam => cam.AsDto()).ToList();
        }

        [HttpGet("{camId}")]
        public async Task Streaming(string camId, long? startTimeUtc = null)
        {
            

            Response.StatusCode = 206;
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            Response.Headers.Add("Connection", "Keep-Alive");

            long? currentTimeUtc = startTimeUtc;

            long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            while (!Request.HttpContext.RequestAborted.IsCancellationRequested)
            {
                var delay = Task.Delay(100);
                if (startTimeUtc is null)
                {
                    int camIndex = ActiveCameras.FindIndex(camInList => camInList.UniqueId == camId);
                    if (camIndex == -1)
                    {
                        await Response.Body.WriteAsync(Encoding.ASCII.GetBytes("Cam not active."));
                        return;
                    }
                    Esp32Cam cam = ActiveCameras[camIndex];

                    int imageIndex = cam.ImageBufferHeadIndex;

                    if (cam.ImageBuffer[imageIndex].valid == false)
                    {
                        await Response.Body.WriteAsync(Encoding.ASCII.GetBytes("Cam image buffer invalid."));
                        return;
                    }

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

                   
                }
                else
                {
                    long currTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    // Console.WriteLine($"FPS: {1000/((double)(currTime-lastTime))}");
                    currentTimeUtc += (currTime-lastTime);
                    lastTime=currTime;
                    // fetch next image info from DB
                    // get -5 seconds
                    DateTimeOffset begin = DateTimeOffset.FromUnixTimeMilliseconds((long)currentTimeUtc - 5 * 1000);
                    DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds((long)currentTimeUtc);
                    List<ECapturedImageInfo> imageInfoList = await capturedImagesRepository.GetImageInfos(camId, begin, end);

                    // if no more image near the selected time
                    if (imageInfoList.Count == 0)
                    {
                        await Response.Body.WriteAsync(Encoding.ASCII.GetBytes("No more recordings."));
                        return;
                    }

                    string imagePath = imageInfoList.Last().ImageFileLocation;
                    byte[] image = await System.IO.File.ReadAllBytesAsync(imagePath);

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

                await delay;


            }

            return;
        }

        [HttpGet("{camId}/preview")]
        public async Task<ActionResult> GetPreviewImage(string camId)
        {

            int camIndex = ActiveCameras.FindIndex(camInList => camInList.UniqueId == camId);

            // this cam is not active
            if (camIndex == -1)
            {
                return NotFound();
            }

            // no valid image
            if (!ActiveCameras[camIndex].ImageBuffer[ActiveCameras[camIndex].ImageBufferHeadIndex].valid)
            {
                return NotFound();
            }

            Response.ContentType = "image/jpeg";
            await Response.Body.WriteAsync(ActiveCameras[camIndex].ImageBuffer[ActiveCameras[camIndex].ImageBufferHeadIndex].image);

            return new EmptyResult();
        }

        [HttpGet("{camId}/available_recording_time_intervals")]
        public async Task<ActionResult<List<TimeIntervalDto>>> GetAvailableRecordingTimeIntervals(string camId, long startTimeUtc, long timeLengthMillis)
        {
            long thresholdMillis = configuration.GetSection("CamControllerSettings").GetValue<long>("DistinctVideosThresholdSeconds")*1000;
            List<TimeIntervalDto> timeIntervals = await capturedImagesRepository.GetRecordedTimeIntervals(camId, startTimeUtc, timeLengthMillis, thresholdMillis);
            return timeIntervals;
        }
    }
}