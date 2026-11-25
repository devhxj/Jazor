


using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
namespace Jero.Core.Service;

/// <summary>
/// 系统OAuth服务 🧩
/// </summary>
[AllowAnonymous]
[ApiDescriptionSettings(Order = 498)]
public class SysOAuthService : IDynamicApiController, ITransient
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SqlSugarRepository<SysWechatUser> _sysWechatUserRep;

    public SysOAuthService(IHttpContextAccessor httpContextAccessor,
        SqlSugarRepository<SysWechatUser> sysWechatUserRep)
    {
        _httpContextAccessor = httpContextAccessor;
        _sysWechatUserRep = sysWechatUserRep;
    }

    /// <summary>
    /// 第三方登录 🔖
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="redirectUrl"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "SignIn"), HttpGet]
    [DisplayName("第三方登录")]
    public virtual async Task<IActionResult> SignIn([FromQuery] string provider, [FromQuery] string redirectUrl)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw Oops.Oh("OAuth上下文不存在");
        if (string.IsNullOrWhiteSpace(provider) || !await httpContext.IsProviderSupportedAsync(provider))
            throw Oops.Oh("不支持的OAuth类型");

        var request = httpContext.Request;
        var url = $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}Callback?provider={provider}&redirectUrl={redirectUrl}";
        var properties = new AuthenticationProperties
        {
            RedirectUri = url,
            Items = { ["LoginProvider"] = provider }
        };
        return await Task.FromResult(new ChallengeResult(provider, properties));
    }

    /// <summary>
    /// 授权回调 🔖
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="redirectUrl"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "SignInCallback"), HttpGet]
    [DisplayName("授权回调")]
    public virtual async Task<IActionResult> SignInCallback([FromQuery] string? provider = null, [FromQuery] string redirectUrl = "")
    {
        var httpContext = _httpContextAccessor.HttpContext ?? throw Oops.Oh("OAuth回调上下文不存在");
        if (string.IsNullOrWhiteSpace(provider) || !await httpContext.IsProviderSupportedAsync(provider))
            throw Oops.Oh("不支持的OAuth类型");

        var authenticateResult = await httpContext.AuthenticateAsync(provider);
        if (!authenticateResult.Succeeded)
            throw Oops.Oh("授权失败");

        var openIdClaim = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier);
        if (openIdClaim == null || string.IsNullOrWhiteSpace(openIdClaim.Value))
            throw Oops.Oh("授权失败");

        var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;
        var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var mobilePhone = authenticateResult.Principal.FindFirst(ClaimTypes.MobilePhone)?.Value;
        var dateOfBirth = authenticateResult.Principal.FindFirst(ClaimTypes.DateOfBirth)?.Value;
        var gender = authenticateResult.Principal.FindFirst(ClaimTypes.Gender)?.Value;
        var avatarUrl = string.Empty;

        var platformType = PlatformTypeEnum.微信公众号;
        if (provider == "Gitee")
        {
            platformType = PlatformTypeEnum.Gitee;
            avatarUrl = authenticateResult.Principal.FindFirst(OAuthClaim.GiteeAvatarUrl)?.Value ?? string.Empty;
        }

        // 若账号不存在则新建
        var wechatUser = await _sysWechatUserRep.AsQueryable().Includes(u => u.SysUser).ClearFilter().FirstAsync(u => u.OpenId == openIdClaim.Value);
        if (wechatUser == null)
        {
            var account = BuildOAuthAccount(provider, openIdClaim.Value, name);
            var realName = string.IsNullOrWhiteSpace(name) ? account : TrimToMaxLength(name, 32);
            var userId = await App.GetRequiredService<SysUserService>().AddUser(new AddUserInput()
            {
                Account = account,
                RealName = realName,
                NickName = realName,
                Email = email,
                Avatar = avatarUrl,
                Phone = mobilePhone,
                OrgId = 1300000000101, // 根组织架构
                RoleIdList = new List<long> { 1300000000104 } // 仅本人数据角色
            });

            await _sysWechatUserRep.InsertAsync(new SysWechatUser()
            {
                UserId = userId,
                OpenId = openIdClaim.Value,
                Avatar = avatarUrl,
                NickName = realName,
                PlatformType = platformType
            });

            wechatUser = await _sysWechatUserRep.AsQueryable().Includes(u => u.SysUser).ClearFilter().FirstAsync(u => u.OpenId == openIdClaim.Value);
        }
        if (wechatUser?.SysUser == null) throw Oops.Oh("OAuth用户绑定不存在");

        // 构建Token令牌
        var token = await App.GetRequiredService<SysAuthService>().CreateToken(wechatUser.SysUser);

        return new RedirectResult($"{redirectUrl}/#/login?token={token.AccessToken}");
    }

    private static string BuildOAuthAccount(string provider, string openId, string? name)
    {
        var account = name?.Trim();
        if (!string.IsNullOrWhiteSpace(account) && account.Length <= 32)
            return account;

        var providerPrefix = new string((provider ?? "oauth").Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(providerPrefix)) providerPrefix = "oauth";

        var suffix = Math.Abs(CommonUtil.GetFixedHashCode($"{provider}:{openId}")).ToString();
        var prefixLength = Math.Max(0, 32 - suffix.Length - 1);
        var prefix = providerPrefix.Length > prefixLength ? providerPrefix[..prefixLength] : providerPrefix;
        return string.IsNullOrWhiteSpace(prefix) ? suffix[..Math.Min(32, suffix.Length)] : $"{prefix}_{suffix}";
    }

    private static string TrimToMaxLength(string value, int maxLength)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
