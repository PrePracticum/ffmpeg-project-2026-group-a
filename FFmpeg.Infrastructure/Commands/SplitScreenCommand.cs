using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class SplitScreenCommand : BaseCommand, ICommand<SplitScreenModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public SplitScreenCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(SplitScreenModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Duplications < 1) model.Duplications = 2;

            // Add the same input multiple times
            var builder = _commandBuilder;

            for (int i = 0; i < model.Duplications; i++)
            {
                builder = (ICommandBuilder)builder.SetInput(model.InputFile);
            }

            // Build filter expression like: [0:v][1:v]hstack=inputs=2[out] (or more inputs)
            var inputs = new StringBuilder();
            for (int i = 0; i < model.Duplications; i++)
            {
                inputs.Append($"[{i}:v]");
            }

            string filterExpression = $"{inputs}hstack=inputs={model.Duplications}[out]";

            CommandBuilder = builder
                .AddFilterComplex(filterExpression)
                .SetVideoCodec(model.VideoCodec)
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
