using System.Buffers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentMultimodalChatClient
{
    Task<string> CompleteAsync(
        SaveLifeAgentOptions options,
        SaveLifeAgentMultimodalChatRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class SaveLifeAgentMultimodalChatClient(
    IHttpClientFactory httpClientFactory) : ISaveLifeAgentMultimodalChatClient, ITransient
{
    private const int MaxResponseBytes = 1024 * 1024;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new ArkChatMediaUrlConverter() }
    };

    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<string> CompleteAsync(
        SaveLifeAgentOptions options,
        SaveLifeAgentMultimodalChatRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(request);
        if (!options.HasChatConfiguration)
            throw new InvalidOperationException("SaveLife:Agent chat configuration is incomplete.");

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            BuildEndpoint(options.GetChatEndpoint()));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            options.GetChatApiKey());
        var requestLimit = Math.Clamp(
            options.MaterialReviewMaxSerializedRequestBytes,
            1,
            SaveLifeMaterialResourceBudget.MaterialReviewSerializedRequestBytes);
        var serializedRequest = SerializeRequest(
            BuildPayload(options.ChatModel, request),
            requestLimit,
            cancellationToken);

        httpRequest.Content = new ByteArrayContent(
            serializedRequest.Buffer,
            0,
            serializedRequest.Length);
        httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };

        var client = _httpClientFactory.CreateClient(SaveLifeConst.AgentChatHttpClientName);
        using var response = await client.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Volcengine multimodal chat failed with HTTP {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        if (response.Content.Headers.ContentLength > MaxResponseBytes)
            throw new HttpRequestException("Volcengine multimodal chat response exceeded the allowed size.");

        var bytes = await ReadResponseBytesAsync(response.Content, cancellationToken);

        try
        {
            using var document = JsonDocument.Parse(bytes);
            return ExtractContent(document.RootElement);
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException("Volcengine multimodal chat returned invalid JSON.", ex);
        }
    }

    private static SerializedRequest SerializeRequest(
        ArkChatCompletionRequest payload,
        long requestLimit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestBuffer = GC.AllocateUninitializedArray<byte>(checked((int)requestLimit));
        var bufferWriter = new FixedBufferWriter(requestBuffer);
        try
        {
            using var jsonWriter = new Utf8JsonWriter(bufferWriter, WriterOptions);
            JsonSerializer.Serialize(jsonWriter, payload, JsonOptions);
            jsonWriter.Flush();
        }
        catch (RequestBudgetExceededException ex)
        {
            throw SerializedBudgetExceeded(ex);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new SerializedRequest(requestBuffer, bufferWriter.BytesWritten);
    }

    private static SaveLifeAgentUnavailableException SerializedBudgetExceeded(Exception innerException) =>
        new(
            SaveLifeAgentFailureCodes.MaterialNotReady,
            SaveLifeAgentFailureStages.Material,
            new InvalidOperationException(
                "Patient material model request exceeded the serialized budget.",
                innerException),
            retryable: false);

    internal static Uri BuildEndpoint(string? endpoint)
    {
        var normalized = endpoint?.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = SaveLifeAgentOptions.DefaultChatEndpoint;

        var baseUri = SaveLifeAgentEndpointPolicy.RequireAllowed(
            normalized,
            "SaveLife:Agent multimodal chat endpoint");

        var builder = new UriBuilder(baseUri)
        {
            Query = string.Empty,
            Fragment = string.Empty
        };
        var path = builder.Path.TrimEnd('/');
        if (!path.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            path = $"{path}/chat/completions";
        builder.Path = path;
        return builder.Uri;
    }

    private static ArkChatCompletionRequest BuildPayload(
        string model,
        SaveLifeAgentMultimodalChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SystemInstructions))
            throw new ArgumentException("System instructions are required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.UserMessage))
            throw new ArgumentException("User message is required.", nameof(request));

        var messages = new List<ArkChatMessage>
        {
            new("system", request.SystemInstructions.Trim())
        };
        messages.AddRange(request.History.Select(message =>
            new ArkChatMessage(message.Role, message.Content)));

        // Volcengine Chat multimodal contract:
        // https://console.volcengine.com/ark/region:cn-beijing/docs/82379/1362931?lang=zh
        var content = request.MaterialContent.Select(ToContentPart).ToList();
        content.Add(new ArkChatContentPart
        {
            Type = "text",
            Text = request.UserMessage.Trim()
        });
        messages.Add(new ArkChatMessage("user", content));

        using var schemaDocument = JsonDocument.Parse(request.AnswerSchema.ToString());
        return new ArkChatCompletionRequest
        {
            Model = model.Trim(),
            Messages = messages,
            ResponseFormat = new ArkChatResponseFormat
            {
                JsonSchema = new ArkChatJsonSchema
                {
                    Schema = schemaDocument.RootElement.Clone()
                }
            }
        };
    }

    private static ArkChatContentPart ToContentPart(SaveLifeAgentModelContentPart part)
    {
        return part.Type switch
        {
            SaveLifeAgentModelContentTypes.Text => new ArkChatContentPart
            {
                Type = "text",
                Text = RequireValue(part.Text, "Material text is required.")
            },
            SaveLifeAgentModelContentTypes.Image => new ArkChatContentPart
            {
                Type = "image_url",
                ImageUrl = new ArkChatMediaUrl
                {
                    Url = RequireValue(part.Url, "Material image URL is required.")
                }
            },
            SaveLifeAgentModelContentTypes.Video => new ArkChatContentPart
            {
                Type = "video_url",
                VideoUrl = new ArkChatMediaUrl
                {
                    Url = RequireValue(part.Url, "Material video URL is required.")
                }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(part), part.Type, "Unknown material content type.")
        };
    }

    private static string ExtractContent(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            throw new HttpRequestException("Volcengine multimodal chat response did not contain choices.");
        }

        var choice = choices[0];
        var finishReason = choice.TryGetProperty("finish_reason", out var finishReasonValue)
            ? finishReasonValue.GetString()
            : null;
        if (!string.Equals(finishReason, "stop", StringComparison.OrdinalIgnoreCase))
            throw new HttpRequestException("Volcengine multimodal chat did not complete normally.");

        if (!choice.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var content) ||
            content.ValueKind != JsonValueKind.String)
        {
            throw new HttpRequestException("Volcengine multimodal chat response did not contain text content.");
        }

        return content.GetString()?.Trim() ?? string.Empty;
    }

    private static async Task<byte[]> ReadResponseBytesAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        var total = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;

            total += read;
            if (total > MaxResponseBytes)
                throw new HttpRequestException("Volcengine multimodal chat response exceeded the allowed size.");

            await memory.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return memory.ToArray();
    }

    private static string RequireValue(string? value, string message)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(message)
            : value.Trim();
    }

    private sealed class ArkChatCompletionRequest
    {
        public string Model { get; init; } = string.Empty;

        public IReadOnlyList<ArkChatMessage> Messages { get; init; } = [];

        [JsonPropertyName("response_format")]
        public ArkChatResponseFormat ResponseFormat { get; init; } = new();
    }

    private sealed record ArkChatMessage(string Role, object Content);

    private sealed class ArkChatContentPart
    {
        public string Type { get; init; } = string.Empty;

        public string? Text { get; init; }

        [JsonPropertyName("image_url")]
        public ArkChatMediaUrl? ImageUrl { get; init; }

        [JsonPropertyName("video_url")]
        public ArkChatMediaUrl? VideoUrl { get; init; }
    }

    private sealed class ArkChatMediaUrl
    {
        public string Url { get; init; } = string.Empty;
    }

    private sealed class ArkChatMediaUrlConverter : JsonConverter<ArkChatMediaUrl>
    {
        public override ArkChatMediaUrl Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => throw new NotSupportedException();

        public override void Write(
            Utf8JsonWriter writer,
            ArkChatMediaUrl value,
            JsonSerializerOptions options)
        {
            var byteCount = Encoding.UTF8.GetByteCount(value.Url);
            var utf8Url = GC.AllocateUninitializedArray<byte>(byteCount);
            Encoding.UTF8.GetBytes(value.Url, utf8Url);
            writer.WriteStartObject();
            writer.WritePropertyName("url");
            writer.WriteStringValue(utf8Url);
            writer.WriteEndObject();
        }
    }

    private sealed class ArkChatResponseFormat
    {
        public string Type { get; init; } = "json_schema";

        [JsonPropertyName("json_schema")]
        public ArkChatJsonSchema JsonSchema { get; init; } = new();
    }

    private sealed class ArkChatJsonSchema
    {
        public string Name { get; init; } = "save_life_agent_answer";

        public string Description { get; init; } = "医线生机当前轮结构化回答、引用和建议追问";

        public bool Strict { get; init; } = true;

        public JsonElement Schema { get; init; }
    }

    private sealed record SerializedRequest(byte[] Buffer, int Length);

    private sealed class FixedBufferWriter(byte[] buffer) : IBufferWriter<byte>
    {
        public int BytesWritten { get; private set; }

        public void Advance(int count)
        {
            if (count < 0 || count > buffer.Length - BytesWritten)
                throw new RequestBudgetExceededException();
            BytesWritten += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return buffer.AsMemory(BytesWritten);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return buffer.AsSpan(BytesWritten);
        }

        private void EnsureCapacity(int sizeHint)
        {
            if (sizeHint < 0 || Math.Max(sizeHint, 1) > buffer.Length - BytesWritten)
                throw new RequestBudgetExceededException();
        }
    }

    private sealed class RequestBudgetExceededException : Exception
    {
    }
}

