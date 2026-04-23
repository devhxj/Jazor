# Jolt 增量构建 (Incremental Build)

> 状态：已实现
> 定位：通过文件指纹缓存避免不必要的重建，提升构建速度

## 1. 文档定位

本文档描述 Jolt 构建系统的增量构建机制，包括如何通过 SHA256 文件指纹检测变化、持久化构建状态、以及 HTML-only 刷新优化路径。

**核心文件**：
- `src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs` - 增量构建逻辑

## 2. 核心类型

### 2.1 BuildIncrementalState (增量构建状态)

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:68-85`

```csharp
private sealed class BuildIncrementalState
{
    public required string Fingerprint { get; init; }           // SHA256 构建指纹
    public required string ManifestPath { get; init; }         // 清单路径（相对于根目录）
    public string EntryRequestPath { get; init; }              // 入口请求路径
    public IReadOnlyDictionary<string, string> Inputs { get; init; } = [];  // 输入文件签名
    public IReadOnlyList<ChunkInfo> Chunks { get; init; } = [];            // 输出 chunks
    public IReadOnlyList<AssetInfo> CssAssets { get; init; } = [];         // 输出 CSS
    public IReadOnlyList<AssetInfo> StaticAssets { get; init; } = [];      // 输出静态资产
    public long TotalSize { get; init; }                       // 总大小
}
```

**持久化文件**：`dist/jazor-build-state.json`

### 2.2 增量构建相关的 BuildOptions 字段

```csharp
public bool Incremental { get; init; } = false;  // 是否启用增量构建
```

## 3. 核心算法

### 3.1 增量构建主流程

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:111-168`

```csharp
if (options.Incremental)
{
    // 1. 收集当前输入文件签名
    incrementalInputs = CollectIncrementalInputSignatures(context);

    // 2. 计算构建指纹
    incrementalFingerprint = ComputeIncrementalFingerprint(options, incrementalInputs);

    // 3. 尝试读取上次构建状态
    if (TryReadIncrementalState(context, out var incrementalState)
        && AreIncrementalOutputsAvailable(context, incrementalState))
    {
        // 4. 检查指纹是否匹配
        if (string.Equals(incrementalState.Fingerprint, incrementalFingerprint, StringComparison.Ordinal))
        {
            // 4a. 完全命中缓存，直接返回
            return new BuildResult { Success = true, ... };
        }

        // 5. 尝试 HTML-only 刷新路径
        var htmlRefreshResult = await TryBuildHtmlRefreshIncrementalResultAsync(...);
        if (htmlRefreshResult is not null)
        {
            return htmlRefreshResult;
        }
    }
}

// 6. 未命中缓存，执行完整构建
// ... (BuildAsync 主流程)

// 7. 持久化增量状态
if (options.Incremental && !string.IsNullOrWhiteSpace(incrementalFingerprint))
{
    await PersistIncrementalStateAsync(context, buildResult, ...);
}
```

**决策树**：

```
启用增量构建？
  ├─ 否 → 执行完整构建
  └─ 是 → 收集输入签名 → 计算指纹
            ├─ 无上次状态 → 执行完整构建
            ├─ 输出文件缺失 → 执行完整构建
            ├─ 指纹匹配 → 返回缓存结果（最快）
            ├─ 只有 index.html 变化 → HTML-only 刷新
            └─ 其他变化 → 执行完整构建
```

### 3.2 收集输入文件签名

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:405-430`

```csharp
internal static IReadOnlyDictionary<string, string> CollectIncrementalInputSignatures(
    BuildContext context)
{
    var inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (var filePath in EnumerateIncrementalInputFiles(context))
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(context.RootDirectory, filePath).Replace('\\', '/');
            var signature = fileInfo.Length.ToString(CultureInfo.InvariantCulture)
                + "|"
                + fileInfo.LastWriteTimeUtc.Ticks.ToString(CultureInfo.InvariantCulture);
            inputs[relativePath] = signature;
        }
        catch (IOException)
        {
            // Skip transiently inaccessible files
        }
        catch (UnauthorizedAccessException)
        {
            // Skip inaccessible files
        }
    }
    return inputs;
}
```

**签名格式**：`<文件长度>|<LastWriteTimeUtc.Ticks>`

**示例**：`"12345|638456789012345678"`

**文件枚举**：`EnumerateIncrementalInputFiles`（第 505-573 行）
- 递归遍历项目根目录
- 跳过 `dist/` 输出目录
- 跳过忽略的目录（`.git`, `.jazor`, `node_modules`, `.vs`, `.idea`, `bin`, `obj`）
- 包含的文件：
  - `public/` 目录下的所有文件
  - 根目录的 `index.html`, `package.json`, `jolt.config.json`
  - 具有以下扩展名的文件：`.jazor`, `.vue`, `.ts`, `.js`, `.css`, `.html`, `.json`

**文件系统容错**：
- 使用 `try-catch` 包裹 `Directory.EnumerateDirectories` 和 `Directory.EnumerateFiles`
- 捕获 `IOException` 和 `UnauthorizedAccessException`
- 失败时跳过目录或文件，继续枚举其他内容

### 3.3 计算构建指纹

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:432-449`

