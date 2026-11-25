namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机小程序公告表
/// </summary>
[SugarTable("sl_announcement", "医线生机小程序公告表")]
[SugarIndex("index_{table}_P", nameof(IsPublished), OrderByType.Asc)]
public class SaveLifeAnnouncement : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 公告标题
    /// </summary>
    [Required, MaxLength(128)]
    [SugarColumn(ColumnDescription = "公告标题", Length = 128)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 公告内容
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "公告内容", Length = 512)]
    public string? Content { get; set; }

    /// <summary>
    /// 公告类型
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "公告类型", Length = 32)]
    public string AnnouncementType { get; set; } = "system";

    /// <summary>
    /// 业务跳转类型
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "业务跳转类型", Length = 32)]
    public string? BusinessType { get; set; }

    /// <summary>
    /// 业务跳转 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "业务跳转 Id")]
    public long? BusinessId { get; set; }

    /// <summary>
    /// 是否发布
    /// </summary>
    [SugarColumn(ColumnDescription = "是否发布")]
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int SortOrder { get; set; } = 100;
}
