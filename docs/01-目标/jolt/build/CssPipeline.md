# Jolt CSS 处理管线 (CSS Pipeline)

从编译结果和 Deno bundle 提取 CSS，计算所有权，重写引用，生成哈希资产。

核心文件：
- `src/Jolt/Build/BuildOrchestrator.CssPipeline.cs` - CSS 提取和处理主逻辑
- `src/Jolt/Build/CssUrlRewriter.cs` - CSS url() 引用重写

## 核心类型

### CssFragment (CSS 片段)

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:52-58`

```csharp
private readonly record struct CssFragment(
    string Content,                              // CSS 内容
    string SourcePublicPath,                     // 来源公共路径
    string SourcePath,                           // 来源文件系统路径
    int? SourceLineStart,                        // 起始行号（用于 source map）
    int? SourceLineCount,                        // 行数（用于 source map）
    IReadOnlyList<string> OwnerChunkFilePaths);   // 所属 chunk 路径列表
```

表示从模块中提取的一段 CSS，来源包括 `.jazor`/`.vue` 文件的 `<style>` 标签、`.css` 文件内容。

### EmittedCssFragment (发出的 CSS 片段)

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:59-64`

```csharp
private readonly record struct EmittedCssFragment(
    string Content,                              // 重写后的 CSS 内容
    string SourcePath,                           // 来源路径
    int? SourceLineStart,                        // 起始行号
    int? SourceLineCount,                        // 行数
    IReadOnlyList<string> OwnerChunkFilePaths);   // 所属 chunk 路径列表
```

重写 CSS 中的资产引用后，准备输出到最终 CSS 文件的内容。

### SourceMapOwnershipContext (Source Map 所有权上下文)

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:65-67`

```csharp
private sealed record SourceMapOwnershipContext(
    IReadOnlyDictionary<string, IReadOnlySet<string>> ChunkFilePathsByModulePath,        // 模块 → chunk 映射
    IReadOnlyDictionary<string, IReadOnlySet<string>> ImporterModulePathsByCssPath);     // CSS → 导入者模块映射
```

通过 Deno bundle 生成的 source map 确定：哪些模块属于哪个 chunk、哪个模块导入了哪个 CSS 文件。

## 核心算法

### CSS 处理主流程

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:243-292`

```csharp
// 1. 创建 source map 所有权上下文
var sourceMapOwnershipContext = CreateSourceMapOwnershipContext(...);

// 2. 收集提取的 CSS 片段
var cssFragments = await CollectExtractedCssFragmentsAsync(...);

// 3. 复制 CSS 引用的源资产（如图片、字体等）
var sourceCssAssets = await CopyReferencedSourceAssetsAsync(...);

// 4. 发出提取的 CSS 资产（哈希文件名 + source map）
var extractedCssAssets = await EmitExtractedCssAssetsAsync(...);

// 5. 解析 Bundler 产出的 CSS 所有者
var bundlerCssAssets = ResolveBundledCssAssetOwners(...);

// 6. 重写 CSS 资产引用（url() 路径）
await RewriteCssAssetReferencesAsync(...);

// 7. 附加 CSS 资产到 chunks（计算 CSS 闭包）
var cssAssets = RefreshAssetSizes(context, [... bundlerCssAssets, .. extractedCssAssets]);
var chunksWithCss = await AttachCssAssetsToChunksAsync(...);

// 8. 重写动态 chunk 的 CSS 导入
await RewriteDynamicChunkCssImportsAsync(...);
```

### 收集 CSS 片段

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:329-360`

两种所有权确定策略：

#### 策略 1：从 Source Map 确定（优先）

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:362-467`

通过 `sourceMapOwnershipContext.ChunkFilePathsByModulePath` 从 Deno bundle 的 source map 读取模块到 chunk 的映射，每个 CSS 片段携带 `OwnerChunkFilePaths`。

步骤：
1. 收集可达模块路径
2. 遍历每个模块，获取所属 chunk
3. 收集嵌入样式依赖（`<style scoped>` 等）
4. 遍历依赖，收集独立的 CSS 文件
5. 收集内联样式（`<style>` 标签或 `styleContent`）
6. 为独立的 CSS 文件创建片段

#### 策略 2：启发式所有权推断（回退）

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:469-513`

触发条件：`sourceMapOwnershipContext is null`（没有 source map）。

推断逻辑：入口点模块 → 入口 chunk；动态导入的根模块 → 通过动态导入关系推断；未匹配的模块 → 通过文件名 stem 匹配（例如 `About-xxx.js` 匹配 `About.ts`）。

### 发出提取的 CSS 资产

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:18-130`

按 chunk 所有者分组 CSS 片段，重写资产引用，合并、压缩，生成哈希文件名和 source map。

CSS 资产命名规则：

| 所属情况 | 基础名称 | 示例 |
|---------|---------|------|
| 只有入口 chunk | `styles` | `styles-abc123.css` |
| 单个非入口 chunk | `<chunk-name>-styles` | `about-def456-styles.css` |
| 多个 chunk（共享） | `shared-<hash>-styles` | `shared-ghi789-styles.css` |

哈希生成：SHA-256 哈希 CSS 内容，取前 N 个字符（N = `AssetHashLength`，默认 8），文件名格式 `<basename>-<hash>.css`。

