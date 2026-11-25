using System.Runtime.ExceptionServices;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentConfigProvider
{
    Task<SaveLifeAgentOptions> GetOptionsAsync();

    Task<SaveLifeAgentOptions> BuildOptionsAsync(SaveLifeAgentConfigInput input);

    Task<SaveLifeAgentConfigOutput> GetConfigAsync();

    Task SaveConfigAsync(SaveLifeAgentConfigInput input);
}

public sealed class SaveLifeAgentConfigProvider : ISaveLifeAgentConfigProvider, ITransient
{
    internal const string InstructionsCacheKey = "save_life_agent_instructions:v1";

    private const string GroupCode = "SaveLifeAgent";
    private const string EnabledCode = "save_life_agent_enabled";
    private const string MainRoleCode = "save_life_agent_main_role";
    private const string PromptCode = "save_life_agent_prompt";
    private const string ChatEndpointCode = "save_life_agent_chat_endpoint";
    private const string ChatApiKeyCode = "save_life_agent_chat_api_key";
    private const string ChatModelCode = "save_life_agent_chat_model";
    private const string ChatTimeoutSecondsCode = "save_life_agent_chat_timeout_seconds";
    private const string EmbeddingEndpointCode = "save_life_agent_embedding_endpoint";
    private const string EmbeddingApiKeyCode = "save_life_agent_embedding_api_key";
    private const string EmbeddingModelCode = "save_life_agent_embedding_model";
    private const string MaterialEmbeddingEndpointCode = "save_life_agent_material_embedding_endpoint";
    private const string MaterialEmbeddingApiKeyCode = "save_life_agent_material_embedding_api_key";
    private const string MaterialEmbeddingModelCode = "save_life_agent_material_embedding_model";
    private const string MaterialEmbeddingDimensionsCode = "save_life_agent_material_embedding_dimensions";
    private const string MaxHistoryRoundsCode = "save_life_agent_max_history_rounds";
    private const string LegacyMaxHistoryMessagesCode = "save_life_agent_max_history_messages";
    private const string MaxKnowledgeReferencesCode = "save_life_agent_max_knowledge_references";
    private const string MaxTrialReferencesCode = "save_life_agent_max_trial_references";
    private const string MinVectorScoreCode = "save_life_agent_min_vector_score";
    private const string SecretPrefix = "sm4:";
    private const int MaxConfigValueLength = 512;
    private const int MaxSecretBytes = 239;
    private static readonly TimeSpan InstructionsCacheDuration = TimeSpan.FromMinutes(5);

    private readonly SqlSugarRepository<SysConfig> _configRep;
    private readonly SysCacheService? _cacheService;
    private readonly SaveLifeAgentOptions? _defaultOptions;

    public SaveLifeAgentConfigProvider(
        SqlSugarRepository<SysConfig> configRep,
        SysCacheService cacheService)
    {
        _configRep = configRep;
        _cacheService = cacheService;
    }

    internal SaveLifeAgentConfigProvider(
        SqlSugarRepository<SysConfig> configRep,
        SaveLifeAgentOptions defaultOptions,
        SysCacheService? cacheService = null)
    {
        _configRep = configRep;
        _defaultOptions = defaultOptions;
        _cacheService = cacheService;
    }

    // 模型配置仍实时读取数据库；这里只缓存不含密钥和请求上下文的静态提示词。
    public async Task<SaveLifeAgentOptions> GetOptionsAsync()
    {
        var options = await LoadOptionsAsync();
        options.SetInstructions(GetOrCreateInstructions(options));
        return options;
    }

