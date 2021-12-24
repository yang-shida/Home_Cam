namespace Home_Cam_Backend.Entities
{
    public class EEsp32CamSetting
    {
        public string Location { get; set; }
        public int FrameSize { get; set; }
        public bool FlashLightOn { get; set; }
        public bool HorizontalMirror { get; set; }
        public bool VerticalMirror { get; set; }
    }
}