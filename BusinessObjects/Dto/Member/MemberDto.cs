using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.Member
{
    public class MemberDto
    {
        public int MemberId { get; set; }

        public string Email { get; set; }

        public string CompanyName { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
    }
}
