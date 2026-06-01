using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class ReverseVideoCommand : BaseCommand, ICommand<ReverseVideoModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ReverseVideoCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ReverseVideoModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption("-vf reverse")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}