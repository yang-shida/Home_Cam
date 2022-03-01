using System;

namespace Home_Cam_Backend.Dtos
{
    public record TimeIntervalDto
    {
        public DateTimeOffset Start { get; init; }
        public DateTimeOffset End { get; init; }
    }
}