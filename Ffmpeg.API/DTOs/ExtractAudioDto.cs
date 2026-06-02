using Microsoft.AspNetCore.Http;

namespace Ffmpeg.API.DTOs
{
    public class ExtractAudioDto
    {
        public IFormFile VideoFile { get; set; }
        public string OutputAudioName { get; set; }
    }
}