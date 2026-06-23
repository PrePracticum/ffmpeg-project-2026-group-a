using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class VolumeCommand : BaseCommand, ICommand<VolumeModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public VolumeCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(VolumeModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-af \"volume={model.VolumeLevel}\"")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
