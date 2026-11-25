namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机患者健康档案表。
/// </summary>
[SugarTable("sl_patient_profile", "医线生机患者健康档案表")]
[SugarIndex("index_{table}_O", nameof(OwnerExternalUserId), OrderByType.Asc)]
[SugarIndex("index_{table}_S", nameof(SourceUserProfileId), OrderByType.Asc)]
[SugarIndex("uq_{table}_K", nameof(AccountPatientKey), OrderByType.Asc, true)]
public class SaveLifePatientProfile : SaveLifeEntityBaseDel
{
    /// <summary>
    /// 所属账号的外部用户 Id。
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "所属账号外部用户 Id", Length = 64)]
    public string OwnerExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// 账号范围内的稳定患者身份键。
    /// </summary>
    [Required, MaxLength(128)]
    [SugarColumn(ColumnDescription = "账号患者身份键", Length = 128)]
    public string AccountPatientKey { get; set; } = string.Empty;

    /// <summary>
    /// 兼容迁移来源的账号资料 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源账号资料 Id", IsNullable = true)]
    public long? SourceUserProfileId { get; set; }

    /// <summary>
    /// 患者与账号使用者的关系。
    /// </summary>
    [Required, MaxLength(32)]
    [SugarColumn(ColumnDescription = "与账号使用者关系", Length = 32)]
    public string RelationshipToAccount { get; set; } = SaveLifePatientRelationships.Other;

    /// <summary>
    /// 用户可读的具体关系描述，例如本人、母亲、父亲或配偶。
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "具体关系描述", Length = 64)]
    public string RelationshipDescription { get; set; } = string.Empty;

    /// <summary>
    /// 聊天页面使用的患者显示名称。
    /// </summary>
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "患者显示名称", Length = 64)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为账号默认患者。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否默认患者")]
    public bool IsDefault { get; set; }

    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "性别", Length = 16)]
    public string? Gender { get; set; }

    [SugarColumn(ColumnDescription = "出生日期")]
    public DateTime? BirthDate { get; set; }

    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "民族", Length = 32)]
    public string? Ethnicity { get; set; }

    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "所在城市", Length = 128)]
    public string? City { get; set; }

    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "居住信息", Length = 256)]
    public string? Residence { get; set; }

    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "疾病或诊断", Length = 256)]
    public string? Diagnosis { get; set; }

    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "疾病阶段", Length = 128)]
    public string? DiseaseStage { get; set; }

    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "当前病情状态", Length = 256)]
    public string? DiseaseStatus { get; set; }

    [SugarColumn(ColumnDescription = "既往治疗", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? TreatmentHistory { get; set; }

    [SugarColumn(ColumnDescription = "当前治疗", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? CurrentTreatment { get; set; }

    [SugarColumn(ColumnDescription = "病理信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Pathology { get; set; }

    [SugarColumn(ColumnDescription = "分子标志物", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Biomarkers { get; set; }

    [SugarColumn(ColumnDescription = "合并疾病", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Comorbidities { get; set; }

    [SugarColumn(ColumnDescription = "既往病史", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? MedicalHistory { get; set; }

    [SugarColumn(ColumnDescription = "过敏史", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Allergies { get; set; }
}

public static class SaveLifePatientRelationships
{
    public const string Self = "self";
    public const string Parent = "parent";
    public const string Child = "child";
    public const string Spouse = "spouse";
    public const string Relative = "relative";
    public const string Other = "other";
}

public static class SaveLifePatientProfileKeys
{
    public static string ForSelf(string externalUserId)
    {
        var owner = NormalizeOwner(externalUserId);
        return $"self:{owner}";
    }

    public static string ForOther(long patientProfileId)
    {
        if (patientProfileId <= 0)
            throw new ArgumentOutOfRangeException(nameof(patientProfileId));

        return $"patient:{patientProfileId}";
    }

    private static string NormalizeOwner(string externalUserId)
    {
        var owner = externalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("账号外部用户 Id 不能为空", nameof(externalUserId));
        if (owner.Length > 64)
            throw new ArgumentException("账号外部用户 Id 长度不能超过 64", nameof(externalUserId));

        return owner;
    }
}

public static class SaveLifePatientProfileFactory
{
    public static SaveLifePatientProfile CreateSelf(
        SaveLifeUserProfile source,
        string ownerExternalUserId)
    {
        ArgumentNullException.ThrowIfNull(source);
        ownerExternalUserId = ownerExternalUserId.Trim();

        return new SaveLifePatientProfile
        {
            Id = source.Id,
            TenantId = source.TenantId,
            OwnerExternalUserId = ownerExternalUserId,
            AccountPatientKey = SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId),
            SourceUserProfileId = source.Id,
            RelationshipToAccount = SaveLifePatientRelationships.Self,
            RelationshipDescription = "本人",
            DisplayName = FirstText(source.RealName) ?? "本人",
            IsDefault = true,
            Gender = FirstText(source.Gender),
            BirthDate = source.BirthDate,
            Ethnicity = FirstText(source.Ethnicity),
            Residence = FirstText(source.Address),
            MedicalHistory = FirstText(source.MedicalHistory),
            Allergies = FirstText(source.Allergies),
            CreateTime = source.CreateTime == default ? DateTime.Now : source.CreateTime,
            UpdateTime = source.UpdateTime
        };
    }

    private static string? FirstText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
