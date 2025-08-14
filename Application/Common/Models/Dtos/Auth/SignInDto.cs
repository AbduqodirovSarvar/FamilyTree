using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.Auth
{
    public abstract record SignInDto
    {
        [Required(ErrorMessage = "Login is required.")]
        public string Login { get; init; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; init; } = null!;
    }
}
