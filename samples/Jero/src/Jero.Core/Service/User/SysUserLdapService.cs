namespace Jero.Core.Service;

/// <summary>
/// 用户域账号服务
/// </summary>
public class SysUserLdapService : ITransient
{
    private readonly SqlSugarRepository<SysUserLdap> _sysUserLdapRep;

    public SysUserLdapService(SqlSugarRepository<SysUserLdap> sysUserLdapRep)
    {
        _sysUserLdapRep = sysUserLdapRep;
    }

    /// <summary>
    /// 批量插入域账号
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="sysUserLdapList"></param>
    /// <returns></returns>
    public async Task InsertUserLdapList(long tenantId, List<SysUserLdap> sysUserLdapList)
    {
        await _sysUserLdapRep.DeleteAsync(u => u.TenantId == tenantId);

        await _sysUserLdapRep.InsertRangeAsync(sysUserLdapList);

        await _sysUserLdapRep.AsUpdateable()
            .InnerJoin<SysUser>((l, u) => l.EmployeeId == u.Account)
            .SetColumns((l, u) => new SysUserLdap { UserId = u.Id })
            .Where((l, u) => l.TenantId == tenantId && u.Status == StatusEnum.Enable)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 增加域账号
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="userId"></param>
    /// <param name="account"></param>
    /// <param name="domainAccount"></param>
    /// <returns></returns>
    public async Task AddUserLdap(long tenantId, long userId, string account, string domainAccount)
    {
        var userLdap = await _sysUserLdapRep.GetFirstAsync(u => u.TenantId == tenantId && (u.Account == account || u.UserId == userId || u.EmployeeId == domainAccount));
        if (userLdap != null) await _sysUserLdapRep.DeleteByIdAsync(userLdap.Id);

        if (!string.IsNullOrWhiteSpace(domainAccount))
            await _sysUserLdapRep.InsertAsync(new SysUserLdap { EmployeeId = account, TenantId = tenantId, UserId = userId, Account = domainAccount });
    }

    /// <summary>
    /// 删除域账号
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task DeleteUserLdapByUserId(long userId)
    {
        await _sysUserLdapRep.DeleteAsync(u => u.UserId == userId);
    }
}