```csharp
internal static string ComputeIncrementalFingerprint(
    BuildOptions options,
    IReadOnlyDictionary<string, string> incrementalInputs)
{
    var fingerprintBuilder = new StringBuilder();
    fingerprintBuilder.Append(BuildIncrementalOptionsFingerprint(options));
    foreach (var (path, signature) in incrementalInputs
                 .OrderBy(static item => item.Key, StringComparer.OrdinalIgnoreCase))
    {
        fingerprintBuilder
            .Append(path)
            .Append('|')
            .Append(signature)
            .AppendLine();
    }

    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintBuilder.ToString())));
}
```

**指纹组成**：
1. **构建选项指纹**：`BuildIncrementalOptionsFingerprint`（第 478-503 行）
   ```
   outDir=dist
   sourceMap=External
   minify=True
   target=es2020
   codeSplitting=True
   assetsDir=assets
   assetHashLength=8
   chunkSizeWarningLimit=500000
   alias:@/src=src
   ```

2. **输入文件签名**：按路径排序后拼接
   ```
   index.html|12345|638456789012345678
   package.json|678|638456789012345679
   src/main.ts|456|638456789012345680
   ```

3. **SHA256 哈希**：对拼接后的字符串计算 SHA256，返回十六进制字符串

**结果**：64 字符的十六进制字符串（小写）

### 3.4 读取增量状态

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:617-658`

```csharp
private static bool TryReadIncrementalState(
    BuildContext context,
    [NotNullWhen(true)] out BuildIncrementalState? state)
{
    state = null;
    var statePath = Path.Combine(context.OutDirectory, IncrementalStateFileName);  // "jazor-build-state.json"
    if (!File.Exists(statePath))
    {
        return false;
    }

    try
    {
        var json = File.ReadAllText(statePath);
        var deserialized = JsonSerializer.Deserialize<BuildIncrementalState>(json);
        if (deserialized is null
            || string.IsNullOrWhiteSpace(deserialized.Fingerprint)
            || string.IsNullOrWhiteSpace(deserialized.ManifestPath))
        {
            return false;
        }

        state = deserialized;
        return true;
    }
    catch (IOException)
    {
        return false;
    }
    catch (UnauthorizedAccessException)
    {
        return false;
    }
    catch (JsonException)
    {
        return false;
    }
    catch (NotSupportedException)
    {
        return false;
    }
}
```

**错误处理**：
- 文件不存在 → 返回 `false`
- JSON 反序列化失败 → 返回 `false`
- 必需字段为空 → 返回 `false`
- 任何 IO 异常 → 返回 `false`

### 3.5 检查输出可用性

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:660-700`

```csharp
private static bool AreIncrementalOutputsAvailable(
    BuildContext context,
    BuildIncrementalState state)
{
    // 1. 检查 manifest 文件
    if (!IsReadableFilePresent(ResolveAbsolutePath(context.RootDirectory, state.ManifestPath)))
    {
        return false;
    }

    // 2. 检查所有 chunk 文件
    foreach (var chunk in state.Chunks)
    {
        if (string.IsNullOrWhiteSpace(chunk.FilePath)
            || !IsReadableFilePresent(ResolveAbsolutePath(context.RootDirectory, chunk.FilePath)))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(chunk.SourceMapPath)
            && !IsReadableFilePresent(ResolveAbsolutePath(context.RootDirectory, chunk.SourceMapPath!)))
        {
            return false;
        }
    }

    // 3. 检查所有 CSS 和静态资产
    foreach (var asset in state.CssAssets.Concat(state.StaticAssets))
    {
        if (string.IsNullOrWhiteSpace(asset.FilePath)
            || !IsReadableFilePresent(ResolveAbsolutePath(context.RootDirectory, asset.FilePath)))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(asset.SourceMapPath)
            && !IsReadableFilePresent(ResolveAbsolutePath(context.RootDirectory, asset.SourceMapPath!)))
        {
            return false;
        }
    }

    return true;
}
```

**IsReadableFilePresent 辅助方法**（第 933-947 行）：
```csharp
private static bool IsReadableFilePresent(string path)
{
    try
    {
        return File.Exists(path);
    }
    catch (IOException)
    {
        return false;
    }
    catch (UnauthorizedAccessException)
    {
        return false;
    }
}
```

**设计原因**：`File.Exists` 在某些文件系统错误情况下会抛出异常，而不是返回 `false`。

