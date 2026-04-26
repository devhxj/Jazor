# Jolt CSS 处理管线 (CSS Pipeline)

> Status: 活跃参考
> Positioning: 从编译结果和 Deno bundle 提取 CSS，计算所有权，重写引用，生成哈希资产

## 1. 文档定位

本文档描述 Jolt 构建系统中 CSS 的处理流程，包括如何从编译结果中提取 CSS、确定 CSS 归属的 chunk、重写 CSS 中的资产引用、以及生成哈希 CSS 文件。

**核心文件**：
- `src/Jolt/Build/BuildOrchestrator.CssPipeline.cs` - CSS 提取和处理主逻辑
- `src/Jolt/Build/CssUrlRewriter.cs` - CSS url() 引用重写

## 2. 核心类型

### 2.1 CssFragment (CSS 片段)

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

**用途**：表示从模块中提取的一段 CSS，可能来自：
- `.jazor` 文件中的 `<style>` 标签
- `.vue` 文件中的 `<style>` 标签
- `.css` 文件的内容

### 2.2 EmittedCssFragment (发出的 CSS 片段)

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:59-64`

```csharp
private readonly record struct EmittedCssFragment(
    string Content,                              // 重写后的 CSS 内容
    string SourcePath,                           // 来源路径
    int? SourceLineStart,                        // 起始行号
    int? SourceLineCount,                        // 行数
    IReadOnlyList<string> OwnerChunkFilePaths);   // 所属 chunk 路径列表
```

**用途**：在重写 CSS 中的资产引用后，准备输出到最终 CSS 文件的内容。

### 2.3 SourceMapOwnershipContext (Source Map 所有权上下文)

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:65-67`

```csharp
private sealed record SourceMapOwnershipContext(
    IReadOnlyDictionary<string, IReadOnlySet<string>> ChunkFilePathsByModulePath,        // 模块 → chunk 映射
    IReadOnlyDictionary<string, IReadOnlySet<string>> ImporterModulePathsByCssPath);     // CSS → 导入者模块映射
```

**用途**：通过 Deno bundle 生成的 source map 确定：
- 哪些模块属于哪个 chunk
- 哪个模块导入了哪个 CSS 文件

## 3. 核心算法

### 3.1 CSS 处理主流程

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:243-292`

```csharp
// 1. 创建 source map 所有权上下文
var sourceMapOwnershipContext = CreateSourceMapOwnershipContext(
    context.RootDirectory,
    bundleResult.Chunks,
    cachedResults,
    moduleResolver);

// 2. 收集提取的 CSS 片段
var cssFragments = await CollectExtractedCssFragmentsAsync(
    context.RootDirectory,
    cachedResults,
    moduleResolver,
    entryPointPath,
    bundleResult.Chunks,
    sourceMapOwnershipContext,
    cancellationToken);

// 3. 复制 CSS 引用的源资产（如图片、字体等）
var sourceCssAssets = await CopyReferencedSourceAssetsAsync(
    context,
    staticAssetHandler,
    cssFragments,
    staticAssets,
    cancellationToken);

// 4. 发出提取的 CSS 资产（哈希文件名 + source map）
var extractedCssAssets = await EmitExtractedCssAssetsAsync(
    context,
    cssFragments,
    staticAssets,
    bundleResult.Chunks.FirstOrDefault(static chunk => chunk.IsEntry)?.FilePath,
    cancellationToken);

// 5. 解析 Bundler 产出的 CSS 所有者
var bundlerCssAssets = ResolveBundledCssAssetOwners(
    context.RootDirectory,
    bundleResult.Chunks,
    bundleResult.CssAssets,
    sourceMapOwnershipContext,
    moduleResolver);

// 6. 重写 CSS 资产引用（url() 路径）
await RewriteCssAssetReferencesAsync(
    context,
    [... bundlerCssAssets, .. staticAssets.Where(asset => asset.FilePath.EndsWith(".css"))],
    staticAssets,
    cancellationToken);

