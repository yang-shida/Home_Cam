using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Net.Http;
using System.Runtime.CompilerServices;

using Home_Cam_Backend.Entities;

namespace Home_Cam_Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            // const string CAM_IP = "http://192.168.50.246";
            // HttpClient client = new();
            // HttpResponseMessage idResult = await client.GetAsync($"{CAM_IP}/who_are_you");
            // Console.WriteLine(await idResult.Content.ReadAsStringAsync());

            // HttpResponseMessage imageResult = await client.GetAsync($"{CAM_IP}/esp32_cam_capture?cb={DateTime.Now.Ticks}");
            // byte[] imageArray = await imageResult.Content.ReadAsByteArrayAsync();
            // File.WriteAllBytes("D:/Download/test_img2.jpg", imageArray);

            // WebRequest request = WebRequest.Create($"{CAM_IP}/who_are_you");
            // WebResponse response = request.GetResponse();
            // Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // using (Stream dataStream = response.GetResponseStream())
            // {
            //     // Open the stream using a StreamReader for easy access.
            //     StreamReader reader = new StreamReader(dataStream);
            //     // Read the content.
            //     string responseFromServer = reader.ReadToEnd();
            //     // Display the content.
            //     Console.WriteLine(responseFromServer);
            // }

            // // Close the response.
            // response.Close();

            // WebClient wc = new WebClient();
            // wc.DownloadFileAsync(new System.Uri($"{CAM_IP}/esp32_cam_capture?cb={DateTime.Now.Ticks}"), "D:/Download/test_img.jpg");

            List<Esp32Cam> list = await Esp32Cam.FindCameras();
            foreach(Esp32Cam cam in list)
            {
                Console.WriteLine($"{cam.IpAddr}={cam.UniqueId}");
                // await cam.AdjustFrameSize(10);
                // byte[] imageArray = await cam.GetSingleShot();
                // File.WriteAllBytes($"D:/Download/{cam.IpAddr}.jpg", imageArray);
            }

            // CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
