﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Request
{
    public record BaseGetOneQuery
    {
        [Required(ErrorMessage = "Id is required.")]
        public Guid Id { get; init; }
    }
}
