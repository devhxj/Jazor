# Jolt 构建管线 (Build Pipeline)

> 状态：已实现
> 定位：Jolt 构建系统的核心编排器，负责从源代码到生产就绪输出的完整转换流程

## 1. 文档定位

本文档描述 Jolt 构建系统的主流程，包括 BuildOrchestrator 如何协调编译、打包、CSS 提取、静态资源复制和 HTML 生成等环节。

**核心文件**：
- `src/Jolt/Build/BuildOrchestrator.cs` - 主编排器
- `src/Jolt/Build/BuildOptions.cs` - 构建配置选项
- `src/Jolt/Build/BuildResult.cs` - 构建结果
- `src/Jolt/Build/BuildEntryPointResolver.cs` - 入口点解析
- `src/Jolt/Build/BuildManifest.cs` - 构建清单生成
- `src/Jolt/Build/BuildCommandOptionsResolver.cs` - CLI 参数解析

## 2. 核心类型

### 2.1 BuildOptions (构建选项)

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

**配置项说明**：

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

### 2.2 BuildResult (构建结果)

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
    public required string FileName { get; init; }                // 文件名（带哈希）
    public required string FilePath { get; init; }                // 相对于根目录的路径
    public required long Size { get; init; }                      // 文件大小
    public string? SourceMapPath { get; init; }                   // Source map 路径
    public string? OriginalPath { get; init; }                    // 原始路径（public/ 中的路径）
    public IReadOnlyList<string> SourceModulePaths { get; init; } // 来源模块路径
    public IReadOnlyList<string> OwnerChunkFilePaths { get; init; } // 所属 chunk 路径
    public string? OwnerChunkFilePath { get; init; }              // 单一所属 chunk 路径
}
```

### 2.3 BuildDiagnostic (构建诊断)

```csharp
internal sealed class BuildDiagnostic
{
    public required DiagnosticSeverity Severity { get; init; } // Error/Warning/Info
    public required string Message { get; init; }
    public string? File { get; init; }
    public (int Line, int Column)? Location { get; init; }
}
```

## 3. 核心算法

### 3.1 BuildAsync 主流程

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:90-372`

```csharp
public async Task<BuildResult> BuildAsync(
    BuildOptions options,
    CancellationToken cancellationToken = default)
```

**流程图**：

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

**关键步骤详解**：

1. **增量构建检查**（第 111-168 行）：
   - 收集输入文件签名（`CollectIncrementalInputSignatures`）
   - 计算构建指纹（`ComputeIncrementalFingerprint`）
   - 尝试读取上次构建状态（`TryReadIncrementalState`）
   - 如果指纹匹配且输出文件存在，直接返回缓存结果
   - 如果只有 index.html 变化，走 HTML-only 刷新路径

2. **准备输出目录**（第 171 行）：
   - 验证输出目录在项目根内且不等于根目录
   - 删除并重新创建输出目录

3. **启动 Deno host 和开发服务器**（第 173-203 行）：
   - 创建 `DenoVolarHost` 用于编译 .jazor/.vue 文件
   - 创建 `DevHttpServer` 用于提供模块服务（端口自动分配）
   - 创建 `ModuleResolver` 和 `OnDemandCompiler`

4. **解析入口点**（第 205-208 行）：
   - `BuildEntryPointResolver.ResolveEntryPoint` 查找入口文件
   - 支持从 index.html 的 `<script type="module" src="...">` 提取
   - 回退到标准候选路径（src/main.ts、main.js 等）

5. **运行 Deno bundle**（第 210-222 行）：
   - 创建 `DenoBundleRunner`
   - 启动 `BundlerModuleProxyServer`（Kestrel）重写 .jazor/.vue 导入
   - 生成 import map（`DenoBuildImportMapGenerator`）
   - 执行 `deno bundle` CLI 命令
   - 收集 chunk 和 CSS 资产信息

6. **复制静态资产**（第 224-225 行）：
   - `StaticAssetHandler.CopyPublicAssetsAsync` 复制 public/ 目录
   - 对符合条件的文件进行内容哈希（< 4KB 的图片、字体等）

7. **确保构建图编译**（第 226-231 行）：
   - `EnsureBuildGraphCompiledAsync` 遍历模块依赖图
   - 编译所有 .jazor/.vue/.ts/.js/.css 文件
   - 收集编译结果和诊断信息

