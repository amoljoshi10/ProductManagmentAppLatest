using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductAPI.Interfaces;
using Product.Business.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CachingService;

namespace ProductAPI.Services
{
    public class ProductService :  IProductService
    {

        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductService> _logger;
        private TelemetryClient _telemetryClient;
        public static List<Product> _products;
        private IWebHostEnvironment _hostingEnvironment;
        //private readonly IRedisCacheService _cache;
        private readonly ICacheService _cache;

        public ProductService(IConfiguration configuration, ILogger<ProductService> logger, TelemetryClient telemetryClient, IWebHostEnvironment env,
                                ICacheService cache)
                                //IRedisCacheService cache)
        {
            _configuration = configuration;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _hostingEnvironment = env;
            _cache = cache;
        }
        public IEnumerable<Product> GetProducts()
        {
            if (_products != null)
            {
                return _products;
            }
            _products = new List<Product>();

            var folderDetails = Path.Combine(_hostingEnvironment.ContentRootPath, "TestData\\productstestdata.json");

            var jsonString = System.IO.File.ReadAllText(folderDetails);

            _products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(jsonString);

            _logger.LogInformation("In ProductAPI_ProductService");
            _logger.LogInformation("Total Count {ProductCount}):", _products.Capacity);
            return _products;
        }
        public Product GetProductById(int productID)
        {
            Product product = _products.FirstOrDefault(p => p.ProductID == productID);
            var productExpiration = new ProductExpirationService(_configuration, _telemetryClient, _cache);
            var productExpirationInfo = productExpiration.GetProductExpirationDetails(product.ProductID, User);
            product.isExpired = productExpirationInfo.isExpired;
            product.ExpirationDate = productExpirationInfo.ExpirationDate;
            _logger.LogInformation("Product Found {ProductId}):", productID);
            return product;

        }
        public void UpdateProduct(Product product, Product updatedProduct)
        {
            _products.Remove(product);
            _products.Add(updatedProduct);
            var sortedProducts = _products.OrderBy(p => p.ProductID);
            string jsonString;
            var folderDetails = Path.Combine(_hostingEnvironment.ContentRootPath, "TestData\\productstestdata.json");

            if (sortedProducts != null)
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(sortedProducts);
            else
                jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(_products);

            File.WriteAllText(folderDetails, jsonString);
        }

        public string User { get; set; }

       

        
        public void AddProduct(Product product)
        {
            _products.Add(product);
        }
        
        public void DeletProduct(Product product)
        {
            _products.Remove(product);
        }

    }
}