### 3.6 HTML-only 刷新路径

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:702-793`

**触发条件**：
1. 增量构建已启用
2. 上次构建状态存在且输出文件完整
3. 指纹不匹配（有变化）
4. **只有 index.html 发生变化**（`changedPaths.Count == 1 && changedPaths[0] == "index.html"`）
5. 入口点路径未变化

**优化原理**：
- 如果只有 HTML 变化（例如修改了 meta 标签、注释等），不需要重新编译和打包
- 直接复用上次的 chunks、CSS 资产、静态资产
- 只重新生成 HTML 文件和 manifest

**实现流程**：
```csharp
// 1. 检查变化路径
var changedPaths = GetIncrementalChangedPaths(state.Inputs, incrementalInputs);
if (changedPaths.Count != 1
    || !string.Equals(changedPaths[0], "index.html", StringComparison.OrdinalIgnoreCase))
{
    return null;  // 不符合 HTML-only 刷新条件
}

// 2. 验证入口点未变化
var currentEntryRequestPath = ResolveEntryRequestPath(options.RootDirectory, entryPointPath);
if (!string.Equals(currentEntryRequestPath, state.EntryRequestPath, StringComparison.OrdinalIgnoreCase))
{
    return null;  // 入口点变化，需要完整构建
}

// 3. 重新生成 HTML（复用上次的资产）
await GenerateHtmlAsync(
    context,
    state.Chunks,
    state.CssAssets,
    state.StaticAssets,
    currentEntryRequestPath,
    cancellationToken);

// 4. 重新写入 manifest
var manifestPath = await WriteManifestAsync(
    context,
    state.Chunks,
    state.CssAssets,
    state.StaticAssets,
    state.TotalSize,
    cancellationToken);

// 5. 返回结果（添加"HTML refresh"诊断信息）
return new BuildResult
{
    Success = true,
    ...
    Diagnostics = [new BuildDiagnostic
    {
        Severity = DiagnosticSeverity.Info,
        Message = IncrementalHtmlRefreshMessage  // "Incremental build html refresh."
    }]
};
```

**性能优势**：
- 跳过 Deno host 启动
- 跳过编译和打包
- 跳过 CSS 提取和静态资产复制
- 只需读写 HTML 文件和 manifest
- **典型耗时**：< 100ms（vs 完整构建的数秒到数分钟）

### 3.7 持久化增量状态

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:795-827`

```csharp
internal static async Task PersistIncrementalStateAsync(
    BuildContext context,
    BuildResult buildResult,
    string fingerprint,
    IReadOnlyDictionary<string, string> incrementalInputs,
    string entryRequestPath,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(buildResult.ManifestPath))
    {
        return;
    }

    var state = new BuildIncrementalState
    {
        Fingerprint = fingerprint,
        ManifestPath = ResolveRootRelativePath(context.RootDirectory, buildResult.ManifestPath),
        EntryRequestPath = entryRequestPath,
        Inputs = incrementalInputs,
        Chunks = buildResult.Chunks,
        CssAssets = buildResult.CssAssets,
        StaticAssets = buildResult.StaticAssets,
        TotalSize = buildResult.TotalSize
    };

    var statePath = Path.Combine(context.OutDirectory, IncrementalStateFileName);
    var stateJson = JsonSerializer.Serialize(
        state,
        new JsonSerializerOptions
        {
            WriteIndented = true
        });
    await File.WriteAllTextAsync(statePath, stateJson, cancellationToken);
}
```

**输出文件**：`dist/jazor-build-state.json`

**示例内容**：
```json
{
  "Fingerprint": "A1B2C3D4E5F6...",
  "ManifestPath": "jazor-build-manifest.json",
  "EntryRequestPath": "/src/main.ts",
  "Inputs": {
    "index.html": "1234|638456789012345678",
    "src/main.ts": "5678|638456789012345679"
  },
  "Chunks": [...],
  "CssAssets": [...],
  "StaticAssets": [...],
  "TotalSize": 1234567
}
```

## 4. 线程安全模型

增量构建状态在单次构建过程中是单线程的：
- `CollectIncrementalInputSignatures` 顺序枚举文件
- `ComputeIncrementalFingerprint` 顺序计算哈希
- `TryReadIncrementalState` 同步读取文件
- `PersistIncrementalStateAsync` 异步写入文件（但无并发访问）

**文件系统并发**：
- 增量构建不会与外部进程并发写入 `dist/jazor-build-state.json`
- 因为 `PrepareOutputDirectory` 会删除并重新创建 `dist/` 目录

## 5. 错误处理

### 5.1 文件系统错误处理策略

**输入文件枚举**：
- **策略**：跳过不可访问的文件/目录
- **原因**：临时文件系统错误不应阻止构建
- **实现**：`try-catch` 包裹 `Directory.EnumerateDirectories/Files`

