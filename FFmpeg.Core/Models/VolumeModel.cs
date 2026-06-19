namespace FFmpeg.Core.Models
{
    public class VolumeModel
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public double VolumeLevel { get; set; }
    }
}
