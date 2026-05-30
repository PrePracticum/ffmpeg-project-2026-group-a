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

        // הוספנו פונקציה בונה (Constructor) שמקבלת את קובץ ההגדרות
        public VideoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ChangeVideoSpeedAsync(string inputPath, double speedMultiplier, string outputPath)
        {
            // קריאת הנתיב מתוך appsettings.json
            string ffmpegDir = _configuration["FFmpeg:Path"];

            // המרת משתנה הסביבה (USERPROFILE) לנתיב אמיתי במחשב
            ffmpegDir = Environment.ExpandEnvironmentVariables(ffmpegDir);

            // חיבור הנתיב של התיקייה יחד עם שם הקובץ
            string ffmpegExePath = Path.Combine(ffmpegDir, "ffmpeg.exe");

            double pts = 1.0 / speedMultiplier;
            string ptsString = pts.ToString(CultureInfo.InvariantCulture);

            string arguments = $"-y -i \"{inputPath}\" -filter:v \"setpts={ptsString}*PTS\" \"{outputPath}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = ffmpegExePath, // שימוש בנתיב המדויק שקראנו מההגדרות
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