using Domain.Enums;
using System;

namespace Application.Common.Models.ViewModels.Tree
{
    public record TreeMemberViewModel
    {
        public Guid Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public Gender Gender { get; init; }
        public DateOnly BirthDay { get; init; }
        public DateOnly? DeathDay { get; init; }
        public Guid? ImageId { get; init; }
        public string? ImageUrl { get; init; }
    }
}
