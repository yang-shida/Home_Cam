using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Home_Cam_Backend.Dtos;

namespace Home_Cam_Backend.Repositories
{
    public interface ICapturedImagesRepository
    {
        Task CreateImageInfo(ECapturedImageInfo imageInfo);
        Task<List<ECapturedImageInfo>> GetImageInfos(string camId, DateTimeOffset beginDateTime, DateTimeOffset endDateTime);
        Task DeleteImageInfos(DateTimeOffset beginDateTime, DateTimeOffset endDateTime);
        Task<long> GetTotalSize();
        Task<List<ECapturedImageInfo>> GetOldestN(int N);
        Task<DateTimeOffset> GetOldestImageDate(string camId);
        Task<List<TimeIntervalDto>> GetRecordedTimeIntervals(string camId, long? startTimeUtc, long? timeLengthMillis, long thresholdMillis);
    }
}