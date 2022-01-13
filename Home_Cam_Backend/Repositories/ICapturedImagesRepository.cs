using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;

namespace Home_Cam_Backend.Repositories
{
    public interface ICapturedImagesRepository
    {
        Task CreateImageInfo(ECapturedImageInfo image);
        // Task<List<ECapturedImageInfo>> GetImageInfos(DateTime beginDateTime, DateTime endDateTime);
        // Task<ECapturedImageInfo> GetImageInfo(DateTime beginDateTime, DateTime endDateTime);
        // Task UpdateImageInfo(ECapturedImageInfo image);
        // Task DeleteImageInfos(DateTime beginDateTime, DateTime endDateTime);
    }
}