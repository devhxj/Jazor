using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePatientProfileProposalTests
{
    [Theory]
    [InlineData(" user_message ", SaveLifePatientProfileProposalSources.UserMessage)]
    [InlineData("material", SaveLifePatientProfileProposalSources.Material)]
    public void SourcePolicy_NormalizesAllowedSourceTypes(string input, string expected)
    {
        Assert.Equal(expected, SaveLifePatientProfileProposalPolicy.NormalizeSourceType(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("history")]
    [InlineData("other_account_material")]
    public void SourcePolicy_RejectsUnknownSourceTypes(string input)
    {
        Assert.Throws<SaveLifeAgentProfileProposalValidationException>(() =>
            SaveLifePatientProfileProposalPolicy.NormalizeSourceType(input));
    }

    [Fact]
    public void SourcePolicy_UserMessage_RequiresExactCurrentMessageEvidenceWithoutMaterial()
    {
        var source = SaveLifePatientProfileProposalPolicy.NormalizeSource(
            " user_message ",
            sourceMaterialId: null,
            evidence: "EGFR突变",
            currentUserMessage: "报告写的是EGFR突变",
            authorizedMaterials: []);

        Assert.Equal(SaveLifePatientProfileProposalSources.UserMessage, source.SourceType);
        Assert.Null(source.SourceMaterialId);
        Assert.Equal("EGFR突变", source.Evidence);
        Assert.Throws<SaveLifeAgentProfileProposalValidationException>(() =>
            SaveLifePatientProfileProposalPolicy.NormalizeSource(
                SaveLifePatientProfileProposalSources.UserMessage,
                sourceMaterialId: null,
                evidence: "egfr突变",
                currentUserMessage: "报告写的是EGFR突变",
                authorizedMaterials: []));
        Assert.Throws<SaveLifeAgentProfileProposalValidationException>(() =>
            SaveLifePatientProfileProposalPolicy.NormalizeSource(
                SaveLifePatientProfileProposalSources.UserMessage,
                sourceMaterialId: 801,
                evidence: "EGFR突变",
                currentUserMessage: "报告写的是EGFR突变",
                authorizedMaterials: []));
    }

    [Fact]
    public void SourcePolicy_DocumentMaterial_RequiresAuthorizedMaterialAndExtractedTextEvidence()
    {
        var materials = new[]
        {
            new SaveLifeAgentPreparedMaterialEvidence(
                801,
                SaveLifeAgentModelContentTypes.Text,
                "病理诊断：肺腺癌。EGFR 19号外显子缺失突变。")
        };

        var source = SaveLifePatientProfileProposalPolicy.NormalizeSource(
            SaveLifePatientProfileProposalSources.Material,
            sourceMaterialId: 801,
            evidence: "肺腺癌",
            currentUserMessage: "请帮我看看报告",
            authorizedMaterials: materials);

        Assert.Equal(SaveLifePatientProfileProposalSources.Material, source.SourceType);
        Assert.Equal(801, source.SourceMaterialId);
        Assert.Throws<SaveLifeAgentProfileProposalValidationException>(() =>
            SaveLifePatientProfileProposalPolicy.NormalizeSource(
                SaveLifePatientProfileProposalSources.Material,
                sourceMaterialId: 802,
                evidence: "肺腺癌",
                currentUserMessage: "请帮我看看报告",
                authorizedMaterials: materials));
        Assert.Throws<SaveLifeAgentProfileProposalValidationException>(() =>
            SaveLifePatientProfileProposalPolicy.NormalizeSource(
                SaveLifePatientProfileProposalSources.Material,
                sourceMaterialId: 801,
                evidence: "ALK融合",
                currentUserMessage: "请帮我看看报告",
                authorizedMaterials: materials));
    }

    [Fact]
    public void SourcePolicy_ImageMaterial_AllowsModelDerivedEvidenceButRejectsVideo()
    {
        var imageSource = SaveLifePatientProfileProposalPolicy.NormalizeSource(
            SaveLifePatientProfileProposalSources.Material,
            sourceMaterialId: 801,
            evidence: "病理诊断为肺腺癌",
            currentUserMessage: "请帮我看看图片",
            authorizedMaterials:
            [
                new SaveLifeAgentPreparedMaterialEvidence(
                    801,
                    SaveLifeAgentModelContentTypes.Image,
                    ExtractedText: null)
            ]);

        Assert.Equal(801, imageSource.SourceMaterialId);
        Assert.Throws<SaveLifeAgentProfileProposalValidationException>(() =>
            SaveLifePatientProfileProposalPolicy.NormalizeSource(
                SaveLifePatientProfileProposalSources.Material,
                sourceMaterialId: 802,
                evidence: "视频中提到肺腺癌",
                currentUserMessage: "请帮我看看视频",
                authorizedMaterials:
                [
                    new SaveLifeAgentPreparedMaterialEvidence(
                        802,
                        SaveLifeAgentModelContentTypes.Video,
                        ExtractedText: null)
                ]));
    }

    [Fact]
    public async Task CreatePending_WithNewCorrection_SupersedesOlderProposalWithoutUpdatingPatient()
    {
        await WithDatabase(async db =>
        {
            var now = new DateTime(2026, 7, 14, 10, 0, 0);
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var userMessage = CreateUserMessage(801, conversation, patient.Id, "我现在住在成都");
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(userMessage).ExecuteCommand();
            db.Insertable(new SaveLifePatientProfileProposal
            {
                Id = 901,
                OwnerExternalUserId = "openid-1",
                ConversationId = conversation.Id,
                PatientProfileId = patient.Id,
                SourceUserMessageId = 800,
                SourceAssistantMessageId = 850,
                FieldName = SaveLifePatientProfileProposalFields.City,
                OldValue = null,
                ProposedValue = "重庆",
                Evidence = "重庆",
                Status = SaveLifePatientProfileProposalStatuses.Pending,
                ExpiresTime = now.AddDays(3),
                CreateTime = now.AddHours(-1)
            }).ExecuteCommand();

            var created = await SaveLifePatientProfileProposalPersistence.CreatePendingAsync(
                db,
                conversation,
                userMessage,
                assistantMessageId: 802,
                patient,
                [new SaveLifeAgentProfileUpdateProposalOutput
                {
                    Field = SaveLifePatientProfileProposalFields.City,
                    Value = "成都",
                    Evidence = "成都"
                }],
                now,
                () => 902);

            var proposals = db.Queryable<SaveLifePatientProfileProposal>()
                .OrderBy(u => u.Id)
                .ToList();
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Superseded, proposals[0].Status);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending, proposals[1].Status);
            Assert.Equal("成都", proposals[1].ProposedValue);
            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().City);
            Assert.Equal("902", Assert.Single(created).Id);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_WithOwnedCurrentPatient_UpdatesProfileAndLegacyAuditIdempotently()
    {
        await WithDatabase(async db =>
        {
            var now = DateTime.Now;
            var patient = CreatePatient(501, "openid-1", "本人");
            patient.RelationshipToAccount = SaveLifePatientRelationships.Self;
            patient.SourceUserProfileId = 401;
            patient.MedicalHistory = "旧病史";
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var legacy = new SaveLifeUserProfile
            {
                Id = 401,
                ExternalUserId = "openid-1",
                RealName = "本人",
                MedicalHistory = "旧病史",
                CreateTime = now.AddDays(-30)
            };
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.MedicalHistory,
                "旧病史",
                "肺癌术后",
                "肺癌术后",
                now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(legacy).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");
            var input = new ConfirmSaveLifePatientProfileProposalInput
            {
                ConversationId = conversation.Id.ToString(),
                PatientProfileId = patient.Id.ToString()
            };

            var first = await service.ConfirmProfileProposal(proposal.Id, input);
            var firstResolvedAt = first.Proposal.ResolvedAt;
            var second = await service.ConfirmProfileProposal(proposal.Id, input);

            Assert.Equal(SaveLifePatientProfileProposalStatuses.Confirmed, first.Proposal.Status);
            Assert.Equal(firstResolvedAt, second.Proposal.ResolvedAt);
            Assert.Equal("肺癌术后", first.Patient.MedicalHistory);
            Assert.Equal("肺癌术后", db.Queryable<SaveLifePatientProfile>().Single().MedicalHistory);
            Assert.Equal("肺癌术后", db.Queryable<SaveLifeUserProfile>().Single().MedicalHistory);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Confirmed,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_WithDiagnosis_EndsLegacyChatCollection()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            conversation.FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion;
            conversation.FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                new SaveLifeAgentConversationState
                {
                    PendingQuestion = new SaveLifeAgentPendingQuestionState
                    {
                        Key = SaveLifeAgentTrialMatchingFields.Diagnosis,
                        Prompt = "请补充当前患者的诊断。"
                    }
                });
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                null,
                "肺癌",
                "肺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            var output = await service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = patient.Id.ToString()
                });

            var persistedPatient = db.Queryable<SaveLifePatientProfile>().Single();
            var persistedConversation = db.Queryable<SaveLifeAgentConversation>().Single();
            var state = SaveLifeAgentConversationStateCodec.Deserialize(
                persistedConversation.FlowStateJson);
            Assert.Equal("肺癌", persistedPatient.Diagnosis);
            Assert.Null(state.PendingQuestion);
            Assert.Null(output.PendingQuestion);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_SequentialRequiredFields_DoNotResumeChatCollection()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            conversation.FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion;
            conversation.FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                new SaveLifeAgentConversationState
                {
                    PendingQuestion = new SaveLifeAgentPendingQuestionState
                    {
                        Key = SaveLifeAgentTrialMatchingFields.Diagnosis,
                        Prompt = "请补充当前患者的诊断。"
                    }
                });
            var proposals = new[]
            {
                CreateProposal(
                    901,
                    conversation,
                    patient,
                    SaveLifePatientProfileProposalFields.Diagnosis,
                    null,
                    "肺癌",
                    "肺癌",
                    DateTime.Now.AddDays(1)),
                CreateProposal(
                    902,
                    conversation,
                    patient,
                    SaveLifePatientProfileProposalFields.DiseaseStage,
                    null,
                    "晚期",
                    "晚期",
                    DateTime.Now.AddDays(1)),
                CreateProposal(
                    903,
                    conversation,
                    patient,
                    SaveLifePatientProfileProposalFields.TreatmentHistory,
                    null,
                    "已接受一线化疗",
                    "已接受一线化疗",
                    DateTime.Now.AddDays(1))
            };
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposals).ExecuteCommand();
            var service = CreateService(db, "openid-1");
            var input = new ConfirmSaveLifePatientProfileProposalInput
            {
                ConversationId = conversation.Id.ToString(),
                PatientProfileId = patient.Id.ToString()
            };

            var diagnosis = await service.ConfirmProfileProposal(901, input);
            var stage = await service.ConfirmProfileProposal(902, input);
            var treatment = await service.ConfirmProfileProposal(903, input);

            Assert.Null(diagnosis.PendingQuestion);
            Assert.Null(stage.PendingQuestion);
            Assert.Null(treatment.PendingQuestion);
            Assert.Empty(diagnosis.SuggestedActions);
            Assert.Empty(stage.SuggestedActions);
            var nextAction = Assert.Single(treatment.SuggestedActions);
            Assert.Equal(SaveLifeAgentSuggestedActionTypes.StartScreening, nextAction.Type);
            Assert.Equal("查看相关项目", nextAction.Label);
            Assert.Equal("trial", nextAction.TargetType);
            Assert.Null(nextAction.TargetId);
            var confirmedPatient = db.Queryable<SaveLifePatientProfile>().Single();
            var gate = SaveLifeAgentTrialSelection.EvaluateMatchingGate(
                SaveLifePatientProfileMapper.ToOutput(confirmedPatient, confirmedPatient.Id));
            Assert.True(gate.CanMatch);
            Assert.Empty(gate.MissingFields);
            var persistedState = SaveLifeAgentConversationStateCodec.Deserialize(
                db.Queryable<SaveLifeAgentConversation>().Single().FlowStateJson);
            Assert.Null(persistedState.PendingQuestion);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_WithExpiredProposal_MarksExpiredWithoutUpdatingPatient()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            patient.City = "成都";
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.City,
                "成都",
                "上海",
                "上海",
                DateTime.Now.AddMinutes(-1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = patient.Id.ToString()
                }));

            Assert.Equal("成都", db.Queryable<SaveLifePatientProfile>().Single().City);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Expired,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Theory]
    [InlineData(SaveLifePatientProfileProposalStatuses.Superseded)]
    [InlineData(SaveLifePatientProfileProposalStatuses.Rejected)]
    public async Task ConfirmProfileProposal_WithProcessedProposal_DoesNotUpdatePatient(string status)
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                null,
                "肺癌",
                "肺癌",
                DateTime.Now.AddDays(1));
            proposal.Status = status;
            proposal.ResolvedTime = DateTime.Now;
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = patient.Id.ToString()
                }));

            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Diagnosis);
            Assert.Equal(status, db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_ForNonCurrentPatient_DoesNotUpdateEitherPatient()
    {
        await WithDatabase(async db =>
        {
            var self = CreatePatient(501, "openid-1", "本人");
            var parent = CreatePatient(502, "openid-1", "母亲");
            var conversation = CreateConversation(701, "openid-1", self.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                parent,
                SaveLifePatientProfileProposalFields.Diagnosis,
                null,
                "肺癌",
                "肺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(new[] { self, parent }).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = parent.Id.ToString()
                }));

            Assert.All(db.Queryable<SaveLifePatientProfile>().ToList(), item => Assert.Null(item.Diagnosis));
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_ForAnotherAccount_DoesNotExposeOrUpdateProposal()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(601, "openid-2", "本人");
            var conversation = CreateConversation(702, "openid-2", patient.Id);
            var proposal = CreateProposal(
                902,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                null,
                "肺癌",
                "肺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = patient.Id.ToString()
                }));

            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Diagnosis);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_WhenProfileChangedAfterProposal_SupersedesWithoutOverwriting()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            patient.City = "上海";
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.City,
                "成都",
                "北京",
                "北京",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = patient.Id.ToString()
                }));

            Assert.Equal("上海", db.Queryable<SaveLifePatientProfile>().Single().City);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Superseded,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ConfirmProfileProposal_WhenAuditUpdateFails_RollsBackPatientAndLegacyChanges()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            patient.SourceUserProfileId = 401;
            patient.MedicalHistory = "旧病史";
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var legacy = new SaveLifeUserProfile
            {
                Id = 401,
                ExternalUserId = "openid-1",
                RealName = "本人",
                MedicalHistory = "旧病史",
                CreateTime = DateTime.Now.AddDays(-30)
            };
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.MedicalHistory,
                "旧病史",
                "新病史",
                "新病史",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(legacy).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_profile_proposal_confirmation
                BEFORE UPDATE ON sl_patient_profile_proposal
                WHEN NEW.Status = 'confirmed'
                BEGIN
                    SELECT RAISE(ABORT, 'forced proposal audit failure');
                END;
                """);
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ConfirmProfileProposal(
                proposal.Id,
                new ConfirmSaveLifePatientProfileProposalInput
                {
                    ConversationId = conversation.Id.ToString(),
                    PatientProfileId = patient.Id.ToString()
                }));

            Assert.Equal("旧病史", db.Queryable<SaveLifePatientProfile>().Single().MedicalHistory);
            Assert.Equal("旧病史", db.Queryable<SaveLifeUserProfile>().Single().MedicalHistory);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public void ConfirmationMethod_IsNotAPublicRoute()
    {
        var method = typeof(SaveLifePatientProfileService).GetMethod(
            nameof(SaveLifePatientProfileService.ConfirmProfileProposal));

        Assert.NotNull(method?.GetCustomAttribute<NonActionAttribute>());
        Assert.Empty(method!.GetCustomAttributes<HttpMethodAttribute>());
    }

    [Fact]
    public async Task ResolveProfileProposal_Confirm_DerivesContextAndIsIdempotent()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            patient.DiseaseStage = "III期";
            patient.TreatmentHistory = "已完成一线治疗";
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "肺腺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            InsertProposalTurnMessages(db, proposal, "openid-1");
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");
            var input = new DecideSaveLifePatientProfileProposalInput { Decision = "confirm" };

            var first = await service.ResolveProfileProposal(proposal.Id, input);
            var second = await service.ResolveProfileProposal(proposal.Id, input);

            Assert.Equal(SaveLifePatientProfileProposalStatuses.Confirmed, first.Proposal.Status);
            Assert.Equal(first.Proposal.ResolvedAt, second.Proposal.ResolvedAt);
            Assert.Equal("肺腺癌", second.Patient.Diagnosis);
            Assert.Equal(
                SaveLifeAgentSuggestedActionTypes.StartScreening,
                Assert.Single(first.SuggestedActions).Type);
            Assert.Equal(
                SaveLifeAgentSuggestedActionTypes.StartScreening,
                Assert.Single(second.SuggestedActions).Type);
        });
    }

    [Fact]
    public async Task ResolveProfileProposal_Reject_IsIdempotentWithoutUpdatingPatient()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "肺腺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            InsertProposalTurnMessages(db, proposal, "openid-1");
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");
            var input = new DecideSaveLifePatientProfileProposalInput { Decision = "reject" };

            var first = await service.ResolveProfileProposal(proposal.Id, input);
            var second = await service.ResolveProfileProposal(proposal.Id, input);

            Assert.Equal(SaveLifePatientProfileProposalStatuses.Rejected, first.Proposal.Status);
            Assert.Equal(first.Proposal.ResolvedAt, second.Proposal.ResolvedAt);
            Assert.Null(second.Patient.Diagnosis);
            Assert.Empty(first.SuggestedActions);
            Assert.Empty(second.SuggestedActions);
            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Diagnosis);
        });
    }

    [Fact]
    public async Task ResolveProfileProposal_WithOppositeDecision_ReturnsConflict()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "肺腺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            InsertProposalTurnMessages(db, proposal, "openid-1");
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");
            await service.ResolveProfileProposal(
                proposal.Id,
                new DecideSaveLifePatientProfileProposalInput { Decision = "reject" });

            await Assert.ThrowsAnyAsync<Exception>(() => service.ResolveProfileProposal(
                proposal.Id,
                new DecideSaveLifePatientProfileProposalInput { Decision = "confirm" }));

            Assert.Equal(SaveLifePatientProfileProposalStatuses.Rejected,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Diagnosis);
        });
    }

    [Fact]
    public async Task ResolveProfileProposal_WithForeignSourceMessages_DoesNotUpdatePatient()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "肺腺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            InsertProposalTurnMessages(db, proposal, "openid-2");
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ResolveProfileProposal(
                proposal.Id,
                new DecideSaveLifePatientProfileProposalInput { Decision = "confirm" }));

            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Diagnosis);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ResolveProfileProposal_WithForeignSourceMaterial_DoesNotUpdatePatient()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Pathology,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "肺腺癌",
                DateTime.Now.AddDays(1));
            proposal.SourceType = SaveLifePatientProfileProposalSources.Material;
            proposal.SourceMaterialId = 991;
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            InsertProposalTurnMessages(db, proposal, "openid-1");
            db.Insertable(new SaveLifeEnrollmentMaterial
            {
                Id = 991,
                ExternalUserId = "openid-2",
                PatientProfileId = patient.Id,
                FileName = "病理报告.pdf",
                Status = "submitted",
                CreateTime = DateTime.Now
            }).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ResolveProfileProposal(
                proposal.Id,
                new DecideSaveLifePatientProfileProposalInput { Decision = "confirm" }));

            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Pathology);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
        });
    }

    [Fact]
    public async Task ResolveProfileProposal_WithOwnedDocumentMaterial_ConfirmsProposal()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Pathology,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "病理诊断：肺腺癌",
                DateTime.Now.AddDays(1));
            proposal.SourceType = SaveLifePatientProfileProposalSources.Material;
            proposal.SourceMaterialId = 991;
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            InsertProposalTurnMessages(db, proposal, "openid-1");
            db.Insertable(new SaveLifeEnrollmentMaterial
            {
                Id = 991,
                ExternalUserId = "openid-1",
                PatientProfileId = patient.Id,
                FileName = "病理报告.pdf",
                FileSuffix = ".pdf",
                ContentType = "application/pdf",
                ExtractedText = "检查结果：病理诊断：肺腺癌。",
                Status = "submitted",
                CreateTime = DateTime.Now
            }).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            var output = await service.ResolveProfileProposal(
                proposal.Id,
                new DecideSaveLifePatientProfileProposalInput { Decision = "confirm" });

            Assert.Equal(SaveLifePatientProfileProposalStatuses.Confirmed, output.Proposal.Status);
            Assert.Equal("肺腺癌", output.Patient.Pathology);
        });
    }

    [Fact]
    public async Task ResolveProfileProposal_WithInvalidDecision_DoesNotChangeProposal()
    {
        await WithDatabase(async db =>
        {
            var patient = CreatePatient(501, "openid-1", "本人");
            var conversation = CreateConversation(701, "openid-1", patient.Id);
            var proposal = CreateProposal(
                901,
                conversation,
                patient,
                SaveLifePatientProfileProposalFields.Diagnosis,
                oldValue: null,
                proposedValue: "肺腺癌",
                evidence: "肺腺癌",
                DateTime.Now.AddDays(1));
            db.Insertable(patient).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();
            var service = CreateService(db, "openid-1");

            await Assert.ThrowsAnyAsync<Exception>(() => service.ResolveProfileProposal(
                proposal.Id,
                new DecideSaveLifePatientProfileProposalInput { Decision = "approve" }));

            Assert.Equal(SaveLifePatientProfileProposalStatuses.Pending,
                db.Queryable<SaveLifePatientProfileProposal>().Single().Status);
            Assert.Null(db.Queryable<SaveLifePatientProfile>().Single().Diagnosis);
        });
    }

    private static SaveLifePatientProfile CreatePatient(long id, string owner, string displayName)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = owner,
            AccountPatientKey = displayName == "本人"
                ? SaveLifePatientProfileKeys.ForSelf(owner)
                : SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = displayName == "本人"
                ? SaveLifePatientRelationships.Self
                : SaveLifePatientRelationships.Parent,
            RelationshipDescription = displayName,
            DisplayName = displayName,
            IsDefault = displayName == "本人",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentConversation CreateConversation(long id, string owner, long patientId)
    {
        return new SaveLifeAgentConversation
        {
            Id = id,
            ExternalUserId = owner,
            CurrentPatientProfileId = patientId,
            Status = "active",
            Phase = "collecting",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentMessage CreateUserMessage(
        long id,
        SaveLifeAgentConversation conversation,
        long patientId,
        string content)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            ConversationId = conversation.Id,
            ExternalUserId = conversation.ExternalUserId,
            PatientProfileId = patientId,
            Role = "user",
            Content = content,
            ClientMessageId = $"client-{id}",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifePatientProfileProposal CreateProposal(
        long id,
        SaveLifeAgentConversation conversation,
        SaveLifePatientProfile patient,
        string field,
        string? oldValue,
        string proposedValue,
        string evidence,
        DateTime expiresAt)
    {
        return new SaveLifePatientProfileProposal
        {
            Id = id,
            OwnerExternalUserId = conversation.ExternalUserId,
            ConversationId = conversation.Id,
            PatientProfileId = patient.Id,
            SourceUserMessageId = 801,
            SourceAssistantMessageId = 802,
            FieldName = field,
            OldValue = oldValue,
            ProposedValue = proposedValue,
            Evidence = evidence,
            Status = SaveLifePatientProfileProposalStatuses.Pending,
            ExpiresTime = expiresAt,
            CreateTime = DateTime.Now
        };
    }

    private static void InsertProposalTurnMessages(
        ISqlSugarClient db,
        SaveLifePatientProfileProposal proposal,
        string externalUserId)
    {
        db.Insertable(new[]
        {
            new SaveLifeAgentMessage
            {
                Id = proposal.SourceUserMessageId,
                ConversationId = proposal.ConversationId,
                ExternalUserId = externalUserId,
                PatientProfileId = proposal.PatientProfileId,
                Role = "user",
                Content = proposal.Evidence,
                CreateTime = DateTime.Now.AddMinutes(-1)
            },
            new SaveLifeAgentMessage
            {
                Id = proposal.SourceAssistantMessageId,
                ConversationId = proposal.ConversationId,
                ExternalUserId = externalUserId,
                PatientProfileId = proposal.PatientProfileId,
                ReplyToMessageId = proposal.SourceUserMessageId,
                Role = "assistant",
                Content = "已回答",
                GenerationStatus = "completed",
                CreateTime = DateTime.Now
            }
        }).ExecuteCommand();
    }

    private static SaveLifePatientProfileService CreateService(ISqlSugarClient db, string openId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimConst.OpenId, openId)],
                "test"))
        };
        return new SaveLifePatientProfileService(
            SqlSugarRepository<SaveLifePatientProfile>.CreateForContext(db),
            SqlSugarRepository<SaveLifeAgentConversation>.CreateForContext(db),
            SqlSugarRepository<SaveLifeUserProfile>.CreateForContext(db),
            new AllowConsentGuard(),
            new FixedPatientIdGenerator(),
            new HttpContextAccessor { HttpContext = httpContext });
    }

    private static async Task WithDatabase(Func<ISqlSugarClient, Task> test)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifePatientProfileProposal),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage),
                typeof(SaveLifeEnrollmentMaterial),
                typeof(SaveLifeUserProfile));
            await test(db);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={dbPath};Pooling=False",
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

    private sealed class AllowConsentGuard : ISaveLifePrivacyConsentGuard
    {
        public Task EnsureConsentedAsync(string? externalUserId = null) => Task.CompletedTask;

        public Task RecordConsentAsync(string externalUserId, string? privacyConsent) => Task.CompletedTask;

        public Task<SaveLifeMiniAppConsentDecision> EvaluateMiniAppUpsertAsync(
            string externalUserId,
            string? privacyConsent)
        {
            return Task.FromResult(new SaveLifeMiniAppConsentDecision(
                CanProceed: true,
                ShouldRecordConsent: false,
                ConsentToRecord: null,
                ErrorMessage: null));
        }
    }

    private sealed class FixedPatientIdGenerator : ISaveLifePatientIdGenerator
    {
        public long NextId() => 999;
    }
}
