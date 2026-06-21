using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class FadeEffectCommand : BaseCommand, ICommand<FadeEffectModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public FadeEffectCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(FadeEffectModel model)
        {
            string mode = model.FadeIn ? "in" : "out";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"fade=t={mode}:st=0:d={model.DurationSeconds}\"")
                .SetVideoCodec(model.VideoCodec)
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
