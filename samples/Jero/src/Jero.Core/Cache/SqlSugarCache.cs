namespace Jero.Core;

/// <summary>
/// SqlSugar二级缓存
/// </summary>
public class SqlSugarCache : ICacheService
{
    /// <summary>
    /// 系统缓存服务
    /// </summary>
    private static readonly SysCacheService _cache = App.GetRequiredService<SysCacheService>();

    public void Add<V>(string key, V value)
    {
        _cache.Set($"{CacheConst.SqlSugar}{key}", value);
    }

    public void Add<V>(string key, V value, int cacheDurationInSeconds)
    {
        _cache.Set($"{CacheConst.SqlSugar}{key}", value, TimeSpan.FromSeconds(cacheDurationInSeconds));
    }

    public bool ContainsKey<V>(string key)
    {
        return _cache.ExistKey($"{CacheConst.SqlSugar}{key}");
    }

    public V Get<V>(string key)
    {
        var value = _cache.Get<V>($"{CacheConst.SqlSugar}{key}");
        return value ?? throw new InvalidOperationException($"SqlSugar 缓存 {key} 不存在或值为空");
    }

    public IEnumerable<string> GetAllKey<V>()
    {
        return _cache.GetKeysByPrefixKey(CacheConst.SqlSugar);
    }

    public V GetOrCreate<V>(string key, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
    {
        var value = _cache.GetOrAdd<V>($"{CacheConst.SqlSugar}{key}", (cacheKey) =>
        {
            var newValue = create();
            return newValue ?? throw new InvalidOperationException($"SqlSugar 缓存 {key} 创建函数返回空值");
        }, cacheDurationInSeconds);
        return value ?? throw new InvalidOperationException($"SqlSugar 缓存 {key} 不存在或值为空");
    }

    public void Remove<V>(string key)
    {
        _cache.Remove(key); // SqlSugar调用Remove方法时，key中已包含了CacheConst.SqlSugar前缀
    }
}
