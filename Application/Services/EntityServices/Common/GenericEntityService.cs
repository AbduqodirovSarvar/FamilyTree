using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Interfaces.Repositories.Common;
using Application.Common.Models;
using Application.Common.Models.Common;
using Application.Common.Models.Dtos.Common;
using Application.Common.Models.Result;
using AutoMapper;
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
    public abstract class GenericEntityService<TEntity, TCreateDto, TUpdateDto, TEntityViewModel>(
        IGenericRepository<TEntity> repository, 
        IPermissionService permissionService,
        IMapper mapper)
        : IGenericEntityService<TEntity, TCreateDto, TUpdateDto, TEntityViewModel>
        where TEntity : AudiTableEntity
        where TEntityViewModel : BaseViewModel
        where TCreateDto : BaseCreateDto
        where TUpdateDto : BaseUpdateDto
    {
        protected readonly IGenericRepository<TEntity> _repository = repository;
        protected readonly IPermissionService _permissionService = permissionService;
        protected readonly IMapper _mapper = mapper;

        public async Task<TEntityViewModel> CreateAsync(TCreateDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.CREATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<TEntity>(entityCreateDto);

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity."); 

            return _mapper.Map<TEntityViewModel>(result);
        }

        public async Task<TEntityViewModel> UpdateAsync(TUpdateDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<TEntity>(entityUpdateDto);

            var result = await _repository.UpdateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<TEntityViewModel>(result);

        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.DELETE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _repository.GetAsync(x => x.Id == id, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");

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

        public async Task<Response<List<TEntityViewModel>>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var (data, totalCount) = await _repository.GetPaginatedAsync(predicate, pageIndex, pageSize, null, cancellationToken);

            return Response<List<TEntityViewModel>>.Ok(_mapper.Map<List<TEntityViewModel>>(data), pageIndex, pageSize, totalCount);
        }

        public async Task<TEntityViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = await _repository.GetAsync(x => x.Id == id, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");

            return _mapper.Map<TEntityViewModel>(entity);
        }

        public async Task<TEntityViewModel> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(TEntity).Name;
            if (!_permissionService.CheckPermission(entityTypeName, OperationType.GET))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity =  await _repository.GetAsync(predicate, cancellationToken)
                                ?? throw new KeyNotFoundException("Entity not found");
            return _mapper.Map<TEntityViewModel>(entity);
        }
    }
}
