using Application.Common.Models.Common;
using Application.Common.Models.Dtos.Common;
using Application.Common.Models.Result;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces.EntityServices.Common
{
    public interface IGenericEntityService<TEntity, TCreateDto, TUpdateDto, TEntityViewModel>
        where TEntity : AudiTableEntity
        where TEntityViewModel : BaseViewModel
        where TCreateDto : BaseCreateDto
        where TUpdateDto : BaseUpdateDto
    {
        Task<TEntityViewModel> CreateAsync(TCreateDto entity, CancellationToken cancellationToken = default);
        Task<TEntityViewModel> UpdateAsync(TUpdateDto entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TEntityViewModel> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<TEntityViewModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Response<List<TEntityViewModel>>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
