using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Security
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        string HashPassword(string password, int workFactor);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
