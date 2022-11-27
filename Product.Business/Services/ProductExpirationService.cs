using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using Product.Business.Interfaces;
using Product.Business.Models;
using CachingService;

namespace Product.Business.Services
{
    public class ProductExpirationService : IProductExpirationService
    {
        private readonly IConfiguration _configuration;
        private TelemetryClient _telemetryClient;
        public static List<ProductExpiryInfo> _productExpiryInfoDetails;
        //private readonly IRedisCacheService _cache;
        private readonly ICacheService _cache;
        public ProductExpirationService(IConfiguration configuration,  TelemetryClient telemetryClient,
                                        ICacheService cache)
                                        //IRedisCacheService cache)
        {
            _configuration = configuration;
            _telemetryClient = telemetryClient;
            _cache = cache;
        }

        public ProductExpiryInfo GetProductExpirationDetails(int productID,string user)
        {

            ProductExpiryInfo result = null;


            var instrumentationKey = _configuration["ApplicationInsights:InstrumentationKey"];

            SendEventTelemetry("GetProductExpirationDetails", "ProductExpirationDetailsRequested", $"Product Expiration details requested for productid {productID}",user);

            if (_productExpiryInfoDetails != null)
            {
                result = _productExpiryInfoDetails.Find(p => p.ProductID == productID);
            }
            else
            {
                InitializeProductExpiryInfoDetails();
                result = _productExpiryInfoDetails.Find(p => p.ProductID == productID);
            }
            return result;
        }

        private void SendEventTelemetry(string methodName, string eventName, string eventDescription,string user)
        {
            var _eventProperties = new Dictionary<string, string>();
            var eventTelemetryParentCacheKey = "EventTelemetryParentID";
            var eventTelemetryParentCacheValue = _cache.GetData(eventTelemetryParentCacheKey);
            _eventProperties.Add(eventTelemetryParentCacheKey, eventTelemetryParentCacheValue);

            //Add in Event Telemetry using Parent Telemetry Id from Redis cache to link all event telemetries for given request

            _eventProperties.Add("EventTelemetryRootName", "ProductManagementEvents");
            _eventProperties.Add("AppInsightTelemetryEventSource", "Product.Business");
            _eventProperties.Add("Component", "ProductExpirationService");
            _eventProperties.Add("MethodName", methodName);
            _eventProperties.Add("EventDescription", eventDescription);
            _eventProperties.Add("User", user);
            _eventProperties.Add("ApplicationTier", "Business");

            _telemetryClient.TrackEvent(eventName, _eventProperties);
        }

        private void InitializeProductExpiryInfoDetails()
        {
            if (_productExpiryInfoDetails != null)
            {
                return ;
            }
            _productExpiryInfoDetails = new List<ProductExpiryInfo>();

            for (int i = 1; i <= 20; i++)
            {
                if(i<=10)
                {
                    _productExpiryInfoDetails.Add(new ProductExpiryInfo { ProductID = i, isExpired = false });
                }
                else
                {
                    _productExpiryInfoDetails.Add(new ProductExpiryInfo { ProductID = i, isExpired = true,ExpirationDate=DateTime.Now });
                }    
            }
        }

    }
}
