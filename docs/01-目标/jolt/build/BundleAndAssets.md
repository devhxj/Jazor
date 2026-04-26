# Jolt 打包与静态资产 (Bundle and Static Assets)

> Status: 活跃参考
> Positioning: 协调 Deno bundler 执行、处理 .jazor/.vue 导入重写、生成 import map、复制静态资产

## 1. 文档定位

本文档描述 Jolt 构建系统中与 Deno bundler 的交互、静态资产处理、以及相关的辅助服务。

**核心文件**：
- `src/Jolt/Build/DenoBundleRunner.cs` - Deno bundle CLI 执行器
- `src/Jolt/Build/BundlerModuleProxyServer.cs` - Kestrel 代理服务器
- `src/Jolt/Build/DenoBuildImportMapGenerator.cs` - Import map 生成器
- `src/Jolt/Build/StaticAssetHandler.cs` - 静态资产处理器

## 2. 核心类型

### 2.1 DenoBundleRunner

**位置**：`src/Jolt/Build/DenoBundleRunner.cs:11-725`

```csharp
internal sealed class DenoBundleRunner
{
    private readonly BuildContext _context;

    public DenoBundleRunner(BuildContext context);

    public async Task<DenoBundleResult> RunAsync(
        Uri entryUri,
        CancellationToken cancellationToken);
}
```

**输出结果**：
```csharp
internal sealed class DenoBundleResult
{
    public bool Success { get; init; }
    public IReadOnlyList<ChunkInfo> Chunks { get; init; } = [];
    public IReadOnlyList<AssetInfo> CssAssets { get; init; } = [];
    public IReadOnlyList<BuildDiagnostic> Diagnostics { get; init; } = [];
    public long TotalSize { get; init; }
}
```

### 2.2 BundlerModuleProxyServer

**位置**：`src/Jolt/Build/BundlerModuleProxyServer.cs:11-314`

```csharp
internal sealed class BundlerModuleProxyServer : IAsyncDisposable
{
    private readonly Uri _originBaseUri;
    private readonly string _requestPrefix;  // "/__jazor_bundle/<guid>/"
    private readonly HttpClient _httpClient;
    private Uri? _listeningUri;
    private WebApplication? _application;

    public Uri ListeningUri { get; }

    public static async Task<BundlerModuleProxyServer> StartAsync(
        Uri originEntryUri,
        CancellationToken cancellationToken);

    public Uri CreateBundlerEntryUri(Uri originEntryUri);

    public async ValueTask DisposeAsync();
}
```

### 2.3 StaticAssetHandler

**位置**：`src/Jolt/Build/StaticAssetHandler.cs:7-331`

```csharp
internal sealed class StaticAssetHandler
{
    private readonly BuildContext _context;

    public StaticAssetHandler(BuildContext context);

    public async Task<IReadOnlyList<AssetInfo>> CopyPublicAssetsAsync(
        CancellationToken cancellationToken);

    public async Task<IReadOnlyList<AssetInfo>> CopySourceAssetsAsync(
        IReadOnlyList<SourceAssetRequest> sourceAssets,
        CancellationToken cancellationToken);
}
```

## 3. 核心算法

### 3.1 DenoBundleRunner 主流程

**位置**：`src/Jolt/Build/DenoBundleRunner.cs:32-183`

