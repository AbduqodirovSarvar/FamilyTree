using Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class HashService : IHashService
    {
        public string Hash(string password)
        {
            throw new NotImplementedException();
        }

        public bool Verify(string password, string hashedPassword)
        {
            throw new NotImplementedException();
        }
    }
}