public sealed class SaveLifeAgentMultimodalChatRequest
{
    public string SystemInstructions { get; init; } = string.Empty;

    public IReadOnlyList<SaveLifeAgentPromptMessage> History { get; init; } = [];

    public string UserMessage { get; init; } = string.Empty;

    public IReadOnlyList<SaveLifeAgentModelContentPart> MaterialContent { get; init; } = [];

    public BinaryData AnswerSchema { get; init; } = BinaryData.FromString("{}");
}

public sealed record SaveLifeAgentPromptMessage(string Role, string Content);

public sealed class SaveLifeAgentModelContentPart
{
    private SaveLifeAgentModelContentPart(string type, string? text = null, string? url = null)
    {
        Type = type;
        Text = text;
        Url = url;
    }

    public string Type { get; }

    public string? Text { get; }

    public string? Url { get; }

    public static SaveLifeAgentModelContentPart CreateText(string text) =>
        new(SaveLifeAgentModelContentTypes.Text, text: text);

    public static SaveLifeAgentModelContentPart CreateImage(string url) =>
        new(SaveLifeAgentModelContentTypes.Image, url: url);

    public static SaveLifeAgentModelContentPart CreateVideo(string url) =>
        new(SaveLifeAgentModelContentTypes.Video, url: url);
}

public static class SaveLifeAgentModelContentTypes
{
    public const string Text = "text";
    public const string Image = "image";
    public const string Video = "video";
}
