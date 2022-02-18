using System;

namespace Home_Cam_Backend.Dtos
{
    public record ImageInfoCreatedDateDto
    {
        public DateTimeOffset CreatedDate { get; init; }
    }
}