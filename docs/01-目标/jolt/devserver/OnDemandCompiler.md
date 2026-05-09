# On-Demand Compiler

`OnDemandCompiler`（`src/Jolt/DevServer/OnDemandCompiler.cs`，约 1011 行），按需编译 .jazor、.vue、.ts、.js、.css 文件为可执行的 JavaScript，支持 CSS Modules、Source Map 链接、HMR 元数据和构建模式转换。

## 核心类型

### OnDemandCompiler

**职责**：统一编译入口，根据文件扩展名分发到不同的编译逻辑。

**核心成员**：
```csharp
internal sealed class OnDemandCompiler
{
    private readonly JazorVueParser _parser;
    private readonly JazorVueCompiler _compiler;
    private readonly IFrontendModuleCompiler _frontendCompiler;
    private readonly CompilationCache _cache;
    private readonly DependencyGraph? _dependencyGraph;
    private readonly ModuleResolver? _moduleResolver;
    private readonly JazorHotReloadMetadataProvider _hotReloadMetadataProvider;
    private readonly ISourceMapService? _sourceMapService;
    private readonly bool _buildMode;
    private readonly Lock _stateGate = new();

    public DependencyGraph? DependencyGraph => _dependencyGraph;

    public async ValueTask<CompilationResult> CompileAsync(
        string absolutePath,
        CancellationToken cancellationToken);

    public async ValueTask<CompilationResult> RecompileAsync(
        string absolutePath,
        CancellationToken cancellationToken);

    public void Invalidate(string absolutePath);
    public void InvalidateAll();
}
```

### CompilationResult

**职责**：编译结果的统一表示。

**定义**（`src/Jolt/DevServer/CompilationResult.cs`）：
```csharp
internal sealed class CompilationResult
{
    public required string ContentType { get; init; }          // "text/javascript" 或 "text/css"
    public required string Content { get; init; }              // 编译后的内容
    public string? SourceMap { get; init; }                    // Source Map JSON
    public string? ModuleSignature { get; init; }              // 模块签名（SHA256）
    public RazorVueManifestEntry? HotReloadManifestEntry { get; init; } // Jazor HMR 元数据
    public string? StyleContent { get; init; }                 // 提取的样式内容
    public IReadOnlyList<CompiledStyleFragment> StyleFragments { get; init; } = [];
    public IReadOnlyList<string> Dependencies { get; init; } = [];          // 依赖模块列表
    public IReadOnlyList<string> EmbeddedStyleDependencies { get; init; } = []; // 嵌入的样式依赖
    public IReadOnlyDictionary<string, string> CssModuleMappings { get; init; } = new(); // CSS Modules 映射
    public IReadOnlyList<string> Diagnostics { get; init; } = [];           // 编译诊断信息
    public bool IsError { get; init; }                        // 是否编译失败
    public string? ErrorMessage { get; init; }                 // 错误消息
    public bool SupportsHmr { get; init; }                     // 是否支持 HMR
}
```

### CompilationCache

**职责**：LRU 缓存编译结果，避免重复编译。

**实现**（`src/Jolt/DevServer/CompilationCache.cs`）：
```csharp
internal sealed class CompilationCache
{
    internal const int DefaultMaxEntries = 512;

    private readonly int _maxEntries;
    private readonly Dictionary<string, CacheEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _leastRecentlyUsedPaths = new();
    private readonly Lock _gate = new();

    public bool TryGet(string absolutePath, string contentHash, [NotNullWhen(true)] out CompilationResult? result);
    public bool TryPeek(string absolutePath, [NotNullWhen(true)] out CompilationResult? result);
    public IReadOnlyList<string> Set(string absolutePath, string contentHash, CompilationResult result);
    public bool Invalidate(string absolutePath);
    public IReadOnlyList<string> InvalidateAll();
}
```

