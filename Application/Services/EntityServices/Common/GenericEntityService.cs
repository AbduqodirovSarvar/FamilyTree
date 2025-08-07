using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Interfaces.Repositories.Common;
using Application.Common.Models.Result;
using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices.Common
{
    public abstract class GenericEntityService<TEntity>(
        IGenericRepository<TEntity> repository, 
        IPermissionService permissionService)
        : IGenericEntityService<TEntity>
        where TEntity : AudiTableEntity
    {
        protected readonly IGenericRepository<TEntity> _repository = repository;
        protected readonly IPermissionService _permissionService = permissionService;

        public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.CREATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            return await _repository.CreateAsync(entity, cancellationToken);
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            return await _repository.UpdateAsync(entity, cancellationToken);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.DELETE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _repository.GetAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
                return false;

            return await _repository.DeleteAsync(entity, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _repository.GetAsync(predicate, cancellationToken);
            return entity != null;
        }

        public async Task<Response<List<TEntity>>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var (data, totalCount) = await _repository.GetPaginatedAsync(predicate, pageIndex, pageSize, null, cancellationToken);
            return Response<List<TEntity>>.Ok(data, pageIndex, pageSize, totalCount);
        }

        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            return await _repository.GetAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            return await _repository.GetAsync(predicate, cancellationToken)
                   ?? throw new KeyNotFoundException("Entity not found");
        }

        //protected Permission GetPermission(string operation)
        //{
        //    var entityType = typeof(TEntity).Name;

        //    return (entityType, operation) switch
        //    {
        //        ("Family", "GET") => Permission.GET_FAMILY,
        //        ("Family", "CREATE") => Permission.CREATE_FAMILY,
        //        ("Family", "UPDATE") => Permission.UPDATE_FAMILY,
        //        ("Family", "DELETE") => Permission.DELETE_FAMILY,

        //        ("Member", "GET") => Permission.GET_MEMBER,
        //        ("Member", "CREATE") => Permission.CREATE_MEMBER,
        //        ("Member", "UPDATE") => Permission.UPDATE_MEMBER,
        //        ("Member", "DELETE") => Permission.DELETE_MEMBER,

        //        ("User", "GET") => Permission.GET_USER,
        //        ("User", "CREATE") => Permission.CREATE_USER,
        //        ("User", "UPDATE") => Permission.UPDATE_USER,
        //        ("User", "DELETE") => Permission.DELETE_USER,

        //        ("Family", "GET") => Permission.GET_FAMILY,
        //        ("Family", "CREATE") => Permission.CREATE_FAMILY,
        //        ("Family", "UPDATE") => Permission.UPDATE_FAMILY,
        //        ("Family", "DELETE") => Permission.DELETE_FAMILY,

        //        _ => throw new ArgumentException($"Unsupported permission for type: {entityType} and operation: {operation}")
        //    };
        //}

    }
}
