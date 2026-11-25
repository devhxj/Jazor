namespace Jero.Core;

/// <summary>
/// 系统机构表种子数据
/// </summary>
[IgnoreUpdateSeed]
public class SysOrgSeedData : ISqlSugarEntitySeedData<SysOrg>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysOrg> HasData()
    {
        var admin = new SysUserSeedData().HasData().First(u => u.Account == "Jero");
        return new[]
        {
            new SysOrg{ Id=SqlSugarConst.DefaultTenantId, Pid=0, Name="系统默认", Code="1001", Type="101", Level=1, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="系统默认", TenantId=SqlSugarConst.DefaultTenantId },
            new SysOrg{ Id=SqlSugarConst.DefaultTenantId + 1, Pid=SqlSugarConst.DefaultTenantId, Name="市场部", Code="100101", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="市场部", CreateUserId=admin.Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysOrg{ Id=SqlSugarConst.DefaultTenantId + 2, Pid=SqlSugarConst.DefaultTenantId, Name="开发部", Code="100102", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="开发部", CreateUserId=admin.Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysOrg{ Id=SqlSugarConst.DefaultTenantId + 3, Pid=SqlSugarConst.DefaultTenantId, Name="售后部", Code="100103", Level=2, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="售后部", CreateUserId=admin.Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysOrg{ Id=SqlSugarConst.DefaultTenantId + 4, Pid=SqlSugarConst.DefaultTenantId, Name="其他", Code="10010301", Level=3, CreateTime=DateTime.Parse("2022-02-10 00:00:00"), Remark="其他", CreateUserId=admin.Id, TenantId=SqlSugarConst.DefaultTenantId },
        };
    }
}