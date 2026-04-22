# File Watching & Configuration

> 状态：已实现
> 定位：文件变更检测、变更处理、依赖跟踪和配置管理

## 1. 文档定位

本文档描述 DevServer 的文件监听和配置系统，包括 `ChangeProcessor`、`FileChangeDebouncer`、`DependencyGraph`、`DevServerOptions` 和 `DevServerOptionsParser`。

## 2. 核心类型

### 2.1 ChangeProcessor

**职责**：文件变更处理和 HMR 策略决策。

**核心成员**（`src/Jolt/DevServer/ChangeProcessor.cs`）：
```csharp
internal sealed class ChangeProcessor
{
    private readonly OnDemandCompiler _compiler;
    private readonly ModuleResolver _moduleResolver;
    private readonly DependencyGraph _dependencyGraph;

    public async ValueTask<ChangeProcessingResult> ProcessChangesAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken);

    public async ValueTask<ChangeProcessingResult> ProcessWorkspaceDocumentChangeAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken);
}
```

**ChangeProcessingResult**（第 845-862 行）：
```csharp
internal sealed class ChangeProcessingResult
{
    public required ChangeUpdateKind UpdateKind { get; init; }
    public string? FullReloadReason { get; init; }
    public required IReadOnlyList<string> ChangedPaths { get; init; }
    public required IReadOnlyList<string> AffectedPaths { get; init; }
    public IReadOnlyList<string> ChangedCssUrls { get; init; } = [];
    public IReadOnlyList<InlineStyleUpdate> InlineStyleUpdates { get; init; } = [];
    public IReadOnlyList<JavaScriptHotUpdate> JavaScriptUpdates { get; init; } = [];
    public string? ErrorMessage { get; init; }
}
```

**ChangeUpdateKind**（第 837-843 行）：
```csharp
internal enum ChangeUpdateKind
{
    FullReload,        // 全页面重新加载
    StyleUpdate,       // CSS 样式更新（无刷新）
    JavaScriptUpdate,  // JavaScript 模块更新（HMR）
    Error              // 编译错误
}
```

**作用域约束**：
- 变更处理先解析 owning project，再计算受影响集合
- 隐式依赖发现只在 owning project 内展开
- HMR 广播只覆盖 owning project 的依赖闭包
- sibling project 的模块和诊断不会因为这次变更被顺带刷新

### 2.2 FileChangeDebouncer

**职责**：文件系统事件防抖，避免频繁变更触发过多编译。

**实现**（`src/Jolt/DevServer/FileChangeDebouncer.cs`）：
```csharp
internal sealed class FileChangeDebouncer : IDisposable
{
    private readonly TimeSpan _debounceInterval;
    private readonly Lock _gate = new();
    private readonly HashSet<string> _pendingPaths = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _flushCancellationSource;

    public event Action<IReadOnlyList<string>>? DebouncedChange;

    public FileChangeDebouncer(TimeSpan debounceInterval);

    public void Record(string path);
    public void Dispose();
}
```

**防抖逻辑**（第 65-102 行）：
```csharp
private async Task ScheduleFlushAsync(CancellationTokenSource flushCancellationSource)
{
    try
    {
        // 等待防抖间隔
        await Task.Delay(_debounceInterval, flushCancellationSource.Token);
    }
    catch (OperationCanceledException)
    {
        return; // 新的变更到来，取消旧的超时
    }
    catch (ObjectDisposedException)
    {
        return;
    }

    IReadOnlyList<string>? changedPaths = null;
    lock (_gate)
    {
        // 检查是否仍是当前的超时任务
        if (_disposed || !ReferenceEquals(_flushCancellationSource, flushCancellationSource))
        {
            return;
        }

        if (_pendingPaths.Count > 0)
        {
            changedPaths = _pendingPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray();
            _pendingPaths.Clear();
        }

        _flushCancellationSource = null;
    }

    flushCancellationSource.Dispose();
    if (changedPaths is not null)
    {
        DebouncedChange?.Invoke(changedPaths);
    }
}
```

### 2.3 DependencyGraph

**职责**：模块依赖跟踪，支持依赖查询和影响分析。

