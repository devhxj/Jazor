namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 后台智能助手模拟会话初始化和清理接口。
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 161)]
public sealed class SaveLifeAgentSimulationService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeSetting> _settingRep;
    private readonly SqlSugarRepository<SaveLifeUserProfile> _profileRep;
    private readonly SqlSugarRepository<SaveLifePatientProfile> _patientRep;
    private readonly SqlSugarRepository<SaveLifeAgentConversation> _conversationRep;

    public SaveLifeAgentSimulationService(
        SqlSugarRepository<SaveLifeSetting> settingRep,
        SqlSugarRepository<SaveLifeUserProfile> profileRep,
        SqlSugarRepository<SaveLifePatientProfile> patientRep,
        SqlSugarRepository<SaveLifeAgentConversation> conversationRep)
    {
        _settingRep = settingRep;
        _profileRep = profileRep;
        _patientRep = patientRep;
        _conversationRep = conversationRep;
    }

    /// <summary>
    /// 初始化当前后台账号对应的隔离模拟用户。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Prepare")]
    public async Task<SaveLifeAgentSimulationOutput> Prepare()
    {
        var context = App.HttpContext
            ?? throw Oops.Oh("登录态无效，请重新登录").StatusCode(401);
        var externalUserId = SaveLifeAgentSimulationIdentity.RequireSimulationExternalUserId(context);
        var cancellationToken = context.RequestAborted;

        // 模拟用户自动视为已同意当前协议，但只写入合成外部 Id。
        await EnsureSettingAsync(externalUserId, cancellationToken);

        var profile = await EnsureProfileAsync(externalUserId, cancellationToken);
        var patient = await EnsureSelfPatientAsync(profile, externalUserId, cancellationToken);

        return new SaveLifeAgentSimulationOutput
        {
            IsSimulation = true,
            DisplayName = profile.RealName ?? "后台模拟用户",
            ProfileId = profile.Id,
            PatientProfileId = patient?.Id
        };
    }

    private async Task EnsureSettingAsync(
        string externalUserId,
        CancellationToken cancellationToken)
    {
        var setting = await _settingRep.AsQueryable()
            .ClearFilter()
            .Where(item => item.ExternalUserId == externalUserId)
            .FirstAsync(cancellationToken);
        if (setting == null)
        {
            setting = new SaveLifeSetting
            {
                ExternalUserId = externalUserId,
                PrivacyConsent = SaveLifePrivacyConsentGuard.Consented,
                PrivacyConsentTime = DateTime.Now,
                UserAgreementVersion = SaveLifeLegalDocumentVersions.UserAgreement,
                PrivacyPolicyVersion = SaveLifeLegalDocumentVersions.PrivacyPolicy,
                CreateTime = DateTime.Now
            };
            await _settingRep.InsertAsync(setting);
            return;
        }

        setting.ExternalUserId = externalUserId;
        setting.PrivacyConsent = SaveLifePrivacyConsentGuard.Consented;
        setting.PrivacyConsentTime ??= DateTime.Now;
        setting.UserAgreementVersion = SaveLifeLegalDocumentVersions.UserAgreement;
        setting.PrivacyPolicyVersion = SaveLifeLegalDocumentVersions.PrivacyPolicy;
        setting.IsDelete = false;
        setting.DeleteTime = null;
        setting.UpdateTime = DateTime.Now;
        await _settingRep.Context.Updateable(setting)
            .UpdateColumns(item => new
            {
                item.PrivacyConsent,
                item.PrivacyConsentTime,
                item.UserAgreementVersion,
                item.PrivacyPolicyVersion,
                item.IsDelete,
                item.DeleteTime,
                item.UpdateTime
            })
            .ExecuteCommandAsync(cancellationToken);
    }

    /// <summary>
    /// 清空当前后台账号的模拟助手会话和消息。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Clear")]
    public async Task<SaveLifeAgentSimulationClearOutput> Clear()
    {
        var context = App.HttpContext
            ?? throw Oops.Oh("登录态无效，请重新登录").StatusCode(401);
        var externalUserId = SaveLifeAgentSimulationIdentity.RequireSimulationExternalUserId(context);
        var result = await SaveLifeAgentConversationClearPersistence.ClearAsync(
            _conversationRep.Context,
            externalUserId,
            DateTime.Now,
            context.RequestAborted);

        return new SaveLifeAgentSimulationClearOutput
        {
            ClearedConversationCount = result.ConversationCount,
            ClearedMessageCount = result.MessageCount,
            ClearedProfileProposalCount = result.ProfileProposalCount,
            ClearedBusinessCommandCount = result.BusinessCommandCount
        };
    }

    private async Task<SaveLifeUserProfile> EnsureProfileAsync(
        string externalUserId,
        CancellationToken cancellationToken)
    {
        async Task<SaveLifeUserProfile?> LoadExistingAsync()
        {
            return await _profileRep.AsQueryable()
                .ClearFilter()
                .Where(item => item.ExternalUserId == externalUserId)
                .FirstAsync(cancellationToken);
        }

        return await SaveLifeAtomicUpsert.ExecuteAsync(
            LoadExistingAsync,
            () => new SaveLifeUserProfile
            {
                ExternalUserId = externalUserId,
                RealName = "后台模拟用户",
                CreateTime = DateTime.Now
            },
            profile =>
            {
                profile.ExternalUserId = externalUserId;
                profile.RealName = string.IsNullOrWhiteSpace(profile.RealName)
                    ? "后台模拟用户"
                    : profile.RealName.Trim();
                profile.IsDelete = false;
                profile.DeleteTime = null;
                profile.UpdateTime = DateTime.Now;
            },
            async profile => await _profileRep.InsertAsync(profile),
            async profile => await _profileRep.Context.Updateable(profile)
                .UpdateColumns(item => new
                {
                    item.ExternalUserId,
                    item.RealName,
                    item.IsDelete,
                    item.DeleteTime,
                    item.UpdateTime
                })
                .ExecuteCommandAsync(cancellationToken));
    }

    private async Task<SaveLifePatientProfile?> EnsureSelfPatientAsync(
        SaveLifeUserProfile profile,
        string externalUserId,
        CancellationToken cancellationToken)
    {
        var patientKey = SaveLifePatientProfileKeys.ForSelf(externalUserId);

        async Task<SaveLifePatientProfile?> LoadExistingAsync()
        {
            return await _patientRep.AsQueryable()
                .ClearFilter()
                .Where(item =>
                    item.OwnerExternalUserId == externalUserId &&
                    item.AccountPatientKey == patientKey)
                .FirstAsync(cancellationToken);
        }

        return await SaveLifeAtomicUpsert.ExecuteAsync(
            LoadExistingAsync,
            () => SaveLifePatientProfileFactory.CreateSelf(profile, externalUserId),
            patient =>
            {
                patient.OwnerExternalUserId = externalUserId;
                patient.AccountPatientKey = patientKey;
                patient.IsDelete = false;
                patient.DeleteTime = null;
                patient.UpdateTime = DateTime.Now;
            },
            async patient => await _patientRep.InsertAsync(patient),
            async patient => await _patientRep.Context.Updateable(patient)
                .UpdateColumns(item => new
                {
                    item.OwnerExternalUserId,
                    item.AccountPatientKey,
                    item.IsDelete,
                    item.DeleteTime,
                    item.UpdateTime
                })
                .ExecuteCommandAsync(cancellationToken));
    }
}

public sealed class SaveLifeAgentSimulationOutput
{
    public bool IsSimulation { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public long ProfileId { get; set; }

    public long? PatientProfileId { get; set; }
}

public sealed class SaveLifeAgentSimulationClearOutput
{
    public int ClearedConversationCount { get; set; }

    public int ClearedMessageCount { get; set; }

    public int ClearedProfileProposalCount { get; set; }

    public int ClearedBusinessCommandCount { get; set; }
}
