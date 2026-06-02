using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;

namespace FFmpeg.Infrastructure.Commands
{
    public class ExtractAudioCommand : ICommand<ExtractAudioModel>
    {
        private readonly FFmpegExecutor _executor;

        public ExtractAudioCommand(FFmpegExecutor executor)
        {
            _executor = executor;
        }

        public async Task<CommandResult> ExecuteAsync(ExtractAudioModel model)
        {
            string arguments = $"-i \"{model.InputFile}\" -q:a 0 -map a \"{model.OutputFile}\"";
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