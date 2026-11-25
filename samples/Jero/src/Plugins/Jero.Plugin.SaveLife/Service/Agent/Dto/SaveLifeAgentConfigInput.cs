namespace Jero.Plugin.SaveLife.Service;

public class SaveLifeAgentConfigInput
{
    public bool Enabled { get; set; } = true;

    public string? MainRole { get; set; }

    public string? Prompt { get; set; }

    public string? ChatEndpoint { get; set; }

    public string? ChatApiKey { get; set; }

    public string? ChatModel { get; set; }

    public int ChatTimeoutSeconds { get; set; } = 45;

    public string? EmbeddingEndpoint { get; set; }

    public string? EmbeddingApiKey { get; set; }

    public string? EmbeddingModel { get; set; }

    public string? MaterialEmbeddingEndpoint { get; set; }

    public string? MaterialEmbeddingApiKey { get; set; }

    public string? MaterialEmbeddingModel { get; set; }

    public int MaterialEmbeddingDimensions { get; set; } = 1024;

    public int? MaxHistoryRounds { get; set; }

    public int MaxKnowledgeReferences { get; set; } = 4;

    public int MaxTrialReferences { get; set; } = 5;

    public double MinVectorScore { get; set; } = 0.18;
}

public class SaveLifeAgentConfigOutput
{
    public bool Enabled { get; set; }

    public string MainRole { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string? ChatEndpoint { get; set; }

    public bool HasChatApiKey { get; set; }

    public string? ChatApiKeyMasked { get; set; }

    public string? ChatModel { get; set; }

    public bool CanUseChat { get; set; }

    public int ChatTimeoutSeconds { get; set; }

    public string? EmbeddingEndpoint { get; set; }

    public bool HasEmbeddingApiKey { get; set; }

    public string? EmbeddingApiKeyMasked { get; set; }

    public string? EmbeddingModel { get; set; }

    public bool CanUseEmbeddings { get; set; }

    public string? MaterialEmbeddingEndpoint { get; set; }

    public bool HasMaterialEmbeddingApiKey { get; set; }

    public string? MaterialEmbeddingApiKeyMasked { get; set; }

    public string? MaterialEmbeddingModel { get; set; }

    public int MaterialEmbeddingDimensions { get; set; }

    public bool CanUseMaterialEmbedding { get; set; }

    public int MaxHistoryRounds { get; set; }

    public int MaxKnowledgeReferences { get; set; }

    public int MaxTrialReferences { get; set; }

    public double MinVectorScore { get; set; }
}

public class SaveLifeAgentConfigTestOutput
{
    public bool Success { get; set; }

    public DateTime TestTime { get; set; } = DateTime.Now;

    public List<SaveLifeAgentConfigTestItemOutput> Items { get; set; } = [];
}

public class SaveLifeAgentConfigTestItemOutput
{
    public string Name { get; set; } = string.Empty;

    public string Target { get; set; } = string.Empty;

    public string? Endpoint { get; set; }

    public string? Model { get; set; }

    public bool Success { get; set; }

    public bool Skipped { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string Message { get; set; } = string.Empty;
}
