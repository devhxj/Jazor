namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeMaterialEmbeddingService
{
    Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text);

    Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
        SaveLifeEnrollmentMaterial material,
        Func<Task<Stream>> openFileStream);
}

public sealed class SaveLifeMaterialEmbeddingService : ISaveLifeMaterialEmbeddingService, ITransient
{
    private const int MaxResponseBytes = 1024 * 1024;

    private readonly ISaveLifeAgentClientFactory _clientFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public SaveLifeMaterialEmbeddingService(
        ISaveLifeAgentClientFactory clientFactory,
        IHttpClientFactory httpClientFactory)
    {
        _clientFactory = clientFactory;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var options = await _clientFactory.GetOptionsAsync();
        if (!options.HasMaterialEmbeddingConfiguration)
            return null;

        var payload = new MultimodalEmbeddingRequest
        {
            Model = options.MaterialEmbeddingModel.Trim(),
            Instructions = "Target_modality: text, image and video.\nInstruction: Embed the medical query for retrieval against private clinical materials.\nQuery:",
            Input =
            [
                new MultimodalEmbeddingInput
                {
                    Type = "text",
                    Text = Limit(text, 2000)
                }
            ],
            Dimensions = options.MaterialEmbeddingDimensions
        };

        return await SendAsync(options, payload);
    }

