using System.Collections.Concurrent;
using System.Text.Json;
using Jolt.Compiler;
using Jolt.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

public sealed partial class Server(Stream input, Stream output)
{
    private readonly Transport _transport = new(input, output);
    private readonly ConcurrentDictionary<string, Buffer> _buffers = new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _requests = new();
    private string? _root;
    private bool _visualStudioClassifications;
    private bool _refreshSemanticTokens;
    private bool _fileRename;
    private bool _changeAnnotations;
    private bool _shutdown;

    public static async Task RunStdioAsync(CancellationToken cancellationToken)
    {
        using var input = StdioInput.Open();
        using var output = Console.OpenStandardOutput();
        await new Server(input, output).RunAsync(cancellationToken);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _lifetime = lifetime.Token;
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
        await lifetime.CancelAsync();
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
            else if (args.TryGetProperty("workspaceFolders", out var folders) && folders.ValueKind == JsonValueKind.Array &&
                folders.GetArrayLength() == 1)
                _root = FilePath(folders[0].GetProperty("uri"));
            _visualStudioClassifications = args.TryGetProperty("initializationOptions", out var options) &&
                options.ValueKind == JsonValueKind.Object &&
                options.TryGetProperty("visualStudioClassifications", out var classify) && classify.ValueKind == JsonValueKind.True;
            _refreshSemanticTokens = args.TryGetProperty("capabilities", out var client) &&
                client.TryGetProperty("workspace", out var workspace) && workspace.TryGetProperty("semanticTokens", out var tokens) &&
                tokens.TryGetProperty("refreshSupport", out var refresh) && refresh.ValueKind == JsonValueKind.True;
            if (client.ValueKind == JsonValueKind.Object && client.TryGetProperty("workspace", out var clientWorkspace) &&
                clientWorkspace.TryGetProperty("workspaceEdit", out var edit))
            {
                _fileRename = edit.TryGetProperty("documentChanges", out var changes) && changes.ValueKind == JsonValueKind.True &&
                    edit.TryGetProperty("resourceOperations", out var operations) && operations.EnumerateArray().Any(operation => operation.GetString() == "rename");
                _changeAnnotations = edit.TryGetProperty("changeAnnotationSupport", out var annotations) && annotations.ValueKind == JsonValueKind.Object;
            }
            return new
            {
                capabilities = new
                {
                    positionEncoding = "utf-16",
                    textDocumentSync = new { openClose = true, change = 2, save = true },
                    completionProvider = new { triggerCharacters = new[] { "<", " ", ".", "@", "#", "\"", ":" }, resolveProvider = true },
                    hoverProvider = true,
                    definitionProvider = true,
                    referencesProvider = true,
                    renameProvider = new { prepareProvider = true },
                    documentFormattingProvider = true,
                    documentRangeFormattingProvider = true,
                    codeActionProvider = new { codeActionKinds = new[] { "quickfix" } },
                    signatureHelpProvider = new { triggerCharacters = new[] { "(", "," } },
                    semanticTokensProvider = new
                    {
                        legend = new { tokenTypes = SemanticTokens.Legend(_visualStudioClassifications), tokenModifiers = Array.Empty<string>() },
                        full = true,
                        range = true,
                    },
                },
                serverInfo = new { name = "Jolt" },
            };
        }
        if (method == "shutdown")
        {
            _shutdown = true;
            return null;
        }
        if (_shutdown || method is not ("textDocument/completion" or "completionItem/resolve" or "textDocument/hover" or "textDocument/definition" or
            "textDocument/references" or "textDocument/prepareRename" or "textDocument/rename" or
            "textDocument/formatting" or "textDocument/rangeFormatting" or "textDocument/codeAction" or
            "textDocument/signatureHelp" or "textDocument/semanticTokens/full" or "textDocument/semanticTokens/range"))
            throw new NotSupportedException(method);

