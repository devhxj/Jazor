using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMiniAppConsentPolicyTests
{
    [Fact]
    public void EvaluateUpsert_WithExplicitConsent_AllowsAndRecordsConsent()
    {
        var decision = SaveLifeMiniAppConsentPolicy.EvaluateUpsert(
            "consented",
            null,
            SaveLifeLegalDocumentVersions.UserAgreement,
            SaveLifeLegalDocumentVersions.PrivacyPolicy,
            null,
            null);

        Assert.True(decision.CanProceed);
        Assert.True(decision.ShouldRecordConsent);
        Assert.Equal("consented", decision.ConsentToRecord);
    }

    [Fact]
    public void EvaluateUpsert_WithStoredConsentAndNoRequestedConsent_AllowsSilentRefresh()
    {
        var decision = SaveLifeMiniAppConsentPolicy.EvaluateUpsert(
            null,
            "consented",
            null,
            null,
            SaveLifeLegalDocumentVersions.UserAgreement,
            SaveLifeLegalDocumentVersions.PrivacyPolicy);

        Assert.True(decision.CanProceed);
        Assert.False(decision.ShouldRecordConsent);
        Assert.Null(decision.ErrorMessage);
    }

    [Theory]
    [InlineData(null, "V1.0")]
    [InlineData("V1.0", null)]
    [InlineData("V0.9", "V1.0")]
    [InlineData("V1.0", "V0.9")]
    public void EvaluateUpsert_WithMissingOrOutdatedStoredVersion_RequiresFreshConsent(
        string? userAgreementVersion,
        string? privacyPolicyVersion)
    {
        var decision = SaveLifeMiniAppConsentPolicy.EvaluateUpsert(
            null,
            "consented",
            null,
            null,
            userAgreementVersion,
            privacyPolicyVersion);

        Assert.False(decision.CanProceed);
        Assert.False(decision.ShouldRecordConsent);
        Assert.Equal(SaveLifeMiniAppConsentPolicy.ConsentVersionRequiredMessage, decision.ErrorMessage);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "unknown")]
    [InlineData("rejected", "consented")]
    [InlineData("unknown", "consented")]
    public void EvaluateUpsert_WithoutEffectiveConsent_BlocksSensitiveIdentityCreation(string? requested, string? stored)
    {
        var decision = SaveLifeMiniAppConsentPolicy.EvaluateUpsert(requested, stored);

        Assert.False(decision.CanProceed);
        Assert.Equal(SaveLifeMiniAppConsentPolicy.ConsentRequiredMessage, decision.ErrorMessage);
    }
}
