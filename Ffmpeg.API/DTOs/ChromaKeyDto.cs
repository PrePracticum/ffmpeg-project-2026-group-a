using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class ChromaKeyDto
    {
        public IFormFile VideoFile { get; set; }
        public IFormFile BackgroundFile { get; set; }
        public string OutputFileName { get; set; }
    }
}
