using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class FileService(IWebHostEnvironment env) : IFileService
    {
        private readonly IWebHostEnvironment _env = env;

        private string GetUploadPath()
        {
            return Path.Combine(_env.WebRootPath, "uploads");
        }

        public async Task<UploadedFile> SaveFileAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var uploadPath = GetUploadPath();
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var extension = Path.GetExtension(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}_{fileName}{extension}";
            var filePath = Path.Combine(uploadPath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new UploadedFile
            {
                Id = Guid.NewGuid(),
                Name = uniqueName,
                Path = $"/uploads/{uniqueName}",
                Type = GetMimeType(extension),
                Size = file.Length,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<byte[]?> GetFileAsync(string fileName)
        {
            var filePath = Path.Combine(GetUploadPath(), fileName);

            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllBytesAsync(filePath);
        }

        public Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(GetUploadPath(), fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        private static FileMimeType GetMimeType(string extension)
        {
            return extension.ToLower() switch
            {
                ".png" => FileMimeType.ImagePng,
                ".jpg" or ".jpeg" => FileMimeType.ImageJpeg,
                ".gif" => FileMimeType.ImageGif,
                ".pdf" => FileMimeType.ApplicationPdf,
                ".docx" => FileMimeType.ApplicationDocx,
                ".xlsx" => FileMimeType.ApplicationXlsx,
                ".mp4" => FileMimeType.VideoMp4,
                ".mp3" => FileMimeType.AudioMp3,
                _ => FileMimeType.Unknown
            };
        }
    }
}
