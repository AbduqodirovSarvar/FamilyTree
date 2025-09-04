using Application.Features.Family.Commands.Create;
using Application.Features.Family.Commands.Delete;
using Application.Features.Family.Commands.Update;
using Application.Features.Family.Queries.GetList;
using Application.Features.Family.Queries.GetOne;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Common;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController(IMediator mediator) 
        : BaseServiceController<
            CreateFamilyCommand,
            UpdateFamilyCommand,
            DeleteFamilyCommand,
            GetFamilyQuery,
            GetFamilyListQuery>(mediator)
    {
    }
}
