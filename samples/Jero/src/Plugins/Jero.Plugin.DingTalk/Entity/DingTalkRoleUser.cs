namespace Jero.Plugin.DingTalk;

/// <summary>
/// 钉钉角色信息
/// </summary>
[SugarTable(null, "钉钉角色表")]
public class DingTalkRoleUser : EntityBaseDel
{
    /// <summary>
    /// 钉钉用户id
    /// </summary>
    [SugarColumn(ColumnDescription = "钉钉用户id", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string? DingTalkUserId { get; set; }

    /// <summary>
    /// 角色组id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色组id")]
    [Required]
    public virtual long groupId { get; set; }

    /// <summary>
    /// 角色组名称
    /// </summary>
    [SugarColumn(ColumnDescription = "角色组名", Length = 64)]
    [MaxLength(64)]
    public string? groupName { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色id")]
    [Required]
    public virtual long roleId { get; set; }

    /// <summary>
    /// 角色名
    /// </summary>
    [SugarColumn(ColumnDescription = "角色名", Length = 64)]
    [MaxLength(64)]
    public string? roleName { get; set; }
}