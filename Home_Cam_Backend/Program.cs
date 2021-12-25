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
using Home_Cam_Backend.Repositories;

// mongod.exe --dbpath C:\Gatech_Class_Files\Home_Cam\Home_Cam_Backend\MongoDB

namespace Home_Cam_Backend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            
            // List<Esp32Cam> list = await Esp32Cam.FindCameras();
            // foreach(Esp32Cam cam in list)
            // {
            //     Console.WriteLine($"{cam.IpAddr}={cam.UniqueId}");
            //     // await cam.AdjustFrameSize(10);
            //     // byte[] imageArray = await cam.GetSingleShot();
            //     // File.WriteAllBytes($"D:/Download/{cam.IpAddr}.jpg", imageArray);
            // }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