**LRU 淘汰算法**（第 135-153 行）：
```csharp
private IReadOnlyList<string> EvictOverflowCore()
{
    if (_entries.Count <= _maxEntries)
    {
        return [];
    }

    var evictedPaths = new List<string>();
    while (_entries.Count > _maxEntries && _leastRecentlyUsedPaths.Last is { } leastRecentlyUsed)
    {
        var evictedPath = leastRecentlyUsed.Value;
        if (RemoveCore(evictedPath))
        {
            evictedPaths.Add(evictedPath);
        }
    }

    return evictedPaths;
}
```

## 核心算法

### 编译分发

**CompileCoreAsync**（第 198-221 行）：
```csharp
private async ValueTask<CompilationResult> CompileCoreAsync(
    string absolutePath,
    string text,
    IReadOnlyList<DocumentSnapshot>? companionDocuments,
    CancellationToken cancellationToken)
{
    return Path.GetExtension(absolutePath).ToLowerInvariant() switch
    {
        ".jazor" => await CompileJazorAsync(absolutePath, text, companionDocuments, cancellationToken),
        ".vue" => await CompileVueAsync(absolutePath, text, cancellationToken),
        ".ts" => await CompileTypeScriptAsync(absolutePath, text, cancellationToken),
        ".js" => await CompileJavaScriptAsync(absolutePath, text, cancellationToken),
        ".css" => await CompileStyleAsync(absolutePath, text, cancellationToken),
        ".html" => CreatePassThrough("text/html", text),
        _ => new CompilationResult
        {
            ContentType = "text/plain",
            Content = string.Empty,
            IsError = true,
            ErrorMessage = $"Unsupported document '{absolutePath}'.",
            Diagnostics = [$"Unsupported document '{absolutePath}'."]
        }
    };
}
```

### .jazor 文件编译

**CompileJazorAsync**（第 272-309 行）：
```csharp
private async ValueTask<CompilationResult> CompileJazorAsync(
    string absolutePath,
    string text,
    IReadOnlyList<DocumentSnapshot>? companionDocuments,
    CancellationToken cancellationToken)
{
    // 1. Parse Jazor 文档
    var document = _parser.Parse(absolutePath, text);

    // 2. 编译为 Vue SFC
    var sfc = _compiler.Compile(document);

    // 3. 前端编译（Vue SFC -> JS）
    var module = await _frontendCompiler.CompileSfcAsync(absolutePath, sfc.GeneratedVueText, cancellationToken);
    if (module is null)
    {
        return CreateFrontendUnavailableResult(
            "Vue SFC compilation is not available because the frontend compiler is unavailable.",
            sfc.Diagnostics);
    }

    // 4. 创建 HMR 元数据
    var hotReloadManifestEntry = CreateJazorHotReloadManifestEntry(absolutePath, document, sfc, module, companionDocuments);

    // 5. 计算模块签名（用于 HMR 变更检测）
    var moduleSignature = ComputeJazorModuleSignature(module.JavaScript, hotReloadManifestEntry);

    // 6. 链接 Source Map（Vue -> Jazor）
    var chainedSourceMap = ChainJazorSourceMap(module.SourceMap, sfc);

    // 7. 准备 JavaScript（添加样式注入逻辑或构建转换）
    var preparedJavaScript = await PrepareJavaScriptForCurrentModeAsync(absolutePath, module.JavaScript, module.StyleContent, cancellationToken);

    // 8. 调整 Source Map 行号偏移
    var servedSourceMap = OffsetSourceMapGeneratedLines(chainedSourceMap, preparedJavaScript.GeneratedLineOffset);

    return new CompilationResult
    {
        ContentType = "text/javascript",
        Content = AttachInlineSourceMap(preparedJavaScript.Content, servedSourceMap),
        ModuleSignature = moduleSignature,
        HotReloadManifestEntry = hotReloadManifestEntry,
        SourceMap = servedSourceMap,
        StyleContent = module.StyleContent,
        StyleFragments = module.StyleFragments,
        Dependencies = module.Dependencies,
        EmbeddedStyleDependencies = module.EmbeddedStyleDependencies,
        Diagnostics = sfc.Diagnostics,
        IsError = false,
        SupportsHmr = module.SupportsHmr
    };
}
```