### CSS 压缩

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:240-327`

```csharp
private static string MinifyExtractedCss(string css, bool preserveLineMapping)
{
    var withoutComments = RemoveBlockComments(css, preserveLineMapping);
    return preserveLineMapping
        ? MinifyCssPreservingLines(withoutComments)
        : MinifyCssCompact(withoutComments);
}
```

压缩策略：
1. 移除 `/* ... */` 注释（`preserveLineMapping` 时保留换行）
2. 保留行压缩：逐行压缩，移除行内多余空格，移除 `;}` 前的分号，规范化结构字符周围的空格
3. 紧凑压缩：在保留行压缩基础上移除所有换行

```css
/* 输入 */
.container {
    margin: 0;
    padding: 10px;
}

/* 保留行压缩 */
.container{margin:0;padding:10px;}

/* 紧凑压缩 */
.container{margin:0;padding:10px}
```

### CSS Source Map 生成

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:132-187`

为每个发出的 CSS 片段创建行级映射：sources（来源文件路径列表）、segments（生成行 → 来源行）、sourceContent（来源文件内容内嵌在 source map 中）。

### 附加 CSS 到 Chunks

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:580-632`

1. 收集直接 CSS（每个 chunk 的直接 CSS）
2. 读取动态导入关系
3. 构建 CSS 闭包（传递性）：静态导入的 chunk 应加载其依赖的 CSS，动态导入通过 `__jazorImportCss` 按需加载
4. 更新 chunk 的 CSS 列表

CSS 闭包计算：

```csharp
IReadOnlyList<string> ResolveCssClosure(string chunkFilePath, HashSet<string> visitingChunkFilePaths)
{
    // 1. 添加直接 CSS
    // 2. 遍历静态导入的 chunk（排除动态导入），递归合并 CSS
    var resolvedCssClosure = cssClosure.OrderBy(...).ToArray();
    cssClosureByChunk[chunkFilePath] = resolvedCssClosure;
    return resolvedCssClosure;
}
```

### 重写动态 Chunk CSS 导入

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:634-693`

扫描每个 chunk 中的动态导入表达式，为目标 chunk 注入 `__jazorImportCss` 调用：

```javascript
// 原始代码
const module = await import('./about.js');

// 转换后
const module = await ((globalThis.__jazorImportCss ??= async function(hrefs){
    if(typeof document==="undefined"||!Array.isArray(hrefs)||hrefs.length===0){return;}
    const registry=globalThis.__jazorLoadedCss ??= new Set();
    await Promise.all(hrefs.map(function(href){
        if(!href||registry.has(href)){return Promise.resolve();}
        const existing=document.querySelector('link[rel="stylesheet"][href="'+href+'"]');
        if(existing){registry.add(href);return Promise.resolve();}
        return new Promise(function(resolve,reject){
            const link=document.createElement("link");
            link.rel="stylesheet";
            link.href=href;
            link.onload=function(){registry.add(href);resolve();};
            link.onerror=function(){reject(new Error("Failed to load stylesheet "+href));};
            document.head.appendChild(link);
        });
    }));
}),globalThis.__jazorImportCss(["/assets/about-styles.css"]).then(function(){
    return import('./about.js');
}));
```

运行时行为：定义全局 `__jazorImportCss` 函数（首次），调用加载 CSS，CSS 加载完成后执行原始动态导入。

## 线程安全模型

CSS 处理流程是单线程的：所有操作顺序执行，异步操作用于 IO 不是并发。每个 `CssFragment` 独立，`SourceMapOwnershipContext` 构建后不修改。

## 错误处理

**Source Map 读取失败**：返回空列表或 `null`，回退到启发式所有权推断。

**CSS 资产引用重写失败**：`CssUrlRewriter.RewriteAssetReferences` 保留原始 `url()`，不中断构建。

## 配置选项

影响 CSS 的 BuildOptions：

| 选项 | 影响 |
|------|------|
| `Minify` | 是否压缩 CSS |
| `SourceMap` | 是否生成 CSS source map |
| `AssetHashLength` | CSS 文件名哈希长度 |

CSS 输出目录由 `AssetsDir` 控制（默认 `"assets"`）：CSS 文件输出到 `dist/assets/`，Source map 输出到 `dist/assets/*.css.map`。

## 与其他子系统的交互

- **编译器**：读取 `CompilationResult.StyleFragments`、`StyleContent`、`EmbeddedStyleDependencies`
- **SourceMap 子系统**：读取 Deno 生成的 source map（`chunk.SourceMapPath`），生成提取的 CSS 的 source map
- **静态资产处理**：读取 `StaticAssetHandler` 复制的资产列表，重写 CSS 中的 `url()` 引用指向哈希后的资产路径

## 设计权衡

### 为什么提取 CSS

CSS 内联在 JavaScript 中加载简单但缓存不友好；提取后 CSS 和 JavaScript 可以并行下载、独立缓存，是现代前端框架的标准做法。

### 为什么计算 CSS 闭包

只附加直接 CSS 会导致动态导入时样式缺失。计算传递性闭包确保静态导入的 chunk 加载其依赖的 CSS，动态导入通过 `__jazorImportCss` 按需加载。

### 为什么注入 `__jazorImportCss`

开发者无需手动为动态导入添加 CSS 加载逻辑，符合"约定优于配置"的设计理念。
