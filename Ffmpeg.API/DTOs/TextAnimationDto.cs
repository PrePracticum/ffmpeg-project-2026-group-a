using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class TextAnimationDto
    {
        public IFormFile VideoFile { get; set; }
        public string TextContent { get; set; }
        public string Color { get; set; } = "white";
        public int Size { get; set; } = 24;
        public bool IsAnimated { get; set; } // אופציה לאנימציה (טקסט זז)
    }
}