// 7. 附加 CSS 资产到 chunks（计算 CSS 闭包）
var cssAssets = RefreshAssetSizes(context, [... bundlerCssAssets, .. extractedCssAssets]);
var chunksWithCss = await AttachCssAssetsToChunksAsync(
    context,
    bundleResult.Chunks,
    cssAssets,
    cancellationToken);

// 8. 重写动态 chunk 的 CSS 导入
await RewriteDynamicChunkCssImportsAsync(
    context,
    chunksWithCss,
    cancellationToken);
```

### 3.2 收集 CSS 片段

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:329-360`

```csharp
private static async Task<IReadOnlyList<CssFragment>> CollectExtractedCssFragmentsAsync(
    string rootDirectory,
    IReadOnlyDictionary<string, CompilationResult> cachedResults,
    ModuleResolver moduleResolver,
    string entryPointPath,
    IReadOnlyList<ChunkInfo> chunks,
    SourceMapOwnershipContext? sourceMapOwnershipContext,
    CancellationToken cancellationToken)
{
    if (cachedResults.Count == 0 || !cachedResults.ContainsKey(entryPointPath))
    {
        return [];
    }

    if (sourceMapOwnershipContext is not null)
    {
        // 优先使用 source map 确定所有权
        return await CollectExtractedCssFragmentsFromSourceMapsAsync(...);
    }

    // 回退到启发式所有权推断
    return await CollectExtractedCssFragmentsWithFallbackOwnershipAsync(...);
}
```

**两种所有权确定策略**：

