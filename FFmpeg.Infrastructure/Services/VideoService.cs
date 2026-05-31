using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FFmpeg.Infrastructure.Services
{
    public class VideoService : IVideoService
    {
        private readonly IConfiguration _configuration;

        public VideoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ChangeVideoSpeedAsync(string inputFileName, double speedMultiplier, string outputFileName)
        {
            // 1. קריאת נתיב הבסיס מתוך ההגדרות (appsettings.json)
            string basePath = _configuration["FFmpeg:Path"] ?? throw new InvalidOperationException("FFmpeg:Path configuration is missing");
            basePath = Environment.ExpandEnvironmentVariables(basePath);
            string ffmpegExePath = Path.Combine(basePath, "ffmpeg.exe");

            // 2. הפתרון הקסום: בניית הנתיבים המלאים בדיוק לאותן תיקיות ש-FileService עובד איתן!
            string inputFullPath = Path.Combine(basePath, "Input", inputFileName);
            string outputFullPath = Path.Combine(basePath, "Output", outputFileName);

            // 3. בדיקה שהקובץ קיים (עכשיו זה יעבוד כי יש לו נתיב פיזי אמיתי!)
            if (!File.Exists(inputFullPath))
            {
                throw new FileNotFoundException($"The input file was not found at the physical path: {inputFullPath}");
            }

            // 4. הרצת הפקודה
            double pts = 1.0 / speedMultiplier;
            string ptsString = pts.ToString(CultureInfo.InvariantCulture);
            string arguments = $"-y -i \"{inputFullPath}\" -filter:v \"setpts={ptsString}*PTS\" \"{outputFullPath}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegExePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                process.Start();

                string errorOutput = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg failed with exit code {process.ExitCode}. Error: {errorOutput}");
                }
            }
        }
    }
}