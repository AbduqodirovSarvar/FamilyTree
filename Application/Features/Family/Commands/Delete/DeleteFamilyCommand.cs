using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Commands.Delete
{
    public class DeleteFamilyCommand : IRequest<Response<bool>>
    {
        [Required]
        public Guid Id { get; set; }
    }
}