### .vue 文件编译

**CompileVueAsync**（第 311-336 行）：
```csharp
private async ValueTask<CompilationResult> CompileVueAsync(
    string absolutePath,
    string text,
    CancellationToken cancellationToken)
{
    // 1. 前端编译（Vue SFC -> JS）
    var module = await _frontendCompiler.CompileSfcAsync(absolutePath, text, cancellationToken);
    if (module is null)
    {
        return CreateFrontendUnavailableResult("Vue SFC compilation is not available because the frontend compiler is unavailable.");
    }

    // 2. 准备 JavaScript
    var preparedJavaScript = await PrepareJavaScriptForCurrentModeAsync(absolutePath, module.JavaScript, module.StyleContent, cancellationToken);

    // 3. 调整 Source Map 行号偏移
    var servedSourceMap = OffsetSourceMapGeneratedLines(module.SourceMap, preparedJavaScript.GeneratedLineOffset);

    return new CompilationResult
    {
        ContentType = "text/javascript",
        Content = AttachInlineSourceMap(preparedJavaScript.Content, servedSourceMap),
        ModuleSignature = ComputeContentHash(module.JavaScript),
        SourceMap = servedSourceMap,
        StyleContent = module.StyleContent,
        StyleFragments = module.StyleFragments,
        Dependencies = module.Dependencies,
        EmbeddedStyleDependencies = module.EmbeddedStyleDependencies,
        SupportsHmr = module.SupportsHmr
    };
}
```

### CSS Modules 编译

**CompileStyleAsync**（第 392-432 行）：
```csharp
private async ValueTask<CompilationResult> CompileStyleAsync(
    string absolutePath,
    string content,
    CancellationToken cancellationToken)
{
    // 1. 检测是否为 CSS Module
    if (!LooksLikeCssModulePath(absolutePath))
    {
        return CreateStylePassThrough(content);
    }

    // 2. 编译 CSS Module（生成哈希类名）
    var module = await _frontendCompiler.CompileCssModuleAsync(absolutePath, content, cancellationToken);
    if (module is null)
    {
        return CreateFrontendUnavailableResult("CSS Modules compilation is not available because the frontend compiler is unavailable.");
    }

    // 3. 构建模式：输出 CSS + 映射对象
    if (_buildMode)
    {
        return new CompilationResult
        {
            ContentType = "text/css",
            Content = module.CssContent,
            StyleContent = module.CssContent,
            CssModuleMappings = module.Mappings,
            Diagnostics = module.Diagnostics
        };
    }

    // 4. 开发模式：输出 JavaScript 模块（包含 HMR 支持）
    var cssModuleJavaScript = CreateCssModuleJavaScript(module.Mappings);
    var servedModule = CreateServedModule(absolutePath, cssModuleJavaScript, module.CssContent);
    return new CompilationResult
    {
        ContentType = "text/javascript",
        Content = servedModule.Content,
        ModuleSignature = ComputeContentHash(servedModule.Content),
        StyleContent = module.CssContent,
        CssModuleMappings = module.Mappings,
        Diagnostics = module.Diagnostics,
        SupportsHmr = true
    };
}
```

**CSS Module JavaScript 模块**（第 712-749 行）：
```csharp
private static string CreateCssModuleJavaScript(IReadOnlyDictionary<string, string> mappings)
{
    var serializedMappings = System.Text.Json.JsonSerializer.Serialize(
        mappings
            .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
            .ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal));

    return $$"""
        const __jazorHotContext = import.meta.hot ?? globalThis.__JAZOR_HMR__?.createHotContext(import.meta.url);
        if (__jazorHotContext) {
          import.meta.hot = __jazorHotContext;
        }

        const __jazorCssModules = {{serializedMappings}};
        function __jazorSyncCssModules(target, source) {
          for (const key of Object.keys(target)) {
            if (!(key in source)) {
              delete target[key];
            }
          }

          Object.assign(target, source);
        }

        export default __jazorCssModules;

        if (import.meta.hot) {
          import.meta.hot.accept((updatedModule) => {
            const updatedMappings = updatedModule?.default;
            if (!updatedMappings || typeof updatedMappings !== "object") {
              import.meta.hot.invalidate?.("CSS modules update payload was unavailable.");
              return;
            }

            __jazorSyncCssModules(__jazorCssModules, updatedMappings);
          });
        }
        """;
}
```

