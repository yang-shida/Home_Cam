using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Home_Cam_Backend.Entities;
using MongoDB.Bson;
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

        public async Task DeleteImageInfos(DateTimeOffset beginDateTime, DateTimeOffset endDateTime)
        {
            var filter = capturedImageFilterBuilder.And(
                capturedImageFilterBuilder.Gte(imageInfoFromDb => imageInfoFromDb.CreatedDate, beginDateTime),
                capturedImageFilterBuilder.Lte(imageInfoFromDb => imageInfoFromDb.CreatedDate, endDateTime)
            );
            await capturedImageInfoCollection.DeleteManyAsync(filter);
        }

        public async Task<List<ECapturedImageInfo>> GetImageInfos(string camId, DateTimeOffset beginDateTime, DateTimeOffset endDateTime)
        {
            var filter = capturedImageFilterBuilder.And(
                capturedImageFilterBuilder.Eq(imageInfoFromDb => imageInfoFromDb.CamId, camId),
                capturedImageFilterBuilder.Gte(imageInfoFromDb => imageInfoFromDb.CreatedDate, beginDateTime),
                capturedImageFilterBuilder.Lte(imageInfoFromDb => imageInfoFromDb.CreatedDate, endDateTime)
            );
            return await (await capturedImageInfoCollection.FindAsync(filter)).ToListAsync();
        }

        public async Task<long> GetTotalSize()
        {
            var group = new BsonDocument
                            {
                                {
                                    "$group",
                                    new BsonDocument
                                        {
                                            {"_id", BsonNull.Value},
                                            {"TotalSize", new BsonDocument{{"$sum", "$Size"}}}
                                        }
                                }
                            };
            var pipeline = new[]{group};
            var doc = await (await capturedImageInfoCollection.AggregateAsync<BsonDocument>(pipeline)).SingleAsync();
            return doc["TotalSize"].AsInt64;
        }

        public async Task<List<ECapturedImageInfo>> GetOldestN(int N)
        {
            var sort = new BsonDocument
                            {
                                {
                                    "$sort",
                                    new BsonDocument {{"CreatedDate", 1}}
                                }
                            };
            var limit = new BsonDocument {{"$limit", N}};

            var pipeline = new[]{sort, limit};

            var doc = await (await capturedImageInfoCollection.AggregateAsync<ECapturedImageInfo>(pipeline)).ToListAsync();

            return doc;
        }

        public async Task<DateTimeOffset> GetOldestImageDate(string camId)
        {
            var match = new BsonDocument
                                {
                                    {
                                        "$match",
                                        new BsonDocument
                                            {
                                                {
                                                    "CamId",
                                                    new BsonDocument {{"$eq", camId}}
                                                }
                                            }
                                    }
                                };
            var sort = new BsonDocument
                            {
                                {
                                    "$sort",
                                    new BsonDocument {{"CreatedDate", 1}}
                                }
                            };
            var limit = new BsonDocument {{"$limit", 1}};
            var pipeline = new[]{match, sort, limit};
            var doc = await (await capturedImageInfoCollection.AggregateAsync<ECapturedImageInfo>(pipeline)).SingleOrDefaultAsync();
            return doc.CreatedDate;
        }
    }
}