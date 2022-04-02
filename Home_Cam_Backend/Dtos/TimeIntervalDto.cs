using System;

namespace Home_Cam_Backend.Dtos
{
    public record TimeIntervalDto
    {
        public long Start { get; init; }
        public long End { get; init; }
    }
}