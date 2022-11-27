using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Newtonsoft.Json;
using DistributedCachingService;

namespace ProductManagementApp.Telemetry
{
    public  class CorelationTelemetryProvider
    {
        private const string _cacheKey = "OperationId";
        private  readonly IRedisCacheService _cache;
        public CorelationTelemetryProvider(IRedisCacheService  cache)
        {
            _cache = cache;
        }

        public  void SetOperationId(string operationid)
        {
            //var options = new DistributedCacheEntryOptions()
            //           .SetAbsoluteExpiration(DateTime.Now.AddMinutes(2))
            //           .SetSlidingExpiration(TimeSpan.FromMinutes(1));

            //var dataToCach = Encoding.UTF8.GetBytes(operationid);

            //_cache.SetData(_cacheKey, dataToCach, options);
            _cache.SetData(_cacheKey, operationid);
        }

        public  string GetOperationId()
        {
            var cachedData =  _cache.GetData(_cacheKey);
            string dataResult=Guid.NewGuid().ToString();

            if (cachedData != null)
            {
                // If data found in cache, encode and deserialize cached data
                //var cachedDataString = Encoding.UTF8.GetString(cachedData);
                dataResult = JsonConvert.DeserializeObject<string>(cachedData);
            }
            return dataResult;
        }
    }
}
