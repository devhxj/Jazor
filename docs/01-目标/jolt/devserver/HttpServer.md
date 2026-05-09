# DevServer HTTP 服务器

`DevHttpServer`（`src/Jolt/DevServer/DevHttpServer.cs`），基于 Kestrel HTTP 服务器，提供按需编译、HTML 转换、Source Map 服务、HMR WebSocket 和 API 代理功能。

## 核心类型

### DevHttpServer

**职责**：Kestrel HTTP 服务器封装，处理所有开发模式下的 HTTP 请求。

**核心成员**：
```csharp
internal sealed class DevHttpServer : IAsyncDisposable, IWorkspaceDocumentChangeSink
{
    private readonly DevServerOptions _options;
    private readonly OnDemandCompiler _compiler;
    private readonly ModuleResolver _moduleResolver;
    private readonly HtmlTransformer _htmlTransformer;
    private readonly IJoltWorkspaceStore? _workspaceStore;
    private readonly DevServerProxy? _proxy;
    private readonly DevServerReloadHub _reloadHub;
    private readonly ChangeProcessor _changeProcessor;

    public Uri? ListeningUri { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken);
    public async ValueTask OnWorkspaceDocumentChangedAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken);
}
```

**启动流程**：
```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    var builder = WebApplication.CreateSlimBuilder();
    builder.Logging.ClearProviders(); // 避免 LSP 流污染
    builder.WebHost.UseUrls($"http://{_options.Host}:{_options.Port}");

    var application = builder.Build();
    if (_options.HmrEnabled)
    {
        application.UseWebSockets();
        application.Map("/@jazor/hmr", async context =>
        {
            using var socket = await context.WebSockets.AcceptWebSocketAsync();
            await _reloadHub.AcceptAsync(socket, context.RequestAborted);
        });
    }

    // Dev 客户端脚本端点
    application.MapGet("/@jazor/client", static (HttpContext context) =>
    {
        ApplyNoCacheHeaders(context.Response);
        return Results.Text(HtmlTransformer.GetDevClientScript(), "text/javascript");
    });

    // 请求处理路由
    application.Map("/{**requestPath}", async (HttpContext context) =>
    {
        if (_proxy is not null && await _proxy.TryProxyAsync(context))
        {
            return;
        }
        var result = await HandleRequestAsync(context);
        await result.ExecuteAsync(context);
    });

    await application.StartAsync(cancellationToken);
    StartFileWatcher();
}
```

### 请求处理管道

**HandleRequestAsync**（第 194-231 行）：
```csharp
private async Task<IResult> HandleRequestAsync(HttpContext context)
{
    var requestPath = context.Request.Path.Value ?? "/";

    // 1. Source Map 请求处理
    if (TryGetSourceMapRequestPath(requestPath, out var sourceRequestPath))
    {
        ApplyNoCacheHeaders(context.Response);
        return await HandleSourceMapRequestAsync(sourceRequestPath, context.RequestAborted);
    }

    // 2. 路径解析
    var resolved = _moduleResolver.Resolve(requestPath);
    if (!resolved.Found)
    {
        return Results.NotFound(resolved.Error);
    }

    // 3. 安全检查：防止路径逃逸
    if (!IsInsideRoot(resolved.AbsolutePath))
    {
        return Results.NotFound("Resolved path escapes the dev-server root.");
    }

    ApplyNoCacheHeaders(context.Response);

    // 4. HTML 服务
    if (resolved.ResolvedUrl.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
    {
        var html = await File.ReadAllTextAsync(resolved.AbsolutePath, context.RequestAborted);
        return Results.Text(_htmlTransformer.Transform(html, resolved.AbsolutePath), "text/html");
    }

    // 5. 编译服务（.jazor, .vue, .ts, .js, .css）
    var result = await CompileResolvedRequestAsync(resolved.AbsolutePath, context.RequestAborted);
    if (result.IsError)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }

    return Results.Text(result.Content, result.ContentType);
}
```

### ModuleResolver

**职责**：请求路径解析、别名支持、扩展名解析、虚拟路径处理。