**实现**（`src/Jolt/DevServer/DependencyGraph.cs`）：
```csharp
internal sealed class DependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _dependenciesByModule = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _dependentsByDependency = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _gate = new();
    private readonly ModuleResolver? _moduleResolver;

    public void Record(string modulePath, IReadOnlyList<string> dependencies);
    public IReadOnlyList<string> GetDependencies(string modulePath);
    public IReadOnlyList<string> GetDependents(string modulePath);
    public IReadOnlyList<string> GetAllAffectedModules(string changedModulePath);
    public void Remove(string modulePath);
    public void Clear();
}
```

**Record**（第 15-46 行）：
```csharp
public void Record(string modulePath, IReadOnlyList<string> dependencies)
{
    var normalizedModulePath = NormalizeModulePath(modulePath);

    lock (_gate)
    {
        // 移除旧依赖关系
        RemoveCore(normalizedModulePath);

        // 规范化依赖路径
        var normalizedDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dependency in dependencies)
        {
            if (TryNormalizeDependency(normalizedModulePath, dependency, out var normalizedDependency))
            {
                normalizedDependencies.Add(normalizedDependency);
            }
        }

        // 记录模块 -> 依赖
        _dependenciesByModule[normalizedModulePath] = normalizedDependencies;

        // 记录依赖 -> 模块（反向索引）
        foreach (var dependency in normalizedDependencies)
        {
            if (!_dependentsByDependency.TryGetValue(dependency, out var dependents))
            {
                dependents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _dependentsByDependency[dependency] = dependents;
            }

            dependents.Add(normalizedModulePath);
        }
    }
}
```

**GetAllAffectedModules**（第 74-106 行）：
```csharp
public IReadOnlyList<string> GetAllAffectedModules(string changedModulePath)
{
    var normalizedChangedModulePath = NormalizeModulePath(changedModulePath);

    lock (_gate)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>();
        queue.Enqueue(normalizedChangedModulePath);

        // BFS 遍历依赖图
        while (queue.Count > 0)
        {
            var modulePath = queue.Dequeue();
            if (!_dependentsByDependency.TryGetValue(modulePath, out var dependents))
            {
                continue;
            }

            foreach (var dependent in dependents)
            {
                if (!visited.Add(dependent))
                {
                    continue;
                }

                queue.Enqueue(dependent);
            }
        }

        return visited.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
```

**项目边界**：
- `DependencyGraph` 只存放当前 owning project 的模块关系
- 跨项目文件即使在磁盘上可达，也不会自动并入这张图
- 这保证了 HMR 影响面不会越过 `.slnx` 定义的项目边界

### 2.4 DevServerOptions

**职责**：开发服务器配置选项。

**定义**（`src/Jolt/DevServer/DevServerOptions.cs`）：
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
        = new Dictionary<string, ProxyTarget>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, string> ResolveAliases { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);
}
```

### 2.5 DevServerOptionsParser

**职责**：解析 CLI 参数和配置文件。

**实现**（`src/Jolt/DevServer/DevServerOptionsParser.cs`）：
```csharp
internal static class DevServerOptionsParser
{
    public static DevServerOptions Parse(string[] args);

    private static DevServerOptions ApplyConfigFile(DevServerOptions options);
    private static bool TryGetOptionValue(string arg, string optionName, out string value);
    private static bool TryParseProxyRule(string value, out string prefix, out ProxyTarget target);
    private static bool TryParseAliasRule(string value, out string prefix, out string target);
}
```

### 2.6 JazorConfig

**职责**：`jazor.config.json` 配置文件结构。

**定义**（`src/Jolt/DevServer/JazorConfig.cs`）：
```csharp
internal sealed class JazorConfig
{
    public JazorServerConfig? Server { get; init; }
    public Dictionary<string, JazorProxyConfig>? Proxy { get; init; }
    public JazorResolveConfig? Resolve { get; init; }
    public JazorBuildConfig? Build { get; init; }
    public JazorExtensionsConfig? Extensions { get; init; }
}

internal sealed class JazorServerConfig
{
    public int? Port { get; init; }
    public string? Host { get; init; }
    public bool? Open { get; init; }
    public bool? Hmr { get; init; }
}

internal sealed class JazorProxyConfig
{
    public string? Target { get; init; }
    public bool? Secure { get; init; }
    public bool? WebSocket { get; init; }
    public string? RewritePath { get; init; }
}

