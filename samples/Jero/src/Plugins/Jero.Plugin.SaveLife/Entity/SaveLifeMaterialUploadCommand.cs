namespace Jero.Plugin.SaveLife;

[SugarTable("sl_material_upload_command", "医线生机材料上传命令表")]
[SugarIndex("index_{table}_M", nameof(MaterialId), OrderByType.Asc)]
public class SaveLifeMaterialUploadCommand : SaveLifeEntityBaseDel
{
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "患者档案 Id")]
    public long PatientProfileId { get; set; }

    [SugarColumn(ColumnDescription = "报名记录 Id，个人材料为 0")]
    public long EnrollmentRecordId { get; set; }

    [Required, MaxLength(128)]
    [SugarColumn(ColumnDescription = "客户端命令 Id", Length = 128)]
    public string ClientCommandId { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "载荷摘要算法", Length = 32)]
    public string PayloadHashAlgorithm { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "载荷摘要", Length = 64)]
    public string PayloadHash { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "文件内容 SHA-256", Length = 64)]
    public string ContentSha256 { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "命令状态", Length = 32)]
    public string Status { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "材料 Id", IsNullable = true)]
    public long? MaterialId { get; set; }

    [SugarColumn(ColumnDescription = "派发 outbox Id", IsNullable = true)]
    public long? OutboxId { get; set; }

    [SugarColumn(
        ColumnDescription = "首次结果 JSON",
        ColumnDataType = StaticConfig.CodeFirst_BigString,
        IsNullable = true)]
    public string? ResultJson { get; set; }
}

[SugarTable("sl_material_work_outbox", "医线生机材料可靠任务表")]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc, nameof(NextAttemptTime), OrderByType.Asc)]
public class SaveLifeMaterialWorkOutbox : SaveLifeEntityBaseDel
{
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "任务类型", Length = 32)]
    public string WorkType { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "聚合 Id")]
    public long AggregateId { get; set; }

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "任务状态", Length = 32)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "租约所有者", Length = 64, IsNullable = true)]
    public string? LeaseOwner { get; set; }

    [SugarColumn(ColumnDescription = "租约过期时间", IsNullable = true)]
    public DateTime? LeaseExpiresTime { get; set; }

    [SugarColumn(ColumnDescription = "尝试次数")]
    public int AttemptCount { get; set; }

    [SugarColumn(ColumnDescription = "下次尝试时间", IsNullable = true)]
    public DateTime? NextAttemptTime { get; set; }

    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "最近错误码", Length = 64, IsNullable = true)]
    public string? LastErrorCode { get; set; }
}
