using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.Core.Interfaces;

namespace FFmpeg.Infrastructure.Services
{
    public class VideoService : IVideoService
    {
        public async Task ChangeVideoSpeedAsync(string inputPath, double speedMultiplier, string outputPath)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input video file not found at: {inputPath}");
            }

            // חישוב המהירות עבור FFmpeg
            double videoScale = 1.0 / speedMultiplier;
            string arguments = $"-i \"{inputPath}\" -filter_complex \"[0:v]setpts={videoScale}*PTS[v];[0:a]atempo={speedMultiplier}[a]\" -map \"[v]\" -map \"[a]\" -y \"{outputPath}\"";

            // הגדרת נתיבים אפשריים לקובץ ffmpeg.exe במחשב שלך
            string ffmpegExePath = "ffmpeg"; // ברירת מחדל מערכתית

            string path1 = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg.exe"); // תיקיית ה-API הראשית
            string path2 = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"); // תיקיית ה-bin/Debug הפנימית

            if (File.Exists(path1))
            {
                ffmpegExePath = path1;
            }
            else if (File.Exists(path2))
            {
                ffmpegExePath = path2;
            }
            else
            {
                // אם הוא לא מצא בשום מקום, נזרוק שגיאה ברורה שתסביר לנו איפה הוא חיפש
                throw new FileNotFoundException($"ffmpeg.exe was not found! Please place it either in:\n1) {path1}\nOR\n2) {path2}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegExePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg speed change failed with exit code {process.ExitCode}. Details: {errors}");
                }
            }
        }
    }
}