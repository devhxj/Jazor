namespace Jero.Core;

/// <summary>
/// 系统角色菜单表种子数据
/// </summary>
public class SysRoleMenuSeedData : ISqlSugarEntitySeedData<SysRoleMenu>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysRoleMenu> HasData()
    {
        var roleMenuList = new List<SysRoleMenu>();

        var roleList = new SysRoleSeedData().HasData().ToList();
        var menuList = new SysMenuSeedData().HasData().ToList();
        var defaultMenuList = new SysTenantMenuSeedData().HasData().ToList();

        // 第一个角色拥有全部默认租户菜单
        //roleMenuList.AddRange(defaultMenuList.Select(u => new SysRoleMenu { Id = u.MenuId + (roleList[0].Id % 1300000000000), RoleId = roleList[0].Id, MenuId = u.MenuId }));
        roleMenuList.AddRange(defaultMenuList.Select(u => new SysRoleMenu { Id = CommonUtil.GetFixedHashCode($"{roleList[0].Id}{u.MenuId}", 1300000000000), RoleId = roleList[0].Id, MenuId = u.MenuId }));

        // 其他角色权限：默认首页、系统管理、个人中心、帮助文档、关于项目
        var otherRoleMenuList = DefaultHomeMenuUtil.GetDefaultHomeMenuTree(menuList, null).ToList();
        otherRoleMenuList.AddRange(menuList.ToChildList(u => u.Id, u => u.Pid, u => new[] { "帮助文档", "关于项目", "个人中心" }.Contains(u.Title)));
        otherRoleMenuList.Add(menuList.First(u => u.Type == MenuTypeEnum.Dir && u.Title == "系统管理"));
        //foreach (var role in roleList.Skip(1)) roleMenuList.AddRange(otherRoleMenuList.Select(u => new SysRoleMenu { Id = u.Id + (role.Id % 1300000000000), RoleId = role.Id, MenuId = u.Id }));
        foreach (var role in roleList.Skip(1)) roleMenuList.AddRange(otherRoleMenuList.DistinctBy(u => u.Id).Select(u => new SysRoleMenu { Id = CommonUtil.GetFixedHashCode($"{role.Id}{u.Id}", 1300000000000), RoleId = role.Id, MenuId = u.Id }));

        return roleMenuList;
    }
}
