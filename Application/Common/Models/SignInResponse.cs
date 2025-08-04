using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    public class SignInResponse
    {
        public TokenViewModel? Token { get; set; }
        public FamilyViewModel? Family { get; set; }
    }
}
