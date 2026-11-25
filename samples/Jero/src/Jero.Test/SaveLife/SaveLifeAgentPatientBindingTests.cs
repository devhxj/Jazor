using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentPatientBindingTests
{
    [Fact]
    public void BindConversationToAccountProfile_NewAndLegacyConversations_UseCanonicalSelfProfile()
    {
        var selfProfile = new SaveLifePatientProfile
        {
            Id = 501,
            OwnerExternalUserId = "openid-1",
            AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-1"),
            DisplayName = "张三"
        };
        var newConversation = CreateConversation(101, "openid-1");
        var legacyConversation = CreateConversation(102, "openid-1");
        legacyConversation.CurrentPatientProfileId = 502;
        legacyConversation.PatientContextVersion = 1;

        SaveLifeAgentService.BindConversationToAccountProfile(newConversation, selfProfile);
        SaveLifeAgentService.BindConversationToAccountProfile(legacyConversation, selfProfile);

        Assert.Equal(501, newConversation.CurrentPatientProfileId);
        Assert.Equal(1, newConversation.PatientContextVersion);
        Assert.Equal(501, legacyConversation.CurrentPatientProfileId);
        Assert.Equal(2, legacyConversation.PatientContextVersion);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, newConversation.FlowStateVersion);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, legacyConversation.FlowStateVersion);
    }

    [Fact]
    public void Apply_WithHistoricalConversationsAndMessages_BackfillsEachAccountSelfPatient()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifeUserProfile),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage));
            db.Insertable(new[]
            {
                CreateUserProfile(11, "openid-1", "张三"),
                CreateUserProfile(22, "openid-2", "李四")
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                CreateConversation(101, "openid-1"),
                CreateConversation(202, "openid-2")
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                CreateMessage(1001, 101, "openid-1", "user", "问题一"),
                CreateMessage(1002, 101, "openid-1", "assistant", "回答一"),
                CreateMessage(2001, 202, "openid-2", "user", "问题二")
            }).ExecuteCommand();

            SaveLifePatientProfileMigration.Apply(db);
            SaveLifePatientProfileMigration.Apply(db);

            var patients = db.Queryable<SaveLifePatientProfile>().ClearFilter().ToList();
            var patient1 = Assert.Single(patients, u => u.OwnerExternalUserId == "openid-1");
            var patient2 = Assert.Single(patients, u => u.OwnerExternalUserId == "openid-2");
            var conversations = db.Queryable<SaveLifeAgentConversation>().ClearFilter().ToList();
            Assert.Equal(patient1.Id, conversations.Single(u => u.Id == 101).CurrentPatientProfileId);
            Assert.Equal(patient2.Id, conversations.Single(u => u.Id == 202).CurrentPatientProfileId);
            Assert.All(conversations, conversation => Assert.Equal(
                SaveLifeAgentConversationState.CurrentVersion,
                conversation.FlowStateVersion));

            var messages = db.Queryable<SaveLifeAgentMessage>().ClearFilter().ToList();
            Assert.All(messages.Where(u => u.ConversationId == 101), message => Assert.Equal(patient1.Id, message.PatientProfileId));
            Assert.All(messages.Where(u => u.ConversationId == 202), message => Assert.Equal(patient2.Id, message.PatientProfileId));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void BindNewMessage_WithCurrentPatient_RecordsPatientAtMessageCreation()
    {
        var conversation = CreateConversation(101, "openid-1");
        conversation.CurrentPatientProfileId = 501;
        var message = CreateMessage(1001, 101, "openid-1", "user", "问题");

        SaveLifeAgentPatientBinding.BindNewMessage(conversation, message);

        Assert.Equal(501, message.PatientProfileId);
    }

    [Fact]
    public void BindNewMessage_WithoutCurrentPatient_LeavesMessageUnscoped()
    {
        var conversation = CreateConversation(101, "openid-1");
        var message = CreateMessage(1001, 101, "openid-1", "user", "通用咨询");

        SaveLifeAgentPatientBinding.BindNewMessage(conversation, message);

        Assert.Null(message.PatientProfileId);
    }

    [Fact]
    public void BindReply_WithoutPatient_KeepsReplyUnscoped()
    {
        var userMessage = CreateMessage(1001, 101, "openid-1", "user", "通用问题");
        var assistantMessage = CreateMessage(1002, 101, "openid-1", "assistant", string.Empty);

        SaveLifeAgentPatientBinding.BindReply(userMessage, assistantMessage);

        Assert.Null(assistantMessage.PatientProfileId);
    }

    [Fact]
    public void BindNewMessage_WithoutPatientAndWithDifferentOwner_RejectsOwnershipMismatch()
    {
        var conversation = CreateConversation(101, "openid-1");
        var message = CreateMessage(1001, 101, "openid-2", "user", "越权消息");

        var error = Assert.Throws<InvalidOperationException>(() =>
            SaveLifeAgentPatientBinding.BindNewMessage(conversation, message));

        Assert.Equal("消息与会话归属不一致", error.Message);
        Assert.Null(message.PatientProfileId);
    }

    [Fact]
    public void BindReply_AfterCurrentPatientChanges_KeepsUserMessagePatientSnapshot()
    {
        var userMessage = CreateMessage(1001, 101, "openid-1", "user", "问题");
        userMessage.PatientProfileId = 501;
        var assistantMessage = CreateMessage(1002, 101, "openid-1", "assistant", string.Empty);

        SaveLifeAgentPatientBinding.BindReply(userMessage, assistantMessage);

        Assert.Equal(501, assistantMessage.PatientProfileId);
    }

    [Fact]
    public void InitializeConversation_WithOwnedSelfPatient_SelectsPatientAndInitializesState()
    {
        var conversation = CreateConversation(101, "openid-1");
        var patient = new SaveLifePatientProfile
        {
            Id = 501,
            OwnerExternalUserId = "openid-1",
            AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-1"),
            RelationshipToAccount = SaveLifePatientRelationships.Self,
            RelationshipDescription = "本人",
            DisplayName = "张三"
        };

        SaveLifeAgentPatientBinding.InitializeConversation(conversation, patient);

        Assert.Equal(patient.Id, conversation.CurrentPatientProfileId);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, conversation.FlowStateVersion);
    }

    [Fact]
    public void InitializeConversation_WithOwnedPreferredNonSelfPatient_SelectsPatient()
    {
        var conversation = CreateConversation(101, "openid-1");
        var patient = new SaveLifePatientProfile
        {
            Id = 502,
            OwnerExternalUserId = "openid-1",
            AccountPatientKey = SaveLifePatientProfileKeys.ForOther(502),
            RelationshipToAccount = SaveLifePatientRelationships.Parent,
            RelationshipDescription = "母亲",
            DisplayName = "李女士",
            IsDefault = true
        };

        SaveLifeAgentPatientBinding.InitializeConversation(conversation, patient);

        Assert.Equal(patient.Id, conversation.CurrentPatientProfileId);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, conversation.FlowStateVersion);
    }

    [Fact]
    public void InitializeConversation_WithAnotherAccountsPatient_DoesNotSelectPatient()
    {
        var conversation = CreateConversation(101, "openid-1");
        var patient = new SaveLifePatientProfile
        {
            Id = 501,
            OwnerExternalUserId = "openid-2",
            AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-2"),
            RelationshipToAccount = SaveLifePatientRelationships.Self,
            RelationshipDescription = "本人",
            DisplayName = "李四"
        };

        SaveLifeAgentPatientBinding.InitializeConversation(conversation, patient);

        Assert.Null(conversation.CurrentPatientProfileId);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, conversation.FlowStateVersion);
    }

    [Fact]
    public void InitializeConversation_WithDeletedSelfPatient_DoesNotSelectPatient()
    {
        var conversation = CreateConversation(101, "openid-1");
        var patient = new SaveLifePatientProfile
        {
            Id = 501,
            OwnerExternalUserId = "openid-1",
            AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-1"),
            RelationshipToAccount = SaveLifePatientRelationships.Self,
            RelationshipDescription = "本人",
            DisplayName = "张三",
            IsDelete = true
        };

        SaveLifeAgentPatientBinding.InitializeConversation(conversation, patient);

        Assert.Null(conversation.CurrentPatientProfileId);
        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, conversation.FlowStateVersion);
    }

    [Fact]
    public void ConversationState_RoundTripsPatientScopedWorkflowWithoutChatTextParsing()
    {
        var state = new SaveLifeAgentConversationState
        {
            PendingQuestion = new SaveLifeAgentPendingQuestionState
            {
                Key = "city",
                Prompt = "患者目前在哪个城市？"
            },
            TrialOptOut = true,
            SelectedTrialId = 9001,
            PendingPatientSwitchProfileId = 502
        };

        var serialized = SaveLifeAgentConversationStateCodec.Serialize(state);
        var restored = SaveLifeAgentConversationStateCodec.Deserialize(serialized);

        Assert.Equal(SaveLifeAgentConversationState.CurrentVersion, restored.Version);
        Assert.Equal("city", restored.PendingQuestion?.Key);
        Assert.True(restored.TrialOptOut);
        Assert.Equal(9001, restored.SelectedTrialId);
        Assert.Equal(502, restored.PendingPatientSwitchProfileId);
    }

    private static SaveLifeUserProfile CreateUserProfile(long id, string externalUserId, string realName)
    {
        return new SaveLifeUserProfile
        {
            Id = id,
            ExternalUserId = externalUserId,
            RealName = realName,
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentConversation CreateConversation(long id, string externalUserId)
    {
        return new SaveLifeAgentConversation
        {
            Id = id,
            ExternalUserId = externalUserId,
            Status = "active",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentMessage CreateMessage(
        long id,
        long conversationId,
        string externalUserId,
        string role,
        string content)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            ConversationId = conversationId,
            ExternalUserId = externalUserId,
            Role = role,
            Content = content,
            CreateTime = new DateTime(2026, 7, 14)
        };
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
}
