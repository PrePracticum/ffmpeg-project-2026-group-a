using Ffmpeg.Command.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;

namespace FFmpeg.Infrastructure.Commands
{
    public class TextAnimationCommand : BaseCommand
    {
        private readonly string _input;
        private readonly string _text;
        private readonly string _color;
        private readonly int _size;
        private readonly string _output;
        private readonly bool _isAnimated;

        public TextAnimationCommand(FFmpegExecutor executor,string input, string text, string color, int size, string output, bool isAnimated) : base(executor)
        {
            _input = input;
            _text = text;
            _color = color;
            _size = size;
            _output = output;
            _isAnimated = isAnimated;
        }

        public  string BuildArguments()
        {
            // אם המשתמש ביקש אנימציה, נשתמש במשתני מיקום דינמיים של זמן (w-t*150)
            // אם לא, נמקם את הטקסט סטטית במרכז או במיקום קבוע (למשל x=100:y=50)
            string xAxis = _isAnimated ? "w-t*150" : "100";
            string yAxis = _isAnimated ? "h-50" : "50";

            return $"-i \"{_input}\" -vf \"drawtext=text='{_text}':x={xAxis}:y={yAxis}:fontsize={_size}:fontcolor={_color}\" \"{_output}\"";
        }
    }
}
