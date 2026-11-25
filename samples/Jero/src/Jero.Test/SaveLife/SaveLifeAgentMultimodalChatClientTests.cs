using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentMultimodalChatClientTests
{
    [Fact]
    public async Task CompleteAsync_WithMixedPatientMaterial_UsesVolcengineChatContentContract()
    {
        var handler = new CapturingHandler(ChatCompletionJson("{\"content\":\"已读取病例\",\"references\":[],\"suggestedFollowUps\":[]}"));
        var client = new SaveLifeAgentMultimodalChatClient(new FakeHttpClientFactory(handler));
        var options = new SaveLifeAgentOptions
        {
            Enabled = true,
            ChatEndpoint = "https://ark.cn-beijing.volces.com/api/coding/v3",
            ChatApiKey = "test-chat-key",
            ChatModel = "doubao-seed-2.0-lite"
        };
        var request = new SaveLifeAgentMultimodalChatRequest
        {
            SystemInstructions = "只输出结构化病例解读",
            History =
            [
                new SaveLifeAgentPromptMessage("user", "上一轮问题"),
                new SaveLifeAgentPromptMessage("assistant", "上一轮回答")
            ],
            UserMessage = "请看看这份病例",
            MaterialContent =
            [
                SaveLifeAgentModelContentPart.CreateImage("data:image/png;base64,AQID"),
                SaveLifeAgentModelContentPart.CreateVideo("data:video/mp4;base64,BAUG"),
                SaveLifeAgentModelContentPart.CreateText("病例文档提取文本：病理诊断内容")
            ],
            AnswerSchema = BinaryData.FromString("""
                {
                  "type": "object",
                  "properties": {
                    "content": { "type": "string" }
                  },
                  "required": ["content"],
                  "additionalProperties": false
                }
                """)
        };

        var content = await client.CompleteAsync(
            options,
            request,
            TestContext.Current.CancellationToken);

        Assert.Contains("已读取病例", content, StringComparison.Ordinal);
        Assert.Equal(
            "https://ark.cn-beijing.volces.com/api/coding/v3/chat/completions",
            handler.RequestUri?.ToString());
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("test-chat-key", handler.AuthorizationParameter);

        using var document = JsonDocument.Parse(handler.RequestJson!);
        var root = document.RootElement;
        Assert.Equal("doubao-seed-2.0-lite", root.GetProperty("model").GetString());

        var messages = root.GetProperty("messages").EnumerateArray().ToList();
        Assert.Equal(4, messages.Count);
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("assistant", messages[2].GetProperty("role").GetString());

        var currentContent = messages[3].GetProperty("content").EnumerateArray().ToList();
        Assert.Equal(4, currentContent.Count);
        Assert.Equal("image_url", currentContent[0].GetProperty("type").GetString());
        Assert.Equal(
            "data:image/png;base64,AQID",
            currentContent[0].GetProperty("image_url").GetProperty("url").GetString());
        Assert.Equal("video_url", currentContent[1].GetProperty("type").GetString());
        Assert.Equal(
            "data:video/mp4;base64,BAUG",
            currentContent[1].GetProperty("video_url").GetProperty("url").GetString());
        Assert.Equal("text", currentContent[2].GetProperty("type").GetString());
        Assert.Contains("病理诊断内容", currentContent[2].GetProperty("text").GetString());
        Assert.Equal("text", currentContent[3].GetProperty("type").GetString());
        Assert.Equal("请看看这份病例", currentContent[3].GetProperty("text").GetString());

        var responseFormat = root.GetProperty("response_format");
        Assert.Equal("json_schema", responseFormat.GetProperty("type").GetString());
        var jsonSchema = responseFormat.GetProperty("json_schema");
        Assert.Equal("save_life_agent_answer", jsonSchema.GetProperty("name").GetString());
        Assert.True(jsonSchema.GetProperty("strict").GetBoolean());
        Assert.Equal(
            "object",
            jsonSchema.GetProperty("schema").GetProperty("type").GetString());
    }

    [Fact]
    public async Task CompleteAsync_WhenProviderEchoesSensitiveInput_DiagnosticsExcludePatientContent()
    {
        const string patientText = "患者隐私内容-ALK阳性";
        const string dataUrl = "data:image/png;base64,AQIDBAUGBwgJ";
        var handler = new CapturingHandler(
            $$"""
            {
              "error": {
                "code": "InvalidParameter",
                "message": "{{patientText}} {{dataUrl}}"
              }
            }
            """,
            HttpStatusCode.BadRequest);
        var client = new SaveLifeAgentMultimodalChatClient(new FakeHttpClientFactory(handler));

        var error = await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(
            new SaveLifeAgentOptions
            {
                Enabled = true,
                ChatEndpoint = "https://ark.cn-beijing.volces.com/api/v3",
                ChatApiKey = "test-chat-key",
                ChatModel = "doubao-seed-2.0-lite"
            },
            new SaveLifeAgentMultimodalChatRequest
            {
                SystemInstructions = "病例解读",
                UserMessage = "请看病例",
                MaterialContent = [SaveLifeAgentModelContentPart.CreateImage(dataUrl)],
                AnswerSchema = BinaryData.FromString("{\"type\":\"object\"}")
            },
            TestContext.Current.CancellationToken));

        var summary = SaveLifeAgentDiagnostics.BuildErrorSummary(error);
        Assert.DoesNotContain(patientText, summary, StringComparison.Ordinal);
        Assert.DoesNotContain(dataUrl, summary, StringComparison.Ordinal);
        Assert.DoesNotContain("base64", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CompleteAsync_WhenChunkedResponseExceedsLimit_StopsReadingAtBoundary()
    {
        var responseStream = new NonSeekableCountingStream(new byte[2 * 1024 * 1024]);
        var client = new SaveLifeAgentMultimodalChatClient(
            new FakeHttpClientFactory(new StreamResponseHandler(responseStream)));

        await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(
            new SaveLifeAgentOptions
            {
                Enabled = true,
                ChatEndpoint = "https://ark.cn-beijing.volces.com/api/coding/v3",
                ChatApiKey = "test-chat-key",
                ChatModel = "doubao-seed-2.0-lite"
            },
            new SaveLifeAgentMultimodalChatRequest
            {
                SystemInstructions = "病例解读",
                UserMessage = "请看病例",
                MaterialContent =
                [
                    SaveLifeAgentModelContentPart.CreateImage("data:image/png;base64,AQID")
                ],
                AnswerSchema = BinaryData.FromString("{\"type\":\"object\"}")
            },
            TestContext.Current.CancellationToken));

        Assert.InRange(responseStream.BytesRead, 1024 * 1024 + 1, 1024 * 1024 + 81920);
    }

    [Fact]
    public async Task CompleteAsync_WhenSerializedRequestExceedsConfiguredBudget_FailsBeforeSending()
    {
        var handler = new CountingHandler();
        var client = new SaveLifeAgentMultimodalChatClient(new FakeHttpClientFactory(handler));
        var options = new SaveLifeAgentOptions
        {
            Enabled = true,
            ChatEndpoint = "https://ark.cn-beijing.volces.com/api/coding/v3",
            ChatApiKey = "test-chat-key",
            ChatModel = "doubao-seed-2.0-lite",
            MaterialReviewMaxSerializedRequestBytes = 256
        };

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            client.CompleteAsync(
                options,
                new SaveLifeAgentMultimodalChatRequest
                {
                    SystemInstructions = new string('规', 300),
                    UserMessage = "请看病例",
                    MaterialContent =
                    [
                        SaveLifeAgentModelContentPart.CreateImage("data:image/png;base64,AQID")
                    ],
                    AnswerSchema = BinaryData.FromString("{\"type\":\"object\"}")
                },
                TestContext.Current.CancellationToken));

        Assert.Equal(SaveLifeAgentFailureCodes.MaterialNotReady, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Material, error.Stage);
        Assert.Equal(0, handler.SendCount);
    }

    private static string ChatCompletionJson(string content)
    {
        return $$"""
        {
          "id": "chatcmpl-multimodal-test",
          "object": "chat.completion",
          "created": 1,
          "model": "doubao-seed-2.0-lite",
          "choices": [
            {
              "index": 0,
              "message": {
                "role": "assistant",
                "content": {{JsonSerializer.Serialize(content)}}
              },
              "finish_reason": "stop"
            }
          ]
        }
        """;
    }

    private sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            Assert.Equal(SaveLifeConst.AgentChatHttpClientName, name);
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

    private sealed class StreamResponseHandler(Stream responseStream) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(responseStream)
            });
        }
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int SendCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SendCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ChatCompletionJson("{}"), Encoding.UTF8)
            });
        }
    }

    private sealed class NonSeekableCountingStream(byte[] bytes) : Stream
    {
        private readonly MemoryStream _inner = new(bytes, writable: false);

        public int BytesRead { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, count);
            BytesRead += read;
            return read;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            var read = await _inner.ReadAsync(buffer, cancellationToken);
            BytesRead += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}
