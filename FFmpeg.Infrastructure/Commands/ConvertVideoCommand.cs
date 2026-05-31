using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Services;
using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class ConvertVideoCommand : ICommand<ConvertVideoModel>
    {
        private readonly FFmpegExecutor _executor;

        public ConvertVideoCommand(FFmpegExecutor executor)
        {
            _executor = executor;
        }

        public async Task<CommandResult> ExecuteAsync(ConvertVideoModel model)
        {
            string arguments = $"-i \"{model.InputVideoName}\" \"{model.OutputVideoName}\"";
            
            var (success, output, error) = await _executor.RunCommandAsync(arguments);

            return new CommandResult
            {
                IsSuccess = success,
                ErrorMessage = success ? string.Empty : $"Command failed: {error}",
                CommandExecuted = arguments,
                OutputLog = success ? output : error
            };
        }
    }
}