```csharp
public async Task<DenoBundleResult> RunAsync(
    Uri entryUri,
    CancellationToken cancellationToken)
{
    // 1. 解析 Deno 可执行文件路径
    var denoExecutablePath = DenoRuntimeAssetResolver.ResolveBundledExecutablePath();
    if (!File.Exists(denoExecutablePath))
    {
        return Failure($"Bundled Deno runtime was not found at '{denoExecutablePath}'.");
    }

    // 2. 启动 BundlerModuleProxyServer
    await using var bundlerProxy = await BundlerModuleProxyServer.StartAsync(entryUri, cancellationToken);
    var bundlerEntryUri = bundlerProxy.CreateBundlerEntryUri(entryUri);

    // 3. 生成 import map
    var importMapPath = await DenoBuildImportMapGenerator.GenerateAsync(_context.RootDirectory, cancellationToken);

    // 4. 配置 Deno bundle 命令行参数
    var startInfo = new ProcessStartInfo
    {
        FileName = denoExecutablePath,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        WorkingDirectory = _context.RootDirectory
    };

    startInfo.ArgumentList.Add("bundle");
    startInfo.ArgumentList.Add("--platform");
    startInfo.ArgumentList.Add("browser");
    startInfo.ArgumentList.Add("--format");
    startInfo.ArgumentList.Add("esm");
    startInfo.ArgumentList.Add("--conditions");
    startInfo.ArgumentList.Add("production");
    startInfo.ArgumentList.Add("--quiet");
    startInfo.ArgumentList.Add("--no-config");
    startInfo.ArgumentList.Add("--import-map");
    startInfo.ArgumentList.Add(importMapPath);
    startInfo.ArgumentList.Add($"--allow-import={bundlerEntryUri.Host}:{bundlerEntryUri.Port}");

    if (_context.Options.Minify)
    {
        startInfo.ArgumentList.Add("--minify");
    }

    if (_context.Options.CodeSplitting)
    {
        startInfo.ArgumentList.Add("--code-splitting");
    }

    var sourceMapArgument = MapSourceMapOption(_context.Options.SourceMap);
    if (sourceMapArgument is not null)
    {
        startInfo.ArgumentList.Add($"--sourcemap={sourceMapArgument}");
    }

    if (_context.Options.CodeSplitting)
    {
        startInfo.ArgumentList.Add("--outdir");
        startInfo.ArgumentList.Add(_context.AssetsDirectory);
    }
    else
    {
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(provisionalOutputPath);
    }

    startInfo.ArgumentList.Add(bundlerEntryUri.AbsoluteUri);

    // 5. 启动 Deno 进程
    using var process = Process.Start(startInfo);
    if (process is null)
    {
        return Failure("Failed to start the bundled Deno bundler process.");
    }

    // 6. 等待进程退出并读取输出
    string stdout;
    string stderr;
    try
    {
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await ChildProcessUtilities.WaitForExitOrTerminateOnCancellationAsync(process, cancellationToken);
        stdout = await stdoutTask;
        stderr = await stderrTask;
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        await ChildProcessUtilities.TerminateProcessAsync(process);
        throw;
    }

    // 7. 检查退出码
    if (process.ExitCode != 0)
    {
        return Failure($"Bundled Deno bundle failed with exit code {process.ExitCode}: {stderr}");
    }

    // 8. 收集输出文件（带快照比较以处理文件系统延迟）
    IReadOnlyList<ChunkInfo> chunks;
    if (_context.Options.CodeSplitting)
    {
        chunks = await FinalizeCodeSplitBundleOutputAsync(assetsDirectory, entryUri, cancellationToken);
    }
    else
    {
        var chunk = await FinalizeSingleBundleOutputAsync(provisionalOutputPath, cancellationToken);
        chunks = [chunk];
    }

    // 9. 收集 CSS 资产
    var cssAssets = await FinalizeCssAssetsAsync(assetsDirectory, cancellationToken);

    // 10. 计算总大小
    var totalSize = chunks.Sum(static chunk => chunk.Size)
        + chunks.Sum(chunk => GetOptionalFileSize(ToAbsolutePath(chunk.SourceMapPath)))
        + cssAssets.Sum(static asset => asset.Size)
        + cssAssets.Sum(asset => GetOptionalFileSize(ToAbsolutePath(asset.SourceMapPath)));

    return new DenoBundleResult
    {
        Success = true,
        Chunks = chunks,
        CssAssets = cssAssets,
        Diagnostics = diagnostics,
        TotalSize = totalSize
    };
}
```

### 3.2 快照比较文件收集

**位置**：`src/Jolt/Build/DenoBundleRunner.cs:208-271`

