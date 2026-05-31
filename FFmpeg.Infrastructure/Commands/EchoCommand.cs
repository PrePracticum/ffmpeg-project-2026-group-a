using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace Ffmpeg.Command.Commands
{
    public class EchoCommand : BaseCommand, ICommand<EchoModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public EchoCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(EchoModel model)
        {
        
            CommandBuilder = _commandBuilder
                .SetInput(model.InputFile)
                .AddOption("-af \"aecho=0.8:0.88:60:0.4\"");

            if (!string.IsNullOrEmpty(model.Duration))
            {
                CommandBuilder.AddOption($"-t {model.Duration}");
            }

            CommandBuilder.SetOutput(model.OutputFile, false);

            return await RunAsync();
        }
    }
}