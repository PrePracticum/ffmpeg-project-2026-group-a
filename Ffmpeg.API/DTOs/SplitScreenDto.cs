using Microsoft.AspNetCore.Http;

namespace FFmpeg.API.DTOs
{
    public class SplitScreenDto
    {
        public IFormFile VideoFile { get; set; }
        public int Duplications { get; set; } = 2;
        /// <summary>
        /// Optional output file name (with extension). If not provided, a unique name will be generated.
        /// </summary>
        public string OutputFileName { get; set; }
    }
}
