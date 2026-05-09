# Jolt 增量构建 (Incremental Build)

通过文件指纹缓存避免不必要的重建，提升构建速度。

核心文件：
- `src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs` - 增量构建逻辑

## 核心类型

### BuildIncrementalState (增量构建状态)

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

持久化到 `dist/jazor-build-state.json`。

### 增量构建相关的 BuildOptions 字段

```csharp
public bool Incremental { get; init; } = false;  // 是否启用增量构建
```

## 核心算法

### 增量构建主流程

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

决策树：

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

### 收集输入文件签名

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
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    return inputs;
}
```

签名格式：`<文件长度>|<LastWriteTimeUtc.Ticks>`，例如 `"12345|638456789012345678"`。

文件枚举（`EnumerateIncrementalInputFiles`，第 505-573 行）：递归遍历项目根目录，跳过 `dist/` 输出目录和忽略目录（`.git`, `.jazor`, `node_modules`, `.vs`, `.idea`, `bin`, `obj`）。包含 `public/` 目录下所有文件、根目录的 `index.html`/`package.json`/`jolt.config.json`，以及 `.jazor`/`.vue`/`.ts`/`.js`/`.css`/`.html`/`.json` 扩展名的文件。文件系统容错使用 `try-catch` 包裹 `Directory.EnumerateDirectories` 和 `Directory.EnumerateFiles`。

### 计算构建指纹

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

指纹组成：
1. 构建选项指纹（`BuildIncrementalOptionsFingerprint`，第 478-503 行）：`outDir=dist`, `sourceMap=External`, `minify=True`, `target=es2020` 等
2. 输入文件签名按路径排序后拼接
3. SHA256 哈希，返回 64 字符的十六进制字符串

### 读取增量状态

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:617-658`

读取 `dist/jazor-build-state.json`，反序列化为 `BuildIncrementalState`。文件不存在、JSON 反序列化失败、必需字段为空、任何 IO 异常都返回 `false`（视为无缓存）。

### 检查输出可用性

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:660-700`

检查 manifest 文件、所有 chunk 文件、所有 CSS 和静态资产是否存在。使用 `IsReadableFilePresent` 辅助方法（`File.Exists` 在某些文件系统错误情况下会抛出异常，需 try-catch）。

### HTML-only 刷新路径

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:702-793`

触发条件：增量构建已启用、上次构建状态存在且输出完整、指纹不匹配、**只有 index.html 发生变化**、入口点路径未变化。

优化原理：只有 HTML 变化（如 meta 标签、注释）不需要重新编译和打包，直接复用上次的 chunks/CSS/静态资产，只重新生成 HTML 和 manifest。跳过 Deno host 启动、编译打包、CSS 提取和静态资产复制，典型耗时 < 100ms。

### 持久化增量状态

**位置**：`src/Jolt/Build/BuildOrchestrator.RuntimeAndIncremental.cs:795-827`

构建成功后将 `BuildIncrementalState` 序列化为 JSON 写入 `dist/jazor-build-state.json`（带缩进）。

输出示例：
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

## 线程安全模型

增量构建在单次构建过程中是单线程的：`CollectIncrementalInputSignatures` 顺序枚举、`ComputeIncrementalFingerprint` 顺序计算、`TryReadIncrementalState` 同步读取、`PersistIncrementalStateAsync` 异步写入但无并发访问。`PrepareOutputDirectory` 会删除并重新创建 `dist/` 目录，保证不会与外部进程并发写入。

## 错误处理

**输入文件枚举**：跳过不可访问的文件/目录（`IOException`、`UnauthorizedAccessException`）。

**签名计算**：跳过不可读取的文件（文件可能被其他进程锁定）。

**状态读取**：任何错误都视为无缓存（不能信任损坏的状态文件）。

**输出可用性检查**：使用 `IsReadableFilePresent` 容错检查。

**损坏状态恢复**：`TryReadIncrementalState` 返回 `false` → 执行完整构建 → `PersistIncrementalStateAsync` 写入新的状态文件。

## 配置选项

启用增量构建：

CLI：`jolt build --incremental=true`

配置文件（`jolt.config.json`）：
```json
{
  "build": {
    "incremental": true
  }
}
```

清除增量缓存：删除 `dist/` 目录，或 `jolt build --incremental=false` 禁用一次，或 `touch src/main.ts` 触摸任何源文件。

## 与其他子系统的交互

**与主构建流程**：在 `BuildAsync` 开头（第 111-168 行）收集签名、计算指纹、检查缓存，可能提前返回；在 `BuildAsync` 结尾（第 331-340 行）持久化增量状态。

**与文件系统**：读取项目根目录下所有源文件和 `dist/jazor-build-state.json`，写入 `dist/jazor-build-state.json`（构建成功后），删除 `dist/` 整个目录（构建开始前 `PrepareOutputDirectory`）。

**与 Deno 子系统**：增量构建不直接与 Deno 交互。命中缓存不启动 Deno host，HTML-only 刷新不运行 Deno bundle，只有完整构建才与 Deno 交互。

## 设计权衡

### 为什么使用文件大小 + 修改时间而不是内容哈希

内容哈希更准确但需要读取所有文件内容。`Length|LastWriteTimeUtc.Ticks` 不需要读取文件内容只需元数据，实际使用中冲突概率极低，减少磁盘 IO。文件内容变化但大小和修改时间不变的情况极罕见。

### 为什么持久化输出资产信息

只持久化输入签名无法验证输出完整性。持久化输入和输出可以检查所有输出文件是否存在（`AreIncrementalOutputsAvailable`），HTML-only 刷新需要复用上次的 chunks/CSS/静态资产信息。输出信息通常只有几十 KB。

### 为什么支持 HTML-only 刷新

修改 `<title>`、`<meta>`、注释等不需要重新编译，能显著提升常见编辑的反馈速度，增加的复杂度约 100 行代码可控。

### 为什么枚举所有文件而不是监听文件系统事件

文件系统监听需要持续运行监听进程且可能丢失事件，按需枚举更可靠，对于中小型项目（< 10000 文件）枚举开销 < 100ms。
