namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeMiniAppSettingOutput
{
    public string ExternalUserId { get; set; } = string.Empty;

    public bool NotificationEnabled { get; set; } = true;

    public string PrivacyConsent { get; set; } = "unknown";

    public DateTime? PrivacyConsentTime { get; set; }

    public string? UserAgreementVersion { get; set; }

    public string? PrivacyPolicyVersion { get; set; }

    public string SensitiveInformationConsent { get; set; } = "unknown";

    public string? SensitiveInformationConsentVersion { get; set; }

    public DateTime? SensitiveInformationConsentTime { get; set; }

    public bool SoundEnabled { get; set; } = true;
}

public class UpdateSaveLifeMiniAppSettingInput
{
    public bool? NotificationEnabled { get; set; }

    public string? PrivacyConsent { get; set; }

    public string? UserAgreementVersion { get; set; }

    public string? PrivacyPolicyVersion { get; set; }

    public string? SensitiveInformationConsent { get; set; }

    public string? SensitiveInformationConsentVersion { get; set; }

    public bool? SoundEnabled { get; set; }
}

public class SaveLifeMiniAppConfigOutput
{
    public string? AppId { get; set; }

    public bool HasAppSecret { get; set; }

    public string? AppSecretMasked { get; set; }

    public string? Token { get; set; }

    public string? EncodingAesKey { get; set; }

    public string? QrImagePath { get; set; }

    public bool IsConfigured { get; set; }

    public bool RecruitmentEnabled { get; set; } = true;
}

public class SaveLifeMiniAppConfigInput
{
    public string? AppId { get; set; }

    public string? AppSecret { get; set; }

    public string? Token { get; set; }

    public string? EncodingAesKey { get; set; }

    public string? QrImagePath { get; set; }
}

public class SaveLifeMiniAppConfigSecret
{
    public string? AppId { get; set; }

    public string? AppSecret { get; set; }

    public string? Token { get; set; }

    public string? EncodingAesKey { get; set; }

    public string? QrImagePath { get; set; }

    public bool RecruitmentEnabled { get; set; } = true;
}

public class SaveLifeRecruitmentEnabledInput
{
    public bool Enabled { get; set; } = true;
}

public class SaveLifeRecruitmentStatusOutput
{
    public bool Enabled { get; set; } = true;
}

public class SubmitSaveLifeFeedbackInput
{
    [Required(ErrorMessage = "反馈类型不能为空")]
    public string FeedbackType { get; set; } = string.Empty;

    [Required(ErrorMessage = "反馈内容不能为空")]
    public string Content { get; set; } = string.Empty;

    public string? Contact { get; set; }
}

public class SaveLifeFeedbackOutput
{
    public string Id { get; set; } = string.Empty;

    public string FeedbackType { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Contact { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
