namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机临床试验项目表
/// </summary>
[SugarTable("sl_clinical_trial", "医线生机临床试验项目表")]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
[SugarIndex("index_{table}_D", nameof(DiseaseArea), OrderByType.Asc)]
[SugarIndex("index_{table}_P", nameof(IsPinned), OrderByType.Desc)]
[SugarIndex("index_{table}_KB", nameof(IsInKnowledgeBase), OrderByType.Asc)]
[SugarIndex("uq_{table}_CDE", nameof(CdeRegistrationNumber), OrderByType.Asc, true)]
public class SaveLifeClinicalTrial : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 试验题目
    /// </summary>
    [Required, MaxLength(256)]
    [SugarColumn(ColumnDescription = "试验题目", Length = 256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 方案编号
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "方案编号", Length = 64)]
    public string? ProtocolNumber { get; set; }

    /// <summary>
    /// CDE 登记号
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "CDE 登记号", Length = 64, IsNullable = false)]
    public string CdeRegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// 项目描述
    /// </summary>
    [SugarColumn(ColumnDescription = "项目描述", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Description { get; set; }

    /// <summary>
    /// 申办方
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "申办方", Length = 128)]
    public string? Sponsor { get; set; }

    /// <summary>
    /// 临床阶段
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "临床阶段", Length = 32)]
    public string? ClinicalPhase { get; set; }

    /// <summary>
    /// 疾病领域
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "疾病领域", Length = 64)]
    public string? DiseaseArea { get; set; }

    /// <summary>
    /// 具体适应症
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "适应症", Length = 512)]
    public string? Indication { get; set; }

    /// <summary>
    /// 项目状态
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "项目状态", Length = 32)]
    public string Status { get; set; } = "recruiting";

    /// <summary>
    /// 是否置顶
    /// </summary>
    [SugarColumn(ColumnDescription = "是否置顶", ColumnDataType = "INTEGER", DefaultValue = "0")]
    public bool IsPinned { get; set; }

    /// <summary>
    /// 目标招募人数
    /// </summary>
    [SugarColumn(ColumnDescription = "目标招募人数")]
    public int? TargetCount { get; set; }

    /// <summary>
    /// 已招募人数
    /// </summary>
    [SugarColumn(ColumnDescription = "已招募人数")]
    public int? EnrolledCount { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    [SugarColumn(ColumnDescription = "开始日期")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    [SugarColumn(ColumnDescription = "结束日期")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 受试者类型
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "受试者类型", Length = 64)]
    public string? ParticipantType { get; set; }

    /// <summary>
    /// 入选标准
    /// </summary>
    [SugarColumn(ColumnDescription = "入选标准", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? InclusionCriteria { get; set; }

    /// <summary>
    /// 排除标准
    /// </summary>
    [SugarColumn(ColumnDescription = "排除标准", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? ExclusionCriteria { get; set; }

    /// <summary>
    /// 试验周期
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "试验周期", Length = 128)]
    public string? TrialDuration { get; set; }

    /// <summary>
    /// 受试者权益
    /// </summary>
    [SugarColumn(ColumnDescription = "受试者权益", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Benefits { get; set; }

    /// <summary>
    /// 伦理委员会信息
    /// </summary>
    [SugarColumn(ColumnDescription = "伦理委员会信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? EthicsCommitteeInfo { get; set; }

    /// <summary>
    /// 研究中心
    /// </summary>
    [SugarColumn(ColumnDescription = "研究中心", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Sites { get; set; }

    /// <summary>
    /// 地点
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "地点", Length = 128)]
    public string? Location { get; set; }

    /// <summary>
    /// 招募要求摘要
    /// </summary>
    [SugarColumn(ColumnDescription = "招募要求摘要", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Requirements { get; set; }

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