**核心方法**（`src/Jolt/DevServer/ModuleResolver.cs`）：
```csharp
public sealed class ModuleResolver
{
    private readonly string _rootDirectory;
    private readonly IReadOnlyList<ResolveAliasRule> _resolveAliasRules;

    public ResolveResult Resolve(string requestPath, string? importerPath = null)
    {
        // 1. 处理根路径
        if (string.Equals(sanitizedRequestPath, "/", StringComparison.Ordinal))
        {
            return ResolveAbsolutePath(Path.Combine(_rootDirectory, "index.html"), "/index.html");
        }

        // 2. 处理虚拟路径（/@jazor/*）
        if (sanitizedRequestPath.StartsWith("/@jazor/", StringComparison.Ordinal))
        {
            return new ResolveResult { IsVirtual = true, Found = true };
        }

        // 3. 处理别名路径
        if (TryResolveAliasPath(sanitizedRequestPath, out var aliasedAbsolutePath))
        {
            return ResolveCandidate(aliasedAbsolutePath, BuildResolvedUrl(aliasedAbsolutePath));
        }

        // 4. 处理绝对路径（以 / 开头）
        if (sanitizedRequestPath.StartsWith("/", StringComparison.Ordinal))
        {
            var relativePath = sanitizedRequestPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return ResolveCandidate(Path.Combine(_rootDirectory, relativePath), sanitizedRequestPath);
        }

        // 5. 处理相对路径（基于 importerPath）
        var baseDirectory = importerPath is null ? _rootDirectory : Path.GetDirectoryName(importerPath) ?? _rootDirectory;
        var combinedPath = Path.GetFullPath(Path.Combine(baseDirectory, sanitizedRequestPath.Replace('/', Path.DirectorySeparatorChar)));
        return ResolveCandidate(combinedPath, BuildResolvedUrl(combinedPath));
    }

    public string GetResolvedUrlForAbsolutePath(string absolutePath);
    public string GetStyleTargetIdForAbsolutePath(string absolutePath);
}
```

**别名解析规则**（第 179-201 行）：
```csharp
private bool TryResolveAliasPath(string requestPath, out string absolutePath)
{
    foreach (var aliasRule in _resolveAliasRules)
    {
        // 精确匹配或前缀匹配（要求后跟 /）
        if (!TryMatchAlias(requestPath, aliasRule.Prefix, out var suffix))
        {
            continue;
        }

        absolutePath = string.IsNullOrEmpty(suffix)
            ? aliasRule.AbsoluteTargetPath
            : Path.Combine(aliasRule.AbsoluteTargetPath, suffix.Replace('/', Path.DirectorySeparatorChar));
        return true;
    }

    return false;
}
```

**扩展名自动补全**（第 86-112 行）：
```csharp
private ResolveResult ResolveCandidate(string absolutePath, string resolvedUrl)
{
    if (!string.IsNullOrWhiteSpace(Path.GetExtension(absolutePath)))
    {
        return ResolveAbsolutePath(absolutePath, resolvedUrl);
    }

    // 尝试自动补全扩展名
    foreach (var extension in SupportedExtensions) // [".jazor", ".vue", ".ts", ".js", ".css", ".html"]
    {
        var candidate = absolutePath + extension;
        var result = ResolveAbsolutePath(candidate, BuildResolvedUrl(candidate));
        if (result.Found)
        {
            return result;
        }
    }

    return new ResolveResult { Found = false, Error = $"Could not resolve '{resolvedUrl}'." };
}
```

### DevServerProxy

**职责**：API 代理支持，将特定路径请求转发到后端服务。

**配置**（通过 `DevServerOptions.ProxyRules`）：
```csharp
internal readonly record struct ProxyTarget
{
    public string Target { get; init; }              // 目标 URL（如 http://localhost:8080）
    public bool Secure { get; init; }                // 是否使用 HTTPS
    public bool WebSocket { get; init; }             // 是否支持 WebSocket
    public bool? RewritePath { get; init; }          // 是否重写路径
}
```

## 核心算法

### 路径解析算法

**输入**：HTTP 请求路径（如 `/src/App.vue` 或 `@/components/Button.vue`）
**输出**：绝对路径 + 解析 URL + 文档类型

**步骤**：
1. **去除查询参数和 Hash**：`/src/App.vue?t=123` → `/src/App.vue`
2. **虚拟路径检测**：`/@jazor/hmr` → 标记为虚拟路径
3. **别名匹配**：`@/components/Button.vue` → `{root}/src/components/Button.vue`
4. **绝对路径解析**：`/src/App.vue` → `{root}/src/App.vue`
5. **相对路径解析**：`./utils.ts`（从 `/src/App.vue` 导入）→ `{root}/src/utils.ts`
6. **扩展名补全**：`/src/App` → 尝试 `/src/App.jazor`, `/src/App.vue`, ...
7. **安全验证**：确保解析后的绝对路径在项目根目录内

**别名匹配规则**：
- **精确匹配**：`@` → `/src`
- **前缀匹配**：`@/components` → `/src/components`，要求后跟 `/`

### Source Map 请求检测

**TryGetSourceMapRequestPath**（第 706-716 行）：
```csharp
private static bool TryGetSourceMapRequestPath(string requestPath, out string sourceRequestPath)
{
    // /src/App.vue.map → /src/App.vue
    if (requestPath.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
    {
        sourceRequestPath = requestPath[..^4];
        return !string.IsNullOrWhiteSpace(sourceRequestPath);
    }

    sourceRequestPath = string.Empty;
    return false;
}
```

