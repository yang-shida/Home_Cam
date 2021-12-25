using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;

namespace Home_Cam_Backend
{
    public class Esp32Cam
    {
        public string IpAddr { get; set; }
        public string UniqueId { get; init; }
        private HttpClient httpClient = new();
        public Esp32Cam(string ip, string id)
        {
            IpAddr = ip;
            UniqueId = id;
        }
        public async Task AdjustFrameSize(int newFrameSizeCode)
        {
            try
            {
                await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=framesize&val={newFrameSizeCode}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception("[AdjustFrameSize] Cannot talk to camera!");
            }
        }

        public async Task TurnOnFlash(bool flashOn)
        {
            try
            {
                await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=flash&val={(flashOn?1:0)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception("[TurnOnFlash] Cannot talk to camera!");
            }
        }

        public async Task HorizontalMirror(bool mirrored)
        {
            HttpResponseMessage res = null;
            try
            {
                res = await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=hmirror&val={(mirrored?1:0)}");
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception("[HorizontalMirror] Cannot talk to camera!");
            }
        }

        public async Task VerticalMirror(bool mirrored)
        {
            try
            {
                await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_control?var=vflip&val={(mirrored?1:0)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception("[VerticalMirror] Cannot talk to camera!");
            }
        }

        public async Task<byte[]> GetSingleShot()
        {
            try
            {
                HttpResponseMessage imageResult = await httpClient.GetAsync($"http://{IpAddr}/esp32_cam_capture?cb={DateTime.Now.Ticks}");
                byte[] imageArray = await imageResult.Content.ReadAsByteArrayAsync();
                return await Task.FromResult(imageArray);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception("[GetSingleShot] Cannot talk to camera!");
            }
        }

        public async Task UpdateAllSettings(EEsp32CamSetting camSetting)
        {
            try
            {
                await AdjustFrameSize(camSetting.FrameSize);
                await TurnOnFlash(camSetting.FlashLightOn);
                await HorizontalMirror(camSetting.HorizontalMirror);
                await VerticalMirror(camSetting.VerticalMirror);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        public static async Task<List<Esp32Cam>> FindCameras()
        {
            // find gateway address and subnet mask
            List<string> ipAddrList = await Home_Cam_Backend.Extensions.getListOfSubnetIpAddresses();

            List<Esp32Cam> cameraList = new();
            HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(2);
            List<Task<HttpResponseMessage>> requestList = new();

            foreach (string CAM_IP in ipAddrList)
            {
                requestList.Add(client.GetAsync($"http://{CAM_IP}/who_are_you"));
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
                            cameraList.Add(new(ipAddr, responseString.Substring(6)));
                        }
                    }
                }
                catch (Exception)
                {

                }

            }

            return await Task.FromResult(cameraList);
        }
    }
}