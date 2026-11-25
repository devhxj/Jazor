namespace Jero.Core.Service;

/// <summary>
/// 系统用户角色服务
/// </summary>
public class SysUserRoleService : ITransient
{
    private readonly SysCacheService _sysCacheService;
    private readonly SqlSugarRepository<SysUserRole> _sysUserRoleRep;

    public SysUserRoleService(SysCacheService sysCacheService,
        SqlSugarRepository<SysUserRole> sysUserRoleRep)
    {
        _sysCacheService = sysCacheService;
        _sysUserRoleRep = sysUserRoleRep;
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task GrantUserRole(UserRoleInput input)
    {
        await _sysUserRoleRep.DeleteAsync(u => u.UserId == input.UserId);

        if (input.RoleIdList == null || input.RoleIdList.Count < 1) return;
        var roles = input.RoleIdList.Select(u => new SysUserRole
        {
            UserId = input.UserId,
            RoleId = u
        }).ToList();
        await _sysUserRoleRep.InsertRangeAsync(roles);
        _sysCacheService.Remove(CacheConst.KeyUserButton + input.UserId);
    }

    /// <summary>
    /// 根据角色Id删除用户角色
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task DeleteUserRoleByRoleId(long roleId)
    {
        await _sysUserRoleRep.AsQueryable()
             .Where(u => u.RoleId == roleId)
             .Select(u => u.UserId)
             .ForEachAsync(userId =>
             {
                 _sysCacheService.Remove(CacheConst.KeyUserButton + userId);
             });

        await _sysUserRoleRep.DeleteAsync(u => u.RoleId == roleId);
    }

    /// <summary>
    /// 根据用户Id删除用户角色
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task DeleteUserRoleByUserId(long userId)
    {
        await _sysUserRoleRep.DeleteAsync(u => u.UserId == userId);
        _sysCacheService.Remove(CacheConst.KeyUserButton + userId);
    }

    /// <summary>
    /// 根据用户Id获取角色集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<SysRole>> GetUserRoleList(long userId)
    {
        var sysUserRoleList = await _sysUserRoleRep.AsQueryable()
            .Includes(u => u.SysRole)
            .Where(u => u.UserId == userId).ToListAsync();
        return sysUserRoleList.Select(u => u.SysRole).OfType<SysRole>().ToList();
    }

    /// <summary>
    /// 根据用户Id获取角色Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserRoleIdList(long userId)
    {
        return await _sysUserRoleRep.AsQueryable()
            .Where(u => u.UserId == userId).Select(u => u.RoleId).ToListAsync();
    }

    /// <summary>
    /// 根据角色Id获取用户Id集合
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserIdList(long roleId)
    {
        return await _sysUserRoleRep.AsQueryable()
            .Where(u => u.RoleId == roleId).Select(u => u.UserId).ToListAsync();
    }
}
