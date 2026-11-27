using Furion;
using Furion.Authorization;
using Furion.DataEncryption;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Jero.Core;
using Jero.Core.Service;
namespace Jero.Web.Handlers;

public class JwtHandler : AppAuthorizeHandler
{
    private readonly SysCacheService _sysCacheService = Furion.App.GetRequiredService<SysCacheService>();
    private readonly SysConfigService _sysConfigService = Furion.App.GetRequiredService<SysConfigService>();
    private static readonly SysMenuService _sysMenuService = Furion.App.GetRequiredService<SysMenuService>();

    /// <summary>
    /// 先检查用户和令牌是否已被拉黑，再按系统配置刷新令牌并进入权限管道。
    /// </summary>
    public override async Task HandleAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        var userId = context.User.FindFirst(ClaimConst.UserId)?.Value;
        var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        // 🛡️ 黑名单校验（包括用户和token）
        if (_sysCacheService.ExistKey($"{CacheConst.KeyBlacklist}{userId}") ||
            _sysCacheService.ExistKey($"blacklist:token:{token}"))
        {
            context.Fail();
            context.GetCurrentHttpContext().SignoutToSwagger();
            return;
        }

        var tokenExpire = await _sysConfigService.GetTokenExpire();
        var refreshTokenExpire = await _sysConfigService.GetRefreshTokenExpire();
        if (JWTEncryption.AutoRefreshToken(context, context.GetCurrentHttpContext(), tokenExpire, refreshTokenExpire))
        {
            await AuthorizeHandleAsync(context);
        }
        else
        {
            context.Fail(); // 授权失败
            var currentHttpContext = context.GetCurrentHttpContext();
            if (currentHttpContext == null) return;

            // 跳过由于 SignatureAuthentication 引发的失败
            if (currentHttpContext.Items.ContainsKey(SignatureAuthenticationDefaults.AuthenticateFailMsgKey)) return;
            currentHttpContext.SignoutToSwagger();
        }
    }

    public override async Task<bool> PipelineAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext)
    {
        // 已自动验证 Jwt Token 有效性
        return await CheckAuthorizeAsync(httpContext);
    }

    /// <summary>
    /// 权限校验核心逻辑
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    private static async Task<bool> CheckAuthorizeAsync(DefaultHttpContext httpContext)
    {
        // 登录模式判断PC、APP
        if (Furion.App.User.FindFirst(ClaimConst.LoginMode)?.Value == ((int)LoginModeEnum.APP).ToString())
            return true;

        // 排除超管
        if (Furion.App.User.FindFirst(ClaimConst.AccountType)?.Value == ((int)AccountTypeEnum.SuperAdmin).ToString())
            return true;

        // 路由名称
        var routeName = httpContext.Request.Path.StartsWithSegments("/api")
            ? httpContext.Request.Path.Value![5..].Replace("/", ":")
            : httpContext.Request.Path.Value![1..].Replace("/", ":");

        // 获取用户拥有按钮权限集合
        var ownBtnPermList = await _sysMenuService.GetOwnBtnPermList();
        if (ownBtnPermList.Exists(u => routeName.Equals(u, StringComparison.CurrentCultureIgnoreCase)))
            return true;

        // 只有已登记为按钮权限的路由才要求用户持有对应权限。
        var allBtnPermList = await _sysMenuService.GetAllBtnPermList();
        return allBtnPermList.TrueForAll(u => !routeName.Equals(u, StringComparison.CurrentCultureIgnoreCase));
    }
}