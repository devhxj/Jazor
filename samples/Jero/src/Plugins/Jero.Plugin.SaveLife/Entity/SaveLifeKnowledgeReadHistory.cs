namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机疾病知识阅读历史表
/// </summary>
[SugarTable("sl_knowledge_read_history", "医线生机疾病知识阅读历史表")]
[SugarIndex("index_{table}_U", nameof(ExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_K", nameof(KnowledgeId), OrderByType.Asc)]
[SugarIndex("index_{table}_UK", nameof(ExternalUserId), OrderByType.Asc, nameof(KnowledgeId), OrderByType.Asc, true)]
public class SaveLifeKnowledgeReadHistory : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 知识文章 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "知识文章 Id")]
    public long KnowledgeId { get; set; }

    /// <summary>
    /// 阅读次数
    /// </summary>
    [SugarColumn(ColumnDescription = "阅读次数")]
    public int ReadCount { get; set; } = 1;

    /// <summary>
    /// 最近阅读时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近阅读时间")]
    public DateTime LastReadTime { get; set; } = DateTime.Now;
}
