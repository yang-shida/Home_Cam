using System;

namespace Home_Cam_Backend.Dtos
{
    public record ControlledFrameDto
    {
        public long TimeSinceStartMs { get; init; }
        public string ImageBase64Str { get; init; }
    }
}