using Microsoft.AspNetCore.Http;
using System.ComponentModel; // חובה להוסיף את השורה הזו כדי להשתמש ב-DefaultValue

namespace FFmpeg.API.DTOs
{
    public class ConvertVideoDto
    {
        public IFormFile VideoFile { get; set; } 
        
        [DefaultValue(".avi")]
        public string TargetFormat { get; set; } = ".avi"; 
    }
}