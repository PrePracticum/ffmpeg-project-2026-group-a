using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class MergeVideosCommand : BaseCommand, ICommand<MergeVideosModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public MergeVideosCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(MergeVideosModel model)
        {
            string filterExpression = model.Orientation?.ToLower() == "vertical" 
                ? "[0:v][1:v]vstack=inputs=2[out]" 
                : "[0:v][1:v]hstack=inputs=2[out]";

            CommandBuilder = _commandBuilder
                .SetInput(model.FirstInputFile)
                .SetInput(model.SecondInputFile)
                .AddFilterComplex(filterExpression)
                .SetVideoCodec(model.VideoCodec)
                .AddOption("-map 0:a?")
                .AddOption("-c:a copy")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
