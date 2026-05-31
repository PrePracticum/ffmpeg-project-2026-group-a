using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class TextAnimationCommand : BaseCommand, ICommand<TextAnimationModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public TextAnimationCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(TextAnimationModel model)
        {
            // קביעת מיקומי הטקסט (אם אנימציה: זז מימין לשמאל, אם סטטי: מיקום קבוע)
            string xAxis = model.IsAnimated ? "w-t*150" : "100";
            string yAxis = model.IsAnimated ? "h-50" : "50";

            // בניית פילטר ה-drawtext של FFmpeg
            string drawtextFilter = $"drawtext=text='{model.TextContent}':x={xAxis}:y={yAxis}:fontsize={model.Size}:fontcolor={model.Color}";

            // שימוש ב-Builder הקיים אצלכם בפרויקט לבניית הארגומנטים
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"{drawtextFilter}\"") // הוספת פילטר הטקסט/אנימציה
                .AddOption($"-map 0:a?")                // שומר על האודיו אם קיים
                .AddOption($"-c:a copy");               // מעתיק את האודיו ללא קידוד מחדש לחסכון בזמן

            // הגדרת קובץ המוצא (output)
            CommandBuilder.SetOutput(model.OutputFile, false);

            // הרצה וביצוע בפועל דרך ה-BaseCommand
            return await RunAsync();
        }
    }
}