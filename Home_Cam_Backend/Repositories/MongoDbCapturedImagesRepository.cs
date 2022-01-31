using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;
using MongoDB.Driver;

namespace Home_Cam_Backend.Repositories
{
    public class MongoDbCapturedImagesRepository : ICapturedImagesRepository
    {
        private const string databaseName = "home_cam_backend";
        private const string collectionName = "capturedImageInfos";
        private readonly IMongoCollection<ECapturedImageInfo> capturedImageInfoCollection;
        private readonly FilterDefinitionBuilder<ECapturedImageInfo> capturedImageFilterBuilder = Builders<ECapturedImageInfo>.Filter;

        public MongoDbCapturedImagesRepository(IMongoClient mongoClient)
        {
            IMongoDatabase database = mongoClient.GetDatabase(databaseName);
            capturedImageInfoCollection = database.GetCollection<ECapturedImageInfo>(collectionName);
        }

        public async Task CreateImageInfo(ECapturedImageInfo imageInfo)
        {
            await capturedImageInfoCollection.InsertOneAsync(imageInfo);
        }

        public async Task DeleteImageInfos(string camId, DateTimeOffset beginDateTime, DateTimeOffset endDateTime)
        {
            var filter = capturedImageFilterBuilder.And(
                capturedImageFilterBuilder.Eq(imageInfoFromDb => imageInfoFromDb.CamId, camId),
                capturedImageFilterBuilder.Gte(imageInfoFromDb => imageInfoFromDb.CreatedDate, beginDateTime),
                capturedImageFilterBuilder.Lt(imageInfoFromDb => imageInfoFromDb.CreatedDate, endDateTime)
            );
            await capturedImageInfoCollection.DeleteManyAsync(filter);
        }

        public async Task<List<ECapturedImageInfo>> GetImageInfos(string camId, DateTimeOffset beginDateTime, DateTimeOffset endDateTime)
        {
            var filter = capturedImageFilterBuilder.And(
                capturedImageFilterBuilder.Eq(imageInfoFromDb => imageInfoFromDb.CamId, camId),
                capturedImageFilterBuilder.Gte(imageInfoFromDb => imageInfoFromDb.CreatedDate, beginDateTime),
                capturedImageFilterBuilder.Lt(imageInfoFromDb => imageInfoFromDb.CreatedDate, endDateTime)
            );
            return await (await capturedImageInfoCollection.FindAsync(filter)).ToListAsync();
        }
    }
}