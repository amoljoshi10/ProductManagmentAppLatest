using System;

namespace ProductAPI
{
    public class Product
    {
        public int ProductID { get; set; }

        public string ProductDescription { get; set; }

        public string ProductCategory { get; set; }
        public bool isExpired { get; set;  }
        public DateTime ExpirationDate { get; set; }

    }
}
