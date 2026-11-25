namespace Jero.Plugin.SaveLife;

/// <summary>
/// 医线生机小程序设置表
/// </summary>
[SugarTable("sl_setting", "医线生机小程序设置表")]
[SugarIndex("index_{table}_E", nameof(ExternalUserId), OrderByType.Asc, true)]
public class SaveLifeSetting : SaveLifeEntityBaseDel
{
    [Required, MaxLength(64)]
    [SugarColumn(ColumnDescription = "外部用户 Id", Length = 64)]
    public string ExternalUserId { get; set; } = string.Empty;

    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "通知开关", Length = 32)]
    public string NotificationEnabled { get; set; } = "true";

    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "隐私授权状态", Length = 32)]
    public string PrivacyConsent { get; set; } = "unknown";

    [SugarColumn(ColumnDescription = "隐私授权时间")]
    public DateTime? PrivacyConsentTime { get; set; }

    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "用户协议版本", Length = 16)]
    public string? UserAgreementVersion { get; set; }

    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "隐私政策版本", Length = 16)]
    public string? PrivacyPolicyVersion { get; set; }

    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "敏感信息授权状态", Length = 32)]
    public string SensitiveInformationConsent { get; set; } = "unknown";

    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "敏感信息授权版本", Length = 16)]
    public string? SensitiveInformationConsentVersion { get; set; }

    [SugarColumn(ColumnDescription = "敏感信息授权时间")]
    public DateTime? SensitiveInformationConsentTime { get; set; }

    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "声音开关", Length = 32)]
    public string SoundEnabled { get; set; } = "true";
}
