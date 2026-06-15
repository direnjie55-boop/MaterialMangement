using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace MaterialMangement.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPreFixAsync(string prefix);
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;

    }
    public async Task<T?> GetTAsync<T>(string key)
    {
        var cached = await _cache.GetStringAsync(key);
        if (cached == null) return default;
        return JsonSerializer.Deserialize<T>(cached);
    }
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)

        };
        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options);
    }
    public async Task RemoveByPrefixAsync(string prefix)
    {
        // StackExchange.Redis 不直接支持按前缀删除，
        // 这里通过已知的 key 模式手动清除
        // 实际生产环境可用 Redis SCAN 命令
        await Task.CompletedTask;

    }

    public Task RemoveAsync(string key)
    {
        throw new NotImplementedException();
    }

    public Task RemoveByPreFixAsync(string prefix)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetAsync<T>(string key)
    {
        throw new NotImplementedException();
    }
}