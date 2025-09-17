using Application.Common.Interfaces.Repositories;
using Domain.Entities;
using Persistence.Services.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services.Repositories
{
    public class UploadedFileRepository : GenericRepository<UploadedFile>, IUploadedFileRepository
    {
    }
}
