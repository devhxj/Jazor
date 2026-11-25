namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机疾病知识点赞记录
/// </summary>
[SugarTable("sl_knowledge_like", "医线生机疾病知识点赞记录")]
[SugarIndex("index_{table}_K", nameof(KnowledgeId), OrderByType.Asc, nameof(LikeKey), OrderByType.Asc, true)]
[SugarIndex("index_{table}_U", nameof(ExternalUserId), OrderByType.Asc)]
public class SaveLifeKnowledgeLike : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 疾病知识 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "疾病知识 Id")]
    public long KnowledgeId { get; set; }

    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// 幂等点赞键
    /// </summary>
    [Required, MaxLength(128)]
    [SugarColumn(ColumnDescription = "幂等点赞键", Length = 128)]
    public string LikeKey { get; set; } = string.Empty;

    /// <summary>
    /// 来源类型 user/client/ip
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "来源类型", Length = 32)]
    public string SourceType { get; set; } = "client";
}