```csharp
private static async Task<string[]> CollectStableOutputPathsAsync(
    string assetsDirectory,
    string searchPattern,
    CancellationToken cancellationToken)
{
    const int maxAttempts = 120;           // 最多 120 次尝试
    const int delayMilliseconds = 50;      // 每次尝试间隔 50ms
    const int quiescenceDurationMilliseconds = 100;  // 静止窗口 100ms

    IReadOnlyList<OutputFileSnapshot> previousSnapshot = [];
    string[] bestPaths = [];
    var bestTotalSize = -1L;
    var lastChangeElapsedMilliseconds = -1L;
    var stopwatch = Stopwatch.StartNew();

    for (var attempt = 0; attempt < maxAttempts; attempt++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // 1. 捕获当前快照
        var currentSnapshot = CaptureOutputFileSnapshots(assetsDirectory, searchPattern);
        var hasChanged = !AreOutputFileSnapshotsEqual(previousSnapshot, currentSnapshot);
        if (hasChanged)
        {
            lastChangeElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }

        // 2. 更新最佳路径（更多文件或更大总大小）
        if (currentSnapshot.Count > 0)
        {
            var currentTotalSize = currentSnapshot.Sum(static snapshot => snapshot.Length);
            if (currentSnapshot.Count > bestPaths.Length
                || (currentSnapshot.Count == bestPaths.Length && currentTotalSize > bestTotalSize))
            {
                bestPaths = currentSnapshot.Select(static snapshot => snapshot.FilePath).ToArray();
                bestTotalSize = currentTotalSize;
            }
        }

        // 3. 检查是否稳定（文件未变化且可读）
        if (currentSnapshot.Count > 0
            && !hasChanged
            && AreOutputFilesReadable(currentSnapshot)
            && lastChangeElapsedMilliseconds >= 0
            && (stopwatch.ElapsedMilliseconds - lastChangeElapsedMilliseconds) >= quiescenceDurationMilliseconds)
        {
            return currentSnapshot.Select(static snapshot => snapshot.FilePath).ToArray();
        }

        previousSnapshot = currentSnapshot;
        if (attempt == maxAttempts - 1)
        {
            return bestPaths;  // 最后一次尝试，返回最佳路径
        }

        await Task.Delay(delayMilliseconds, cancellationToken);
    }

    return bestPaths;
}
```

**快照结构**：
```csharp
internal readonly record struct OutputFileSnapshot(
    string FilePath,
    long Length,
    long LastWriteTimeUtcTicks);
```

**为什么需要快照比较**：
- Deno 进程退出后，文件系统可能仍有延迟写入
- 直接枚举可能捕获不完整的文件
- 快照比较确保文件大小和修改时间稳定

**快照相等性检查**（第 307-329 行）：
```csharp
private static bool AreOutputFileSnapshotsEqual(
    IReadOnlyList<OutputFileSnapshot> left,
    IReadOnlyList<OutputFileSnapshot> right)
{
    if (left.Count != right.Count)
    {
        return false;
    }

    for (var index = 0; index < left.Count; index++)
    {
        var leftSnapshot = left[index];
        var rightSnapshot = right[index];
        if (!string.Equals(leftSnapshot.FilePath, rightSnapshot.FilePath, PathComparison)
            || leftSnapshot.Length != rightSnapshot.Length
            || leftSnapshot.LastWriteTimeUtcTicks != rightSnapshot.LastWriteTimeUtcTicks)
        {
            return false;
        }
    }

    return true;
}
```

### 3.3 Bundle 文件哈希与重写

**位置**：`src/Jolt/Build/DenoBundleRunner.cs:360-434`

```csharp
private async Task<IReadOnlyList<ChunkInfo>> FinalizeBundleOutputsAsync(
    IReadOnlyList<string> provisionalOutputPaths,
    string provisionalEntryOutputPath,
    CancellationToken cancellationToken)
{
    var bundleFiles = new List<ProvisionalBundleFile>(provisionalOutputPaths.Count);

    // 1. 读取所有 bundle 文件
    foreach (var provisionalOutputPath in provisionalOutputPaths.OrderBy(static path => path, PathComparer))
    {
        var jsContent = await File.ReadAllTextAsync(provisionalOutputPath, cancellationToken);
        var hashedFileName = CreateHashedFileName(provisionalOutputPath, jsContent);
        var hashedOutputPath = Path.Combine(GetContainingDirectoryPath(provisionalOutputPath), hashedFileName);
        var sourceMapPath = provisionalOutputPath + ".map";
        var hashedSourceMapPath = File.Exists(sourceMapPath)
            ? hashedOutputPath + ".map"
            : null;

        bundleFiles.Add(new ProvisionalBundleFile
        {
            OriginalPath = Path.GetFullPath(provisionalOutputPath),
            HashedPath = Path.GetFullPath(hashedOutputPath),
            HashedFileName = hashedFileName,
            OriginalContent = jsContent,
            OriginalSourceMapPath = File.Exists(sourceMapPath)
                ? Path.GetFullPath(sourceMapPath)
                : null,
            HashedSourceMapPath = hashedSourceMapPath is null
                ? null
                : Path.GetFullPath(hashedSourceMapPath),
            IsEntry = string.Equals(
                Path.GetFullPath(provisionalOutputPath),
                Path.GetFullPath(provisionalEntryOutputPath),
                PathComparison)
        });
    }

    // 2. 构建路径映射（原始路径 → 哈希路径）
    var pathMap = bundleFiles.ToDictionary(
        static file => file.OriginalPath,
        static file => file.HashedPath,
        PathComparer);

    // 3. 重写每个文件中的导入路径
    foreach (var bundleFile in bundleFiles)
    {
        bundleFile.RewrittenContent = RewriteBundleContent(bundleFile, pathMap, out var imports);
        bundleFile.Imports = imports;
    }

    // 4. 写入哈希文件
    foreach (var bundleFile in bundleFiles)
    {
        await WriteFinalChunkAsync(bundleFile, cancellationToken);
    }

    // 5. 删除原始文件
    foreach (var bundleFile in bundleFiles)
    {
        DeleteIfExists(bundleFile.OriginalPath);
        DeleteIfExists(bundleFile.OriginalSourceMapPath);
    }

    // 6. 返回 ChunkInfo 列表
    return bundleFiles
        .OrderByDescending(static file => file.IsEntry)
        .ThenBy(static file => file.HashedFileName, StringComparer.Ordinal)
        .Select(file => new ChunkInfo
        {
            FileName = file.HashedFileName,
            FilePath = Path.GetRelativePath(_context.RootDirectory, file.HashedPath).Replace('\\', '/'),
            Size = new FileInfo(file.HashedPath).Length,
            IsEntry = file.IsEntry,
            IsDynamic = !file.IsEntry && bundleFiles.Count > 1,
            Imports = file.Imports,
            Css = [],
            SourceMapPath = file.HashedSourceMapPath is null
                ? null
                : Path.GetRelativePath(_context.RootDirectory, file.HashedSourceMapPath).Replace('\\', '/')
        })
        .ToArray();
}
```

