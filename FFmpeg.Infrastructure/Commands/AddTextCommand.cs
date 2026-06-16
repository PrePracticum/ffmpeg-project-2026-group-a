using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FFmpeg.Infrastructure.Commands
{
    public class AddTextCommand : BaseCommand, ICommand<AddTextModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public AddTextCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(AddTextModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // בניית מחרוזת הפילטר בצורה נקייה עם הגרשים הנדרשים ל-FFmpeg
            string filterExpression = $"-vf \"drawtext=text='{model.TextContent}':x={model.XPosition}:y={model.YPosition}:fontsize={model.FontSize}:fontcolor={model.FontColor}\"";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption(filterExpression)
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
