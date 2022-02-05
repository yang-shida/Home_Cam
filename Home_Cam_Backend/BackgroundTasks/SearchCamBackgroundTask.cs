using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Home_Cam_Backend.Controllers;
using Home_Cam_Backend.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Home_Cam_Backend.BackgroundTasks
{
    public class SearchCamBackgroundTask : BackgroundService
    {   
        private Timer MyTimer;
        private readonly ICamSettingsRepository repository;
        private readonly IConfiguration Configuration;

        public SearchCamBackgroundTask(IConfiguration configuration, ICamSettingsRepository repo)
        {
            repository = repo;
            Configuration=configuration;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            MyTimer=new(SearchCameras, null, TimeSpan.Zero, TimeSpan.FromMinutes(Configuration.GetSection("BackgroundTasksTimingSettings").GetValue<int>("SearchCamerasMinutes")));
            return Task.CompletedTask;
        }

        private async void SearchCameras(object state)
        {
            // Extensions.WriteToLogFile("Find cameras from background thread.");
            await Esp32Cam.FindCameras(repository);
        }

    }
}