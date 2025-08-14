using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.ViewModels
{
    public record EnumViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
