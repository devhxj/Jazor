namespace Jero.Plugin.DingTalk;

/// <summary>
/// 钉钉部门信息
/// </summary>
[SugarTable("ding_talk_dept", "钉钉部门表")]
public class DingTalkDept
{
    /// <summary>
    /// 部门id
    /// </summary>
    [SugarColumn(ColumnName = "Id", ColumnDescription = "部门id", IsPrimaryKey = true, IsIdentity = false)]
    [Required]
    public long dept_id { get; set; }

    /// <summary>
    /// 上级部门id
    /// </summary>
    [SugarColumn(ColumnDescription = "上级部门id")]
    [Required]
    public virtual long parent_id { get; set; }

    /// <summary>
    /// 部门名
    /// </summary>
    [SugarColumn(ColumnDescription = "部门名", Length = 64)]
    [MaxLength(64)]
    public string? name { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsNullable = true, IsOnlyIgnoreUpdate = true)]
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public virtual DateTime? UpdateTime { get; set; }
}