8. **CSS 处理流程**（第 243-283 行）：
   - 创建 Source map 所有权上下文（`CreateSourceMapOwnershipContext`）
   - 收集提取的 CSS 片段（`CollectExtractedCssFragmentsAsync`）
   - 复制 CSS 引用的源资产（`CopyReferencedSourceAssetsAsync`）
   - 发出哈希 CSS 资产（`EmitExtractedCssAssetsAsync`）
   - 解析 Bundler 产出的 CSS 所有者（`ResolveBundledCssAssetOwners`）
   - 重写 CSS 中的资产引用（`RewriteCssAssetReferencesAsync`）

9. **CSS 到 Chunk 附加**（第 284-292 行）：
   - 读取动态导入关系（`ReadDynamicImportsByChunkAsync`）
   - 构建 CSS 闭包（`BuildCssClosureByChunk`）
   - 将直接 CSS 和传递 CSS 附加到每个 chunk
   - 重写动态 chunk 的 CSS 导入（注入 `__jazorImportCss`）

10. **HTML 生成**（第 302-308 行）：
    - 读取项目根目录的 index.html
    - 移除开发模式脚本引用
    - 重写静态资产引用（`HtmlTransformer.RewriteAssetReferences`）
    - 注入生产 script 标签（入口 chunk）
    - 注入 CSS link 标签（入口 chunk 的 CSS）
    - 写入 dist/index.html

11. **Manifest 生成**（第 309-315 行）：
    - 创建 `BuildManifest` 对象
    - 包含入口路径、chunks、CSS、静态资产、总大小
    - 写入 dist/jazor-build-manifest.json

### 3.2 BuildEntryPointResolver (入口点解析)

**位置**：`src/Jolt/Build/BuildEntryPointResolver.cs`

```csharp
public static string ResolveEntryPoint(string rootDirectory)
```

**解析策略**：

1. **从 index.html 提取**（优先）：
   - 读取项目根目录的 index.html
   - 使用正则表达式匹配 `<script src="...">` 标签
   - 优先选择 `type="module"` 的脚本
   - 回退到第一个有效的本地脚本

2. **标准候选路径**（回退）：
   ```
   src/main.ts
   src/main.js
   src/main.mjs
   src/main.tsx
   src/main.jsx
   main.ts
   main.js
   main.mjs
   main.tsx
   main.jsx
   ```

**支持的后缀**：`.js`, `.jsx`, `.jazor`, `.mjs`, `.mts`, `.ts`, `.tsx`, `.vue`

**安全检查**：
- 拒绝绝对路径、外部 URL、data URI
- 确保解析路径在项目根内

### 3.3 BuildManifest (清单生成)

**位置**：`src/Jolt/Build/BuildManifest.cs`

```csharp
internal sealed class BuildManifest
{
    public required string Entry { get; init; }
    public IReadOnlyList<BuildManifestChunk> Chunks { get; init; } = [];
    public IReadOnlyList<string> Css { get; init; } = [];
    public IReadOnlyList<BuildManifestStaticAsset> StaticAssets { get; init; } = [];
    public long TotalSize { get; init; }
}
```

**输出示例**：
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

### 3.4 BuildCommandOptionsResolver (CLI 参数解析)

**位置**：`src/Jolt/Build/BuildCommandOptionsResolver.cs`

```csharp
public static BuildOptions ResolveBuildOptions(
    string[] args,
    string rootDirectory,
    JazorConfig? config)
```

**参数覆盖顺序**：CLI 参数 > 配置文件 > 默认值

**支持的 CLI 参数**：

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

**配置文件解析**：
- 从 `jazor.config.json` 读取 `build` 配置
- 从 `resolve.alias` 读取别名配置
- 规范化别名键值对（去除首尾空格）

## 4. 线程安全模型

BuildOrchestrator 本身是无状态的，所有状态保存在 `BuildContext` 中：

```csharp
internal sealed class BuildContext
{
    public BuildOptions Options { get; }
    public string RootDirectory { get; }
    public string OutDirectory { get; }
    public string AssetsDirectory { get; }
    public List<BuildDiagnostic> Diagnostics { get; }
}
```

**线程安全保证**：
- `BuildContext` 实例在单次构建中不共享
- 异步操作使用 `CancellationToken` 协调取消
- 文件系统操作使用 `File.WriteAllTextAsync` 等异步方法
- 进程间通信（Deno CLI）使用标准输入输出重定向

## 5. 错误处理

### 5.1 构建失败场景

