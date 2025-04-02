using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.Auth
{
    public class AuthenticationResponse
    {
        public string Token { get; set; } = default!;
        public long ExpirationTime { get; set; }
    }
}
