using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eStore.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Member ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid member")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        public DateTime OrderDate { get; set; }

        public DateTime? RequiredDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Freight must be 0 or greater")]
        public decimal? Freight { get; set; }
    }
} 