## 线程安全模型

### 文件变更处理

**Channel-based 异步处理**：
```csharp
private readonly Channel<IReadOnlyList<string>> _fileChangeChannel =
    Channel.CreateUnbounded<IReadOnlyList<string>>();

private async Task PumpFileChangesAsync(CancellationToken cancellationToken)
{
    await foreach (var changedPaths in _fileChangeChannel.Reader.ReadAllAsync(cancellationToken))
    {
        try
        {
            await ProcessAndBroadcastChangesAsync(changedPaths, cancellationToken);
        }
        catch (Exception ex)
        {
            await _reloadHub.BroadcastErrorAsync(
                $"Hot update failed while processing file changes: {ex.Message}",
                cancellationToken);
        }
    }
}
```

### 广播快照同步

**锁保护**（第 552-608 行）：
```csharp
private readonly object _lastBroadcastSnapshotsLock = new();
private readonly Dictionary<string, DevServerObservedFileSnapshot?> _lastBroadcastSnapshots = new();
private readonly Dictionary<string, string> _pendingWorkspaceBroadcastHashes = new();

private IReadOnlyList<string> FilterAlreadyBroadcastChanges(IReadOnlyList<string> changedPaths)
{
    lock (_lastBroadcastSnapshotsLock)
    {
        // 去重和防抖逻辑
        foreach (var path in changedPaths)
        {
            var snapshot = CaptureObservedFileSnapshot(path);
            if (_lastBroadcastSnapshots.TryGetValue(path, out var previousSnapshot)
                && Nullable.Equals(previousSnapshot, snapshot))
            {
                continue; // 已广播过相同快照
            }

            pathsToProcess.Add(path);
        }
    }

    return pathsToProcess;
}
```

## 错误处理

### 路径逃逸防护

**IsInsideRoot**（第 718-732 行）：
```csharp
private bool IsInsideRoot(string absolutePath)
{
    var fullPath = Path.GetFullPath(absolutePath);
    var relativePath = Path.GetRelativePath(Path.GetFullPath(_options.RootDirectory), fullPath);
    return string.Equals(relativePath, ".", StringComparison.Ordinal)
        || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath));
}
```

### 编译错误处理

**CompileResolvedRequestAsync**（第 256-289 行）：
```csharp
private async Task<CompilationResult> CompileResolvedRequestAsync(
    string absolutePath,
    CancellationToken cancellationToken)
{
    // ...

    var result = await CompileResolvedRequestAsync(resolved.AbsolutePath, cancellationToken);
    if (result.IsError)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    }

    return Results.Text(result.Content, result.ContentType);
}
```

## 配置选项

**DevServerOptions**（`src/Jolt/DevServer/DevServerOptions.cs`）：
```csharp
internal sealed record DevServerOptions
{
    public string RootDirectory { get; init; } = Directory.GetCurrentDirectory();
    public int Port { get; init; } = 5173;
    public string Host { get; init; } = "localhost";
    public bool OpenBrowser { get; init; }
    public bool HmrEnabled { get; init; } = true;
    public string FrontendCompiler { get; init; } = "deno";
    public TimeSpan FileChangeDebounceInterval { get; init; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan FileChangePollingInterval { get; init; } = TimeSpan.FromSeconds(1);
    public IReadOnlyDictionary<string, ProxyTarget> ProxyRules { get; init; }
    public IReadOnlyDictionary<string, string> ResolveAliases { get; init; }
}
```

**环境变量覆盖**（第 348-360 行）：
```csharp
private static TimeSpan ResolveIntervalOverride(
    TimeSpan configuredInterval,
    string environmentVariableName)
{
    if (Environment.GetEnvironmentVariable(environmentVariableName) is { Length: > 0 } rawValue
        && int.TryParse(rawValue, out var milliseconds)
        && milliseconds > 0)
    {
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    return configuredInterval;
}
```

## 与其他子系统的交互

### 与 LSP 的集成

