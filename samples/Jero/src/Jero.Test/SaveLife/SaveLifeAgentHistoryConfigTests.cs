using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SqlSugar;
using Jero.Core;
using Jero.Core.Service;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentHistoryConfigTests
{
    [Fact]
    public void HistoryConfiguration_UsesTenRoundDefaultAndTwentyRoundMaximum()
    {
        Assert.Equal(10, SaveLifeAgentOptions.DefaultHistoryRounds);
        Assert.Equal(20, SaveLifeAgentOptions.MaximumHistoryRounds);
        Assert.Equal(10, new SaveLifeAgentOptions().MaxHistoryRounds);
        Assert.Null(new SaveLifeAgentConfigInput().MaxHistoryRounds);
        Assert.NotNull(typeof(SaveLifeAgentConfigOutput).GetProperty("MaxHistoryRounds"));
        Assert.Null(typeof(SaveLifeAgentOptions).GetProperty("MaxHistoryMessages"));
        Assert.Null(typeof(SaveLifeAgentConfigInput).GetProperty("MaxHistoryMessages"));
        Assert.Null(typeof(SaveLifeAgentConfigOutput).GetProperty("MaxHistoryMessages"));
    }

    [Fact]
    public async Task ConfigProvider_BoundsHistoryRoundsToSupportedRange()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = CreateProvider(db);

            var maximum = await provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false,
                MaxHistoryRounds = 99
            });
            var minimum = await provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false,
                MaxHistoryRounds = 0
            });

            Assert.Equal(20, maximum.MaxHistoryRounds);
            Assert.Equal(1, minimum.MaxHistoryRounds);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_RejectsInsecureRemoteModelEndpoints()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = CreateProvider(db);

            await Assert.ThrowsAnyAsync<Exception>(() => provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                ChatEndpoint = "http://chat.example.com/v1",
                Enabled = false
            }));
            await Assert.ThrowsAnyAsync<Exception>(() => provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                EmbeddingEndpoint = "http://embedding.example.com/v1",
                Enabled = false
            }));
            await Assert.ThrowsAnyAsync<Exception>(() => provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                MaterialEmbeddingEndpoint = "http://material.example.com/v1",
                Enabled = false
            }));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_AllowsHttpLoopbackModelEndpointsForLocalDevelopment()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = CreateProvider(db);

            var options = await provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                ChatEndpoint = "http://localhost:11434/v1",
                EmbeddingEndpoint = "http://127.0.0.1:11434/v1",
                Enabled = false,
                MaterialEmbeddingEndpoint = "http://[::1]:11434/v1"
            });

            Assert.Equal("http://localhost:11434/v1", options.GetChatEndpoint());
            Assert.Equal("http://127.0.0.1:11434/v1", options.GetEmbeddingEndpoint());
            Assert.Equal("http://[::1]:11434/v1", options.GetMaterialEmbeddingEndpoint());
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AgentClientFactory_RejectsInsecureEndpointsLoadedFromExistingConfiguration()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = new SaveLifeAgentConfigProvider(
                SqlSugarRepository<SysConfig>.CreateForContext(db),
                new SaveLifeAgentOptions
                {
                    ChatApiKey = "chat-secret",
                    ChatEndpoint = "http://chat.example.com/v1",
                    EmbeddingApiKey = "embedding-secret",
                    EmbeddingEndpoint = "http://embedding.example.com/v1",
                    Enabled = true
                });
            var factory = new SaveLifeAgentClientFactory(provider);

            await Assert.ThrowsAsync<InvalidOperationException>(factory.CreateChatClientAsync);
            await Assert.ThrowsAsync<InvalidOperationException>(factory.CreateEmbeddingClientAsync);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_MigratesLegacyMessageCountOnlyOnce()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            db.Insertable(new SysConfig
            {
                Id = 1,
                Code = "save_life_agent_max_history_messages",
                Name = "旧历史消息数",
                GroupCode = "SaveLifeAgent",
                Value = "12",
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();
            var provider = CreateProvider(db);

            var migrated = await provider.GetOptionsAsync();
            var newConfig = db.Queryable<SysConfig>()
                .Single(u => u.Code == "save_life_agent_max_history_rounds");

            Assert.Equal(6, migrated.MaxHistoryRounds);
            Assert.Equal("6", newConfig.Value);

            db.Updateable<SysConfig>()
                .SetColumns(u => u.Value == "40")
                .Where(u => u.Code == "save_life_agent_max_history_messages")
                .ExecuteCommand();

            var loadedAgain = await provider.GetOptionsAsync();
            var omittedByOlderClient = await provider.BuildOptionsAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false
            });

            Assert.Equal(6, loadedAgain.MaxHistoryRounds);
            Assert.Equal(6, omittedByOlderClient.MaxHistoryRounds);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_DecryptsSm4SecretWithoutApplicationHost()
    {
        const string testApiKey = "test-chat-key";
        const string existingSm4Key = "0123456789abcdeffedcba9876543210";
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            db.Insertable(new SysConfig
            {
                Id = 1,
                Code = "save_life_agent_chat_api_key",
                Name = "聊天模型测试密钥",
                GroupCode = "SaveLifeAgent",
                Value = $"sm4:{GMUtil.SM4EncryptECB(existingSm4Key, testApiKey)}",
                CreateTime = new DateTime(2026, 7, 15)
            }).ExecuteCommand();
            var provider = CreateProvider(db);

            var options = await provider.GetOptionsAsync();

            Assert.Equal(testApiKey, options.GetChatApiKey());
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_LongRoleAndPrompt_RoundTripsWithoutTruncationOrStaleChunks()
    {
        ConfigureSnowflakeIds();
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = CreateProvider(db);
            db.Insertable(new SysConfig
            {
                Name = "保留的扩展配置",
                Code = "save_life_agent_prompt_template",
                Value = "不得被分片清理覆盖",
                SysFlag = YesNoEnum.N,
                GroupCode = "SaveLifeAgent",
                OrderNo = 999,
                CreateTime = DateTime.Now
            }).ExecuteCommand();
            var mainRole = string.Concat(Enumerable.Repeat("主角色说明", 250));
            var prompt = new string('规', 511) + "😀" +
                string.Concat(Enumerable.Repeat("业务提示规则", 900));

            await provider.SaveConfigAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false,
                MainRole = mainRole,
                Prompt = prompt
            });

            var options = await provider.GetOptionsAsync();
            var output = await provider.GetConfigAsync();
            var promptConfigs = db.Queryable<SysConfig>()
                .Where(item => item.GroupCode == "SaveLifeAgent" &&
                    item.Code!.StartsWith("save_life_agent_prompt"))
                .ToList();

            Assert.Equal(mainRole, options.MainRole);
            Assert.Equal(prompt, options.Prompt);
            Assert.Equal(mainRole, output.MainRole);
            Assert.Equal(prompt, output.Prompt);
            Assert.True(promptConfigs.Count > 1);
            Assert.All(promptConfigs, item => Assert.InRange(item.Value?.Length ?? 0, 0, 512));

            await provider.SaveConfigAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false,
                MainRole = "精简主角色",
                Prompt = "精简提示词"
            });
            await provider.SaveConfigAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false
            });

            var shortened = await provider.GetOptionsAsync();
            var extensionConfig = db.Queryable<SysConfig>()
                .Single(item => item.Code == "save_life_agent_prompt_template");
            Assert.Equal("精简主角色", shortened.MainRole);
            Assert.Equal("精简提示词", shortened.Prompt);
            Assert.Equal("不得被分片清理覆盖", extensionConfig.Value);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_SaveConfig_CachesStaticInstructionsWithoutRequestContext()
    {
        ConfigureSnowflakeIds();
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        var cache = CreateCacheService();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = CreateProvider(db, cache);

            await provider.SaveConfigAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false,
                MainRole = "第一版主角色",
                Prompt = "第一版业务提示词"
            });
            await provider.SaveConfigAsync(new SaveLifeAgentConfigInput
            {
                Enabled = false,
                MainRole = "第二版主角色",
                Prompt = "第二版业务提示词"
            });

            var expected = SaveLifeMafAgent.BuildStaticInstructions(
                "第二版主角色",
                "第二版业务提示词");
            var cached = cache.Get<string>(SaveLifeAgentConfigProvider.InstructionsCacheKey);
            Assert.Equal(expected, cached);
            Assert.Contains(SaveLifeMafAgent.ToolInstructions, cached, StringComparison.Ordinal);

            db.Updateable<SysConfig>()
                .SetColumns(item => item.Value == "仅写入数据库的主角色")
                .Where(item => item.Code == "save_life_agent_main_role")
                .ExecuteCommand();
            db.Updateable<SysConfig>()
                .SetColumns(item => item.Value == "仅写入数据库的提示词")
                .Where(item => item.Code == "save_life_agent_prompt")
                .ExecuteCommand();

            var options = await provider.GetOptionsAsync();
            const string requestContext = "本轮可信上下文不得进入缓存";
            var requestOptions = SaveLifeMafChatClient.BuildChatOptions(
                new ChatOptions { Instructions = requestContext },
                options);

            Assert.Equal("仅写入数据库的主角色", options.MainRole);
            Assert.Equal(expected, options.Instructions);
            Assert.Equal($"{expected}\n\n{requestContext}", requestOptions.Instructions);
            Assert.DoesNotContain(
                requestContext,
                cache.Get<string>(SaveLifeAgentConfigProvider.InstructionsCacheKey) ?? string.Empty,
                StringComparison.Ordinal);
        }
        finally
        {
            cache.Remove(SaveLifeAgentConfigProvider.InstructionsCacheKey);
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ConfigProvider_PromptOverMaximumLength_IsRejected()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SysConfig>();
            var provider = CreateProvider(db);

            await Assert.ThrowsAnyAsync<Exception>(() => provider.BuildOptionsAsync(
                new SaveLifeAgentConfigInput
                {
                    Enabled = false,
                    MainRole = new string('角', 2001)
                }));
            await Assert.ThrowsAnyAsync<Exception>(() => provider.BuildOptionsAsync(
                new SaveLifeAgentConfigInput
                {
                    Enabled = false,
                    Prompt = new string('词', 8001)
                }));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void CryptogramUtil_Sm4Decrypt_DoesNotRequireApplicationHost()
    {
        const string testSecret = "test-secret";
        const string existingSm4Key = "0123456789abcdeffedcba9876543210";
        var cipherText = GMUtil.SM4EncryptECB(existingSm4Key, testSecret);

        var decrypted = CryptogramUtil.SM4DecryptECB(cipherText);

        Assert.Equal(testSecret, decrypted);
    }

    private static SaveLifeAgentConfigProvider CreateProvider(
        ISqlSugarClient db,
        SysCacheService? cache = null)
    {
        return new SaveLifeAgentConfigProvider(
            SqlSugarRepository<SysConfig>.CreateForContext(db),
            new SaveLifeAgentOptions(),
            cache);
    }

    private static SysCacheService CreateCacheService()
    {
        return new SysCacheService(
            new NewLife.Caching.CacheProvider(),
            Options.Create(new CacheOptions
            {
                Prefix = $"save-life-agent-tests:{Guid.NewGuid():N}:"
            }));
    }

    private static void ConfigureSnowflakeIds()
    {
        var options = new Yitter.IdGenerator.IdGeneratorOptions
        {
            WorkerId = 1,
            WorkerIdBitLength = 6,
            SeqBitLength = 6
        };
        Yitter.IdGenerator.YitIdHelper.SetIdGenerator(options);
        SnowFlakeSingle.WorkId = options.WorkerId;
        StaticConfig.CustomSnowFlakeFunc = Yitter.IdGenerator.YitIdHelper.NextId;
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        var db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={dbPath};Pooling=False",
            DbType = DbType.Sqlite,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                        column.IsNullable = true;
                }
            }
        });
        db.Aop.DataExecuting = (_, entityInfo) =>
        {
            if (entityInfo.OperationType != DataFilterType.InsertByObject ||
                !entityInfo.EntityColumnInfo.IsPrimarykey ||
                entityInfo.EntityColumnInfo.IsIdentity ||
                entityInfo.EntityColumnInfo.PropertyInfo.PropertyType != typeof(long))
            {
                return;
            }

            var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
            if (id == null || (long)id == 0)
                entityInfo.SetValue(Yitter.IdGenerator.YitIdHelper.NextId());
        };
        return db;
    }

}
