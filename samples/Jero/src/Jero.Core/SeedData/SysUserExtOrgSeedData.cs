namespace Jero.Core;

/// <summary>
/// 系统用户扩展机构表种子数据
/// </summary>
public class SysUserExtOrgSeedData : ISqlSugarEntitySeedData<SysUserExtOrg>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysUserExtOrg> HasData()
    {
        var userList = new SysUserSeedData().HasData().ToList();
        var orgList = new SysOrgSeedData().HasData().ToList();
        var posList = new SysPosSeedData().HasData().ToList();
        var admin = userList.First(u => u.Account == "Jero");
        var user3 = userList.First(u => u.Account == "TestUser3");
        var org1 = orgList.First(u => u.Name == "系统默认");
        var org2 = orgList.First(u => u.Name == "开发部");
        var pos1 = posList.First(u => u.Name == "部门经理");
        var pos2 = posList.First(u => u.Name == "主任");
        return new[]
        {
            new SysUserExtOrg{ Id=1300000000101, UserId=admin.Id, OrgId=org1.Id, PosId=pos1.Id },
            new SysUserExtOrg{ Id=1300000000102, UserId=user3.Id, OrgId=org2.Id, PosId=pos2.Id  }
        };
    }
}