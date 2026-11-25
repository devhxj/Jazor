namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机小程序通知表
/// </summary>
[SugarTable("sl_notification", "医线生机小程序通知表")]
[SugarIndex("index_{table}_U", nameof(ExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_R", nameof(IsRead), OrderByType.Asc)]
[SugarIndex("index_{table}_B", nameof(BusinessType), OrderByType.Asc, nameof(BusinessId), OrderByType.Asc)]
public class SaveLifeNotification : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 通知类型
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "通知类型", Length = 32)]
    public string NotificationType { get; set; } = "system";

    /// <summary>
    /// 标题
    /// </summary>
    [Required, MaxLength(128)]
    [SugarColumn(ColumnDescription = "标题", Length = 128)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "内容", Length = 512)]
    public string? Content { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "业务类型", Length = 32)]
    public string? BusinessType { get; set; }

    /// <summary>
    /// 业务 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "业务 Id")]
    public long? BusinessId { get; set; }

    /// <summary>
    /// 是否已读
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已读")]
    public bool IsRead { get; set; }

    /// <summary>
    /// 已读时间
    /// </summary>
    [SugarColumn(ColumnDescription = "已读时间")]
    public DateTime? ReadTime { get; set; }
}
