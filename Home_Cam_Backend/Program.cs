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
        public static void Main(string[] args)
        {
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