#### 策略 1：从 Source Map 确定（优先）

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:362-467`

```csharp
private static async Task<IReadOnlyList<CssFragment>> CollectExtractedCssFragmentsFromSourceMapsAsync(
    IReadOnlyDictionary<string, CompilationResult> cachedResults,
    ModuleResolver moduleResolver,
    string entryPointPath,
    SourceMapOwnershipContext sourceMapOwnershipContext,
    CancellationToken cancellationToken)
{
    var reachableModulePaths = CollectReachableModulePaths(entryPointPath, cachedResults, moduleResolver);
    var cssFragments = new List<CssFragment>();
    var cssOwnerChunkPathsByPath = new Dictionary<string, HashSet<string>>(FilePathComparer);

    foreach (var modulePath in reachableModulePaths)
    {
        if (!cachedResults.TryGetValue(modulePath, out var result))
        {
            continue;
        }

        // 1. 获取当前模块所属的 chunk
        var ownerChunkFilePaths = GetOwnerChunkFilePaths(
            modulePath,
            sourceMapOwnershipContext.ChunkFilePathsByModulePath);

        // 2. 收集嵌入样式依赖（<style scoped> 等）
        var embeddedStyleDependencyPaths = result.EmbeddedStyleDependencies
            .Select(dependency => moduleResolver.Resolve(dependency, modulePath))
            .Where(static resolved => resolved.Found && !resolved.IsVirtual)
            .Select(static resolved => resolved.AbsolutePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 3. 遍历依赖，收集独立的 CSS 文件
        foreach (var dependency in result.Dependencies)
        {
            var resolved = moduleResolver.Resolve(dependency, modulePath);
            if (!resolved.Found || resolved.IsVirtual
                || !string.Equals(Path.GetExtension(resolved.AbsolutePath), ".css", StringComparison.OrdinalIgnoreCase)
                || embeddedStyleDependencyPaths.Contains(resolved.AbsolutePath))
            {
                continue;
            }

            if (!cssOwnerChunkPathsByPath.TryGetValue(resolved.AbsolutePath, out var cssOwnerChunkPaths))
            {
                cssOwnerChunkPaths = new HashSet<string>(FilePathComparer);
                cssOwnerChunkPathsByPath[resolved.AbsolutePath] = cssOwnerChunkPaths;
            }

            cssOwnerChunkPaths.UnionWith(ownerChunkFilePaths);
        }

        // 4. 收集内联样式（<style> 标签或 styleContent）
        if (!string.Equals(Path.GetExtension(modulePath), ".css", StringComparison.OrdinalIgnoreCase))
        {
            if (result.StyleFragments.Count > 0)
            {
                foreach (var styleFragment in result.StyleFragments)
                {
                    if (string.IsNullOrWhiteSpace(styleFragment.Content))
                    {
                        continue;
                    }

                    cssFragments.Add(new CssFragment(
                        styleFragment.Content,
                        GetStyleFragmentSourcePublicPath(moduleResolver, modulePath, styleFragment),
                        GetStyleFragmentSourcePath(modulePath, styleFragment),
                        styleFragment.SourceLineStart,
                        styleFragment.SourceLineCount,
                        ownerChunkFilePaths));
                }
            }
            else if (!string.IsNullOrWhiteSpace(result.StyleContent))
            {
                cssFragments.Add(new CssFragment(
                    result.StyleContent!,
                    moduleResolver.GetResolvedUrlForAbsolutePath(modulePath).TrimStart('/'),
                    modulePath,
                    null,
                    null,
                    ownerChunkFilePaths));
            }
        }
    }

    // 5. 为独立的 CSS 文件创建片段
    foreach (var (cssPath, ownerChunkPaths) in cssOwnerChunkPathsByPath)
    {
        var cssFragment = await CreateCssDependencyFragmentAsync(
            cssPath,
            ownerChunkPaths,
            cachedResults,
            moduleResolver,
            cancellationToken);
        if (cssFragment is not null)
        {
            cssFragments.Add(cssFragment.Value);
        }
    }

    return cssFragments;
}
```

**关键点**：
- `sourceMapOwnershipContext.ChunkFilePathsByModulePath` 从 Deno bundle 的 source map 读取
- 每个 CSS 片段携带 `OwnerChunkFilePaths`，记录所属的 chunk 路径

#### 策略 2：启发式所有权推断（回退）

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:469-513`

**触发条件**：`sourceMapOwnershipContext is null`（例如没有 source map）

**推断逻辑**：
1. 入口点模块 → 入口 chunk
2. 动态导入的根模块 → 通过动态导入关系推断
3. 未匹配的模块 → 通过文件名 stem 匹配（例如 `About-xxx.js` 匹配 `About.ts`）

### 3.3 发出提取的 CSS 资产

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:18-130`

```csharp
private static async Task<IReadOnlyList<AssetInfo>> EmitExtractedCssAssetsAsync(
    BuildContext context,
    IReadOnlyList<CssFragment> cssFragments,
    IReadOnlyList<AssetInfo> staticAssets,
    string? entryChunkFilePath,
    CancellationToken cancellationToken)
{
    if (cssFragments.Count == 0)
    {
        return [];
    }

    var htmlAssets = staticAssets
        .Select(asset => CreateHtmlAssetInfo(context, asset))
        .ToArray();
    Directory.CreateDirectory(context.AssetsDirectory);
    var assets = new List<AssetInfo>();

    // 1. 按 chunk 所有者分组
    var groupedFragments = cssFragments
        .GroupBy(
            fragment => CreateOwnerChunkSetKey(fragment.OwnerChunkFilePaths, entryChunkFilePath),
            StringComparer.Ordinal)
        .OrderBy(group => IsEntryOnlyOwnerSet(group.First().OwnerChunkFilePaths, entryChunkFilePath) ? 0 : 1)
        .ThenBy(static group => group.Key, StringComparer.Ordinal);

    foreach (var group in groupedFragments)
    {
        var ownerChunkFilePaths = NormalizeOwnerChunkFilePaths(group.First().OwnerChunkFilePaths, entryChunkFilePath);
        var baseName = CreateCssAssetBaseName(ownerChunkFilePaths, entryChunkFilePath);
        var extractedCssPublicPath = Path.GetRelativePath(
            context.OutDirectory,
            Path.Combine(context.AssetsDirectory, baseName + ".css")).Replace('\\', '/');

        // 2. 重写 CSS 中的资产引用
        var emittedFragments = group
            .Select(fragment => new EmittedCssFragment(
                CssUrlRewriter.RewriteAssetReferences(
                    fragment.Content,
                    fragment.SourcePublicPath,
                    extractedCssPublicPath,
                    htmlAssets),
                fragment.SourcePath,
                fragment.SourceLineStart,
                fragment.SourceLineCount,
                ownerChunkFilePaths))
            .Where(static fragment => !string.IsNullOrWhiteSpace(fragment.Content))
            .ToArray();

        if (emittedFragments.Length == 0)
        {
            continue;
        }

        // 3. 合并片段
        var content = string.Join(
            Environment.NewLine + Environment.NewLine,
            emittedFragments.Select(static fragment => fragment.Content));

        // 4. 压缩 CSS（可选）
        var optimizedContent = context.Options.Minify
            ? MinifyExtractedCss(
                content,
                preserveLineMapping: context.Options.GenerateSourceMap)
            : content;

        // 5. 生成哈希文件名
        var fileName = CreateHashedAssetFileName(baseName, ".css", optimizedContent, context.Options.AssetHashLength);
        var outputPath = Path.Combine(context.AssetsDirectory, fileName);

        // 6. 生成 source map（可选）
        string? sourceMapPath = null;
        var finalContent = optimizedContent;
        if (context.Options.GenerateSourceMap)
        {
            var sourceMap = CreateExtractedCssSourceMap(context, emittedFragments, fileName);
            if (!string.IsNullOrWhiteSpace(sourceMap))
            {
                switch (context.Options.SourceMap)
                {
                    case SourceMapOption.External:
                        var sourceMapOutputPath = outputPath + ".map";
                        await File.WriteAllTextAsync(sourceMapOutputPath, sourceMap, cancellationToken);
                        sourceMapPath = Path.GetRelativePath(context.RootDirectory, sourceMapOutputPath).Replace('\\', '/');
                        finalContent = AppendCssSourceMapComment(optimizedContent, Path.GetFileName(sourceMapOutputPath));
                        break;
                    case SourceMapOption.Inline:
                        finalContent = AppendInlineCssSourceMapComment(optimizedContent, sourceMap);
                        break;
                }
            }
        }

        // 7. 写入文件
        await File.WriteAllTextAsync(outputPath, finalContent, cancellationToken);

        assets.Add(new AssetInfo
        {
            FileName = fileName,
            FilePath = Path.GetRelativePath(context.RootDirectory, outputPath).Replace('\\', '/'),
            Size = new FileInfo(outputPath).Length,
            SourceMapPath = sourceMapPath,
            SourceModulePaths = emittedFragments
                .Select(static fragment => fragment.SourcePath)
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Distinct(FilePathComparer)
                .ToArray(),
            OwnerChunkFilePaths = ownerChunkFilePaths,
            OwnerChunkFilePath = ownerChunkFilePaths.Count == 1
                ? ownerChunkFilePaths[0]
                : null
        });
    }

    return assets;
}
```

**CSS 资产命名规则**：

| 所属情况 | 基础名称 | 示例 |
|---------|---------|------|
| 只有入口 chunk | `styles` | `styles-abc123.css` |
| 单个非入口 chunk | `<chunk-name>-styles` | `about-def456-styles.css` |
| 多个 chunk（共享） | `shared-<hash>-styles` | `shared-ghi789-styles.css` |

**哈希生成**：`CreateHashedAssetFileName`
- 使用 SHA-256 哈希 CSS 内容
- 取前 N 个字符（N = `AssetHashLength`，默认 8）
- 文件名格式：`<basename>-<hash>.css`

### 3.4 CSS 压缩

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

**压缩策略**：

1. **移除块注释**（`RemoveBlockComments`）：
   - 移除 `/* ... */` 注释
   - 如果 `preserveLineMapping` 为 true，保留换行以维持行号映射

2. **保留行的压缩**（`MinifyCssPreservingLines`）：
   - 逐行压缩（保留换行）
   - 移除行内多余空格
   - 移除 `;}` 前的分号
   - 规范化结构字符周围的空格（`{}:;,>~`）

3. **紧凑压缩**（`MinifyCssCompact`）：
   - 在行保留压缩基础上，移除所有换行
   - 生成最小化输出

**示例**：
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

### 3.5 CSS Source Map 生成

**位置**：`src/Jolt/Build/BuildOrchestrator.CssPipeline.cs:132-187`

```csharp
private static string? CreateExtractedCssSourceMap(
    BuildContext context,
    IReadOnlyList<EmittedCssFragment> cssFragments,
    string outputFileName)
{
    var sources = new List<SourceMapSource>();
    var segments = new List<SourceMapSegment>();
    var sourceContentCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var sourceIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    var generatedLine = 0;

    for (var fragmentIndex = 0; fragmentIndex < cssFragments.Count; fragmentIndex++)
    {
        var fragment = cssFragments[fragmentIndex];
        var generatedLineCount = CountSourceMapLines(fragment.Content);

        // 从来源文件读取内容
        if (!string.IsNullOrWhiteSpace(fragment.SourcePath)
            && fragment.SourceLineStart.HasValue
            && fragment.SourceLineCount.HasValue
            && TryReadSourceMapContent(fragment.SourcePath, sourceContentCache, out var sourceContent))
        {
            if (!sourceIndices.TryGetValue(fragment.SourcePath, out var sourceIndex))
            {
                sourceIndex = sources.Count;
                sourceIndices[fragment.SourcePath] = sourceIndex;
                sources.Add(new SourceMapSource(
                    CreateSourceMapRelativePath(context.AssetsDirectory, fragment.SourcePath),
                    sourceContent));
            }

            // 映射每一行
            var sourceStartLine = Math.Max(fragment.SourceLineStart.Value - 1, 0);
            var maxSourceLineOffset = Math.Max(fragment.SourceLineCount.Value - 1, 0);
            for (var lineIndex = 0; lineIndex < generatedLineCount; lineIndex++)
            {
                segments.Add(new SourceMapSegment(
                    generatedLine + lineIndex,
                    0,
                    sourceIndex,
                    sourceStartLine + Math.Min(lineIndex, maxSourceLineOffset),
                    0));
            }
        }

        generatedLine += generatedLineCount;
        if (fragmentIndex < cssFragments.Count - 1)
        {
            generatedLine++;  // 片段之间的空行
        }
    }

    if (segments.Count == 0 || sources.Count == 0)
    {
        return null;
    }

    return new SourceMapWriter().Write(new SourceMapDocument(outputFileName, sources, segments));
}
```

**Source Map 结构**：
- **sources**：来源文件路径列表（相对于 assets/ 目录）
- **segments**：映射段（生成行 → 来源行）
- **sourceContent**：来源文件内容（内嵌在 source map 中）

### 3.6 附加 CSS 到 Chunks

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:580-632`

