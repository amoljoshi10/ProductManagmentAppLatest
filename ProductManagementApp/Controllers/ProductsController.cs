using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using ProductManagementApp.Models;
using CachingService;

namespace ProductManagementApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsController> _logger;
        private TelemetryClient _telemetryClient;
        //private readonly IRedisCacheService _cache;
        private readonly ICacheService _cache;
        private string _user;
        public ProductsController(IConfiguration configuration, 
            ILogger<ProductsController> logger, 
            TelemetryClient telemetryClient,
            //IRedisCacheService cache,
            ICacheService cache
            )
        {
            _configuration = configuration;
            _logger = logger;
            _telemetryClient = telemetryClient;
            _cache = cache;
            _user = _configuration["User1"];
        }
        public async Task<IActionResult> List()
        {
            List<Product> products = new List<Product>();
            var apiUrl = _configuration["APIURI"];
            var cacheKey = "GET_ALL_PRODUCTS";
            var eventTelemetryParentCacheKey = "EventTelemetryParentID";
            var eventTelemetryParentCacheValue = Guid.NewGuid().ToString();
            
            _cache.SetData(eventTelemetryParentCacheKey, eventTelemetryParentCacheValue);

            SendEventTelemetry(apiUrl, "List", "ProductListRequestedFromWebApp", "Products List Requested", eventTelemetryParentCacheValue,_user);
            _logger.LogInformation("In ProductManagmentWeb APP_ProductsController");
            _logger.LogInformation("{Controller}: {CurrentMethodName}):", "Products", "List");
            _logger.LogInformation("Calling URL {DestinationURI})", apiUrl);

            try
            {
                var cachedData = _cache.GetData(cacheKey);
                if (cachedData != null)
                {
                    products = JsonConvert.DeserializeObject<List<Product>>(cachedData);
                    _logger.LogInformation("Products {ProductCount} served from cache", products.Capacity);
                }
                else
                {
                    // If data isn't exist in cache, then fetch data from API
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(apiUrl))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            products = JsonConvert.DeserializeObject<List<Product>>(apiResponse);
                        }
                    }
                    _logger.LogInformation("Products {ProductCount} served from API", products.Capacity);
                     var cachedDataString = JsonConvert.SerializeObject(products);

                    _cache.SetData(cacheKey, cachedDataString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.Source);
                throw;
            }
            return View(products);
        }


        // GET: ProductsController/Details/1
        public async Task<IActionResult> Details(int id)
        {
            Product product = null;
            var apiUrl = _configuration["APIURI"];
            var eventTelemetryParentCacheKey = "EventTelemetryParentID";
            var eventTelemetryParentCacheValue = _cache.GetData(eventTelemetryParentCacheKey);

            SendEventTelemetry(apiUrl, "Details", "ProductDetailsRequested_UI", $"Product Details requested for product:{id}", eventTelemetryParentCacheValue,_user);
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(apiUrl + "/" +id))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    product = JsonConvert.DeserializeObject<Product>(apiResponse);
                }
            }
            return View(product);
        }
        private void SendEventTelemetry(string apiUrl,
                                        string methodName, 
                                        string eventName, 
                                        string eventDescription,
                                        string eventTelemetryParentID,
                                        string userName)
        {
            var _eventProperties = new Dictionary<string, string>();
            var eventTelemetryParentCacheKey = "EventTelemetryParentID";
                
            _eventProperties.Add(eventTelemetryParentCacheKey, eventTelemetryParentID);
            
            _eventProperties.Add("EventTelemetryRootName", "ProductManagementEvents");

            var redisCacheConnection = _configuration["RedisCacheConnection"];

            _eventProperties.Add("redisCacheConnection", redisCacheConnection);

            _eventProperties.Add("AppInsightTelemetryEventSource", "ProductManagementApp");
            _eventProperties.Add("Component", "ProductManagementApp_ProductsController");
            _eventProperties.Add("TargetURL", apiUrl);
            _eventProperties.Add("MethodName", methodName);
            _eventProperties.Add("EventDescription", eventDescription);
            _eventProperties.Add("User", userName);
            _eventProperties.Add("ApplicationTier", "UI");
            _telemetryClient.TrackEvent(eventName, _eventProperties);
        }
        // GET: ProductsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductsController/Edit/1
        public async Task<ActionResult> Edit(int ?id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                Product product = null;
                var apiUrl = _configuration["APIURI"];
                var eventTelemetryParentCacheKey = "EventTelemetryParentID";
                var eventTelemetryParentCacheValue = _cache.GetData(eventTelemetryParentCacheKey);
                SendEventTelemetry(apiUrl, "Edit", "EditProductViewRequested", $"User selected to edit product id:{id}", eventTelemetryParentCacheValue,_user);
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        product = JsonConvert.DeserializeObject<Product>(apiResponse);
                    }
                }
                return View(product);
            }
            catch
            {
                return View();
            }
        }


        // POST: ProductsController/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int ?id, Product updatedProduct)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            try
            {

                Product product = null;
                var apiUrl = _configuration["APIURI"];
                var eventTelemetryParentCacheKey = "EventTelemetryParentID";
                var eventTelemetryParentCacheValue = _cache.GetData(eventTelemetryParentCacheKey);
                string updatedProductJson;
                SendEventTelemetry(apiUrl, "Edit_Post", "ProductUpdatesPostedFromUI", $"User performs some updates on product details for product id:{id}", eventTelemetryParentCacheValue,_user);
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        product = JsonConvert.DeserializeObject<Product>(apiResponse);
                        product.ProductDescription = updatedProduct.ProductDescription;
                        product.isExpired = updatedProduct.isExpired;
                        product.ExpirationDate = updatedProduct.ExpirationDate;
                        updatedProductJson = JsonConvert.SerializeObject(product);
                    }
                    using (var response = await httpClient.PutAsync(apiUrl + "/" + id, new StringContent(updatedProductJson, Encoding.UTF8, "application/json")))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        product = JsonConvert.DeserializeObject<Product>(apiResponse);
                    }
                }
                SendEventTelemetry(apiUrl, "Edit_Post", "EditProductCompletedFromUI", $"Product details updated for product id:{id}", eventTelemetryParentCacheValue,_user);
                return RedirectToAction(nameof(List));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        private void UpdateEventTelemetryContext(TelemetryContext context, string operationParentId, string operationID, string operationName)
        {
            context.Operation.ParentId = operationParentId;
            context.Operation.Id = operationID;
            context.Operation.Name = operationName;
        }

    }
}
