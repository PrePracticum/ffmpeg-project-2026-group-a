namespace FFmpeg.Core.Models
{
    public class FadeEffectModel
    {
        public string InputFile { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public double DurationSeconds { get; set; } = 2;
        // true = fade in, false = fade out
        public bool FadeIn { get; set; } = true;
        public string VideoCodec { get; set; } = "libx264";
    }
}
