namespace Jero.Core;

/// <summary>
/// 系统租户表种子数据
/// </summary>
[IgnoreUpdateSeed]
public class SysTenantSeedData : ISqlSugarEntitySeedData<SysTenant>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysTenant> HasData()
    {
        var defaultDbConfig = App.GetOptions<DbConnectionOptions>().ConnectionConfigs[0];
        var userList = new SysUserSeedData().HasData().ToList();
        var admin = userList.First(u => u.Account == "Jero");
        return new[]
        {
            new SysTenant{ Id=SqlSugarConst.DefaultTenantId, OrgId=SqlSugarConst.DefaultTenantId, UserId=admin.Id, Host="gitee.com", TenantType=TenantTypeEnum.Id, DbType=defaultDbConfig.DbType, Connection=defaultDbConfig.ConnectionString, ConfigId=SqlSugarConst.MainConfigId, Logo="/images/logo.png", Title="Jero", ViceTitle="Jero", ViceDesc="站在巨人肩膀上的 .NET 通用权限开发框架", Watermark="Jero", Copyright="Copyright \u00a9 2021-present Jero All rights reserved.", Icp="省ICP备12345678号", IcpUrl="https://beian.miit.gov.cn", Remark="系统默认", CreateTime=DateTime.Parse("2022-02-10 00:00:00") },
        };
    }
}
