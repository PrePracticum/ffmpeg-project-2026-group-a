namespace FFmpeg.Core.Models
{
    public class BlurVideoModel
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public double Sigma { get; set; } = 10;
    }
}