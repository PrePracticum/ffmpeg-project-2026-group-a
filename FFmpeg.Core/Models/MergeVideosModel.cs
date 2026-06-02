using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class MergeVideosModel
    {
        public required string FirstInputFile { get; set; }
        public required string SecondInputFile { get; set; }
        public required string OutputFile { get; set; }
        public string Orientation { get; set; } = "horizontal"; // "horizontal" or "vertical"
        public string VideoCodec { get; set; } = "libx264"; // Default codec
    }
}
