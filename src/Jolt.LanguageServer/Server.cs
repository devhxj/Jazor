using System.Collections.Concurrent;
using System.Text.Json;
using Jolt.Compiler;
using Jolt.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

public sealed class Server(Stream input, Stream output)
{
    private readonly Transport _transport = new(input, output);
    private readonly ConcurrentDictionary<string, Buffer> _buffers = new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _requests = new();
    private CancellationTokenSource? _analysis;
    private string? _root;
    private bool _shutdown;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var pending = new List<Task>();
        while (await _transport.ReadAsync(cancellationToken) is { } message)
        {
            pending.RemoveAll(task => task.IsCompletedSuccessfully);
            if (!message.TryGetProperty("method", out var methodValue))
                continue;
            var method = methodValue.GetString()!;
            message.TryGetProperty("params", out var args);
            if (method == "exit")
                break;

            if (message.TryGetProperty("id", out var id))
            {
                var request = RespondAsync(id, method, args, cancellationToken);
                if (method is "initialize" or "shutdown")
                    await request;
                else
                    pending.Add(request);
                continue;
            }

            if (method == "$/cancelRequest")
            {
                if (_requests.TryGetValue(args.GetProperty("id").GetRawText(), out var request))
                    request.Cancel();
                continue;
            }
            if (!_shutdown)
                pending.Add(NotifyAsync(method, args, cancellationToken));
        }
        _analysis?.Cancel();
        foreach (var request in _requests.Values)
            request.Cancel();
        await Task.WhenAll(pending);
    }

    private async Task RespondAsync(JsonElement id, string method, JsonElement args, CancellationToken stopping)
    {
        var key = id.GetRawText();
        using var request = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        if (!_requests.TryAdd(key, request))
            return;
        try
        {
            var result = await RequestAsync(method, args, request.Token);
            request.Token.ThrowIfCancellationRequested();
            await _transport.WriteAsync(new { jsonrpc = "2.0", id, result }, stopping);
        }
        catch (Exception error)
        {
            var code = error switch
            {
                OperationCanceledException => -32800,
                NotSupportedException => -32601,
                ArgumentException => -32602,
                _ => -32603,
            };
            await _transport.WriteAsync(new { jsonrpc = "2.0", id, error = new { code, message = error.Message } }, stopping);
        }
        finally
        {
            _requests.TryRemove(key, out _);
        }
    }

    private async Task<object?> RequestAsync(string method, JsonElement args, CancellationToken cancellationToken)
    {
        if (method == "initialize")
        {
            if (args.TryGetProperty("rootUri", out var uri) && uri.ValueKind == JsonValueKind.String)
                _root = FilePath(uri);
            return new
            {
                capabilities = new
                {
                    positionEncoding = "utf-16",
                    textDocumentSync = new { openClose = true, change = 2 },
                    completionProvider = new { triggerCharacters = new[] { "<", " ", ".", "@", "#", "\"" } },
                    hoverProvider = true,
                    definitionProvider = true,
                    signatureHelpProvider = new { triggerCharacters = new[] { "(", "," } },
                },
                serverInfo = new { name = "Jolt" },
            };
        }
        if (method == "shutdown")
        {
            _shutdown = true;
            return null;
        }
        if (_shutdown || method is not ("textDocument/completion" or "textDocument/hover" or "textDocument/definition" or "textDocument/signatureHelp"))
            throw new NotSupportedException(method);

        var path = FilePath(args.GetProperty("textDocument").GetProperty("uri"));
        _buffers.TryGetValue(path, out var requestedBuffer);
        var snapshot = await LoadAsync(path, cancellationToken);
        var position = args.GetProperty("position");
        var document = snapshot.Analysis.Documents.FirstOrDefault(item => PathEquals(Path.GetFullPath(item.Document.Path, snapshot.Directory), path));
        if (document is null)
            return null;
        var text = SourceText.From(document.Document.Source);
        var offset = Offset(text, position);
        var result = await new LanguageFeatures(document, snapshot.Analysis.Documents, snapshot.Workspace, snapshot.Directory).GetAsync(method, offset, cancellationToken);
        _buffers.TryGetValue(path, out var currentBuffer);
        if (requestedBuffer != currentBuffer)
            throw new OperationCanceledException("The author document changed while handling the request.");
        return result;
    }

    private async Task NotifyAsync(string method, JsonElement args, CancellationToken stopping)
    {
        try
        {
            switch (method)
            {
                case "textDocument/didOpen":
                {
                    var doc = args.GetProperty("textDocument");
                    var path = FilePath(doc.GetProperty("uri"));
                    _buffers[path] = new(SourceText.From(doc.GetProperty("text").GetString()!), doc.GetProperty("version").GetInt32());
                    break;
                }
                case "textDocument/didChange":
                {
                    var doc = args.GetProperty("textDocument");
                    var path = FilePath(doc.GetProperty("uri"));
                    var version = doc.GetProperty("version").GetInt32();
                    if (!_buffers.TryGetValue(path, out var buffer) || version <= buffer.Version)
                        return;
                    var text = buffer.Text;
                    foreach (var change in args.GetProperty("contentChanges").EnumerateArray())
                    {
                        var newText = change.GetProperty("text").GetString()!;
                        if (change.TryGetProperty("range", out var range))
                        {
                            var span = TextSpan.FromBounds(Offset(text, range.GetProperty("start")), Offset(text, range.GetProperty("end")));
                            text = text.WithChanges(new TextChange(span, newText));
                        }
                        else
                            text = SourceText.From(newText);
                    }
                    _buffers[path] = new(text, version);
                    break;
                }
                case "textDocument/didClose":
                {
                    var uri = args.GetProperty("textDocument").GetProperty("uri");
                    _buffers.TryRemove(FilePath(uri), out _);
                    await PublishAsync(uri.GetString()!, null, [], stopping);
                    break;
                }
                case "workspace/didChangeWatchedFiles":
                    break;
                default:
                    return;
            }
            var analysis = CancellationTokenSource.CreateLinkedTokenSource(stopping);
            Interlocked.Exchange(ref _analysis, analysis)?.Cancel();
            await ValidateAsync(analysis.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
        }
    }

    private async Task ValidateAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(125, cancellationToken);
        foreach (var group in _buffers.ToArray().GroupBy(entry => ProjectDirectory(entry.Key)))
        {
            var snapshot = await LoadAsync(group.First().Key, cancellationToken);
            var diagnostics = snapshot.Workspace.Diagnostics.Concat(snapshot.Analysis.Diagnostics);
            foreach (var entry in group)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!_buffers.TryGetValue(entry.Key, out var buffer) || buffer != entry.Value)
                    continue;
                var result = diagnostics.Select(d => (Diagnostic: d, Span: d.Location.GetMappedLineSpan()))
                    .Where(pair => pair.Span.IsValid && PathEquals(Path.GetFullPath(pair.Span.Path, snapshot.Directory), entry.Key))
                    .Select(pair => (object)new
                    {
                        range = Range(pair.Span.Span),
                        severity = pair.Diagnostic.Severity switch
                        {
                            DiagnosticSeverity.Error => 1,
                            DiagnosticSeverity.Warning => 2,
                            DiagnosticSeverity.Info => 3,
                            _ => 4,
                        },
                        code = pair.Diagnostic.Id,
                        source = "Jolt",
                        message = pair.Diagnostic.GetMessage(),
                    }).ToArray();
                await PublishAsync(new Uri(entry.Key).AbsoluteUri, buffer.Version, result, cancellationToken);
            }
        }
    }

    private Task PublishAsync(string uri, int? version, object[] diagnostics, CancellationToken cancellationToken) =>
        _transport.WriteAsync(new { jsonrpc = "2.0", method = "textDocument/publishDiagnostics", @params = new { uri, version, diagnostics } }, cancellationToken);

    private async Task<(string Directory, JoltWorkspace Workspace, JoltAnalysis Analysis)> LoadAsync(string path, CancellationToken cancellationToken)
    {
        var directory = ProjectDirectory(path);
        var overlays = _buffers.ToArray().Where(entry => Path.GetRelativePath(directory, entry.Key) is var relative &&
            relative != ".." && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) && !Path.IsPathRooted(relative))
            .ToDictionary(entry => entry.Key, entry => entry.Value.Text.ToString());
        var workspace = await JoltProjectLoader.LoadAsync(directory, overlays, cancellationToken);
        var analysis = JoltProjectCompiler.Analyze(workspace.Sources, workspace.Options, cancellationToken);
        return (directory, workspace, analysis);
    }

    private string ProjectDirectory(string path)
    {
        for (var directory = Directory.GetParent(path); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "package.json")))
                return directory.FullName;
            if (PathEquals(directory.FullName, _root))
                return directory.FullName;
        }
        return Path.GetDirectoryName(path)!;
    }

    private static string FilePath(JsonElement uri) => Path.GetFullPath(new Uri(uri.GetString()!).LocalPath);

    private static bool PathEquals(string? left, string? right) =>
        string.Equals(left, right, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static int Offset(SourceText text, JsonElement position)
    {
        var line = text.Lines[position.GetProperty("line").GetInt32()];
        return line.Start + Math.Clamp(position.GetProperty("character").GetInt32(), 0, line.Span.Length);
    }

    internal static object Range(LinePositionSpan span) => new
    {
        start = new { line = span.Start.Line, character = span.Start.Character },
        end = new { line = span.End.Line, character = span.End.Character },
    };

    private sealed record Buffer(SourceText Text, int Version);
}
