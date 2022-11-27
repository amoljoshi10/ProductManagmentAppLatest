using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ProductAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using CachingService;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("products") ]
    public class ProductsController : ControllerBase
    {
        private IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsController> _logger;
        private TelemetryClient _telemetryClient;
        //private readonly IRedisCacheService _cache;
        private readonly ICacheService _cache;
        private string _user;
        public ProductsController(IProductService productService, IConfiguration configuration, ILogger<ProductsController> logger, 
                                TelemetryClient telemetryClient,
                                //IRedisCacheService cache)
                                ICacheService cache)
        {
            _productService = productService;
            _configuration = configuration;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _cache = cache;
            _user = _configuration["User2"];
            _productService.User = _user;
        }
        [HttpGet()]
        public IEnumerable<Product> Get()
        {
            var instrumentationKey = _configuration["ApplicationInsights:InstrumentationKey"];
            SendEventTelemetry("Get", "ProductListRequested","User requested products", _user);

            _logger.LogInformation("In ProductAPI_ProductController");
            _logger.LogInformation("{CurrentMethodName}):", "Get");
            
            IEnumerable<Product> products = null;

            try
            {
                products = _productService.GetProducts();
                SendEventTelemetry("Get", "ProductListReceived", $"Total Product Count retreived :{products.ToList().Capacity}", _user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.Source);
                throw;
            }
            return products;
        }

        private void SendEventTelemetry(string methodName,string eventName ,string eventDescription, string userName)
        {
            var _eventProperties = new Dictionary<string, string>();
            //Add in Event Telemetry using Parent Telemetry Id from Redis cache to link all event telemetries for given request

            var eventTelemetryParentCacheKey = "EventTelemetryParentID";
            var eventTelemetryParentCacheValue = _cache.GetData(eventTelemetryParentCacheKey);
            _eventProperties.Add(eventTelemetryParentCacheKey, eventTelemetryParentCacheValue);

            var redisCacheConnection = _configuration["RedisCacheConnection"];
            _eventProperties.Add("redisCacheConnection", redisCacheConnection);

            _eventProperties.Add("EventTelemetryRootName", "ProductManagementEvents");
            _eventProperties.Add("AppInsightTelemetryEventSource", "ProductAPI");
            _eventProperties.Add("Component", "ProductsController");
            _eventProperties.Add("MethodName", methodName);
            _eventProperties.Add("EventDescription", eventDescription);
            _eventProperties.Add("User", userName);
            _eventProperties.Add("ApplicationTier", "Service");
            _telemetryClient.TrackEvent(eventName, _eventProperties);
        }

        //[HttpGet("{productId}")]
        //public IActionResult GetById(int productId)
        //{

        //    //Request.Headers.TryGetValue("x-security-header", out var headerValue);

        //    // read your secret from Azure Key Vault
        //    //string kvUri =
        //    //"https://kv-for-apim.vault.azure.net/";

        //    //SecretClient client =
        //    //    new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        //    //var secret= client.GetSecretAsync("securityHeader").Result.Value.Value;

        //    //if (!headerValue.Equals(secret))
        //    //    return Unauthorized("Direct Access to API is restricted");

        //    var instrumentationKey = _configuration["ApplicationInsights:InstrumentationKey"];

        //    var product=_productService.GetProductById(productId);
        //    SendEventTelemetry("GetById", "ProductDetailsFetched_API", $"Product Details found for product id:{product.ProductID}", _user);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(product);

        //}
        [HttpGet("{productId}")]
        public IActionResult GetById(int productId)
        {
            var instrumentationKey = _configuration["ApplicationInsights:InstrumentationKey"];

            var product = _productService.GetProductById(productId);
            SendEventTelemetry("GetById", "ProductDetailsFetched_API", $"Product Details found for product id:{product.ProductID}", _user);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPut("{productId}")]
        public IActionResult UpdateProduct(int productId, [FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var productEntity = _productService.GetProductById(productId);
            SendEventTelemetry("UpdateProduct", "FoundProductToBUpdated_API", $"Found productid {product.ProductID} which user requested to update", _user);
            if (productEntity == null)
            {
                return NotFound();
            }
            _productService.UpdateProduct(productEntity, product);
            SendEventTelemetry("UpdateProduct", "ProductUpdatedFromProductAPI", $"Product details updated for productid {product.ProductID}", _user);
            return NoContent();
        }

        [HttpDelete("{productId}")]
        public IActionResult DeleteProduct(int productId)
        {
            var productEntity = _productService.GetProductById(productId);
            if (productEntity == null)
            {
                return NotFound();
            }
            _productService.DeletProduct(productEntity);
            return Ok(true);
        }

        [HttpGet("{kvname}/{secret}")]
        public string GetSecretsAndKeys(string kvname,string secret)
        {
            // read your secret from Azure Key Vault
            string kvUri =
                    "https://kv-for-apim.vault.azure.net/";

            SecretClient client =
                new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            return client.GetSecretAsync(secret).Result.Value.Value;
        }

        private void UpdateEventTelemetryContext(TelemetryContext context, string operationParentId, string operationID,string operationName)
        {
            context.Operation.ParentId = operationParentId;
            context.Operation.Id = operationID;
            context.Operation.Name = operationName;
        }


    }
}
