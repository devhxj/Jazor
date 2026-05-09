# Jolt 构建管线 (Build Pipeline)

Jolt 构建系统的核心编排器，负责从源代码到生产就绪输出的完整转换流程。

核心文件：
- `src/Jolt/Build/BuildOrchestrator.cs` - 主编排器
- `src/Jolt/Build/BuildOptions.cs` - 构建配置选项
- `src/Jolt/Build/BuildResult.cs` - 构建结果
- `src/Jolt/Build/BuildEntryPointResolver.cs` - 入口点解析
- `src/Jolt/Build/BuildManifest.cs` - 构建清单生成
- `src/Jolt/Build/BuildCommandOptionsResolver.cs` - CLI 参数解析

## 核心类型

### BuildOptions (构建选项)

**位置**：`src/Jolt/Build/BuildOptions.cs`

```csharp
internal sealed record BuildOptions
{
    public required string RootDirectory { get; init; }
    public string OutDir { get; init; } = "dist";
    public SourceMapOption SourceMap { get; init; } = SourceMapOption.External;
    public bool Minify { get; init; } = true;
    public string Target { get; init; } = "es2020";
    public bool CodeSplitting { get; init; } = true;
    public int ChunkSizeWarningLimit { get; init; } = 500_000;
    public string AssetsDir { get; init; } = "assets";
    public int AssetHashLength { get; init; } = 8;
    public IReadOnlyDictionary<string, string> ResolveAliases { get; init; }
    public bool Incremental { get; init; } = false;
    public bool GenerateSourceMap => SourceMap != SourceMapOption.None;
}
```

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `RootDirectory` | `string` | 必需 | 项目根目录 |
| `OutDir` | `string` | `"dist"` | 输出目录（相对于根目录） |
| `SourceMap` | `SourceMapOption` | `External` | Source map 生成选项（None/Inline/External） |
| `Minify` | `bool` | `true` | 是否压缩代码 |
| `Target` | `string` | `"es2020"` | JavaScript 目标版本 |
| `CodeSplitting` | `bool` | `true` | 是否启用代码分割 |
| `ChunkSizeWarningLimit` | `int` | `500000` | Chunk 大小警告阈值（字节） |
| `AssetsDir` | `string` | `"assets"` | 资产输出目录名称 |
| `AssetHashLength` | `int` | `8` | 内容哈希长度（1-64） |
| `ResolveAliases` | `IReadOnlyDictionary<string, string>` | 空字典 | 模块解析别名 |
| `Incremental` | `bool` | `false` | 是否启用增量构建 |

### BuildResult (构建结果)

**位置**：`src/Jolt/Build/BuildResult.cs`

```csharp
internal sealed class BuildResult
{
    public bool Success { get; init; }
    public string? OutDirectory { get; init; }
    public string? ManifestPath { get; init; }
    public IReadOnlyList<ChunkInfo> Chunks { get; init; } = [];
    public IReadOnlyList<AssetInfo> CssAssets { get; init; } = [];
    public IReadOnlyList<AssetInfo> StaticAssets { get; init; } = [];
    public IReadOnlyList<BuildDiagnostic> Diagnostics { get; init; } = [];
    public TimeSpan Duration { get; init; }
    public long TotalSize { get; init; }
}
```

**ChunkInfo**（JavaScript chunk 信息）：
```csharp
internal sealed class ChunkInfo
{
    public required string FileName { get; init; }      // 文件名（带哈希）
    public required string FilePath { get; init; }      // 相对于根目录的路径
    public required long Size { get; init; }            // 文件大小（字节）
    public bool IsEntry { get; init; }                  // 是否为入口 chunk
    public bool IsDynamic { get; init; }                // 是否为动态导入 chunk
    public IReadOnlyList<string> Imports { get; init; } // 导入的其他 chunk
    public IReadOnlyList<string> Css { get; init; }     // 关联的 CSS 资产
    public string? SourceMapPath { get; init; }         // Source map 路径
}
```