**Bundle 内容重写**（第 436-483 行）：
```csharp
private string RewriteBundleContent(
    ProvisionalBundleFile bundleFile,
    IReadOnlyDictionary<string, string> pathMap,
    out IReadOnlyList<string> imports)
{
    var importedChunks = new HashSet<string>(StringComparer.Ordinal);
    var currentDirectory = GetContainingDirectoryPath(bundleFile.OriginalPath);

    var rewrittenContent = JavaScriptModuleSpecifierScanner.RewriteSpecifiers(
        bundleFile.OriginalContent,
        specifier =>
        {
            var (originalSpecifier, suffix) = JavaScriptModuleSpecifierScanner.SplitPathAndSuffix(specifier.Value);
            if (!IsRelativeJavaScriptSpecifier(originalSpecifier))
            {
                return null;  // 跳过非相对路径导入
            }

            // 解析导入的绝对路径
            var resolvedImportPath = Path.GetFullPath(Path.Combine(
                currentDirectory,
                originalSpecifier.Replace('/', Path.DirectorySeparatorChar)));

            // 查找哈希路径
            if (!pathMap.TryGetValue(resolvedImportPath, out var rewrittenImportPath))
            {
                return null;  // 未找到，保留原样
            }

            // 计算相对路径
            var rewrittenSpecifier = Path.GetRelativePath(currentDirectory, rewrittenImportPath).Replace('\\', '/');
            if (!rewrittenSpecifier.StartsWith("./", StringComparison.Ordinal)
                && !rewrittenSpecifier.StartsWith("../", StringComparison.Ordinal))
            {
                rewrittenSpecifier = "./" + rewrittenSpecifier;
            }

            importedChunks.Add(Path.GetRelativePath(_context.RootDirectory, rewrittenImportPath).Replace('\\', '/'));
            return rewrittenSpecifier + suffix;
        });

    // 重写 source map 引用
    if (bundleFile.OriginalSourceMapPath is not null && bundleFile.HashedSourceMapPath is not null)
    {
        rewrittenContent = rewrittenContent.Replace(
            $"//# sourceMappingURL={Path.GetFileName(bundleFile.OriginalSourceMapPath)}",
            $"//# sourceMappingURL={Path.GetFileName(bundleFile.HashedSourceMapPath)}",
            StringComparison.Ordinal);
    }

    imports = importedChunks.OrderBy(static path => path, StringComparer.Ordinal).ToArray();
    return rewrittenContent;
}
```

**重写示例**：
```javascript
// 原始内容（Deno 输出）
import { foo } from './vendor.js';
//# sourceMappingURL=main.js.map

// 重写后
import { foo } from './vendor-abc123.js';
//# sourceMappingURL=main-def456.js.map
```

### 3.4 BundlerModuleProxyServer 工作流程

**位置**：`src/Jolt/Build/BundlerModuleProxyServer.cs:46-100`