**IWorkspaceDocumentChangeSink** 实现（第 413-490 行）：
```csharp
public async ValueTask OnWorkspaceDocumentChangedAsync(
    DocumentSnapshot document,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    CancellationToken cancellationToken)
{
    // LSP 驱动的 HMR
    if (document.DocumentKind is DocumentKind.Jazor or DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript or DocumentKind.Css)
    {
        var normalizedDocument = new DocumentSnapshot(fullPath, document.DocumentKind, document.Text, document.Version);
        if (ShouldSuppressWorkspaceBroadcastForDiskSyncedSnapshot(normalizedDocument))
        {
            return; // 避免与磁盘文件系统事件重复广播
        }

        await ProcessAndBroadcastWorkspaceDocumentChangeAsync(normalizedDocument, openDocuments, cancellationToken);
        return;
    }

    // C# 代码后置文件变更
    if (document.DocumentKind == DocumentKind.CSharp
        && JoltWorkspaceResolver.TryResolveOwningJazorPath(fullPath, out _))
    {
        await ProcessAndBroadcastWorkspaceDocumentChangeAsync(normalizedDocument, openDocuments, cancellationToken);
        return;
    }
}
```

### 与 HtmlTransformer 的集成

**HTML 转换**（第 218-222 行）：
```csharp
if (resolved.ResolvedUrl.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
{
    var html = await File.ReadAllTextAsync(resolved.AbsolutePath, context.RequestAborted);
    return Results.Text(_htmlTransformer.Transform(html, resolved.AbsolutePath), "text/html");
}
```

### 与 OnDemandCompiler 的集成

**编译请求**（第 256-289 行）：
```csharp
private async Task<CompilationResult> CompileResolvedRequestAsync(
    string absolutePath,
    CancellationToken cancellationToken)
{
    if (_workspaceStore is null)
    {
        return await _compiler.CompileAsync(absolutePath, cancellationToken);
    }

    // 使用 LSP 跟踪的文档版本
    var trackedDocument = await _workspaceStore.GetDocumentAsync(absolutePath, cancellationToken);
    if (!absolutePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
    {
        return trackedDocument is null
            ? await _compiler.CompileAsync(absolutePath, cancellationToken)
            : await _compiler.CompileAsync(absolutePath, trackedDocument.Text, cancellationToken);
    }

    // .jazor 文件需要加载同伴 C# 文档
    var trackedCompanionDocuments = await GetTrackedCompanionDocumentsAsync(absolutePath, cancellationToken);
    if (trackedDocument is not null)
    {
        return await _compiler.CompileAsync(absolutePath, trackedDocument.Text, trackedCompanionDocuments, cancellationToken);
    }

    // ...
}
```

## 设计权衡

### FileSystemWatcher + 轮询混合模式

**原因**：
- **FileSystemWatcher**：在大多数操作系统上可靠，但某些编辑器（如 WSL + Vim）可能不触发事件
- **轮询备份**：确保即使 FileSystemWatcher 失败也能检测到文件变更

**实现**（第 317-346 行）：
```csharp
private void StartFileWatcher()
{
    _fileWatcher = new FileSystemWatcher(_options.RootDirectory)
    {
        IncludeSubdirectories = true,
        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size
    };
    _fileWatcher.Changed += OnFileChanged;
    _fileWatcher.Created += OnFileChanged;
    _fileWatcher.Deleted += OnFileChanged;
    _fileWatcher.Renamed += OnFileRenamed;
    _fileWatcher.EnableRaisingEvents = true;

    // 轮询备份
    _fileSnapshotPoller = new DevServerFileSnapshotPoller(
        _options.RootDirectory,
        _fileChangePollingInterval,
        OnDebouncedFileChanges);
    _fileSnapshotPoller.Start();
}
```

### 防抖 + 去重 + 快照比对

**三级过滤**：
1. **防抖**：`FileChangeDebouncer` 将 100 ms 内的多次变更合并
2. **去重**：比较文件快照（长度 + 修改时间），跳过相同内容
3. **Workspace 同步抑制**：LSP 变更与磁盘变更的哈希比对

**Workspace 同步抑制**（第 623-643 行）：
```csharp
private bool ShouldSuppressWorkspaceBroadcastForDiskSyncedSnapshot(DocumentSnapshot document)
{
    var fullPath = Path.GetFullPath(document.DocumentPath);
    if (!TryComputeFileContentHash(fullPath, out var diskHash))
    {
        return false;
    }

    var workspaceHash = ComputeContentHash(document.Text);
    if (!string.Equals(diskHash, workspaceHash, StringComparison.Ordinal))
    {
        return false;
    }

    var snapshot = CaptureObservedFileSnapshot(fullPath);
    lock (_lastBroadcastSnapshotsLock)
    {
        return _lastBroadcastSnapshots.TryGetValue(fullPath, out var previousSnapshot)
            && Nullable.Equals(previousSnapshot, snapshot);
    }
}
```

### No-Cache Headers

**原因**：开发模式下避免浏览器缓存编译结果

**实现**（第 734-739 行）：
```csharp
private static void ApplyNoCacheHeaders(HttpResponse response)
{
    response.Headers.CacheControl = "no-store";
    response.Headers.Pragma = "no-cache";
    response.Headers.Expires = "0";
}
```
