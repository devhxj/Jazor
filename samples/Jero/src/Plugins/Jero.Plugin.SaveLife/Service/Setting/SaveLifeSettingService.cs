namespace Jero.Plugin.SaveLife.Service;

/// <summary>
/// 小程序设置服务
/// </summary>
[Authorize]
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 175)]
public class SaveLifeSettingService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<SaveLifeSetting> _settingRep;
    private readonly ISaveLifeMiniAppConfigProvider _miniAppConfigProvider;
    private readonly ISaveLifePrivacyConsentGuard _privacyConsentGuard;

    public SaveLifeSettingService(
        SqlSugarRepository<SaveLifeSetting> settingRep,
        ISaveLifeMiniAppConfigProvider miniAppConfigProvider,
        ISaveLifePrivacyConsentGuard privacyConsentGuard)
    {
        _settingRep = settingRep;
        _miniAppConfigProvider = miniAppConfigProvider;
        _privacyConsentGuard = privacyConsentGuard;
    }

    [HttpGet("/api/v1/save-life/settings"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppGet")]
    public async Task<SaveLifeMiniAppSettingOutput> MiniAppGet()
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var setting = await GetOrCreateSetting(externalUserId);
        return ToOutput(setting);
    }

    [HttpPost("/api/v1/save-life/settings"), NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppUpdate")]
    public async Task<SaveLifeMiniAppSettingOutput> MiniAppUpdate(UpdateSaveLifeMiniAppSettingInput input)
    {
        await _privacyConsentGuard.EnsureAccountActiveAsync();
        var externalUserId = GetCurrentMiniAppOpenId();
        var setting = await GetOrCreateSetting(externalUserId);

        var preferencesChanged = false;
        if (input.NotificationEnabled.HasValue)
        {
            setting.NotificationEnabled = input.NotificationEnabled.Value ? "true" : "false";
            preferencesChanged = true;
        }
        if (input.SoundEnabled.HasValue)
        {
            setting.SoundEnabled = input.SoundEnabled.Value ? "true" : "false";
            preferencesChanged = true;
        }
        if (preferencesChanged)
        {
            await _settingRep.AsUpdateable(setting)
                .UpdateColumns(u => new { u.NotificationEnabled, u.SoundEnabled })
                .ExecuteCommandAsync();
        }

        if (!string.IsNullOrWhiteSpace(input.PrivacyConsent))
            await _privacyConsentGuard.RecordRegistrationConsentAsync(
                externalUserId,
                input.PrivacyConsent,
                input.UserAgreementVersion,
                input.PrivacyPolicyVersion);
        if (!string.IsNullOrWhiteSpace(input.SensitiveInformationConsent))
            await _privacyConsentGuard.RecordSensitiveInformationConsentAsync(
                externalUserId,
                input.SensitiveInformationConsent,
                input.SensitiveInformationConsentVersion);

        var updated = await _settingRep.GetFirstAsync(u => u.ExternalUserId == externalUserId);
        return ToOutput(updated ?? setting);
    }

    [HttpPost]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SaveLifeSetting>> Page(SaveLifeSettingInput input)
    {
        var externalUserId = input.ExternalUserId?.Trim();
        var privacyConsent = input.PrivacyConsent?.Trim();
        return await _settingRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(externalUserId), u => u.ExternalUserId == externalUserId)
            .WhereIF(!string.IsNullOrWhiteSpace(privacyConsent), u => u.PrivacyConsent == privacyConsent)
            .OrderBy(u => u.CreateTime, OrderByType.Desc)
            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取医线生机小程序全局配置
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "MiniAppConfig")]
    public async Task<SaveLifeMiniAppConfigOutput> MiniAppConfig()
    {
        return await _miniAppConfigProvider.GetConfigAsync();
    }

    /// <summary>
    /// 保存医线生机小程序全局配置
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SaveMiniAppConfig")]
    public async Task SaveMiniAppConfig(SaveLifeMiniAppConfigInput input)
    {
        await _miniAppConfigProvider.SaveConfigAsync(input);
    }

    /// <summary>
    /// 保存医线生机项目招募开关
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SaveRecruitmentEnabled")]
    public async Task SaveRecruitmentEnabled(SaveLifeRecruitmentEnabledInput input)
    {
        await _miniAppConfigProvider.SaveRecruitmentEnabledAsync(input.Enabled);
    }

    /// <summary>
    /// 获取医线生机项目招募开关
    /// </summary>
    [AllowAnonymous]
    [HttpGet("/api/v1/save-life/recruitment-status"), NonUnify]
    [ApiDescriptionSettings(Name = "RecruitmentStatus")]
    public async Task<SaveLifeRecruitmentStatusOutput> RecruitmentStatus()
    {
        return new SaveLifeRecruitmentStatusOutput
        {
            Enabled = await _miniAppConfigProvider.GetRecruitmentEnabledAsync()
        };
    }

    /// <summary>
    /// 获取医线生机微信小程序码
    /// </summary>
    [HttpGet]
    [NonUnify]
    [ApiDescriptionSettings(Name = "MiniAppQrCode")]
    public async Task<IActionResult> MiniAppQrCode()
    {
        var bytes = await _miniAppConfigProvider.GetQrCodeAsync();
        return new FileContentResult(bytes, "image/png")
        {
            FileDownloadName = "医线生机-微信小程序二维码.png"
        };
    }

    private async Task<SaveLifeSetting> GetOrCreateSetting(string externalUserId)
    {
        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == externalUserId);
        if (setting != null)
            return setting;

        setting = new SaveLifeSetting
        {
            ExternalUserId = externalUserId
        };
        await _settingRep.InsertAsync(setting);
        return setting;
    }

    private static SaveLifeMiniAppSettingOutput ToOutput(SaveLifeSetting setting)
    {
        return new SaveLifeMiniAppSettingOutput
        {
            ExternalUserId = setting.ExternalUserId,
            NotificationEnabled = string.Equals(setting.NotificationEnabled, "true", StringComparison.OrdinalIgnoreCase),
            PrivacyConsent = setting.PrivacyConsent,
            PrivacyConsentTime = setting.PrivacyConsentTime,
            UserAgreementVersion = setting.UserAgreementVersion,
            PrivacyPolicyVersion = setting.PrivacyPolicyVersion,
            SensitiveInformationConsent = setting.SensitiveInformationConsent,
            SensitiveInformationConsentVersion = setting.SensitiveInformationConsentVersion,
            SensitiveInformationConsentTime = setting.SensitiveInformationConsentTime,
            SoundEnabled = string.Equals(setting.SoundEnabled, "true", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string GetCurrentMiniAppOpenId()
    {
        var openId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(openId))
            return openId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }

}
