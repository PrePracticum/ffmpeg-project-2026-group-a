using FFmpeg.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    internal class CreateThumbnailCommand
    {
        private readonly ICommandBuilder _commandBuilder;

        public CreateThumbnailCommand(FFmpegExecutor executor, ICommandBuilder commandBuilder)
            : base(executor)
        {
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<CommandResult> ExecuteAsync(CreateThumbnailModel model)
        {
            CommandBuilder = _commandBuilder
                .SetInput(model.VideoName)
                .AddOption("-ss 00:00:05") // זמן קבוע של 5 שניות
                .AddOption("-vframes 1")    // פריים בודד (תמונה)
                .SetOutput(model.OutputImageName, true); // דריסה אם קיים

            return await RunAsync();
        }
    
}
}


