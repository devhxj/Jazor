namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机报名记录表
/// </summary>
[SugarTable("sl_enrollment_record", "医线生机报名记录表")]
[SugarIndex("index_{table}_P", nameof(ProjectId), OrderByType.Asc)]
[SugarIndex("index_{table}_PT", nameof(PatientProfileId), OrderByType.Asc)]
[SugarIndex("index_{table}_S", nameof(Status), OrderByType.Asc)]
public class SaveLifeEnrollmentRecord : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 患者档案 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "患者档案 Id", IsNullable = true)]
    public long? PatientProfileId { get; set; }

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
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// 项目 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "项目 Id")]
    public long ProjectId { get; set; }

    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "项目名称", Length = 256)]
    public string? ProjectTitle { get; set; }

    /// <summary>
    /// 联系人姓名
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "联系人姓名", Length = 64)]
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "联系电话", Length = 32)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 报名状态
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "报名状态", Length = 32)]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// 提交时间
    /// </summary>
    [SugarColumn(ColumnDescription = "提交时间")]
    public DateTime? SubmittedTime { get; set; }

    /// <summary>
    /// 审核时间
    /// </summary>
    [SugarColumn(ColumnDescription = "审核时间")]
    public DateTime? AuditTime { get; set; }

    /// <summary>
    /// 审核备注
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "审核备注", Length = 512)]
    public string? AuditRemark { get; set; }

    /// <summary>
    /// 病情描述
    /// </summary>
    [SugarColumn(ColumnDescription = "病情描述", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? DiseaseDescription { get; set; }

    /// <summary>
    /// 病史
    /// </summary>
    [SugarColumn(ColumnDescription = "病史", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MedicalHistory { get; set; }

    /// <summary>
    /// 诊断报告
    /// </summary>
    [SugarColumn(ColumnDescription = "诊断报告", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? DiagnosisReport { get; set; }

    /// <summary>
    /// 用药情况
    /// </summary>
    [SugarColumn(ColumnDescription = "用药情况", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MedicationStatus { get; set; }

    /// <summary>
    /// 其他信息
    /// </summary>
    [SugarColumn(ColumnDescription = "其他信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? OtherInfo { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Notes { get; set; }
}