```csharp
private static async Task<IReadOnlyList<ChunkInfo>> AttachCssAssetsToChunksAsync(
    BuildContext context,
    IReadOnlyList<ChunkInfo> chunks,
    IReadOnlyList<AssetInfo> cssAssets,
    CancellationToken cancellationToken)
{
    if (chunks.Count == 0)
    {
        return chunks;
    }

    var entryChunk = chunks.FirstOrDefault(static chunk => chunk.IsEntry)
        ?? chunks.First();

    // 1. 收集直接 CSS（每个 chunk 的直接 CSS）
    var directCssByChunk = chunks.ToDictionary(
        static chunk => chunk.FilePath,
        static _ => new HashSet<string>(StringComparer.Ordinal),
        FilePathComparer);

    foreach (var cssAsset in cssAssets)
    {
        foreach (var ownerChunkFilePath in GetAssetOwnerChunkFilePaths(cssAsset, entryChunk.FilePath))
        {
            if (directCssByChunk.TryGetValue(ownerChunkFilePath, out var cssFilePaths))
            {
                cssFilePaths.Add(cssAsset.FilePath);
            }
        }
    }

    // 2. 读取动态导入关系
    var dynamicImportsByChunk = await ReadDynamicImportsByChunkAsync(context, chunks, cancellationToken);

    // 3. 构建 CSS 闭包（传递性）
    var cssClosureByChunk = BuildCssClosureByChunk(
        chunks,
        directCssByChunk.ToDictionary(
            static entry => entry.Key,
            static entry => (IReadOnlySet<string>)entry.Value,
            FilePathComparer),
        dynamicImportsByChunk);

    // 4. 更新 chunk 的 CSS 列表
    return chunks.Select(chunk => new ChunkInfo
    {
        FileName = chunk.FileName,
        FilePath = chunk.FilePath,
        Size = chunk.Size,
        IsEntry = chunk.IsEntry,
        IsDynamic = chunk.IsDynamic,
        Imports = chunk.Imports,
        Css = cssClosureByChunk.TryGetValue(chunk.FilePath, out var chunkCss)
            ? chunkCss
            : [],
        SourceMapPath = chunk.SourceMapPath
    })
    .ToArray();
}
```

