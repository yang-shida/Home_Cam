using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Home_Cam_Backend.Controllers;
using Home_Cam_Backend.Repositories;
using Microsoft.Extensions.Hosting;

namespace Home_Cam_Backend.BackgroundTasks
{
    public class SearchCamBackgroundTask : BackgroundService
    {   
        private Timer MyTimer;
        private readonly ICamSettingsRepository repository;

        public SearchCamBackgroundTask(ICamSettingsRepository repo)
        {
            repository = repo;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            MyTimer=new(SearchCameras, null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }

        private async void SearchCameras(object? state)
        {
            await Esp32Cam.FindCameras(repository);
        }

    }
}