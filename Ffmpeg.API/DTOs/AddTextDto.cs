
using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class AddTextDto
    {
        public IFormFile VideoFile { get; set; }
        public string TextContent { get; set; }
        public string FontColor { get; set; } = "white";
        public int FontSize { get; set; } = 24;
        public string XPosition { get; set; } = "100";
        public string YPosition { get; set; } = "50";
    }
}