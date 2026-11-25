using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMaterialEmbeddingServiceTests
{
    [Fact]
    public async Task GenerateMaterialEmbeddingAsync_WithImageMaterial_SendsVolcengineCompatiblePayload()
    {
        // Arrange
        var handler = new CapturingHandler("""
            {
              "model": "doubao-embedding-vision-251215",
              "data": [
                {
                  "embedding": [0.1, 0.2, 0.3]
                }
              ]
            }
            """);
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision-251215",
                MaterialEmbeddingDimensions = 3
            }),
            new FakeHttpClientFactory(handler));
        var material = new SaveLifeEnrollmentMaterial
        {
            MaterialType = "medical_record",
            FileName = "报名图片_1783147917055_1.jpg",
            FileSuffix = ".jpg",
            ContentType = "image/jpeg",
            Remark = "报名图片_1783147917055_1.jpg"
        };

        // Act
        var result = await service.GenerateMaterialEmbeddingAsync(
            material,
            () => Task.FromResult<Stream>(new MemoryStream([1, 2, 3])));

        // Assert
        Assert.NotNull(result);
        Assert.Equal([0.1f, 0.2f, 0.3f], result.Embedding);
        Assert.Equal("doubao-embedding-vision-251215", result.ConfiguredModel);
        Assert.Equal(3, result.Dimensions);
        Assert.Equal(
            "https://ark.cn-beijing.volces.com/api/v3/embeddings/multimodal",
            handler.RequestUri?.ToString());
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("vision-key", handler.AuthorizationParameter);

        using var document = JsonDocument.Parse(handler.RequestJson!);
        var root = document.RootElement;
        Assert.Equal("doubao-embedding-vision-251215", root.GetProperty("model").GetString());
        Assert.Equal("float", root.GetProperty("encoding_format").GetString());
        Assert.Equal(3, root.GetProperty("dimensions").GetInt32());
        Assert.Equal(
            "Target_modality: text and image.\nInstruction: Embed this private clinical material for retrieval. Preserve medical relevance; do not diagnose.\nQuery:",
            root.GetProperty("instructions").GetString());
        Assert.False(root.TryGetProperty("multi_embedding", out _));
        Assert.False(root.TryGetProperty("sparse_embedding", out _));

        var input = root.GetProperty("input").EnumerateArray().ToList();
        Assert.Equal(2, input.Count);
        Assert.Equal("text", input[0].GetProperty("type").GetString());
        Assert.Contains("材料类型：病例资料", input[0].GetProperty("text").GetString());
        Assert.Equal("image_url", input[1].GetProperty("type").GetString());
        Assert.Equal(
            "data:image/jpeg;base64,AQID",
            input[1].GetProperty("image_url").GetProperty("url").GetString());
    }

    [Fact]
    public async Task GenerateMaterialEmbeddingAsync_WithDocumentMaterial_SendsExtractedTextToMultimodalEndpoint()
    {
        // Arrange
        var handler = new CapturingHandler("""
            {
              "model": "doubao-embedding-vision",
              "data": [
                {
                  "embedding": [0.7, 0.8, 0.9]
                }
              ]
            }
            """);
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision",
                MaterialEmbeddingDimensions = 3
            }),
            new FakeHttpClientFactory(handler));
        var material = new SaveLifeEnrollmentMaterial
        {
            MaterialType = "diagnosis_report",
            FileName = "病理报告.pdf",
            FileSuffix = ".pdf",
            ContentType = "application/pdf",
            ExtractedText = "病理诊断：肺腺癌，ALK 融合阳性。"
        };
        var openFileCount = 0;

        // Act
        var result = await service.GenerateMaterialEmbeddingAsync(
            material,
            () =>
            {
                openFileCount++;
                return Task.FromResult<Stream>(new MemoryStream([1, 2, 3]));
            });

        // Assert
        Assert.NotNull(result);
        Assert.Equal([0.7f, 0.8f, 0.9f], result.Embedding);
        Assert.Equal(0, openFileCount);
        Assert.Equal(
            "https://ark.cn-beijing.volces.com/api/v3/embeddings/multimodal",
            handler.RequestUri?.ToString());

        using var document = JsonDocument.Parse(handler.RequestJson!);
        var root = document.RootElement;
        Assert.Equal("doubao-embedding-vision", root.GetProperty("model").GetString());
        Assert.Equal(
            "Target_modality: text.\nInstruction: Embed this private clinical material for retrieval. Preserve medical relevance; do not diagnose.\nQuery:",
            root.GetProperty("instructions").GetString());
        Assert.False(root.TryGetProperty("multi_embedding", out _));

        var input = root.GetProperty("input").EnumerateArray().ToList();
        Assert.Single(input);
        Assert.Equal("text", input[0].GetProperty("type").GetString());
        Assert.Contains("材料类型：诊断报告", input[0].GetProperty("text").GetString());
        Assert.Contains("病理诊断：肺腺癌，ALK 融合阳性。", input[0].GetProperty("text").GetString());
    }

    [Fact]
    public async Task GenerateMaterialEmbeddingAsync_WithOversizedImage_StopsReadingAtSizeLimit()
    {
        // Arrange
        var handler = new CapturingHandler("{}");
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision"
            }),
            new FakeHttpClientFactory(handler));
        var material = new SaveLifeEnrollmentMaterial
        {
            MaterialType = "medical_record",
            FileName = "oversized.jpg",
            FileSuffix = ".jpg",
            ContentType = "image/jpeg"
        };
        var stream = new CountingReadStream(
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 8 * 1024 * 1024);

        // Act
        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.GenerateMaterialEmbeddingAsync(material, () => Task.FromResult<Stream>(stream)));

        // Assert
        Assert.InRange(
            stream.BytesRead,
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1,
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 81920);
        Assert.Null(handler.RequestJson);
    }

    [Fact]
    public async Task GenerateQueryEmbeddingAsync_WithOversizedChunkedResponse_StopsReadingAtResponseLimit()
    {
        // Arrange
        var stream = new CountingReadStream(8 * 1024 * 1024);
        var responseContent = new StreamContent(stream);
        responseContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision"
            }),
            new FakeHttpClientFactory(new StreamingResponseHandler(responseContent)));

        // Act
        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.GenerateQueryEmbeddingAsync("肺癌病例匹配"));

        // Assert
        Assert.InRange(stream.BytesRead, 1024 * 1024 + 1, 1024 * 1024 + 81920);
    }

    [Fact]
    public async Task GenerateQueryEmbeddingAsync_WithInsecureRemoteEndpoint_RejectsBeforeRequest()
    {
        // Arrange
        var handler = new CapturingHandler("""
            {
              "model": "doubao-embedding-vision",
              "data": [{ "embedding": [0.1, 0.2] }]
            }
            """);
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "http://example.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision"
            }),
            new FakeHttpClientFactory(handler));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateQueryEmbeddingAsync("肺癌病例匹配"));

        // Assert
        Assert.Null(handler.RequestUri);
    }

    [Fact]
    public async Task GenerateQueryEmbeddingAsync_WithTextQuery_RequestsOnlyTheConsumedDenseEmbedding()
    {
        // Arrange
        var handler = new CapturingHandler("""
            {
              "model": "doubao-embedding-vision-251215",
              "data": [
                {
                  "embedding": [0.4, 0.5, 0.6]
                }
              ]
            }
            """);
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision-251215",
                MaterialEmbeddingDimensions = 3
            }),
            new FakeHttpClientFactory(handler));

        // Act
        var result = await service.GenerateQueryEmbeddingAsync("肺癌病例匹配");

        // Assert
        Assert.NotNull(result);
        Assert.Equal([0.4f, 0.5f, 0.6f], result.Embedding);

        using var document = JsonDocument.Parse(handler.RequestJson!);
        var root = document.RootElement;
        Assert.Equal(
            "Target_modality: text, image and video.\nInstruction: Embed the medical query for retrieval against private clinical materials.\nQuery:",
            root.GetProperty("instructions").GetString());
        Assert.False(root.TryGetProperty("multi_embedding", out _));
        Assert.False(root.TryGetProperty("sparse_embedding", out _));
        var input = root.GetProperty("input").EnumerateArray().ToList();
        Assert.Single(input);
        Assert.Equal("text", input[0].GetProperty("type").GetString());
        Assert.Equal("肺癌病例匹配", input[0].GetProperty("text").GetString());
    }

    [Fact]
    public async Task GenerateQueryEmbeddingAsync_WhenProviderReturnsWrongDimensions_FailsClosed()
    {
        var handler = new CapturingHandler("""
            {
              "model": "doubao-embedding-vision-revision",
              "data": [{ "embedding": [0.1, 0.2] }]
            }
            """);
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                Enabled = true,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingModel = "doubao-embedding-vision",
                MaterialEmbeddingDimensions = 3
            }),
            new FakeHttpClientFactory(handler));

        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            service.GenerateQueryEmbeddingAsync("肺癌病例匹配"));

        Assert.Contains("维度", error.Message);
        Assert.DoesNotContain("0.1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeProviderError_WhenProviderEchoesPatientContent_ReturnsOnlyErrorCode()
    {
        // Arrange
        var method = typeof(SaveLifeMaterialEmbeddingService)
            .GetMethod("NormalizeProviderError", BindingFlags.NonPublic | BindingFlags.Static)!;
        const string patientText = "病理诊断：肺腺癌，ALK 融合阳性";
        var body = """
            {
              "error": {
                "code": "InvalidParameter",
                "message": "病理诊断：肺腺癌，ALK 融合阳性 data:image/png;base64,AQID"
              }
            }
            """;

        // Act
        var message = (string)method.Invoke(null, new object?[] { body })!;

        // Assert
        Assert.Equal("InvalidParameter", message);
        Assert.DoesNotContain(patientText, message, StringComparison.Ordinal);
        Assert.DoesNotContain("base64", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateQueryEmbeddingAsync_WhenProviderRejectsRequest_ReturnsSanitizedHttpFailure()
    {
        const string patientText = "病理诊断：肺腺癌，ALK 融合阳性";
        var handler = new CapturingHandler(
            $$"""
            {
              "error": {
                "code": "InvalidParameter",
                "message": "{{patientText}} data:image/png;base64,AQID"
              }
            }
            """,
            HttpStatusCode.BadRequest);
        var service = new SaveLifeMaterialEmbeddingService(
            new FakeAgentClientFactory(new SaveLifeAgentOptions
            {
                MaterialEmbeddingApiKey = "vision-key",
                MaterialEmbeddingDimensions = 1024,
                MaterialEmbeddingEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                MaterialEmbeddingModel = "doubao-embedding-vision"
            }),
            new FakeHttpClientFactory(handler));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GenerateQueryEmbeddingAsync("病例检索配置验证"));

        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Contains("InvalidParameter", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(patientText, error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("base64", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeAgentClientFactory(SaveLifeAgentOptions options) : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync()
        {
            return Task.FromResult(options);
        }

        public Task<bool> CanUseChatAsync()
        {
            return Task.FromResult(options.HasChatConfiguration);
        }

        public Task<bool> CanUseEmbeddingsAsync()
        {
            return Task.FromResult(options.HasEmbeddingConfiguration);
        }

        public Task<ChatClient> CreateChatClientAsync()
        {
            throw new NotSupportedException();
        }

        public Task<EmbeddingClient> CreateEmbeddingClientAsync()
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal(SaveLifeConst.MaterialEmbeddingHttpClientName, name);
            return new HttpClient(handler, disposeHandler: false);
        }
    }

    private sealed class CapturingHandler(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        public string? RequestJson { get; private set; }

        public string? AuthorizationScheme { get; private set; }

        public string? AuthorizationParameter { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            RequestJson = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        }
    }

    private sealed class StreamingResponseHandler(HttpContent responseContent) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            });
        }
    }

    private sealed class CountingReadStream : Stream
    {
        private readonly long _length;
        private long _remaining;

        public CountingReadStream(long length)
        {
            _length = length;
            _remaining = length;
        }

        public long BytesRead { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position
        {
            get => BytesRead;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = (int)Math.Min(count, _remaining);
            _remaining -= read;
            BytesRead += read;
            return read;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer.Span));
        }

        public override int Read(Span<byte> buffer)
        {
            var read = (int)Math.Min(buffer.Length, _remaining);
            _remaining -= read;
            BytesRead += read;
            return read;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
