using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class TextAnimationModel
    {

        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string TextContent { get; set; }
        public string Color { get; set; }
        public int Size { get; set; }
        public bool IsAnimated { get; set; }
    }
}

