namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifeAgentOptions
{
    private string? _instructions;

    public const string DefaultChatEndpoint = "https://ark.cn-beijing.volces.com/api/coding/v3";

    public const string DefaultChatModel = "doubao-seed-2.0-lite";

    public const string DefaultMaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/coding/v3";

    public const string DefaultMaterialEmbeddingModel = "doubao-embedding-vision";

    public const int DefaultHistoryRounds = 10;

    public const int MaximumHistoryRounds = 20;

    public const int MaximumMainRoleLength = 2000;

    public const int MaximumPromptLength = 8000;

    public bool Enabled { get; set; } = true;

    public string MainRole { get; set; } = SaveLifeMafAgent.DefaultMainRole;

    public string Prompt { get; set; } = SaveLifeMafAgent.DefaultPrompt;

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public string? ChatEndpoint { get; set; } = DefaultChatEndpoint;

    public string? ChatApiKey { get; set; }

    public string ChatModel { get; set; } = DefaultChatModel;

    public int ChatTimeoutSeconds { get; set; } = 45;

    public string? EmbeddingEndpoint { get; set; }

    public string? EmbeddingApiKey { get; set; }

    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    public string? MaterialEmbeddingEndpoint { get; set; } = DefaultMaterialEmbeddingEndpoint;

    public string? MaterialEmbeddingApiKey { get; set; }

    public string MaterialEmbeddingModel { get; set; } = DefaultMaterialEmbeddingModel;

    public int MaterialEmbeddingDimensions { get; set; } = 1024;

    public int MaterialReviewMaxMediaCount { get; set; } =
        SaveLifeMaterialResourceBudget.MaterialReviewMediaCount;

    public long MaterialReviewMaxMediaBytes { get; set; } =
        SaveLifeMaterialResourceBudget.MaterialReviewMediaBytes;

    public int MaterialReviewMaxDocumentCharacters { get; set; } =
        SaveLifeMaterialResourceBudget.MaterialReviewDocumentCharacters;

    public long MaterialReviewMaxSerializedRequestBytes { get; set; } =
        SaveLifeMaterialResourceBudget.MaterialReviewSerializedRequestBytes;

    public int MaxHistoryRounds { get; set; } = DefaultHistoryRounds;

    public int MaxKnowledgeReferences { get; set; } = 4;

    public int MaxTrialReferences { get; set; } = 5;

    public double MinVectorScore { get; set; } = 0.18;

    public string Instructions => _instructions ??
        SaveLifeMafAgent.BuildStaticInstructions(MainRole, Prompt);

    public bool HasChatConfiguration =>
        Enabled &&
        !string.IsNullOrWhiteSpace(GetChatApiKey()) &&
        !string.IsNullOrWhiteSpace(ChatModel);

    public bool HasEmbeddingConfiguration =>
        Enabled &&
        !string.IsNullOrWhiteSpace(GetEmbeddingApiKey()) &&
        !string.IsNullOrWhiteSpace(EmbeddingModel);

    public bool HasMaterialEmbeddingConfiguration =>
        Enabled &&
        !string.IsNullOrWhiteSpace(GetMaterialEmbeddingApiKey()) &&
        !string.IsNullOrWhiteSpace(MaterialEmbeddingModel);

    public string? GetChatEndpoint()
    {
        return FirstNonEmpty(ChatEndpoint, Endpoint);
    }

    public string? GetChatApiKey()
    {
        return FirstNonEmpty(ChatApiKey, ApiKey);
    }

    public string? GetEmbeddingEndpoint()
    {
        return FirstNonEmpty(EmbeddingEndpoint, Endpoint);
    }

    public string? GetEmbeddingApiKey()
    {
        return FirstNonEmpty(EmbeddingApiKey, ApiKey);
    }

    public string? GetMaterialEmbeddingEndpoint()
    {
        return FirstNonEmpty(MaterialEmbeddingEndpoint);
    }

    public string? GetMaterialEmbeddingApiKey()
    {
        return FirstNonEmpty(MaterialEmbeddingApiKey);
    }

    internal void SetInstructions(string instructions)
    {
        _instructions = string.IsNullOrWhiteSpace(instructions)
            ? null
            : instructions.Trim();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))?.Trim();
    }
}

internal static class SaveLifeAgentEndpointPolicy
{
    public static bool IsAllowed(Uri endpoint)
    {
        return string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            (string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                endpoint.IsLoopback);
    }

    public static Uri RequireAllowed(string? endpointValue, string settingName)
    {
        if (!Uri.TryCreate(endpointValue?.Trim(), UriKind.Absolute, out var endpoint))
            throw new InvalidOperationException($"{settingName} is invalid.");
        if (!IsAllowed(endpoint))
            throw new InvalidOperationException($"{settingName} must use HTTPS unless it targets loopback.");

        return endpoint;
    }
}
