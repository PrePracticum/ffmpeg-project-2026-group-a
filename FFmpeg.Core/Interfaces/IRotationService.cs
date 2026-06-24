using FFmpeg.Core.Models;

namespace FFmpeg.Core.Interfaces
{
    public interface IRotationService
    {
        void RotateVideo(RotationModel model);
    }
}