using Microsoft.AspNetCore.Http; 

namespace FFmpeg.API.DTOs
{
    public class BrightnessContrastDto
    {
        public IFormFile VideoFile { get; set; } = null!;
        public double Brightness { get; set; }
        public double Contrast { get; set; }
    }
}