### 构建模式 CSS Modules 转换

**TransformBuildJavaScriptAsync**（第 616-637 行）：
```csharp
private async ValueTask<string> TransformBuildJavaScriptAsync(
    string documentPath,
    string javaScript,
    CancellationToken cancellationToken)
{
    // 1. 移除静态 CSS 导入（将在打包时处理）
    var strippedJavaScript = StripBuildCssImports(javaScript);

    // 2. 转换默认导入：import styles from './foo.module.css'
    var withDefaultCssModules = await RewriteBuildCssModuleImportsAsync(
        documentPath,
        strippedJavaScript,
        StaticCssModuleDefaultImportPattern,
        cancellationToken);

    // 3. 转换命名空间导入：import * as styles from './foo.module.css'
    var withNamespaceCssModules = await RewriteBuildCssModuleImportsAsync(
        documentPath,
        withDefaultCssModules,
        StaticCssModuleNamespaceImportPattern,
        cancellationToken);

    // 4. 转换命名默认导入：import { default as styles } from './foo.module.css'
    return await RewriteBuildCssModuleImportsAsync(
        documentPath,
        withNamespaceCssModules,
        StaticCssModuleNamedDefaultImportPattern,
        cancellationToken);
}
```

**导入转换示例**：
```javascript
// 转换前
import styles from './Button.module.css';

// 转换后
const styles = { "__button__abc123": "_button_abc123", "__disabled__def456": "_disabled_def456" };
```

### Source Map 链接

**ChainJazorSourceMap**（第 822-868 行）：
```csharp
private static string? ChainJazorSourceMap(
    string? javaScriptSourceMap,
    JazorVueCompilationResult compilation)
{
    if (string.IsNullOrWhiteSpace(javaScriptSourceMap))
    {
        return javaScriptSourceMap;
    }

    try
    {
        var generatedVueFileName = Path.GetFileName(compilation.Document.FilePath);
        var generatedVueSourceMap = compilation.GeneratedVueSourceMap;
        if (string.IsNullOrWhiteSpace(generatedVueSourceMap))
        {
            return javaScriptSourceMap;
        }

        // 链接 Source Map：JavaScript -> Vue SFC -> Jazor
        var chainedMap = new SourceMapChainBuilder().Chain(
            javaScriptSourceMap,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [generatedVueFileName] = generatedVueSourceMap
            });

        return new SourceMapWriter().Write(chainedMap);
    }
    catch (System.Text.Json.JsonException)
    {
        return javaScriptSourceMap;
    }
    // ... 其他异常处理
}
```

**Source Map 行号偏移**（第 786-820 行）：
```csharp
private static string? OffsetSourceMapGeneratedLines(string? sourceMap, int generatedLineOffset)
{
    if (string.IsNullOrWhiteSpace(sourceMap) || generatedLineOffset <= 0)
    {
        return sourceMap;
    }

    try
    {
        if (JsonNode.Parse(sourceMap) is not JsonObject sourceMapObject)
        {
            return sourceMap;
        }

        var mappings = sourceMapObject["mappings"]?.GetValue<string>() ?? string.Empty;
        // 在 mappings 前添加分号（行分隔符），偏移行号
        sourceMapObject["mappings"] = string.Concat(new string(';', generatedLineOffset), mappings);
        return sourceMapObject.ToJsonString();
    }
    catch (System.Text.Json.JsonException)
    {
        return sourceMap;
    }
    // ... 其他异常处理
}
```

