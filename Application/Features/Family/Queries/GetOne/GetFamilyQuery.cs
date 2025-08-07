using Application.Common.Models;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Queries.GetOne
{
    public record GetFamilyQuery : IRequest<Response<FamilyViewModel>>
    {
        public Guid? Id { get; init; } = null;
        public string FamilyName { get; set; } = null!;
    }
}
