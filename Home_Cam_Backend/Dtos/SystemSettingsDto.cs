namespace Home_Cam_Backend.Dtos
{
    public record SystemSettingsDto
    {
        public int MaxSpaceGBs { get; init; }
        public int PercentToDeleteWhenFull { get; init; }
        public int SearchCamerasMinutes { get; init; }
        public int ImageStorageSizeControlMinutes { get; init; }
    }
}