using Jero.Core.Service;
using Furion.FriendlyException;
using System.ComponentModel;

namespace Jero.Application.Modules.GoView.Service;

/// <summary>
/// 系统登录服务 🧩
/// </summary>
[UnifyProvider("GoView")]
[ApiDescriptionSettings(GoViewConst.GroupName, Module = "goview", Name = "sys", Order = 100, Description = "系统登录")]
public class GoViewSysService : IDynamicApiController
{
    private readonly SysAuthService _sysAuthService;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly SysCacheService _sysCacheService;

    public GoViewSysService(SysAuthService sysAuthService,
        SqlSugarRepository<SysUser> sysUserRep,
        SysCacheService sysCacheService)
    {
        _sysAuthService = sysAuthService;
        _sysUserRep = sysUserRep;
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// GoView 登录 🔖
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("GoView 登录")]
    public async Task<GoViewLoginOutput> Login(GoViewLoginInput input)
    {
        // 设置默认租户
        input.TenantId ??= SqlSugarConst.DefaultTenantId;

        _sysCacheService.Set($"{CacheConst.KeyConfig}{ConfigConst.SysCaptcha}", false);

        input.Password = CryptogramUtil.SM2Encrypt(input.Password);
        var loginResult = await _sysAuthService.Login(new LoginInput()
        {
            Account = input.Username,
            Password = input.Password,
        });

        _sysCacheService.Remove($"{CacheConst.KeyConfig}{ConfigConst.SysCaptcha}");

        var sysUser = await _sysUserRep.AsQueryable().ClearFilter().FirstAsync(u => u.Account == input.Username)
            ?? throw Oops.Oh("GoView 登录用户不存在");
        return new GoViewLoginOutput()
        {
            Userinfo = new GoViewLoginUserInfo
            {
                Id = sysUser.Id.ToString(),
                Username = sysUser.Account,
                Nickname = sysUser.NickName ?? sysUser.Account,
            },
            Token = new GoViewLoginToken
            {
                TokenValue = $"Bearer {loginResult.AccessToken}"
            }
        };
    }

    /// <summary>
    /// GoView 退出 🔖
    /// </summary>
    [DisplayName("GoView 退出")]
    public void GetLogout()
    {
        _sysAuthService.Logout();
    }

    /// <summary>
    /// 获取 OSS 上传接口 🔖
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [ApiDescriptionSettings(Name = "GetOssInfo")]
    [DisplayName("获取 OSS 上传接口")]
    public Task<GoViewOssUrlOutput> GetOssInfo()
    {
        return Task.FromResult(new GoViewOssUrlOutput { BucketURL = "" });
    }
}
