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
using FFmpeg.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace FFmpeg.API.Endpoints
{
    public static class VideoEndpoints
    {
        public static void MapEndpoints(this WebApplication app)
        {
            app.MapPost("/api/video/watermark", AddWatermark)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600)); // 100 MB

            app.MapPost("/api/video/brightness-contrast", ChangeBrightnessContrast)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));

            app.MapPost("/api/video/extract-frame", ExtractFrame)
                .DisableAntiforgery()
                .WithMetadata(new RequestSizeLimitAttribute(104857600));
            app.MapPost("/api/video/gif-video", CreateGif)
               .DisableAntiforgery()
               .WithMetadata(new RequestSizeLimitAttribute(104857600));
        }

        private static async Task<IResult> AddWatermark(
            HttpContext context,
            [FromForm] WatermarkDto dto)
        {
            var fileService = context.RequestServices.GetRequiredService<IFileService>();
            var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // or a specific logger type

            try
            {
                // Validate request
                if (dto.VideoFile == null || dto.WatermarkFile == null)
                {
                    return Results.BadRequest("Video file and watermark file are required");
                }

                // Save uploaded files
                string videoFileName = await fileService.SaveUploadedFileAsync(dto.VideoFile);
                string watermarkFileName = await fileService.SaveUploadedFileAsync(dto.WatermarkFile);

                // Generate output filename
                string extension = Path.GetExtension(dto.VideoFile.FileName);
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(extension);

                // Track files to clean up
                List<string> filesToCleanup = new List<string> { videoFileName, watermarkFileName, outputFileName };

                try
                {
                    // Create and execute the watermark command
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

                    // Read the output file
                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                    // Clean up temporary files
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    // Return the file
                    return Results.File(fileBytes, "video/mp4", dto.VideoFile.FileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing watermark request");
                    // Clean up on error
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
                    var builder = context.RequestServices.GetRequiredService<FFmpeg.Infrastructure.Commands.ICommandBuilder>();
                    var command = new FFmpeg.Infrastructure.Commands.BrightnessContrastCommand(executor, builder);

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
                    //var executor = context.RequestServices.GetRequiredService<FFmpegExecutor>();
                    //var builder = context.RequestServices.GetRequiredService<FFmpeg.Infrastructure.Commands.ICommandBuilder>();
                    //var command = new FFmpeg.Infrastructure.Commands.ExtractFrameCommand(executor, builder);

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
        private static async Task<IResult> CreateGif(
        HttpContext context,
        [FromForm] GifFromVideoDto dto)
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
                string outputFileName = await fileService.GenerateUniqueFileNameAsync(".gif");

                List<string> filesToCleanup = new List<string> { videoFileName, outputFileName };

                try
                {
                    var ffmpegService = context.RequestServices.GetRequiredService<IFFmpegServiceFactory>();
                    var command = ffmpegService.CreateGifFromVideoCommand();

                    var result = await command.ExecuteAsync(new GifFromVideoModel
                    {
                        InputFile = videoFileName,
                        OutputFile = outputFileName
                    });

                    if (!result.IsSuccess)
                    {
                        logger.LogError("FFmpeg command failed: {ErrorMessage}, Command: {Command}",
                            result.ErrorMessage, result.CommandExecuted);
                        return Results.Problem("Failed to create GIF: " + result.ErrorMessage, statusCode: 500);
                    }

                    byte[] fileBytes = await fileService.GetOutputFileAsync(outputFileName);

                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);

                    return Results.File(fileBytes, "image/gif", dto.OutputGifName ?? "output.gif");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing gif request");
                    _ = fileService.CleanupTempFilesAsync(filesToCleanup);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreateGif endpoint");
                return Results.Problem("An error occurred: " + ex.Message, statusCode: 500);
            }
        }
    }
}