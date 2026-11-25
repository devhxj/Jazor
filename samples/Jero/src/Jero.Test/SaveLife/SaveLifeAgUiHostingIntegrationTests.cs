using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeAgUiHostingIntegrationTests
{
    [Fact]
    public async Task MapAgUi_WithMafFunctionTool_StreamsStandardEvents()
    {
        var toolInvocationCount = 0;
        var chatClient = new ToolLoopChatClient();
        var tool = AIFunctionFactory.Create(
            (string query) =>
            {
                Interlocked.Increment(ref toolInvocationCount);
                return new { echoedQuery = query, status = "ok" };
            },
            name: "ProtocolTestTool");
        AIAgent agent = new ChatClientAgent(
            chatClient,
            new ChatClientAgentOptions
            {
                Name = "save-life-ag-ui-protocol-test",
                ChatOptions = new ChatOptions { Tools = [tool] }
            });
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(SaveLifeAgUiHostingIntegrationTests).Assembly.FullName,
            EnvironmentName = Environments.Development
        });
        // The test assembly can inherit Jero.Web's appsettings and its configured listeners.
        builder.Configuration.Sources.Clear();
        builder.WebHost.ConfigureKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 0));
        builder.Services.AddAGUIServer();

        await using var app = builder.Build();
        app.MapAGUIServer("/ag-ui", agent);
        await app.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            var address = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();
            using var client = new HttpClient { BaseAddress = new Uri(address) };
            using var response = await client.PostAsJsonAsync(
                "/ag-ui",
                new
                {
                    threadId = "thread-1",
                    runId = "run-1",
                    context = new[]
                    {
                        new
                        {
                            description = "SaveLife business context",
                            value = "{\"contextType\":\"trial\",\"contextId\":\"901\"}"
                        }
                    },
                    forwardedProps = new
                    {
                        saveLife = new
                        {
                            attachmentUnavailable = true,
                            materialIds = new[] { "9001", "9002", "9003", "9001", "" }
                        }
                    },
                    messages = new[]
                    {
                        new { id = "user-1", role = "user", content = "请调用协议测试工具" }
                    }
                },
                TestContext.Current.CancellationToken);
            var body = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();
            Assert.StartsWith("text/event-stream", response.Content.Headers.ContentType?.MediaType);
            var eventTypes = ReadEventTypes(body);
            Assert.Contains("RUN_STARTED", eventTypes);
            Assert.Contains("TOOL_CALL_START", eventTypes);
            Assert.Contains("TOOL_CALL_ARGS", eventTypes);
            Assert.Contains("TOOL_CALL_END", eventTypes);
            Assert.Contains("TOOL_CALL_RESULT", eventTypes);
            Assert.Contains("TEXT_MESSAGE_START", eventTypes);
            Assert.Contains("TEXT_MESSAGE_CONTENT", eventTypes);
            Assert.Contains("TEXT_MESSAGE_END", eventTypes);
            Assert.Contains("RUN_FINISHED", eventTypes);
            Assert.True(eventTypes.IndexOf("RUN_STARTED") < eventTypes.IndexOf("RUN_FINISHED"));
            Assert.Contains(
                ReadTextDeltas(body),
                delta => delta.Contains("协议回答", StringComparison.Ordinal));
            var envelope = SaveLifeMafRequestEnvelope.FromChatOptions(
                Assert.IsType<ChatOptions>(chatClient.LastOptions),
                chatClient.LastMessages);
            Assert.Equal("thread-1", envelope.ThreadId);
            Assert.Equal("run-1", envelope.RunId);
            Assert.Equal(["9001", "9002", "9003"], envelope.LatestUserMessage.MaterialIds);
            Assert.True(envelope.LatestUserMessage.AttachmentUnavailable);
            Assert.Equal(
                "{\"contextType\":\"trial\",\"contextId\":\"901\"}",
                envelope.BusinessContextJson);
            Assert.Equal(1, toolInvocationCount);
            Assert.Equal(2, chatClient.InvocationCount);
        }
        finally
        {
            await app.StopAsync(CancellationToken.None);
        }
    }

    private static List<string> ReadEventTypes(string body)
    {
        var eventTypes = new List<string>();
        foreach (var line in body.Split('\n'))
        {
            if (!line.StartsWith("data: ", StringComparison.Ordinal))
                continue;

            using var document = JsonDocument.Parse(line[6..].TrimEnd('\r'));
            if (document.RootElement.TryGetProperty("type", out var type))
                eventTypes.Add(type.GetString() ?? string.Empty);
        }
        return eventTypes;
    }

    private static List<string> ReadTextDeltas(string body)
    {
        var deltas = new List<string>();
        foreach (var line in body.Split('\n'))
        {
            if (!line.StartsWith("data: ", StringComparison.Ordinal))
                continue;

            using var document = JsonDocument.Parse(line[6..].TrimEnd('\r'));
            if (document.RootElement.TryGetProperty("type", out var type) &&
                type.GetString() == "TEXT_MESSAGE_CONTENT" &&
                document.RootElement.TryGetProperty("delta", out var delta))
            {
                deltas.Add(delta.GetString() ?? string.Empty);
            }
        }
        return deltas;
    }

    private sealed class ToolLoopChatClient : IChatClient
    {
        private int _invocationCount;

        public int InvocationCount => _invocationCount;

        public ChatOptions? LastOptions { get; private set; }

        public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var updates = new List<ChatResponseUpdate>();
            await foreach (var update in GetStreamingResponseAsync(
                messages,
                options,
                cancellationToken))
            {
                updates.Add(update);
            }
            return updates.ToChatResponse();
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _invocationCount);
            LastOptions = options;
            LastMessages = messages.ToList();
            var hasToolResult = LastMessages
                .SelectMany(message => message.Contents)
                .OfType<FunctionResultContent>()
                .Any();
            if (!hasToolResult)
            {
                yield return new ChatResponseUpdate(
                    ChatRole.Assistant,
                    [
                        new FunctionCallContent(
                            "call-1",
                            "ProtocolTestTool",
                            new Dictionary<string, object?> { ["query"] = "标准协议" })
                    ])
                {
                    MessageId = "assistant-tool-call"
                };
                yield break;
            }

            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, "协议回答")
            {
                MessageId = "assistant-answer"
            };
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return serviceKey == null && serviceType.IsInstanceOfType(this) ? this : null;
        }

        public void Dispose()
        {
        }
    }
}
