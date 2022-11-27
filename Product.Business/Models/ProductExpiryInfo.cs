using System;
using System.Collections.Generic;

using System.Text;

namespace Product.Business.Models
{
    public class ProductExpiryInfo
    {
        public int ProductID { get; set; }
        public bool isExpired { get; set; }
        
        public DateTime ExpirationDate { get; set; }
    }
}
