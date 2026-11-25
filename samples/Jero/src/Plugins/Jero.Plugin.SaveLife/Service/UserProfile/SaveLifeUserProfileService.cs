using Microsoft.AspNetCore.Http.Timeouts;

namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 用户健康档案服务
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 140)]
public class SaveLifeUserProfileService : IDynamicApiController, ITransient
{
    private const string MiniAppAvatarPath = "upload/save-life/avatar";
    private const string UserPagePermission = "saveLifeUser:page";

    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientProfileRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentRecord> _enrollmentRep;
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep;
    private readonly SqlSugarRepository<SaveLifeAgentConversation> _conversationRep;
    private readonly SqlSugarRepository<SaveLifeAgentMessage> _messageRep;
    private readonly SqlSugarRepository<SaveLifeNotification> _notificationRep;
    private readonly SqlSugarRepository<SaveLifeFavorite> _favoriteRep;
    private readonly SysConfigService _sysConfigService;
    private readonly ISaveLifeMiniAppConfigProvider _miniAppConfigProvider;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;
    private readonly SysFileService _sysFileService;
    private readonly ISaveLifeFileAccessService _fileAccessService;
    private readonly UserManager _userManager;
    private readonly SysMenuService _sysMenuService;

    public SaveLifeUserProfileService(
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifePatientProfile> patientProfileRep,
        SqlSugarRepository<SaveLifeEnrollmentRecord> enrollmentRep,
        SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
        SqlSugarRepository<SaveLifeAgentConversation> conversationRep,
        SqlSugarRepository<SaveLifeAgentMessage> messageRep,
        SqlSugarRepository<SaveLifeNotification> notificationRep,
        SqlSugarRepository<SaveLifeFavorite> favoriteRep,
        SysConfigService sysConfigService,
        ISaveLifeMiniAppConfigProvider miniAppConfigProvider,
        ISaveLifePrivacyConsentGuard privacyConsentGuard,
        SysFileService sysFileService,
        ISaveLifeFileAccessService fileAccessService,
        UserManager userManager,
        SysMenuService sysMenuService)
    {
        _profileRep = profileRep;
        _patientProfileRep = patientProfileRep;
        _enrollmentRep = enrollmentRep;
        _materialRep = materialRep;
        _conversationRep = conversationRep;
        _messageRep = messageRep;
        _notificationRep = notificationRep;
        _favoriteRep = favoriteRep;
        _sysConfigService = sysConfigService;
        _miniAppConfigProvider = miniAppConfigProvider;
        _privacyConsentGuard = privacyConsentGuard;
        _sysFileService = sysFileService;
        _fileAccessService = fileAccessService;
        _userManager = userManager;
        _sysMenuService = sysMenuService;
    }

