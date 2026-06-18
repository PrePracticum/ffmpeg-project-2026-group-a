namespace FFmpeg.API.DTOs
{
    public class MergeVideosDto
    {
        public IFormFile FirstVideoFile { get; set; }
        public IFormFile SecondVideoFile { get; set; }
        public string Orientation { get; set; } = "horizontal"; // "horizontal" or "vertical"
    }
}
