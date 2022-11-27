using StackExchange.Redis;
using System;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CachingService
{
    public class RedisCacheService : IRedisCacheService
    {

        private readonly ILogger<RedisCacheService> _logger;
        private readonly IConfiguration _configuration;
        public static int RetryMaxAttempts => 5;

        public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var redisCacheConnection = _configuration["RedisCacheConnection"];
            _logger.LogInformation("RedisCacheService.GetData Retrived Redis Cache Connection Keyfrom cache:{redisCacheConnection}", redisCacheConnection);
            RedisConnectionManager.InitializeConfiguration(configuration);
        }

        // In real applications, consider using a framework such as
        // Polly to make it easier to customize the retry approach.
        private T BasicRetry<T>(Func<T> func)
        {
            int reconnectRetry = 0;
            int disposedRetry = 0;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                        throw;
                    RedisConnectionManager.ForceReconnect();
                }
                catch (ObjectDisposedException)
                {
                    disposedRetry++;
                    if (disposedRetry > RetryMaxAttempts)
                        throw;
                }
            }
        }

        private IDatabase GetDatabase()
        {
            return BasicRetry(() => RedisConnectionManager.Connection.GetDatabase());
        }

        private System.Net.EndPoint[] GetEndPoints()
        {
            return BasicRetry(() => RedisConnectionManager.Connection.GetEndPoints());
        }

        private IServer GetServer(string host, int port)
        {
            return BasicRetry(() => RedisConnectionManager.Connection.GetServer(host, port));
        }

        public string GetData(string cacheKey)
        {
            RedisConnectionManager.InitializeConfiguration(_configuration);
            _logger.LogInformation("RedisCacheService.GetData Redis Configuration initialized.");
            IDatabase cache = GetDatabase();
            _logger.LogInformation("RedisCacheService.GetData Redis Database created.");
            var data= cache.StringGet(cacheKey);
            _logger.LogInformation("RedisCacheService.GetData Retrived cacheData :{data} from Redis Cache  Key:{cacheKey}", data,cacheKey);
            cache.KeyExpire(cacheKey, DateTime.Now.AddMinutes(5));
            return data;
        }
        public void SetData(string cacheKey, string data)
        {
            RedisConnectionManager.InitializeConfiguration(_configuration);
            _logger.LogInformation("RedisCacheService.SetData: Redis Configuration initialized");
            IDatabase cache = GetDatabase();
            _logger.LogInformation("RedisCacheService.GetData Redis Database created");
            cache.StringSet(cacheKey,data);
            _logger.LogInformation("RedisCacheService.SetData executed cacheData :{data} from Redis Cache  Key:{cacheKey}", data, cacheKey);
            cache.KeyExpire(cacheKey,DateTime.Now.AddMinutes(5));
        }
        public void DeleteData(string cacheKey, string data)
        {
            RedisConnectionManager.InitializeConfiguration(_configuration);
            _logger.LogInformation("RedisCacheService.DeleteData: Redis Configuration initialized");
            IDatabase cache = GetDatabase();
            _logger.LogInformation("RedisCacheService.DeleteData Redis Database created");
            cache.KeyDelete(cacheKey);
            _logger.LogInformation("RedisCacheService.DeleteData executed cacheData :{data} from Redis Cache  Key:{cacheKey}", data, cacheKey);
            cache.KeyExpire(cacheKey, DateTime.Now.AddMinutes(5));
            //RedisConnectionManager.CloseConnection();
        }

    }
}