**AssetInfo**（CSS 和静态资产信息）：
```csharp
internal sealed class AssetInfo
{
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public required long Size { get; init; }
    public string? SourceMapPath { get; init; }
    public string? OriginalPath { get; init; }
    public IReadOnlyList<string> SourceModulePaths { get; init; }
    public IReadOnlyList<string> OwnerChunkFilePaths { get; init; }
    public string? OwnerChunkFilePath { get; init; }
}
```

### BuildDiagnostic (构建诊断)

```csharp
internal sealed class BuildDiagnostic
{
    public required DiagnosticSeverity Severity { get; init; } // Error/Warning/Info
    public required string Message { get; init; }
    public string? File { get; init; }
    public (int Line, int Column)? Location { get; init; }
}
```

## 核心算法

### BuildAsync 主流程

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:90-372`

```csharp
public async Task<BuildResult> BuildAsync(
    BuildOptions options,
    CancellationToken cancellationToken = default)
```

```
开始
  ↓
[增量构建检查] → [命中缓存] → 返回缓存结果
  ↓ 未命中
[准备输出目录] (清空 dist/)
  ↓
[启动 Deno host] (DenoVolarHost)
  ↓
[启动开发服务器] (DevHttpServer, 端口 0)
  ↓
[解析入口点] (BuildEntryPointResolver)
  ↓
[运行 Deno bundle] (DenoBundleRunner)
  ↓
[复制 public/ 资产] (StaticAssetHandler)
  ↓
[确保构建图编译] (OnDemandCompiler 遍历依赖)
  ↓
[收集 CSS 片段] (从编译结果和 SourceMap)
  ↓
[复制 CSS 引用的源资产]
  ↓
[发出提取的 CSS 资产] (哈希文件名 + SourceMap)
  ↓
[解析 Bundler 产出的 CSS 所有者]
  ↓
[重写 CSS 资产引用] (url() 路径)
  ↓
[附加 CSS 资产到 chunks] (计算 CSS 闭包)
  ↓
[重写动态 chunk CSS 导入] (注入 __jazorImportCss)
  ↓
[刷新 chunk/asset 大小]
  ↓
[生成 HTML] (index.html, 注入 script/link)
  ↓
[写入 manifest] (jazor-build-manifest.json)
  ↓
[持久化增量状态] (可选)
  ↓
