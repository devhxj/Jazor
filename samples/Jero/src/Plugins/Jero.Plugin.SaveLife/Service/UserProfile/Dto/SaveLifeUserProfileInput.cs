// Jero 项目的版权、商标、专利和其他相关权利均受相应法律法规和许可证的要求。
namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeUserProfileInput : BasePageInput
{
    public string? Gender { get; set; }
}

public class SaveLifeUserProfileBaseInput
{
    public virtual string? ExternalUserId { get; set; }

    public virtual string? RealName { get; set; }

    public virtual string? Gender { get; set; }

    public virtual DateTime? BirthDate { get; set; }

    public virtual string? Phone { get; set; }

    public virtual string? Email { get; set; }

    public virtual string? Address { get; set; }

    public virtual string? Ethnicity { get; set; }

    public virtual string? EmergencyContact { get; set; }

    public virtual string? EmergencyPhone { get; set; }

    public virtual string? MedicalHistory { get; set; }

    public virtual string? Allergies { get; set; }
}

public class AddSaveLifeUserProfileInput : SaveLifeUserProfileBaseInput
{
}

public class UpdateSaveLifeUserProfileInput : SaveLifeUserProfileBaseInput
{
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}

public class DeleteSaveLifeUserProfileInput : BaseIdInput
{
}

public class QueryByIdSaveLifeUserProfileInput : DeleteSaveLifeUserProfileInput
{
}

public class SaveLifeUserProfileDetailOutput
{
    public SaveLifeUserProfile Profile { get; set; } = default!;

    public List<SaveLifeUserProfileMaterialOutput> Materials { get; set; } = new();

    public SaveLifeUserProfileConversationOutput? Conversation { get; set; }
}

public class SaveLifeUserProfileMaterialOutput
{
    public string Id { get; set; } = string.Empty;

    public string EnrollmentRecordId { get; set; } = string.Empty;

    public bool IsPersonalMaterial { get; set; }

    public string? ExternalUserId { get; set; }

    public string ProjectId { get; set; } = string.Empty;

    public string MaterialType { get; set; } = "other";

    public string MaterialTypeText { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? FileSuffix { get; set; }

    public string? ContentType { get; set; }

    public long SizeKb { get; set; }

    public string? SizeInfo { get; set; }

    public string Status { get; set; } = "submitted";

    public string StatusText { get; set; } = string.Empty;

    public string? Remark { get; set; }

    public string? ReviewRemark { get; set; }

    public string? SupplementRequest { get; set; }

    public string ExtractStatus { get; set; } = "pending";

    public string? ExtractStatusText { get; set; }

    public string IndexStatus { get; set; } = "pending";

    public string? IndexStatusText { get; set; }

    public DateTime? ReviewedTime { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }
}

public class SaveLifeUserProfileConversationOutput
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Phase { get; set; }

    public string? LastMessage { get; set; }

    public string? ContextType { get; set; }

    public string? ContextId { get; set; }

    public string? EnrollmentRecordId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<SaveLifeUserProfileConversationMessageOutput> Messages { get; set; } = new();
}

public class SaveLifeUserProfileConversationMessageOutput
{
    public string Id { get; set; } = string.Empty;

    public string ConversationId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? ModelProvider { get; set; }

    public string? UsedModel { get; set; }

    public string? RunStatus { get; set; }

    public string? FallbackReason { get; set; }

    public string? ErrorSummary { get; set; }

    public SaveLifeAgentRetrievalDiagnostics? RetrievalDiagnostics { get; set; }

    public SaveLifeAgentRetrievalHitCountsOutput RetrievalHitCounts { get; set; } = new();
}

public class UpsertSaveLifeMiniAppUserInput
{
    /// <summary>
    /// 微信小程序 Taro.login/wx.login 返回的 code，用于服务端换取 openid
    /// </summary>
    public string? JsCode { get; set; }

    /// <summary>
    /// 微信手机号授权 getPhoneNumber 返回的 code，用于服务端换取手机号
    /// </summary>
    public string? PhoneCode { get; set; }

    public string? RealName { get; set; }

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Address { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public string? PrivacyConsent { get; set; }

    public string? UserAgreementVersion { get; set; }

    public string? PrivacyPolicyVersion { get; set; }
}

public class UpdateSaveLifeMiniAppProfileInput
{
    public string? RealName { get; set; }

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Ethnicity { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }
}

public class UploadSaveLifeMiniAppAvatarInput
{
    [Required(ErrorMessage = "头像文件不能为空")]
    public IFormFile File { get; set; } = default!;
}

public class SaveLifeMiniAppAvatarOutput
{
    public string AvatarUrl { get; set; } = string.Empty;
}

public class RequestSaveLifeMiniAppAccountDeletionInput
{
    [Required(ErrorMessage = "请确认已知悉注销影响后继续")]
    public bool Confirmed { get; set; }
}

public class SaveLifeMiniAppAccountDeletionOutput
{
    public DateTime ProcessedTime { get; set; }

    public bool ReRegistrationAllowed { get; set; }
}

public class SaveLifeMiniAppUserOutput
{
    public string UserId { get; set; } = string.Empty;

    public string OpenId { get; set; } = string.Empty;

    public string ProfileId { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? RealName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? AccessToken { get; set; }

    public int ProfileCompleteness { get; set; }
}

public class SaveLifeMiniAppUserStatsOutput
{
    public int ApplicationCount { get; set; }

    public int FavoriteCount { get; set; }

    public int ConsultationCount { get; set; }

    public int UnreadCount { get; set; }

    public int ProfileCompleteness { get; set; }
}

public class SaveLifeMiniAppProfileOutput
{
    public string ProfileId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string OpenId { get; set; } = string.Empty;

    public string? RealName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Ethnicity { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? MedicalHistory { get; set; }

    public string? Allergies { get; set; }

    public int ProfileCompleteness { get; set; }

    public bool HasBasicProfile { get; set; }

    public bool HasContact { get; set; }

    public bool HasHealthRecord { get; set; }

    public bool HasEmergencyContact { get; set; }
}
