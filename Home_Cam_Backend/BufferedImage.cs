using System;

namespace Home_Cam_Backend
{
    public class BufferedImage
    {
        public bool valid { get; set; }
        public byte[] image { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public BufferedImage()
        {
            valid=false;
            image=null;
        }
    }
}