using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Family> Families { get; set; }
        DbSet<UploadedFile> UploadedFiles { get; set; }
        DbSet<User> Users { get; set; }

        // SaveChangesAsync method to commit changes to the database
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        DbSet<TEntity> Set<TEntity>() where TEntity : AudiTableEntity;
    }
}
