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
using Microsoft.AspNetCore.Builder;

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

            // הניתוב החדש שלנו לשינוי מהירות
            app.MapPost("/api/video/change-speed", ChangeSpeed)
                .DisableAntiforgery();
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

        // הפונקציה החדשה שלנו לטיפול בשינוי מהירות
        private static async Task<IResult> ChangeSpeed(
            [FromBody] ChangeSpeedRequest request,
            [FromServices] IVideoService videoService,
            [FromServices] ILogger<Program> logger)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.InputPath))
                {
                    return Results.BadRequest("Invalid request data. InputPath is required.");
                }

                await videoService.ChangeVideoSpeedAsync(request.InputPath, request.SpeedMultiplier, request.OutputPath);
                return Results.Ok(new { Message = "Video speed changed successfully." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing video speed change");
                return Results.Problem("Error processing video: " + ex.Message, statusCode: 500);
            }
        }
    }
}