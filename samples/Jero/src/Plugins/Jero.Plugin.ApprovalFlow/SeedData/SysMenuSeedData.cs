namespace Jero.Plugin.ApprovalFlow;

/// <summary>
/// 审批流程菜单表种子数据
/// </summary>
public class SysMenuSeedData : ISqlSugarEntitySeedData<SysMenu>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysMenu> HasData()
    {
        return new[]
        {
            new SysMenu{ Id=1310300010101, Pid=1300300000101, Title="审批流程", Path="/platform/approvalFlow", Name="approvalFlow", Component="/approvalFlow/index", Icon="ele-Help", Type=MenuTypeEnum.Menu, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=2000 },
        };
    }
}