### 开发模式样式注入

**CreateServedModule**（第 448-476 行）：
```csharp
private ServedModuleContent CreateServedModule(
    string documentPath,
    string javaScript,
    string? styleContent)
{
    if (string.IsNullOrWhiteSpace(styleContent))
    {
        return new ServedModuleContent(javaScript, JavaScriptLineOffset: 0);
    }

    var styleTargetId = GetStyleTargetId(documentPath);
    var prefix = $$"""
        const __jazorStyleId = {{System.Text.Json.JsonSerializer.Serialize(styleTargetId)}};
        const __jazorStyle = {{System.Text.Json.JsonSerializer.Serialize(styleContent)}};
        if (typeof document !== "undefined" && __jazorStyle) {
          let style = document.querySelector(`style[data-jolt="${__jazorStyleId}"]`);
          if (!style) {
            style = document.createElement("style");
            style.setAttribute("data-jolt", __jazorStyleId);
            document.head.appendChild(style);
          }
          style.textContent = __jazorStyle;
        }
        """;

    return new ServedModuleContent(
        string.Concat(prefix, "\n", javaScript),
        CountLines(prefix));
}
```

**生成的 JavaScript 示例**：
```javascript
const __jazorStyleId = "/src/App.vue";
const __jazorStyle = ".button { color: red; }";
if (typeof document !== "undefined" && __jazorStyle) {
  let style = document.querySelector(`style[data-jolt="${__jazorStyleId}"]`);
  if (!style) {
    style = document.createElement("style");
    style.setAttribute("data-jolt", __jazorStyleId);
    document.head.appendChild(style);
  }
  style.textContent = __jazorStyle;
}

// 原始模块代码
export default { name: "App" };
```

## 线程安全模型

### Lock 保护

**CompileAsync**（第 74-91 行）：
```csharp
public async ValueTask<CompilationResult> CompileAsync(
    string absolutePath,
    CancellationToken cancellationToken)
{
    var text = await File.ReadAllTextAsync(absolutePath, cancellationToken);
    var contentHash = ComputeCacheHash(text, companionDocuments: null);

    // 检查缓存
    if (TryGetCachedResultCore(absolutePath, contentHash, out var cached))
    {
        return cached;
    }

    // 执行编译
    var result = await CompileCoreAsync(absolutePath, text, companionDocuments: null, cancellationToken);

    // 更新缓存和依赖图
    PublishCompilationResult(absolutePath, contentHash, result);
    return result;
}
```

**PublishCompilationResult**（第 553-566 行）：
```csharp
private void PublishCompilationResult(string absolutePath, string contentHash, CompilationResult result)
{
    lock (_stateGate)
    {
        // 同步 SourceMap 注册
        SynchronizeSourceMapRegistration(absolutePath, result);

        // 更新依赖图
        _dependencyGraph?.Record(absolutePath, result.Dependencies);

        // 更新缓存（可能触发 LRU 淘汰）
        var evictedPaths = _cache.Set(absolutePath, contentHash, result);
        foreach (var evictedPath in evictedPaths)
        {
            _dependencyGraph?.Remove(evictedPath);
            UnregisterSourceMap(evictedPath);
        }
    }
}
```

## 错误处理

### 前端编译器不可用

**CreateFrontendUnavailableResult**（第 434-446 行）：
```csharp
private static CompilationResult CreateFrontendUnavailableResult(
    string message,
    IReadOnlyList<string>? diagnostics = null)
{
    return new CompilationResult
    {
        ContentType = "text/javascript",
        Content = $$"""
            throw new Error({{System.Text.Json.JsonSerializer.Serialize(message)}});
            """,
        Diagnostics = diagnostics ?? [message],
        IsError = true,
        ErrorMessage = message
    };
}
```

### Source Map 解析容错

