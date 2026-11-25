namespace Jero.Core.Service;

/// <summary>
/// 微信账号服务 🧩
/// </summary>
[ApiDescriptionSettings(Order = 220)]
public class SysWechatUserService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SysWechatUser> _sysWechatUserRep;

    public SysWechatUserService(SqlSugarRepository<SysWechatUser> sysWechatUserRep)
    {
        _sysWechatUserRep = sysWechatUserRep;
    }

    /// <summary>
    /// 获取微信用户列表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取微信用户列表")]
    public async Task<SqlSugarPagedList<SysWechatUser>> Page(WechatUserInput input)
    {
        var nickName = input.NickName?.Trim() ?? string.Empty;
        var mobile = input.Mobile?.Trim() ?? string.Empty;

        return await _sysWechatUserRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(nickName), u => !string.IsNullOrWhiteSpace(u.NickName) && u.NickName.Contains(nickName))
            .WhereIF(!string.IsNullOrWhiteSpace(mobile), u => !string.IsNullOrWhiteSpace(u.Mobile) && u.Mobile.Contains(mobile))
            .OrderBy(u => u.Id, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加微信用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    [DisplayName("增加微信用户")]
    public async Task AddWechatUser(SysWechatUser input)
    {
        await _sysWechatUserRep.InsertAsync(input.Adapt<SysWechatUser>());
    }

    /// <summary>
    /// 更新微信用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    [DisplayName("更新微信用户")]
    public async Task UpdateWechatUser(SysWechatUser input)
    {
        var weChatUser = input.Adapt<SysWechatUser>();
        await _sysWechatUserRep.AsUpdateable(weChatUser).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除微信用户 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    [DisplayName("删除微信用户")]
    public async Task DeleteWechatUser(DeleteWechatUserInput input)
    {
        await _sysWechatUserRep.DeleteAsync(u => u.Id == input.Id);
    }
}
