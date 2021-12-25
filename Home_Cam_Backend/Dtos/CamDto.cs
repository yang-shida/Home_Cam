namespace Home_Cam_Backend.Dtos
{
    public record CamDto
    {
        public string IpAddr { get; init; }
        public string UniqueId { get; init; }   // MAC address of camera
    }
}