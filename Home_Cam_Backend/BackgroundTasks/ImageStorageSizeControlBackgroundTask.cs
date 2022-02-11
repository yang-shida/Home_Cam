using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;
using Home_Cam_Backend.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Home_Cam_Backend.BackgroundTasks
{
    public class ImageStorageSizeControlBackgroundTask : BackgroundService
    {
        private readonly IConfiguration Configuration;
        private readonly ICapturedImagesRepository capturedImageInfoRepository;
        private long maxImageStorageSpaceBytes;
        private long sizeAfterDeleting;
        private Timer MyTimer, MyTimerLongtermCheck;
        private int numOfImagePerBatch;

        public ImageStorageSizeControlBackgroundTask(IConfiguration configuration, ICapturedImagesRepository repo)
        {
            this.Configuration = configuration;
            this.capturedImageInfoRepository = repo;
            maxImageStorageSpaceBytes = Configuration.GetSection("ImageStorageSettings").GetValue<long>("MaxSpaceBytes");
            sizeAfterDeleting = (long)(maxImageStorageSpaceBytes * (1 - Configuration.GetSection("ImageStorageSettings").GetValue<double>("PercentToDeleteWhenFull") / 100));
            numOfImagePerBatch = Configuration.GetSection("ImageStorageSettings").GetValue<int>("BatchSize");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            MyTimer = new(MaintainStorageSpace, null, TimeSpan.Zero, TimeSpan.FromMinutes(Configuration.GetSection("BackgroundTasksTimingSettings").GetValue<int>("ImageStorageSizeControlMinutes")));
            MyTimerLongtermCheck = new(MaintainStorageSpaceLongterm, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        private async void MaintainStorageSpace(object state)
        {
            
            long currSize = 0;

            try
            {
                currSize = await capturedImageInfoRepository.GetTotalSize();
            }
            catch
            {
                Console.WriteLine("MaintainStorageSpace: DB not available");
                return;
            }

            Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] Checking storage size. Current size = {Math.Round(((double)currSize) / 1024 / 1024, 3)} MB");

            // limit not hit, do nothing
            if (currSize < maxImageStorageSpaceBytes)
            {
                return;
            }

            // hit storage limit, need to delete some old pictures
            long sizeToDelete = currSize - sizeAfterDeleting;

            List<ECapturedImageInfo> currImageBatch = new();

            int count = 0;

            do
            {
                currImageBatch = await capturedImageInfoRepository.GetOldestN(numOfImagePerBatch);
                if (currImageBatch.Count == 0)
                {
                    break;
                }
                DateTimeOffset fromDate = currImageBatch.First().CreatedDate;
                DateTimeOffset toDate = currImageBatch.Last().CreatedDate;
                foreach (var imageInfo in currImageBatch)
                {
                    try
                    {
                        // delete image on disk
                        File.Delete(imageInfo.ImageFileLocation);
                        // update remaining size to delete
                        sizeToDelete -= imageInfo.Size;
                    }
                    catch
                    {
                        Console.WriteLine($"Fail to delete old image. Path={imageInfo.ImageFileLocation}");
                    }
                }
                // remove info from DB
                await capturedImageInfoRepository.DeleteImageInfos(fromDate, toDate);
                count += numOfImagePerBatch;
            } while (sizeToDelete > 0);

            Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] Deleted {count} images.");

        }

        private async void MaintainStorageSpaceLongterm(object state)
        {
            string dirName = Configuration.GetSection("ImageStorageSettings").GetValue<string>("Path");
            string[] camDirs = {};
            try
            {
                camDirs = Directory.GetDirectories(dirName);
            }
            catch
            {
                Console.WriteLine($"MaintainStorageSpaceLongterm: Fail to get directory. Path={dirName}");
                return;
            }
            
            int count = 0;

            foreach (string camDir in camDirs)
            {
                string camId = camDir.Substring(camDir.LastIndexOf('\\') + 1);
                camId = camId.Substring(0, 2) + ":" + camId.Substring(2, 2) + ":" + camId.Substring(4, 2) + ":" + camId.Substring(6, 2) + ":" + camId.Substring(8, 2) + ":" + camId.Substring(10, 2);

                string[] fileNames = Directory.GetFiles(camDir);
                long utcMsCutoff = (await capturedImageInfoRepository.GetOldestImageDate(camId)).ToUnixTimeMilliseconds();
                foreach (string fileName in fileNames)
                {
                    int begin = fileName.LastIndexOf('\\') + 1;
                    int len = fileName.LastIndexOf('.') - begin;

                    long utcMsImage = long.Parse(fileName.Substring(begin, len));
                    if (utcMsImage < utcMsCutoff)
                    {
                        try
                        {
                            File.Delete(fileName);
                            count++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.WriteLine($"Fail to delete old image (long term check). Path={fileName}");
                        }
                    }

                }
            }

            Extensions.WriteToLogFile($"[{DateTime.Now.ToString("MM/dd/yyyy-hh:mm:ss")}] Deleted {count} images (long term check).");

        }
    }
}