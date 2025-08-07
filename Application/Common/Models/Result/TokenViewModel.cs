using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Result
{
    public class TokenViewModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
