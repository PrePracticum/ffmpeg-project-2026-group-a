using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
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
                .AddOption("-an")
                .SetOutput(model.OutputFile, true)
             ; 

            return await RunAsync();
        }
    }
}