    public async Task<SaveLifeAgentConfigOutput> GetConfigAsync()
    {
        var options = await GetOptionsAsync();
        var chatApiKey = options.GetChatApiKey();
        var embeddingApiKey = options.GetEmbeddingApiKey();
        var materialEmbeddingApiKey = options.GetMaterialEmbeddingApiKey();

        return new SaveLifeAgentConfigOutput
        {
            Enabled = options.Enabled,
            MainRole = options.MainRole,
            Prompt = options.Prompt,
            ChatEndpoint = options.GetChatEndpoint(),
            HasChatApiKey = !string.IsNullOrWhiteSpace(chatApiKey),
            ChatApiKeyMasked = MaskApiKey(chatApiKey),
            ChatModel = options.ChatModel,
            CanUseChat = options.HasChatConfiguration,
            ChatTimeoutSeconds = options.ChatTimeoutSeconds,
            EmbeddingEndpoint = options.GetEmbeddingEndpoint(),
            HasEmbeddingApiKey = !string.IsNullOrWhiteSpace(embeddingApiKey),
            EmbeddingApiKeyMasked = MaskApiKey(embeddingApiKey),
            EmbeddingModel = options.EmbeddingModel,
            CanUseEmbeddings = options.HasEmbeddingConfiguration,
            MaterialEmbeddingEndpoint = options.GetMaterialEmbeddingEndpoint(),
            HasMaterialEmbeddingApiKey = !string.IsNullOrWhiteSpace(materialEmbeddingApiKey),
            MaterialEmbeddingApiKeyMasked = MaskApiKey(materialEmbeddingApiKey),
            MaterialEmbeddingModel = options.MaterialEmbeddingModel,
            MaterialEmbeddingDimensions = options.MaterialEmbeddingDimensions,
            CanUseMaterialEmbedding = options.HasMaterialEmbeddingConfiguration,
            MaxHistoryRounds = options.MaxHistoryRounds,
            MaxKnowledgeReferences = options.MaxKnowledgeReferences,
            MaxTrialReferences = options.MaxTrialReferences,
            MinVectorScore = options.MinVectorScore
        };
    }

    public async Task<SaveLifeAgentOptions> BuildOptionsAsync(SaveLifeAgentConfigInput input)
    {
        ValidateInput(input);

        var current = await LoadOptionsAsync();
        return new SaveLifeAgentOptions
        {
            Enabled = input.Enabled,
            MainRole = Normalize(input.MainRole) ?? current.MainRole,
            Prompt = Normalize(input.Prompt) ?? current.Prompt,
            Endpoint = current.Endpoint,
            ApiKey = current.ApiKey,
            ChatEndpoint = Normalize(input.ChatEndpoint) ?? current.GetChatEndpoint(),
            ChatApiKey = Normalize(input.ChatApiKey) ?? current.GetChatApiKey(),
            ChatModel = Normalize(input.ChatModel) ?? current.ChatModel,
            ChatTimeoutSeconds = Clamp(input.ChatTimeoutSeconds, 1, 300),
            EmbeddingEndpoint = Normalize(input.EmbeddingEndpoint) ?? current.GetEmbeddingEndpoint(),
            EmbeddingApiKey = Normalize(input.EmbeddingApiKey) ?? current.GetEmbeddingApiKey(),
            EmbeddingModel = Normalize(input.EmbeddingModel) ?? current.EmbeddingModel,
            MaterialEmbeddingEndpoint = Normalize(input.MaterialEmbeddingEndpoint) ?? current.GetMaterialEmbeddingEndpoint(),
            MaterialEmbeddingApiKey = Normalize(input.MaterialEmbeddingApiKey) ?? current.GetMaterialEmbeddingApiKey(),
            MaterialEmbeddingModel = Normalize(input.MaterialEmbeddingModel) ?? current.MaterialEmbeddingModel,
            MaterialEmbeddingDimensions = Clamp(input.MaterialEmbeddingDimensions, 256, 4096),
            MaxHistoryRounds = Clamp(
                input.MaxHistoryRounds ?? current.MaxHistoryRounds,
                1,
                SaveLifeAgentOptions.MaximumHistoryRounds),
            MaxKnowledgeReferences = Clamp(input.MaxKnowledgeReferences, 1, 20),
            MaxTrialReferences = Clamp(input.MaxTrialReferences, 1, 20),
            MinVectorScore = Math.Clamp(input.MinVectorScore, 0, 1)
        };
    }