**CSS 闭包计算**（`BuildCssClosureByChunk`）：
```csharp
IReadOnlyList<string> ResolveCssClosure(string chunkFilePath, HashSet<string> visitingChunkFilePaths)
{
    if (cssClosureByChunk.TryGetValue(chunkFilePath, out var cachedCssClosure))
    {
        return cachedCssClosure;
    }

    // 1. 添加直接 CSS
    var cssClosure = new HashSet<string>(StringComparer.Ordinal);
    if (directCssByChunk.TryGetValue(chunkFilePath, out var chunkDirectCss))
    {
        cssClosure.UnionWith(chunkDirectCss);
    }

    // 2. 遍历静态导入的 chunk（排除动态导入）
    var dynamicImports = dynamicImportsByChunk.TryGetValue(chunkFilePath, out var chunkDynamicImports)
        ? chunkDynamicImports
        : new HashSet<string>(FilePathComparer);

    foreach (var importedChunkFilePath in chunk.Imports)
    {
        if (dynamicImports.Contains(importedChunkFilePath))
        {
            continue;  // 跳过动态导入
        }

        cssClosure.UnionWith(ResolveCssClosure(importedChunkFilePath, visitingChunkFilePaths));
    }

    var resolvedCssClosure = cssClosure.OrderBy(static cssPath => cssPath, StringComparer.Ordinal).ToArray();
    cssClosureByChunk[chunkFilePath] = resolvedCssClosure;
    return resolvedCssClosure;
}
```

