using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePatientProfileApiTests
{
    [Fact]
    public async Task GetPatients_WithoutActiveConversation_RestoresAccountDefaultPatient()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeUserProfile));
            var self = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
            var parent = CreatePatient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");
            self.IsDefault = false;
            parent.IsDefault = true;
            db.Insertable(new[] { self, parent }).ExecuteCommand();

            var page = await CreateService(db, "openid-1").GetPatients();

            Assert.Equal(parent.Id.ToString(), Assert.Single(page.Items, item => item.IsCurrent).Id);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task PatientService_WithAuthenticatedAccount_ListsAndMutatesOnlyOwnedPatients()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeUserProfile));
            var self = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
            var parent = CreatePatient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");
            var deleted = CreatePatient(503, "openid-1", SaveLifePatientRelationships.Other, "旧档案");
            deleted.IsDelete = true;
            var otherAccount = CreatePatient(601, "openid-2", SaveLifePatientRelationships.Self, "本人");
            db.Insertable(new[] { self, parent, deleted, otherAccount }).ExecuteCommand();
            db.Insertable(new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = parent.Id,
                Status = "active",
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimConst.OpenId, "openid-1")],
                    "test"))
            };
            var service = new SaveLifePatientProfileService(
                CreateRepository<SaveLifePatientProfile>(db),
                CreateRepository<SaveLifeAgentConversation>(db),
                CreateRepository<SaveLifeUserProfile>(db),
                new AllowConsentGuard(),
                new FixedPatientIdGenerator(801),
                new HttpContextAccessor { HttpContext = httpContext });

            var page = await service.GetPatients();

            Assert.Equal(2, page.Total);
            Assert.Equal(2, page.Items.Count);
            Assert.Equal(parent.Id.ToString(), Assert.Single(page.Items, u => u.IsCurrent).Id);

            var created = await service.CreatePatient(new CreateSaveLifePatientProfileInput
            {
                RelationshipToAccount = SaveLifePatientRelationships.Spouse,
                RelationshipDescription = "配偶",
                DisplayName = "李四",
                Gender = "female"
            });
            var createdId = long.Parse(created.Id);
            Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
            Assert.Equal("配偶", created.RelationshipDescription);
            Assert.Equal("openid-1", db.Queryable<SaveLifePatientProfile>()
                .Single(u => u.Id == createdId)
                .OwnerExternalUserId);

            var detail = await service.GetPatient(createdId);
            var updated = await service.UpdatePatient(createdId, new UpdateSaveLifePatientProfileInput
            {
                DisplayName = "李女士",
                Diagnosis = "乳腺癌"
            });
            Assert.Equal(detail.Id, updated.Id);
            Assert.Equal("李女士", updated.DisplayName);
            Assert.Equal("乳腺癌", updated.Diagnosis);

            await Assert.ThrowsAnyAsync<Exception>(() => service.GetPatient(otherAccount.Id));
            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdatePatient(
                otherAccount.Id,
                new UpdateSaveLifePatientProfileInput { DisplayName = "越权修改" }));
            Assert.Equal("本人", db.Queryable<SaveLifePatientProfile>()
                .Single(u => u.Id == otherAccount.Id)
                .DisplayName);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task SwitchCurrentPatient_WithOwnedActiveConversation_ConfirmsSwitchAndClearsPatientState()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifePatientProfileProposal),
                typeof(SaveLifeUserProfile));
            var self = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
            var parent = CreatePatient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲");
            var otherAccount = CreatePatient(601, "openid-2", SaveLifePatientRelationships.Self, "本人");
            db.Insertable(new[] { self, parent, otherAccount }).ExecuteCommand();
            db.Insertable(new[]
            {
                new SaveLifeAgentConversation
                {
                    Id = 701,
                    ExternalUserId = "openid-1",
                    CurrentPatientProfileId = self.Id,
                    Status = "active",
                    Phase = "matching",
                    ContextType = "trial",
                    ContextId = 81,
                    EnrollmentRecordId = 91,
                    MatchedTrialsJson = "[{\"id\":81}]",
                    FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion,
                    FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                        new SaveLifeAgentConversationState
                        {
                            PendingQuestion = new SaveLifeAgentPendingQuestionState
                            {
                                Key = "city",
                                Prompt = "请选择城市"
                            },
                            SelectedTrialId = 81,
                            TrialOptOut = true
                        }),
                    CreateTime = new DateTime(2026, 7, 14)
                },
                new SaveLifeAgentConversation
                {
                    Id = 702,
                    ExternalUserId = "openid-2",
                    CurrentPatientProfileId = otherAccount.Id,
                    Status = "active",
                    CreateTime = new DateTime(2026, 7, 14)
                }
            }).ExecuteCommand();
            db.Insertable(new SaveLifePatientProfileProposal
            {
                Id = 1001,
                OwnerExternalUserId = "openid-1",
                ConversationId = 701,
                PatientProfileId = self.Id,
                SourceUserMessageId = 1101,
                SourceAssistantMessageId = 1102,
                FieldName = SaveLifePatientProfileProposalFields.City,
                ProposedValue = "成都",
                Evidence = "成都",
                Status = SaveLifePatientProfileProposalStatuses.Pending,
                ExpiresTime = new DateTime(2026, 7, 15),
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();

            var service = CreateService(db, "openid-1");

            var selected = await service.SwitchCurrentPatient(
                "701",
                new SwitchSaveLifeCurrentPatientInput { PatientProfileId = parent.Id.ToString() });

            Assert.Equal(parent.Id.ToString(), selected.Id);
            Assert.True(selected.IsCurrent);
            Assert.True(selected.IsDefault);
            var conversation = db.Queryable<SaveLifeAgentConversation>().Single(u => u.Id == 701);
            Assert.Equal(parent.Id, conversation.CurrentPatientProfileId);
            Assert.Equal("created", conversation.Phase);
            Assert.Null(conversation.ContextType);
            Assert.Null(conversation.ContextId);
            Assert.Null(conversation.EnrollmentRecordId);
            Assert.Null(conversation.MatchedTrialsJson);
            var flowState = SaveLifeAgentConversationStateCodec.Deserialize(conversation.FlowStateJson);
            Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, flowState.Version);
            Assert.Null(flowState.PendingQuestion);
            Assert.False(flowState.TrialOptOut);
            Assert.Null(flowState.SelectedTrialId);
            Assert.Null(flowState.PendingPatientSwitchProfileId);
            var proposal = db.Queryable<SaveLifePatientProfileProposal>().Single();
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Rejected, proposal.Status);
            Assert.NotNull(proposal.ResolvedTime);

            await Assert.ThrowsAnyAsync<Exception>(() => service.SwitchCurrentPatient(
                "701",
                new SwitchSaveLifeCurrentPatientInput
                {
                    PatientProfileId = otherAccount.Id.ToString()
                }));
            await Assert.ThrowsAnyAsync<Exception>(() => service.SwitchCurrentPatient(
                "702",
                new SwitchSaveLifeCurrentPatientInput { PatientProfileId = self.Id.ToString() }));
            Assert.Equal(parent.Id, db.Queryable<SaveLifeAgentConversation>()
                .Single(u => u.Id == 701)
                .CurrentPatientProfileId);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Theory]
    [InlineData(nameof(SaveLifePatientProfileService.GetPatients), "GET", "/api/v1/save-life/patients")]
    [InlineData(nameof(SaveLifePatientProfileService.CreatePatient), "POST", "/api/v1/save-life/patients")]
    [InlineData(nameof(SaveLifePatientProfileService.GetPatient), "GET", "/api/v1/save-life/patients/{patientProfileId}")]
    [InlineData(nameof(SaveLifePatientProfileService.UpdatePatient), "PATCH", "/api/v1/save-life/patients/{patientProfileId}")]
    [InlineData(nameof(SaveLifePatientProfileService.SwitchCurrentPatient), "PUT", "/api/v1/agent/conversations/{conversationId}/current-patient")]
    public void PatientEndpoints_ExposeAuthenticatedResourceRoutes(
        string methodName,
        string httpMethod,
        string route)
    {
        var method = typeof(SaveLifePatientProfileService)
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        var routeAttribute = method?.GetCustomAttributes<HttpMethodAttribute>().Single();

        Assert.NotNull(typeof(SaveLifePatientProfileService).GetCustomAttribute<AuthorizeAttribute>());
        Assert.Contains(
            typeof(SaveLifePatientProfileService).GetInterfaces(),
            type => type.Name == "IDynamicApiController");
        Assert.NotNull(method);
        Assert.Equal(route, routeAttribute?.Template);
        Assert.Contains(httpMethod, routeAttribute?.HttpMethods ?? Array.Empty<string>());
    }

    [Theory]
    [InlineData(typeof(CreateSaveLifePatientProfileInput))]
    [InlineData(typeof(UpdateSaveLifePatientProfileInput))]
    [InlineData(typeof(SwitchSaveLifeCurrentPatientInput))]
    [InlineData(typeof(CreateAndSelectSaveLifePatientInput))]
    public void PatientMutationInputs_DoNotAcceptAccountOrPersistenceIdentity(Type inputType)
    {
        var forbiddenNames = new[]
        {
            "ExternalUserId",
            "OwnerExternalUserId",
            "AccountPatientKey",
            "SourceUserProfileId",
            "IsDefault",
            "IsDelete"
        };

        Assert.All(forbiddenNames, name => Assert.Null(inputType.GetProperty(name)));
    }

    [Fact]
    public void OwnershipPolicy_OnlyAcceptsPatientOwnedByCurrentAccount()
    {
        var patient = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Parent, "母亲");

        Assert.True(SaveLifePatientOwnershipPolicy.IsOwnedBy(patient, "openid-1"));
        Assert.False(SaveLifePatientOwnershipPolicy.IsOwnedBy(patient, "openid-2"));
        Assert.ThrowsAny<Exception>(() =>
            SaveLifePatientOwnershipPolicy.EnsureOwnedBy(patient, "openid-2"));
    }

    [Fact]
    public void RelationshipPolicy_NormalizesConcreteRelationshipAndProtectsSelfIdentity()
    {
        var relationship = SaveLifePatientOwnershipPolicy.NormalizeRelationship(
            SaveLifePatientRelationships.Parent,
            "  母亲  ");

        Assert.Equal(SaveLifePatientRelationships.Parent, relationship.Code);
        Assert.Equal("母亲", relationship.Description);
        Assert.ThrowsAny<Exception>(() =>
            SaveLifePatientOwnershipPolicy.EnsureRelationshipChangeAllowed(
                SaveLifePatientRelationships.Self,
                SaveLifePatientRelationships.Parent));
    }

    [Fact]
    public void PatientOutput_IncludesCurrentMarkerConcreteRelationshipAndCompleteness()
    {
        var patient = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Parent, "母亲");
        patient.Gender = "female";
        patient.BirthDate = new DateTime(1965, 3, 2);
        patient.City = "成都市";
        patient.Diagnosis = "非小细胞肺癌";
        patient.DiseaseStage = "III期";
        patient.TreatmentHistory = "已完成一线治疗";
        patient.Biomarkers = "EGFR 19del";

        var output = SaveLifePatientProfileMapper.ToOutput(patient, patient.Id);

        Assert.Equal("母亲", output.RelationshipDescription);
        Assert.True(output.IsCurrent);
        Assert.Equal(100, output.ProfileCompleteness);
    }

    [Fact]
    public void LegacyMiniAppProfileEndpoints_KeepExistingReturnContract()
    {
        var getMethod = typeof(SaveLifeUserProfileService)
            .GetMethod(nameof(SaveLifeUserProfileService.MiniAppProfile));
        var updateMethod = typeof(SaveLifeUserProfileService)
            .GetMethod(nameof(SaveLifeUserProfileService.MiniAppUpdateProfile));

        Assert.Equal(typeof(Task<SaveLifeMiniAppProfileOutput>), getMethod?.ReturnType);
        Assert.Equal(typeof(Task<SaveLifeMiniAppProfileOutput>), updateMethod?.ReturnType);
    }

    [Fact]
    public void LegacyCompatibility_SynchronizesSelfPatientWithoutChangingOutputShape()
    {
        var patient = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人");
        patient.DisplayName = "张三";
        patient.Gender = "male";
        patient.BirthDate = new DateTime(1980, 1, 2);
        patient.Residence = "四川省成都市";
        patient.Ethnicity = "汉族";
        patient.MedicalHistory = "肺癌术后";
        patient.Allergies = "青霉素";
        var legacy = new SaveLifeUserProfile { ExternalUserId = "openid-1" };

        SaveLifePatientProfileCompatibility.ApplyPatientToLegacy(patient, legacy);

        Assert.Equal("张三", legacy.RealName);
        Assert.Equal("四川省成都市", legacy.Address);
        Assert.Equal("肺癌术后", legacy.MedicalHistory);
        Assert.Equal("青霉素", legacy.Allergies);
    }

    private static SaveLifePatientProfile CreatePatient(
        long id,
        string ownerExternalUserId,
        string relationship,
        string relationshipDescription)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = ownerExternalUserId,
            AccountPatientKey = relationship == SaveLifePatientRelationships.Self
                ? SaveLifePatientProfileKeys.ForSelf(ownerExternalUserId)
                : SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = relationship,
            RelationshipDescription = relationshipDescription,
            DisplayName = relationshipDescription,
            IsDefault = relationship == SaveLifePatientRelationships.Self,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifePatientProfileService CreateService(
        ISqlSugarClient db,
        string openId)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimConst.OpenId, openId)],
                "test"))
        };
        return new SaveLifePatientProfileService(
            CreateRepository<SaveLifePatientProfile>(db),
            CreateRepository<SaveLifeAgentConversation>(db),
            CreateRepository<SaveLifeUserProfile>(db),
            new AllowConsentGuard(),
            new FixedPatientIdGenerator(801),
            new HttpContextAccessor { HttpContext = httpContext });
    }

    private static SqlSugarRepository<T> CreateRepository<T>(ISqlSugarClient db)
        where T : class, new()
    {
        return SqlSugarRepository<T>.CreateForContext(db);
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
        public Task EnsureConsentedAsync(string? externalUserId = null)
        {
            return Task.CompletedTask;
        }

        public Task RecordConsentAsync(string externalUserId, string? privacyConsent)
        {
            return Task.CompletedTask;
        }

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

    private sealed class FixedPatientIdGenerator(long id) : ISaveLifePatientIdGenerator
    {
        public long NextId()
        {
            return id;
        }
    }
}
