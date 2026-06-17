namespace FFmpeg.Core.Models
{
    public class ChromaKeyModel
    {
        public string InputFile { get; set; }
        public string BackgroundFile { get; set; }
        public string OutputFile { get; set; }
        public string ChromaColor { get; set; } = "0x00FF00";
        public double Similarity { get; set; } = 0.1;
        public double Blend { get; set; } = 0.2;
        public string VideoCodec { get; set; } = "libx264";
    }
}
