using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class VolumeDto
    {
        public IFormFile VideoFile { get; set; } = null!;
        public double VolumeLevel { get; set; }
    }
}