```csharp
public static async Task<BundlerModuleProxyServer> StartAsync(
    Uri originEntryUri,
    CancellationToken cancellationToken)
{
    var server = new BundlerModuleProxyServer(originEntryUri);
    await server.StartCoreAsync(cancellationToken);
    return server;
}

private async Task StartCoreAsync(CancellationToken cancellationToken)
{
    if (_application is not null)
    {
        return;
    }

    // 1. 获取可用端口
    var port = GetAvailablePort();

    // 2. 创建 Kestrel 应用
    var builder = WebApplication.CreateSlimBuilder();
    builder.WebHost.UseUrls($"http://127.0.0.1:{port}");

    var application = builder.Build();

    // 3. 映射所有请求到代理逻辑
    application.Map(
        "/{**requestPath}",
        async context =>
        {
            if (!HttpMethods.IsGet(context.Request.Method)
                && !HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            await ProxyAsync(context);
        });

    // 4. 启动应用
    await application.StartAsync(cancellationToken);
    _application = application;
    _listeningUri = ResolveListeningUri(application)
        ?? throw new InvalidOperationException("Failed to resolve bundler proxy listening URI.");
}
```

**代理逻辑**（第 102-145 行）：
```csharp
private async Task ProxyAsync(HttpContext context)
{
    var requestPath = context.Request.Path.HasValue
        ? context.Request.Path.Value!
        : "/";
    var originRequestPath = MapBundlerRequestPathToOriginPath(requestPath);
    if (context.Request.QueryString.HasValue)
    {
        originRequestPath += context.Request.QueryString.Value;
    }

    // 1. 构造原始服务器 URI
    var originUri = new Uri(_originBaseUri, originRequestPath);

    // 2. 发送 HTTP 请求到原始服务器
    using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), originUri);
    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

    // 3. 复制响应状态码
    context.Response.StatusCode = (int)response.StatusCode;
    if (!response.IsSuccessStatusCode)
    {
        await WriteSanitizedErrorResponseAsync(context, response.StatusCode, context.RequestAborted);
        return;
    }

    // 4. 复制 Content-Type
    if (response.Content.Headers.ContentType is MediaTypeHeaderValue contentType)
    {
        context.Response.ContentType = contentType.ToString();
    }

    // 5. 处理 JavaScript 内容（重写导入路径）
    var mediaType = response.Content.Headers.ContentType?.MediaType;
    if (IsJavaScriptMediaType(mediaType))
    {
        var content = await response.Content.ReadAsStringAsync(context.RequestAborted);
        var rewrittenContent = RewriteJavaScriptSpecifiers(content);
        await context.Response.WriteAsync(rewrittenContent, Encoding.UTF8, context.RequestAborted);
        return;
    }

    // 6. 复制二进制内容
    var bytes = await response.Content.ReadAsByteArrayAsync(context.RequestAborted);
    await context.Response.Body.WriteAsync(bytes, context.RequestAborted);
}
```

**导入路径重写**（第 168-177 行）：
```csharp
private string RewriteJavaScriptSpecifiers(string content)
    => JavaScriptModuleSpecifierScanner.RewriteSpecifiers(
        content,
        specifier =>
        {
            var rewrittenSpecifier = RewriteSpecifierForBundler(specifier.Value, _requestPrefix);
            return string.Equals(rewrittenSpecifier, specifier.Value, StringComparison.Ordinal)
                ? null
                : rewrittenSpecifier;
        });
```

**重写规则**（第 179-224 行）：
```csharp
private static string RewriteSpecifierForBundler(string specifier, string requestPrefix)
{
    if (string.IsNullOrWhiteSpace(specifier))
    {
        return specifier;
    }

    // 1. 处理绝对 URI
    if (Uri.TryCreate(specifier, UriKind.Absolute, out var absoluteUri))
    {
        if (!IsAuthoredModulePath(absoluteUri.AbsolutePath))
        {
            return specifier;  // 外部 URI，不重写
        }

        // .jazor/.vue → ..js
        var builder = new UriBuilder(absoluteUri)
        {
            Path = absoluteUri.AbsolutePath + ".js"
        };
        return builder.Uri.AbsoluteUri;
    }

    // 2. 处理相对路径
    var suffixIndex = specifier.IndexOfAny(['?', '#']);
    var path = suffixIndex >= 0
        ? specifier[..suffixIndex]
        : specifier;

    if (path.StartsWith("/", StringComparison.Ordinal)
        && !path.StartsWith(requestPrefix, StringComparison.OrdinalIgnoreCase))
    {
        path = requestPrefix + path[1..];  // /src/main.ts → /__jazor_bundle/<guid>/src/main.ts
    }

    // 3. .jazor/.vue → ..js
    var rewrittenPath = IsAuthoredModulePath(path)
        ? path + ".js"
        : path;

    if (string.Equals(rewrittenPath, path, StringComparison.Ordinal)
        && suffixIndex < 0)
    {
        return specifier;
    }

    return suffixIndex >= 0
        ? string.Concat(rewrittenPath, specifier.AsSpan(suffixIndex))
        : rewrittenPath;
}
```

