using Application.Common.Models.Common;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.ViewModels
{
    public record FamilyViewModel : BaseViewModel
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? FamilyName { get; init; }
        public Guid? ImageId { get; init; }
        public UploadedFileViewModel? Image { get; init; }
        public Guid? OwnerId { get; init; }
        public UserViewModel? Owner { get; init; }
        public ICollection<UserViewModel> Users { get; init; } = [];
        public ICollection<MemberViewModel> Members { get; init; } = [];
    }
}
