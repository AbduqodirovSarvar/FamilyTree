using Application.Common.Interfaces;
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

        public Task<string> SaveFileAsync(byte[] fileBytes, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