    public async Task SaveConfigAsync(SaveLifeAgentConfigInput input)
    {
        ValidateInput(input);

        var transaction = await _configRep.Context.Ado.UseTranAsync(async () =>
        {
            var current = await LoadOptionsAsync();
            var values = new Dictionary<string, string?>
            {
                [EnabledCode] = input.Enabled.ToString(),
                [ChatEndpointCode] = Normalize(input.ChatEndpoint),
                [ChatModelCode] = Normalize(input.ChatModel) ?? current.ChatModel,
                [ChatTimeoutSecondsCode] = Clamp(input.ChatTimeoutSeconds, 1, 300).ToString(),
                [EmbeddingEndpointCode] = Normalize(input.EmbeddingEndpoint),
                [EmbeddingModelCode] = Normalize(input.EmbeddingModel) ?? current.EmbeddingModel,
                [MaterialEmbeddingEndpointCode] = Normalize(input.MaterialEmbeddingEndpoint),
                [MaterialEmbeddingModelCode] = Normalize(input.MaterialEmbeddingModel) ?? current.MaterialEmbeddingModel,
                [MaterialEmbeddingDimensionsCode] = Clamp(input.MaterialEmbeddingDimensions, 256, 4096).ToString(),
                [MaxHistoryRoundsCode] = Clamp(
                    input.MaxHistoryRounds ?? current.MaxHistoryRounds,
                    1,
                    SaveLifeAgentOptions.MaximumHistoryRounds).ToString(),
                [MaxKnowledgeReferencesCode] = Clamp(input.MaxKnowledgeReferences, 1, 20).ToString(),
                [MaxTrialReferencesCode] = Clamp(input.MaxTrialReferences, 1, 20).ToString(),
                [MinVectorScoreCode] = Math.Clamp(input.MinVectorScore, 0, 1).ToString("0.####")
            };

            var chatApiKey = Normalize(input.ChatApiKey);
            if (!string.IsNullOrWhiteSpace(chatApiKey))
                values[ChatApiKeyCode] = ProtectSecret(chatApiKey);

            var embeddingApiKey = Normalize(input.EmbeddingApiKey);
            if (!string.IsNullOrWhiteSpace(embeddingApiKey))
                values[EmbeddingApiKeyCode] = ProtectSecret(embeddingApiKey);

            var materialEmbeddingApiKey = Normalize(input.MaterialEmbeddingApiKey);
            if (!string.IsNullOrWhiteSpace(materialEmbeddingApiKey))
                values[MaterialEmbeddingApiKeyCode] = ProtectSecret(materialEmbeddingApiKey);

            foreach (var item in values)
            {
                await UpsertConfigAsync(item.Key, item.Value ?? string.Empty);
            }

            var mainRole = Normalize(input.MainRole);
            if (mainRole != null)
                await UpsertChunkedConfigAsync(MainRoleCode, mainRole);

            var prompt = Normalize(input.Prompt);
            if (prompt != null)
                await UpsertChunkedConfigAsync(PromptCode, prompt);

            return SaveLifeMafAgent.BuildStaticInstructions(
                mainRole ?? current.MainRole,
                prompt ?? current.Prompt);
        }, _ => { });

        if (!transaction.IsSuccess)
        {
            ExceptionDispatchInfo.Capture(
                transaction.ErrorException ?? new InvalidOperationException("智能助手配置保存事务失败"))
                .Throw();
        }

        CacheInstructions(
            transaction.Data ?? throw new InvalidOperationException("智能助手提示词生成失败"));
    }

