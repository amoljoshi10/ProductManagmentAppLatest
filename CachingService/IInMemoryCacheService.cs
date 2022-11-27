
namespace CachingService
{
    public interface IInMemoryCacheService
    {
        string GetData(string cacheKey);
        void SetData(string cacheKey, string data);
    }
}
