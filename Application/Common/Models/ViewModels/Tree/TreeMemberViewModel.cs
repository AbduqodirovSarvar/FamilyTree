using Domain.Enums;
using System;

namespace Application.Common.Models.ViewModels.Tree
{
    public record TreeMemberViewModel
    {
        public Guid Id { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Description { get; init; }
        public Gender Gender { get; init; }
        public DateOnly BirthDay { get; init; }
        public DateOnly? DeathDay { get; init; }
        public Guid? ImageId { get; init; }
        public string? ImageUrl { get; init; }
        /// <summary>"FirstName LastName" of the father, when the father exists in the same family.</summary>
        public string? FatherName { get; init; }
        public string? MotherName { get; init; }
        public string? SpouseName { get; init; }
    }
}
