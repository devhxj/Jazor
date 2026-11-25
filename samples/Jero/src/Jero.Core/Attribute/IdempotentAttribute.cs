


using System.Text.Json;
using System.Security.Claims;
namespace Jero.Core;

/// <summary>
/// 防止重复请求过滤器特性(此特性使用了分布式锁，需确保系统支持分布式锁)
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// 请求间隔时间/秒
    /// </summary>
    public int IntervalTime { get; set; } = 5;

    /// <summary>
    /// 错误提示内容
    /// </summary>
    public string Message { get; set; } = "你操作频率过快，请稍后重试！";

    /// <summary>
    /// 缓存前缀: Key+请求路由+用户Id+请求参数
    /// </summary>
    public string CacheKey { get; set; } = CacheConst.KeyIdempotent;

    /// <summary>
    /// 是否直接抛出异常：Ture是，False返回上次请求结果
    /// </summary>
    public bool ThrowBah { get; set; }

    /// <summary>
    /// 锁前缀
    /// </summary>
    public string LockPrefix { get; set; } = "lock_";

    public IdempotentAttribute()
    {
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var userId = httpContext.User?.FindFirstValue(ClaimConst.UserId);
        var cacheExpireTime = TimeSpan.FromSeconds(IntervalTime);

        var parameters = JsonSerializer.Serialize(context.ActionArguments);

        var cacheKey = CacheKey + MD5Encryption.Encrypt($"{path}{userId}{parameters}");
        var sysCacheService = httpContext.RequestServices.GetService<SysCacheService>()
            ?? throw Oops.Oh("未配置系统缓存服务，无法执行幂等校验");
        try
        {
            // 分布式锁
            using var distributedLock = sysCacheService.BeginCacheLock($"{LockPrefix}{cacheKey}") ?? throw Oops.Oh(Message);

            var cacheValue = sysCacheService.Get<ResponseData>(cacheKey);
            if (cacheValue != null)
            {
                if (ThrowBah) throw Oops.Oh(Message);
                context.Result = new ObjectResult(cacheValue.Value);
                return;
            }
            else
            {
                var resultContext = await next();
                // 缓存请求结果,null值不缓存
                if (resultContext.Result is ObjectResult { Value: { } } objectResult)
                {
                    var typeName = objectResult.Value.GetType().Name;
                    var responseData = new ResponseData
                    {
                        Type = typeName,
                        Value = objectResult.Value
                    };
                    sysCacheService.Set(cacheKey, responseData, cacheExpireTime);
                }
            }
        }
        catch (Exception ex)
        {
            throw Oops.Oh($"{Message}-{ex}");
        }
    }

    /// <summary>
    /// 请求结果数据
    /// </summary>
    private class ResponseData
    {
        /// <summary>
        /// 结果类型
        /// </summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// 请求结果
        /// </summary>
        public object? Value { get; set; }
    }
}
