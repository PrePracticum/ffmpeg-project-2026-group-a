using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FFmpeg.API.DTOs;
using FFmpeg.Core.Interfaces;
using FFmpeg.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using FFmpeg.Infrastructure.Commands;
using FFmpeg.Infrastructure.Services;
using FFmpeg.Infrastructure;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/chroma-key", ChromaKey)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/brightness-contrast", ChangeBrightnessContrast)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/change-speed", ChangeSpeed)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/reverse", ReverseVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/extract-frame", ExtractFrame)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/convert", ConvertVideo)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/merge", MergeVideos)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));
        }

        private static async Task<IResult> AddWatermark(
            HttpContext context,
            [FromForm] WatermarkDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.WatermarkFile == null)
                {
                    return Results.BadRequest("Video file and watermark file are required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);

                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateWatermarkCommand();
                    var result = await command.ExecuteAsync(new WatermarkModel
                    {
                        InputFile = videoFileName,
                        WatermarkFile = watermarkFileName,
                        OutputFile = outputFileName,
                        XPosition = dto.XPosition,
                        YPosition = dto.YPosition,
                        IsVideo = true,
                        VideoCodec = "libx264"
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to add watermark: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing watermark request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddWatermark endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ChromaKey(
            HttpContext context,
            [FromForm] ChromaKeyDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null || dto.BackgroundFile == null || string.IsNullOrWhiteSpace(dto.OutputFileName))
                {
                    return Results.BadRequest("Video file, background file and output file name are required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string backgroundFileName = await fileService.SaveUploadedFileAsync(dto.BackgroundFile);

                string outputFileNameRaw = Path.GetFileName(dto.OutputFileName);
                string extension = Path.GetExtension(outputFileNameRaw);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = Path.GetExtension(dto.VideoFile.FileName);
                }

                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".mp4";
                }

                string downloadName = outputFileNameRaw.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                    ? outputFileNameRaw
                    : outputFileNameRaw + extension;

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);
                List<string> filesToCleanup = new List<string> { videoFileName, backgroundFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateChromaKeyCommand();
                    var result = await command.ExecuteAsync(new ChromaKeyModel
                    {
                        InputFile = videoFileName,
                        BackgroundFile = backgroundFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to replace green screen: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    string contentType = extension.ToLowerInvariant() switch
                    {
                        ".mp4" => "video/mp4",
                        ".mov" => "video/quicktime",
                        ".avi" => "video/x-msvideo",
                        _ => "application/octet-stream"
                    };

                    return Results.File(fileBytes, contentType, downloadName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing chroma-key request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChromaKey endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ChangeBrightnessContrast(
            HttpContext context,
            [FromForm] BrightnessContrastDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var executor = context.RequestServices.GetRequiredService<FFmpegExecutor>();
                    var builder = context.RequestServices.GetRequiredService<ICommandBuilder>();
                    var command = new BrightnessContrastCommand(executor, builder);

                    var result = await command.ExecuteAsync(new BrightnessContrastModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName,
                        Brightness = dto.Brightness,
                        Contrast = dto.Contrast
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to change brightness and contrast: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing brightness/contrast request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChangeBrightnessContrast endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ChangeSpeed(
            HttpContext context,
            [FromForm] ChangeSpeedDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var configuration = context.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }
                if (dto.SpeedMultiplier <= 0)
                {
                    return Results.BadRequest("Speed multiplier must be greater than 0");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                string fullInputPath = fileService.GetFullInputPath(videoFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                try
                {
                    double videoScale = 1.0 / dto.SpeedMultiplier;

                    // Safe handling for audio multiplier (FFmpeg atempo filter is restricted between 0.5 and 2.0)
                    string audioFilter = $"atempo={dto.SpeedMultiplier}";
                    if (dto.SpeedMultiplier > 2.0)
                    {
                        audioFilter = "atempo=2.0,atempo=" + (dto.SpeedMultiplier / 2.0);
                    }
                    else if (dto.SpeedMultiplier < 0.5)
                    {
                        audioFilter = "atempo=0.5,atempo=" + (dto.SpeedMultiplier / 0.5);
                    }

                    string arguments = $"-i \"{fullInputPath}\" -filter_complex \"[0:v]setpts={videoScale}*PTS[v];[0:a]{audioFilter}[a]\" -map \"[v]\" -map \"[a]\" -c:v libx264 -pix_fmt yuv420p -y \"{fullOutputPath}\"";

                    string ffmpegPath = configuration["FFmpeg:ExecutablePath"] ?? "ffmpeg";

                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = arguments,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new System.Diagnostics.Process { StartInfo = startInfo })
                    {
                        process.Start();
                        string errors = await process.StandardError.ReadToEndAsync();
                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            logger.LogError("FFmpeg raw execution failed: {Errors}", errors);
                            return Results.Problem("FFmpeg failed to process video speed. Technical details: " + errors, statusCode: 500);
                        }
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", "speed_" + dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing FFmpeg process directly");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChangeSpeed endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ReverseVideo(
            HttpContext context,
            [FromForm] ReverseVideoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var ffmpegFactory = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var command = ffmpegFactory.CreateReverseVideoCommand();
                    var result = await command.ExecuteAsync(new ReverseVideoModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to reverse video: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing reverse video request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReverseVideo endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ExtractFrame(
             HttpContext context,
             [FromForm] ExtractFrameDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);

                string extension = ".png";
                if (!string.IsNullOrEmpty(dto.OutputImageName) && dto.OutputImageName.Contains("."))
                {
                    extension = System.IO.Path.GetExtension(dto.OutputImageName);
                }
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
                    var command = ffmpegService.CreateExtractFrameCommand();

                    var result = await command.ExecuteAsync(new ExtractFrameModel
                    {
                        VideoName = videoFileName,
                        DesiredTime = string.IsNullOrEmpty(dto.DesiredTime) ? "00:00:01" : dto.DesiredTime,
                        OutputImageName = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to extract frame: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    string contentType = extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" ? "image/jpeg" : "image/png";
                    string downloadName = string.IsNullOrEmpty(dto.OutputImageName) ? "frame" + extension : dto.OutputImageName;

                    return Results.File(fileBytes, contentType, downloadName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing extract frame request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ExtractFrame endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ConvertVideo(
            HttpContext context,
            [FromForm] ConvertVideoDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.VideoFile == null)
                {
                    return Results.BadRequest("Video file is required");
                }

                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);

                string extension = string.IsNullOrEmpty(dto.TargetFormat) ? ".avi" : dto.TargetFormat;
                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }

                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                string fullInputPath = fileService.GetFullInputPath(videoFileName);
                string fullOutputPath = fileService.GetFullOutputPath(outputFileName);

                try
                {
                    var command = ffmpegService.CreateConvertVideoCommand();

                    var result = await command.ExecuteAsync(new ConvertVideoModel
                    {
                        InputVideoName = fullInputPath,
                        OutputVideoName = fullOutputPath
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to convert video: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    string downloadName = Path.GetFileNameWithoutExtension(dto.VideoFile.FileName) + extension;
                    return Results.File(fileBytes, "video/avi", downloadName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing video conversion request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ConvertVideo endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> MergeVideos(
            HttpContext context,
            [FromForm] MergeVideosDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

            try
            {
                if (dto.FirstVideoFile == null || dto.SecondVideoFile == null)
                {
                    return Results.BadRequest("Both video files are required");
                }

                string firstVideoFileName = await fileService.SaveUploadedFileAsync(dto.FirstVideoFile);
                string secondVideoFileName = await fileService.SaveUploadedFileAsync(dto.SecondVideoFile);

                string extension = Path.GetExtension(dto.FirstVideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                List<string> filesToCleanup = new List<string> { firstVideoFileName, secondVideoFileName, outputFileName };

                try
                {
                    var command = ffmpegService.CreateMergeVideosCommand();
                    var result = await command.ExecuteAsync(new MergeVideosModel
                    {
                        FirstInputFile = firstVideoFileName,
                        SecondInputFile = secondVideoFileName,
                        OutputFile = outputFileName,
                        Orientation = dto.Orientation ?? "horizontal",
                        VideoCodec = "libx264"
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to merge videos: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    string downloadName = "merged_" + dto.FirstVideoFile.FileName;
                    return Results.File(fileBytes, "video/mp4", downloadName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing merge videos request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MergeVideos endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }
    }
}