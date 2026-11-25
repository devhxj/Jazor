namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机小程序反馈表
/// </summary>
[SugarTable("sl_feedback", "医线生机小程序反馈表")]
[SugarIndex("index_{table}_E", nameof(ExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
public class SaveLifeFeedback : SaveLifeEntityBaseDel
{
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "反馈类型", Length = 32)]
    public string FeedbackType { get; set; } = "general";

    [Required, MaxLength(512)]
    [SugarColumn(ColumnDescription = "反馈内容", Length = 512)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "联系方式", Length = 64)]
    public string? Contact { get; set; }

    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "处理状态", Length = 32)]
    public string Status { get; set; } = "pending";
}
