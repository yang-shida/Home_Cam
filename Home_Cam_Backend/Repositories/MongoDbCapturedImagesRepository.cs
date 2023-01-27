using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home_Cam_Backend.Dtos;
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
            var pipeline = new[] { group };
            var doc = await (await capturedImageInfoCollection.AggregateAsync<BsonDocument>(pipeline)).SingleOrDefaultAsync() ?? new BsonDocument
                                        {
                                            {"_id", BsonNull.Value},
                                            {"TotalSize", 0}
                                        };
            return doc["TotalSize"].ToInt64();
        }

        public async Task<List<ECapturedImageInfo>> GetOldestN(int N)
        {
            var sort = new BsonDocument
                            {
                                {
                                    "$sort",
                                    new BsonDocument {{"CreatedDate.0", 1}}
                                }
                            };
            var limit = new BsonDocument { { "$limit", N } };

            var pipeline = new[] { sort, limit };

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
                                    new BsonDocument {{"CreatedDate.0", 1}}
                                }
                            };
            var limit = new BsonDocument { { "$limit", 1 } };
            var project = new BsonDocument
                            {
                                {
                                    "$project", new BsonDocument
                                        {
                                            {
                                                "CreatedDate", 1
                                            },
                                            {
                                                "_id", 0
                                            }
                                        }
                                }
                            };
            var pipeline = new[] { match, sort, limit, project };
            var doc = await (await capturedImageInfoCollection.AggregateAsync<ImageInfoCreatedDateDto>(pipeline)).SingleOrDefaultAsync();
            return doc!=null?doc.CreatedDate:DateTimeOffset.MinValue;
        }

        public async Task<List<TimeIntervalDto>> GetRecordedTimeIntervals(string camId, long? startTimeUtc, long? timeLengthMillis, long thresholdMillis)
        {
            var match = new BsonDocument
                                {
                                    {
                                        "$match",
                                        new BsonDocument
                                        {
                                            {
                                                "$and",
                                                startTimeUtc == null || timeLengthMillis == null?
                                                new BsonArray
                                                {
                                                    new BsonDocument {{"CamId",new BsonDocument {{"$eq", camId}}}}
                                                }
                                                :
                                                new BsonArray
                                                {
                                                    new BsonDocument {{"CamId",new BsonDocument {{"$eq", camId}}}},
                                                    new BsonDocument {{"CreatedDate.0",new BsonDocument {{"$gte", DateTimeOffset.FromUnixTimeMilliseconds((long)startTimeUtc).Ticks}}}},
                                                    new BsonDocument {{"CreatedDate.0",new BsonDocument {{"$lt", DateTimeOffset.FromUnixTimeMilliseconds((long)startTimeUtc+(long)timeLengthMillis).Ticks}}}}
                                                }
                                            }
                                        }
                                    }
                                };
            var sort = new BsonDocument
                            {
                                {
                                    "$sort",
                                    new BsonDocument {{"CreatedDate.0", 1}}
                                }
                            };
            var project = new BsonDocument
                            {
                                {
                                    "$project", new BsonDocument
                                        {
                                            {
                                                "CreatedDate", 1
                                            },
                                            {
                                                "_id", 0
                                            }
                                        }
                                }
                            };
            var pipeline = new[] { match, sort, project };
            var doc = await (await capturedImageInfoCollection.AggregateAsync<ImageInfoCreatedDateDto>(pipeline)).ToListAsync();

            List<TimeIntervalDto> res = new();
            if(doc.Count==0)
            {
                return res;
            }
            bool findingStart = true;
            DateTimeOffset currStart = doc[0].CreatedDate;
            DateTimeOffset currEnd = currStart;

            for (int i = 0; i < doc.Count; i++)
            {
                if (i == doc.Count - 1)
                {
                    if (findingStart)
                    {
                        res.Add(new() { Start = doc[i].CreatedDate.ToUnixTimeMilliseconds(), End = doc[i].CreatedDate.ToUnixTimeMilliseconds() });
                    }
                    else
                    {
                        res.Add(new() { Start = currStart.ToUnixTimeMilliseconds(), End = doc[i].CreatedDate.ToUnixTimeMilliseconds() });
                    }
                }
                else
                {
                    if (findingStart)
                    {
                        currStart = doc[i].CreatedDate;
                        currEnd = currStart;
                        findingStart = false;
                    }
                    else
                    {
                        if (doc[i].CreatedDate.ToUnixTimeMilliseconds() - currEnd.ToUnixTimeMilliseconds() > thresholdMillis)
                        {
                            res.Add(new() { Start = currStart.ToUnixTimeMilliseconds(), End = currEnd.ToUnixTimeMilliseconds() });
                            currStart = doc[i].CreatedDate;
                            currEnd = currStart;
                            findingStart = false;
                        }
                        else
                        {
                            currEnd = doc[i].CreatedDate;
                        }
                    }
                }

            }
            return res;
        }

    }
}