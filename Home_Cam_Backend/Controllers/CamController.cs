using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
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
        private readonly ICamSettingsRepository camSettingsRepository;
        private readonly IConfiguration configuration;
        public static List<Esp32Cam> ActiveCameras = new();


        public CamController(ICamSettingsRepository repo, ICapturedImagesRepository capturedImagesRepository, IConfiguration configuration, ICamSettingsRepository camSettingsRepository)
        {
            this.repository = repo;
            this.capturedImagesRepository = capturedImagesRepository;
            this.configuration = configuration;
            this.camSettingsRepository = camSettingsRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<CamDto>>> GetActiveCameras(bool needRescan = false)
        {
            // Extensions.WriteToLogFile("Find cameras from web api.");
            if (needRescan)
            {
                List<Esp32Cam> camList = await Esp32Cam.FindCameras(repository);
            }

            List<string> camIdList = await camSettingsRepository.GetCurrentCamIds();

            return (camIdList.Select(
                camId => {
                    Esp32Cam cam = ActiveCameras.Find(cam => cam.UniqueId == camId);
                    if(cam==null)
                    {
                        return new CamDto(){
                            UniqueId=camId,
                            IpAddr="N/A"
                        };
                    }
                    else
                    {
                        return new CamDto(){
                            UniqueId=camId,
                            IpAddr=cam.IpAddr
                        };
                    }
                }
            )).ToList();
        }

        [HttpGet("{camId}")]
        public async Task Streaming(string camId, long? startTimeUtc = null)
        {

            var response = Response;
            response.Headers.Add("Content-Type", "text/event-stream");

            long? currentTimeUtc = startTimeUtc;

            long lastTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            List<ECapturedImageInfo>[] imageInfoList = {new(), new()};
            int currList = 0;
            bool isFetchingImageInfo = false;

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

                    string imageBase64Str = Convert.ToBase64String(image);

                    await response.Body.WriteAsync(Encoding.ASCII.GetBytes($"data: {imageBase64Str}\n\n"));
                    await response.Body.FlushAsync();


                }
                else
                {
                    long currTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    // Console.WriteLine($"FPS: {1000 / ((double)(currTime - lastTime))}");
                    currentTimeUtc += (currTime - lastTime);
                    lastTime = currTime;

                    // just started
                    if(imageInfoList[currList].Count == 0)
                    {
                        // Console.WriteLine("Fetch first new batch");
                        // fetch next image info from DB
                        DateTimeOffset begin = DateTimeOffset.FromUnixTimeMilliseconds((long)currentTimeUtc);
                        DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds((long)currentTimeUtc + 15 * 1000);
                        imageInfoList[currList] = await capturedImagesRepository.GetImageInfos(camId, begin, end);
                        // if no more image near the selected time
                        if (imageInfoList[currList].Count == 0)
                        {
                            await Response.Body.WriteAsync(Encoding.ASCII.GetBytes("No more recordings."));
                            return;
                        }
                    }
                    // need to fetch next batch
                    else if(!isFetchingImageInfo && currentTimeUtc + 3000 > imageInfoList[currList].Last().CreatedDate.ToUnixTimeMilliseconds())
                    {
                        // fetch next image info from DB
                        // Console.WriteLine("Fetch new batch");
                        DateTimeOffset begin = DateTimeOffset.FromUnixTimeMilliseconds((long)currentTimeUtc + 3000);
                        DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds((long)currentTimeUtc + 3000 + 15 * 1000);
                        isFetchingImageInfo = true;
                        Task.Run(
                            async () => {
                                imageInfoList[1-currList] = await capturedImagesRepository.GetImageInfos(camId, begin, end);
                                // if(imageInfoList[1-currList].Count==0)
                                // {
                                //     Console.WriteLine("Nothing fetched");
                                // }
                                // Console.WriteLine("Fetch done");
                            }
                        );
                        
                    }

                    if (currentTimeUtc > imageInfoList[currList].Last().CreatedDate.ToUnixTimeMilliseconds())
                    {
                        isFetchingImageInfo = false;
                        currList = 1-currList;
                    }

                    // if no more image near the selected time
                    if (imageInfoList[currList].Count == 0)
                    {
                        // Console.WriteLine("Exiting");
                        return;
                    }

                    ECapturedImageInfo imageInfo = imageInfoList[currList].Find(
                        imgInfo => imgInfo.CreatedDate.ToUnixTimeMilliseconds() > currentTimeUtc
                    );

                    if (imageInfo != null)
                    {
                        string imagePath = imageInfo.ImageFileLocation;
                        byte[] image = await System.IO.File.ReadAllBytesAsync(imagePath);

                        string imageBase64Str = Convert.ToBase64String(image);

                        ControlledFrameDto frame = new()
                        {
                            TimeSinceStartMs = (long)currentTimeUtc - (long)startTimeUtc,
                            ImageBase64Str = imageBase64Str
                        };

                        await response.Body.WriteAsync(Encoding.ASCII.GetBytes($"data: {Newtonsoft.Json.JsonConvert.SerializeObject(frame)}\n\n"));
                        await response.Body.FlushAsync();
                    }



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
            long thresholdMillis = configuration.GetSection("CamControllerSettings").GetValue<long>("DistinctVideosThresholdSeconds") * 1000;
            List<TimeIntervalDto> timeIntervals = await capturedImagesRepository.GetRecordedTimeIntervals(camId, startTimeUtc, timeLengthMillis, thresholdMillis);
            return timeIntervals;
        }
    }
}