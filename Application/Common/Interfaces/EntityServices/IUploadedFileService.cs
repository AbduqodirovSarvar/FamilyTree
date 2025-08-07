using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Models;
using Application.Common.Models.Dtos.UploadedFile;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices
{
    internal interface IUploadedFileService 
        : IGenericEntityService<UploadedFile, CreateUploadedFileDto, UpdateUploadedFileDto, UploadedFileViewModel>
    {
    }
}