**为什么需要 CSS 闭包**：
- Chunk A 导入 Chunk B，Chunk A 应该加载 Chunk B 的 CSS
- 否则动态导入时样式会缺失

### 3.7 重写动态 Chunk CSS 导入

**位置**：`src/Jolt/Build/BuildOrchestrator.cs:634-693`

```csharp
private static async Task RewriteDynamicChunkCssImportsAsync(
    BuildContext context,
    IReadOnlyList<ChunkInfo> chunks,
    CancellationToken cancellationToken)
{
    if (chunks.Count == 0)
    {
        return;
    }

    var chunkCssByFilePath = chunks.ToDictionary(
        static chunk => chunk.FilePath,
        chunk => chunk.Css
            .Select(cssFilePath => ToHtmlPath(context, cssFilePath))
            .Distinct(StringComparer.Ordinal)
            .ToArray(),
        FilePathComparer);

    foreach (var chunk in chunks)
    {
        var chunkAbsolutePath = Path.Combine(
            context.RootDirectory,
            chunk.FilePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(chunkAbsolutePath))
        {
            continue;
        }

        var originalContent = await File.ReadAllTextAsync(chunkAbsolutePath, cancellationToken);
        var currentChunkDirectory = GetContainingDirectoryPath(chunkAbsolutePath);

        // 重写动态导入表达式
        var rewrittenContent = JavaScriptModuleSpecifierScanner.RewriteDynamicImportExpressions(
            originalContent,
            specifier =>
            {
                if (!TryGetBuiltChunkDynamicImportPath(specifier.Value, out var specifierPath))
                {
                    return null;
                }

                var targetAbsolutePath = Path.GetFullPath(Path.Combine(
                    currentChunkDirectory,
                    specifierPath.Replace('/', Path.DirectorySeparatorChar)));
                var targetChunkFilePath = Path.GetRelativePath(context.RootDirectory, targetAbsolutePath).Replace('\\', '/');

                if (!chunkCssByFilePath.TryGetValue(targetChunkFilePath, out var targetCssPaths) || targetCssPaths.Length == 0)
                {
                    return null;
                }

                var originalImportExpression = originalContent.Substring(specifier.ExpressionStart, specifier.ExpressionLength);
                return CreateDynamicChunkCssImportExpression(originalImportExpression, targetCssPaths);
            });

        if (string.Equals(originalContent, rewrittenContent, StringComparison.Ordinal))
        {
            continue;
        }

        await File.WriteAllTextAsync(chunkAbsolutePath, rewrittenContent, cancellationToken);
    }
}
```

