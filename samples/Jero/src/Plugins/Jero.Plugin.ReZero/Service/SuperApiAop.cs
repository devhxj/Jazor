


using Jero.Core;
using Furion.DataEncryption;
using Furion.FriendlyException;
using Furion.JsonSerialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReZero.SuperAPI;
namespace Jero.Plugin.ReZero.Service;

/// <summary>
/// 超级API接口拦截器
/// </summary>
public class SuperApiAop : DefaultSuperApiAop
{
    public override async Task OnExecutingAsync(InterfaceContext aopContext)
    {
        //if (aopContext.InterfaceType == InterfaceType.DynamicApi)
        //{
        var httpContext = aopContext.HttpContext ?? throw Oops.Oh("当前请求缺少 HTTP 上下文");
        var authenticateResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            throw Oops.Oh("没权限 Unauthorized");
        //}

        var accessToken = GetBearerToken(httpContext);
        if (string.IsNullOrWhiteSpace(accessToken))
            throw Oops.Oh("Token 缺失");

        var (isValid, _, _) = JWTEncryption.Validate(accessToken);
        if (!isValid)
            throw Oops.Oh("Token 无效");

        await base.OnExecutingAsync(aopContext);
    }

    public override async Task OnExecutedAsync(InterfaceContext aopContext)
    {
        InitLogContext(aopContext, LogLevel.Information);

        await base.OnExecutedAsync(aopContext);
    }

    public override async Task OnErrorAsync(InterfaceContext aopContext)
    {
        InitLogContext(aopContext, LogLevel.Error);

        await base.OnErrorAsync(aopContext);
    }

    /// <summary>
    /// 保存超级API接口日志
    /// </summary>
    /// <param name="aopContext"></param>
    /// <param name="logLevel"></param>
    private void InitLogContext(InterfaceContext aopContext, LogLevel logLevel)
    {
        var api = aopContext.InterfaceInfo;
        var context = aopContext.HttpContext;

        var accessToken = context == null ? null : GetBearerToken(context);
        var claims = ReadClaimsSafely(accessToken);
        var userName = claims?.FirstOrDefault(u => u.Type == ClaimConst.Account)?.Value;
        var realName = claims?.FirstOrDefault(u => u.Type == ClaimConst.RealName)?.Value;

        var requestUrl = api?.Url ?? context?.Request.Path.Value ?? string.Empty;
        var paths = requestUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var actionName = paths.Length > 0 ? paths[^1] : string.Empty;
        var groupName = api?.GroupName ?? string.Empty;

        var apiInfo = new
        {
            requestUrl,
            httpMethod = api?.HttpMethod ?? context?.Request.Method ?? string.Empty,
            displayTitle = api?.Name ?? string.Empty,
            actionTypeName = actionName,
            controllerName = aopContext.InterfaceType == InterfaceType.DynamicApi ? $"ReZero动态-{groupName}" : $"ReZero系统-{groupName}",
            remoteIPv4 = context?.GetRemoteIpAddressToIPv4(),
            userAgent = context?.Request.Headers["User-Agent"].ToString() ?? string.Empty,
            returnInformation = new
            {
                httpStatusCode = context?.Response.StatusCode,
            },
            authorizationClaims = new[]
            {
                new
                {
                    type = ClaimConst.Account,
                    value = userName
                },
                new
                {
                    type = ClaimConst.RealName,
                    value = realName
                },
            },
            exception = aopContext.Exception == null ? null : JSON.Serialize(aopContext.Exception)
        };

        var logger = App.GetRequiredService<ILoggerFactory>().CreateLogger(CommonConst.SysLogCategoryName);
        using var scope = logger.ScopeContext(new Dictionary<object, object> {
            { "loggingMonitor", apiInfo.ToJson() }
        });
        logger.Log(logLevel, "ReZero超级API接口日志");
    }

    private static string? GetBearerToken(HttpContext context)
    {
        var authorization = context.Request.Headers["Authorization"].ToString();
        const string bearerPrefix = "Bearer ";

        return !string.IsNullOrWhiteSpace(authorization) &&
               authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorization[bearerPrefix.Length..].Trim()
            : null;
    }

    private static IEnumerable<System.Security.Claims.Claim>? ReadClaimsSafely(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return null;

        try
        {
            return JWTEncryption.ReadJwtToken(accessToken)?.Claims;
        }
        catch
        {
            return null;
        }
    }
}
