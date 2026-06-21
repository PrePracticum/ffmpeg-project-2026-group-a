using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class FadeEffectDto
    {
        public IFormFile VideoFile { get; set; }
        public double DurationSeconds { get; set; } = 2;
        // true = fade in, false = fade out
        public bool FadeIn { get; set; } = true;
    }
}
// Adding PR for review