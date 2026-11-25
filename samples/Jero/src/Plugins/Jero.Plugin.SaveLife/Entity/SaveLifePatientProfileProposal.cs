namespace Jero.Plugin.SaveLife;

/// <summary>
/// 智能助手识别出的待确认患者档案变更。
/// </summary>
[SugarTable("sl_patient_profile_proposal", "患者档案变更提案表")]
[SugarIndex("index_{table}_O", nameof(OwnerExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_CS", nameof(ConversationId), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
[SugarIndex("index_{table}_AM", nameof(SourceAssistantMessageId), OrderByType.Asc)]
[SugarIndex(
    "uq_{table}_PSF",
    nameof(PatientProfileId),
    OrderByType.Asc,
    nameof(SourceUserMessageId),
    OrderByType.Asc,
    nameof(FieldName),
    OrderByType.Asc,
    true)]
public class SaveLifePatientProfileProposal : SaveLifeEntityBaseDel
{
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "所属账号外部用户 Id", Length = 64)]
    public string OwnerExternalUserId { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "来源会话 Id")]
    public long ConversationId { get; set; }

    [SugarColumn(ColumnDescription = "患者档案 Id")]
    public long PatientProfileId { get; set; }

    [SugarColumn(ColumnDescription = "来源用户消息 Id")]
    public long SourceUserMessageId { get; set; }

    [SugarColumn(ColumnDescription = "来源助手消息 Id")]
    public long SourceAssistantMessageId { get; set; }

    [Required, MaxLength(32)]
    [SugarColumn(
        ColumnDescription = "提案来源类型",
        Length = 32,
        DefaultValue = "'user_message'")]
    public string SourceType { get; set; } = SaveLifePatientProfileProposalSources.UserMessage;

    [SugarColumn(ColumnDescription = "来源材料 Id", IsNullable = true)]
    public long? SourceMaterialId { get; set; }

    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "档案字段", Length = 64)]
    public string FieldName { get; set; } = string.Empty;

    [SugarColumn(ColumnDescription = "提案前字段值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? OldValue { get; set; }

    [Required]
    [SugarColumn(ColumnDescription = "建议字段值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ProposedValue { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    [SugarColumn(ColumnDescription = "最新消息原文证据", Length = 500)]
    public string Evidence { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "处理状态", Length = 32)]
    public string Status { get; set; } = SaveLifePatientProfileProposalStatuses.Pending;

    [SugarColumn(ColumnDescription = "过期时间")]
    public DateTime ExpiresTime { get; set; }

    [SugarColumn(ColumnDescription = "处理时间")]
    public DateTime? ResolvedTime { get; set; }
}

public static class SaveLifePatientProfileProposalStatuses
{
    public const string Pending = "pending";
    public const string Confirmed = "confirmed";
    public const string Superseded = "superseded";
    public const string Rejected = "rejected";
    public const string Expired = "expired";
}

public static class SaveLifePatientProfileProposalSources
{
    public const string UserMessage = "user_message";
    public const string Material = "material";
}

public static class SaveLifePatientProfileProposalFields
{
    public const string City = "city";
    public const string Diagnosis = "diagnosis";
    public const string DiseaseStage = "diseaseStage";
    public const string DiseaseStatus = "diseaseStatus";
    public const string TreatmentHistory = "treatmentHistory";
    public const string CurrentTreatment = "currentTreatment";
    public const string Pathology = "pathology";
    public const string Biomarkers = "biomarkers";
    public const string Comorbidities = "comorbidities";
    public const string MedicalHistory = "medicalHistory";
    public const string Allergies = "allergies";

    internal static readonly HashSet<string> All =
    [
        City,
        Diagnosis,
        DiseaseStage,
        DiseaseStatus,
        TreatmentHistory,
        CurrentTreatment,
        Pathology,
        Biomarkers,
        Comorbidities,
        MedicalHistory,
        Allergies
    ];

    public static bool IsAllowed(string? field)
    {
        return !string.IsNullOrWhiteSpace(field) && All.Contains(field);
    }
}
