namespace Jero.Plugin.SaveLife.Service;

using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.ClientModel;
using System.Diagnostics;

/// <summary>
/// 医线生机智能助手模型配置
/// </summary>
[ApiDescriptionSettings(SaveLifeConst.GroupName, Order = 160)]
public class SaveLifeAgentConfigService : IDynamicApiController, ITransient
{
    private const string SyntheticRedPngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAdSURBVDhPY/jPwPCfEsyALkAqHjVg1IBRAwaLAQAwxP4Q7zYsrwAAAABJRU5ErkJggg==";

    private static readonly BinaryData TextProbeSchema = BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "status": { "type": "string", "enum": ["ok"] }
          },
          "required": ["status"],
          "additionalProperties": false
        }
        """);

    private static readonly BinaryData ImageProbeSchema = BinaryData.FromString("""
        {
          "type": "object",
          "properties": {
            "color": { "type": "string", "enum": ["red"] }
          },
          "required": ["color"],
          "additionalProperties": false
        }
        """);

    private readonly ISaveLifeAgentConfigProvider _configProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public SaveLifeAgentConfigService(
        ISaveLifeAgentConfigProvider configProvider,
        IHttpClientFactory httpClientFactory)
    {
        _configProvider = configProvider;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 获取智能助手模型配置
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "Config")]
    public async Task<SaveLifeAgentConfigOutput> GetConfig()
    {
        return await _configProvider.GetConfigAsync();
    }

    /// <summary>
    /// 保存智能助手模型配置
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SaveConfig")]
    public async Task SaveConfig(SaveLifeAgentConfigInput input)
    {
        await _configProvider.SaveConfigAsync(input);
    }

    /// <summary>
    /// 验证智能助手模型配置
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "TestConfig")]
    public async Task<SaveLifeAgentConfigTestOutput> TestConfig(SaveLifeAgentConfigInput input)
    {
        var options = await _configProvider.BuildOptionsAsync(input);
        var output = new SaveLifeAgentConfigTestOutput();

        output.Items.Add(await TestTextChatAsync(options));
        output.Items.Add(await TestImageChatAsync(options));
        output.Items.Add(await TestEmbeddingAsync(options));
        output.Items.Add(await TestMaterialTextEmbeddingAsync(options));
        output.Items.Add(await TestMaterialImageEmbeddingAsync(options));
        output.Success = output.Items.All(u => u.Success);

        return output;
    }

    private static async Task<SaveLifeAgentConfigTestItemOutput> TestTextChatAsync(SaveLifeAgentOptions options)
    {
        var item = CreateTestItem("文本回答", "chatText", options.GetChatEndpoint(), options.ChatModel);
        if (string.IsNullOrWhiteSpace(options.GetChatApiKey()) || string.IsNullOrWhiteSpace(options.ChatModel))
            return Skip(item, "聊天模型、ApiKey 未配置完整");

        return await MeasureAsync(item, async () =>
        {
            var client = new ChatClient(
                options.ChatModel.Trim(),
                new ApiKeyCredential(options.GetChatApiKey()!),
                BuildClientOptions(options.GetChatEndpoint(), "SaveLife:Agent chat endpoint"));
            var completion = await client.CompleteChatAsync(
                [
                    new SystemChatMessage("你正在验证文本结构化回答能力。"),
                    new UserChatMessage("请严格按 Schema 返回 status=ok。")
                ],
                new ChatCompletionOptions
                {
                    Temperature = 0,
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "save_life_text_capability",
                        jsonSchema: TextProbeSchema,
                        jsonSchemaFormatDescription: "医线生机文本回答能力验证",
                        jsonSchemaIsStrict: true)
                });
            var content = string.Join("", completion.Value.Content
                .Select(u => u.Text)
                .Where(u => !string.IsNullOrWhiteSpace(u)))
                .Trim();

            RequireProbeValue(content, "status", "ok", "文本回答结构化响应无效");
            item.Message = "调用成功，严格结构化文本响应有效";
        });
    }

    private async Task<SaveLifeAgentConfigTestItemOutput> TestImageChatAsync(SaveLifeAgentOptions options)
    {
        var item = CreateTestItem("图片理解", "chatImage", options.GetChatEndpoint(), options.ChatModel);
        if (string.IsNullOrWhiteSpace(options.GetChatApiKey()) || string.IsNullOrWhiteSpace(options.ChatModel))
            return Skip(item, "聊天模型、ApiKey 未配置完整，无法验证图片理解");

        return await MeasureAsync(item, async () =>
        {
            var client = new SaveLifeAgentMultimodalChatClient(_httpClientFactory);
            var content = await client.CompleteAsync(options, new SaveLifeAgentMultimodalChatRequest
            {
                SystemInstructions = "识别用户提供的合成色块，只按严格 JSON Schema 回答。",
                UserMessage = "图中唯一的颜色是什么？",
                MaterialContent =
                [
                    SaveLifeAgentModelContentPart.CreateImage(
                        $"data:image/png;base64,{SyntheticRedPngBase64}")
                ],
                AnswerSchema = ImageProbeSchema
            });
            RequireProbeValue(content, "color", "red", "图片理解结构化响应无效");
            item.Message = "调用成功，合成图片识别和严格结构化响应有效";
        });
    }

    private static async Task<SaveLifeAgentConfigTestItemOutput> TestEmbeddingAsync(SaveLifeAgentOptions options)
    {
        var item = CreateTestItem("公共知识/项目文本向量", "embedding", options.GetEmbeddingEndpoint(), options.EmbeddingModel);
        if (string.IsNullOrWhiteSpace(options.GetEmbeddingApiKey()) || string.IsNullOrWhiteSpace(options.EmbeddingModel))
            return Skip(item, "嵌入模型、ApiKey 未配置完整");

        return await MeasureAsync(item, async () =>
        {
            var client = new EmbeddingClient(
                options.EmbeddingModel.Trim(),
                new ApiKeyCredential(options.GetEmbeddingApiKey()!),
                BuildClientOptions(options.GetEmbeddingEndpoint(), "SaveLife:Agent embedding endpoint"));
            var result = await client.GenerateEmbeddingAsync("医线生机助手配置验证");
            var dimensions = result.Value.ToFloats().Length;
            if (dimensions <= 0)
                throw new InvalidOperationException("嵌入模型未返回向量");

            item.Message = $"调用成功，向量维度：{dimensions}";
        });
    }

    private async Task<SaveLifeAgentConfigTestItemOutput> TestMaterialTextEmbeddingAsync(SaveLifeAgentOptions options)
    {
        var item = CreateTestItem("病例文本向量", "materialTextEmbedding", options.GetMaterialEmbeddingEndpoint(), options.MaterialEmbeddingModel);
        if (string.IsNullOrWhiteSpace(options.GetMaterialEmbeddingApiKey()) || string.IsNullOrWhiteSpace(options.MaterialEmbeddingModel))
            return Skip(item, "病例资料多模态向量模型、ApiKey 未配置完整，无法验证文本向量");

        return await MeasureAsync(item, async () =>
        {
            var service = CreateMaterialEmbeddingService(options);
            var result = await service.GenerateQueryEmbeddingAsync("医线生机病例检索配置验证")
                ?? throw new InvalidOperationException("病例文本向量响应为空");
            RequireMaterialDimensions(result, options);
            item.Message = $"调用成功，向量维度：{result.Dimensions}";
            item.Model = result.Model;
        });
    }

    private async Task<SaveLifeAgentConfigTestItemOutput> TestMaterialImageEmbeddingAsync(SaveLifeAgentOptions options)
    {
        var item = CreateTestItem("病例图片向量", "materialImageEmbedding", options.GetMaterialEmbeddingEndpoint(), options.MaterialEmbeddingModel);
        if (string.IsNullOrWhiteSpace(options.GetMaterialEmbeddingApiKey()) || string.IsNullOrWhiteSpace(options.MaterialEmbeddingModel))
            return Skip(item, "病例资料多模态向量模型、ApiKey 未配置完整，无法验证图片向量");

        return await MeasureAsync(item, async () =>
        {
            var service = CreateMaterialEmbeddingService(options);
            var imageBytes = Convert.FromBase64String(SyntheticRedPngBase64);
            var result = await service.GenerateMaterialEmbeddingAsync(
                new SaveLifeEnrollmentMaterial
                {
                    ContentType = "image/png",
                    FileName = "capability-probe.png",
                    FileSuffix = ".png",
                    MaterialType = "other"
                },
                () => Task.FromResult<Stream>(new MemoryStream(imageBytes, writable: false)))
                ?? throw new InvalidOperationException("病例图片向量响应为空");
            RequireMaterialDimensions(result, options);
            item.Message = $"调用成功，向量维度：{result.Dimensions}";
            item.Model = result.Model;
        });
    }

    private SaveLifeMaterialEmbeddingService CreateMaterialEmbeddingService(SaveLifeAgentOptions options)
    {
        return new SaveLifeMaterialEmbeddingService(
            new FixedOptionsClientFactory(options),
            _httpClientFactory);
    }

    private static void RequireMaterialDimensions(
        SaveLifeMaterialEmbeddingResult result,
        SaveLifeAgentOptions options)
    {
        if (result.Embedding.Length != options.MaterialEmbeddingDimensions ||
            result.Dimensions != options.MaterialEmbeddingDimensions)
        {
            throw new InvalidOperationException("病例资料多模态向量维度不匹配");
        }
    }

    private static void RequireProbeValue(
        string content,
        string propertyName,
        string expectedValue,
        string errorMessage)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty(propertyName, out var value) &&
                value.ValueKind == JsonValueKind.String &&
                string.Equals(value.GetString(), expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
        catch (JsonException)
        {
        }

        throw new InvalidOperationException(errorMessage);
    }

    private static SaveLifeAgentConfigTestItemOutput CreateTestItem(string name, string target, string? endpoint, string? model)
    {
        return new SaveLifeAgentConfigTestItemOutput
        {
            Name = name,
            Target = target,
            Endpoint = endpoint,
            Model = model
        };
    }

    private static SaveLifeAgentConfigTestItemOutput Skip(SaveLifeAgentConfigTestItemOutput item, string message)
    {
        item.Skipped = true;
        item.Message = message;
        return item;
    }

    private static async Task<SaveLifeAgentConfigTestItemOutput> MeasureAsync(
        SaveLifeAgentConfigTestItemOutput item,
        Func<Task> action)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await action();
            item.Success = true;
        }
        catch (Exception ex)
        {
            item.Success = false;
            item.Message = NormalizeErrorMessage(ex);
        }
        finally
        {
            stopwatch.Stop();
            item.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }

        return item;
    }

    private static OpenAIClientOptions BuildClientOptions(string? endpointValue, string settingName)
    {
        return new OpenAIClientOptions
        {
            Endpoint = SaveLifeAgentEndpointPolicy.RequireAllowed(endpointValue, settingName)
        };
    }

    private static string NormalizeErrorMessage(Exception exception)
    {
        return SaveLifeAgentDiagnostics.BuildErrorSummary(exception) ?? "调用失败";
    }

    private sealed class FixedOptionsClientFactory(SaveLifeAgentOptions options)
        : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync() => Task.FromResult(options);

        public Task<bool> CanUseChatAsync() => Task.FromResult(options.HasChatConfiguration);

        public Task<bool> CanUseEmbeddingsAsync() => Task.FromResult(options.HasEmbeddingConfiguration);

        public Task<ChatClient> CreateChatClientAsync() => throw new NotSupportedException();

        public Task<EmbeddingClient> CreateEmbeddingClientAsync() => throw new NotSupportedException();
    }
}
