﻿using Application.Common.Interfaces;
using Application.Common.Models.ViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FileService : IFileService
    {
        public Task DeleteFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]?> GetFileAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<UploadedFile> SaveFileAsync(IFormFile fileBytes, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
