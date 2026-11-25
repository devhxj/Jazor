using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentEmbeddingServiceTests
{
    [Fact]
    public async Task GenerateEmbeddingAsync_WhenProviderRejectsRequest_ExposesModelAndProviderReason()
    {
        var handler = new ErrorHandler("""
            {
              "error": {
                "message": "input length exceeds model context",
                "type": "invalid_request_error",
                "code": "context_length_exceeded"
              }
            }
            """);
        var client = new EmbeddingClient(
            "BAAI/bge-large-zh-v1.5",
            new ApiKeyCredential("test-key"),
            new OpenAIClientOptions
            {
                Endpoint = new Uri("https://embedding.example/v1"),
                Transport = new HttpClientPipelineTransport(new HttpClient(handler, disposeHandler: false))
            });
        var service = new SaveLifeAgentEmbeddingService(new FakeClientFactory(client));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GenerateEmbeddingAsync(new string('中', 1000)));

        Assert.Contains("model=BAAI/bge-large-zh-v1.5", exception.Message, StringComparison.Ordinal);
        Assert.Contains("inputChars=1000", exception.Message, StringComparison.Ordinal);
        Assert.Contains("endpoint=https://embedding.example/v1", exception.Message, StringComparison.Ordinal);
        Assert.Contains("context_length_exceeded", exception.Message, StringComparison.Ordinal);
        Assert.Contains("input length exceeds model context", exception.Message, StringComparison.Ordinal);
        Assert.Contains("512 tokens", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("test-key", exception.Message, StringComparison.Ordinal);
        Assert.Contains("model=BAAI/bge-large-zh-v1.5", SaveLifeAgentDiagnostics.BuildErrorSummary(exception));
    }

    private sealed class FakeClientFactory(EmbeddingClient client) : ISaveLifeAgentClientFactory
    {
        private static readonly SaveLifeAgentOptions Options = new()
        {
            Enabled = true,
            EmbeddingModel = "BAAI/bge-large-zh-v1.5",
            EmbeddingEndpoint = "https://embedding.example/v1",
            EmbeddingApiKey = "test-key"
        };

        public Task<SaveLifeAgentOptions> GetOptionsAsync()
        {
            return Task.FromResult(Options);
        }

        public Task<bool> CanUseChatAsync()
        {
            return Task.FromResult(false);
        }

        public Task<bool> CanUseEmbeddingsAsync()
        {
            return Task.FromResult(true);
        }

        public Task<ChatClient> CreateChatClientAsync()
        {
            throw new NotSupportedException();
        }

        public Task<EmbeddingClient> CreateEmbeddingClientAsync()
        {
            return Task.FromResult(client);
        }
    }

    private sealed class ErrorHandler(string responseJson) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
