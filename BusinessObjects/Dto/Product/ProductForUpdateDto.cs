using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dto.Product
{
    public class ProductForUpdateDto
    {
        public int? CategoryId { get; set; }

        public string ProductName { get; set; }

        public string Weight { get; set; }

        public decimal? UnitPrice { get; set; }

        public int UnitsInStock { get; set; }
    }
}
