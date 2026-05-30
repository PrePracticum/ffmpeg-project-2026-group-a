namespace FFmpeg.API.DTOs
{
    public class GifFromVideoDto
    {
        public IFormFile VideoFile { get; set; }
        public string OutputGifName { get; set; }
    }
}