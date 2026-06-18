namespace FFmpeg.API.DTOs
{
    public class BlurVideoDto
    {
        public IFormFile VideoFile { get; set; }
        public double Sigma { get; set; } = 10;
    }
}