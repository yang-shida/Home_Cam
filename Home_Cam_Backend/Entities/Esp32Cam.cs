using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Home_Cam_Backend.Entities
{
    public class Esp32Cam
    {
        public string IpAddr { get; set; }
        public string Location { get; set; }
        public static async Task<List<Esp32Cam>> FindCameras()
        {
            // find gateway address and subnet mask
            List<string> ipAddrList = await Home_Cam_Backend.Extensions.getListOfSubnetIpAddresses();

            List<Esp32Cam> cameraList=new();
            HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(2);
            List<Task<HttpResponseMessage>> requestList=new();

            foreach(string CAM_IP in ipAddrList)
            {
                requestList.Add(client.GetAsync($"http://{CAM_IP}/who_are_you"));
            }

            for(int i=0; i<requestList.Count; i++)
            {
                var req=requestList[i];
                var ipAddr=ipAddrList[i];
                try{
                    await req;
                    var idResult = req.Result;
                
                    if(idResult.StatusCode.ToString()=="OK")
                    {
                        string responseString = await idResult.Content.ReadAsStringAsync();
                        if(responseString=="ESP32\0")
                        {
                            cameraList.Add(new(){IpAddr=ipAddr});
                        }
                    }
                }
                catch(Exception)
                {
                    
                }
                
            }
                
            return await Task.FromResult(cameraList);
        }
    }
}