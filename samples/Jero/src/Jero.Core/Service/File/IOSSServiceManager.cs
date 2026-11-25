using OnceMi.AspNetCore.OSS;
namespace Jero.Core.Service;

/// <summary>
/// OSS服务管理器接口
/// </summary>
public interface IOSSServiceManager : IDisposable
{
    /// <summary>
    /// 获取OSS服务实例
    /// </summary>
    /// <param name="provider">存储提供者配置</param>
    /// <returns></returns>
    Task<IOSSService> GetOSSServiceAsync(SysFileProvider provider);

    /// <summary>
    /// 清除缓存
    /// </summary>
    void ClearCache();
}

/// <summary>
/// OSS服务管理器实现
/// </summary>
public class OSSServiceManager : IOSSServiceManager, ITransient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, IOSSService> _ossServiceCache;
    private readonly object _lockObject = new object();
    private bool _disposed = false;

    public OSSServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _ossServiceCache = new ConcurrentDictionary<string, IOSSService>();
    }

    /// <summary>
    /// 获取OSS服务实例（带缓存）
    /// </summary>
    /// <param name="provider">存储提供者配置</param>
    /// <returns></returns>
    public async Task<IOSSService> GetOSSServiceAsync(SysFileProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        var cacheKey = provider.ConfigKey;

        // 尝试从缓存获取
        if (_ossServiceCache.TryGetValue(cacheKey, out var cachedService))
        {
            return cachedService;
        }

        // 验证配置
        if (!await ValidateConfigurationAsync(provider))
        {
            throw new InvalidOperationException($"OSS提供者配置无效: {provider.DisplayName}");
        }

        // 线程安全地创建新服务
        lock (_lockObject)
        {
            // 双重检查锁定模式
            if (_ossServiceCache.TryGetValue(cacheKey, out cachedService))
            {
                return cachedService;
            }

            // 转换配置并创建服务
            var ossOptions = ConvertToOSSOptions(provider);
            var ossService = CreateOSSService(ossOptions);

            // 添加到缓存
            _ossServiceCache.TryAdd(cacheKey, ossService);

            return ossService;
        }
    }

    /// <summary>
    /// 创建OSS服务实例
    /// </summary>
    /// <param name="options">OSS配置选项</param>
    /// <returns></returns>
    private IOSSService CreateOSSService(OSSOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            // 使用现有的IOSSServiceFactory，但需要先注册配置
            var providerName = Enum.GetName(options.Provider) ?? throw Oops.Oh("OSS提供者配置无效");
            var configSectionName = $"TempOSS_{Guid.NewGuid():N}";

            // 创建临时配置
            var configData = new Dictionary<string, string?>
            {
                [$"{configSectionName}:Provider"] = providerName,
                [$"{configSectionName}:Endpoint"] = options.Endpoint ?? "",
                [$"{configSectionName}:AccessKey"] = options.AccessKey ?? "",
                [$"{configSectionName}:SecretKey"] = options.SecretKey ?? "",
                [$"{configSectionName}:Region"] = options.Region ?? "",
                [$"{configSectionName}:IsEnableHttps"] = options.IsEnableHttps.ToString(),
                [$"{configSectionName}:IsEnableCache"] = options.IsEnableCache.ToString()
            };

            var tempConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // 创建临时服务集合，但不立即释放
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(tempConfig);
            services.AddLogging();
            services.AddOSSService(providerName, configSectionName);

            // 构建服务提供者并创建OSS服务
            var tempServiceProvider = services.BuildServiceProvider();
            var ossServiceFactory = tempServiceProvider.GetRequiredService<IOSSServiceFactory>();
            var ossService = ossServiceFactory.Create(providerName);

            // 注意：不要释放tempServiceProvider，因为ossService可能依赖它
            // 这里我们接受这个内存开销，因为缓存会减少创建频率

            return ossService;
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"创建OSS服务失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <param name="provider">存储提供者配置</param>
    /// <returns></returns>
    private Task<bool> ValidateConfigurationAsync(SysFileProvider provider)
    {
        if (provider == null) return Task.FromResult(false);

        // 基本字段验证
        var isValid = !string.IsNullOrWhiteSpace(provider.Provider) &&
                     !string.IsNullOrWhiteSpace(provider.BucketName) &&
                     !string.IsNullOrWhiteSpace(provider.AccessKey) &&
                     !string.IsNullOrWhiteSpace(provider.SecretKey);

        // Minio额外需要Endpoint
        if (provider.Provider.ToUpper() == "MINIO")
        {
            isValid = isValid && !string.IsNullOrWhiteSpace(provider.Endpoint);
        }

        return Task.FromResult(isValid);
    }

    /// <summary>
    /// 将SysFileProvider转换为OSSOptions
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    private OSSOptions ConvertToOSSOptions(SysFileProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        var ossOptions = new OSSOptions
        {
            Provider = Enum.Parse<OSSProvider>(provider.Provider),
            Endpoint = provider.Endpoint,
            Region = provider.Region,
            IsEnableHttps = provider.IsEnableHttps ?? true,
            IsEnableCache = provider.IsEnableCache ?? true
        };

        // 设置认证信息（所有提供者现在都使用统一的字段）
        ossOptions.AccessKey = provider.AccessKey;
        ossOptions.SecretKey = provider.SecretKey;

        return ossOptions;
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        lock (_lockObject)
        {
            _ossServiceCache.Clear();
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            lock (_lockObject)
            {
                _ossServiceCache.Clear();
            }
            _disposed = true;
        }
    }
}
