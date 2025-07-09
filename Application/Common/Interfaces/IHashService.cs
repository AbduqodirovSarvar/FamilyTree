using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IHashService
    {
        string GetHash(string key);
        bool VerifyHash(string key, string hash);
    }
}