internal sealed class JazorResolveConfig
{
    public Dictionary<string, string>? Alias { get; init; }
}
```

### 2.7 HtmlTransformer

**职责**：HTML 转换，包括脚本注入、link 重写和资源引用重写。

**核心方法**（`src/Jolt/DevServer/HtmlTransformer.cs`）：
```csharp
internal sealed class HtmlTransformer
{
    private readonly DevServerOptions _options;

    public HtmlTransformer(DevServerOptions options);

    public string Transform(string html);
    public string Transform(string html, string? htmlPath);

    public static string GetDevClientScript();
    public static string InjectScript(string html, string scriptPath);
    public static string InjectCss(string html, string cssPath);
    public static string RemoveDevScriptRefs(string html);
    public static string RemoveScriptReference(string html, string scriptPath);
    public static string RewriteAssetReferences(string html, IReadOnlyList<AssetInfo> assets);
}
```

**Transform**（第 60-93 行）：
```csharp
public string Transform(string html, string? htmlPath)
{
    ArgumentNullException.ThrowIfNull(html);

    // 1. 重写入口脚本（添加 type="module"）
    var transformedHtml = RewriteEntryScripts(html);

    // 2. 准备注入内容
    var builder = new StringBuilder(transformedHtml.Length + 256);
    builder.Append(VueImportMap).AppendLine();

    if (_options.HmrEnabled)
    {
        builder.Append("<script type=\"module\" src=\"")
            .Append(DevClientPath)
            .AppendLine("\"></script>");
    }

    var injection = builder.ToString();

    // 3. 注入到 </head> 前
    var headIndex = transformedHtml.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
    if (headIndex >= 0)
    {
        return transformedHtml.Insert(headIndex, injection);
    }

    // 4. 回退到 </body> 前
    var bodyIndex = transformedHtml.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
    if (bodyIndex >= 0)
    {
        return transformedHtml.Insert(bodyIndex, injection);
    }

    return injection + transformedHtml;
}
```

## 3. 核心算法

### 3.1 变更处理流程

**ProcessChangesCoreAsync**（第 124-208 行）：
```csharp
private async ValueTask<ChangeProcessingResult> ProcessChangesCoreAsync(
    IReadOnlyList<string> changedPaths,
    IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
    CancellationToken cancellationToken)
{
    // 1. 路由变更路径（处理 .jazor 的同伴文件）
    var normalizedChangedPaths = changedPaths
        .Where(static path => !string.IsNullOrWhiteSpace(path))
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Order(StringComparer.OrdinalIgnoreCase)
        .ToArray();
    var routedChanges = normalizedChangedPaths
        .Select(static path => ChangeRoute.Create(path))
        .ToArray();

    // 2. 检查分类重载条件
    var classifiedReload = TryCreateClassifiedReload(routedChanges, normalizedChangedPaths, documentOverrides);
    if (classifiedReload is not null)
    {
        return classifiedReload;
    }

    // 3. 尝试 SFC 热更新
    var sfcHotUpdate = await TryCreateSfcHotUpdateAsync(routedChanges, normalizedChangedPaths, documentOverrides, cancellationToken);
    if (sfcHotUpdate is not null)
    {
        return sfcHotUpdate;
    }

    // 4. 尝试脚本热更新
    var scriptHotUpdate = await TryCreateScriptHotUpdateAsync(normalizedChangedPaths, documentOverrides, cancellationToken);
    if (scriptHotUpdate is not null)
    {
        return scriptHotUpdate;
    }

    // 5. 尝试 CSS Module 热更新
    var cssModuleHotUpdate = await TryCreateCssModuleHotUpdateAsync(normalizedChangedPaths, documentOverrides, cancellationToken);
    if (cssModuleHotUpdate is not null)
    {
        return cssModuleHotUpdate;
    }

    // 6. 尝试样式更新
    var styleUpdate = await TryCreateStyleUpdateAsync(normalizedChangedPaths, documentOverrides, cancellationToken);
    if (styleUpdate is not null)
    {
        return styleUpdate;
    }

    // 7. 回退到全页面重新加载
    var affectedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var routedChange in routedChanges)
    {
        affectedModules.Add(routedChange.OriginalPath);
        affectedModules.Add(routedChange.EffectivePath);
        foreach (var affectedModule in _dependencyGraph.GetAllAffectedModules(routedChange.EffectivePath))
        {
            affectedModules.Add(affectedModule);
        }
    }

    foreach (var affectedModule in affectedModules)
    {
        _compiler.Invalidate(affectedModule);
    }

    var orderedAffectedModules = affectedModules.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    return new ChangeProcessingResult
    {
        UpdateKind = ChangeUpdateKind.FullReload,
        FullReloadReason = affectedModules.SetEquals(normalizedChangedPaths)
            ? "frontend-change"
            : "frontend-change-with-dependents",
        AffectedPaths = orderedAffectedModules,
        ChangedPaths = normalizedChangedPaths,
    };
}
```

### 3.2 分类重载检测

**TryCreateClassifiedReload**（第 210-258 行）：
```csharp
private static ChangeProcessingResult? TryCreateClassifiedReload(
    IReadOnlyList<ChangeRoute> routedChanges,
    IReadOnlyList<string> changedPaths,
    IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides)
{
    string? reason = null;
    foreach (var routedChange in routedChanges)
    {
        var fileName = Path.GetFileName(routedChange.OriginalPath);

        // 1. index.html 变更 -> 全页面重新加载
        if (string.Equals(fileName, "index.html", StringComparison.OrdinalIgnoreCase))
        {
            reason = "index-html-change";
            break;
        }

        // 2. jazor.config.json 变更 -> 全页面重新加载
        if (string.Equals(fileName, "jazor.config.json", StringComparison.OrdinalIgnoreCase))
        {
            reason = "config-change";
            break;
        }

        // 3. 文档覆盖变更（LSP） -> 跳过分类重载
        if (HasDocumentOverride(routedChange.OriginalPath, documentOverrides))
        {
            continue;
        }

        // 4. 同伴文件（.jazor.cs）变更 -> 跳过（由主文件处理）
        if (!File.Exists(routedChange.OriginalPath)
            && !string.Equals(routedChange.OriginalPath, routedChange.EffectivePath, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        // 5. 文件不存在 -> 全页面重新加载
        if (!File.Exists(routedChange.OriginalPath))
        {
            reason = "missing-file-change";
            break;
        }
    }

    return reason is null
        ? null
        : new ChangeProcessingResult
        {
            UpdateKind = ChangeUpdateKind.FullReload,
            FullReloadReason = reason,
            ChangedPaths = changedPaths,
            AffectedPaths = changedPaths
        };
}
```

### 3.3 SFC 热更新检测

**TryCreateSfcHotUpdateAsync**（第 398-512 行）：
```csharp
private async ValueTask<ChangeProcessingResult?> TryCreateSfcHotUpdateAsync(
    IReadOnlyList<ChangeRoute> routedChanges,
    IReadOnlyList<string> changedPaths,
    IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
    CancellationToken cancellationToken)
{
    // 1. 检查是否为 SFC 文件
    if (routedChanges.Count == 0
        || routedChanges.Any(static route =>
            !route.EffectivePath.EndsWith(".vue", StringComparison.OrdinalIgnoreCase)
            && !route.EffectivePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)))
    {
        return null;
    }

    var affectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var inlineStyleUpdates = new List<InlineStyleUpdate>(routedChanges.Count);
    var jsUpdates = new List<JavaScriptHotUpdate>(routedChanges.Count);

    // 2. 按文件分组处理
    foreach (var routeGroup in routedChanges
                 .GroupBy(static route => route.EffectivePath, StringComparer.OrdinalIgnoreCase)
                 .OrderBy(static group => group.Key, StringComparer.OrdinalIgnoreCase))
    {
        var changedPath = routeGroup.Key;

        // 3. 检查缓存
        if (!_compiler.TryGetCachedResult(changedPath, out var previousResult) || previousResult is null)
        {
            return null;
        }

        // 4. 重新编译
        var nextResult = await RecompileAsync(changedPath, documentOverrides, cancellationToken);
        if (nextResult.IsError)
        {
            return CreateErrorResult(changedPaths, affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(), nextResult.ErrorMessage);
        }

        if (previousResult.IsError)
        {
            return null;
        }

        // 5. 计算影响路径
        affectedPaths.Add(changedPath);
        foreach (var dependent in _dependencyGraph.GetAllAffectedModules(changedPath))
        {
            affectedPaths.Add(dependent);
        }

        // 6. 检查模块签名是否变更
        if (string.Equals(previousResult.ModuleSignature, nextResult.ModuleSignature, StringComparison.Ordinal))
        {
            // 模块签名未变 -> 仅检查样式更新
            if (!string.Equals(previousResult.StyleContent, nextResult.StyleContent, StringComparison.Ordinal))
            {
                inlineStyleUpdates.Add(
                    new InlineStyleUpdate
                    {
                        TargetId = _moduleResolver.GetStyleTargetIdForAbsolutePath(changedPath),
                        Content = nextResult.StyleContent ?? string.Empty
                    });
            }

            continue;
        }

        // 7. 检查是否支持 HMR
        if (!nextResult.SupportsHmr)
        {
            return null;
        }

        // 8. Jazor 文件特殊处理
        if (changedPath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            var manifestDiff = TryDiffJazorHotReload(previousResult, nextResult);
            if (manifestDiff is null)
            {
                return null;
            }

            if (manifestDiff.Action == RazorVueHotUpdateAction.FullReload)
            {
                return new ChangeProcessingResult
                {
                    UpdateKind = ChangeUpdateKind.FullReload,
                    FullReloadReason = manifestDiff.Reason,
                    ChangedPaths = changedPaths,
                    AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray()
                };
            }
        }

        // 9. 创建 JavaScript 热更新
        foreach (var update in CreateJavaScriptHotUpdates(changedPath, nextResult.SupportsHmr))
        {
            jsUpdates.Add(update);
        }
    }

    // 10. 返回结果
    if (jsUpdates.Count == 0)
    {
        return inlineStyleUpdates.Count == 0
            ? null
            : new ChangeProcessingResult
            {
                UpdateKind = ChangeUpdateKind.StyleUpdate,
                ChangedPaths = changedPaths,
                AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                InlineStyleUpdates = inlineStyleUpdates
            };
    }

    return new ChangeProcessingResult
    {
        UpdateKind = ChangeUpdateKind.JavaScriptUpdate,
        ChangedPaths = changedPaths,
        AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
        JavaScriptUpdates = jsUpdates
    };
}
```

### 3.4 配置解析

**Parse**（第 7-85 行）：
```csharp
public static DevServerOptions Parse(string[] args)
{
    var options = new DevServerOptions();

    // 1. 首先处理 --dev-root（用于定位配置文件）
    foreach (var arg in args)
    {
        if (TryGetOptionValue(arg, "--dev-root", out var rootDirectory) &&
            !string.IsNullOrWhiteSpace(rootDirectory))
        {
            options = options with { RootDirectory = Path.GetFullPath(rootDirectory) };
        }
    }

    if (string.IsNullOrWhiteSpace(options.RootDirectory))
    {
        options = options with { RootDirectory = Directory.GetCurrentDirectory() };
    }

    // 2. 应用配置文件
    options = ApplyConfigFile(options);

    // 3. 应用 CLI 参数（覆盖配置文件）
    foreach (var arg in args)
    {
        if (TryGetOptionValue(arg, "--dev-port", out var portValue) &&
            int.TryParse(portValue, out var port))
        {
            options = options with { Port = port };
            continue;
        }

        if (string.Equals(arg, "--no-hmr", StringComparison.OrdinalIgnoreCase))
        {
            options = options with { HmrEnabled = false };
            continue;
        }

        // ... 其他参数处理
    }

    return options;
}
```

**ApplyConfigFile**（第 87-170 行）：
```csharp
private static DevServerOptions ApplyConfigFile(DevServerOptions options)
{
    var configPath = Path.Combine(options.RootDirectory, "jazor.config.json");
    if (!File.Exists(configPath))
    {
        return options;
    }

    JazorConfig? config;
    try
    {
        config = JsonSerializer.Deserialize<JazorConfig>(
            File.ReadAllText(configPath),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }
    catch (JsonException ex)
    {
        throw new InvalidOperationException($"Failed to parse dev-server config '{configPath}'.", ex);
    }

    if (config is null)
    {
        return options;
    }

    // 应用服务器配置
    if (config.Server is not null)
    {
        if (config.Server.Port is { } port)
        {
            options = options with { Port = port };
        }

        if (!string.IsNullOrWhiteSpace(config.Server.Host))
        {
            options = options with { Host = config.Server.Host };
        }

        if (config.Server.Open is { } openBrowser)
        {
            options = options with { OpenBrowser = openBrowser };
        }

        if (config.Server.Hmr is { } hmrEnabled)
        {
            options = options with { HmrEnabled = hmrEnabled };
        }
    }

    // 应用路径别名配置
    if (config.Resolve?.Alias is not null)
    {
        foreach (var (prefix, target) in config.Resolve.Alias)
        {
            if (string.IsNullOrWhiteSpace(prefix) || string.IsNullOrWhiteSpace(target))
            {
                continue;
            }

            options = ApplyResolveAlias(options, prefix.Trim(), target.Trim());
        }
    }

    // 应用代理配置
    if (config.Proxy is not null)
    {
        foreach (var (prefix, proxyConfig) in config.Proxy)
        {
            if (!TryCreateProxyTarget(proxyConfig, out var proxyTarget))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(prefix) || !prefix.StartsWith("/", StringComparison.Ordinal))
            {
                continue;
            }

            options = ApplyProxyRule(options, prefix, proxyTarget);
        }
    }

    return options;
}
```

## 4. 线程安全模型

### 4.1 FileChangeDebouncer

**Lock 保护**（第 6 行）：
```csharp
private readonly Lock _gate = new();
```

**Record**（第 23-43 行）：
```csharp
public void Record(string path)
{
    Task flushTask;
    lock (_gate)
    {
        if (_disposed)
        {
            return;
        }

        _pendingPaths.Add(Path.GetFullPath(path));

        // 取消旧的超时任务
        _flushCancellationSource?.Cancel();
        _flushCancellationSource?.Dispose();

        // 创建新的超时任务
        _flushCancellationSource = new CancellationTokenSource();
        flushTask = ScheduleFlushAsync(_flushCancellationSource);
    }

    _ = flushTask;
}
```

### 4.2 DependencyGraph

**Lock 保护**（第 7 行）：
```csharp
private readonly Lock _gate = new();
```

**所有公共方法都使用 lock 保护**：
```csharp
public void Record(string modulePath, IReadOnlyList<string> dependencies)
{
    lock (_gate)
    {
        RemoveCore(normalizedModulePath);
        // ...
    }
}

public IReadOnlyList<string> GetAllAffectedModules(string changedModulePath)
{
    lock (_gate)
    {
        // BFS 遍历依赖图
        // ...
    }
}
```

## 5. 错误处理

### 5.1 配置文件解析错误

**ApplyConfigFile**（第 96-108 行）：
```csharp
try
{
    config = JsonSerializer.Deserialize<JazorConfig>(
        File.ReadAllText(configPath),
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
}
catch (JsonException ex)
{
    throw new InvalidOperationException($"Failed to parse dev-server config '{configPath}'.", ex);
}
```

### 5.2 路径别名解析错误

**TryNormalizeDependency**（第 150-181 行）：
```csharp
private bool TryNormalizeDependency(
    string modulePath,
    string dependency,
    out string normalizedDependency)
{
    normalizedDependency = string.Empty;

    // 跳过外部依赖和裸模块说明符
    if (string.IsNullOrWhiteSpace(dependency) || IsExternalSpecifier(dependency) || IsBareSpecifier(dependency))
    {
        return false;
    }

    if (_moduleResolver is null)
    {
        normalizedDependency = NormalizeModulePath(dependency);
        return true;
    }

    // 解析路径
    if (Path.IsPathFullyQualified(dependency) && File.Exists(dependency))
    {
        normalizedDependency = NormalizeModulePath(dependency);
        return true;
    }

    var resolved = _moduleResolver.Resolve(dependency, modulePath);
    if (!resolved.Found || resolved.IsVirtual)
    {
        return false; // 解析失败或虚拟路径 -> 不记录为依赖
    }

    normalizedDependency = resolved.AbsolutePath;
    return true;
}
```

## 6. 配置选项

### 6.1 CLI 参数

| 参数 | 说明 | 示例 |
|------|------|------|
| `--dev-root` | 项目根目录 | `--dev-root=/path/to/project` |
| `--dev-port` | HTTP 端口 | `--dev-port=3000` |
| `--dev-host` | HTTP 主机 | `--dev-host=0.0.0.0` |
| `--open-browser` | 自动打开浏览器 | `--open-browser` |
| `--no-hmr` | 禁用 HMR | `--no-hmr` |
| `--dev-frontend` | 前端编译器 | `--dev-frontend=deno` |
| `--dev-proxy` | API 代理 | `--dev-proxy=/api=http://localhost:8080` |
| `--dev-alias` | 路径别名 | `--dev-alias=@=/src` |

### 6.2 配置文件

**jazor.config.json**：
```json
{
  "server": {
    "port": 5173,
    "host": "localhost",
    "open": true,
    "hmr": true
  },
  "resolve": {
    "alias": {
      "@": "/src",
      "@components": "/src/components"
    }
  },
  "proxy": {
    "/api": {
      "target": "http://localhost:8080",
      "secure": false,
      "webSocket": true,
      "rewritePath": false
    }
  },
  "build": {
    "outDir": "dist",
    "sourceMap": "external",
    "minify": true,
    "target": "es2020",
    "codeSplitting": true,
    "assetsDir": "assets",
    "assetHashLength": 8,
    "chunkSizeWarningLimit": 500000,
    "incremental": false
  },
  "extensions": {
    "enabled": false,
    "directory": "extensions",
    "allowExternalDirectory": false,
    "disabled": [],
    "trusted": [],
    "trustedPublicKeys": {},
    "trustKeysFile": null,
    "requireAssemblyHash": false,
    "enforceProviderPermissions": false,
    "requireManifestSignature": false,
    "requireProcessIsolation": false,
    "maxIoCapability": null,
    "maxNetworkCapability": null,
    "loadLogFile": null,
    "loadEventRetention": null,
    "providerLogFile": null,
    "providerEventRetention": null
  }
}
```

## 7. 与其他子系统的交互

### 7.1 与 DevHttpServer 的集成

**文件变更监听**（`DevHttpServer.StartFileWatcher`，第 317-346 行）：
```csharp
private void StartFileWatcher()
{
    if (!_options.HmrEnabled || !Directory.Exists(_options.RootDirectory))
    {
        return;
    }

    _fileChangeCancellationSource = new CancellationTokenSource();
    _fileChangePump = PumpFileChangesAsync(_fileChangeCancellationSource.Token);

    // 防抖器
    _fileChangeDebouncer = new FileChangeDebouncer(_fileChangeDebounceInterval);
    _fileChangeDebouncer.DebouncedChange += OnDebouncedFileChanges;

    // 文件系统监听器
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

### 7.2 与 OnDemandCompiler 的集成

**依赖记录**（`OnDemandCompiler.PublishCompilationResult`，第 558 行）：
```csharp
_dependencyGraph?.Record(absolutePath, result.Dependencies);
```

**缓存失效时清理依赖**（第 179 行）：
```csharp
if (_cache.Invalidate(absolutePath))
{
    _dependencyGraph?.Remove(absolutePath);
    UnregisterSourceMap(absolutePath);
}
```

### 7.3 与 DevServerReloadHub 的集成

**广播 HMR 更新**（`DevHttpServer.BroadcastChangeResultAsync`，第 520-550 行）：
```csharp
private async Task BroadcastChangeResultAsync(
    ChangeProcessingResult result,
    CancellationToken cancellationToken)
{
    if (result.UpdateKind == ChangeUpdateKind.StyleUpdate)
    {
        await _reloadHub.BroadcastStyleUpdateAsync(
            result.ChangedCssUrls,
            result.InlineStyleUpdates,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            cancellationToken);
        return;
    }

    if (result.UpdateKind == ChangeUpdateKind.JavaScriptUpdate)
    {
        await _reloadHub.BroadcastJavaScriptUpdateAsync(
            result.JavaScriptUpdates,
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            cancellationToken);
        return;
    }

    if (result.UpdateKind == ChangeUpdateKind.Error)
    {
        await _reloadHub.BroadcastErrorAsync(result.ErrorMessage, cancellationToken);
        return;
    }

    await _reloadHub.BroadcastReloadAsync(result.FullReloadReason, cancellationToken);
}
```

## 8. 设计权衡

### 8.1 防抖间隔选择

**默认值**：100 毫秒

**权衡**：
- **更短间隔**（如 50ms）：更快响应，但更多编译任务
- **更长间隔**（如 300ms）：更少编译任务，但响应延迟高

**选择理由**：
- 100ms 对人类感知接近即时
- 大多数编辑器保存操作在 100ms 内完成
- 平衡响应速度和资源消耗

### 8.2 轮询备份间隔选择

**默认值**：1 秒

**权衡**：
- **更短间隔**（如 100ms）：更快检测，但 CPU 开销高
- **更长间隔**（如 5 秒）：降低 CPU 开销，但检测延迟高

**选择理由**：
- 1 秒间隔的 CPU 开销可接受（单次扫描约 5-10ms）
- 与 FileSystemWatcher 配合提供可靠保障
- 开发服务器的场景下，1 秒延迟可接受

### 8.3 依赖图规范化

**依赖过滤**（第 150-159 行）：
```csharp
private bool TryNormalizeDependency(
    string modulePath,
    string dependency,
    out string normalizedDependency)
{
    // 跳过外部依赖和裸模块说明符
    if (string.IsNullOrWhiteSpace(dependency) || IsExternalSpecifier(dependency) || IsBareSpecifier(dependency))
    {
        return false;
    }

    // ...
}
```

**原因**：
- 外部依赖（如 `vue`, `lodash`）不会变更，无需跟踪
- 裸模块说明符（如 `vue`, `lodash`）无法解析为文件路径
- 减少依赖图大小，提升性能

### 8.4 HMR 策略优先级

**处理优先级**（从高到低）：
1. **分类重载**：index.html、配置文件变更 -> 全页面重新加载
2. **SFC 热更新**：.jazor、.vue 文件 -> 组件级 HMR
3. **脚本热更新**：.ts、.js 文件 -> 模块级 HMR
4. **CSS Module 热更新**：.module.css 文件 -> 样式映射更新
5. **样式更新**：.css 文件 -> 样式刷新
6. **全页面重新加载**：回退方案

**原因**：
- 精细化更新优先，提升开发体验
- 回退到全页面重新加载确保一致性
- 优先级匹配开发者预期（UI 变更 > 逻辑变更 > 样式变更）

### 8.5 Workspace 文档覆盖处理

**BuildDocumentOverridesAsync**（第 54-113 行）：
```csharp
private async ValueTask<IReadOnlyDictionary<string, DocumentSnapshot>> BuildDocumentOverridesAsync(
    DocumentSnapshot document,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    CancellationToken cancellationToken)
{
    var overrides = new Dictionary<string, DocumentSnapshot>(StringComparer.OrdinalIgnoreCase)
    {
        [Path.GetFullPath(document.DocumentPath)] = document
    };

    var effectivePath = JoltWorkspaceResolver.TryResolveOwningJazorPath(document.DocumentPath, out var owningJazorPath)
        ? Path.GetFullPath(owningJazorPath)
        : Path.GetFullPath(document.DocumentPath);

    if (!effectivePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
    {
        return overrides;
    }

    // 加载同伴 C# 文档
    foreach (var candidatePath in EnumerateWorkspaceSourcePaths(effectivePath))
    {
        var normalizedCandidatePath = Path.GetFullPath(candidatePath);
        if (overrides.ContainsKey(normalizedCandidatePath))
        {
            continue;
        }

        // 优先使用 LSP 跟踪的文档版本
        var openDocument = openDocuments.FirstOrDefault(openDocument =>
            string.Equals(
                Path.GetFullPath(openDocument.DocumentPath),
                normalizedCandidatePath,
                StringComparison.OrdinalIgnoreCase));
        if (openDocument is not null)
        {
            overrides[normalizedCandidatePath] = new DocumentSnapshot(
                normalizedCandidatePath,
                openDocument.DocumentKind,
                openDocument.Text,
                openDocument.Version);
            continue;
        }

        // 回退到磁盘文件
        var resolvedDocument = await JoltWorkspaceResolver.ResolveDocumentAsync(
            normalizedCandidatePath,
            openDocuments,
            cancellationToken);
        if (resolvedDocument is not null)
        {
            overrides[normalizedCandidatePath] = new DocumentSnapshot(
                Path.GetFullPath(resolvedDocument.DocumentPath),
                resolvedDocument.DocumentKind,
                resolvedDocument.Text,
                resolvedDocument.Version);
        }
    }

    return overrides;
}
```

**原因**：
- .jazor 文件的编译依赖于同伴 C# 文档
- LSP 跟踪的文档版本是最新的（未保存的编辑）
- 确保编译结果与编辑器内容一致
