


using Jero.Core;
namespace Jero.Plugin.ReZero;

/// <summary>
/// 超级API菜单表种子数据
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
            new SysMenu{ Id=1310500010101, Pid=1300500000101, Title="超级API", Path="/develop/reZero", Name="sysReZero", Component="Layout", Icon="ele-MagicStick", Type=MenuTypeEnum.Dir, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=140 },
            new SysMenu{ Id=1310500020101, Pid=1310500010101, Title="动态接口", Path="/develop/reZero/dynamicApi", Name="sysReZeroDynamicApi", Component="layout/routerView/iframe", Icon="ele-Menu", Type=MenuTypeEnum.Menu, IsIframe=true, OutLink="http://localhost:5005/rezero/dynamic_interface.html?model=small", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=100 },
            new SysMenu{ Id=1310500030101, Pid=1310500010101, Title="数据库管理", Path="/develop/reZero/database", Name="sysReZeroDatabase", Component="layout/routerView/iframe", Icon="ele-Menu", Type=MenuTypeEnum.Menu, IsIframe=true, OutLink="http://localhost:5005/rezero/database_manager.html?model=small", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=110 },
            new SysMenu{ Id=1310500040101, Pid=1310500010101, Title="实体表管理", Path="/develop/reZero/entity", Name="sysReZeroEntity", Component="layout/routerView/iframe", Icon="ele-Menu", Type=MenuTypeEnum.Menu, IsIframe=true, OutLink="http://localhost:5005/rezero/entity_manager.html?model=small", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=120 },
            new SysMenu{ Id=1310500050101, Pid=1310500010101, Title="接口分类", Path="/develop/reZero/apiCategory", Name="sysReZeroApiCategory", Component="layout/routerView/iframe", Icon="ele-Menu", Type=MenuTypeEnum.Menu, IsIframe=true, OutLink="http://localhost:5005/rezero/interface_categroy.html?model=small", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=130 },
            new SysMenu{ Id=1310500060101, Pid=1310500010101, Title="接口定义", Path="/develop/reZero/apiDefine", Name="sysReZeroApiDefine", Component="layout/routerView/iframe", Icon="ele-Menu", Type=MenuTypeEnum.Menu, IsIframe=true, OutLink="http://localhost:5005/rezero/interface_manager.html?model=small", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrderNo=140 },
        };
    }
}