    /// <summary>
    /// 分页查询用户健康档案
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeUserProfile>> Page(SaveLifeUserProfileInput input)
    {
        var keyword = input.Keyword?.Trim();
        return await _profileRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Gender), u => u.Gender == input.Gender)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                (u.RealName != null && u.RealName.Contains(keyword!)) ||
                (u.Phone != null && u.Phone.Contains(keyword!)) ||
                (u.Email != null && u.Email.Contains(keyword!)) ||
                (u.ExternalUserId != null && u.ExternalUserId.Contains(keyword!)))
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 新增用户健康档案
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<long> Add(AddSaveLifeUserProfileInput input)
    {
        var entity = input.Adapt<SaveLifeUserProfile>();
        await _profileRep.InsertAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 更新用户健康档案
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Update")]
    public async Task Update(UpdateSaveLifeUserProfileInput input)
    {
        await _profileRep.AsUpdateable(input.Adapt<SaveLifeUserProfile>()).IgnoreColumns(true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除用户健康档案
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Delete")]
    public async Task Delete(DeleteSaveLifeUserProfileInput input)
    {
        var entity = await _profileRep.GetByIdAsync(input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _profileRep.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 获取用户健康档案详情
    /// </summary>
    public async Task<SaveLifeUserProfileDetailOutput> GetDetail([FromQuery] QueryByIdSaveLifeUserProfileInput input)
    {
        var profile = await _profileRep.GetByIdAsync(input.Id)
            ?? throw Oops.Oh(ErrorCodeEnum.D1002);

        return new SaveLifeUserProfileDetailOutput
        {
            Profile = profile,
            Materials = await LoadUserMaterials(profile),
            Conversation = await LoadUserConversation(profile)
        };
    }

    /// <summary>
    /// 下载后台用户详情中的上传资料
    /// </summary>
    [Authorize]
    [HttpGet("/api/saveLifeUserProfile/materials/{userId}/{materialId}/download"), NonUnify]
    [ApiDescriptionSettings(Name = "DownloadMaterial")]
    [SuppressMonitor]
    public async Task<IActionResult> DownloadMaterial(long userId, long materialId)
    {
        await EnsureCanDownloadUserMaterial();

        var profile = await _profileRep.GetByIdAsync(userId)
            ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        var externalUserId = profile.ExternalUserId?.Trim();
        var material = await _materialRep.AsQueryable()
            .Where(u => u.Id == materialId)
            .WhereIF(string.IsNullOrWhiteSpace(externalUserId), u => u.UserProfileId == profile.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(externalUserId),
                u => u.ExternalUserId == externalUserId || u.UserProfileId == profile.Id)
            .FirstAsync()
            ?? throw Oops.Oh("资料不存在或不属于该用户").StatusCode(404);

        return await _fileAccessService.DownloadAsync(material.FileId);
    }

    /// <summary>
    /// 小程序注册/授权用户并写入医线生机用户档案
    /// </summary>
    [AllowAnonymous]
    [HttpPost("/api/v1/save-life/users/miniapp/upsert"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppUpsert")]
    public async Task<SaveLifeMiniAppUserOutput> MiniAppUpsert(UpsertSaveLifeMiniAppUserInput input)
    {
        var openId = await ResolveOpenId(input);
        var consentDecision = await _privacyConsentGuard.EvaluateMiniAppUpsertAsync(
            openId,
            input.PrivacyConsent,
            input.UserAgreementVersion,
            input.PrivacyPolicyVersion);
        if (consentDecision.ShouldRecordConsent && !string.IsNullOrWhiteSpace(consentDecision.ConsentToRecord))
            await _privacyConsentGuard.RecordRegistrationConsentAsync(
                openId,
                consentDecision.ConsentToRecord,
                input.UserAgreementVersion,
                input.PrivacyPolicyVersion);

        if (!consentDecision.CanProceed)
            throw Oops.Oh(consentDecision.ErrorMessage ?? SaveLifeMiniAppConsentPolicy.ConsentRequiredMessage).StatusCode(403);

        var phone = await ResolvePhone(input, openId);

        var profile = await SaveLifeAtomicUpsert.ExecuteAsync(
            async () => (SaveLifeUserProfile?)await _profileRep.AsQueryable()
                .ClearFilter()
                .Where(u => u.ExternalUserId == openId)
                .FirstAsync(),
            () => new SaveLifeUserProfile { ExternalUserId = openId },
            entity =>
            {
                entity.ExternalUserId = openId;
                entity.IsDelete = false;
                entity.DeleteTime = null;
                ApplyMiniAppUser(entity, input, phone);
            },
            async entity =>
            {
                await _profileRep.InsertAsync(entity);
            },
            entity =>
            {
                _profileRep.RunWithoutFilter(() =>
                    _profileRep.AsUpdateable(entity).ExecuteCommand());
                return Task.CompletedTask;
            });
        await SyncLegacyProfileToSelfPatient(profile);

        return new SaveLifeMiniAppUserOutput
        {
            UserId = openId,
            OpenId = openId,
            ProfileId = profile.Id.ToString(),
            Phone = profile.Phone,
            RealName = profile.RealName,
            AvatarUrl = BuildPublicAvatarUrl(profile.AvatarUrl),
            AccessToken = await CreateMiniAppAccessToken(profile, openId),
            ProfileCompleteness = SaveLifeMiniAppProfileHelper.CalculateCompleteness(profile).Score
        };
    }

    /// <summary>
    /// 小程序当前用户资料详情
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/users/miniapp/profile"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppProfile")]
    public async Task<SaveLifeMiniAppProfileOutput> MiniAppProfile()
    {
        await _privacyConsentGuard.EnsureConsentedAsync();
        var profile = await GetCurrentMiniAppProfile();
        var selfPatient = await GetSelfPatient(profile.ExternalUserId!);
        if (selfPatient != null)
            SaveLifePatientProfileCompatibility.ApplyPatientToLegacy(selfPatient, profile);

        return ToMiniAppProfileOutput(profile);
    }

    /// <summary>
    /// 小程序当前用户资料更新
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/users/miniapp/profile"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppUpdateProfile")]
    public async Task<SaveLifeMiniAppProfileOutput> MiniAppUpdateProfile(UpdateSaveLifeMiniAppProfileInput input)
    {
        await _privacyConsentGuard.EnsureConsentedAsync();
        await _privacyConsentGuard.EnsureSensitiveInformationConsentedAsync();
        var profile = await GetCurrentMiniAppProfile();

        ApplyMiniAppProfile(profile, input);
        await SaveLifeMiniAppProfilePersistence.UpdateAsync(_profileRep.Context, profile);
        await SyncLegacyProfileToSelfPatient(profile);

        return ToMiniAppProfileOutput(profile);
    }

    /// <summary>
    /// 小程序当前用户上传头像
    /// </summary>
    [Authorize]
    [HttpPost("/api/v1/save-life/users/miniapp/avatar"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppUploadAvatar")]
    [RequestSizeLimit(SaveLifeAvatarUploadPolicy.UploadRequestBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = SaveLifeAvatarUploadPolicy.MaxFileBytes,
        ValueCountLimit = SaveLifeAvatarUploadPolicy.FormValueCountLimit,
        KeyLengthLimit = SaveLifeAvatarUploadPolicy.FormKeyLengthLimit,
        ValueLengthLimit = SaveLifeAvatarUploadPolicy.FormValueLengthLimit,
        MultipartBoundaryLengthLimit = SaveLifeAvatarUploadPolicy.MultipartBoundaryLengthLimit,
        MultipartHeadersCountLimit = SaveLifeAvatarUploadPolicy.MultipartHeadersCountLimit,
        MultipartHeadersLengthLimit = SaveLifeAvatarUploadPolicy.MultipartHeadersLengthLimit)]
    [RequestTimeout(SaveLifeAvatarUploadPolicy.UploadTimeoutMilliseconds)]
    public async Task<SaveLifeMiniAppAvatarOutput> MiniAppUploadAvatar(
        [FromForm] UploadSaveLifeMiniAppAvatarInput input)
    {
        await _privacyConsentGuard.EnsureConsentedAsync();
        var profile = await GetCurrentMiniAppProfile();
        SaveLifeAvatarUploadPolicy.Validate(input.File);

        SysFile? uploadedFile = null;
        try
        {
            uploadedFile = await _sysFileService.UploadFile(
                new UploadFileInput
                {
                    File = input.File,
                    AllowSuffix = ".jpg.png"
                },
                MiniAppAvatarPath);

            var previousAvatarFileId = profile.AvatarFileId;
            profile.AvatarFileId = uploadedFile.Id;
            profile.AvatarUrl = uploadedFile.Url;
            await SaveLifeMiniAppProfilePersistence.UpdateAvatarAsync(_profileRep.Context, profile);

            if (previousAvatarFileId != uploadedFile.Id)
                await DeleteAvatarFileIfUnreferenced(previousAvatarFileId);

            return new SaveLifeMiniAppAvatarOutput
            {
                AvatarUrl = BuildPublicAvatarUrl(uploadedFile.Url) ?? string.Empty
            };
        }
        catch
        {
            if (uploadedFile != null)
                await DeleteAvatarFileIfUnreferenced(uploadedFile.Id);
            throw;
        }
    }

    /// <summary>
    /// 小程序用户个人中心统计
    /// </summary>
    [Authorize]
    [HttpGet("/api/v1/save-life/users/miniapp/stats"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppStats")]
    public async Task<SaveLifeMiniAppUserStatsOutput> MiniAppStats()
    {
        var profile = await GetCurrentMiniAppProfile();
        await _privacyConsentGuard.EnsureConsentedAsync(profile.ExternalUserId);
        var externalUserId = profile.ExternalUserId!;
        var applicationCount = await _enrollmentRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .CountAsync();
        var hasConversation = await _conversationRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .AnyAsync();
        var unreadCount = await _notificationRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId && !u.IsRead)
            .CountAsync();
        var favoriteCount = await _favoriteRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .CountAsync();

        return new SaveLifeMiniAppUserStatsOutput
        {
            ApplicationCount = applicationCount,
            ConsultationCount = hasConversation ? 1 : 0,
            FavoriteCount = favoriteCount,
            UnreadCount = unreadCount,
            ProfileCompleteness = SaveLifeMiniAppProfileHelper.CalculateCompleteness(profile).Score
        };
    }

    private async Task<string> ResolveOpenId(UpsertSaveLifeMiniAppUserInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.JsCode))
        {
            var output = await _miniAppConfigProvider.GetOpenIdAsync(input.JsCode.Trim());
            return output.OpenId;
        }

        throw Oops.Oh("微信登录 code 不能为空").StatusCode(400);
    }

    private static string NormalizeRequired(string? value, string errorMessage)
    {
        var normalized = value?.Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
            return normalized;

        throw Oops.Oh(errorMessage).StatusCode(400);
    }

    private async Task<SaveLifeUserProfile> GetCurrentMiniAppProfile()
    {
        var openId = NormalizeRequired(App.User?.FindFirst(ClaimConst.OpenId)?.Value, "小程序登录态无效，请重新登录");
        var profile = await _profileRep.GetFirstAsync(u => u.ExternalUserId == openId);

        return profile ?? throw Oops.Oh("用户档案不存在，请重新登录").StatusCode(404);
    }

    private async Task EnsureCanDownloadUserMaterial()
    {
        var loginMode = App.User?.FindFirst(ClaimConst.LoginMode)?.Value;
        if (loginMode == ((int)LoginModeEnum.APP).ToString() ||
            loginMode?.Equals(nameof(LoginModeEnum.APP), StringComparison.OrdinalIgnoreCase) == true)
        {
            throw Oops.Oh("小程序账号无权下载后台用户资料").StatusCode(403);
        }

        if (_userManager.SuperAdmin || _userManager.SysAdmin)
            return;

        var permissions = await _sysMenuService.GetOwnBtnPermList();
        if (permissions.Any(permission => permission.Equals(UserPagePermission, StringComparison.OrdinalIgnoreCase)))
            return;

        throw Oops.Oh("无权下载用户资料").StatusCode(403);
    }

    private async Task<SaveLifePatientProfile?> GetSelfPatient(string externalUserId)
    {
        return await _patientProfileRep.GetFirstAsync(u =>
            u.OwnerExternalUserId == externalUserId &&
            u.RelationshipToAccount == SaveLifePatientRelationships.Self);
    }

    private async Task<SaveLifePatientProfile> SyncLegacyProfileToSelfPatient(
        SaveLifeUserProfile profile)
    {
        var externalUserId = NormalizeRequired(
            profile.ExternalUserId,
            "用户档案缺少小程序账号标识");
        async Task<SaveLifePatientProfile?> LoadExisting()
        {
            return await GetSelfPatient(externalUserId);
        }

        return await SaveLifeAtomicUpsert.ExecuteAsync(
            LoadExisting,
            () => SaveLifePatientProfileFactory.CreateSelf(profile, externalUserId),
            patient => SaveLifePatientProfileCompatibility.ApplyLegacyToSelf(profile, patient),
            async patient => await _patientProfileRep.InsertAsync(patient),
            async patient => await _patientProfileRep.AsUpdateable(patient)
                .UpdateColumns(u => new
                {
                    u.DisplayName,
                    u.Gender,
                    u.BirthDate,
                    u.Ethnicity,
                    u.Residence,
                    u.MedicalHistory,
                    u.Allergies,
                    u.UpdateTime
                })
                .ExecuteCommandAsync());
    }

    private async Task<string> CreateMiniAppAccessToken(SaveLifeUserProfile profile, string openId)
    {
        var tokenExpire = await _sysConfigService.GetTokenExpire();

        return JWTEncryption.Encrypt(new Dictionary<string, object>
        {
            { ClaimConst.UserId, profile.Id },
            { ClaimConst.OpenId, openId },
            { ClaimConst.RealName, string.IsNullOrWhiteSpace(profile.RealName) ? "医线生机小程序用户" : profile.RealName },
            { ClaimConst.LoginMode, LoginModeEnum.APP },
            { "SaveLifeProfileId", profile.Id }
        }, tokenExpire);
    }

    private async Task<string?> ResolvePhone(UpsertSaveLifeMiniAppUserInput input, string openId)
    {
        if (!string.IsNullOrWhiteSpace(input.PhoneCode))
        {
            var output = await _miniAppConfigProvider.GetPhoneAsync(openId, input.PhoneCode.Trim());
            return output.PhoneNumber;
        }

        return null;
    }

    private async Task DeleteAvatarFileIfUnreferenced(long? fileId)
    {
        if (!fileId.HasValue || fileId.Value <= 0)
            return;

        try
        {
            var isReferenced = await _profileRep.AsQueryable()
                .ClearFilter()
                .AnyAsync(u => u.AvatarFileId == fileId.Value);
            if (isReferenced)
                return;

            await _sysFileService.TryDeleteFile(fileId.Value);
        }
        catch
        {
            // File cleanup must not invalidate an already persisted avatar change.
        }
    }

    private static string? BuildPublicAvatarUrl(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri) &&
            absoluteUri.Scheme is "http" or "https")
        {
            return normalized;
        }

        return $"{CommonUtil.GetLocalhost().TrimEnd('/')}/{normalized.TrimStart('/')}";
    }

    private static void ApplyMiniAppUser(
        SaveLifeUserProfile profile,
        UpsertSaveLifeMiniAppUserInput input,
        string? phone)
    {
        profile.RealName = Pick(profile.RealName, input.RealName, 64);
        profile.Gender = Pick(profile.Gender, input.Gender, 16);
        profile.BirthDate ??= input.BirthDate;
        profile.Phone = Pick(profile.Phone, phone, 32);
        profile.Address = Pick(profile.Address, input.Address, 256);
        profile.MedicalHistory = Pick(profile.MedicalHistory, input.MedicalHistory, 4000);
        profile.Allergies = Pick(profile.Allergies, input.Allergies, 4000);
    }

    private static void ApplyMiniAppProfile(SaveLifeUserProfile profile, UpdateSaveLifeMiniAppProfileInput input)
    {
        profile.RealName = SaveLifeMiniAppProfileHelper.NormalizeText(input.RealName, 64, "姓名");
        profile.Gender = SaveLifeMiniAppProfileHelper.NormalizeGender(input.Gender);
        profile.BirthDate = SaveLifeMiniAppProfileHelper.NormalizeBirthDate(input.BirthDate);
        profile.Phone = SaveLifeMiniAppProfileHelper.NormalizePhone(input.Phone, "手机号");
        profile.Email = SaveLifeMiniAppProfileHelper.NormalizeEmail(input.Email);
        profile.Address = SaveLifeMiniAppProfileHelper.NormalizeText(input.Address, 256, "地址");
        profile.Ethnicity = SaveLifeMiniAppProfileHelper.NormalizeText(input.Ethnicity, 32, "民族");
        profile.EmergencyContact = SaveLifeMiniAppProfileHelper.NormalizeText(input.EmergencyContact, 64, "紧急联系人");
        profile.EmergencyPhone = SaveLifeMiniAppProfileHelper.NormalizePhone(input.EmergencyPhone, "紧急联系电话");
        profile.MedicalHistory = SaveLifeMiniAppProfileHelper.NormalizeText(input.MedicalHistory, 4000, "既往病史");
        profile.Allergies = SaveLifeMiniAppProfileHelper.NormalizeText(input.Allergies, 4000, "过敏史");
    }

    private static SaveLifeMiniAppProfileOutput ToMiniAppProfileOutput(SaveLifeUserProfile profile)
    {
        var completeness = SaveLifeMiniAppProfileHelper.CalculateCompleteness(profile);
        var openId = profile.ExternalUserId ?? string.Empty;

        return new SaveLifeMiniAppProfileOutput
        {
            ProfileId = profile.Id.ToString(),
            UserId = openId,
            OpenId = openId,
            RealName = profile.RealName,
            AvatarUrl = BuildPublicAvatarUrl(profile.AvatarUrl),
            Gender = profile.Gender,
            BirthDate = profile.BirthDate,
            Phone = profile.Phone,
            Email = profile.Email,
            Address = profile.Address,
            Ethnicity = profile.Ethnicity,
            EmergencyContact = profile.EmergencyContact,
            EmergencyPhone = profile.EmergencyPhone,
            MedicalHistory = profile.MedicalHistory,
            Allergies = profile.Allergies,
            ProfileCompleteness = completeness.Score,
            HasBasicProfile = completeness.HasBasicProfile,
            HasContact = completeness.HasContact,
            HasHealthRecord = completeness.HasHealthRecord,
            HasEmergencyContact = completeness.HasEmergencyContact
        };
    }

    private async Task<List<SaveLifeUserProfileMaterialOutput>> LoadUserMaterials(SaveLifeUserProfile profile)
    {
        var externalUserId = profile.ExternalUserId?.Trim();
        var query = _materialRep.AsQueryable();
        query = string.IsNullOrWhiteSpace(externalUserId)
            ? query.Where(u => u.UserProfileId == profile.Id)
            : query.Where(u => u.ExternalUserId == externalUserId || u.UserProfileId == profile.Id);

        var materials = await query
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToListAsync();

        return materials.Select(ToMaterialOutput).ToList();
    }

    private async Task<SaveLifeUserProfileConversationOutput?> LoadUserConversation(SaveLifeUserProfile profile)
    {
        var externalUserId = profile.ExternalUserId?.Trim();
        if (string.IsNullOrWhiteSpace(externalUserId))
            return null;

        var conversation = await _conversationRep.AsQueryable()
            .Where(u => u.ExternalUserId == externalUserId)
            .OrderBy(u => u.UpdateTime, OrderByType.Desc)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .FirstAsync();

        if (conversation == null)
            return null;

        var messages = await _messageRep.AsQueryable()
            .Where(u => u.ConversationId == conversation.Id && u.ExternalUserId == externalUserId)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .Take(10)
            .ToListAsync();

        return ToConversationOutput(
            conversation,
            messages
                .OrderBy(u => u.CreateTime)
                .Select(ToConversationMessageOutput)
                .ToList());
    }

    private static SaveLifeUserProfileMaterialOutput ToMaterialOutput(SaveLifeEnrollmentMaterial material)
    {
        return new SaveLifeUserProfileMaterialOutput
        {
            Id = material.Id.ToString(),
            EnrollmentRecordId = material.EnrollmentRecordId.ToString(),
            IsPersonalMaterial = SaveLifeEnrollmentMaterialHelper.IsPersonalMaterial(material),
            ExternalUserId = material.ExternalUserId,
            ProjectId = material.ProjectId.ToString(),
            MaterialType = material.MaterialType,
            MaterialTypeText = SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType),
            FileName = BuildDisplayFileName(material),
            FileSuffix = material.FileSuffix,
            ContentType = material.ContentType,
            SizeKb = material.SizeKb,
            SizeInfo = material.SizeInfo ?? SaveLifeEnrollmentMaterialHelper.FormatSize(material.SizeKb),
            Status = material.Status,
            StatusText = SaveLifeEnrollmentMaterialHelper.GetStatusText(material.Status),
            Remark = material.Remark,
            ReviewRemark = material.ReviewRemark,
            SupplementRequest = material.SupplementRequest,
            ExtractStatus = material.ExtractStatus,
            ExtractStatusText = SaveLifeEnrollmentMaterialHelper.GetExtractStatusText(material.ExtractStatus),
            IndexStatus = material.IndexStatus,
            IndexStatusText = GetIndexStatusText(material.IndexStatus),
            ReviewedTime = material.ReviewedTime,
            CreateTime = material.CreateTime,
            UpdateTime = material.UpdateTime
        };
    }

    private static SaveLifeUserProfileConversationOutput ToConversationOutput(
        SaveLifeAgentConversation conversation,
        List<SaveLifeUserProfileConversationMessageOutput> messages)
    {
        return new SaveLifeUserProfileConversationOutput
        {
            Id = conversation.Id.ToString(),
            UserId = conversation.ExternalUserId,
            Status = conversation.Status,
            Phase = conversation.Phase,
            LastMessage = conversation.LastMessage,
            ContextType = conversation.ContextType,
            ContextId = conversation.ContextId?.ToString(),
            EnrollmentRecordId = conversation.EnrollmentRecordId?.ToString(),
            CreatedAt = conversation.CreateTime,
            UpdatedAt = conversation.UpdateTime,
            Messages = messages
        };
    }

    private static SaveLifeUserProfileConversationMessageOutput ToConversationMessageOutput(SaveLifeAgentMessage message)
    {
        var metadata = ParseConversationMessageMetadata(message.MetadataJson);

        return new SaveLifeUserProfileConversationMessageOutput
        {
            Id = message.Id.ToString(),
            ConversationId = message.ConversationId.ToString(),
            Role = message.Role,
            Content = Limit(message.Content, 1000) ?? string.Empty,
            CreatedAt = message.CreateTime,
            ModelProvider = metadata?.ModelProvider,
            UsedModel = metadata?.UsedModel,
            RunStatus = metadata?.RunStatus,
            FallbackReason = metadata?.FallbackReason,
            ErrorSummary = metadata?.ErrorSummary,
            RetrievalDiagnostics = metadata?.RetrievalDiagnostics,
            RetrievalHitCounts = metadata?.RetrievalHitCounts ?? new()
        };
    }

    private static SaveLifeUserProfileConversationMessageMetadata? ParseConversationMessageMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson) || !JSON.IsValid(metadataJson))
            return null;

        try
        {
            return JSON.Deserialize<SaveLifeUserProfileConversationMessageMetadata>(metadataJson);
        }
        catch
        {
            return null;
        }
    }

    private sealed class SaveLifeUserProfileConversationMessageMetadata
    {
        public string? ModelProvider { get; set; }

        public string? UsedModel { get; set; }

        public string? RunStatus { get; set; }

        public string? FallbackReason { get; set; }

        public string? ErrorSummary { get; set; }

        public SaveLifeAgentRetrievalDiagnostics? RetrievalDiagnostics { get; set; }

        public SaveLifeAgentRetrievalHitCountsOutput RetrievalHitCounts { get; set; } = new();
    }

    private static string GetIndexStatusText(string? status)
    {
        return status switch
        {
            "synced" => "已进入私有知识库",
            "failed" => "索引失败",
            _ => "等待索引"
        };
    }

    private static string BuildDisplayFileName(SaveLifeEnrollmentMaterial material)
    {
        var fileName = material.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return "病例材料";

        return !string.IsNullOrWhiteSpace(material.FileSuffix) && !fileName.EndsWith(material.FileSuffix, StringComparison.OrdinalIgnoreCase)
            ? $"{fileName}{material.FileSuffix}"
            : fileName;
    }

    private static string? Pick(string? current, string? candidate, int maxLength)
    {
        var normalized = candidate?.Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
            return normalized.Length <= maxLength ? normalized : normalized[..maxLength];

        return current;
    }

    private static string? Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