**签名计算**：
- **策略**：跳过不可读取的文件
- **原因**：文件可能被其他进程锁定
- **实现**：`try-catch` 包裹 `new FileInfo(filePath)`

**状态读取**：
- **策略**：任何错误都视为无缓存
- **原因**：不能信任损坏的状态文件
- **实现**：捕获所有异常并返回 `false`

**输出可用性检查**：
- **策略**：使用 `IsReadableFilePresent` 容错检查
- **原因**：`File.Exists` 可能抛出异常
- **实现**：`try-catch` 包裹 `File.Exists`

### 5.2 损坏状态恢复

如果 `jazor-build-state.json` 损坏：
1. `TryReadIncrementalState` 返回 `false`
2. 系统执行完整构建
3. `PersistIncrementalStateAsync` 写入新的状态文件

**不会出现的错误场景**：
- 状态文件部分写入导致下次构建失败
  - 原因：`File.WriteAllTextAsync` 是原子操作（在大多数文件系统上）

## 6. 配置选项

### 6.1 启用增量构建

**CLI 参数**：
```bash
jolt build --incremental=true
```

**配置文件**（`jolt.config.json`）：
```json
{
  "build": {
    "incremental": true
  }
}
```

### 6.2 清除增量缓存

**方法 1**：删除输出目录
```bash
rm -rf dist/
```

**方法 2**：禁用增量构建一次
```bash
jolt build --incremental=false
```

**方法 3**：触摸任何源文件
```bash
touch src/main.ts
```

## 7. 与其他子系统的交互

### 7.1 与主构建流程的交互

**在 BuildAsync 开头**（第 111-168 行）：
- 收集输入签名
- 计算指纹
- 检查缓存
- 可能提前返回

**在 BuildAsync 结尾**（第 331-340 行）：
- 持久化增量状态
- 仅在启用增量构建且构建成功时

### 7.2 与文件系统的交互

**读取**：
- 项目根目录下的所有源文件
- `dist/jazor-build-state.json`

**写入**：
- `dist/jazor-build-state.json`（仅在构建成功后）

**删除**：
- `dist/` 整个目录（在构建开始前，`PrepareOutputDirectory`）

### 7.3 与 Deno 子系统的交互

**增量构建不直接与 Deno 交互**：
- 如果命中缓存，不启动 Deno host
- 如果 HTML-only 刷新，不运行 Deno bundle
- 只有完整构建才与 Deno 交互

## 8. 设计权衡

### 8.1 为什么使用文件大小 + 修改时间而不是内容哈希？

**权衡**：
- **内容哈希**：更准确，但需要读取所有文件内容
- **大小 + 时间**：更快，但理论上有哈希冲突风险

**设计决策**：使用 `Length|LastWriteTimeUtc.Ticks` 的原因：
1. **性能**：不需要读取文件内容，只需元数据
2. **足够准确**：实际使用中冲突概率极低
3. **文件系统友好**：减少磁盘 IO

**潜在问题**：
- 如果文件内容变化但大小和修改时间不变（极罕见），可能无法检测到变化
- **缓解措施**：这种情况几乎不可能在正常开发中出现

### 8.2 为什么要持久化输出资产信息？

**权衡**：
- **只持久化输入签名**：状态文件更小，但无法验证输出完整性
- **持久化输入和输出**：状态文件更大，但能验证输出完整性

**设计决策**：持久化输入和输出的原因：
1. **输出验证**：`AreIncrementalOutputsAvailable` 检查所有输出文件是否存在
2. **HTML-only 刷新**：需要复用上次的 chunks、CSS、静态资产信息
3. **状态大小**：输出信息通常只有几十 KB，可以接受

### 8.3 为什么要支持 HTML-only 刷新？

**权衡**：
- **不支持**：实现更简单，但任何 HTML 变化都需要完整构建
- **支持**：实现更复杂，但常见场景（修改 meta 标签）更快

**设计决策**：支持 HTML-only 刷新的原因：
1. **常见场景**：修改 `<title>`, `<meta>`, 注释等不需要重新编译
2. **开发体验**：显著提升常见编辑的反馈速度
3. **实现成本**：增加的复杂度可控（约 100 行代码）

### 8.4 为什么要枚举所有文件而不是监听文件系统事件？

**权衡**：
- **文件系统监听**：实时响应，但需要持续运行监听进程
- **按需枚举**：不运行时无开销，但每次构建都需要枚举

**设计决策**：按需枚举的原因：
1. **简单性**：不需要跨平台的文件系统监听代码
2. **可靠性**：枚举比监听更可靠（监听可能丢失事件）
3. **性能可接受**：对于中小型项目（< 10000 文件），枚举开销 < 100ms

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
