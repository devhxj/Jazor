namespace Jero.Core;

/// <summary>
/// 系统用户表种子数据
/// </summary>
[IgnoreUpdateSeed]
public class SysUserSeedData : ISqlSugarEntitySeedData<SysUser>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysUser> HasData()
    {
        var defaultPassword = new SysConfigSeedData().HasData().First(u => u.Code == ConfigConst.SysPassword).Value
            ?? throw Oops.Oh("系统默认密码种子配置不能为空");
        var encryptPassword = CryptogramUtil.Encrypt(defaultPassword);
        var posList = new SysPosSeedData().HasData().ToList();
        return new[]
        {
            new SysUser{ Id=1300000000101, Account="superJero", Password=encryptPassword, NickName="超级管理员", RealName="超级管理员", Phone="18012345678", Birthday=DateTime.Parse("2000-01-01"), Sex=GenderEnum.Male, AccountType=AccountTypeEnum.SuperAdmin, Remark="超级管理员", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), TenantId=SqlSugarConst.DefaultTenantId },
            new SysUser{ Id=1300000000111, Account="Jero", Password=encryptPassword, NickName="系统管理员", RealName="系统管理员", Phone="18012345677", Birthday=DateTime.Parse("2000-01-01"), Sex=GenderEnum.Male, AccountType=AccountTypeEnum.SysAdmin, Remark="系统管理员", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrgId=SqlSugarConst.DefaultTenantId, PosId=posList[0].Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysUser{ Id=1300000000112, Account="TestUser1", Password=encryptPassword, NickName="部门主管", RealName="部门主管", Phone="18012345676", Birthday=DateTime.Parse("2000-01-01"), Sex=GenderEnum.Female, AccountType=AccountTypeEnum.NormalUser, Remark="部门主管", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrgId=SqlSugarConst.DefaultTenantId + 1, PosId=posList[1].Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysUser{ Id=1300000000113, Account="TestUser2", Password=encryptPassword, NickName="部门职员", RealName="部门职员", Phone="18012345675", Birthday=DateTime.Parse("2000-01-01"), Sex=GenderEnum.Female, AccountType=AccountTypeEnum.NormalUser, Remark="部门职员", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrgId=SqlSugarConst.DefaultTenantId + 2, PosId=posList[2].Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysUser{ Id=1300000000114, Account="TestUser3", Password=encryptPassword, NickName="普通用户", RealName="普通用户", Phone="18012345674", Birthday=DateTime.Parse("2000-01-01"), Sex=GenderEnum.Female, AccountType=AccountTypeEnum.NormalUser, Remark="普通用户", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrgId=SqlSugarConst.DefaultTenantId + 3, PosId=posList[3].Id, TenantId=SqlSugarConst.DefaultTenantId },
            new SysUser{ Id=1300000000115, Account="TestUser4", Password=encryptPassword, NickName="其他", RealName="其他", Phone="18012345673", Birthday=DateTime.Parse("2000-01-01"), Sex=GenderEnum.Female, AccountType=AccountTypeEnum.Member, Remark="会员", CreateTime=DateTime.Parse("2022-02-10 00:00:00"), OrgId=SqlSugarConst.DefaultTenantId + 4, PosId=posList[4].Id, TenantId=SqlSugarConst.DefaultTenantId },
        };
    }
}
