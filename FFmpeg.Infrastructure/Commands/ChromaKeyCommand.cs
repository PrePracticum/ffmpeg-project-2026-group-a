using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class ChromaKeyCommand : BaseCommand, ICommand<ChromaKeyModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ChromaKeyCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ChromaKeyModel model)
        {
            string filterExpression = $"[0:v]chromakey={model.ChromaColor}:{model.Similarity}:{model.Blend}[ckout];[1:v][ckout]overlay[out]";

            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .SetInput(model.BackgroundFile)
                .AddFilterComplex(filterExpression)
                .SetVideoCodec(model.VideoCodec)
                .AddOption("-map 0:a?")
                .AddOption("-c:a copy")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