        var resolve = method == "completionItem/resolve";
        JsonElement completionData = default;
        if (resolve && (!args.TryGetProperty("data", out completionData) || completionData.ValueKind != JsonValueKind.Object))
            return args;
        var path = FilePath(resolve ? completionData.GetProperty("uri") : args.GetProperty("textDocument").GetProperty("uri"));
        _buffers.TryGetValue(path, out var requestedBuffer);
        if (resolve && completionData.GetProperty("version") is { ValueKind: JsonValueKind.Number } version &&
            requestedBuffer?.Version != version.GetInt32())
            return args;
        var project = ProjectFor(path);
        var versioned = GetProject(project);
        var snapshot = await versioned.Input.WaitAsync(cancellationToken);
        if (snapshot.Npm is { IsCompletedSuccessfully: true } ready)
            snapshot = ready.Result;
        if (resolve && (!completionData.TryGetProperty("snapshotId", out var snapshotId) ||
            !snapshotId.TryGetInt64(out var id) || id != snapshot.Id))
            return args;
        var document = snapshot.Analysis.Documents.FirstOrDefault(item => PathEquals(Path.GetFullPath(item.Document.Path, snapshot.Directory), path));
        if (document is null)
            return resolve ? args : null;
        var text = SourceText.From(document.Document.Source);
        var features = new LanguageFeatures(document, snapshot.Analysis, snapshot.Workspace, snapshot.Directory, snapshot.Id);
        var result = method switch
        {
            "textDocument/semanticTokens/full" => await features.ClassifyAsync(new TextSpan(0, text.Length), _visualStudioClassifications, cancellationToken),
            "textDocument/semanticTokens/range" => await features.ClassifyAsync(ReadRange(text, args.GetProperty("range")), _visualStudioClassifications, cancellationToken),
            "completionItem/resolve" => await features.ResolveCompletionAsync(args, completionData, cancellationToken),
            "textDocument/formatting" => await features.FormatAsync(new TextSpan(0, text.Length), args.GetProperty("options"), cancellationToken),
            "textDocument/rangeFormatting" => await features.FormatAsync(ReadRange(text, args.GetProperty("range")), args.GetProperty("options"), cancellationToken),
            "textDocument/codeAction" => features.CodeActions(args.GetProperty("context"), ReadRange(text, args.GetProperty("range")),
                requestedBuffer?.Version, cancellationToken),
            "textDocument/references" or "textDocument/prepareRename" or "textDocument/rename" =>
                await features.RefactorAsync(method, Offset(text, args.GetProperty("position")),
                    method == "textDocument/references" && args.GetProperty("context").GetProperty("includeDeclaration").GetBoolean(),
                    method == "textDocument/rename" ? args.GetProperty("newName").GetString() : null, _fileRename, _changeAnnotations,
                    documentPath => _buffers.TryGetValue(documentPath, out var buffer) ? buffer.Version : null, cancellationToken),
            _ => await features.GetAsync(method, Offset(text, args.GetProperty("position")), cancellationToken, requestedBuffer?.Version),
        };
        _buffers.TryGetValue(path, out var currentBuffer);
        if (requestedBuffer != currentBuffer || !IsCurrent(project, versioned))
            throw new OperationCanceledException("The author document changed while handling the request.");
        return result;
    }

    private async Task NotifyAsync(string method, JsonElement args, CancellationToken stopping)
    {
        try
        {
            var affected = new HashSet<ProjectState>();
            switch (method)
            {
                case "textDocument/didOpen":
                {
                    var doc = args.GetProperty("textDocument");
                    var path = FilePath(doc.GetProperty("uri"));
                    _buffers[path] = new(SourceText.From(doc.GetProperty("text").GetString()!), doc.GetProperty("version").GetInt32());
                    if (IsJazor(path))
                        ProjectFor(path);
                    affected.UnionWith(Invalidate(path));
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
                    affected.UnionWith(Invalidate(path));
                    break;
                }
                case "textDocument/didClose":
                {
                    var uri = args.GetProperty("textDocument").GetProperty("uri");
                    var path = FilePath(uri);
                    _buffers.TryRemove(path, out _);
                    affected.UnionWith(Invalidate(path));
                    if (IsJazor(path))
                        await PublishAsync(uri.GetString()!, null, [], stopping);
                    break;
                }
                case "textDocument/didSave":
                    // Saved TypeScript declarations are read from disk by the npm provider.
                    affected.UnionWith(Invalidate(FilePath(args.GetProperty("textDocument").GetProperty("uri")), fileSystem: true));
                    break;
                case "workspace/didChangeWatchedFiles":
                    foreach (var change in args.GetProperty("changes").EnumerateArray())
                    {
                        var path = FilePath(change.GetProperty("uri"));
                        // Editors also watch .mjs tooling resources. Rebuilding on these events restarts the query that wrote them.
                        if (IsVueMetadataResource(path))
                            continue;
                        affected.UnionWith(Invalidate(path, fileSystem: true));
                        if (Path.GetFileName(path) == "package.json")
                            foreach (var buffer in _buffers.Keys.Where(open => IsJazor(open) && IsWithin(Path.GetDirectoryName(path)!, open)))
                                affected.Add(ProjectFor(buffer));
                    }
                    break;
                default:
                    return;
            }
            await Task.WhenAll(affected.Select(ValidateAsync));

            async Task ValidateAsync(ProjectState project)
            {
                using var validation = CancellationTokenSource.CreateLinkedTokenSource(stopping, _lifetime);
                lock (_projectsLock)
                {
                    project.Validation?.Cancel();
                    project.Validation = validation;
                }
                try
                {
                    await ValidateProjectAsync(project, validation.Token);
                }
                finally
                {
                    lock (_projectsLock)
                        if (project.Validation == validation)
                            project.Validation = null;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
        }
    }

    private async Task ValidateProjectAsync(ProjectState project, CancellationToken cancellationToken)
    {
        await Task.Delay(125, cancellationToken);
        var buffers = _buffers.ToArray().Where(entry => IsJazor(entry.Key) && IsWithin(project.Directory, entry.Key) &&
            PathEquals(ProjectDirectory(entry.Key), project.Directory)).ToArray();
        if (buffers.Length == 0)
            return;
        var versioned = GetProject(project);
        var snapshot = await versioned.Input.WaitAsync(cancellationToken);
        if (snapshot.Npm is { } npm)
        {
            if (!npm.IsCompletedSuccessfully)
                await PublishProject(snapshot, buffers, project, versioned, cancellationToken);
            snapshot = await npm.WaitAsync(cancellationToken);
        }
        await PublishProject(snapshot, buffers, project, versioned, cancellationToken);
        bool refresh;
        lock (_projectsLock)
            refresh = _refreshSemanticTokens && project.Version == versioned &&
                snapshot.NpmIdentity is { } ready && project.ClassifiedNpm != ready;
        if (refresh)
        {
            await _transport.WriteAsync(new { jsonrpc = "2.0", id = "jolt:semanticTokens:" + Interlocked.Increment(ref _serverRequestId),
                method = "workspace/semanticTokens/refresh" }, cancellationToken);
            lock (_projectsLock)
                project.ClassifiedNpm = snapshot.NpmIdentity;
        }
    }

    private async Task PublishProject(ProjectSnapshot snapshot, IEnumerable<KeyValuePair<string, Buffer>> documents, ProjectState project,
        VersionedProject versioned, CancellationToken cancellationToken)
    {
        var diagnostics = snapshot.Workspace.Diagnostics.Concat(snapshot.Analysis.Diagnostics);
        foreach (var entry in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsCurrent(project, versioned) || !_buffers.TryGetValue(entry.Key, out var buffer) || buffer != entry.Value)
                continue;
            var sourceDiagnostics = diagnostics.Select(d => (Diagnostic: d, Span: d.Location.GetMappedLineSpan()))
                    .Where(pair => pair.Span.IsValid && PathEquals(Path.GetFullPath(pair.Span.Path, snapshot.Directory), entry.Key))
                    .Where(pair => snapshot.Npm is null || !snapshot.NpmImports.Contains(pair.Span.Path) || pair.Diagnostic.Id != "JOLT1001");
            var result = sourceDiagnostics.Select(pair => (object)new
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
                    }).ToList();
            if (snapshot.Npm is not null && snapshot.NpmImports.Contains(Path.GetRelativePath(snapshot.Directory, entry.Key).Replace('\\', '/')))
                result.Add(new { range = Range(new LinePositionSpan(default, default)), severity = 3,
                    code = "JOLT1018", source = "Jolt", message = "Loading npm declarations." });
            await PublishAsync(new Uri(entry.Key).AbsoluteUri, buffer.Version, result.ToArray(), cancellationToken);
        }
    }

    private Task PublishAsync(string uri, int? version, object[] diagnostics, CancellationToken cancellationToken) =>
        _transport.WriteAsync(new { jsonrpc = "2.0", method = "textDocument/publishDiagnostics", @params = new { uri, version, diagnostics } }, cancellationToken);

    private static bool IsVueMetadataResource(string path)
    {
        const string prefix = "vue-metadata-";
        for (var directory = Path.GetDirectoryName(path); directory is not null; directory = Path.GetDirectoryName(directory))
        {
            var name = Path.GetFileName(directory);
            if (name.StartsWith(prefix, StringComparison.Ordinal) && Guid.TryParseExact(name.AsSpan(prefix.Length), "N", out _) &&
                PathEquals(Path.GetFileName(Path.GetDirectoryName(directory)), ".jazor"))
                return true;
        }
        return false;
    }

    private string ProjectDirectory(string path)
    {
        string? projectRoot = null;
        string? solutionRoot = null;
        for (var directory = Directory.GetParent(path); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "package.json")))
                return directory.FullName;
            if (directory.Exists)
            {
                if (projectRoot is null && directory.EnumerateFiles("*.csproj").Any())
                    projectRoot = directory.FullName;
                if (solutionRoot is null && (directory.EnumerateFiles("*.slnx").Any() || directory.EnumerateFiles("*.sln").Any()))
                    solutionRoot = directory.FullName;
            }
            if (PathEquals(directory.FullName, _root))
                return projectRoot ?? directory.FullName;
        }
        return projectRoot ?? solutionRoot ?? Path.GetDirectoryName(path)!;
    }

    private static string FilePath(JsonElement uri)
    {
        var value = uri.GetString()!;
        const string localFilePrefix = "file:///";
        var driveIndex = localFilePrefix.Length;
        // VS Code escapes the drive separator. Uri recognizes a DOS path only with a literal colon.
        if (OperatingSystem.IsWindows() && value.StartsWith(localFilePrefix, StringComparison.OrdinalIgnoreCase) &&
            value.Length > driveIndex + 4 && char.IsAsciiLetter(value[driveIndex]) && value[driveIndex + 4] == '/' &&
            value.AsSpan(driveIndex + 1, 3).Equals("%3A", StringComparison.OrdinalIgnoreCase))
            value = value[..(driveIndex + 1)] + ":" + value[(driveIndex + 4)..];

        return Path.GetFullPath(new Uri(value).LocalPath);
    }

    internal static bool PathEquals(string? left, string? right) =>
        string.Equals(left, right, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static int Offset(SourceText text, JsonElement position)
    {
        var line = text.Lines[position.GetProperty("line").GetInt32()];
        return line.Start + Math.Clamp(position.GetProperty("character").GetInt32(), 0, line.Span.Length);
    }

    private static TextSpan ReadRange(SourceText text, JsonElement range) =>
        TextSpan.FromBounds(Offset(text, range.GetProperty("start")), Offset(text, range.GetProperty("end")));

    internal static object Range(LinePositionSpan span) => new
    {
        start = new { line = span.Start.Line, character = span.Start.Character },
        end = new { line = span.End.Line, character = span.End.Character },
    };

    private sealed record Buffer(SourceText Text, int Version);
}
