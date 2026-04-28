using System;
using System.Collections.Generic;

namespace Application.Common.Models.ViewModels.Tree
{
    public record FamilyTreeViewModel
    {
        public Guid FamilyId { get; init; }
        /// <summary>Display name (e.g. "Karimovlar oilasi").</summary>
        public string? Name { get; init; }
        /// <summary>Family surname used for hub labels (e.g. "Karimov").</summary>
        public string? FamilyName { get; init; }
        public int TotalMembers { get; init; }
        public List<TreeNodeViewModel> Roots { get; init; } = [];
    }
}
