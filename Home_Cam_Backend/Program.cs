using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// mongod.exe --dbpath C:\Gatech_Class_Files\Home_Cam\Home_Cam_Backend\MongoDB

namespace Home_Cam_Backend
{
    public class Program
    {
        public static bool isRestart;
        private static CancellationTokenSource cancelTokenSource;
        public static void Main(string[] args)
        {
            Task shutdownTask;
            do
            {
                isRestart = false;
                cancelTokenSource = new System.Threading.CancellationTokenSource();
                CancellationToken cancellationToken = cancelTokenSource.Token;
                IHost host = CreateHostBuilder(args).Build();
                host.RunAsync(cancellationToken);
                shutdownTask = host.WaitForShutdownAsync(cancellationToken);
                while(!shutdownTask.IsCompleted);
            }while(isRestart);
            
        }

        public static void Restart()
        {
            isRestart=true;
            cancelTokenSource.Cancel();
        }

        public static void Shutdown()
        {
            isRestart=false;
            cancelTokenSource.Cancel();
        }





        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
