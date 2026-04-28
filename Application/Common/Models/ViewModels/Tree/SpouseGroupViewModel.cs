using System.Collections.Generic;

namespace Application.Common.Models.ViewModels.Tree
{
    public record SpouseGroupViewModel
    {
        public TreeMemberViewModel Spouse { get; init; } = default!;
        public List<TreeNodeViewModel> Children { get; init; } = [];
    }
}
