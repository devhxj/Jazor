


using Microsoft.AspNetCore.SignalR;
namespace Jero.Core.Service;

/// <summary>
/// 系统在线用户服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 300)]
public class SysOnlineUserService : IDynamicApiController, ITransient
{
    private readonly UserManager _userManager;
    private readonly SysConfigService _sysConfigService;
    private readonly IHubContext<OnlineUserHub, IOnlineUserHub> _onlineUserHubContext;
    private readonly SqlSugarRepository<SysOnlineUser> _sysOnlineUerRep;

    public SysOnlineUserService(SysConfigService sysConfigService,
        IHubContext<OnlineUserHub, IOnlineUserHub> onlineUserHubContext,
        SqlSugarRepository<SysOnlineUser> sysOnlineUerRep,
        UserManager userManager)
    {
        _userManager = userManager;
        _sysConfigService = sysConfigService;
        _onlineUserHubContext = onlineUserHubContext;
        _sysOnlineUerRep = sysOnlineUerRep;
    }

    /// <summary>
    /// 获取在线用户分页列表 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取在线用户分页列表")]
    public async Task<SqlSugarPagedList<SysOnlineUser>> Page(PageOnlineUserInput input)
    {
        var userName = input.UserName?.Trim() ?? string.Empty;
        var realName = input.RealName?.Trim() ?? string.Empty;

        return await _sysOnlineUerRep.AsQueryable()
            .WhereIF(_userManager.SuperAdmin && input.TenantId > 0, u => u.TenantId == input.TenantId)
            .WhereIF(!string.IsNullOrWhiteSpace(userName), u => u.UserName.Contains(userName))
            .WhereIF(!string.IsNullOrWhiteSpace(realName), u => u.RealName != null && u.RealName.Contains(realName))
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 强制下线 🔖
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [NonValidation]
    [DisplayName("强制下线")]
    public async Task ForceOffline(SysOnlineUser user)
    {
        await _onlineUserHubContext.Clients.Client(user.ConnectionId ?? "").ForceOffline("强制下线");
        await _sysOnlineUerRep.DeleteAsync(user);
    }

    /// <summary>
    /// 发布站内消息
    /// </summary>
    /// <param name="notice"></param>
    /// <param name="userIds"></param>
    /// <returns></returns>
    [NonAction]
    public async Task PublicNotice(SysNotice notice, List<long> userIds)
    {
        var userList = await _sysOnlineUerRep.GetListAsync(u => userIds.Contains(u.UserId));
        if (userList.Count == 0) return;

        foreach (var item in userList)
        {
            await _onlineUserHubContext.Clients.Client(item.ConnectionId ?? "").PublicNotice(notice);
        }
    }

    /// <summary>
    /// 单用户登录
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task SingleLogin(long userId)
    {
        if (await _sysConfigService.GetConfigValue<bool>(ConfigConst.SysSingleLogin))
        {
            var users = await _sysOnlineUerRep.GetListAsync(u => u.UserId == userId);
            foreach (var user in users)
            {
                await ForceOffline(user);
            }
        }
    }

    /// <summary>
    /// 通过用户ID踢掉在线用户
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [NonAction]
    public async Task ForceOffline(long userId)
    {
        var users = await _sysOnlineUerRep.GetListAsync(u => u.UserId == userId);
        foreach (var user in users)
        {
            await ForceOffline(user);
        }
    }
}
