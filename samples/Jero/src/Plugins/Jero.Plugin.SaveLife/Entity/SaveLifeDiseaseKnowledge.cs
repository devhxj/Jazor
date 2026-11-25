namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机疾病知识文章表
/// </summary>
[SugarTable("sl_disease_knowledge", "医线生机疾病知识文章表")]
[SugarIndex("index_{table}_C", nameof(CategoryId), OrderByType.Asc)]
[SugarIndex("index_{table}_P", nameof(IsPublished), OrderByType.Asc)]
[SugarIndex("index_{table}_KB", nameof(IsInKnowledgeBase), OrderByType.Asc)]
public class SaveLifeDiseaseKnowledge : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 分类 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "分类 Id")]
    public long? CategoryId { get; set; }

    /// <summary>
    /// 小程序资讯栏目
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "小程序资讯栏目", Length = 32, IsNullable = true)]
    public string? ContentCategory { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required, MaxLength(256)]
    [SugarColumn(ColumnDescription = "标题", Length = 256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 正文
    /// </summary>
    [SugarColumn(ColumnDescription = "正文", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Content { get; set; }

    /// <summary>
    /// 摘要
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "摘要", Length = 512)]
    public string? Summary { get; set; }

    /// <summary>
    /// 封面图
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "封面图", Length = 512)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 标签 JSON
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "标签 JSON", Length = 512)]
    public string? Tags { get; set; }

    /// <summary>
    /// 疾病名称
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "疾病名称", Length = 128)]
    public string? DiseaseName { get; set; }

    /// <summary>
    /// 浏览量
    /// </summary>
    [SugarColumn(ColumnDescription = "浏览量")]
    public int ViewCount { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    [SugarColumn(ColumnDescription = "点赞数")]
    public int LikeCount { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "作者", Length = 64)]
    public string? Author { get; set; }

    /// <summary>
    /// 是否发布
    /// </summary>
    [SugarColumn(ColumnDescription = "是否发布")]
    public bool IsPublished { get; set; }

    /// <summary>
    /// 是否已进入助手公开知识库
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已进入助手公开知识库")]
    public bool IsInKnowledgeBase { get; set; }

    /// <summary>
    /// 知识库入库失败消息
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "知识库入库失败消息", Length = 512)]
    public string? KnowledgeBaseError { get; set; }

    /// <summary>
    /// 知识库入库重试次数
    /// </summary>
    [SugarColumn(ColumnDescription = "知识库入库重试次数")]
    public int KnowledgeBaseRetryCount { get; set; }

    /// <summary>
    /// 最近一次知识库入库尝试时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近一次知识库入库尝试时间")]
    public DateTime? KnowledgeBaseLastAttemptTime { get; set; }

    /// <summary>
    /// 下次允许知识库入库重试时间
    /// </summary>
    [SugarColumn(ColumnDescription = "下次允许知识库入库重试时间")]
    public DateTime? KnowledgeBaseNextRetryTime { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int SortOrder { get; set; } = 100;
}