返回 BuildResult
```

### BuildEntryPointResolver (入口点解析)

**位置**：`src/Jolt/Build/BuildEntryPointResolver.cs`

解析策略：
1. 从 index.html 提取（优先）：读取 `<script src="...">` 标签，优先 `type="module"` 脚本，回退到第一个有效本地脚本
2. 标准候选路径（回退）：`src/main.ts`、`src/main.js`、`src/main.mjs`、`src/main.tsx`、`src/main.jsx`、`main.ts`、`main.js`、`main.mjs`、`main.tsx`、`main.jsx`

安全检查：拒绝绝对路径、外部 URL、data URI，确保解析路径在项目根内。

### BuildManifest (清单生成)

**位置**：`src/Jolt/Build/BuildManifest.cs`

输出示例（`dist/jazor-build-manifest.json`）：
```json
{
  "Entry": "assets/index-abc123.js",
  "Chunks": [
    {
      "File": "assets/index-abc123.js",
      "IsEntry": true,
      "Imports": ["assets/vendor-def456.js"],
      "Css": ["assets/styles-ghi789.css"],
      "SourceMap": "assets/index-abc123.js.map"
    }
  ],
  "Css": ["assets/styles-ghi789.css"],
  "StaticAssets": [
    {
      "File": "assets/logo-jkl012.png",
      "OriginalPath": "/logo.png"
    }
  ],
  "TotalSize": 1234567
}
```

### BuildCommandOptionsResolver (CLI 参数解析)

**位置**：`src/Jolt/Build/BuildCommandOptionsResolver.cs`

参数覆盖顺序：CLI 参数 > 配置文件 > 默认值。

| 参数 | 格式 | 示例 |
|------|------|------|
| `--sourcemap` | `--sourcemap=inline\|external\|none` | `--sourcemap=inline` |
| `--minify` | `--minify=true\|false` | `--minify=false` |
| `--out-dir` | `--out-dir=<path>` | `--out-dir=build` |
| `--target` | `--target=<esversion>` | `--target=es2022` |
| `--code-splitting` | `--code-splitting=true\|false` | `--code-splitting=false` |
| `--assets-dir` | `--assets-dir=<name>` | `--assets-dir=static` |
| `--asset-hash-length` | `--asset-hash-length=<number>` | `--asset-hash-length=16` |
| `--chunk-size-warning-limit` | `--chunk-size-warning-limit=<bytes>` | `--chunk-size-warning-limit=1000000` |
| `--incremental` | `--incremental=true\|false` | `--incremental=true` |

## 线程安全模型

BuildOrchestrator 本身无状态，所有状态保存在 `BuildContext` 中。`BuildContext` 实例在单次构建中不共享，异步操作使用 `CancellationToken` 协调取消，文件系统操作使用 `File.WriteAllTextAsync` 等异步方法，进程间通信使用标准输入输出重定向。

## 错误处理

**Deno bundle 失败**：检查退出码（非零表示失败），从 stderr 收集错误信息，返回 `Success = false` 的 BuildResult。

**编译错误**：`OnDemandCompiler` 返回诊断信息，检查 Error 级别诊断，在编译完成后中断构建。

**文件系统错误**：`IOException` 和 `UnauthorizedAccessException` 被捕获，静态资产复制失败添加 Warning 诊断，关键文件读取失败中断构建。

## 配置选项

### Source Map 选项

| 选项 | Deno 参数 | 行为 |
|------|----------|------|
| `None` | 不传递 `--sourcemap` | 不生成 source map |
| `Inline` | `--sourcemap=inline` | Source map 内联在文件中 |
| `External` | `--sourcemap=linked` | Source map 输出为独立 .map 文件 |

### 代码分割

启用（`--code-splitting`）时 Deno 生成多个 chunk 支持动态导入，禁用时生成单个 bundle 文件。

### 资产哈希

哈希条件：扩展名在 `HashExtensions` 中（图片、字体、音视频等）且文件大小 < 4KB（`HashSizeThreshold`）。哈希算法 SHA-256，取前 N 个字符，文件名格式 `<basename>-<hash><extension>`。

## 与其他子系统的交互

**DevServer**：复用 `DevHttpServer` 提供模块服务、`HtmlTransformer` 进行 HTML 转换、`OnDemandCompiler` 按需编译。生产模式禁用 HMR（`HmrEnabled = false`）。

**Deno 子系统**：使用 `DenoVolarHost` 编译 .jazor/.vue 文件、`DenoBundleRunner` 执行 deno bundle CLI、`DenoBuildImportMapGenerator` 生成 import map、`BundlerModuleProxyServer` 重写导入路径。

**SourceMap 子系统**：使用 `SourceMapWriter` 生成提取的 CSS 的 source map，读取 Deno 生成的 source map 确定 chunk 所有者。

## 设计权衡

### 为什么使用 DevHttpServer

复用开发模式的编译和解析逻辑避免重复实现。编译逻辑已在 OnDemandCompiler 中实现，模块解析已在 ModuleResolver 中实现，Deno bundle 需要通过 HTTP 访问模块。

### 为什么用 BundlerModuleProxyServer

Deno bundler 不理解 .jazor/.vue 扩展名，必须重写为 .js。使用代理服务器比修改源文件更安全。

### 为什么单独提取 CSS

CSS 独立加载支持浏览器并行下载、缓存策略分离、按需加载减少 JavaScript 大小。即使 Deno bundler 已经提取了 CSS，仍需从编译结果提取内联 CSS、计算 CSS 到 chunk 的所有者关系、重写 CSS 中的资产引用。

### 为什么计算 CSS 闭包

如果 chunk A 导入 chunk B，chunk A 应该加载 chunk B 的 CSS。否则动态导入时样式会缺失。
