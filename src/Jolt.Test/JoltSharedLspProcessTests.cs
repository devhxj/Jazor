using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Jolt.Test;

[TestClass]
public sealed class JoltSharedLspProcessTests
{
    [TestMethod]
    public async Task Jolt_Lsp_SharedInstance_AllowsMultipleInFlightCompletionRequests()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(Jolt_Lsp_SharedInstance_AllowsMultipleInFlightCompletionRequests));
        var solution = topology.CreateSolution("RealtimeWorkspace");
        var counterProject = solution.AddProject("CounterApp");
        var reportProject = solution.AddProject("ReportApp");

        var counterPath = counterProject.GetPath("Counter.cs");
        var counterText =
            """
            public class Counter
            {
                public int Count { get; set; }

                public void Increment()
                {
                    this.
                    this.
                }
            }
            """;
        await File.WriteAllTextAsync(counterPath, counterText);

        var reportPath = reportProject.GetPath("Report.cs");
        var reportText =
            """
            public class Report
            {
                public string Title { get; set; } = string.Empty;

                public void Render()
                {
                    this.
                    this.
                }
            }
            """;
        await File.WriteAllTextAsync(reportPath, reportText);

        await using var client = await SharedLspTestClient.StartAsync("--no-deno-worker");
        await client.InitializeAsync(solution.RootPath);
        await client.OpenDocumentAsync(new Uri(counterPath).AbsoluteUri, counterText, languageId: "csharp");
        await client.OpenDocumentAsync(new Uri(reportPath).AbsoluteUri, reportText, languageId: "csharp");

        var counterCompletionTask = client.RequestCompletionLabelsAsync(
            new Uri(counterPath).AbsoluteUri,
            line: 6,
            character: 13);
        var reportCompletionTask = client.RequestCompletionLabelsAsync(
            new Uri(reportPath).AbsoluteUri,
            line: 6,
            character: 13);

        await Task.WhenAll(counterCompletionTask, reportCompletionTask);

        CollectionAssert.Contains(counterCompletionTask.Result, "Count");
        CollectionAssert.Contains(reportCompletionTask.Result, "Title");
    }

    [TestMethod]
    public async Task Jolt_Lsp_SharedInstance_CanRunBatchScenariosWithoutRestartingProcess()
    {
        using var topology = JoltIntegrationTestTopology.Create(nameof(Jolt_Lsp_SharedInstance_CanRunBatchScenariosWithoutRestartingProcess));
        var alphaSolution = topology.CreateSolution("AlphaSolution");
        var alphaProject = alphaSolution.AddProject("AlphaApp");
        var betaSolution = topology.CreateSolution("BetaSolution");
        var betaProject = betaSolution.AddProject("BetaApp");

        var alphaPath = alphaProject.GetPath("Alpha.cs");
        var alphaText =
            """
            public class Alpha
            {
                public int Value { get; set; }

                public void Touch()
                {
                    this.
                }
            }
            """;
        await File.WriteAllTextAsync(alphaPath, alphaText);

        var betaPath = betaProject.GetPath("Beta.cs");
        var betaText =
            """
            public class Beta
            {
                public string Name { get; set; } = string.Empty;

                public void Touch()
                {
                    this.
                }
            }
            """;
        await File.WriteAllTextAsync(betaPath, betaText);

        await using var client = await SharedLspTestClient.StartAsync("--no-deno-worker");
        await client.InitializeAsync(alphaSolution.RootPath, betaSolution.RootPath);

        var alphaUri = new Uri(alphaPath).AbsoluteUri;
        await client.OpenDocumentAsync(alphaUri, alphaText, languageId: "csharp");
        var alphaLabels = await client.RequestCompletionLabelsAsync(alphaUri, line: 6, character: 13);
        CollectionAssert.Contains(alphaLabels, "Value");
        await client.CloseDocumentAsync(alphaUri);

        var betaUri = new Uri(betaPath).AbsoluteUri;
        await client.OpenDocumentAsync(betaUri, betaText, languageId: "csharp");
        var betaLabels = await client.RequestCompletionLabelsAsync(betaUri, line: 6, character: 13);
        CollectionAssert.Contains(betaLabels, "Name");
        await client.CloseDocumentAsync(betaUri);
    }

    private sealed class SharedLspTestClient : IAsyncDisposable
    {
        private readonly Process _process;
        private readonly Stream _input;
        private readonly Stream _output;
        private readonly SemaphoreSlim _sendGate = new(1, 1);
        private readonly Lock _notificationGate = new();
        private readonly List<JsonElement> _bufferedNotifications = [];
        private readonly Channel<JsonElement> _notificationChannel = Channel.CreateUnbounded<JsonElement>();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pendingResponses = new(StringComparer.Ordinal);
        private readonly CancellationTokenSource _readerCancellationSource = new();
        private readonly Task _readerTask;
        private int _nextRequestId;

        private SharedLspTestClient(Process process, Stream input, Stream output)
        {
            _process = process;
            _input = input;
            _output = output;
            _readerTask = RunReadLoopAsync();
        }

        public static async Task<SharedLspTestClient> StartAsync(params string[] additionalArguments)
        {
            var hostAssemblyPath = GetBuiltAssemblyPath("Jolt", "Jolt.dll");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add(hostAssemblyPath);
            process.StartInfo.ArgumentList.Add("--lsp");
            foreach (var argument in additionalArguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            Assert.IsTrue(process.Start(), "Expected Jolt LSP process to start.");
            await Task.Yield();
            return new SharedLspTestClient(
                process,
                process.StandardInput.BaseStream,
                process.StandardOutput.BaseStream);
        }

        public async Task InitializeAsync(params string[] workspaceRoots)
        {
            Assert.IsTrue(workspaceRoots.Length > 0, "Expected at least one workspace root.");

            var response = await SendRequestAsync(
                "initialize",
                new
                {
                    // 共享实例集成测试显式传入多个 workspace folder，
                    // 用单个 Jolt 进程覆盖多 solution / 多 project 拓扑。
                    workspaceFolders = workspaceRoots.Select(workspaceRoot => new
                    {
                        uri = new Uri(workspaceRoot).AbsoluteUri,
                        name = Path.GetFileName(workspaceRoot)
                    }).ToArray()
                });

            Assert.IsTrue(response.TryGetProperty("result", out _));
        }

        public async Task OpenDocumentAsync(string uri, string text, string languageId)
        {
            await SendNotificationAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didOpen",
                @params = new
                {
                    textDocument = new
                    {
                        uri,
                        languageId,
                        version = 1,
                        text
                    }
                }
            });

            var notification = await WaitForNotificationAsync(
                static (message, expectedUri) => IsPublishDiagnosticsForUri(message, expectedUri),
                uri);
            Assert.AreEqual("textDocument/publishDiagnostics", notification.GetProperty("method").GetString());
        }

        public async Task CloseDocumentAsync(string uri)
        {
            await SendNotificationAsync(new
            {
                jsonrpc = "2.0",
                method = "textDocument/didClose",
                @params = new
                {
                    textDocument = new
                    {
                        uri
                    }
                }
            });

            var notification = await WaitForNotificationAsync(
                static (message, expectedUri) => IsPublishDiagnosticsForUri(message, expectedUri),
                uri);
            Assert.AreEqual("textDocument/publishDiagnostics", notification.GetProperty("method").GetString());
        }

        public async Task<string[]> RequestCompletionLabelsAsync(string uri, int line, int character)
        {
            var response = await SendRequestAsync(
                "textDocument/completion",
                new
                {
                    textDocument = new
                    {
                        uri
                    },
                    position = new
                    {
                        line,
                        character
                    }
                });

            if (!response.TryGetProperty("result", out var result))
            {
                Assert.Fail("Expected completion response to contain a result payload. Raw response: " + response.GetRawText());
            }

            return result
                .EnumerateArray()
                .Select(static item => item.GetProperty("label").GetString() ?? string.Empty)
                .ToArray();
        }

        private async Task<JsonElement> SendRequestAsync(string method, object parameters)
        {
            var requestId = Interlocked.Increment(ref _nextRequestId);
            var responseKey = CreateResponseKey(requestId);
            var responseSource = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_pendingResponses.TryAdd(responseKey, responseSource))
            {
                throw new InvalidOperationException($"Duplicate request id '{requestId}' was generated.");
            }

            try
            {
                await SendAsync(new
                {
                    jsonrpc = "2.0",
                    id = requestId,
                    method,
                    @params = parameters
                });

                return await responseSource.Task.WaitAsync(TimeSpan.FromSeconds(20));
            }
            finally
            {
                _pendingResponses.TryRemove(responseKey, out _);
            }
        }

        private async Task SendNotificationAsync(object payload)
            => await SendAsync(payload);

        private async Task SendAsync(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var body = Encoding.UTF8.GetBytes(json);
            var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n");

            await _sendGate.WaitAsync();
            try
            {
                await _input.WriteAsync(header);
                await _input.WriteAsync(body);
                await _input.FlushAsync();
            }
            finally
            {
                _sendGate.Release();
            }
        }

        private async Task<JsonElement> WaitForNotificationAsync<TState>(
            Func<JsonElement, TState, bool> predicate,
            TState state)
        {
            lock (_notificationGate)
            {
                for (var index = 0; index < _bufferedNotifications.Count; index++)
                {
                    var candidate = _bufferedNotifications[index];
                    if (!predicate(candidate, state))
                    {
                        continue;
                    }

                    _bufferedNotifications.RemoveAt(index);
                    return candidate;
                }
            }

            using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            while (true)
            {
                var candidate = await _notificationChannel.Reader.ReadAsync(timeoutSource.Token);
                if (predicate(candidate, state))
                {
                    return candidate;
                }

                lock (_notificationGate)
                {
                    _bufferedNotifications.Add(candidate);
                }
            }
        }

        private async Task RunReadLoopAsync()
        {
            try
            {
                while (true)
                {
                    var message = await ReadMessageAsync(_output, _readerCancellationSource.Token);
                    if (message is null)
                    {
                        break;
                    }

                    var root = message.Value;
                    if (TryGetResponseKey(root, out var responseKey)
                        && _pendingResponses.TryRemove(responseKey, out var responseSource))
                    {
                        responseSource.TrySetResult(root);
                        continue;
                    }

                    await _notificationChannel.Writer.WriteAsync(root, _readerCancellationSource.Token);
                }
            }
            catch (OperationCanceledException) when (_readerCancellationSource.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                FailPendingResponses(exception);
                _notificationChannel.Writer.TryComplete(exception);
                return;
            }

            FailPendingResponses(new EndOfStreamException("The Jolt LSP process closed its output stream."));
            _notificationChannel.Writer.TryComplete();
        }

        private void FailPendingResponses(Exception exception)
        {
            foreach (var entry in _pendingResponses)
            {
                if (_pendingResponses.TryRemove(entry.Key, out var pendingResponse))
                {
                    pendingResponse.TrySetException(exception);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_process.HasExited)
                {
                    try
                    {
                        var response = await SendRequestAsync("shutdown", new { });
                        Assert.AreEqual(JsonValueKind.Null, response.GetProperty("result").ValueKind);
                        await SendNotificationAsync(new
                        {
                            jsonrpc = "2.0",
                            method = "exit",
                            @params = new { }
                        });
                        await _process.WaitForExitAsync(CancellationToken.None);
                    }
                    catch
                    {
                        _process.Kill(entireProcessTree: true);
                        await _process.WaitForExitAsync(CancellationToken.None);
                    }
                }
            }
            finally
            {
                _readerCancellationSource.Cancel();
                try
                {
                    await _readerTask;
                }
                catch
                {
                }

                _readerCancellationSource.Dispose();
                _sendGate.Dispose();
                _process.Dispose();
            }
        }

        private static bool IsPublishDiagnosticsForUri(JsonElement message, string expectedUri)
        {
            if (!message.TryGetProperty("method", out var methodProperty)
                || !string.Equals(methodProperty.GetString(), "textDocument/publishDiagnostics", StringComparison.Ordinal))
            {
                return false;
            }

            return message.TryGetProperty("params", out var paramsProperty)
                && paramsProperty.TryGetProperty("uri", out var uriProperty)
                && string.Equals(uriProperty.GetString(), expectedUri, StringComparison.Ordinal);
        }

        private static string CreateResponseKey(int requestId)
            => "n:" + requestId.ToString(System.Globalization.CultureInfo.InvariantCulture);

        private static bool TryGetResponseKey(JsonElement message, out string responseKey)
        {
            if (message.TryGetProperty("id", out var idProperty))
            {
                switch (idProperty.ValueKind)
                {
                    case JsonValueKind.Number when idProperty.TryGetInt32(out var numericId):
                        responseKey = CreateResponseKey(numericId);
                        return true;
                    case JsonValueKind.String:
                        responseKey = "s:" + (idProperty.GetString() ?? string.Empty);
                        return true;
                }
            }

            responseKey = string.Empty;
            return false;
        }

        private static async Task<JsonElement?> ReadMessageAsync(Stream stream, CancellationToken cancellationToken)
        {
            var contentLength = await ReadContentLengthAsync(stream, cancellationToken);
            if (contentLength is null)
            {
                return null;
            }

            var body = new byte[contentLength.Value];
            var offset = 0;
            while (offset < body.Length)
            {
                var read = await stream.ReadAsync(body.AsMemory(offset, body.Length - offset), cancellationToken);
                if (read == 0)
                {
                    return null;
                }

                offset += read;
            }

            using var document = JsonDocument.Parse(body);
            return document.RootElement.Clone();
        }

        private static async Task<int?> ReadContentLengthAsync(Stream stream, CancellationToken cancellationToken)
        {
            var headerBytes = new List<byte>();
            var buffer = new byte[1];
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
                if (read == 0)
                {
                    return headerBytes.Count == 0
                        ? null
                        : throw new EndOfStreamException("Unexpected EOF while reading LSP message headers.");
                }

                headerBytes.Add(buffer[0]);
                var count = headerBytes.Count;
                if (count >= 4
                    && headerBytes[count - 4] == '\r'
                    && headerBytes[count - 3] == '\n'
                    && headerBytes[count - 2] == '\r'
                    && headerBytes[count - 1] == '\n')
                {
                    break;
                }
            }

            var headerText = Encoding.ASCII.GetString(headerBytes.ToArray());
            foreach (var line in headerText.Split(["\r\n"], StringSplitOptions.RemoveEmptyEntries))
            {
                if (!line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return int.Parse(
                    line["Content-Length:".Length..].Trim(),
                    System.Globalization.CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException("Missing Content-Length header.");
        }

        private static string GetBuiltAssemblyPath(string projectDirectoryName, string assemblyFileName)
        {
            var candidatePaths = new[]
            {
                Path.Combine(
                    GetRepositoryRoot(),
                    "src",
                    projectDirectoryName,
                    "bin",
                    "Debug",
                    "net10.0",
                    assemblyFileName),
                Path.Combine(AppContext.BaseDirectory, assemblyFileName)
            };

            foreach (var candidatePath in candidatePaths)
            {
                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }

            Assert.Fail("Expected built assembly to exist. Probed: " + string.Join(", ", candidatePaths));
            return candidatePaths[0];
        }

        private static string GetRepositoryRoot()
            => Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                ".."));
    }
}
