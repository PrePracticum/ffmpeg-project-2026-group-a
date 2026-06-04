using Ffmpeg.Command.Commands;
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
    public class AudioRemovalCommand : BaseCommand, ICommand<AudioRemovalModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public AudioRemovalCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(AudioRemovalModel model)
        {
         
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .SetVideoCodec(model.VideoCodec)
                .AddOption("-an")
                .SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}
