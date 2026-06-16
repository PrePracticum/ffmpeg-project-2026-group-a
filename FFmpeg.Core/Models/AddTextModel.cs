using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Core.Models
{
    public class AddTextModel
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string TextContent { get; set; }
        public string FontColor { get; set; } = "white"; // צבע ברירת מחדל
        public int FontSize { get; set; } = 24;          // גודל ברירת מחדל

        // הגדרת מיקום כסטרינג מאפשרת להעביר מספר קבוע (למשל "100") 
        // או ביטוי של פקודה לאנימציה (למשל "t*50" לפקודה זזה)
        public string XPosition { get; set; } = "100";
        public string YPosition { get; set; } = "50";
    }
}
