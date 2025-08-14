using Application.Common.Models.Request;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Queries.GetOne
{
    public record GetFamilyQuery : BaseGetOneQuery, IRequest<Response<FamilyViewModel>>
    {
        public new Guid? Id { get; init; }
        public string? FamilyName { get; set; } = null;
    }
}
