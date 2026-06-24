using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class RotateVideoDto
    {
        public IFormFile VideoFile { get; set; }
        public double Angle { get; set; }
    }
}