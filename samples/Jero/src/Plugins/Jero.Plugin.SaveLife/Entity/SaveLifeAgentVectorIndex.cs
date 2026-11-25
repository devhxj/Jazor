namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机助手向量索引表
/// </summary>
[SugarTable("sl_agent_vector_index", "医线生机助手向量索引表")]
[SugarIndex("index_{table}_R", nameof(ResourceType), OrderByType.Asc, nameof(ResourceId), OrderByType.Asc, nameof(ExternalUserId), OrderByType.Asc, true)]
[SugarIndex("index_{table}_D", nameof(DiseaseName), OrderByType.Asc)]
[SugarIndex("index_{table}_P", nameof(IsPinned), OrderByType.Desc)]
[SugarIndex("index_{table}_PT", nameof(PatientProfileId), OrderByType.Asc)]
[SugarIndex("index_{table}_U", nameof(ExternalUserId), OrderByType.Asc)]
public class SaveLifeAgentVectorIndex : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 私有索引对应的患者档案 Id；公共索引为空
    /// </summary>
    [SugarColumn(ColumnDescription = "患者档案 Id", IsNullable = true)]
    public long? PatientProfileId { get; set; }

    /// <summary>
    /// 资源类型 knowledge/trial/user/enrollment
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "资源类型", Length = 32)]
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// 资源 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "资源 Id")]
    public long ResourceId { get; set; }

    /// <summary>
    /// 外部用户 Id；为空表示公共索引，非空表示用户私有索引
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required, MaxLength(256)]
    [SugarColumn(ColumnDescription = "标题", Length = 256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 疾病名称
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "疾病名称", Length = 128)]
    public string? DiseaseName { get; set; }

    /// <summary>
    /// 索引文本
    /// </summary>
    [SugarColumn(ColumnDescription = "索引文本", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Content { get; set; }

    /// <summary>
    /// 向量 JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "向量 JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? EmbeddingJson { get; set; }

    /// <summary>
    /// 向量模型
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "向量模型", Length = 128)]
    public string? EmbeddingModel { get; set; }

    /// <summary>
    /// 生成向量时请求的配置模型别名
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "配置向量模型", Length = 128, IsNullable = true)]
    public string? EmbeddingConfiguredModel { get; set; }

    /// <summary>
    /// 实际持久化向量维度
    /// </summary>
    [SugarColumn(ColumnDescription = "向量维度", IsNullable = true)]
    public int? EmbeddingDimensions { get; set; }

    /// <summary>
    /// 向量类型 text/multimodal
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "向量类型", Length = 32)]
    public string EmbeddingKind { get; set; } = "text";

    /// <summary>
    /// 文本指纹
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "文本指纹", Length = 64)]
    public string? ContentHash { get; set; }

    /// <summary>
    /// 病例原始文件内容 SHA-256；公共文本索引为空
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "源文件内容 SHA-256", Length = 64, IsNullable = true)]
    public string? SourceContentSha256 { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    [SugarColumn(ColumnDescription = "是否置顶", ColumnDataType = "INTEGER", DefaultValue = "0")]
    public bool IsPinned { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int SortOrder { get; set; } = 100;

    /// <summary>
    /// 同步状态 pending/synced/failed
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "同步状态", Length = 32)]
    public string SyncStatus { get; set; } = "pending";

    /// <summary>
    /// 最近同步错误
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "最近同步错误", Length = 512)]
    public string? SyncError { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    [SugarColumn(ColumnDescription = "重试次数")]
    public int RetryCount { get; set; }

    /// <summary>
    /// 最近索引时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近索引时间")]
    public DateTime? IndexedTime { get; set; }
}
