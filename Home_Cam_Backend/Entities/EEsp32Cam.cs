namespace Home_Cam_Backend.Entities
{
    public class EEsp32Cam
    {
        public string IpAddr { get; set; }
        public string UniqueId { get; init; }
        public EEsp32CamSetting Setting { get; set; }
    }
}