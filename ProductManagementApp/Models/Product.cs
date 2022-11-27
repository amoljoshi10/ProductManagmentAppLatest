using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProductManagementApp.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductDescription { get; set; }

        public string ProductCategory { get; set; }
        public bool isExpired { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

    }
}
