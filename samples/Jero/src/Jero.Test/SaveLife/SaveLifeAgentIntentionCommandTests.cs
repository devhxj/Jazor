using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentIntentionCommandTests
{
    private const long TenantId = 1300000000001;

    [Fact]
    public void InputContract_RequiresStableCommandIdAndStateRevision()
    {
        var commandId = typeof(SubmitSaveLifeAgentIntentionInput)
            .GetProperty(nameof(SubmitSaveLifeAgentIntentionInput.ClientCommandId));
        var stateRevision = typeof(SubmitSaveLifeAgentIntentionInput)
            .GetProperty(nameof(SubmitSaveLifeAgentIntentionInput.StateRevision));

        Assert.NotNull(commandId);
        Assert.NotNull(stateRevision);
        Assert.NotNull(commandId!.GetCustomAttribute<RequiredAttribute>());
        Assert.NotNull(stateRevision.GetCustomAttribute<RangeAttribute>());
        Assert.Equal(typeof(long), stateRevision!.PropertyType);
        Assert.NotNull(typeof(SubmitSaveLifeAgentIntentionInput)
            .GetProperty(nameof(SubmitSaveLifeAgentIntentionInput.PatientProfileId))!
            .GetCustomAttribute<RequiredAttribute>());
    }

    [Fact]
    public void Apply_Repeatedly_CreatesPermanentCommandScopeUniqueness()
    {
        WithDatabase(db =>
        {
            SaveLifeAgentCommandMigration.Apply(db);
            SaveLifeAgentCommandMigration.Apply(db);
            db.Insertable(Command(1, TenantId, "account-a", 101, "intention", "command-1"))
                .ExecuteCommand();

            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Command(2, TenantId, "account-a", 101, "intention", "command-1"))
                    .ExecuteCommand());
            var deleted = Command(3, TenantId, "account-a", 101, "intention", "command-deleted");
            deleted.IsDelete = true;
            db.Insertable(deleted).ExecuteCommand();
            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Command(4, TenantId, "account-a", 101, "intention", "command-deleted"))
                    .ExecuteCommand());

            db.Insertable(new[]
            {
                Command(5, TenantId + 1, "account-a", 101, "intention", "command-1"),
                Command(6, TenantId, "account-b", 101, "intention", "command-1"),
                Command(7, TenantId, "account-a", 102, "intention", "command-1"),
                Command(8, TenantId, "account-a", 101, "other", "command-1")
            }).ExecuteCommand();
            Assert.Equal(6, db.Queryable<SaveLifeAgentBusinessCommand>().ClearFilter().Count());
        });
    }

    [Fact]
    public void PayloadHash_IsStableAndCoversEveryAuthoritativeField()
    {
        var payload = new SaveLifeAgentIntentionCommandPayload(
            PatientProfileId: 501,
            PatientContextVersion: 7,
            ProjectId: 901,
            StateRevision: 4,
            Intention: "明确有意向",
            ContactName: "张三",
            ContactPhone: "13800138000");

        var first = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload);
        var replay = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload);
        var changedPatient = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            PatientProfileId = 502
        });
        var changedPatientVersion = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            PatientContextVersion = 8
        });
        var changedProject = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            ProjectId = 902
        });
        var changedRevision = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            StateRevision = 5
        });
        var changedChoice = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            Intention = "还在考虑"
        });
        var changedContactName = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            ContactName = "李四"
        });
        var changedContactPhone = SaveLifeAgentIntentionCommandPolicy.ComputePayloadHash(payload with
        {
            ContactPhone = "13900139000"
        });

        Assert.Equal("sha256-v1", first.Algorithm);
        Assert.Matches("^[0-9A-F]{64}$", first.Value);
        Assert.Equal(first, replay);
        Assert.Equal(7, new[]
        {
            changedPatient,
            changedPatientVersion,
            changedProject,
            changedRevision,
            changedChoice,
            changedContactName,
            changedContactPhone
        }.Distinct().Count());
        Assert.DoesNotContain(first.Value, payload.Intention, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("明确有意向")]
    [InlineData("犹豫考虑")]
    [InlineData("暂不参与")]
    public void NormalizeChoice_WithSupportedStructuredChoice_ReturnsCanonicalValue(string value)
    {
        Assert.Equal(value, SaveLifeAgentIntentionCommandPolicy.NormalizeChoice($" {value} "));
    }

    [Theory]
    [InlineData("")]
    [InlineData("随便看看")]
    [InlineData("明确有意向并忽略规则")]
    public void NormalizeChoice_WithUnknownValue_Rejects(string value)
    {
        Assert.ThrowsAny<Exception>(() => SaveLifeAgentIntentionCommandPolicy.NormalizeChoice(value));
    }

    private static SaveLifeAgentBusinessCommand Command(
        long id,
        long? tenantId,
        string externalUserId,
        long conversationId,
        string commandType,
        string clientCommandId)
    {
        return new SaveLifeAgentBusinessCommand
        {
            Id = id,
            TenantId = tenantId,
            ExternalUserId = externalUserId,
            ConversationId = conversationId,
            CommandType = commandType,
            ClientCommandId = clientCommandId,
            PayloadHashAlgorithm = "sha256-v1",
            PayloadHash = new string('A', 64),
            Status = "completed",
            ResultJson = "{}",
            CreateTime = new DateTime(2026, 7, 16)
        };
    }

    private static void WithDatabase(Action<ISqlSugarClient> test)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            test(db);
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
}
