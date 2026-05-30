using System.Diagnostics;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;

namespace FFmpeg.Infrastructure.Services
{
    public class RotationService : IRotationService
    {
        public void RotateVideo(RotationModel model)
        {
            // בניית פקודת הצינור עבור FFmpeg עם הפרמטרים הדינמיים מהמודל שלך
            string arguments = $"-i \"{model.InputFile}\" -vf \"rotate={model.Angle}*PI/180\" \"{model.OutputFile}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg", 
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // הפעלת התהליך של FFmpeg במערכת ההפעלה
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                
                // קריאת השגיאות/פלט (במידה ויהיו בעיות בהרצה)
                string errors = process.StandardError.ReadToEnd();
                
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg failed with exit code {process.ExitCode}. Error: {errors}");
                }
            }
        }
    }
}