**动态导入转换示例**：

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
    });
}),globalThis.__jazorImportCss(["/assets/about-styles.css"]).then(function(){
    return import('./about.js');
}));
```

**运行时行为**：
1. 定义全局 `__jazorImportCss` 函数（首次定义）
2. 调用 `__jazorImportCss(["/assets/about-styles.css"])` 加载 CSS
3. CSS 加载完成后，执行原始的动态导入

## 4. 线程安全模型

CSS 处理流程是单线程的：
- 所有操作顺序执行
- 异步操作（`File.ReadAllTextAsync`）用于 IO，不是并发

**无共享状态**：
- 每个 `CssFragment` 是独立的
- `SourceMapOwnershipContext` 在构建过程中不修改

## 5. 错误处理

### 5.1 Source Map 读取失败

**场景**：
- Source map 文件不存在
- JSON 格式错误
- Source 路径解析失败

**处理**：
- 返回空列表或 `null`
- 回退到启发式所有权推断

### 5.2 CSS 资产引用重写失败

**场景**：
- `url()` 中的路径不存在
- 相对路径解析失败

**处理**：
- `CssUrlRewriter.RewriteAssetReferences` 保留原始 `url()`
- 不中断构建

## 6. 配置选项

### 6.1 影响 CSS 的 BuildOptions

| 选项 | 影响 |
|------|------|
| `Minify` | 是否压缩 CSS |
| `SourceMap` | 是否生成 CSS source map |
| `AssetHashLength` | CSS 文件名哈希长度 |

### 6.2 CSS 输出目录

由 `AssetsDir` 配置项控制（默认 `"assets"`）：
- CSS 文件输出到 `dist/assets/`
- Source map 输出到 `dist/assets/*.css.map`

## 7. 与其他子系统的交互

### 7.1 与编译器的交互

- **读取**：`CompilationResult.StyleFragments`, `StyleContent`, `EmbeddedStyleDependencies`
- **用途**：提取 .jazor/.vue 文件中的内联样式

### 7.2 与 SourceMap 子系统的交互

- **读取**：Deno 生成的 source map（`chunk.SourceMapPath`）
- **生成**：提取的 CSS 的 source map（使用 `SourceMapWriter`）

### 7.3 与静态资产处理的交互

- **读取**：`StaticAssetHandler` 复制的资产列表
- **重写**：CSS 中的 `url()` 引用指向哈希后的资产路径

## 8. 设计权衡

### 8.1 为什么要提取 CSS？

**权衡**：
- **不提取**：CSS 内联在 JavaScript 中，加载简单但缓存不友好
- **提取**：CSS 独立加载，缓存友好，但需要管理加载顺序

**设计决策**：提取 CSS 的原因：
1. **浏览器并行加载**：CSS 和 JavaScript 可以并行下载
2. **缓存策略**：CSS 和 JS 可以独立缓存
3. **最佳实践**：现代前端框架的标准做法

### 8.2 为什么要计算 CSS 闭包？

**权衡**：
- **不计算闭包**：只附加直接 CSS，简单但动态导入样式缺失
- **计算闭包**：包含传递性 CSS，复杂但正确

**设计决策**：计算 CSS 闭包的原因：
- 静态导入的 chunk 应该加载其依赖的 CSS
- 动态导入的 chunk 通过 `__jazorImportCss` 按需加载 CSS

### 8.3 为什么要注入 `__jazorImportCss`？

**权衡**：
- **不注入**：动态导入的 chunk 不加载 CSS，需要手动管理
- **注入**：自动加载 CSS，但增加运行时复杂性

**设计决策**：注入 `__jazorImportCss` 的原因：
- 开发者无需手动为动态导入添加 CSS 加载逻辑
- 符合"约定优于配置"的设计理念

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
