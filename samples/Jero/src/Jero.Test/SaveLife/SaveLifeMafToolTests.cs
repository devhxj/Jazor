using AGUI.Abstractions;
using AGUI.Server;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeMafToolTests
{
    [Fact]
    public async Task HealthProfileProposalTool_UserMessage_PersistsOnlyWithCompletedAnswer()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: false),
            latestUserMessage: "我已经确诊肺腺癌");

        var result = await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.Diagnosis,
            "肺腺癌",
            SaveLifePatientProfileProposalSources.UserMessage,
            "确诊肺腺癌",
            sourceMaterialId: null,
            TestContext.Current.CancellationToken);

        Assert.Equal("pending", result.Status);
        var staged = Assert.IsType<SaveLifePatientProfileProposalOutput>(result.Proposal);
        Assert.Equal("肺腺癌", staged.ProposedValue);
        Assert.Empty(fixture.ProfileProposals());

        await fixture.CompleteTurnAsync("肺腺癌需要结合分期和病理结果进一步评估。", TestContext.Current.CancellationToken);

        var persisted = Assert.Single(fixture.ProfileProposals());
        Assert.Equal(staged.Id, persisted.Id.ToString());
        Assert.Equal(SaveLifePatientProfileProposalSources.UserMessage, persisted.SourceType);
        Assert.Null(persisted.SourceMaterialId);
        Assert.Equal("肺腺癌", persisted.ProposedValue);
        Assert.Equal("确诊肺腺癌", persisted.Evidence);
        using var metadata = JsonDocument.Parse(fixture.CompletedAssistantMessage().MetadataJson!);
        var metadataProposal = Assert.Single(metadata.RootElement
            .GetProperty("profileProposals")
            .EnumerateArray());
        Assert.Equal(staged.Id, metadataProposal.GetProperty("id").GetString());
    }

    [Fact]
    public async Task HealthProfileProposalTool_WhenTurnFails_DoesNotPersistStagedProposal()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: false),
            latestUserMessage: "我已经确诊肺腺癌");
        var result = await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.Diagnosis,
            "肺腺癌",
            SaveLifePatientProfileProposalSources.UserMessage,
            "确诊肺腺癌",
            sourceMaterialId: null,
            TestContext.Current.CancellationToken);

        await fixture.FailTurnAsync(new InvalidOperationException("model failed"));

        Assert.Equal("pending", result.Status);
        Assert.Empty(fixture.ProfileProposals());
    }

    [Fact]
    public async Task HealthProfileProposalTool_WhenAssistantCompletionFails_RollsBackProposal()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: false),
            latestUserMessage: "我已经确诊肺腺癌");
        await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.Diagnosis,
            "肺腺癌",
            SaveLifePatientProfileProposalSources.UserMessage,
            "确诊肺腺癌",
            sourceMaterialId: null,
            TestContext.Current.CancellationToken);
        fixture.FailAssistantCompletion();

        await Assert.ThrowsAnyAsync<Exception>(() => fixture.CompleteTurnAsync(
            "肺腺癌需要结合分期和病理结果进一步评估。",
            TestContext.Current.CancellationToken));

        Assert.Empty(fixture.ProfileProposals());
    }

    [Fact]
    public async Task HealthProfileProposalTool_WhenCalledTwice_KeepsOnlyFirstProposal()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: false),
            latestUserMessage: "我已经确诊肺腺癌，现在住在成都");
        var first = await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.Diagnosis,
            "肺腺癌",
            SaveLifePatientProfileProposalSources.UserMessage,
            "确诊肺腺癌",
            sourceMaterialId: null,
            TestContext.Current.CancellationToken);

        var second = await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.City,
            "成都",
            SaveLifePatientProfileProposalSources.UserMessage,
            "成都",
            sourceMaterialId: null,
            TestContext.Current.CancellationToken);
        await fixture.CompleteTurnAsync("我先帮你记住最重要的一条。", TestContext.Current.CancellationToken);

        Assert.Equal("pending", first.Status);
        Assert.Equal("already_proposed", second.Status);
        Assert.Equal(first.Proposal?.Id, second.Proposal?.Id);
        var persisted = Assert.Single(fixture.ProfileProposals());
        Assert.Equal(SaveLifePatientProfileProposalFields.Diagnosis, persisted.FieldName);
    }

    [Fact]
    public async Task HealthProfileProposalTool_WhenValueAlreadyConfirmed_ReturnsUnchanged()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我的诊断还是非小细胞肺癌");

        var result = await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.Diagnosis,
            "非小细胞肺癌",
            SaveLifePatientProfileProposalSources.UserMessage,
            "非小细胞肺癌",
            sourceMaterialId: null,
            TestContext.Current.CancellationToken);
        await fixture.CompleteTurnAsync("这条诊断已经记录过了。", TestContext.Current.CancellationToken);

        Assert.Equal("unchanged", result.Status);
        Assert.Null(result.Proposal);
        Assert.Empty(fixture.ProfileProposals());
    }

    [Fact]
    public async Task MedicalInformationTool_WithKnowledgeHit_ReturnsPublishedReference()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                Knowledge =
                [
                    new SaveLifeAgentKnowledgeReference
                    {
                        Id = "801",
                        Title = "腰椎间盘突出症科普",
                        DiseaseName = "腰椎间盘突出症",
                        Summary = "多数患者可先接受保守治疗。",
                        Score = 0.91
                    }
                ]
            });

        var result = await fixture.ToolService.SearchMedicalInformationAsync(
            "腰椎间盘突出怎么治疗",
            ["腰椎间盘突出"],
            TestContext.Current.CancellationToken);

        Assert.Equal("ok", result.Status);
        var item = Assert.Single(result.Items);
        Assert.Equal("801", item.Id);
        Assert.Equal("腰椎间盘突出症科普", item.Title);
        Assert.True(fixture.RetrievalService.LastScope?.IncludeKnowledge);
        Assert.False(fixture.RetrievalService.LastScope?.IncludeTrials);
    }

    [Fact]
    public async Task MedicalInformationTool_WhenRetrievalFails_ReturnsUnavailableInsteadOfThrowing()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new InvalidOperationException("knowledge backend unavailable"));

        var result = await fixture.ToolService.SearchMedicalInformationAsync(
            "腰痛怎么办",
            ["腰痛"],
            TestContext.Current.CancellationToken);

        Assert.Equal("unavailable", result.Status);
        Assert.Empty(result.Items);
        Assert.Equal("后台医疗资讯暂时不可用。", result.Message);
    }

    [Fact]
    public async Task MedicalInformationTool_WithNoHits_ReturnsNeutralBusinessState()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult());

        var result = await fixture.ToolService.SearchMedicalInformationAsync(
            "罕见疾病资料",
            ["罕见疾病"],
            TestContext.Current.CancellationToken);

        Assert.Equal("no_results", result.Status);
        Assert.Equal("后台医疗资讯暂未找到相关内容。", result.Message);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task MedicalInformationTool_WithUnrelatedPatient_SearchesOnlyTheQuestion()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true));

        await fixture.ToolService.SearchMedicalInformationAsync(
            "腰椎间盘突出怎么治疗",
            ["腰椎间盘突出"],
            TestContext.Current.CancellationToken);

        Assert.NotNull(fixture.RetrievalService.LastSignal);
        Assert.Null(fixture.RetrievalService.LastSignal.CurrentPatient);
        Assert.Null(fixture.RetrievalService.LastSignal.DiseaseKeyword);
        Assert.Equal(["腰椎间盘突出"], fixture.RetrievalService.LastSignal.KnowledgeSearchTerms);
        Assert.True(fixture.RetrievalService.LastSignal.UseSourceSearchTerms);
    }

    [Fact]
    public async Task ClinicalTrialTool_ExplicitPatientQuestion_UsesProvidedTermsWithoutMatchingMode()
    {
        var patient = CreatePatient(501, complete: true);
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery,
                Trials =
                [
                    new SaveLifeAgentTrialReference
                    {
                        Id = "901",
                        Title = "肺癌新药研究",
                        DiseaseArea = "肺癌",
                        ClinicalPhase = "III期",
                        Location = "成都"
                    }
                ]
            },
            patient);

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "帮我找适合当前患者的临床试验",
            ["非小细胞肺癌"],
            false,
            TestContext.Current.CancellationToken);

        Assert.Equal("ok", result.Status);
        Assert.Equal(SaveLifeAgentTrialResultLevels.Discovery, result.Mode);
        Assert.Equal("901", Assert.Single(result.Items).Id);
        Assert.Equal(SaveLifeAgentTrialRetrievalModes.Discovery, fixture.RetrievalService.LastScope?.TrialRetrievalMode);
        Assert.Null(fixture.RetrievalService.LastSignal?.CurrentPatient);
        Assert.Null(fixture.RetrievalService.LastSignal?.DiseaseKeyword);
        Assert.Equal(["非小细胞肺癌"], fixture.RetrievalService.LastSignal?.TrialSearchTerms);
        Assert.Equal("openid-1", fixture.RetrievalService.LastExternalUserId);
    }

    [Fact]
    public async Task ClinicalTrialTool_ReturnsProjectDescriptionAndParticipantBenefitsToTheAgent()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery,
                Trials =
                [
                    new SaveLifeAgentTrialReference
                    {
                        Id = "905",
                        Title = "项目内容问答研究",
                        Description = "项目采用每三周一次给药方案。",
                        Benefits = "受试者可获得规定的交通补贴。"
                    }
                ]
            });

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "这个项目的给药安排和受试者权益是什么？",
            ["给药安排", "交通补贴"],
            false,
            TestContext.Current.CancellationToken);

        var item = Assert.Single(result.Items);
        Assert.Equal("项目采用每三周一次给药方案。", item.Description);
        Assert.Equal("受试者可获得规定的交通补贴。", item.Benefits);
    }

    [Fact]
    public async Task ClinicalTrialTool_PublicQuery_UsesSearchTermsWithoutPatientContext()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery,
                Trials =
                [
                    new SaveLifeAgentTrialReference
                    {
                        Id = "902",
                        Title = "公开招募研究"
                    }
                ]
            });

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "有哪些肺癌临床试验",
            ["肺癌"],
            false,
            TestContext.Current.CancellationToken);

        Assert.Equal(SaveLifeAgentTrialRetrievalModes.Discovery, fixture.RetrievalService.LastScope?.TrialRetrievalMode);
        Assert.Null(fixture.RetrievalService.LastSignal?.CurrentPatient);
        Assert.Null(fixture.RetrievalService.LastSignal?.DiseaseKeyword);
        Assert.Equal(["肺癌"], fixture.RetrievalService.LastSignal?.TrialSearchTerms);
        Assert.True(fixture.RetrievalService.LastSignal?.UseSourceSearchTerms);
    }

    [Fact]
    public async Task ClinicalTrialTool_PublicVitiligoQuery_DoesNotUseUnrelatedCompletePatient()
    {
        var patient = CreatePatient(501, complete: true);
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery,
                Trials = [new SaveLifeAgentTrialReference { Id = "903", Title = "白癜风招募研究" }]
            },
            patient,
            "有没有白癜风实验项目",
            businessContext: new
            {
                contextId = "777",
                contextType = "trial",
                patientProfileId = patient.Id.ToString(),
                trialId = "777"
            });

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "有没有白癜风实验项目",
            ["白癜风"],
            false,
            TestContext.Current.CancellationToken);

        Assert.Equal("903", Assert.Single(result.Items).Id);
        Assert.Null(fixture.RetrievalService.LastSignal?.CurrentPatient);
        Assert.Null(fixture.RetrievalService.LastSignal?.DiseaseKeyword);
        Assert.Equal(["白癜风"], fixture.RetrievalService.LastSignal?.TrialSearchTerms);
        Assert.Empty(fixture.RetrievalService.LastScope?.RequestedTrialIds ?? []);
    }

    [Fact]
    public async Task ClinicalTrialTool_ExplicitPatientQuestion_DoesNotSwitchRetrievalMode()
    {
        var patient = CreatePatient(501, complete: false);
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery
            },
            patient,
            "我有肺癌，请查找相关临床试验");

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "我有肺癌，请查找相关临床试验",
            ["肺癌"],
            false,
            TestContext.Current.CancellationToken);

        Assert.Equal("no_results", result.Status);
        Assert.Equal(
            SaveLifeAgentTrialRetrievalModes.Discovery,
            fixture.RetrievalService.LastScope?.TrialRetrievalMode);
        Assert.Null(fixture.RetrievalService.LastSignal?.CurrentPatient);
        Assert.Equal(["肺癌"], fixture.RetrievalService.LastSignal?.TrialSearchTerms);
    }

    [Fact]
    public async Task ClinicalTrialTool_PersonalizedSearch_DoesNotUsePendingProposalForGate()
    {
        var patient = CreatePatient(501, complete: false);
        patient.DiseaseStage = "III期";
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            patient,
            "我确诊肺腺癌，帮我看看有没有适合我的项目");
        var proposal = await fixture.ToolService.ProposeHealthProfileAsync(
            SaveLifePatientProfileProposalFields.Diagnosis,
            "肺腺癌",
            SaveLifePatientProfileProposalSources.UserMessage,
            "肺腺癌",
            null,
            TestContext.Current.CancellationToken);

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "帮我看看有没有适合我的项目",
            ["肺腺癌"],
            true,
            TestContext.Current.CancellationToken);

        Assert.Equal("pending", proposal.Status);
        Assert.Equal("insufficient_profile", result.Status);
        Assert.Equal(SaveLifeAgentTrialResultLevels.InitialMatch, result.Mode);
        Assert.Equal(
            [
                SaveLifeAgentTrialMatchingFields.Diagnosis,
                SaveLifeAgentTrialMatchingFields.Treatment
            ],
            result.MissingFields);
        Assert.Contains("已经确认的疾病或诊断", result.Message, StringComparison.Ordinal);
        Assert.Contains("既往治疗或当前治疗", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("询问", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("最优先", result.Message, StringComparison.Ordinal);
        Assert.Empty(result.Items);
        Assert.Null(fixture.RetrievalService.LastScope);
    }

    [Fact]
    public async Task ClinicalTrialTool_PersonalizedSearch_IgnoresUnconfirmedModelTermsAndReturnsAtMostThreeInitialMatches()
    {
        var patient = CreatePatient(501, complete: true);
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Matching,
                TrialMatchingGate = new SaveLifeAgentTrialMatchingGate
                {
                    WasEvaluated = true,
                    CanMatch = true
                },
                Trials = Enumerable.Range(1, 4)
                    .Select(index => new SaveLifeAgentTrialReference
                    {
                        Id = (900 + index).ToString(),
                        Title = $"肺癌初筛项目 {index}"
                    })
                    .ToList()
            },
            patient);

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            "我刚怀疑自己是肝癌，帮我看看有没有适合我的项目",
            ["肝癌"],
            true,
            TestContext.Current.CancellationToken);

        Assert.Equal("ok", result.Status);
        Assert.Equal(SaveLifeAgentTrialResultLevels.InitialMatch, result.Mode);
        Assert.Equal(3, result.Items.Count);
        Assert.All(result.Items, item =>
            Assert.Equal(SaveLifeAgentTrialResultLevels.InitialMatch, item.ResultLevel));
        Assert.Equal(SaveLifeAgentTrialRetrievalModes.Matching, fixture.RetrievalService.LastScope?.TrialRetrievalMode);
        Assert.Equal(patient.Id.ToString(), fixture.RetrievalService.LastSignal?.CurrentPatient?.Id);
        Assert.Equal(patient.Diagnosis, fixture.RetrievalService.LastSignal?.CurrentPatient?.Diagnosis);
        Assert.Equal(string.Empty, fixture.RetrievalService.LastQuery);
        Assert.Empty(fixture.RetrievalService.LastSignal?.SearchTerms ?? []);
        Assert.Empty(fixture.RetrievalService.LastSignal?.TrialSearchTerms ?? []);

        await fixture.CompleteTurnAsync(
            "根据已确认资料，找到 3 个初步相关项目。",
            TestContext.Current.CancellationToken);
        using var metadata = JsonDocument.Parse(fixture.CompletedAssistantMessage().MetadataJson!);
        var persistedTrials = metadata.RootElement
            .GetProperty("matchedTrials")
            .EnumerateArray()
            .ToList();
        Assert.Equal(3, persistedTrials.Count);
        Assert.All(persistedTrials, item => Assert.Equal(
            SaveLifeAgentTrialResultLevels.InitialMatch,
            item.GetProperty("resultLevel").GetString()));
    }

    [Fact]
    public async Task ClinicalTrialTool_PersonalizedSearch_WithNoMatches_ReturnsNeutralBusinessState()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Matching
            },
            CreatePatient(501, complete: true));

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            string.Empty,
            [],
            true,
            TestContext.Current.CancellationToken);

        Assert.Equal("no_results", result.Status);
        Assert.Equal("根据当前已确认资料暂未找到初步相关的招募项目。", result.Message);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ClinicalTrialTool_WhenRetrievalFails_ReturnsNeutralUnavailableState()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new InvalidOperationException("trial backend unavailable"),
            CreatePatient(501, complete: true));

        var result = await fixture.ToolService.SearchClinicalTrialsAsync(
            string.Empty,
            [],
            true,
            TestContext.Current.CancellationToken);

        Assert.Equal("unavailable", result.Status);
        Assert.Equal("临床试验数据暂时不可用。", result.Message);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task RoutineGuidanceTool_ForNoDestination_ReturnsEmpty()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            latestUserMessage: "腰椎间盘突出怎么治疗");

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "none",
            TestContext.Current.CancellationToken);

        Assert.Equal("empty", result.Status);
        Assert.Null(result.GuidanceText);
        Assert.Empty(result.Actions);
    }

    [Fact]
    public async Task RoutineGuidanceTool_ForExplicitHealthProfileRequest_ReturnsNavigationOnly()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            latestUserMessage: "怎么完善健康档案");

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "health_profile",
            TestContext.Current.CancellationToken);

        Assert.Equal("guidance", result.Status);
        Assert.Contains("前往健康档案页", result.GuidanceText, StringComparison.Ordinal);
        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.CompleteHealthProfile, action.Type);
        Assert.Equal("health_profile", action.TargetType);
        Assert.Null(action.TargetId);
    }

    [Fact]
    public async Task RoutineGuidanceTool_TrialSearch_DoesNotCreateProjectGuidance()
    {
        var patient = CreatePatient(501, complete: false);
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult
            {
                TrialRetrievalMode = SaveLifeAgentTrialRetrievalModes.Discovery,
                Trials = [new SaveLifeAgentTrialReference { Id = "904", Title = "公开招募研究" }]
            },
            patient,
            "请按当前患者情况查找临床试验");
        await fixture.ToolService.SearchClinicalTrialsAsync(
            "请按当前患者情况查找临床试验",
            ["肺癌"],
            false,
            TestContext.Current.CancellationToken);

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "trial_detail",
            TestContext.Current.CancellationToken);

        Assert.Equal("empty", result.Status);
        Assert.Empty(result.Actions);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithIncompleteProfile_PrioritizesPersonalProfile()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            latestUserMessage: "我接下来怎么报名");

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.CompleteAccountProfile, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithCompleteProfileAndNoMaterial_PrioritizesMaterial()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么报名",
            seed: database => database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand());

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.UploadMaterial, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithProfileAndMaterial_OpensTrialList()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么报名",
            seed: database =>
            {
                database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand();
                database.Insertable(CreatePersonalMaterial(801, 501)).ExecuteCommand();
            });

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.StartScreening, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithExistingEnrollment_OpensEnrollmentList()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么办",
            seed: database =>
            {
                database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand();
                database.Insertable(CreatePersonalMaterial(801, 501)).ExecuteCommand();
                database.Insertable(CreateEnrollment(901, 501)).ExecuteCommand();
            });

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.ViewEnrollments, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithIncompleteProfileAndExistingEnrollment_PrioritizesPersonalProfile()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么办",
            seed: database => database.Insertable(CreateEnrollment(901, 501)).ExecuteCommand());

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.CompleteAccountProfile, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithCompleteProfileAndExistingEnrollmentButNoMaterial_PrioritizesMaterial()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么办",
            seed: database =>
            {
                database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand();
                database.Insertable(CreateEnrollment(901, 501)).ExecuteCommand();
            });

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.UploadMaterial, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WithVerifiedEnrollmentContext_OpensCurrentEnrollment()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "这个报名接下来怎么办",
            businessContext: new
            {
                contextId = "901",
                contextType = "enrollment",
                enrollmentId = "901"
            },
            seed: database => database.Insertable(CreateEnrollment(901, 501)).ExecuteCommand());

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.ViewEnrollment, action.Type);
        Assert.Equal("901", action.TargetId);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_IgnoresMaterialsOutsideCurrentAccountAndSelfProfile()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么报名",
            seed: database =>
            {
                database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand();
                database.Insertable(CreatePersonalMaterial(801, 501, "openid-2")).ExecuteCommand();
                database.Insertable(CreatePersonalMaterial(802, 999)).ExecuteCommand();
            });

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.UploadMaterial, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_IgnoresDeletedEnrollment()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么报名",
            seed: database =>
            {
                database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand();
                database.Insertable(CreatePersonalMaterial(801, 501)).ExecuteCommand();
                var enrollment = CreateEnrollment(901, 501);
                enrollment.IsDelete = true;
                database.Insertable(enrollment).ExecuteCommand();
                var foreignEnrollment = CreateEnrollment(902, 501);
                foreignEnrollment.ExternalUserId = "openid-2";
                database.Insertable(foreignEnrollment).ExecuteCommand();
            });

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.StartScreening, action.Type);
    }

    [Fact]
    public async Task RoutineGuidanceTool_NextStep_WhenStateLookupFails_ReturnsNoGuidance()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            CreatePatient(501, complete: true),
            latestUserMessage: "我接下来怎么报名",
            seed: database =>
            {
                database.Insertable(CreateUserProfile(complete: true)).ExecuteCommand();
                database.Insertable(CreatePersonalMaterial(801, 501)).ExecuteCommand();
            });
        fixture.DropEnrollmentTable();

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "next_step",
            TestContext.Current.CancellationToken);

        Assert.Equal("empty", result.Status);
        Assert.Empty(result.Actions);
    }

    [Fact]
    public async Task RoutineGuidanceTool_ExplicitTrialList_DoesNotRunNextStepPrerequisites()
    {
        await using var fixture = await ToolFixture.CreateAsync(
            new SaveLifeAgentRetrievalResult(),
            latestUserMessage: "打开可报名项目列表");

        var result = await fixture.ToolService.BuildRoutineGuidanceAsync(
            "trial_list",
            TestContext.Current.CancellationToken);

        var action = Assert.Single(result.Actions);
        Assert.Equal(SaveLifeAgentSuggestedActionTypes.StartScreening, action.Type);
    }

    private static SaveLifePatientProfile CreatePatient(long id, bool complete)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = "openid-1",
            AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-1"),
            RelationshipToAccount = SaveLifePatientRelationships.Self,
            RelationshipDescription = "本人",
            DisplayName = "张三",
            Diagnosis = complete ? "非小细胞肺癌" : null,
            DiseaseStage = complete ? "III期" : null,
            TreatmentHistory = complete ? "已完成一线治疗" : null,
            IsDefault = true,
            CreateTime = new DateTime(2026, 7, 18)
        };
    }

    private static SaveLifeUserProfile CreateUserProfile(bool complete)
    {
        return new SaveLifeUserProfile
        {
            Id = 601,
            ExternalUserId = "openid-1",
            RealName = "张三",
            Gender = complete ? "male" : null,
            BirthDate = complete ? new DateTime(1980, 1, 1) : null,
            Phone = complete ? "13800000000" : null,
            Address = complete ? "四川省/成都市/武侯区 人民南路" : null,
            MedicalHistory = complete ? "肺癌" : null,
            Allergies = complete ? "无" : null,
            EmergencyContact = complete ? "李四" : null,
            EmergencyPhone = complete ? "13900000000" : null,
            CreateTime = new DateTime(2026, 7, 18)
        };
    }

    private static SaveLifeEnrollmentMaterial CreatePersonalMaterial(
        long id,
        long patientProfileId,
        string externalUserId = "openid-1")
    {
        return new SaveLifeEnrollmentMaterial
        {
            Id = id,
            ExternalUserId = externalUserId,
            PatientProfileId = patientProfileId,
            EnrollmentRecordId = 0,
            FileId = 1001,
            FileName = "病例.pdf",
            MaterialType = "medical_record",
            Status = "submitted",
            ExtractStatus = "pending",
            IndexStatus = "pending",
            CreateTime = new DateTime(2026, 7, 18)
        };
    }

    private static SaveLifeEnrollmentRecord CreateEnrollment(long id, long patientProfileId)
    {
        return new SaveLifeEnrollmentRecord
        {
            Id = id,
            ExternalUserId = "openid-1",
            PatientProfileId = patientProfileId,
            ProjectId = 777,
            ProjectTitle = "肺癌招募项目",
            Status = "pending",
            CreateTime = new DateTime(2026, 7, 18)
        };
    }

    private sealed class ToolFixture : IAsyncDisposable
    {
        private readonly string _databasePath;
        private readonly SqlSugarClient _database;

        private ToolFixture(
            string databasePath,
            SqlSugarClient database,
            StubRetrievalService retrievalService,
            SaveLifeMafRequestContext requestContext,
            SaveLifeMafToolService toolService)
        {
            _databasePath = databasePath;
            _database = database;
            RetrievalService = retrievalService;
            RequestContext = requestContext;
            ToolService = toolService;
        }

        public StubRetrievalService RetrievalService { get; }

        public SaveLifeMafRequestContext RequestContext { get; }

        public SaveLifeMafToolService ToolService { get; }

        public List<SaveLifePatientProfileProposal> ProfileProposals() =>
            _database.Queryable<SaveLifePatientProfileProposal>().ClearFilter().ToList();

        public SaveLifeAgentMessage CompletedAssistantMessage() =>
            _database.Queryable<SaveLifeAgentMessage>().ClearFilter().Single(message =>
                message.Role == "assistant" &&
                message.GenerationStatus == "completed");

        public Task CompleteTurnAsync(string content, CancellationToken cancellationToken) =>
            RequestContext.CompleteTurnAsync(content, cancellationToken);

        public Task FailTurnAsync(Exception exception) =>
            RequestContext.FailTurnAsync(exception, CancellationToken.None);

        public void DropEnrollmentTable()
        {
            _database.DbMaintenance.DropTable<SaveLifeEnrollmentRecord>();
        }

        public void FailAssistantCompletion()
        {
            _database.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_assistant_completion
                BEFORE UPDATE ON sl_agent_message
                WHEN OLD.Role = 'assistant' AND NEW.GenerationStatus = 'completed'
                BEGIN
                    SELECT RAISE(ABORT, 'forced assistant completion failure');
                END;
                """);
        }

        public static Task<ToolFixture> CreateAsync(
            SaveLifeAgentRetrievalResult result,
            SaveLifePatientProfile? patient = null,
            string latestUserMessage = "测试问题",
            object? businessContext = null,
            Action<SqlSugarClient>? seed = null)
        {
            return CreateAsync(
                new StubRetrievalService(result),
                patient,
                latestUserMessage,
                businessContext,
                seed);
        }

        public static Task<ToolFixture> CreateAsync(
            Exception exception,
            SaveLifePatientProfile? patient = null,
            string latestUserMessage = "测试问题",
            object? businessContext = null,
            Action<SqlSugarClient>? seed = null)
        {
            return CreateAsync(
                new StubRetrievalService(exception),
                patient,
                latestUserMessage,
                businessContext,
                seed);
        }

        private static async Task<ToolFixture> CreateAsync(
            StubRetrievalService retrievalService,
            SaveLifePatientProfile? patient,
            string latestUserMessage,
            object? businessContext,
            Action<SqlSugarClient>? seed)
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            var database = CreateDatabase(databasePath);
            database.CodeFirst.InitTables(
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage),
                typeof(SaveLifeUserProfile),
                typeof(SaveLifePatientProfile),
                typeof(SaveLifePatientProfileProposal),
                typeof(SaveLifeClinicalTrial),
                typeof(SaveLifeDiseaseKnowledge),
                typeof(SaveLifeEnrollmentRecord),
                typeof(SaveLifeEnrollmentMaterial));
            if (patient != null)
                database.Insertable(patient).ExecuteCommand();
            seed?.Invoke(database);
            if (businessContext != null)
            {
                database.Insertable(new SaveLifeClinicalTrial
                {
                    Id = 777,
                    Title = "肺癌页面项目",
                    CdeRegistrationNumber = "CTR777",
                    DiseaseArea = "肺癌",
                    Indication = "非小细胞肺癌",
                    Status = SaveLifeAgentTrialSelection.RecruitingStatus,
                    CreateTime = new DateTime(2026, 7, 18)
                }).ExecuteCommand();
            }
            database.Insertable(new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = patient?.Id,
                PatientContextVersion = 1,
                NextTurnSequence = 1,
                StateRevision = 0,
                Status = "active",
                Phase = "created",
                CreateTime = new DateTime(2026, 7, 18)
            }).ExecuteCommand();

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimConst.OpenId, "openid-1")],
                    "test"))
            };
            var requestContext = new SaveLifeMafRequestContext(
                new HttpContextAccessor { HttpContext = httpContext },
                SqlSugarRepository<SaveLifeAgentConversation>.CreateForContext(database),
                SqlSugarRepository<SaveLifeAgentMessage>.CreateForContext(database),
                SqlSugarRepository<SaveLifeUserProfile>.CreateForContext(database),
                SqlSugarRepository<SaveLifePatientProfile>.CreateForContext(database),
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(database),
                SqlSugarRepository<SaveLifeDiseaseKnowledge>.CreateForContext(database),
                SqlSugarRepository<SaveLifeEnrollmentRecord>.CreateForContext(database),
                new StubPrivacyConsentGuard(),
                retrievalService,
                new StubMaterialModelInputService(),
                new StubClientFactory(),
                NullLogger<SaveLifeMafRequestContext>.Instance);
            long nextId = 1000;
            requestContext.NextId = () => Interlocked.Increment(ref nextId);
            await requestContext.BeginTurnAsync(
                [new ChatMessage(ChatRole.User, latestUserMessage) { MessageId = "client-1" }],
                CreateAgUiChatOptions(businessContext),
                TestContext.Current.CancellationToken);
            if (businessContext != null)
            {
                await requestContext.BuildAgentMessagesAsync(
                    [],
                    session: null,
                    TestContext.Current.CancellationToken);
            }
            var toolService = new SaveLifeMafToolService(
                requestContext,
                retrievalService,
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(database),
                SqlSugarRepository<SaveLifeEnrollmentRecord>.CreateForContext(database),
                NullLogger<SaveLifeMafToolService>.Instance);
            return new ToolFixture(
                databasePath,
                database,
                retrievalService,
                requestContext,
                toolService);
        }

        public ValueTask DisposeAsync()
        {
            _database.Dispose();
            if (File.Exists(_databasePath))
                File.Delete(_databasePath);
            return ValueTask.CompletedTask;
        }

        private static ChatOptions CreateAgUiChatOptions(object? businessContext)
        {
            var input = new RunAgentInput
            {
                ThreadId = "701",
                RunId = $"run-{Guid.NewGuid():N}"
            };
            if (businessContext != null)
            {
                input.Context =
                [
                    new AGUIContext
                    {
                        Description = "SaveLife business context",
                        Value = JsonSerializer.Serialize(businessContext),
                    },
                ];
            }
            return input.ToChatRequestContext(JsonSerializerOptions.Web).ChatOptions;
        }

        private static SqlSugarClient CreateDatabase(string path)
        {
            return new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={path};Pooling=False",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    EntityService = (property, column) =>
                    {
                        if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                            column.IsNullable = true;
                    }
                }
            });
        }
    }

    private sealed class StubRetrievalService : ISaveLifeAgentRetrievalService
    {
        private readonly Exception? _exception;
        private readonly SaveLifeAgentRetrievalResult? _result;

        public StubRetrievalService(SaveLifeAgentRetrievalResult result)
        {
            _result = result;
        }

        public StubRetrievalService(Exception exception)
        {
            _exception = exception;
        }

        public string? LastExternalUserId { get; private set; }

        public SaveLifeAgentRetrievalScope? LastScope { get; private set; }

        public SaveLifeAgentPatientSignal? LastSignal { get; private set; }

        public string? LastQuery { get; private set; }

        public Task<SaveLifeAgentRetrievalResult> BuildRetrievalAsync(
            string query,
            SaveLifeAgentPatientSignal signal,
            string? externalUserId = null,
            long? enrollmentRecordId = null,
            CancellationToken cancellationToken = default,
            SaveLifeAgentRetrievalScope? scope = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastQuery = query;
            LastExternalUserId = externalUserId;
            LastScope = scope;
            LastSignal = signal;
            return _exception != null
                ? Task.FromException<SaveLifeAgentRetrievalResult>(_exception)
                : Task.FromResult(_result!);
        }

        public Task SyncKnowledgeIndexAsync(SaveLifeDiseaseKnowledge knowledge) =>
            throw new NotSupportedException();

        public Task SyncTrialIndexAsync(SaveLifeClinicalTrial trial) =>
            throw new NotSupportedException();

        public Task SyncMaterialIndexAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>>? openFileStream = null) => throw new NotSupportedException();

        public Task DeleteMaterialIndexAsync(long materialId, string externalUserId) =>
            throw new NotSupportedException();
    }

    private sealed class StubPrivacyConsentGuard : ISaveLifePrivacyConsentGuard
    {
        public Task EnsureConsentedAsync(string? externalUserId = null) => Task.CompletedTask;

        public Task RecordConsentAsync(string externalUserId, string? privacyConsent) =>
            throw new NotSupportedException();

        public Task<SaveLifeMiniAppConsentDecision> EvaluateMiniAppUpsertAsync(
            string externalUserId,
            string? privacyConsent) => throw new NotSupportedException();
    }

    private sealed class StubMaterialModelInputService : ISaveLifeAgentMaterialModelInputService
    {
        public Task<SaveLifeAgentPreparedMaterialInput> BuildAsync(
            SaveLifeAgentMaterialAccessScope scope,
            IReadOnlyList<SaveLifeAgentMaterialReference> references,
            SaveLifeAgentOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SaveLifeAgentPreparedMaterialInput.Empty);
        }
    }

    private sealed class StubClientFactory : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync() => Task.FromResult(new SaveLifeAgentOptions());

        public Task<bool> CanUseChatAsync() => Task.FromResult(false);

        public Task<bool> CanUseEmbeddingsAsync() => Task.FromResult(false);

        public Task<OpenAI.Chat.ChatClient> CreateChatClientAsync() => throw new NotSupportedException();

        public Task<OpenAI.Embeddings.EmbeddingClient> CreateEmbeddingClientAsync() =>
            throw new NotSupportedException();
    }
}
