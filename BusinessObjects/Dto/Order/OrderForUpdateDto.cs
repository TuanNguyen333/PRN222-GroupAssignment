using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.Order
{
    public class OrderForUpdateDto
    {
        public int MemberId { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public decimal? Freight { get; set; }
    }
}