**重写示例**：
```javascript
// 原始代码（DevServer 返回）
import { foo } from './bar.jazor';
import { baz } from '/src/qux.vue';

// 重写后（Proxy 返回给 Deno）
import { foo } from './bar.jazor.js';
import { baz } from '/__jazor_bundle/<guid>/src/qux.vue.js';
```

**为什么需要代理服务器**：
- Deno bundler 不理解 `.jazor` 和 `.vue` 扩展名
- DevServer 能正确处理这些扩展名
- 代理服务器重写导入路径，让 Deno 能正确解析
- 回程时将 `.jazor.js` 和 `.vue.js` 还原为原始路径

### 3.5 Import Map 生成

**位置**：`src/Jolt/Build/DenoBuildImportMapGenerator.cs:5-37`

```csharp
public static async Task<string> GenerateAsync(
    string rootDirectory,
    CancellationToken cancellationToken)
{
    // 1. 初始化默认导入
    var imports = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["vue"] = "npm:vue@3",
        ["vue/"] = "npm:vue@3/"
    };

    // 2. 读取 package.json
    var packageJsonPath = Path.Combine(rootDirectory, "package.json");
    if (File.Exists(packageJsonPath))
    {
        using var packageJson = JsonDocument.Parse(await File.ReadAllTextAsync(packageJsonPath, cancellationToken));
        AddPackageImports(packageJson.RootElement, "dependencies", imports);
        AddPackageImports(packageJson.RootElement, "devDependencies", imports);
    }

    // 3. 写入 import map
    var jazorDirectory = Path.Combine(rootDirectory, ".jazor");
    Directory.CreateDirectory(jazorDirectory);

    var importMapPath = Path.Combine(jazorDirectory, "build.importmap.json");
    await File.WriteAllTextAsync(
        importMapPath,
        JsonSerializer.Serialize(new { imports }, new JsonSerializerOptions { WriteIndented = true }),
        cancellationToken);
    return importMapPath;
}
```

**package.json 依赖添加**（第 39-61 行）：
```csharp
private static void AddPackageImports(
    JsonElement root,
    string propertyName,
    IDictionary<string, string> imports)
{
    if (!root.TryGetProperty(propertyName, out var dependencies)
        || dependencies.ValueKind != JsonValueKind.Object)
    {
        return;
    }

    foreach (var dependency in dependencies.EnumerateObject())
    {
        var version = dependency.Value.GetString();
        if (!IsSupportedNpmVersion(version))
        {
            continue;
        }

        imports[dependency.Name] = $"npm:{dependency.Name}@{version}";
        imports[dependency.Name + "/"] = $"npm:{dependency.Name}@{version}/";
    }
}

private static bool IsSupportedNpmVersion(string? version)
    => !string.IsNullOrWhiteSpace(version)
        && !version.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
        && !version.StartsWith("workspace:", StringComparison.OrdinalIgnoreCase)
        && !version.StartsWith("link:", StringComparison.OrdinalIgnoreCase);
```

**输出示例**（`.jazor/build.importmap.json`）：
```json
{
  "imports": {
    "vue": "npm:vue@3",
    "vue/": "npm:vue@3/",
    "pinia": "npm:pinia@2.1.7",
    "pinia/": "npm:pinia@2.1.7/",
    "axios": "npm:axios@1.6.2",
    "axios/": "npm:axios@1.6.2/"
  }
}
```

### 3.6 静态资产复制

**位置**：`src/Jolt/Build/StaticAssetHandler.cs:31-116`

