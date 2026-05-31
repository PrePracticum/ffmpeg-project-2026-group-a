using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class EchoDto
    {
        public IFormFile VideoFile { get; set; }
        public string Duration { get; set; } 
    }
}