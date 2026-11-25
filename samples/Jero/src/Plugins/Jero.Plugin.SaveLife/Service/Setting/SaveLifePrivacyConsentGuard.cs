namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifePrivacyConsentGuard
{
    Task EnsureAccountActiveAsync() => Task.CompletedTask;

    Task EnsureConsentedAsync(string? externalUserId = null);

    Task EnsureSensitiveInformationConsentedAsync(string? externalUserId = null) =>
        EnsureConsentedAsync(externalUserId);

    Task RecordConsentAsync(string externalUserId, string? privacyConsent);

    Task RecordRegistrationConsentAsync(
        string externalUserId,
        string? privacyConsent,
        string? userAgreementVersion,
        string? privacyPolicyVersion) => RecordConsentAsync(externalUserId, privacyConsent);

    Task RecordSensitiveInformationConsentAsync(
        string externalUserId,
        string? consent,
        string? version) => Task.CompletedTask;

    Task<SaveLifeMiniAppConsentDecision> EvaluateMiniAppUpsertAsync(string externalUserId, string? privacyConsent);

    Task<SaveLifeMiniAppConsentDecision> EvaluateMiniAppUpsertAsync(
        string externalUserId,
        string? privacyConsent,
        string? userAgreementVersion,
        string? privacyPolicyVersion) => EvaluateMiniAppUpsertAsync(externalUserId, privacyConsent);
}

public sealed class SaveLifePrivacyConsentGuard : ISaveLifePrivacyConsentGuard, ITransient
{
    public const string Consented = "consented";
    public const string Rejected = "rejected";
    public const string Unknown = "unknown";

    private readonly SqlSugarRepository<SaveLifeSetting> _settingRep;
    private readonly SqlSugarRepository<SaveLifeAccountDeletionRecord> _accountDeletionRep;

    public SaveLifePrivacyConsentGuard(
        SqlSugarRepository<SaveLifeSetting> settingRep,
        SqlSugarRepository<SaveLifeAccountDeletionRecord> accountDeletionRep)
    {
        _settingRep = settingRep;
        _accountDeletionRep = accountDeletionRep;
    }

    public async Task EnsureAccountActiveAsync()
    {
        var profileIdValue = App.User?.FindFirst(ClaimConst.UserId)?.Value;
        if (!long.TryParse(profileIdValue, out var profileId) || profileId <= 0)
            return;

        if (await _accountDeletionRep.IsAnyAsync(item => item.DeletedProfileId == profileId))
            throw Oops.Oh("账号已注销，请重新注册后使用").StatusCode(401);
    }

