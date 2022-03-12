namespace Home_Cam_Backend.Dtos
{
    public record AuthDto
    {
        public string currPwd { get; init; }
        public string newPwd { get; init; }
    }
}