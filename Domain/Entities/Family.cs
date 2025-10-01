using Domain.Behaviours;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Family : AudiTableEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;
        public string FamilyName { get; set; } = null!;
        
        public Guid? ImageId { get; set; }
        public UploadedFile? Image { get; set; }

        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
        public ICollection<User> Users { get; set; } = [];
        public ICollection<Member> Members { get; set; } = [];
    }
}