**所有 SourceMap 操作都使用 try-catch 包装**，失败时返回原始内容：
```csharp
try
{
    if (JsonNode.Parse(sourceMap) is not JsonObject sourceMapObject)
    {
        return sourceMap;
    }

    // ... SourceMap 操作
    return modifiedSourceMap;
}
catch (System.Text.Json.JsonException)
{
    return sourceMap;
}
catch (FormatException)
{
    return sourceMap;
}
// ... 其他异常类型
```

## 配置选项

### 构建模式 vs 开发模式

**构建模式特点**：
- CSS Modules 输出 CSS 文件 + 映射对象
- JavaScript 导入转换为内联映射对象
- 不注入样式代码
- 不附加内联 Source Map

**开发模式特点**：
- CSS Modules 输出 JavaScript 模块
- 自动注入样式到 DOM
- 内联 Source Map（data URI）
- 支持 HMR

**PrepareJavaScriptForCurrentModeAsync**（第 478-493 行）：
```csharp
private async ValueTask<PreparedJavaScriptContent> PrepareJavaScriptForCurrentModeAsync(
    string documentPath,
    string javaScript,
    string? styleContent,
    CancellationToken cancellationToken)
{
    if (_buildMode)
    {
        return new PreparedJavaScriptContent(
            await TransformBuildJavaScriptAsync(documentPath, javaScript, cancellationToken),
            GeneratedLineOffset: 0);
    }

    var servedModule = CreateServedModule(documentPath, javaScript, styleContent);
    return new PreparedJavaScriptContent(servedModule.Content, servedModule.JavaScriptLineOffset);
}
```

## 与其他子系统的交互

### 与 DependencyGraph 的集成

**依赖记录**（第 558 行）：
```csharp
_dependencyGraph?.Record(absolutePath, result.Dependencies);
```

**缓存失效时清理依赖**（第 173-183 行）：
```csharp
public void Invalidate(string absolutePath)
{
    lock (_stateGate)
    {
        if (_cache.Invalidate(absolutePath))
        {
            _dependencyGraph?.Remove(absolutePath);
            UnregisterSourceMap(absolutePath);
        }
    }
}
```

### 与 ISourceMapService 的集成

**SourceMap 注册**（第 223-254 行）：
```csharp
private void SynchronizeSourceMapRegistration(string absolutePath, CompilationResult result)
{
    if (_sourceMapService is null)
    {
        return;
    }

    foreach (var key in GetSourceMapKeys(absolutePath))
    {
        if (string.IsNullOrWhiteSpace(result.SourceMap))
        {
            _sourceMapService.Unregister(key);
        }
        else
        {
            _sourceMapService.Register(key, result.SourceMap);
        }
    }
}

private IReadOnlyList<string> GetSourceMapKeys(string absolutePath)
{
    if (_moduleResolver is not null)
    {
        try
        {
            return [_moduleResolver.GetResolvedUrlForAbsolutePath(absolutePath)];
        }
        catch (InvalidOperationException)
        {
        }
    }

    return [absolutePath];
}
```

### 与 IWorkspaceStore 的集成

**Companion Documents 加载**（`DevHttpServer.CompileResolvedRequestAsync`，第 291-315 行）：
```csharp
private async Task<IReadOnlyList<DocumentSnapshot>> GetTrackedCompanionDocumentsAsync(
    string documentPath,
    CancellationToken cancellationToken)
{
    if (_workspaceStore is null || !documentPath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
    {
        return Array.Empty<DocumentSnapshot>();
    }

    var trackedDocuments = await _workspaceStore.GetDocumentsAsync(
        JoltWorkspaceResolver.GetCoLocatedCodeBehindPaths(documentPath)
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray(),
        cancellationToken);

    return trackedDocuments
        .Where(static document => document.DocumentKind == DocumentKind.CSharp)
        .Select(static document => new DocumentSnapshot(
            Path.GetFullPath(document.DocumentPath),
            document.DocumentKind,
            document.Text,
            document.Version))
        .ToArray();
}
```

## 设计权衡

### 双层哈希缓存

