using System.Threading.Tasks;

namespace FFmpeg.Core.Interfaces
{
    public interface IVideoService
    {
        Task ChangeVideoSpeedAsync(string inputPath, double speedMultiplier, string outputPath);
    }
}