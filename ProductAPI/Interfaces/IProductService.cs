using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductAPI.Interfaces
{
    public interface IProductService
    {
        IEnumerable<Product> GetProducts();
        Product GetProductById(int productID);
        void UpdateProduct(Product product, Product productToUpdate);
        void DeletProduct(Product product) ;
        string User
        {
            get;
            set;
        }


    }
}
