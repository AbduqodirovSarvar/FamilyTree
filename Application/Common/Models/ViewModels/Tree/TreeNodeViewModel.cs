using System.Collections.Generic;

namespace Application.Common.Models.ViewModels.Tree
{
    public record TreeNodeViewModel
    {
        public TreeMemberViewModel Primary { get; init; } = default!;
        public List<SpouseGroupViewModel> Spouses { get; init; } = [];
        public List<TreeNodeViewModel> CommonChildren { get; init; } = [];
    }
}
