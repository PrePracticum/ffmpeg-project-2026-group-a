using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class GifFromVideoCommand : BaseCommand, ICommand<GifFromVideoModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public GifFromVideoCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(GifFromVideoModel model)
        {
            // ffmpeg -i input.mp4 -vf "fps=10,scale=320:-1" output.gif
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption("-vf \"fps=10,scale=320:-1\"")
                .SetOutput(model.OutputFile, true);

            return await RunAsync();
        }
    }
}