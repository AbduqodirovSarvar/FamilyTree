using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntitySaveChangesInterceptor auditableInterceptor) : DbContext(options)
    {
        private readonly AuditableEntitySaveChangesInterceptor _auditableInterceptor = auditableInterceptor;
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserRolePermission> UserRolePermissions { get; set; }
        public virtual DbSet<Family> Families { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<UploadedFile> UploadedFiles { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.AddInterceptors(_auditableInterceptor);
        }
    }
}
