# Jolt 打包与静态资产 (Bundle and Static Assets)

Jolt 构建系统中与 Deno bundler 的交互、静态资产处理、以及相关的辅助服务。

核心文件：
- `src/Jolt/Build/DenoBundleRunner.cs` - Deno bundle CLI 执行器
- `src/Jolt/Build/BundlerModuleProxyServer.cs` - Kestrel 代理服务器
- `src/Jolt/Build/DenoBuildImportMapGenerator.cs` - Import map 生成器
- `src/Jolt/Build/StaticAssetHandler.cs` - 静态资产处理器

## 核心类型

### DenoBundleRunner

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

输出结果：
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

### BundlerModuleProxyServer

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

### StaticAssetHandler

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

## 核心算法

### DenoBundleRunner 主流程

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

### 快照比较文件收集

**位置**：`src/Jolt/Build/DenoBundleRunner.cs:208-271`

Deno 进程退出后文件系统可能仍有延迟写入，直接枚举可能捕获不完整的文件。快照比较确保文件大小和修改时间稳定：

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

        var currentSnapshot = CaptureOutputFileSnapshots(assetsDirectory, searchPattern);
        var hasChanged = !AreOutputFileSnapshotsEqual(previousSnapshot, currentSnapshot);
        if (hasChanged)
        {
            lastChangeElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        }

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
            return bestPaths;
        }

        await Task.Delay(delayMilliseconds, cancellationToken);
    }

    return bestPaths;
}
```

快照结构：
```csharp
internal readonly record struct OutputFileSnapshot(
    string FilePath,
    long Length,
    long LastWriteTimeUtcTicks);
```

### Bundle 文件哈希与重写

**位置**：`src/Jolt/Build/DenoBundleRunner.cs:360-434`

```csharp
private async Task<IReadOnlyList<ChunkInfo>> FinalizeBundleOutputsAsync(
    IReadOnlyList<string> provisionalOutputPaths,
    string provisionalEntryOutputPath,
    CancellationToken cancellationToken)
{
    // 1. 读取所有 bundle 文件，计算哈希文件名
    // 2. 构建路径映射（原始路径 → 哈希路径）
    // 3. 重写每个文件中的导入路径
    // 4. 写入哈希文件
    // 5. 删除原始文件
    // 6. 返回 ChunkInfo 列表
}
```

Bundle 内容重写（第 436-483 行）：

```csharp
private string RewriteBundleContent(
    ProvisionalBundleFile bundleFile,
    IReadOnlyDictionary<string, string> pathMap,
    out IReadOnlyList<string> imports)
```

重写示例：
```javascript
// 原始内容（Deno 输出）
import { foo } from './vendor.js';
//# sourceMappingURL=main.js.map

// 重写后
import { foo } from './vendor-abc123.js';
//# sourceMappingURL=main-def456.js.map
```

### BundlerModuleProxyServer 工作流程

**位置**：`src/Jolt/Build/BundlerModuleProxyServer.cs:46-100`

Kestrel 代理服务器，将 `.jazor`/`.vue` 导入重写为 `.jazor.js`/`.vue.js`，让 Deno bundler 能正确处理。代理到 DevHttpServer，回程时还原路径。

代理逻辑（第 102-145 行）：
1. 构造原始服务器 URI
2. 发送 HTTP 请求到原始服务器
3. 处理 JavaScript 内容：重写导入路径
4. 复制二进制内容

导入路径重写规则：
- 绝对 URI：`.jazor`/`.vue` → 加 `.js` 后缀
- 相对路径：添加 `/__jazor_bundle/<guid>/` 前缀，`.jazor`/`.vue` → 加 `.js` 后缀

```javascript
// 原始代码（DevServer 返回）
import { foo } from './bar.jazor';
import { baz } from '/src/qux.vue';

// 重写后（Proxy 返回给 Deno）
import { foo } from './bar.jazor.js';
import { baz } from '/__jazor_bundle/<guid>/src/qux.vue.js';
```

### Import Map 生成

**位置**：`src/Jolt/Build/DenoBuildImportMapGenerator.cs:5-37`

读取 `package.json` 的 `dependencies` 和 `devDependencies`，生成 Deno import map。过滤 `file:`、`workspace:`、`link:` 版本。

输出示例（`.jazor/build.importmap.json`）：
```json
{
  "imports": {
    "vue": "vue",
    "vue/": "vue/",
    "pinia": "npm:pinia@2.1.7",
    "pinia/": "npm:pinia@2.1.7/",
    "axios": "npm:axios@1.6.2",
    "axios/": "npm:axios@1.6.2/"
  }
}
```

### 静态资产复制

**位置**：`src/Jolt/Build/StaticAssetHandler.cs:31-116`

递归枚举 `public/` 目录，对符合条件的文件计算 SHA-256 内容哈希，复制到 `dist/`。

哈希条件：扩展名在 `HashExtensions`（图片、字体、音视频、PDF）中且文件大小 < 4KB。

哈希算法：SHA-256，取前 N 个字符（N = `AssetHashLength`），文件名格式 `<basename>-<hash><extension>`。

文件枚举使用 `SafeEnumerate` 包裹 `Directory.EnumerateFiles/Directories`，捕获 `DirectoryNotFoundException`、`IOException`、`UnauthorizedAccessException`，失败时跳过继续。

## 线程安全模型

- **DenoBundleRunner**：每次运行创建新进程，不共享；文件操作顺序读写；HttpClient 在代理服务器中独立实例
- **BundlerModuleProxyServer**：Kestrel 处理并发 HTTP 请求；`SocketsHttpHandler` 支持连接池，线程安全；每个请求独立处理
- **StaticAssetHandler**：文件枚举顺序执行，哈希计算每文件独立，无共享状态

## 错误处理

**Deno 进程失败**：返回 `Success = false` 的 `DenoBundleResult`，包含错误诊断信息。

**代理服务器错误**：返回相应的 HTTP 状态码，写入错误消息到响应体，不抛出异常。

**静态资产复制错误**：添加 Warning 诊断，跳过当前文件，继续处理其他文件。

## 配置选项

影响打包的 BuildOptions：

| 选项 | 影响 |
|------|------|
| `Minify` | 传递 `--minify` 参数给 Deno |
| `CodeSplitting` | 传递 `--code-splitting` 或 `--output` 参数 |
| `SourceMap` | 传递 `--sourcemap=inline/linked` 参数 |
| `AssetHashLength` | 控制 bundle 文件名哈希长度 |

影响静态资产的 BuildOptions：

| 选项 | 影响 |
|------|------|
| `AssetsDir` | 控制资产输出目录名称 |
| `AssetHashLength` | 控制静态资产哈希长度 |

## 与其他子系统的交互

- **DevServer**：BundlerModuleProxyServer 代理到 DevHttpServer，重写模块路径
- **编译器**：间接交互，通过 DevServer 访问编译结果
- **Deno 子系统**：DenoBundleRunner 启动 Deno CLI 进程，传递 import map，解析输出的 chunk 和 CSS

## 设计权衡

### 为什么用代理服务器而不是修改源文件

修改源文件实现简单但污染代码；代理服务器实现复杂但不修改源代码，构建过程不影响开发体验，避免意外的源文件修改。

### 为什么用快照比较而不是简单等待

固定延迟可能等待不足或过度。快照比较确保文件系统写入完成，避免不必要等待，适应不同文件系统性能。

### 为什么哈希静态资产

内容变化时文件名变化（缓存破坏），可以设置长期缓存策略，CDN 能正确缓存哈希文件。只哈希 < 4KB 文件：大文件哈希慢且收益低，大多数小型资产（图标、字体）能被哈希。
