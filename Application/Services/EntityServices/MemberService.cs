using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Dtos.Family;
using Application.Common.Models.Dtos.Member;
using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Create;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.EntityServices
{
    internal class MemberService(
        IMemberRepository memberRepository,
        IPermissionService permissionService,
        IMediator mediator,
        IMapper mapper) 
        : GenericEntityService<Member, CreateMemberDto, UpdateMemberDto, MemberViewModel>(memberRepository, permissionService, mapper), IMemberService
    {
        private readonly IMediator _mediator = mediator;
        public override async Task<MemberViewModel> CreateAsync(CreateMemberDto entityCreateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.CREATE, null))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<Member>(entityCreateDto);

            if (entityCreateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityCreateDto.Image, Alt = entity.FirstName, Description = entityCreateDto.Description }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.CreateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create entity.");

            return _mapper.Map<MemberViewModel>(result);
        }

        public override async Task<MemberViewModel> UpdateAsync(UpdateMemberDto entityUpdateDto, CancellationToken cancellationToken = default)
        {
            string entityTypeName = typeof(Family).Name;
            if (!await _permissionService.CheckPermission(entityTypeName, OperationType.UPDATE))
                throw new UnauthorizedAccessException("You do not have permission to create this entity.");

            var entity = _mapper.Map<Member>(entityUpdateDto);

            if (entityUpdateDto.Image != null)
            {
                var image = await _mediator.Send(new CreateUploadedFileCommand() { File = entityUpdateDto.Image, Alt = entity.FirstName, Description = entityUpdateDto.Description }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the file!");
                if (image.Data != null)
                    entity.ImageId = image.Data.Id;
            }

            var result = await _repository.UpdateAsync(entity, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to update entity.");

            return _mapper.Map<MemberViewModel>(result);
        }
    }
}
