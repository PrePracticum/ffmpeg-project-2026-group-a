using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class ExtractFrameModel
    {
        public string VideoName { get; set; }

        public string DesiredTime { get; set; }

        public string OutputImageName { get; set; }
    }
}
