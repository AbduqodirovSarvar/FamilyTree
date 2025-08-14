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
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? FamilyName { get; set; }

        public Guid? ImageId { get; set; }
        public UploadedFileViewModel? Image { get; set; }

        public Guid? OwnerId { get; set; }
        public UserViewModel? Owner { get; set; }
        public ICollection<UserViewModel> Users { get; set; } = [];
        public ICollection<MemberViewModel> Members { get; set; } = [];
    }
}