    private string GetOrCreateInstructions(SaveLifeAgentOptions options)
    {
        var cached = _cacheService?.Get<string>(InstructionsCacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
            return cached;

        var instructions = SaveLifeMafAgent.BuildStaticInstructions(
            options.MainRole,
            options.Prompt);
        CacheInstructions(instructions);
        return instructions;
    }

    private void CacheInstructions(string instructions)
    {
        _cacheService?.Set(
            InstructionsCacheKey,
            instructions,
            InstructionsCacheDuration);
    }

    private async Task<SaveLifeAgentOptions> LoadOptionsAsync()
    {
        var defaults = _defaultOptions ??
            App.GetConfig<SaveLifeAgentOptions>("SaveLife:Agent", true) ??
            new SaveLifeAgentOptions();
        var configs = await _configRep.AsQueryable()
            .Where(u => u.GroupCode == GroupCode)
            .ToListAsync();
        var values = configs
            .Where(u => !string.IsNullOrWhiteSpace(u.Code))
            .ToDictionary(u => u.Code!, u => u.Value, StringComparer.OrdinalIgnoreCase);
        var maxHistoryRounds = await ReadHistoryRoundsAsync(
            values,
            defaults.MaxHistoryRounds);

        return new SaveLifeAgentOptions
        {
            Enabled = ReadBool(values, EnabledCode, defaults.Enabled),
            MainRole = ReadChunkedString(values, MainRoleCode, defaults.MainRole),
            Prompt = ReadChunkedString(values, PromptCode, defaults.Prompt),
            Endpoint = defaults.Endpoint,
            ApiKey = defaults.ApiKey,
            ChatEndpoint = ReadString(values, ChatEndpointCode, defaults.GetChatEndpoint()),
            ChatApiKey = ReadSecret(values, ChatApiKeyCode, defaults.GetChatApiKey()),
            ChatModel = ReadString(values, ChatModelCode, defaults.ChatModel) ?? defaults.ChatModel,
            ChatTimeoutSeconds = ReadInt(values, ChatTimeoutSecondsCode, defaults.ChatTimeoutSeconds, 1, 300),
            EmbeddingEndpoint = ReadString(values, EmbeddingEndpointCode, defaults.GetEmbeddingEndpoint()),
            EmbeddingApiKey = ReadSecret(values, EmbeddingApiKeyCode, defaults.GetEmbeddingApiKey()),
            EmbeddingModel = ReadString(values, EmbeddingModelCode, defaults.EmbeddingModel) ?? defaults.EmbeddingModel,
            MaterialEmbeddingEndpoint = ReadString(values, MaterialEmbeddingEndpointCode, defaults.GetMaterialEmbeddingEndpoint()),
            MaterialEmbeddingApiKey = ReadSecret(values, MaterialEmbeddingApiKeyCode, defaults.GetMaterialEmbeddingApiKey()),
            MaterialEmbeddingModel = ReadString(values, MaterialEmbeddingModelCode, defaults.MaterialEmbeddingModel) ?? defaults.MaterialEmbeddingModel,
            MaterialEmbeddingDimensions = ReadInt(values, MaterialEmbeddingDimensionsCode, defaults.MaterialEmbeddingDimensions, 256, 4096),
            MaxHistoryRounds = maxHistoryRounds,
            MaxKnowledgeReferences = ReadInt(values, MaxKnowledgeReferencesCode, defaults.MaxKnowledgeReferences, 1, 20),
            MaxTrialReferences = ReadInt(values, MaxTrialReferencesCode, defaults.MaxTrialReferences, 1, 20),
            MinVectorScore = ReadDouble(values, MinVectorScoreCode, defaults.MinVectorScore, 0, 1)
        };
    }

    private async Task UpsertChunkedConfigAsync(string code, string value)
    {
        var chunks = SplitConfigValue(value);
        var desiredCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < chunks.Count; index++)
        {
            var chunkCode = GetChunkCode(code, index);
            desiredCodes.Add(chunkCode);
            await UpsertConfigAsync(chunkCode, chunks[index]);
        }

        var prefix = $"{code}_";
        var existingCodes = await _configRep.AsQueryable()
            .Where(item => item.GroupCode == GroupCode &&
                (item.Code == code || item.Code!.StartsWith(prefix)))
            .Select(item => item.Code)
            .ToListAsync();
        foreach (var existingCode in existingCodes.Where(existingCode =>
            existingCode != null &&
            TryGetChunkIndex(existingCode, code, out _) &&
            !desiredCodes.Contains(existingCode)))
        {
            await UpsertConfigAsync(existingCode!, string.Empty);
        }
    }

    private static List<string> SplitConfigValue(string value)
    {
        var chunks = new List<string>();
        for (var offset = 0; offset < value.Length;)
        {
            var length = Math.Min(MaxConfigValueLength, value.Length - offset);
            if (offset + length < value.Length &&
                char.IsHighSurrogate(value[offset + length - 1]) &&
                char.IsLowSurrogate(value[offset + length]))
            {
                length--;
            }

            chunks.Add(value.Substring(offset, length));
            offset += length;
        }

        return chunks;
    }

    private async Task UpsertConfigAsync(string code, string value)
    {
        var entity = await _configRep.GetFirstAsync(u => u.Code == code);
        if (entity == null)
        {
            await _configRep.InsertAsync(new SysConfig
            {
                Name = GetConfigName(code),
                Code = code,
                Value = Limit(value, MaxConfigValueLength),
                SysFlag = YesNoEnum.N,
                GroupCode = GroupCode,
                OrderNo = GetConfigOrder(code),
                Remark = "医线生机智能助手模型配置",
                CreateTime = DateTime.Now
            });
            return;
        }

        entity.Value = Limit(value, MaxConfigValueLength);
        entity.GroupCode = GroupCode;
        await _configRep.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandAsync();
    }

    private async Task<int> ReadHistoryRoundsAsync(
        Dictionary<string, string?> values,
        int defaultValue)
    {
        if (values.ContainsKey(MaxHistoryRoundsCode))
        {
            return ReadInt(
                values,
                MaxHistoryRoundsCode,
                defaultValue,
                1,
                SaveLifeAgentOptions.MaximumHistoryRounds);
        }

        var legacyValue = ReadString(values, LegacyMaxHistoryMessagesCode, null);
        if (!int.TryParse(legacyValue, out var legacyMessageCount))
        {
            return Clamp(defaultValue, 1, SaveLifeAgentOptions.MaximumHistoryRounds);
        }

        var migratedRounds = Clamp(
            (legacyMessageCount + 1) / 2,
            1,
            SaveLifeAgentOptions.MaximumHistoryRounds);
        await UpsertConfigAsync(MaxHistoryRoundsCode, migratedRounds.ToString());
        return migratedRounds;
    }

