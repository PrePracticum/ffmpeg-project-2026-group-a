namespace FFmpeg.Core.Models
{
    public class ChangeSpeedModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public double SpeedMultiplier { get; set; }
    }
}