```csharp
public async Task<IReadOnlyList<AssetInfo>> CopyPublicAssetsAsync(
    CancellationToken ct)
{
    var publicDir = Path.Combine(_context.RootDirectory, "public");

    if (!Directory.Exists(publicDir))
    {
        _context.Diagnostics.Add(new BuildDiagnostic
        {
            Severity = DiagnosticSeverity.Info,
            Message = "No public directory found, skipping static asset copying"
        });
        return [];
    }

    var assets = new List<AssetInfo>();
    var distDir = _context.OutDirectory;

    // 1. 异步枚举所有文件
    await foreach (var assetPath in EnumerateFilesAsync(publicDir, ct))
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var relativePath = Path.GetRelativePath(publicDir, assetPath);
            var fileName = Path.GetFileName(assetPath);
            var extension = Path.GetExtension(assetPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(assetPath);

            // 2. 确定是否哈希
            var fileInfo = new FileInfo(assetPath);
            var shouldHash = ShouldHash(assetPath) && fileInfo.Length < HashSizeThreshold;

            // 3. 计算输出文件名
            var destFileName = fileName;
            if (shouldHash)
            {
                var hash = await ComputeFileHashAsync(assetPath, _context.Options.AssetHashLength, ct);
                destFileName = $"{fileNameWithoutExt}-{hash}{extension}";
            }

            // 4. 复制文件
            var destPath = Path.Combine(distDir, relativePath);
            var destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (shouldHash)
            {
                destPath = Path.Combine(destDir!, destFileName);
            }

            await CopyFileAsync(assetPath, destPath, ct);

            // 5. 记录资产信息
            assets.Add(new AssetInfo
            {
                FileName = destFileName,
                FilePath = Path.GetRelativePath(_context.RootDirectory, destPath).Replace('\\', '/'),
                Size = new FileInfo(destPath).Length,
                OriginalPath = NormalizePublicAssetPath(relativePath)
            });
        }
        catch (DirectoryNotFoundException)
        {
            AddSkippedAssetDiagnostic(assetPath);
        }
        catch (FileNotFoundException)
        {
            AddSkippedAssetDiagnostic(assetPath);
        }
        catch (IOException)
        {
            AddSkippedAssetDiagnostic(assetPath);
        }
        catch (UnauthorizedAccessException)
        {
            AddSkippedAssetDiagnostic(assetPath);
        }
    }

    return assets;
}
```

**哈希条件**：
```csharp
private static readonly HashSet<string> HashExtensions = new(StringComparer.OrdinalIgnoreCase)
{
    ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp",
    ".woff", ".woff2", ".ttf", ".eot",
    ".mp4", ".webm", ".ogg", ".mp3",
    ".pdf"
};

private const int HashSizeThreshold = 4 * 1024;  // 4KB
```

**哈希算法**（第 208-215 行）：
```csharp
private static async Task<string> ComputeFileHashAsync(
    string filePath,
    int hashLength,
    CancellationToken ct)
{
    var bytes = await File.ReadAllBytesAsync(filePath, ct);

    using var sha256 = SHA256.Create();
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToHexString(hash)[..hashLength].ToLowerInvariant();
}
```

**文件枚举**（第 232-259 行）：
```csharp
private static async IAsyncEnumerable<string> EnumerateFilesAsync(
    string directory,
    [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
{
    var stack = new Stack<string>();
    stack.Push(directory);

    while (stack.Count > 0)
    {
        ct.ThrowIfCancellationRequested();

        var currentDir = stack.Pop();

        // 枚举文件
        foreach (var file in SafeEnumerate(() => Directory.EnumerateFiles(currentDir)))
        {
            yield return file;
        }

        // 添加子目录到栈
        foreach (var subDir in SafeEnumerate(() => Directory.EnumerateDirectories(currentDir)))
        {
            stack.Push(subDir);
        }

        await Task.Yield();
    }
}
```

**安全枚举**（第 270-320 行）：
```csharp
private static IEnumerable<string> SafeEnumerate(Func<IEnumerable<string>> factory)
{
    IEnumerator<string>? enumerator = null;
    try
    {
        enumerator = factory().GetEnumerator();
    }
    catch (DirectoryNotFoundException)
    {
        yield break;
    }
    catch (IOException)
    {
        yield break;
    }
    catch (UnauthorizedAccessException)
    {
        yield break;
    }

    using (enumerator)
    {
        while (true)
        {
            string current;
            try
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }

                current = enumerator.Current;
            }
            catch (DirectoryNotFoundException)
            {
                yield break;
            }
            catch (IOException)
            {
                yield break;
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }

            yield return current;
        }
    }
}
```

**为什么需要安全枚举**：
- 文件系统可能在枚举过程中发生变化（外部进程删除文件）
- `Directory.EnumerateFiles/Directories` 可能抛出异常
- 安全枚举捕获异常并继续枚举其他文件

