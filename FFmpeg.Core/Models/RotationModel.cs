namespace FFmpeg.Core.Models
{
    public class RotationModel
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public double Angle { get; set; }
    }
}