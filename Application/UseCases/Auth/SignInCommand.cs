using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Auth
{
    public class SignInCommand : IRequest<SignInResponse> 
    {
        [Required]
        public string Login { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
