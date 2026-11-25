namespace Jero.Core;

/// <summary>
/// 系统租户菜单表种子数据
/// </summary>
[IgnoreUpdateSeed]
public class SysTenantMenuSeedData : ISqlSugarEntitySeedData<SysTenantMenu>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysTenantMenu> HasData()
    {
        return App.GetService<SysTenantService>().GetTenantDefaultMenuList()
            .Select(u => new SysTenantMenu
            {
                Id = CommonUtil.GetFixedHashCode("" + SqlSugarConst.DefaultTenantId + u.MenuId, 1300000000000),
                TenantId = SqlSugarConst.DefaultTenantId,
                MenuId = u.MenuId
            });
    }
}