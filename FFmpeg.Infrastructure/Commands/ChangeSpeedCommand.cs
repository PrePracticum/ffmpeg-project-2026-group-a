using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class ChangeSpeedCommand : BaseCommand, ICommand<ChangeSpeedModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ChangeSpeedCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ChangeSpeedModel model)
        {
            double videoScale = 1.0 / model.SpeedMultiplier;

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-filter_complex \"[0:v]setpts={videoScale}*PTS[v];[0:a]atempo={model.SpeedMultiplier}\" -map \"[v]\" -map \"[a]\"")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}