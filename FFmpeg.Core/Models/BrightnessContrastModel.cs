namespace FFmpeg.Core.Models
{
    public class BrightnessContrastModel
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public double Brightness { get; set; }
        public double Contrast { get; set; }
    }
}