using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Home_Cam_Backend.Entities
{
    [BsonIgnoreExtraElements]
    public record ECapturedImageInfo
    {
        public string CamId { get; init; }
        public string ImageFileLocation { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}