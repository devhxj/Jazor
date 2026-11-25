using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeAgentLiveCapabilityTests
{
    private const string EnabledVariable = "SAVELIFE_LIVE_CAPABILITY_EVAL";
    private const string ConfigDatabaseVariable = "SAVELIFE_LIVE_CONFIG_DB";
    private const string ReportPathVariable = "SAVELIFE_LIVE_REPORT_PATH";

    private static readonly string[] RequiredTargets =
    [
        "chatText",
        "chatImage",
        "embedding",
        "materialTextEmbedding",
        "materialImageEmbedding"
    ];

    [Fact]
    [Trait("Category", "Live")]
    public async Task CurrentProductionConfiguration_AllRequiredCapabilitiesSucceed()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(EnabledVariable),
                "1",
                StringComparison.Ordinal))
        {
            Assert.Skip($"设置 {EnabledVariable}=1 后运行当前生产配置能力验证。");
        }

        var databasePath = RequireFileEnvironmentVariable(ConfigDatabaseVariable);
        var originalHash = await ComputeFileHashAsync(databasePath);
        SaveLifeAgentConfigTestOutput result;
        SaveLifeAgentOptions options;

        using (var db = CreateReadOnlySqliteClient(databasePath))
        using (var httpClientFactory = new LiveHttpClientFactory())
        {
            var provider = new SaveLifeAgentConfigProvider(
                SqlSugarRepository<SysConfig>.CreateForContext(db),
                new SaveLifeAgentOptions());
            options = await provider.GetOptionsAsync();
            Assert.Equal(SaveLifeAgentOptions.DefaultChatModel, options.ChatModel);
            Assert.Equal(
                SaveLifeAgentOptions.DefaultMaterialEmbeddingModel,
                options.MaterialEmbeddingModel);
            Assert.True(options.HasChatConfiguration, "当前只读配置缺少独立 Chat 密钥。");
            Assert.True(options.HasEmbeddingConfiguration, "当前只读配置缺少公共文本向量密钥。");
            Assert.True(options.HasMaterialEmbeddingConfiguration, "当前只读配置缺少独立病例向量密钥。");

            var service = new SaveLifeAgentConfigService(provider, httpClientFactory);
            result = await service.TestConfig(ToInput(options));
        }

        Assert.Equal(originalHash, await ComputeFileHashAsync(databasePath));
        Assert.Equal(RequiredTargets, result.Items.Select(item => item.Target));
        await WriteReportAsync(options, result);

        var failures = result.Items.Where(item => !item.Success).ToList();
        Assert.True(
            result.Success && failures.Count == 0,
            string.Join(
                Environment.NewLine,
                failures.Select(item => $"{item.Target}: {item.Message}")));
    }

    private static SaveLifeAgentConfigInput ToInput(SaveLifeAgentOptions options) => new()
    {
        ChatEndpoint = options.GetChatEndpoint(),
        ChatModel = options.ChatModel,
        ChatTimeoutSeconds = options.ChatTimeoutSeconds,
        EmbeddingEndpoint = options.GetEmbeddingEndpoint(),
        EmbeddingModel = options.EmbeddingModel,
        Enabled = options.Enabled,
        MaterialEmbeddingDimensions = options.MaterialEmbeddingDimensions,
        MaterialEmbeddingEndpoint = options.GetMaterialEmbeddingEndpoint(),
        MaterialEmbeddingModel = options.MaterialEmbeddingModel,
        MaxHistoryRounds = options.MaxHistoryRounds,
        MaxKnowledgeReferences = options.MaxKnowledgeReferences,
        MaxTrialReferences = options.MaxTrialReferences,
        MinVectorScore = options.MinVectorScore
    };

    private static SqlSugarClient CreateReadOnlySqliteClient(string path)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                        column.IsNullable = true;
                }
            },
            ConnectionString = $"Data Source={path};Mode=ReadOnly;Pooling=False",
            DbType = DbType.Sqlite,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true
        });
    }

    private static string RequireFileEnvironmentVariable(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable)?.Trim();
        Assert.False(string.IsNullOrWhiteSpace(value), $"环境变量 {variable} 未设置。");
        var path = ResolveRepositoryPath(value!);
        Assert.True(File.Exists(path), $"环境变量 {variable} 指向的文件不存在。");
        return path;
    }

    private static string ResolveRepositoryPath(string path)
    {
        if (Path.IsPathFullyQualified(path))
            return Path.GetFullPath(path);

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "Jero.slnx")))
                return Path.GetFullPath(Path.Combine(directory.FullName, path));

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("无法定位仓库根目录。");
    }

    private static async Task<string> ComputeFileHashAsync(string path)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            useAsync: true);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream));
    }

    private static async Task WriteReportAsync(
        SaveLifeAgentOptions options,
        SaveLifeAgentConfigTestOutput result)
    {
        var reportPath = Environment.GetEnvironmentVariable(ReportPathVariable)?.Trim();
        Assert.False(string.IsNullOrWhiteSpace(reportPath), $"环境变量 {ReportPathVariable} 未设置。");
        var fullPath = ResolveRepositoryPath(reportPath!);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var report = new
        {
            ChatEndpoint = new Uri(options.GetChatEndpoint()!).GetLeftPart(UriPartial.Path).TrimEnd('/'),
            ChatModel = options.ChatModel,
            GeneratedAtUtc = DateTime.UtcNow,
            MaterialEmbeddingDimensions = options.MaterialEmbeddingDimensions,
            MaterialEmbeddingEndpoint = new Uri(options.GetMaterialEmbeddingEndpoint()!).GetLeftPart(UriPartial.Path).TrimEnd('/'),
            MaterialEmbeddingModel = options.MaterialEmbeddingModel,
            Passed = result.Success,
            Results = result.Items.Select(item => new
            {
                item.ElapsedMilliseconds,
                item.Skipped,
                item.Success,
                item.Target
            })
        };
        await File.WriteAllTextAsync(
            fullPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class LiveHttpClientFactory : IHttpClientFactory, IDisposable
    {
        private readonly HttpClient _client = new()
        {
            Timeout = TimeSpan.FromSeconds(90)
        };

        public HttpClient CreateClient(string name) => _client;

        public void Dispose() => _client.Dispose();
    }
}
