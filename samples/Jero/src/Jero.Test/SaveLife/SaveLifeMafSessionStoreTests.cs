using System.Reflection;
using System.Security.Claims;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeMafSessionStoreTests
{
    [Fact]
    public async Task GetSessionAsync_ThreadOwnedByCurrentAccount_ReturnsSession()
    {
        await using var fixture = await SessionStoreFixture.CreateAsync(
            conversationOwner: "openid-1",
            requestOwner: "openid-1");

        var session = await fixture.Store.GetSessionAsync(
            fixture.Agent,
            "701",
            TestContext.Current.CancellationToken);

        Assert.NotNull(session);
    }

    [Fact]
    public async Task GetSessionAsync_ForeignThreadId_RejectsAccess()
    {
        await using var fixture = await SessionStoreFixture.CreateAsync(
            conversationOwner: "openid-2",
            requestOwner: "openid-1");

        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await fixture.Store.GetSessionAsync(
                fixture.Agent,
                "701",
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveSessionAsync_OwnedThread_PersistsSerializedSession()
    {
        await using var fixture = await SessionStoreFixture.CreateAsync(
            conversationOwner: "openid-1",
            requestOwner: "openid-1");
        var session = await fixture.Agent.CreateSessionAsync(
            TestContext.Current.CancellationToken);
        session.StateBag.SetValue("acceptance", "saved");

        await fixture.Store.SaveSessionAsync(
            fixture.Agent,
            "701",
            session,
            TestContext.Current.CancellationToken);

        var conversation = fixture.GetConversation();
        Assert.False(string.IsNullOrWhiteSpace(conversation.AgentSessionJson));
        Assert.Contains("acceptance", conversation.AgentSessionJson, StringComparison.Ordinal);
    }

    private sealed class SessionStoreFixture : IAsyncDisposable
    {
        private readonly string _databasePath;
        private readonly SqlSugarClient _database;
        private readonly ServiceProvider _services;

        private SessionStoreFixture(
            string databasePath,
            SqlSugarClient database,
            ServiceProvider services,
            SaveLifeMafSessionStore store,
            AIAgent agent)
        {
            _databasePath = databasePath;
            _database = database;
            _services = services;
            Store = store;
            Agent = agent;
        }

        public SaveLifeMafSessionStore Store { get; }

        public AIAgent Agent { get; }

        public SaveLifeAgentConversation GetConversation() =>
            _database.Queryable<SaveLifeAgentConversation>().InSingle(701);

        public static Task<SessionStoreFixture> CreateAsync(
            string conversationOwner,
            string requestOwner)
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            var database = CreateDatabase(databasePath);
            database.CodeFirst.InitTables<SaveLifeAgentConversation>();
            database.Insertable(new SaveLifeAgentConversation
            {
                Id = 701,
                ExternalUserId = conversationOwner,
                PatientContextVersion = 1,
                Status = "active",
                Phase = "created",
                CreateTime = new DateTime(2026, 7, 18)
            }).ExecuteCommand();

            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton(SqlSugarRepository<SaveLifeAgentConversation>.CreateForContext(database))
                .BuildServiceProvider();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = services,
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimConst.OpenId, requestOwner)],
                    "test"))
            };
            var store = new SaveLifeMafSessionStore(
                new HttpContextAccessor { HttpContext = httpContext });
            AIAgent agent = new ChatClientAgent(
                new NoopChatClient(),
                new ChatClientAgentOptions { Name = "session-store-test" });
            return Task.FromResult(new SessionStoreFixture(
                databasePath,
                database,
                services,
                store,
                agent));
        }

        public async ValueTask DisposeAsync()
        {
            await _services.DisposeAsync();
            _database.Dispose();
            if (File.Exists(_databasePath))
                File.Delete(_databasePath);
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

    private sealed class NoopChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<ChatResponseUpdate>();
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return serviceKey == null && serviceType.IsInstanceOfType(this) ? this : null;
        }

        public void Dispose()
        {
        }
    }
}
