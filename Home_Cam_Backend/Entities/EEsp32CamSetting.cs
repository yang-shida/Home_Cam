using MongoDB.Bson.Serialization.Attributes;

namespace Home_Cam_Backend.Entities
{
    [BsonIgnoreExtraElements]
    public record EEsp32CamSetting
    {
        public string UniqueId { get; init; }   // MAC address of camera
        public string Location { get; init; }
        public int FrameSize { get; init; }
        public bool FlashLightOn { get; init; }
        public bool HorizontalMirror { get; init; }
        public bool VerticalMirror { get; init; }
    }
}