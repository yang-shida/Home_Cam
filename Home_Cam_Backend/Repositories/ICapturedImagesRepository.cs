using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;

namespace Home_Cam_Backend.Repositories
{
    public interface ICapturedImagesRepository
    {
        Task CreateImageInfo(ECapturedImageInfo imageInfo);
        Task<List<ECapturedImageInfo>> GetImageInfos(string camId, DateTimeOffset beginDateTime, DateTimeOffset endDateTime);
        Task DeleteImageInfos(string camId, DateTimeOffset beginDateTime, DateTimeOffset endDateTime);
    }
}