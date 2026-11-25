namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMiniAppConsentPolicy
{
    public const string ConsentRequiredMessage = "请先同意用户协议和隐私政策后继续";

    public const string ConsentVersionRequiredMessage = "用户协议或隐私政策已更新，请重新阅读并同意";

    public static SaveLifeMiniAppConsentDecision EvaluateUpsert(string? requestedConsent, string? storedConsent)
    {
        return EvaluateUpsert(
            requestedConsent,
            storedConsent,
            requestedConsent == SaveLifePrivacyConsentGuard.Consented ? SaveLifeLegalDocumentVersions.UserAgreement : null,
            requestedConsent == SaveLifePrivacyConsentGuard.Consented ? SaveLifeLegalDocumentVersions.PrivacyPolicy : null,
            storedConsent == SaveLifePrivacyConsentGuard.Consented ? SaveLifeLegalDocumentVersions.UserAgreement : null,
            storedConsent == SaveLifePrivacyConsentGuard.Consented ? SaveLifeLegalDocumentVersions.PrivacyPolicy : null);
    }

    public static SaveLifeMiniAppConsentDecision EvaluateUpsert(
        string? requestedConsent,
        string? storedConsent,
        string? requestedUserAgreementVersion,
        string? requestedPrivacyPolicyVersion,
        string? storedUserAgreementVersion,
        string? storedPrivacyPolicyVersion)
    {
        var requested = NormalizeOptionalConsent(requestedConsent);
        var stored = NormalizeOptionalConsent(storedConsent);

        if (requested == SaveLifePrivacyConsentGuard.Consented)
        {
            if (!HasCurrentRegistrationVersions(requestedUserAgreementVersion, requestedPrivacyPolicyVersion))
            {
                return new SaveLifeMiniAppConsentDecision(
                    false,
                    false,
                    null,
                    ConsentVersionRequiredMessage);
            }

            return new SaveLifeMiniAppConsentDecision(
                true,
                true,
                SaveLifePrivacyConsentGuard.Consented,
                null);
        }

        if (requested is SaveLifePrivacyConsentGuard.Rejected or SaveLifePrivacyConsentGuard.Unknown)
        {
            return new SaveLifeMiniAppConsentDecision(
                false,
                true,
                requested,
                ConsentRequiredMessage);
        }

        if (!string.IsNullOrWhiteSpace(requestedConsent))
        {
            return new SaveLifeMiniAppConsentDecision(
                false,
                false,
                null,
                ConsentRequiredMessage);
        }

        if (stored == SaveLifePrivacyConsentGuard.Consented &&
            HasCurrentRegistrationVersions(storedUserAgreementVersion, storedPrivacyPolicyVersion))
        {
            return new SaveLifeMiniAppConsentDecision(
                true,
                false,
                null,
                null);
        }

        var errorMessage = stored == SaveLifePrivacyConsentGuard.Consented
            ? ConsentVersionRequiredMessage
            : ConsentRequiredMessage;
        return new SaveLifeMiniAppConsentDecision(
            false,
            false,
            null,
            errorMessage);
    }

    public static bool HasCurrentRegistrationVersions(
        string? userAgreementVersion,
        string? privacyPolicyVersion)
    {
        return string.Equals(
                userAgreementVersion?.Trim(),
                SaveLifeLegalDocumentVersions.UserAgreement,
                StringComparison.Ordinal) &&
            string.Equals(
                privacyPolicyVersion?.Trim(),
                SaveLifeLegalDocumentVersions.PrivacyPolicy,
                StringComparison.Ordinal);
    }

    private static string? NormalizeOptionalConsent(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}

public sealed record SaveLifeMiniAppConsentDecision(
    bool CanProceed,
    bool ShouldRecordConsent,
    string? ConsentToRecord,
    string? ErrorMessage);
