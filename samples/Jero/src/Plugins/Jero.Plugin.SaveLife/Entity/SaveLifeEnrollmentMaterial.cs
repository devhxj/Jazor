namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机报名材料表
/// </summary>
[SugarTable("sl_enrollment_material", "医线生机报名材料表")]
[SugarIndex("index_{table}_E", nameof(EnrollmentRecordId), OrderByType.Asc)]
[SugarIndex("index_{table}_P", nameof(PatientProfileId), OrderByType.Asc)]
[SugarIndex("index_{table}_U", nameof(ExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
public class SaveLifeEnrollmentMaterial : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 患者档案 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "患者档案 Id", IsNullable = true)]
    public long? PatientProfileId { get; set; }

    /// <summary>
    /// 报名记录 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "报名记录 Id")]
    public long EnrollmentRecordId { get; set; }

    /// <summary>
    /// 用户档案 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户档案 Id")]
    public long? UserProfileId { get; set; }

    /// <summary>
    /// 外部用户 Id
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 项目 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "项目 Id")]
    public long ProjectId { get; set; }

    /// <summary>
    /// 系统文件 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "系统文件 Id")]
    public long FileId { get; set; }

    /// <summary>
    /// 文件实际字节数
    /// </summary>
    [SugarColumn(ColumnDescription = "文件实际字节数")]
    public long SizeBytes { get; set; }

    /// <summary>
    /// 文件内容 SHA-256
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "文件内容 SHA-256", Length = 64, IsNullable = true)]
    public string? ContentSha256 { get; set; }

    /// <summary>
    /// 患者材料范围内的活动槽位 1-10
    /// </summary>
    [SugarColumn(ColumnDescription = "活动上传槽位", IsNullable = true)]
    public int? UploadSlot { get; set; }

    /// <summary>
    /// 材料类型
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "材料类型", Length = 32)]
    public string MaterialType { get; set; } = "other";

    /// <summary>
    /// 文件名称
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "文件名称", Length = 128)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件后缀
    /// </summary>
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "文件后缀", Length = 16)]
    public string? FileSuffix { get; set; }

    /// <summary>
    /// 文件 Content-Type
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "文件 Content-Type", Length = 128)]
    public string? ContentType { get; set; }

    /// <summary>
    /// 文件大小 KB
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小 KB")]
    public long SizeKb { get; set; }

    /// <summary>
    /// 文件大小文本
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "文件大小文本", Length = 64)]
    public string? SizeInfo { get; set; }

    /// <summary>
    /// 材料状态
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "材料状态", Length = 32)]
    public string Status { get; set; } = "submitted";

    /// <summary>
    /// 用户备注
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "用户备注", Length = 512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 审核备注
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "审核备注", Length = 512)]
    public string? ReviewRemark { get; set; }

    /// <summary>
    /// 补充资料要求
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "补充资料要求", Length = 512)]
    public string? SupplementRequest { get; set; }

    /// <summary>
    /// 材料提取文本，用于私有知识检索
    /// </summary>
    [SugarColumn(ColumnDescription = "材料提取文本", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? ExtractedText { get; set; }

    /// <summary>
    /// 材料解析状态 pending/processing/synced/failed/skipped
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "材料解析状态", Length = 32)]
    public string ExtractStatus { get; set; } = "pending";

    /// <summary>
    /// 最近解析错误
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "最近解析错误", Length = 512)]
    public string? ExtractError { get; set; }

    /// <summary>
    /// 最近解析时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近解析时间")]
    public DateTime? ExtractedTime { get; set; }

    /// <summary>
    /// 解析模型
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "解析模型", Length = 128)]
    public string? ExtractModel { get; set; }

    /// <summary>
    /// 材料索引状态 pending/synced/failed
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "材料索引状态", Length = 32)]
    public string IndexStatus { get; set; } = "pending";

    /// <summary>
    /// 最近索引错误
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "最近索引错误", Length = 512)]
    public string? IndexError { get; set; }

    /// <summary>
    /// 最近索引时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近索引时间")]
    public DateTime? IndexedTime { get; set; }

    /// <summary>
    /// 后台处理重试次数
    /// </summary>
    [SugarColumn(ColumnDescription = "后台处理重试次数")]
    public int ProcessRetryCount { get; set; }

    /// <summary>
    /// 下次允许后台处理重试时间
    /// </summary>
    [SugarColumn(ColumnDescription = "下次允许后台处理重试时间")]
    public DateTime? NextProcessRetryTime { get; set; }

    /// <summary>
    /// 后台处理代次；删除、重排和内容变化时单调递增
    /// </summary>
    [SugarColumn(ColumnDescription = "后台处理代次", DefaultValue = "1")]
    public long ProcessingRevision { get; set; } = 1;

    /// <summary>
    /// 当前后台处理租约所有者
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "后台处理租约所有者", Length = 64, IsNullable = true)]
    public string? ProcessingLeaseOwner { get; set; }

    /// <summary>
    /// 当前后台处理租约过期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "后台处理租约过期时间", IsNullable = true)]
    public DateTime? ProcessingLeaseExpiresTime { get; set; }

    /// <summary>
    /// 审核时间
    /// </summary>
    [SugarColumn(ColumnDescription = "审核时间")]
    public DateTime? ReviewedTime { get; set; }
}
