using System;
using System.Collections.Generic;
using System.Text;
using Product.Business.Models;

namespace Product.Business.Interfaces
{
    public interface IProductExpirationService
    {
        ProductExpiryInfo GetProductExpirationDetails(int productID, string user);
    }
}
