using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentPatientSwitchTests
{
    [Fact]
    public void SwitchPolicy_ExposesOnlyExplicitConfirmation()
    {
        var publicMethods = typeof(SaveLifePatientSwitchPolicy)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.Equal([nameof(SaveLifePatientSwitchPolicy.ConfirmAsync)], publicMethods);
    }

    [Fact]
    public async Task ConfirmAsync_WithOwnedTarget_ResetsWorkflowAndRejectsPendingProposals()
    {
        await WithDatabase(async db =>
        {
            var now = new DateTime(2026, 7, 14, 12, 0, 0);
            var current = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人", "张三");
            var mother = CreatePatient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲", "李女士");
            var conversation = new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = current.Id,
                PatientContextVersion = 7,
                Status = "active",
                Phase = "matching",
                ContextType = "trial",
                ContextId = 801,
                EnrollmentRecordId = 901,
                MatchedTrialsJson = "[{\"id\":801}]",
                AgentSessionJson = "{\"history\":[]}",
                FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion,
                FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                    new SaveLifeAgentConversationState
                    {
                        PendingQuestion = new SaveLifeAgentPendingQuestionState
                        {
                            Key = "city",
                            Prompt = "患者目前在哪个城市？"
                        },
                        PendingPatientSwitchProfileId = mother.Id,
                        SelectedTrialId = 801,
                        TrialOptOut = true
                    }),
                CreateTime = now.AddDays(-1)
            };
            var proposal = new SaveLifePatientProfileProposal
            {
                Id = 1001,
                OwnerExternalUserId = "openid-1",
                ConversationId = conversation.Id,
                PatientProfileId = current.Id,
                SourceUserMessageId = 1101,
                SourceAssistantMessageId = 1102,
                FieldName = SaveLifePatientProfileProposalFields.City,
                ProposedValue = "成都",
                Evidence = "成都",
                Status = SaveLifePatientProfileProposalStatuses.Pending,
                ExpiresTime = now.AddDays(1),
                CreateTime = now.AddHours(-1)
            };
            db.Insertable(new[] { current, mother }).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();
            db.Insertable(proposal).ExecuteCommand();

            await SaveLifePatientSwitchPolicy.ConfirmAsync(db, conversation, mother, now);

            var persisted = db.Queryable<SaveLifeAgentConversation>().Single();
            Assert.Equal(mother.Id, persisted.CurrentPatientProfileId);
            Assert.Equal(8, persisted.PatientContextVersion);
            Assert.Equal(8, conversation.PatientContextVersion);
            Assert.Equal("created", persisted.Phase);
            Assert.Null(persisted.ContextType);
            Assert.Null(persisted.ContextId);
            Assert.Null(persisted.EnrollmentRecordId);
            Assert.Null(persisted.MatchedTrialsJson);
            Assert.Null(persisted.AgentSessionJson);
            Assert.False(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == current.Id).IsDefault);
            Assert.True(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == mother.Id).IsDefault);
            var state = SaveLifeAgentConversationStateCodec.Deserialize(persisted.FlowStateJson);
            Assert.Null(state.PendingQuestion);
            Assert.Null(state.PendingPatientSwitchProfileId);
            Assert.Null(state.SelectedTrialId);
            Assert.False(state.TrialOptOut);
            Assert.Contains(current.Id, state.TrialOptOutPatientProfileIds);
            Assert.DoesNotContain(mother.Id, state.TrialOptOutPatientProfileIds);
            var resolvedProposal = db.Queryable<SaveLifePatientProfileProposal>().Single();
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Rejected, resolvedProposal.Status);
            Assert.Equal(now, resolvedProposal.ResolvedTime);

            await SaveLifePatientSwitchPolicy.ConfirmAsync(db, conversation, mother, now.AddMinutes(1));
            Assert.Equal(8, db.Queryable<SaveLifeAgentConversation>().Single().PatientContextVersion);
        });
    }

    [Fact]
    public async Task ConfirmAsync_WhenConversationAlreadyUsesTarget_RepairsAccountDefaultPatient()
    {
        await WithDatabase(async db =>
        {
            var self = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人", "张三");
            var mother = CreatePatient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲", "李女士");
            var conversation = new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = mother.Id,
                PatientContextVersion = 4,
                Status = "active",
                CreateTime = new DateTime(2026, 7, 15)
            };
            db.Insertable(new[] { self, mother }).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();

            await SaveLifePatientSwitchPolicy.ConfirmAsync(db, conversation, mother, DateTime.Now);

            Assert.False(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == self.Id).IsDefault);
            Assert.True(db.Queryable<SaveLifePatientProfile>().Single(item => item.Id == mother.Id).IsDefault);
            Assert.Equal(4, conversation.PatientContextVersion);
        });
    }

    [Fact]
    public async Task ConfirmAsync_WithForeignTarget_DoesNotChangeConversation()
    {
        await WithDatabase(async db =>
        {
            var current = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人", "张三");
            var foreign = CreatePatient(601, "openid-2", SaveLifePatientRelationships.Parent, "母亲", "王女士");
            var conversation = new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = current.Id,
                Status = "active",
                CreateTime = DateTime.Now
            };

            await Assert.ThrowsAnyAsync<Exception>(() =>
                SaveLifePatientSwitchPolicy.ConfirmAsync(db, conversation, foreign, DateTime.Now));
            Assert.Equal(current.Id, conversation.CurrentPatientProfileId);
        });
    }

    [Fact]
    public async Task ConfirmAsync_SwitchAwayAndBack_RestoresPatientScopedTrialOptOut()
    {
        await WithDatabase(async db =>
        {
            var self = CreatePatient(501, "openid-1", SaveLifePatientRelationships.Self, "本人", "张三");
            var mother = CreatePatient(502, "openid-1", SaveLifePatientRelationships.Parent, "母亲", "李女士");
            var conversation = new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = self.Id,
                PatientContextVersion = 4,
                FlowStateVersion = SaveLifeAgentConversationState.CurrentVersion,
                FlowStateJson = SaveLifeAgentConversationStateCodec.Serialize(
                    new SaveLifeAgentConversationState
                    {
                        GeneralTrialOptOut = true,
                        TrialOptOutPatientProfileIds = [self.Id],
                        TrialOptOut = true
                    }),
                Status = "active",
                CreateTime = new DateTime(2026, 7, 15)
            };
            db.Insertable(new[] { self, mother }).ExecuteCommand();
            db.Insertable(conversation).ExecuteCommand();

            await SaveLifePatientSwitchPolicy.ConfirmAsync(db, conversation, mother, DateTime.Now);
            var motherState = SaveLifeAgentConversationStateCodec.Deserialize(conversation.FlowStateJson);
            Assert.False(motherState.TrialOptOut);
            Assert.True(motherState.GeneralTrialOptOut);
            Assert.Contains(self.Id, motherState.TrialOptOutPatientProfileIds);

            await SaveLifePatientSwitchPolicy.ConfirmAsync(db, conversation, self, DateTime.Now.AddMinutes(1));
            var selfState = SaveLifeAgentConversationStateCodec.Deserialize(conversation.FlowStateJson);
            Assert.True(selfState.TrialOptOut);
            Assert.True(selfState.GeneralTrialOptOut);
            Assert.Contains(self.Id, selfState.TrialOptOutPatientProfileIds);
            Assert.DoesNotContain(mother.Id, selfState.TrialOptOutPatientProfileIds);
        });
    }

    private static SaveLifePatientProfile CreatePatient(
        long id,
        string owner,
        string relationship,
        string relationshipDescription,
        string displayName)
    {
        return new SaveLifePatientProfile
        {
            Id = id,
            OwnerExternalUserId = owner,
            AccountPatientKey = relationship == SaveLifePatientRelationships.Self
                ? SaveLifePatientProfileKeys.ForSelf(owner)
                : SaveLifePatientProfileKeys.ForOther(id),
            RelationshipToAccount = relationship,
            RelationshipDescription = relationshipDescription,
            DisplayName = displayName,
            IsDefault = relationship == SaveLifePatientRelationships.Self,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static async Task WithDatabase(Func<ISqlSugarClient, Task> test)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
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
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifePatientProfileProposal),
                typeof(SaveLifeAgentConversation));
            await test(db);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
