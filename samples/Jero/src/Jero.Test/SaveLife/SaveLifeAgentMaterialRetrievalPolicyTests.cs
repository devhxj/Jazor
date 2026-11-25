using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentMaterialRetrievalPolicyTests
{
    [Fact]
    public void Build_WithoutEnrollmentAndNoMaterialIntent_DoesNotSearchPrivateMaterials()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "肺癌怎么治疗",
            new SaveLifeAgentPatientSignal { DiseaseKeyword = "肺癌" });

        var plan = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: null,
            patientProfileId: 501);

        Assert.False(plan.CanSearch);
        Assert.False(plan.HasMaterialIntent);
        Assert.Empty(plan.SearchTerms);
    }

    [Theory]
    [InlineData("我上传的报告够不够")]
    [InlineData("病历和CT还需要补什么资料")]
    [InlineData("诊断报告能不能用于报名")]
    public void Build_WithMaterialIntent_SearchesPrivateMaterials(string message)
    {
        var query = SaveLifeAgentRetrievalQuery.Build(message, new SaveLifeAgentPatientSignal
        {
            SearchTerms = ["报告"]
        });

        var plan = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: null,
            patientProfileId: 501,
            hasMaterialIntent: true);

        Assert.True(plan.CanSearch);
        Assert.True(plan.HasMaterialIntent);
        Assert.NotEmpty(plan.SearchTerms);
    }

    [Fact]
    public void Build_WithAiMaterialTerms_UsesOnlyStructuredTerms()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "我上传的资料够吗",
            new SaveLifeAgentPatientSignal { SearchTerms = ["资料"] });

        var plan = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: null,
            patientProfileId: 501,
            hasMaterialIntent: true);

        Assert.Contains("资料", plan.SearchTerms);
        Assert.Single(plan.SearchTerms);
    }

    [Fact]
    public void Build_WithEnrollmentContext_SearchesCurrentEnrollmentWithoutMaterialIntent()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "这个项目接下来怎么办",
            new SaveLifeAgentPatientSignal());

        var plan = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: 3001,
            patientProfileId: 501);

        Assert.True(plan.CanSearch);
        Assert.False(plan.HasMaterialIntent);
        Assert.Equal(3001, plan.EnrollmentRecordId);
    }

    [Fact]
    public void ScoreCandidate_WithEnrollmentContext_PrioritizesCurrentEnrollmentMaterial()
    {
        var query = SaveLifeAgentRetrievalQuery.Build(
            "我上传的资料够吗",
            new SaveLifeAgentPatientSignal());
        var plan = SaveLifeAgentMaterialRetrievalPolicy.Build(
            query,
            "openid-1",
            enrollmentRecordId: 3001,
            patientProfileId: 501);
        var currentEnrollmentMaterial = new SaveLifeEnrollmentMaterial
        {
            Id = 1001,
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 3001,
            MaterialType = "medical_record",
            Status = "submitted"
        };
        var otherEnrollmentMaterial = new SaveLifeEnrollmentMaterial
        {
            Id = 1002,
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 3002,
            MaterialType = "medical_record",
            Status = "submitted"
        };

        var currentScore = SaveLifeAgentMaterialRetrievalPolicy.ScoreCandidate(
            new SaveLifeAgentVectorIndex { ResourceId = 1001, Content = "其他资料" },
            currentEnrollmentMaterial,
            plan,
            0.1);
        var otherScore = SaveLifeAgentMaterialRetrievalPolicy.ScoreCandidate(
            new SaveLifeAgentVectorIndex { ResourceId = 1002, Content = "病历资料" },
            otherEnrollmentMaterial,
            plan,
            0.9);

        Assert.True(currentScore > otherScore);
    }

    [Fact]
    public void IsEligibleForRetrieval_RejectsDeletedOtherUserAndUnknownStatusMaterials()
    {
        Assert.True(SaveLifeAgentMaterialRetrievalPolicy.IsEligibleForRetrieval(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            Status = "submitted",
            ExtractStatus = "synced",
            IndexStatus = "synced"
        }, "openid-1"));

        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsEligibleForRetrieval(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            Status = "submitted",
            IsDelete = true
        }, "openid-1"));

        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsEligibleForRetrieval(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-2",
            Status = "submitted"
        }, "openid-1"));

        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsEligibleForRetrieval(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            Status = "archived"
        }, "openid-1"));
    }

    [Fact]
    public void IsInScope_WithEnrollmentContext_IncludesPersonalAndCurrentEnrollmentOnly()
    {
        var plan = new SaveLifeAgentMaterialRetrievalPlan
        {
            CanSearch = true,
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 3001
        };

        Assert.True(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 0
        }, plan));
        Assert.True(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 3001
        }, plan));
        Assert.False(SaveLifeAgentMaterialRetrievalPolicy.IsInScope(new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            EnrollmentRecordId = 3002
        }, plan));
    }

    [Theory]
    [InlineData("submitted", true, false)]
    [InlineData("approved", true, false)]
    [InlineData("need_supplement", false, true)]
    [InlineData("rejected", false, true)]
    public void MaterialStatusPolicy_DistinguishesUsableAndSupplementMaterials(
        string status,
        bool canSatisfyRequiredMaterial,
        bool requiresSupplement)
    {
        var material = new SaveLifeEnrollmentMaterial
        {
            ExternalUserId = "openid-1",
            Status = status
        };

        Assert.Equal(
            canSatisfyRequiredMaterial,
            SaveLifeAgentMaterialRetrievalPolicy.CanSatisfyRequiredMaterial(material));
        Assert.Equal(
            requiresSupplement,
            SaveLifeAgentMaterialRetrievalPolicy.RequiresSupplement(material));
    }
}
