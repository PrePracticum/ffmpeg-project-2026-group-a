namespace FFmpeg.API.DTOs
{
    public class ChangeSpeedRequest
    {
        public string InputPath { get; set; }
        public double SpeedMultiplier { get; set; }
        public string OutputPath { get; set; }
    }
}