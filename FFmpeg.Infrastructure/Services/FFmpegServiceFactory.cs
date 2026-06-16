using Ffmpeg.Command;
using Ffmpeg.Command.Commands;
using FFmpeg.Core.Models;
using FFmpeg.Infrastructure.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpeg.Infrastructure.Services
{
    public interface IFFmpegServiceFactory
    {
        ICommand<WatermarkModel> CreateWatermarkCommand();
        ICommand<ExtractFrameModel> CreateExtractFrameCommand();
        ICommand<ConvertVideoModel> CreateConvertVideoCommand();
        ICommand<ReverseVideoModel> CreateReverseVideoCommand();
        ICommand<MergeVideosModel> CreateMergeVideosCommand();
        ICommand<AudioRemovalModel> CreateAudioRemovalCommand();
        ICommand<GifFromVideoModel> CreateGifFromVideoCommand();
        ICommand<BlurVideoModel> CreateBlurVideoCommand();
        ICommand<AddBorderModel> CreateAddBorderCommand();
        ICommand<AddTextModel> CreateAddTextCommand();
    }

    public class FFmpegServiceFactory : IFFmpegServiceFactory
    {
        private readonly FFmpegExecutor _executor;
        private readonly ICommandBuilder _commandBuilder;

        public FFmpegServiceFactory(IConfiguration configuration, ILogger logger = null)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDirectory, "external", "ffmpeg.exe");

            bool logOutput = bool.TryParse(configuration["FFmpeg:LogOutput"], out bool log) && log;

            _executor = new FFmpegExecutor(ffmpegPath, logOutput, logger);
            _commandBuilder = new CommandBuilder(configuration);
        }

        public ICommand<WatermarkModel> CreateWatermarkCommand()
        {
            return new WatermarkCommand(_executor, _commandBuilder);
        }

        public ICommand<ExtractFrameModel> CreateExtractFrameCommand()
        {
            return new ExtractFrameCommand(_executor, _commandBuilder);
        }

        public ICommand<ConvertVideoModel> CreateConvertVideoCommand()
        {
            return new ConvertVideoCommand(_executor);
        }

        public ICommand<ReverseVideoModel> CreateReverseVideoCommand()
        {
            return new ReverseVideoCommand(_executor, _commandBuilder);
        }

        public ICommand<MergeVideosModel> CreateMergeVideosCommand()
        {
            return new MergeVideosCommand(_executor, _commandBuilder);
        }

        public ICommand<AudioRemovalModel> CreateAudioRemovalCommand()
        {
            return new AudioRemovalCommand(_executor, _commandBuilder);
        }

        public ICommand<GifFromVideoModel> CreateGifFromVideoCommand()
        {
            return new GifFromVideoCommand(_executor, _commandBuilder);
        }

        public ICommand<BlurVideoModel> CreateBlurVideoCommand()
        {
            return new BlurVideoCommand(_executor, _commandBuilder);
        }

        public ICommand<AddBorderModel> CreateAddBorderCommand()
        {
            return new AddBorderCommand(_executor, _commandBuilder);
        }
        public ICommand<AddTextModel> CreateAddTextCommand()
        {
            return new AddTextCommand(_executor, _commandBuilder);
        }
    }
}