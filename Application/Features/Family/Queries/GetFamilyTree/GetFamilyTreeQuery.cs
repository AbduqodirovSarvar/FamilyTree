using Application.Common.Models.Result;
using Application.Common.Models.ViewModels.Tree;
using MediatR;
using System;

namespace Application.Features.Family.Queries.GetFamilyTree
{
    public record GetFamilyTreeQuery : IRequest<Response<FamilyTreeViewModel>>
    {
        public Guid FamilyId { get; init; }
    }
}
