namespace FFmpeg.API.DTOs
{
    public class ExtractFrameDto
    {
        public IFormFile VideoFile { get; set; }
        public string DesiredTime { get; set; }
        public string OutputImageName { get; set; }
    }
}
