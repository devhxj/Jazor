using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Jero.Application.Modules.DingTalk;

/// <summary>
/// 钉钉用户表
/// </summary>
[SugarTable(null, "钉钉用户表")]
public class DingTalkUser : EntityBase
{
    /// <summary>
    /// 系统用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "系统用户Id")]
    public long SysUserId { get; set; }

    /// <summary>
    /// 系统用户
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [Navigate(NavigateType.OneToOne, nameof(SysUserId))]
    [JsonIgnore]
    public SysUser? SysUser { get; set; }

    /// <summary>
    /// 钉钉用户id
    /// </summary>
    [SugarColumn(ColumnDescription = "钉钉用户id", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string? DingTalkUserId { get; set; }

    /// <summary>
    /// UnionId
    /// </summary>
    [SugarColumn(ColumnDescription = "UnionId", Length = 64)]
    [MaxLength(64)]
    public string? UnionId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [SugarColumn(ColumnDescription = "用户名", Length = 64)]
    [MaxLength(64)]
    public string? Name { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    [SugarColumn(ColumnDescription = "手机号码", Length = 16)]
    [MaxLength(16)]
    public string? Mobile { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [SugarColumn(ColumnDescription = "性别")]
    public int? Sex { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    [SugarColumn(ColumnDescription = "头像", Length = 256)]
    [MaxLength(256)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 工号
    /// </summary>
    [SugarColumn(ColumnDescription = "工号", Length = 16)]
    [MaxLength(16)]
    public string? JobNumber { get; set; }

    /// <summary>
    /// 主部门Id
    /// </summary>
    [SugarColumn(ColumnDescription = "主部门Id", Length = 16)]
    [MaxLength(16)]
    public string? DeptId { get; set; }

    /// <summary>
    /// 主部门
    /// </summary>
    [SugarColumn(ColumnDescription = "主部门", Length = 16)]
    [MaxLength(16)]
    public string? Dept { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [SugarColumn(ColumnDescription = "职位", Length = 16)]
    [MaxLength(16)]
    public string? Position { get; set; }
}
