using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class AddBorderCommand : BaseCommand, ICommand<AddBorderModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public AddBorderCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(AddBorderModel model)
        {
            int total = model.BorderSize * 2;

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption($"-vf \"pad=width=iw+{total}:height=ih+{total}:x={model.BorderSize}:y={model.BorderSize}:color={model.BorderColor}\"")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