    private static void ValidateInput(SaveLifeAgentConfigInput input)
    {
        ValidatePromptText(
            input.MainRole,
            SaveLifeAgentOptions.MaximumMainRoleLength,
            $"主角色不能超过 {SaveLifeAgentOptions.MaximumMainRoleLength} 个字符");
        ValidatePromptText(
            input.Prompt,
            SaveLifeAgentOptions.MaximumPromptLength,
            $"提示词不能超过 {SaveLifeAgentOptions.MaximumPromptLength} 个字符");
        ValidateAbsoluteUrl(input.ChatEndpoint, "聊天模型 BaseUrl 必须使用 HTTPS，本机回环地址除外");
        ValidateAbsoluteUrl(input.EmbeddingEndpoint, "嵌入模型 BaseUrl 必须使用 HTTPS，本机回环地址除外");
        ValidateAbsoluteUrl(input.MaterialEmbeddingEndpoint, "病例资料多模态向量 BaseUrl 必须使用 HTTPS，本机回环地址除外");
        ValidateApiKey(input.ChatApiKey, "聊天模型密钥不能超过 240 个字符");
        ValidateApiKey(input.EmbeddingApiKey, "嵌入模型密钥不能超过 240 个字符");
        ValidateApiKey(input.MaterialEmbeddingApiKey, "病例资料多模态向量密钥不能超过 240 个字符");

        if (input.Enabled)
        {
            if (string.IsNullOrWhiteSpace(input.ChatModel))
                throw Oops.Oh("聊天模型名称不能为空").StatusCode(400);
            if (string.IsNullOrWhiteSpace(input.EmbeddingModel))
                throw Oops.Oh("嵌入模型名称不能为空").StatusCode(400);
            if (string.IsNullOrWhiteSpace(input.MaterialEmbeddingModel))
                throw Oops.Oh("病例资料多模态向量模型名称不能为空").StatusCode(400);
        }
    }

