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

public sealed class SaveLifeMafRequestContextFallbackTests
{
    [Fact]
    public async Task BuildAgentMessages_LegacyPatientContext_DoesNotBlockAccountOwnedEnrollment()
    {
        await using var fixture = await RequestContextFixture.CreateAsync();
        var options = fixture.CreateOptions(
            businessContext: new
            {
                contextId = "801",
                contextType = "enrollment",
                enrollmentId = "801",
                patientProfileId = "502",
                title = "其他患者的报名记录"
            });
        await fixture.BeginTurnAsync(options);

        var messages = await fixture.RequestContext.BuildAgentMessagesAsync(
            [],
            session: null,
            TestContext.Current.CancellationToken);

        Assert.Contains(messages, message =>
            message.Role == ChatRole.User && message.Text == "腰椎间盘突出怎么治疗");
        Assert.NotNull(fixture.RequestContext.BusinessContext);
        Assert.Equal("enrollment", fixture.RequestContext.BusinessContext.ContextType);
        Assert.Equal("801", fixture.RequestContext.BusinessContext.ContextId);
        var instructions = fixture.RequestContext.BuildVerifiedContextInstructions();
        Assert.DoesNotContain("患者不一致", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("切换患者", instructions, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BeginTurn_LegacySelectedPatient_UsesMinimizedAccountAndSelfHealthContext()
    {
        await using var fixture = await RequestContextFixture.CreateAsync();
        await fixture.BeginTurnAsync(fixture.CreateOptions());

        var instructions = fixture.RequestContext.BuildVerifiedContextInstructions();

        Assert.Contains("当前登录用户的个人概况", instructions, StringComparison.Ordinal);
        Assert.Contains("年龄段：30-39岁", instructions, StringComparison.Ordinal);
        Assert.Contains("当前登录用户的健康档案", instructions, StringComparison.Ordinal);
        Assert.Contains("城市：成都", instructions, StringComparison.Ordinal);
        Assert.Contains("诊断：肺癌", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("张三", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("1988-01-01", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("13800000000", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("zhangsan@example.test", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("四川省成都市武侯区人民南路1号", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("汉族", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("李四", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("13900000000", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("母亲", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("乳腺癌", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("关系类型", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("与账号关系", instructions, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BeginTurn_NewConversationWithoutLegacyPatientBinding_StillUsesAccountSelfHealthProfile()
    {
        await using var fixture = await RequestContextFixture.CreateAsync(
            conversationPatientProfileId: null);
        await fixture.BeginTurnAsync(fixture.CreateOptions());

        var instructions = fixture.RequestContext.BuildVerifiedContextInstructions();

        Assert.Contains("当前登录用户的健康档案", instructions, StringComparison.Ordinal);
        Assert.Contains("诊断：肺癌", instructions, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BeginTurn_NormalAgUiResume_RebindsLegacyConversationToAccountSelfProfile()
    {
        await using var fixture = await RequestContextFixture.CreateAsync();

        await fixture.BeginTurnAsync(fixture.CreateOptions());

        var conversation = await fixture.Database.Queryable<SaveLifeAgentConversation>()
            .Where(item => item.Id == 701)
            .FirstAsync(TestContext.Current.CancellationToken);

        Assert.Equal(501, conversation.CurrentPatientProfileId);
    }

    [Fact]
    public async Task BuildAgentMessages_OtherAccountsEnrollment_IsNotAddedToModelContext()
    {
        await using var fixture = await RequestContextFixture.CreateAsync();
        await fixture.BeginTurnAsync(fixture.CreateOptions(
            businessContext: new
            {
                contextId = "802",
                contextType = "enrollment",
                enrollmentId = "802"
            }));

        await fixture.RequestContext.BuildAgentMessagesAsync(
            [],
            session: null,
            TestContext.Current.CancellationToken);

        Assert.Null(fixture.RequestContext.BusinessContext);
        var instructions = fixture.RequestContext.BuildVerifiedContextInstructions();
        Assert.Contains("本轮页面上下文不可用，已忽略", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("其他账号项目", instructions, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildAgentMessages_SelectedAttachmentUnavailable_StopsBeforeModelInvocation()
    {
        await using var fixture = await RequestContextFixture.CreateAsync(
            new InvalidOperationException("material lookup unavailable"));
        var options = fixture.CreateOptions(materialIds: ["9001"]);
        await fixture.BeginTurnAsync(options);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.RequestContext.BuildAgentMessagesAsync(
                [],
                session: null,
                TestContext.Current.CancellationToken));

        Assert.Equal("material lookup unavailable", error.Message);
        Assert.Null(fixture.RequestContext.BuildAuthorizedMaterialContextMessages());
    }

    [Fact]
    public async Task BuildAgentMessages_ClientReportedUnavailableAttachment_PreservesUserMessage()
    {
        await using var fixture = await RequestContextFixture.CreateAsync();
        var options = fixture.CreateOptions(attachmentUnavailable: true);
        await fixture.BeginTurnAsync(options);

        var messages = await fixture.RequestContext.BuildAgentMessagesAsync(
            [],
            session: null,
            TestContext.Current.CancellationToken);

        Assert.Contains(messages, message =>
            message.Role == ChatRole.User && message.Text == "腰椎间盘突出怎么治疗");
        Assert.Contains(
            "本轮所选附件不可用，未向模型提供附件内容",
            fixture.RequestContext.BuildVerifiedContextInstructions(),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildAgentMessages_RequestCancelled_DoesNotDegradeCancellation()
    {
        await using var fixture = await RequestContextFixture.CreateAsync();
        await fixture.BeginTurnAsync(fixture.CreateOptions(materialIds: ["9001"]));
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            fixture.RequestContext.BuildAgentMessagesAsync([], session: null, cancellation.Token));
    }

    private sealed class RequestContextFixture : IAsyncDisposable
    {
        private readonly SqlSugarClient _database;
        private readonly string _databasePath;

        private RequestContextFixture(
            string databasePath,
            SqlSugarClient database,
            SaveLifeMafRequestContext requestContext)
        {
            _databasePath = databasePath;
            _database = database;
            RequestContext = requestContext;
        }

        public SaveLifeMafRequestContext RequestContext { get; }

        public SqlSugarClient Database => _database;

        public static Task<RequestContextFixture> CreateAsync(
            Exception? retrievalException = null,
            long? conversationPatientProfileId = 502)
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            var database = CreateDatabase(databasePath);
            database.CodeFirst.InitTables(
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage),
                typeof(SaveLifeUserProfile),
                typeof(SaveLifePatientProfile),
                typeof(SaveLifeClinicalTrial),
                typeof(SaveLifeDiseaseKnowledge),
                typeof(SaveLifeEnrollmentRecord));
            database.Insertable(new SaveLifeUserProfile
            {
                Id = 401,
                ExternalUserId = "openid-1",
                RealName = "张三",
                Gender = "男",
                BirthDate = new DateTime(1988, 1, 1),
                Phone = "13800000000",
                Email = "zhangsan@example.test",
                Address = "四川省成都市武侯区人民南路1号",
                Ethnicity = "汉族",
                EmergencyContact = "李四",
                EmergencyPhone = "13900000000",
                CreateTime = new DateTime(2026, 7, 18)
            }).ExecuteCommand();
            database.Insertable(new[]
            {
                new SaveLifePatientProfile
                {
                    Id = 501,
                    OwnerExternalUserId = "openid-1",
                    AccountPatientKey = SaveLifePatientProfileKeys.ForSelf("openid-1"),
                    DisplayName = "张三",
                    RelationshipToAccount = SaveLifePatientRelationships.Self,
                    RelationshipDescription = "本人",
                    Gender = "男",
                    BirthDate = new DateTime(1988, 1, 1),
                    City = "成都",
                    Diagnosis = "肺癌",
                    CreateTime = new DateTime(2026, 7, 18)
                },
                new SaveLifePatientProfile
                {
                    Id = 502,
                    OwnerExternalUserId = "openid-1",
                    AccountPatientKey = SaveLifePatientProfileKeys.ForOther(502),
                    DisplayName = "母亲",
                    RelationshipToAccount = SaveLifePatientRelationships.Parent,
                    RelationshipDescription = "母亲",
                    Diagnosis = "乳腺癌",
                    IsDefault = true,
                    CreateTime = new DateTime(2026, 7, 18)
                }
            }).ExecuteCommand();
            database.Insertable(new SaveLifeEnrollmentRecord
            {
                Id = 801,
                ExternalUserId = "openid-1",
                PatientProfileId = 502,
                ProjectId = 901,
                ProjectTitle = "账号已报名项目",
                Status = "pending",
                CreateTime = new DateTime(2026, 7, 18)
            }).ExecuteCommand();
            database.Insertable(new SaveLifeEnrollmentRecord
            {
                Id = 802,
                ExternalUserId = "openid-2",
                PatientProfileId = 999,
                ProjectId = 902,
                ProjectTitle = "其他账号项目",
                Status = "pending",
                CreateTime = new DateTime(2026, 7, 18)
            }).ExecuteCommand();
            database.Insertable(new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = "openid-1",
                CurrentPatientProfileId = conversationPatientProfileId,
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
                new StubRetrievalService(retrievalException),
                new StubMaterialModelInputService(),
                new StubClientFactory(),
                NullLogger<SaveLifeMafRequestContext>.Instance);
            long nextId = 1000;
            requestContext.NextId = () => Interlocked.Increment(ref nextId);
            return Task.FromResult(new RequestContextFixture(databasePath, database, requestContext));
        }

        public ChatOptions CreateOptions(
            object? businessContext = null,
            IReadOnlyList<string>? materialIds = null,
            bool attachmentUnavailable = false)
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
            if (materialIds != null || attachmentUnavailable)
            {
                input.ForwardedProperties = JsonSerializer.SerializeToElement(new { saveLife = new { attachmentUnavailable, materialIds } });
            }
            return input.ToChatRequestContext(JsonSerializerOptions.Web).ChatOptions;
        }

        public Task BeginTurnAsync(ChatOptions options) => RequestContext.BeginTurnAsync(
            [new ChatMessage(ChatRole.User, "腰椎间盘突出怎么治疗") { MessageId = "client-1" }],
            options,
            TestContext.Current.CancellationToken);

        public async ValueTask DisposeAsync()
        {
            await RequestContext.DisposePreparedInputAsync();
            _database.Dispose();
            if (File.Exists(_databasePath)) File.Delete(_databasePath);
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

    private sealed class StubRetrievalService(Exception? exception) : ISaveLifeAgentRetrievalService
    {
        public Task<SaveLifeAgentRetrievalResult> BuildRetrievalAsync(
            string query,
            SaveLifeAgentPatientSignal signal,
            string? externalUserId = null,
            long? enrollmentRecordId = null,
            CancellationToken cancellationToken = default,
            SaveLifeAgentRetrievalScope? scope = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return exception == null
                ? Task.FromResult(new SaveLifeAgentRetrievalResult())
                : Task.FromException<SaveLifeAgentRetrievalResult>(exception);
        }

        public Task SyncKnowledgeIndexAsync(SaveLifeDiseaseKnowledge knowledge) => throw new NotSupportedException();

        public Task SyncTrialIndexAsync(SaveLifeClinicalTrial trial) => throw new NotSupportedException();

        public Task SyncMaterialIndexAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>>? openFileStream = null) => throw new NotSupportedException();

        public Task DeleteMaterialIndexAsync(long materialId, string externalUserId) => throw new NotSupportedException();
    }

    private sealed class StubPrivacyConsentGuard : ISaveLifePrivacyConsentGuard
    {
        public Task EnsureConsentedAsync(string? externalUserId = null) => Task.CompletedTask;

        public Task RecordConsentAsync(string externalUserId, string? privacyConsent) => throw new NotSupportedException();

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
            CancellationToken cancellationToken = default) =>
            Task.FromResult(SaveLifeAgentPreparedMaterialInput.Empty);
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
