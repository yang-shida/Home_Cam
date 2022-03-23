using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Home_Cam_Backend.BackgroundTasks;
using Home_Cam_Backend.Controllers;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Home_Cam_Backend
{
    public class Esp32Cam
    {
        public enum ParsingStatus : ushort
        {
            LookingForLengthAndTime = 0,
            GettingData = 1,
            OutlierReading = 200
        }
        public string IpAddr { get; set; }
        public string UniqueId { get; init; }
        private HttpClient httpClient = new();

        public DateTimeOffset DiscoverTimeServer { get; set; }
        public long DiscoverTimeCameraMilliseconds { get; set; }

        public List<BufferedImage> ImageBuffer { get; set; }
        public int CurrentImageByteIndex { get; set; }
        public int ImageBufferHeadIndex { get; set; }
        public static readonly int ImageBufferMaxSize = 20;
        public Stream CamStream { get; set; }
        public byte[] StreamBuffer { get; set; }
        public int RemainingData { get; set; }
        public static int StreamBufferSize = 1024 * 1024;
        public ParsingStatus MyParsingStatus { get; set; }
        public static int MaxRecoverTimeout = 30;
        public int RecoverTimeout { get; set; }

        public Esp32Cam(string ip, string id, long cameraTimeMicroSeconds)
        {
            IpAddr = ip;
            UniqueId = id;
            ImageBufferHeadIndex = 0;
            CurrentImageByteIndex = 0;
            ImageBuffer = new();
            for (int i = 0; i < ImageBufferMaxSize; i++)
            {
                ImageBuffer.Add(new BufferedImage());
            }
            StreamBuffer = new byte[StreamBufferSize];
            RemainingData = 0;
            MyParsingStatus = ParsingStatus.LookingForLengthAndTime;

            DiscoverTimeServer = new DateTimeOffset(DateTime.UtcNow);
            DiscoverTimeCameraMilliseconds = cameraTimeMicroSeconds / 1000;
            RecoverTimeout = MaxRecoverTimeout;
        }
        public async Task AdjustFrameSize(int newFrameSizeCode)
        {
            try
            {
                await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=framesize&val={newFrameSizeCode}");
            }
            catch
            {
                throw new Exception("[AdjustFrameSize] Cannot talk to camera!");
            }
        }

        public async Task TurnOnFlash(bool flashOn)
        {
            try
            {
                await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=flash&val={(flashOn ? 1 : 0)}");
            }
            catch
            {
                throw new Exception("[TurnOnFlash] Cannot talk to camera!");
            }
        }

        public async Task HorizontalMirror(bool mirrored)
        {
            HttpResponseMessage res = null;
            try
            {
                res = await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=hmirror&val={(mirrored ? 1 : 0)}");

            }
            catch
            {
                throw new Exception("[HorizontalMirror] Cannot talk to camera!");
            }
        }

        public async Task VerticalMirror(bool mirrored)
        {
            try
            {
                await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=vflip&val={(mirrored ? 1 : 0)}");
            }
            catch
            {
                throw new Exception("[VerticalMirror] Cannot talk to camera!");
            }
        }

        public async Task<byte[]> GetSingleShot()
        {
            return await Task.FromResult(ImageBuffer[ImageBufferHeadIndex].valid ? ImageBuffer[ImageBufferHeadIndex].image : null);
        }

        // http://192.168.1.105:81/esp32_cam_stream
        public async Task<Stream> Streaming()
        {
            try
            {
                Stream imageResult = await httpClient.GetStreamAsync($"http://{IpAddr}:81/esp32_cam_stream");
                CamStream = imageResult;
                return imageResult;
            }
            catch
            {
                throw new Exception("[Streaming] Cannot talk to camera!");
            }
        }

        public async Task UpdateAllSettings(EEsp32CamSetting camSetting)
        {
            await AdjustFrameSize(camSetting.FrameSize);
            await TurnOnFlash(camSetting.FlashLightOn);
            await HorizontalMirror(camSetting.HorizontalMirror);
            await VerticalMirror(camSetting.VerticalMirror);
        }
        public static async Task<List<Esp32Cam>> FindCameras(ICamSettingsRepository repository)
        {
            List<Esp32Cam> cameraList = new();

            // find gateway address and subnet mask
            List<string> ipAddrList = await Home_Cam_Backend.Extensions.getListOfSubnetIpAddresses(false);
            if(ipAddrList.Count==0)
            {
                return await Task.FromResult(cameraList);
            }
            
            HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(2);
            client.DefaultRequestHeaders.ConnectionClose = true;
            List<Task<HttpResponseMessage>> requestList = new();

            for (int i = 0; i < ipAddrList.Count; i++)
            {
                string CAM_IP = ipAddrList[i];
                if (CamController.ActiveCameras.Find(camInList => camInList.IpAddr == CAM_IP) is null)
                {
                    requestList.Add(client.GetAsync($"http://{CAM_IP}/who_are_you"));
                }
                else
                {
                    ipAddrList.Remove(CAM_IP);
                    i--;
                }
            }

            for (int i = 0; i < requestList.Count; i++)
            {
                var req = requestList[i];
                var ipAddr = ipAddrList[i];
                try
                {
                    await req;
                    var idResult = req.Result;


                    if (idResult.StatusCode.ToString() == "OK")
                    {
                        string responseString = await idResult.Content.ReadAsStringAsync();
                        if (responseString.StartsWith("ESP32="))
                        {
                            var cam = new Esp32Cam(ipAddr, responseString.Substring(6, 17), long.Parse(responseString.Substring(24)));
                            await cam.Streaming();
                            if (CamController.ActiveCameras.Find(camInList => camInList.UniqueId == cam.UniqueId) is null)
                            {
                                cameraList.Add(cam);

                                var camSetting = await repository.GetCamSettingAsync(cam.UniqueId);
                                // if the camera's setting is in the database
                                if (camSetting is not null)
                                {
                                    // update the camera's setting
                                    await cam.UpdateAllSettings(camSetting);
                                }
                                // camera's setting is not in the databse
                                else
                                {
                                    // create a default camera setting and update the camera setting
                                    camSetting = new()
                                    {
                                        UniqueId = cam.UniqueId,
                                        Location = "Default Location",
                                        FrameSize = 10,
                                        FlashLightOn = false,
                                        HorizontalMirror = false,
                                        VerticalMirror = false
                                    };
                                    await cam.UpdateAllSettings(camSetting);
                                    await repository.CreateCamSettingAsync(camSetting);
                                }

                                // add the camera to active list
                                CamController.ActiveCameras.Add(cam);
                                Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] FindCameras with MAC = {cam.UniqueId} and IP = {cam.IpAddr}");
                            }

                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.GetType().ToString() != "System.Threading.Tasks.TaskCanceledException")
                    {
                        // Console.WriteLine(e.ToString());
                    }
                }

            }

            if (cameraList.Count == 0)
            {
                Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] FindCameras: No new camera found.");
            }

            Extensions.WriteToLogFile("---------------- Current Active Cameras ----------------");
            int count = 1;
            foreach (Esp32Cam cam in CamController.ActiveCameras)
                Extensions.WriteToLogFile($"[{count++}] MAC = {cam.UniqueId} and IP = {cam.IpAddr}");
            Extensions.WriteToLogFile("--------------------------------------------------------");

            client.Dispose();

            return await Task.FromResult(cameraList);
        }
    }
}