**ContentHash**（文件内容 + 同伴文档）：
```csharp
private static string ComputeCacheHash(
    string text,
    IReadOnlyList<DocumentSnapshot>? companionDocuments)
{
    if (companionDocuments is null || companionDocuments.Count == 0)
    {
        return ComputeContentHash(text);
    }

    var builder = new StringBuilder(text.Length + (companionDocuments.Count * 64));
    builder.Append(text);
    foreach (var companion in companionDocuments
                 .Where(static document => document.DocumentKind == DocumentKind.CSharp)
                 .OrderBy(static document => document.DocumentPath, StringComparer.OrdinalIgnoreCase))
    {
        builder
            .Append("\n// companion:")
            .Append(Path.GetFullPath(companion.DocumentPath))
            .Append('|')
            .Append(companion.Version)
            .Append('\n')
            .Append(companion.Text);
    }

    return ComputeContentHash(builder.ToString());
}
```

**原因**：
- .jazor 文件的编译结果依赖于同伴 C# 文档
- 双层哈希确保 C# 代码变更时正确失效缓存
- 同伴文档按路径排序确保哈希稳定性

### ModuleSignature vs ContentHash

**ModuleSignature**（用于 HMR）：
```csharp
private static string ComputeJazorModuleSignature(
    string javaScript,
    RazorVueManifestEntry? hotReloadManifestEntry)
{
    if (hotReloadManifestEntry is null)
    {
        return ComputeContentHash(javaScript);
    }

    return ComputeContentHash(string.Create(
        javaScript.Length
        + hotReloadManifestEntry.DescriptorHash.Length
        + hotReloadManifestEntry.TemplateHash.Length
        + hotReloadManifestEntry.LogicHash.Length
        + 3,
        (javaScript, hotReloadManifestEntry),
        static (buffer, state) =>
        {
            var offset = 0;
            state.javaScript.AsSpan().CopyTo(buffer[offset..]);
            offset += state.javaScript.Length;
            buffer[offset++] = '\n';
            state.hotReloadManifestEntry.DescriptorHash.AsSpan().CopyTo(buffer[offset..]);
            offset += state.hotReloadManifestEntry.DescriptorHash.Length;
            buffer[offset++] = '\n';
            state.hotReloadManifestEntry.TemplateHash.AsSpan().CopyTo(buffer[offset..]);
            offset += state.hotReloadManifestEntry.TemplateHash.Length;
            buffer[offset++] = '\n';
            state.hotReloadManifestEntry.LogicHash.AsSpan().CopyTo(buffer[offset..]);
        }));
}
```

**原因**：
- HMR 需要检测组件结构变更（模板、逻辑、描述符）
- 单纯的 JavaScript 内容哈希无法捕获模板变更
- 多层哈希组合确保细粒度变更检测

### 内联 Source Map vs 外部文件

**开发模式使用内联 Source Map**（`AttachInlineSourceMap`，第 870-880 行）：
```csharp
private static string AttachInlineSourceMap(string content, string? sourceMap)
{
    if (string.IsNullOrWhiteSpace(sourceMap))
    {
        return content;
    }

    var normalizedContent = TrailingSourceMapCommentPattern.Replace(content, string.Empty).TrimEnd();
    var dataUri = "data:application/json;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceMap));
    return string.Concat(normalizedContent, "\n//# sourceMappingURL=", dataUri);
}
```

**原因**：
- 避免额外的 HTTP 请求
- 确保浏览器总是能加载到最新的 Source Map
- 简化缓存失效逻辑

**构建模式使用外部 Source Map**（通过 `ISourceMapService`）：
- 允许打包工具优化 Source Map
- 支持生产环境 Source Map 部署策略

### CSS Modules 的双模式实现

**开发模式**：JavaScript 模块 + DOM 注入
**构建模式**：CSS 文件 + 映射对象

**原因**：
- 开发模式需要快速迭代和 HMR
- 构建模式需要静态资产提取和优化
- 双模式支持不同的部署场景
