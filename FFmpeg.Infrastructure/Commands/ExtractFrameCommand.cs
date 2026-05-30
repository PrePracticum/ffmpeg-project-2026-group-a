using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class ExtractFrameCommand : BaseCommand, ICommand<ExtractFrameModel>
    {
        private readonly ICommandBuilder _commandBuilder;

        public ExtractFrameCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(ExtractFrameModel model)
        {
            // כאן אנחנו מרכיבים את הפקודה: 
            // ffmpeg -i input.mp4 -ss 00:00:05 -vframes 1 output.png
            CommandBuilder = _commandBuilder
                .SetInput(model.VideoName)
                .AddOption($"-ss {model.DesiredTime}")
                .AddOption("-vframes 1")
                .SetOutput(model.OutputImageName, true); // true אומר שמותר לדרוס תמונה אם כבר קיימת אחת באותו שם

            return await RunAsync();
        }
    }
}