## 4. 线程安全模型

### 4.1 DenoBundleRunner

- **进程管理**：每次运行创建新进程，不共享
- **文件操作**：顺序读写，无并发访问
- **HttpClient**（在 BundlerModuleProxyServer 中）：每个代理服务器独立实例

### 4.2 BundlerModuleProxyServer

- **Kestrel**：处理并发 HTTP 请求
- **HttpClient**：`SocketsHttpHandler` 支持连接池，线程安全
- **请求映射**：无共享状态，每个请求独立处理

### 4.3 StaticAssetHandler

- **文件枚举**：顺序枚举，无并发
- **哈希计算**：每个文件独立计算，无共享状态

## 5. 错误处理

### 5.1 Deno 进程失败

**场景**：
- Deno 可执行文件不存在
- 进程启动失败
- 进程退出码非零
- 输出文件未生成

**处理**：
- 返回 `Success = false` 的 `DenoBundleResult`
- 包含错误诊断信息

### 5.2 代理服务器错误

**场景**：
- 原始服务器返回错误状态码
- 网络超时
- 请求取消

**处理**：
- 返回相应的 HTTP 状态码
- 写入错误消息到响应体
- 不抛出异常

### 5.3 静态资产复制错误

**场景**：
- 文件在枚举过程中被删除
- 无权限访问文件
- IO 错误

**处理**：
- 添加 Warning 诊断
- 跳过当前文件
- 继续处理其他文件

## 6. 配置选项

### 6.1 影响打包的 BuildOptions

| 选项 | 影响 |
|------|------|
| `Minify` | 传递 `--minify` 参数给 Deno |
| `CodeSplitting` | 传递 `--code-splitting` 或 `--output` 参数 |
| `SourceMap` | 传递 `--sourcemap=inline/linked` 参数 |
| `AssetHashLength` | 控制 bundle 文件名哈希长度 |

### 6.2 影响静态资产的 BuildOptions

| 选项 | 影响 |
|------|------|
| `AssetsDir` | 控制资产输出目录名称 |
| `AssetHashLength` | 控制静态资产哈希长度 |

## 7. 与其他子系统的交互

### 7.1 与 DevServer 的交互

**BundlerModuleProxyServer**：
- 代理到 DevHttpServer（在 BuildAsync 中启动）
- 重写 DevServer 返回的模块路径
- 让 Deno bundler 能正确处理 .jazor/.vue 文件

### 7.2 与编译器的交互

**间接交互**：
- 通过 DevServer 访问编译结果
- 编译器生成的 JS/CSS 被 Deno bundle 处理

### 7.3 与 Deno 子系统的交互

**DenoBundleRunner**：
- 启动 Deno CLI 进程
- 传递 import map 配置
- 解析 Deno 输出的 chunk 和 CSS

## 8. 设计权衡

### 8.1 为什么要用代理服务器而不是修改源文件？

**权衡**：
- **修改源文件**：实现简单，但污染源代码
- **代理服务器**：实现复杂，但不修改源代码

**设计决策**：使用代理服务器的原因：
1. **源代码清洁**：不修改 `.jazor` 和 `.vue` 文件
2. **可逆性**：构建过程不影响开发体验
3. **安全性**：避免意外的源文件修改

### 8.2 为什么要用快照比较而不是简单等待？

**权衡**：
- **固定延迟**：实现简单，但可能等待不足或过度
- **快照比较**：实现复杂，但能准确检测稳定状态

**设计决策**：使用快照比较的原因：
1. **可靠性**：确保文件系统写入完成
2. **性能**：避免不必要的等待
3. **自适应性**：适应不同文件系统性能

### 8.3 为什么要哈希静态资产？

**权衡**：
- **不哈希**：实现简单，但缓存不友好
- **哈希**：实现复杂，但缓存友好

**设计决策**：哈希静态资产的原因：
1. **缓存破坏**：内容变化时文件名变化
2. **长期缓存**：可以设置长期缓存策略
3. **CDN 友好**：CDN 能正确缓存哈希文件

### 8.4 为什么要限制哈希文件大小？

**权衡**：
- **全部哈希**：缓存最优，但大文件哈希慢
- **限制大小哈希**：性能更好，但大文件不哈希

**设计决策**：只哈希 < 4KB 文件的原因：
1. **性能**：大文件哈希计算时间长
2. **收益递减**：大文件（如视频）通常不需要长期缓存
3. **实用主义**：大多数小型资产（图标、字体）能被哈希

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
