namespace FFmpeg.API.DTOs
{
    public class AddBorderDto
    {
        public IFormFile VideoFile { get; set; }
        public string BorderColor { get; set; } = "black";
        public int BorderSize { get; set; } = 20;
    }
}