    public async Task EnsureConsentedAsync(string? externalUserId = null)
    {
        await EnsureAccountActiveAsync();
        var userId = ResolveExternalUserId(externalUserId);
        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == userId);
        if (setting == null ||
            !string.Equals(setting.PrivacyConsent, Consented, StringComparison.OrdinalIgnoreCase))
            throw Oops.Oh("请先同意用户协议和隐私政策后继续").StatusCode(403);
        if (!SaveLifeMiniAppConsentPolicy.HasCurrentRegistrationVersions(
                setting.UserAgreementVersion,
                setting.PrivacyPolicyVersion))
            throw Oops.Oh(SaveLifeMiniAppConsentPolicy.ConsentVersionRequiredMessage).StatusCode(403);
    }

    public async Task EnsureSensitiveInformationConsentedAsync(string? externalUserId = null)
    {
        var userId = ResolveExternalUserId(externalUserId);
        await EnsureConsentedAsync(userId);
        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == userId);
        if (setting == null ||
            !string.Equals(
                setting.SensitiveInformationConsent,
                Consented,
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                setting.SensitiveInformationConsentVersion,
                SaveLifeLegalDocumentVersions.SensitiveInformationAuthorization,
                StringComparison.Ordinal))
            throw Oops.Oh("请先阅读并同意当前版本的个人敏感信息授权书").StatusCode(403);
    }

    public async Task RecordConsentAsync(string externalUserId, string? privacyConsent)
    {
        var userId = ResolveExternalUserId(externalUserId);
        var normalized = NormalizePrivacyConsent(privacyConsent);
        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == userId);
        if (setting == null)
        {
            setting = new SaveLifeSetting
            {
                ExternalUserId = userId
            };
            ApplyConsent(setting, normalized);
            await _settingRep.InsertAsync(setting);
            return;
        }

        ApplyConsent(setting, normalized);
        await _settingRep.AsUpdateable(setting)
            .UpdateColumns(u => new { u.PrivacyConsent, u.PrivacyConsentTime })
            .ExecuteCommandAsync();
    }

    public async Task RecordRegistrationConsentAsync(
        string externalUserId,
        string? privacyConsent,
        string? userAgreementVersion,
        string? privacyPolicyVersion)
    {
        var userId = ResolveExternalUserId(externalUserId);
        var normalized = NormalizePrivacyConsent(privacyConsent);
        if (normalized == Consented &&
            !SaveLifeMiniAppConsentPolicy.HasCurrentRegistrationVersions(
                userAgreementVersion,
                privacyPolicyVersion))
            throw Oops.Oh(SaveLifeMiniAppConsentPolicy.ConsentVersionRequiredMessage).StatusCode(403);

        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == userId);
        if (setting == null)
        {
            setting = new SaveLifeSetting { ExternalUserId = userId };
            ApplyRegistrationConsent(setting, normalized, userAgreementVersion, privacyPolicyVersion);
            await _settingRep.InsertAsync(setting);
            return;
        }

        ApplyRegistrationConsent(setting, normalized, userAgreementVersion, privacyPolicyVersion);
        await _settingRep.AsUpdateable(setting)
            .UpdateColumns(u => new
            {
                u.PrivacyConsent,
                u.PrivacyConsentTime,
                u.UserAgreementVersion,
                u.PrivacyPolicyVersion
            })
            .ExecuteCommandAsync();
    }

    public async Task RecordSensitiveInformationConsentAsync(
        string externalUserId,
        string? consent,
        string? version)
    {
        var userId = ResolveExternalUserId(externalUserId);
        var normalized = NormalizePrivacyConsent(consent);
        var normalizedVersion = version?.Trim();
        if (normalized == Consented &&
            !string.Equals(
                normalizedVersion,
                SaveLifeLegalDocumentVersions.SensitiveInformationAuthorization,
                StringComparison.Ordinal))
            throw Oops.Oh("个人敏感信息授权书已更新，请重新阅读并同意").StatusCode(403);

        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == userId);
        if (setting == null)
        {
            setting = new SaveLifeSetting { ExternalUserId = userId };
            ApplySensitiveInformationConsent(setting, normalized, normalizedVersion);
            await _settingRep.InsertAsync(setting);
            return;
        }

        ApplySensitiveInformationConsent(setting, normalized, normalizedVersion);
        await _settingRep.AsUpdateable(setting)
            .UpdateColumns(u => new
            {
                u.SensitiveInformationConsent,
                u.SensitiveInformationConsentVersion,
                u.SensitiveInformationConsentTime
            })
            .ExecuteCommandAsync();
    }

    public async Task<SaveLifeMiniAppConsentDecision> EvaluateMiniAppUpsertAsync(string externalUserId, string? privacyConsent)
    {
        return await EvaluateMiniAppUpsertAsync(externalUserId, privacyConsent, null, null);
    }

    public async Task<SaveLifeMiniAppConsentDecision> EvaluateMiniAppUpsertAsync(
        string externalUserId,
        string? privacyConsent,
        string? userAgreementVersion,
        string? privacyPolicyVersion)
    {
        var userId = ResolveExternalUserId(externalUserId);
        var setting = await _settingRep.GetFirstAsync(u => u.ExternalUserId == userId);

        return SaveLifeMiniAppConsentPolicy.EvaluateUpsert(
            privacyConsent,
            setting?.PrivacyConsent,
            userAgreementVersion,
            privacyPolicyVersion,
            setting?.UserAgreementVersion,
            setting?.PrivacyPolicyVersion);
    }

    public static string NormalizePrivacyConsent(string? privacyConsent)
    {
        var normalized = privacyConsent?.Trim();
        return normalized switch
        {
            Consented => Consented,
            Rejected => Rejected,
            Unknown => Unknown,
            _ => throw Oops.Oh("请先同意用户协议和隐私政策后继续").StatusCode(403)
        };
    }

    private static void ApplyConsent(SaveLifeSetting setting, string privacyConsent)
    {
        setting.PrivacyConsent = privacyConsent;
        if (privacyConsent == Consented)
            setting.PrivacyConsentTime = DateTime.Now;
    }

    private static void ApplyRegistrationConsent(
        SaveLifeSetting setting,
        string privacyConsent,
        string? userAgreementVersion,
        string? privacyPolicyVersion)
    {
        ApplyConsent(setting, privacyConsent);
        if (privacyConsent != Consented)
            return;

        setting.UserAgreementVersion = userAgreementVersion?.Trim();
        setting.PrivacyPolicyVersion = privacyPolicyVersion?.Trim();
    }

    private static void ApplySensitiveInformationConsent(
        SaveLifeSetting setting,
        string consent,
        string? version)
    {
        setting.SensitiveInformationConsent = consent;
        if (consent != Consented)
            return;

        setting.SensitiveInformationConsentVersion = version;
        setting.SensitiveInformationConsentTime = DateTime.Now;
    }

    private static string ResolveExternalUserId(string? externalUserId)
    {
        var userId = externalUserId?.Trim();
        if (!string.IsNullOrWhiteSpace(userId))
            return userId;

        userId = App.User?.FindFirst(ClaimConst.OpenId)?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(userId))
            return userId;

        throw Oops.Oh("小程序登录态无效，请重新登录").StatusCode(400);
    }
}