    public async Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
        SaveLifeEnrollmentMaterial material,
        Func<Task<Stream>> openFileStream)
    {
        var options = await _clientFactory.GetOptionsAsync();
        if (!options.HasMaterialEmbeddingConfiguration)
            return null;

        var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(material.FileSuffix ?? material.FileName);
        var contentType = SaveLifeEnrollmentMaterialHelper.ResolveContentType(suffix, material.ContentType);
        var isImageOrVideo = SaveLifeEnrollmentMaterialHelper.IsImageOrVideo(suffix, contentType);

        var input = new List<MultimodalEmbeddingInput>
        {
            new()
            {
                Type = "text",
                Text = BuildMaterialMetadataText(material)
            }
        };

        var targetModality = "text";
        if (isImageOrVideo)
        {
            var fileDataUrl = await BuildDataUrlAsync(openFileStream, contentType);
            var isVideo = IsVideo(contentType, suffix);
            targetModality = isVideo ? "text and video" : "text and image";
            if (isVideo)
            {
                input.Add(new MultimodalEmbeddingInput
                {
                    Type = "video_url",
                    VideoUrl = new MultimodalEmbeddingUrl { Url = fileDataUrl }
                });
            }
            else
            {
                input.Add(new MultimodalEmbeddingInput
                {
                    Type = "image_url",
                    ImageUrl = new MultimodalEmbeddingUrl { Url = fileDataUrl }
                });
            }
        }

        var payload = new MultimodalEmbeddingRequest
        {
            Model = options.MaterialEmbeddingModel.Trim(),
            Instructions = BuildMaterialEmbeddingInstructions(targetModality),
            Input = input,
            Dimensions = options.MaterialEmbeddingDimensions
        };

        return await SendAsync(options, payload);
    }

    private async Task<SaveLifeMaterialEmbeddingResult> SendAsync(
        SaveLifeAgentOptions options,
        MultimodalEmbeddingRequest payload)
    {
        var endpoint = BuildEndpoint(options.GetMaterialEmbeddingEndpoint());
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.GetMaterialEmbeddingApiKey());
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        var client = _httpClientFactory.CreateClient(SaveLifeConst.MaterialEmbeddingHttpClientName);
        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead);
        if (response.Content.Headers.ContentLength > MaxResponseBytes)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"病例资料多模态向量调用失败：{(int)response.StatusCode} 响应超过大小限制",
                    null,
                    response.StatusCode);
            }

            throw ProviderResponseTooLarge();
        }

        var content = await ReadResponseTextAsync(response.Content);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"病例资料多模态向量调用失败：{(int)response.StatusCode} {NormalizeProviderError(content)}",
                null,
                response.StatusCode);
        }

        var parsed = JsonSerializer.Deserialize<MultimodalEmbeddingResponse>(content, JsonOptions)
            ?? throw new HttpRequestException("病例资料多模态向量响应为空");
        var embedding = parsed.GetFirstEmbedding();
        if (embedding.Length == 0)
            throw new HttpRequestException("病例资料多模态向量响应未包含向量");
        if (embedding.Length != options.MaterialEmbeddingDimensions)
        {
            throw new SaveLifeMaterialEmbeddingDimensionException(
                options.MaterialEmbeddingDimensions,
                embedding.Length,
                payload.Model);
        }

        return new SaveLifeMaterialEmbeddingResult
        {
            Embedding = embedding,
            Model = parsed.Model ?? payload.Model,
            ConfiguredModel = payload.Model,
            Dimensions = embedding.Length,
            ContentText = string.Join("\n", payload.Input.Select(u => u.Text).Where(u => !string.IsNullOrWhiteSpace(u)))
        };
    }

    private static async Task<string> BuildDataUrlAsync(Func<Task<Stream>> openFileStream, string contentType)
    {
        await using var stream = await openFileStream();
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        long total = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer);
            if (read == 0)
                break;

            total += read;
            if (total > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes)
                throw Oops.Oh("病例资料文件超过多模态解析限制").StatusCode(413);

            await memory.WriteAsync(buffer.AsMemory(0, read));
        }

        if (total == 0)
            throw Oops.Oh("病例资料文件为空").StatusCode(400);

        return $"data:{contentType};base64,{Convert.ToBase64String(memory.ToArray())}";
    }

    private static async Task<string> ReadResponseTextAsync(HttpContent content)
    {
        await using var stream = await content.ReadAsStreamAsync();
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        var total = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer);
            if (read == 0)
                break;

            total += read;
            if (total > MaxResponseBytes)
                throw ProviderResponseTooLarge();

            await memory.WriteAsync(buffer.AsMemory(0, read));
        }

        return Encoding.UTF8.GetString(memory.ToArray());
    }

    private static Exception ProviderResponseTooLarge()
    {
        return new HttpRequestException("病例资料多模态向量响应超过大小限制");
    }

    private static Uri BuildEndpoint(string? endpoint)
    {
        var normalized = endpoint?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = SaveLifeAgentOptions.DefaultMaterialEmbeddingEndpoint;

        var baseUri = SaveLifeAgentEndpointPolicy.RequireAllowed(
            normalized,
            "SaveLife:Agent material embedding endpoint");

        var builder = new UriBuilder(baseUri)
        {
            Query = string.Empty,
            Fragment = string.Empty
        };
        var path = builder.Path.TrimEnd('/');
        if (!path.EndsWith("/embeddings/multimodal", StringComparison.OrdinalIgnoreCase))
            path = $"{path}/embeddings/multimodal";
        builder.Path = path;
        return builder.Uri;
    }

    private static bool IsVideo(string contentType, string suffix)
    {
        return contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ||
            suffix is ".mp4" or ".mov";
    }

    private static string BuildMaterialEmbeddingInstructions(string targetModality)
    {
        return $"Target_modality: {targetModality}.\nInstruction: Embed this private clinical material for retrieval. Preserve medical relevance; do not diagnose.\nQuery:";
    }

    private static string BuildMaterialMetadataText(SaveLifeEnrollmentMaterial material)
    {
        return string.Join("\n", new[]
        {
            $"材料类型：{SaveLifeEnrollmentMaterialHelper.GetMaterialTypeText(material.MaterialType)}",
            $"文件名：{BuildDisplayFileName(material)}",
            $"文件类型：{material.ContentType}",
            $"用户备注：{material.Remark}",
            $"审核备注：{material.ReviewRemark}",
            $"补充要求：{material.SupplementRequest}",
            material.ExtractedText
        }.Where(u => !string.IsNullOrWhiteSpace(u)));
    }

    private static string BuildDisplayFileName(SaveLifeEnrollmentMaterial material)
    {
        if (string.IsNullOrWhiteSpace(material.FileName))
            return "病例材料";

        return !string.IsNullOrWhiteSpace(material.FileSuffix) && !material.FileName.EndsWith(material.FileSuffix, StringComparison.OrdinalIgnoreCase)
            ? $"{material.FileName}{material.FileSuffix}"
            : material.FileName;
    }

    private static string Limit(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string NormalizeProviderError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "响应为空";

        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("code", out var code) &&
                code.ValueKind == JsonValueKind.String)
            {
                var normalizedCode = code.GetString()?.Trim();
                if (!string.IsNullOrWhiteSpace(normalizedCode) &&
                    normalizedCode.Length <= 64 &&
                    normalizedCode.All(character =>
                        char.IsAsciiLetterOrDigit(character) ||
                        character is '_' or '-' or '.'))
                {
                    return normalizedCode;
                }
            }
        }
        catch (JsonException)
        {
        }

        return "响应格式无效";
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class MultimodalEmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;

        public string? Instructions { get; set; }

        public List<MultimodalEmbeddingInput> Input { get; set; } = new();

        public int Dimensions { get; set; }

        [JsonPropertyName("encoding_format")]
        public string EncodingFormat { get; set; } = "float";
    }

    private sealed class MultimodalEmbeddingInput
    {
        public string Type { get; set; } = string.Empty;

        public string? Text { get; set; }

        [JsonPropertyName("image_url")]
        public MultimodalEmbeddingUrl? ImageUrl { get; set; }

        [JsonPropertyName("video_url")]
        public MultimodalEmbeddingUrl? VideoUrl { get; set; }
    }

    private sealed class MultimodalEmbeddingUrl
    {
        public string Url { get; set; } = string.Empty;
    }

    private sealed class MultimodalEmbeddingResponse
    {
        public string? Model { get; set; }

        public JsonElement Data { get; set; }

        public float[] GetFirstEmbedding()
        {
            return SaveLifeMaterialEmbeddingResponseParser.GetFirstEmbedding(Data);
        }
    }
}

public sealed class SaveLifeMaterialEmbeddingResult
{
    public float[] Embedding { get; set; } = [];

    public string Model { get; set; } = string.Empty;

    public string ConfiguredModel { get; set; } = string.Empty;

    public int Dimensions { get; set; }

    public string? ContentText { get; set; }
}

public sealed class SaveLifeMaterialEmbeddingDimensionException : InvalidOperationException
{
    public SaveLifeMaterialEmbeddingDimensionException(
        int expectedDimensions,
        int actualDimensions,
        string configuredModel)
        : base($"病例资料多模态向量维度不匹配，期望 {expectedDimensions}，实际 {actualDimensions}")
    {
        ExpectedDimensions = expectedDimensions;
        ActualDimensions = actualDimensions;
        ConfiguredModel = configuredModel;
    }

    public int ExpectedDimensions { get; }

    public int ActualDimensions { get; }

    public string ConfiguredModel { get; }
}
