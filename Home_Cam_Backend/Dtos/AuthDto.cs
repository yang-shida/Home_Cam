namespace Home_Cam_Backend.Dtos
{
    public record AuthDto
    {
        public string CurrPwd { get; init; }
        public string NewPwd { get; init; }
    }
}