1. **Deno bundle 失败**：
   - 检查退出码（非零表示失败）
   - 从 stderr 收集错误信息
   - 返回 `Success = false` 的 BuildResult

2. **编译错误**：
   - `OnDemandCompiler` 返回诊断信息
   - 检查是否有 Error 级别的诊断
   - 在早期阶段（编译完成后）中断构建

3. **文件系统错误**：
   - `IOException` 和 `UnauthorizedAccessException` 被捕获
   - 静态资产复制失败会添加 Warning 诊断
   - 关键文件读取失败会中断构建

### 5.2 诊断收集

诊断信息来源：
- Deno CLI stderr（Info 级别）
- 编译器诊断（Error/Warning/Info）
- 构建系统自身诊断（Chunk 大小警告、资产跳过警告等）

## 6. 配置选项

### 6.1 Source Map 选项

| 选项 | Deno 参数 | 行为 |
|------|----------|------|
| `None` | 不传递 `--sourcemap` | 不生成 source map |
| `Inline` | `--sourcemap=inline` | Source map 内联在文件中 |
| `External` | `--sourcemap=linked` | Source map 输出为独立 .map 文件 |

### 6.2 代码分割

- **启用**（`--code-splitting`）：Deno 生成多个 chunk，支持动态导入
- **禁用**：生成单个 bundle 文件

### 6.3 资产哈希

**哈希条件**：
- 扩展名在 `HashExtensions` 集合中（图片、字体、音视频等）
- 文件大小 < 4KB（`HashSizeThreshold`）

**哈希算法**：SHA-256，取前 N 个字符（N 由 `AssetHashLength` 决定）

**文件名格式**：`<basename>-<hash><extension>`

## 7. 与其他子系统的交互

### 7.1 与 DevServer 的交互

- 复用 `DevHttpServer` 提供模块服务
- 复用 `HtmlTransformer` 进行 HTML 转换
- 复用 `OnDemandCompiler` 进行按需编译
- 生产模式禁用 HMR（`HmrEnabled = false`）

### 7.2 与 Deno 子系统的交互

- 使用 `DenoVolarHost` 编译 .jazor/.vue 文件
- 使用 `DenoBundleRunner` 执行 deno bundle CLI
- 使用 `DenoBuildImportMapGenerator` 生成 import map
- 使用 `BundlerModuleProxyServer` 重写导入路径

### 7.3 与 SourceMap 子系统的交互

- 使用 `SourceMapWriter` 生成提取的 CSS 的 source map
- 读取 Deno 生成的 source map 确定 chunk 所有者
- 规范化 source map 中的 source 路径

## 8. 设计权衡

### 8.1 为什么要使用 DevHttpServer？

**权衡**：
- **优势**：复用开发模式的编译和解析逻辑，避免重复实现
- **劣势**：需要启动额外的 HTTP 服务器，增加复杂性

**设计决策**：复用 DevServer 是更好的选择，因为：
1. 编译逻辑已经在 OnDemandCompiler 中实现
2. 模块解析逻辑已经在 ModuleResolver 中实现
3. Deno bundle 需要通过 HTTP 访问模块

### 8.2 为什么要用 BundlerModuleProxyServer？

**权衡**：
- **优势**：重写 .jazor/.vue 导入为 .js，让 Deno bundler 能正确处理
- **劣势**：增加一层代理，可能影响性能

**设计决策**：Deno bundler 不理解 .jazor/.vue 扩展名，必须重写为 ..js。使用代理服务器比修改源文件更安全。

### 8.3 为什么要单独提取 CSS？

**权衡**：
- **优势**：CSS 可以独立缓存，支持按需加载，减少 JavaScript 大小
- **劣势**：增加构建复杂度，需要计算 CSS 所有者

**设计决策**：现代前端最佳实践是分离 CSS 和 JavaScript，即使 Deno bundler 已经提取了 CSS，Jolt 仍需：
1. 从编译结果中提取内联 CSS（<style> 标签）
2. 计算 CSS 到 chunk 的所有者关系
3. 重写 CSS 中的资产引用

### 8.4 为什么要计算 CSS 闭包？

**权衡**：
- **优势**：确保动态导入的 chunk 能加载其依赖的 CSS
- **劣势**：需要复杂的图遍历算法

**设计决策**：如果 chunk A 导入 chunk B，chunk A 应该加载 chunk B 的 CSS。否则动态导入时样式会缺失。

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
