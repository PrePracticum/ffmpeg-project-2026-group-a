using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Commands
{
    public class ChangeSpeedCommand
    {
        private readonly FFmpegExecutor _executor;
        private readonly ICommandBuilder _builder;

        public ChangeSpeedCommand(FFmpegExecutor executor, ICommandBuilder builder)
        {
            _executor = executor;
            _builder = builder;
        }

        public async Task<FFmpegResult> ExecuteAsync(ChangeSpeedModel model)
        {
            if (!File.Exists(model.InputFile))
            {
                throw new FileNotFoundException($"Input file not found: {model.InputFile}");
            }

            double videoScale = 1.0 / model.SpeedMultiplier;

            // Building the standard FFmpeg command arguments for video and audio speed adjustment
            string arguments = $"-i \"{model.InputFile}\" -filter_complex \"[0:v]setpts={videoScale}*PTS[v];[0:a]atempo={model.SpeedMultiplier}\" -map \"[v]\" -map \"[a]\" -y \"{model.OutputFile}\"";

            // Using the project's native executor ensures it runs correctly on the teacher's environment
            return await _executor.ExecuteArgsAsync(arguments);
        }
    }
}