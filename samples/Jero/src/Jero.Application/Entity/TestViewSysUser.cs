


using SqlSugar;
namespace Jero.Application;

/// <summary>
/// 用户表视图（必须加IgnoreTable，防止被生成为表）
/// </summary>
[SugarTable(null, "用户表视图"), IgnoreTable]
public class TestViewSysUser : EntityBase, ISqlSugarView
{
    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号")]
    public required virtual string Account { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "真实姓名")]
    public required virtual string RealName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 机构名称
    /// </summary>
    [SugarColumn(ColumnDescription = "机构名称")]
    public string? OrgName { get; set; }

    /// <summary>
    /// 职位名称
    /// </summary>
    [SugarColumn(ColumnDescription = "职位名称")]
    public string? PosName { get; set; }

    /// <summary>
    /// 查询实例
    /// </summary>
    /// <param name="db"></param>
    /// <returns></returns>
    public string GetQueryableSqlString(SqlSugarScopeProvider db)
    {
        return db.Queryable<SysUser>()
            .LeftJoin<SysOrg>((u, a) => u.OrgId == a.Id)
            .LeftJoin<SysPos>((u, a, b) => u.PosId == b.Id)
            .Select((u, a, b) => new TestViewSysUser
            {
                Id = u.Id,
                Account = u.Account,
                RealName = u.RealName,
                NickName = u.NickName,
                OrgName = a.Name,
                PosName = b.Name,
            }).ToMappedSqlString();
    }
}