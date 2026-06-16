using System;

namespace FFmpeg.Core.Models
{
    public class SplitScreenModel
    {
        public required string InputFile { get; set; }
        public required string OutputFile { get; set; }
        /// <summary>
        /// How many times to duplicate the input horizontally (default 2)
        /// </summary>
        public int Duplications { get; set; } = 2;
        public string VideoCodec { get; set; } = "libx264";
    }
}
