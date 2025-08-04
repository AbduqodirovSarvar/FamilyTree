using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Models
{
    public sealed class JWTConfiguration
    {
        public string ValidIssuer { get; set; } = null!;
        public string ValidAudience { get; set; } = null!;
        public string Secret { get; set; } = null!;
    }
}