    private static void ValidateAbsoluteUrl(string? value, string message)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return;

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri) ||
            !SaveLifeAgentEndpointPolicy.IsAllowed(uri))
        {
            throw Oops.Oh(message).StatusCode(400);
        }
    }

    private static string? ReadString(Dictionary<string, string?> values, string code, string? defaultValue)
    {
        return values.TryGetValue(code, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : defaultValue;
    }

    private static string ReadChunkedString(
        Dictionary<string, string?> values,
        string code,
        string defaultValue)
    {
        if (!values.TryGetValue(code, out var firstChunk) || string.IsNullOrWhiteSpace(firstChunk))
            return defaultValue;

        var chunks = new SortedDictionary<int, string>();
        foreach (var item in values)
        {
            if (TryGetChunkIndex(item.Key, code, out var index))
                chunks[index] = item.Value ?? string.Empty;
        }

        return Normalize(string.Concat(chunks.Values)) ?? defaultValue;
    }

    private static string? ReadSecret(Dictionary<string, string?> values, string code, string? defaultValue)
    {
        var value = ReadString(values, code, null);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return UnprotectSecret(value) ?? defaultValue;
    }

    private static bool ReadBool(Dictionary<string, string?> values, string code, bool defaultValue)
    {
        var value = ReadString(values, code, null);
        return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    private static int ReadInt(Dictionary<string, string?> values, string code, int defaultValue, int min, int max)
    {
        var value = ReadString(values, code, null);
        return int.TryParse(value, out var parsed) ? Clamp(parsed, min, max) : Clamp(defaultValue, min, max);
    }

    private static double ReadDouble(Dictionary<string, string?> values, string code, double defaultValue, double min, double max)
    {
        var value = ReadString(values, code, null);
        return double.TryParse(value, out var parsed) ? Math.Clamp(parsed, min, max) : Math.Clamp(defaultValue, min, max);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void ValidatePromptText(string? value, int maxLength, string message)
    {
        var normalized = Normalize(value);
        if (normalized != null && normalized.Length > maxLength)
            throw Oops.Oh(message).StatusCode(400);
    }

    private static string GetChunkCode(string code, int index)
    {
        return index == 0 ? code : $"{code}_{index:D2}";
    }

    private static bool TryGetChunkIndex(string candidate, string code, out int index)
    {
        if (string.Equals(candidate, code, StringComparison.OrdinalIgnoreCase))
        {
            index = 0;
            return true;
        }

        var prefix = $"{code}_";
        index = 0;
        if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        return int.TryParse(candidate[prefix.Length..], out index) && index > 0;
    }

    private static int Clamp(int value, int min, int max)
    {
        return Math.Clamp(value, min, max);
    }

    private static string Limit(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string ProtectSecret(string value)
    {
        try
        {
            return $"{SecretPrefix}{CryptogramUtil.SM4EncryptECB(value)}";
        }
        catch
        {
            throw Oops.Oh("模型密钥加密失败").StatusCode(500);
        }
    }

    private static string? UnprotectSecret(string value)
    {
        var normalized = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (!normalized.StartsWith(SecretPrefix, StringComparison.OrdinalIgnoreCase))
            return normalized;

        try
        {
            return CryptogramUtil.SM4DecryptECB(normalized[SecretPrefix.Length..]);
        }
        catch
        {
            return null;
        }
    }

    private static void ValidateApiKey(string? value, string message)
    {
        var normalized = Normalize(value);
        if (!string.IsNullOrWhiteSpace(normalized) && Encoding.UTF8.GetByteCount(normalized) > MaxSecretBytes)
            throw Oops.Oh(message).StatusCode(400);
    }

    private static string? MaskApiKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length <= 8)
            return "****";

        return $"{trimmed[..4]}****{trimmed[^4..]}";
    }

    private static string GetConfigName(string code)
    {
        if (TryGetChunkIndex(code, MainRoleCode, out var mainRoleIndex))
            return mainRoleIndex == 0 ? "医线生机助手主角色" : $"医线生机助手主角色分片 {mainRoleIndex + 1}";
        if (TryGetChunkIndex(code, PromptCode, out var promptIndex))
            return promptIndex == 0 ? "医线生机助手提示词" : $"医线生机助手提示词分片 {promptIndex + 1}";

        return code switch
        {
            EnabledCode => "医线生机智能助手启用",
            ChatEndpointCode => "医线生机聊天模型 BaseUrl",
            ChatApiKeyCode => "医线生机聊天模型密钥",
            ChatModelCode => "医线生机聊天模型名称",
            ChatTimeoutSecondsCode => "医线生机聊天模型超时秒数",
            EmbeddingEndpointCode => "医线生机嵌入模型 BaseUrl",
            EmbeddingApiKeyCode => "医线生机嵌入模型密钥",
            EmbeddingModelCode => "医线生机嵌入模型名称",
            MaterialEmbeddingEndpointCode => "医线生机病例资料多模态向量 BaseUrl",
            MaterialEmbeddingApiKeyCode => "医线生机病例资料多模态向量密钥",
            MaterialEmbeddingModelCode => "医线生机病例资料多模态向量模型名称",
            MaterialEmbeddingDimensionsCode => "医线生机病例资料多模态向量维度",
            MaxHistoryRoundsCode => "医线生机助手历史轮数",
            MaxKnowledgeReferencesCode => "医线生机助手知识引用数",
            MaxTrialReferencesCode => "医线生机助手项目引用数",
            MinVectorScoreCode => "医线生机助手向量阈值",
            _ => code
        };
    }

    private static int GetConfigOrder(string code)
    {
        if (TryGetChunkIndex(code, MainRoleCode, out var mainRoleIndex))
            return 160 + mainRoleIndex;
        if (TryGetChunkIndex(code, PromptCode, out var promptIndex))
            return 180 + promptIndex;

        return code switch
        {
            EnabledCode => 10,
            ChatEndpointCode => 20,
            ChatApiKeyCode => 30,
            ChatModelCode => 40,
            ChatTimeoutSecondsCode => 45,
            EmbeddingEndpointCode => 50,
            EmbeddingApiKeyCode => 60,
            EmbeddingModelCode => 70,
            MaterialEmbeddingEndpointCode => 80,
            MaterialEmbeddingApiKeyCode => 90,
            MaterialEmbeddingModelCode => 100,
            MaterialEmbeddingDimensionsCode => 110,
            MaxHistoryRoundsCode => 120,
            MaxKnowledgeReferencesCode => 130,
            MaxTrialReferencesCode => 140,
            MinVectorScoreCode => 150,
            _ => 999
        };
    }
}
