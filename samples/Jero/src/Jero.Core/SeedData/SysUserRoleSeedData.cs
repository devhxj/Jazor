namespace Jero.Core;

/// <summary>
/// 系统用户角色表种子数据
/// </summary>
public class SysUserRoleSeedData : ISqlSugarEntitySeedData<SysUserRole>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SysUserRole> HasData()
    {
        var userList = new SysUserSeedData().HasData().ToList();
        var roleList = new SysRoleSeedData().HasData().ToList();
        return new[]
        {
            new SysUserRole{ Id=1300000000101, UserId=userList.First(u => u.Account == "TestUser1").Id, RoleId=roleList.First(u => u.Code == "sys_deptChild").Id },
            new SysUserRole{ Id=1300000000102, UserId=userList.First(u => u.Account == "TestUser2").Id, RoleId=roleList.First(u => u.Code == "sys_dept").Id },
            new SysUserRole{ Id=1300000000103, UserId=userList.First(u => u.Account == "TestUser3").Id, RoleId=roleList.First(u => u.Code == "sys_self").Id },
            new SysUserRole{ Id=1300000000104, UserId=userList.First(u => u.Account == "TestUser4").Id, RoleId=roleList.First(u => u.Code == "sys_define").Id },
        };
    }
}