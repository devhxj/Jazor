# Workspace Resolver 子系统


## 1. 文档定位

Workspace Resolver 子系统是 Jolt 工作区管理的核心解析引擎，负责处理文档路径的规范化、候选路径展开、导入解析、Vue 组件发现和工作区文件枚举。本文档同时说明 `.slnx` 解决方案边界和 owning project 归属规则；更完整的 scoping 约定见 [SolutionScoping.md](SolutionScoping.md)。该子系统位于 `src/Jolt/Workspace/JoltWorkspaceResolver.cs`（约 1434 行），为 LSP 服务、DevServer 和编译器提供统一的工作区查询能力。

核心设计目标：
- 提供跨平台兼容的路径规范化
- 支持多种解析策略（nearby、tracked、workspace）
- 智能工作区边界检测
- 高效的文件枚举和缓存
- 灵活的工作区文件夹作用域

## 2. 核心类型

### 2.1 `JoltWorkspaceResolver` 静态类

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`

工作区解析的核心工具类，包含所有解析逻辑：

```csharp
internal static class JoltWorkspaceResolver
{
    // 工作区文件缓存
    private static readonly ConcurrentDictionary<string, string[]> WorkspaceFileCache;
    private static readonly object WorkspaceFileCacheSync = new();
    private static readonly Dictionary<string, long> WorkspaceFileCacheAges;

    // 作用域工作区文件夹根（AsyncLocal）
    private static readonly AsyncLocal<string[]?> WorkspaceFolderRoots = new();

    // 工作区边界标记
    private static readonly string[] WorkspaceBoundaryDirectories = [".git", ".hg", ".svn"];
    private static readonly string[] WorkspaceBoundaryFiles = [
        "jolt.config.json",
        "package.json",
        "global.json",
        "Directory.Build.props",
        "Directory.Build.targets"
    ];
    private static readonly string SolutionBoundaryFile = ".slnx";
}
```

**设计特点**：
- 静态类提供无状态解析方法
- 使用 `AsyncLocal` 支持异步作用域
- 缓存层优化重复查询
- 常量定义工作区边界规则

### 2.2 `WorkspaceVueComponentResolution` 记录

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 1430-1433）

Vue 组件解析结果的数据结构：

```csharp
internal readonly record struct WorkspaceVueComponentResolution(
    string ComponentName,    // 组件名称（如 "MyButton"）
    string AbsolutePath,     // 组件文件绝对路径
    string ImportPath);      // 相对导入路径（如 "./components/MyButton.vue"）
```

**使用场景**：
- Vue 组件自动导入
- 组件引用解析
- IntelliSense 补全

### 2.3 `JazorRelatedDocumentResolver` 类

**文件位置**：`src/Jolt/Workspace/JazorRelatedDocumentResolver.cs`

关联文档解析器，处理导入路径、组件引用和同目录资产：

```csharp
internal sealed class JazorRelatedDocumentResolver
{
    private readonly IJoltWorkspaceStore _workspaceStore;
    private readonly JazorVueParser _parser = new();

    public async Task<IReadOnlyList<DocumentSnapshot>> ResolveAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<string> explicitPaths,
        CancellationToken cancellationToken);
}
```

## 3. 核心算法

### 3.1 路径规范化算法

**方法**：`NormalizePath(string documentPath)`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 82-151）

**算法流程**：

```
输入：documentPath
    ↓
如果是空白路径 → 返回空字符串
    ↓
展开绝对路径（Path.GetFullPath）
    ↓
统一分隔符（\ → /）
    ↓
分离驱动器/根路径前缀（如 "C:” 或 "/"）
    ↓
处理路径段：
    - 跳过 "." 段
    - 处理 ".." 段（弹出栈）
    - 其他段压入栈
    ↓
检测路径段深度（MaxPathSegmentDepth = 256）
    ↓
重新组装路径
    ↓
返回规范化路径
```

**关键特性**：

1. **相对路径优化**：
   - `./` 前缀被移除
   - `..` 段正确处理嵌套
   - 路径段深度限制防止恶意路径

2. **跨平台兼容**：
   - Windows 驱动器前缀保留（`C:/`）
   - POSIX 根路径保留（`/`）
   - 统一使用 `/` 分隔符

3. **安全性**：
   - 路径段深度限制（256 段）
   - 防止路径遍历攻击
   - 异常路径抛出 `InvalidOperationException`

**示例**：

| 输入路径 | 规范化结果 |
|---------|----------|
| `C:\Users\..\Users\Test\file.js` | `C:/Users/Test/file.js` |
| `./src/../src/components/Button.vue` | `src/components/Button.vue` |
| `/var/www/../html/index.html` | `/var/html/index.html` |

### 3.2 文档类型映射

**方法**：`MapDocumentKind(string documentPath)`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 153-163）

**映射规则**：

```csharp
public static DocumentKind MapDocumentKind(string documentPath)
    => Path.GetExtension(documentPath).ToLowerInvariant() switch
    {
        ".jazor" => DocumentKind.Jazor,
        ".cs" => DocumentKind.CSharp,
        ".vue" => DocumentKind.Vue,
        ".js" => DocumentKind.JavaScript,
        ".ts" => DocumentKind.TypeScript,
        ".css" => DocumentKind.Css,
        _ => DocumentKind.Unknown
    };
```

**前端文档过滤**：

```csharp
public static DocumentKind? GetFrontendDocumentKind(string documentPath)
    => MapDocumentKind(documentPath) switch
    {
        DocumentKind.Vue => DocumentKind.Vue,
        DocumentKind.JavaScript => DocumentKind.JavaScript,
        DocumentKind.TypeScript => DocumentKind.TypeScript,
        DocumentKind.Css => DocumentKind.Css,
        _ => null
    };
```

### 3.3 路径候选展开

**方法**：`ExpandPathCandidates(string documentPath)`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 175-212）

**算法目的**：将模糊路径扩展为多个候选路径，支持无扩展名路径和分隔符变体。

**展开规则**：

```csharp
public static IEnumerable<string> ExpandPathCandidates(string documentPath)
{
    // 1. 原始路径
    yield return documentPath;

    // 2. 如果无扩展名，尝试常见扩展名
    if (string.IsNullOrWhiteSpace(Path.GetExtension(documentPath)))
    {
        yield return documentPath + ".vue";
        yield return documentPath + ".ts";
        yield return documentPath + ".js";
        yield return documentPath + ".css";
    }

    // 3. 分隔符规范化（\ → /）
    var slashNormalized = documentPath.Replace('\\', '/');
    if (!string.Equals(documentPath, slashNormalized, StringComparison.Ordinal))
    {
        yield return slashNormalized;
    }

    // 4. 绝对路径展开
    if (Path.IsPathRooted(documentPath))
    {
        var fullPath = Path.GetFullPath(documentPath);
        if (!string.Equals(documentPath, fullPath, StringComparison.OrdinalIgnoreCase))
        {
            yield return fullPath;
        }

        var fullSlashNormalized = fullPath.Replace('\\', '/');
        if (!string.Equals(fullPath, fullSlashNormalized, StringComparison.Ordinal))
        {
            yield return fullSlashNormalized;
        }
    }
}
```

**示例**：

| 输入路径 | 候选路径列表 |
|---------|-------------|
| `Button` | `Button`, `Button.vue`, `Button.ts`, `Button.js`, `Button.css` |
| `components\Button` | `components\Button`, `components\Button.vue`, `components/Button`, ... |
| `C:\Project\src` | `C:\Project\src`, `C:/Project/src`, `C:\Project\src.vue`, ... |

### 3.4 导入路径解析

**方法**：`GetImportPathCandidates(string jazorDocumentPath, string importSource)`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 286-318）

**算法目的**：解析 C# 中的导入语句，生成可能的文件路径候选。

**解析策略**：

```csharp
public static IEnumerable<string> GetImportPathCandidates(
    string jazorDocumentPath,
    string importSource)
{
    // 1. 过滤非前端导入
    if (!IsFrontendImport(importSource))
    {
        yield break;
    }

    // 2. 绝对路径导入
    if (Path.IsPathRooted(importSource))
    {
        foreach (var candidate in ExpandPathCandidates(importSource))
        {
            yield return candidate;
        }
        yield break;
    }

    // 3. 相对路径导入（相对于 Jazor 文档目录）
    var jazorDirectory = Path.GetDirectoryName(jazorDocumentPath);
    if (!string.IsNullOrWhiteSpace(jazorDirectory))
    {
        foreach (var candidate in ExpandPathCandidates(Path.Combine(jazorDirectory, importSource)))
        {
            yield return candidate;
        }
    }

    // 4. 项目根路径导入
    foreach (var candidate in ExpandPathCandidates(importSource))
    {
        yield return candidate;
    }
}
```

**前端导入检测**：

```csharp
private static bool IsFrontendImport(string importSource)
    => GetFrontendDocumentKind(importSource) is not null
        || importSource.StartsWith("./", StringComparison.Ordinal)
        || importSource.StartsWith("../", StringComparison.Ordinal)
        || importSource.StartsWith(".\\", StringComparison.Ordinal)
        || importSource.StartsWith("..\\", StringComparison.Ordinal);
```

**示例**：

| Jazor 文档路径 | 导入语句 | 候选路径 |
|--------------|---------|---------|
| `/src/Pages/Index.jazor` | `./Button.vue` | `/src/Pages/Button.vue`, `/src/Pages/Button` |
| `/src/Pages/Index.jazor` | `../components/Header` | `/src/components/Header.vue`, `/src/components/Header.ts`, ... |
| `/src/Pages/Index.jazor` | `/src/Shared/Layout` | `/src/Shared/Layout.vue`, `/src/Shared/Layout` |

### 3.5 Vue 组件解析策略

**四种解析策略**：

#### 3.5.1 Nearby Vue 组件解析

**方法**：`TryResolveNearbyVueComponent`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 364-393）

**搜索目录顺序**：

```csharp
public static IEnumerable<string> GetNearbyVueSearchDirectories(string documentDirectory)
{
    // 1. 当前目录
    yield return documentDirectory;

    // 2. 当前目录的 Components 子目录（大小写敏感）
    yield return Path.Combine(documentDirectory, "Components");
    yield return Path.Combine(documentDirectory, "components");

    // 3. 父目录
    var parentDirectory = GetParentDirectoryPath(documentDirectory);
    yield return parentDirectory;

    // 4. 父目录的 Components 子目录
    yield return Path.Combine(parentDirectory, "Components");
    yield return Path.Combine(parentDirectory, "components");
}
```

**使用场景**：
- 同目录组件（如 `Button.vue`）
- Components 子目录组件
- 父目录共享组件

#### 3.5.2 Tracked Nearby Vue 组件解析

**方法**：`TryResolveTrackedNearbyVueComponent`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 395-426）

**解析逻辑**：

```csharp
public static bool TryResolveTrackedNearbyVueComponent(
    string documentPath,
    string componentName,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    out WorkspaceVueComponentResolution resolvedComponent)
{
    var documentDirectory = Path.GetDirectoryName(documentPath);
    if (!string.IsNullOrWhiteSpace(documentDirectory))
    {
        // 遍历 nearby 搜索目录
        foreach (var candidate in GetNearbyVueSearchDirectories(documentDirectory))
        {
            var expectedPath = NormalizePath(Path.Combine(candidate, componentName + ".vue"));

            // 在打开的文档中查找
            var tracked = openDocuments.FirstOrDefault(openDocument =>
                openDocument.DocumentKind == DocumentKind.Vue
                && string.Equals(
                    NormalizePath(openDocument.DocumentPath),
                    expectedPath,
                    StringComparison.OrdinalIgnoreCase));

            if (tracked is not null)
            {
                resolvedComponent = new WorkspaceVueComponentResolution(
                    componentName,
                    NormalizePath(tracked.DocumentPath),
                    ToImportPath(documentDirectory, tracked.DocumentPath));
                return true;
            }
        }
    }

    resolvedComponent = default;
    return false;
}
```

**优先级**：高于文件系统解析（用于未保存的文档）

#### 3.5.3 Tracked Vue 组件解析

**方法**：`TryResolveTrackedVueComponent`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 428-455）

**解析逻辑**：

```csharp
public static bool TryResolveTrackedVueComponent(
    string documentPath,
    string componentName,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    out WorkspaceVueComponentResolution resolvedComponent)
{
    var documentDirectory = Path.GetDirectoryName(documentPath);
    var scopedWorkspaceRoot = TryGetScopedWorkspaceRootForDocument(documentPath);

    // 在打开的文档中查找（考虑作用域工作区根）
    var tracked = openDocuments.FirstOrDefault(openDocument =>
        openDocument.DocumentKind == DocumentKind.Vue
        && (scopedWorkspaceRoot is null
            || IsPathWithinWorkspaceRoot(openDocument.DocumentPath, scopedWorkspaceRoot))
        && string.Equals(
            Path.GetFileNameWithoutExtension(openDocument.DocumentPath),
            componentName,
            StringComparison.Ordinal));

    if (tracked is not null && !string.IsNullOrWhiteSpace(documentDirectory))
    {
        resolvedComponent = new WorkspaceVueComponentResolution(
            componentName,
            NormalizePath(tracked.DocumentPath),
            ToImportPath(documentDirectory, tracked.DocumentPath));
        return true;
    }

    resolvedComponent = default;
    return false;
}
```

**特点**：
- 支持作用域工作区根限制
- 按文件名匹配（不限制目录）
- 用于全局组件查找

#### 3.5.4 Workspace Vue 组件解析

**方法**：`ResolveWorkspaceVueComponent`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 525-549）

**解析逻辑**：

```csharp
public static WorkspaceVueComponentResolution? ResolveWorkspaceVueComponent(
    string documentPath,
    string componentName,
    IReadOnlyList<DocumentSnapshot> openDocuments,
    CancellationToken cancellationToken)
{
    var documentDirectory = Path.GetDirectoryName(documentPath);
    if (string.IsNullOrWhiteSpace(documentDirectory))
    {
        return null;
    }

    // 枚举工作区文件
    foreach (var filePath in EnumerateWorkspaceFiles(
                 GetWorkspaceSearchRoots(documentPath, secondaryDocumentPath: null, openDocuments),
                 componentName + ".vue",
                 cancellationToken))
    {
        return new WorkspaceVueComponentResolution(
            componentName,
            filePath,
            ToImportPath(documentDirectory, filePath));
    }

    return null;
}
```

**特点**：
- 扫描整个工作区
- 使用缓存优化性能
- 最后的解析策略

**解析优先级**（从高到低）：

1. `TryResolveTrackedNearbyVueComponent`（打开的 nearby 组件）
2. `TryResolveNearbyVueComponent`（文件系统 nearby 组件）
3. `TryResolveTrackedVueComponent`（打开的全局组件）
4. `ResolveWorkspaceVueComponent`（文件系统全局组件）

### 3.6 工作区搜索根计算

**方法**：`GetWorkspaceSearchRoots`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 587-608）

**算法目的**：确定工作区搜索的起始目录，避免过度广泛的文件扫描。

**算法流程**：

```
收集搜索目录（文档路径、打开文档目录）
    ↓
是否有作用域工作区文件夹根？
    ↓ 是
返回限定在文件夹根内的搜索根
    ↓ 否
返回默认搜索根（向上枚举祖先目录）
```

**默认搜索根算法**：

```csharp
private static IReadOnlyList<string> GetDefaultWorkspaceSearchRoots(IReadOnlyList<string> directories)
{
    // 单目录：枚举所有祖先
    if (directories.Count == 1)
    {
        foreach (var ancestor in EnumerateSearchAncestors(directories[0]))
        {
            yield return ancestor;
        }
        yield break;
    }

    // 多目录：查找公共祖先
    if (TryGetCommonSearchAncestor(directories) is { } commonAncestor)
    {
        yield return commonAncestor;
        yield break;
    }

    // 多目录无公共祖先：枚举所有目录的所有祖先
    var emitted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var directory in directories)
    {
        foreach (var ancestor in EnumerateSearchAncestors(directory))
        {
            if (emitted.Add(ancestor))
            {
                yield return ancestor;
            }
        }
    }
}
```

**祖先枚举算法**：

```csharp
private static IEnumerable<string> EnumerateSearchAncestors(
    string directory,
    string? stopAtDirectory = null)
{
    var current = Path.GetFullPath(directory);
    var normalizedStopAt = string.IsNullOrWhiteSpace(stopAtDirectory)
        ? null
        : NormalizeComparablePath(Path.GetFullPath(stopAtDirectory));
    var emittedStopDirectory = false;

    while (!string.IsNullOrWhiteSpace(current))
    {
        var normalizedCurrent = NormalizeComparablePath(current);

        // 检查停止条件
        if (normalizedStopAt is not null
            && !PathMatchesOrContains(normalizedCurrent, normalizedStopAt))
        {
            break;
        }

        // 到达驱动器根
        if (string.Equals(current, Path.GetPathRoot(current), StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        yield return current;

        // 到达停止目录
        if (normalizedStopAt is not null
            && string.Equals(normalizedCurrent, normalizedStopAt, StringComparison.OrdinalIgnoreCase))
        {
            emittedStopDirectory = true;
            yield break;
        }

        // 检测工作区边界标记
        if (normalizedStopAt is null && ContainsWorkspaceBoundaryMarker(current))
        {
            yield break;
        }

        // 避免过度广泛的临时目录
        if (normalizedStopAt is null && IsTooBroadTempAncestor(current))
        {
            yield break;
        }

        // 移动到父目录
        var parent = Directory.GetParent(current)?.FullName;
        if (string.IsNullOrWhiteSpace(parent)
            || string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        current = parent;
    }

    // 如果停止目录未发出，确保发出
    if (normalizedStopAt is not null && !emittedStopDirectory)
    {
        yield return Path.GetFullPath(stopAtDirectory!);
    }
}
```

**工作区边界检测**：

```csharp
private static bool ContainsWorkspaceBoundaryMarker(string directoryPath)
{
    // 1. 检查版本控制目录
    foreach (var markerDirectory in WorkspaceBoundaryDirectories)
    {
        if (Directory.Exists(Path.Combine(directoryPath, markerDirectory)))
        {
            return true;
        }
    }

    // 2. 检查项目配置文件
    foreach (var markerFile in WorkspaceBoundaryFiles)
    {
        if (File.Exists(Path.Combine(directoryPath, markerFile)))
        {
            return true;
        }
    }

    // 3. 检查解决方案文件（仅 .slnx）
    if (File.Exists(Path.Combine(directoryPath, SolutionBoundaryFile)))
    {
        return true;
    }

    return false;
}
```

**边界标记列表**：

| 类型 | 标记 |
|------|------|
| 版本控制目录 | `.git`, `.hg`, `.svn` |
| 项目配置文件 | `jolt.config.json`, `package.json`, `global.json`, `Directory.Build.props`, `Directory.Build.targets` |
| 解决方案文件 | `.slnx` |

**临时目录保护**：

```csharp
private static bool IsTooBroadTempAncestor(string directoryPath)
{
    var normalizedSystemTemp = NormalizePath(Path.GetTempPath()).TrimEnd('/', '\\');
    var normalizedDirectory = NormalizePath(directoryPath).TrimEnd('/', '\\');

    if (!normalizedDirectory.StartsWith(normalizedSystemTemp + "/", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    var relativePath = normalizedDirectory[(normalizedSystemTemp.Length + 1)..];
    var segmentCount = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

    // 仅允许临时目录的直接子目录
    return segmentCount <= 1;
}
```

### 3.7 工作区文件枚举与缓存

**方法**：`EnumerateWorkspaceFiles`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 768-811）

**算法流程**：

```
遍历搜索根
    ↓
规范化根路径
    ↓
跳过系统临时目录
    ↓
检查缓存（键：搜索根|搜索模式）
    ↓ 命中
返回缓存的文件列表
    ↓ 未命中
扫描工作区文件（ScanWorkspaceFiles）
    ↓
更新缓存
    ↓
遍历文件列表
    ↓
检查文件是否仍然存在（SafeFileExists）
    ↓ 如果不存在
使缓存失效，重新扫描
    ↓
去重并返回文件路径
```

**缓存键设计**：

```csharp
private static string CreateCacheKey(string searchRoot, string searchPattern)
    => NormalizePath(searchRoot) + "|" + searchPattern;
```

**缓存策略**：

```csharp
private const int MaxWorkspaceCacheEntries = 1000;

private static void SetWorkspaceCacheEntry(string cacheKey, string[] files)
{
    WorkspaceFileCache[cacheKey] = files;
    TouchWorkspaceCacheEntry(cacheKey);

    // LRU 淘汰策略
    string[] keysToTrim;
    lock (WorkspaceFileCacheSync)
    {
        if (WorkspaceFileCacheAges.Count <= MaxWorkspaceCacheEntries)
        {
            return;
        }

        // 移除最旧的条目
        keysToTrim = WorkspaceFileCacheAges
            .OrderBy(static pair => pair.Value)
            .Take(WorkspaceFileCacheAges.Count - MaxWorkspaceCacheEntries)
            .Select(static pair => pair.Key)
            .ToArray();

        foreach (var key in keysToTrim)
        {
            WorkspaceFileCacheAges.Remove(key);
        }
    }

    foreach (var key in keysToTrim)
    {
        WorkspaceFileCache.TryRemove(key, out _);
    }
}
```

**缓存更新**：

```csharp
private static void TouchWorkspaceCacheEntry(string cacheKey)
{
    lock (WorkspaceFileCacheSync)
    {
        WorkspaceFileCacheAges[cacheKey] = Environment.TickCount64;
    }
}
```

**缓存失效**：

```csharp
public static void InvalidatePath(string? documentPath)
{
    if (string.IsNullOrWhiteSpace(documentPath))
    {
        return;
    }

    var normalizedPath = NormalizePath(documentPath);
    var normalizedDirectory = NormalizePath(Path.GetDirectoryName(normalizedPath) ?? normalizedPath);

    // 移除所有相关的缓存条目
    foreach (var cacheKey in WorkspaceFileCache.Keys)
    {
        if (!TryParseCacheKey(cacheKey, out var normalizedRoot))
        {
            continue;
        }

        if (PathMatchesOrContains(normalizedRoot, normalizedPath)
            || PathMatchesOrContains(normalizedRoot, normalizedDirectory)
            || PathMatchesOrContains(normalizedPath, normalizedRoot)
            || PathMatchesOrContains(normalizedDirectory, normalizedRoot))
        {
            RemoveWorkspaceCacheEntry(cacheKey);
        }
    }
}
```

**文件扫描算法**：

```csharp
private static string[] ScanWorkspaceFiles(
    string searchRoot,
    string searchPattern,
    CancellationToken cancellationToken)
{
    if (!Directory.Exists(searchRoot))
    {
        return Array.Empty<string>();
    }

    var visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var results = new List<string>();
    var pendingDirectories = new Stack<string>();
    pendingDirectories.Push(searchRoot);

    while (pendingDirectories.Count > 0)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentDirectory = pendingDirectories.Pop();
        var normalizedDirectory = NormalizePath(currentDirectory);

        // 跳过已访问或应跳过的目录
        if (!visitedDirectories.Add(normalizedDirectory)
            || ShouldSkipWorkspaceDirectory(currentDirectory))
        {
            continue;
        }

        // 枚举文件
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(currentDirectory, searchPattern, SearchOption.TopDirectoryOnly);
        }
        catch (IOException) { continue; }
        catch (UnauthorizedAccessException) { continue; }

        foreach (var filePath in SafeEnumerate(files))
        {
            var normalizedPath = NormalizePath(filePath);
            if (visitedFiles.Add(normalizedPath))
            {
                results.Add(normalizedPath);
            }
        }

        // 枚举子目录
        IEnumerable<string> directories;
        try
        {
            directories = Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly);
        }
        catch (IOException) { continue; }
        catch (UnauthorizedAccessException) { continue; }

        foreach (var childDirectory in SafeEnumerate(directories))
        {
            if (!ShouldSkipWorkspaceDirectory(childDirectory))
            {
                pendingDirectories.Push(childDirectory);
            }
        }
    }

    return results.ToArray();
}
```

**目录跳过列表**：

```csharp
private static bool ShouldSkipWorkspaceDirectory(string directoryPath)
{
    var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    return directoryName switch
    {
        ".git" => true,
        ".hg" => true,
        ".svn" => true,
        ".vs" => true,
        ".idea" => true,
        "bin" => true,
        "obj" => true,
        "node_modules" => true,
        ".deno" => true,
        _ => false
    };
}
```

### 3.8 Scoped 工作区文件夹

**方法**：`PushWorkspaceFolderRoots`

**文件位置**：`src/Jolt/Workspace/JoltWorkspaceResolver.cs`（行 39-53）

**设计目的**：支持 LSP 多根工作区（multi-root workspace），限制组件解析在特定文件夹根内。

**实现机制**：

```csharp
public static IDisposable PushWorkspaceFolderRoots(IEnumerable<string> workspaceFolderRoots)
{
    ArgumentNullException.ThrowIfNull(workspaceFolderRoots);

    var previous = WorkspaceFolderRoots.Value;
    var normalizedRoots = workspaceFolderRoots
        .Where(static root => !string.IsNullOrWhiteSpace(root))
        .Select(root => Path.GetFullPath(root))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    WorkspaceFolderRoots.Value = normalizedRoots.Length == 0
        ? null
        : normalizedRoots;

    return new WorkspaceFolderRootScope(previous);
}

private sealed class WorkspaceFolderRootScope(string[]? previousRoots) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        WorkspaceFolderRoots.Value = previousRoots;
        _disposed = true;
    }
}
```

**使用示例**：

```csharp
// 设置作用域工作区根
using (JoltWorkspaceResolver.PushWorkspaceFolderRoots(new[] { "/path/to/frontend", "/path/to/backend" }))
{
    // 在此作用域内，组件解析仅限于这些文件夹
    var component = await ResolveVueComponentAsync(documentPath, "Button");
}

// 退出作用域后，自动恢复到之前的配置
```

**作用域限制应用**：

```csharp
private static string? TryGetScopedWorkspaceRootForDocument(string documentPath)
{
    var scopedRoots = GetScopedWorkspaceFolderRoots();
    if (scopedRoots.Count == 0 || !Path.IsPathRooted(documentPath))
    {
        return null;
    }

    var documentDirectory = Path.GetDirectoryName(documentPath);
    if (string.IsNullOrWhiteSpace(documentDirectory))
    {
        return null;
    }

    return FindContainingWorkspaceFolderRoot(documentDirectory, scopedRoots);
}
```

### 3.9 关联文档解析

**类**：`JazorRelatedDocumentResolver`

**文件位置**：`src/Jolt/Workspace/JazorRelatedDocumentResolver.cs`

**解析目标**：解析 Jazor 文档的所有关联文档（导入、组件引用、同目录资产）。

**解析流程**：

```
解析 Jazor 文档（JazorVueParser）
    ↓
收集候选路径：
    1. 显式路径（explicitPaths）
    2. 导入路径（parsed.Imports）
    3. Vue 组件引用（正则匹配 <Component> 标签）
    4. 同目录资产（.css, .js, .ts）
    ↓
遍历候选路径
    ↓
解析文档（ResolveDocumentAsync）
    ↓
过滤支持的文档类型
    ↓
去重并返回
```

**Vue 组件引用提取**：

```csharp
private static string[] GetReferencedVueComponents(string text)
    => JazorMarkupPatterns.ComponentTagPattern.Matches(text)
        .Select(static match => match.Groups["name"].Value)
        .Where(static value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.Ordinal)
        .ToArray();
```

**支持的文档类型**：

```csharp
private static bool IsSupportedFrontendDocument(DocumentSnapshot document)
    => document.DocumentKind is DocumentKind.Vue
        or DocumentKind.JavaScript
        or DocumentKind.TypeScript
        or DocumentKind.Css;
```

**同目录资产路径**：

```csharp
public static IEnumerable<string> GetCoLocatedAssetPaths(string documentPath)
{
    var documentDirectory = Path.GetDirectoryName(documentPath);
    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(documentPath);
    if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
    {
        yield break;
    }

    foreach (var extension in new[] { ".css", ".js", ".ts" })
    {
        yield return Path.Combine(documentDirectory, fileNameWithoutExtension + extension);
    }
}
```

**Code-Behind 文件解析**：

```csharp
public static IEnumerable<string> GetCoLocatedCodeBehindPaths(string jazorDocumentPath)
{
    var documentDirectory = Path.GetDirectoryName(jazorDocumentPath);
    var fileName = Path.GetFileName(jazorDocumentPath);
    var componentName = Path.GetFileNameWithoutExtension(jazorDocumentPath);
    if (string.IsNullOrWhiteSpace(documentDirectory)
        || string.IsNullOrWhiteSpace(fileName)
        || string.IsNullOrWhiteSpace(componentName))
    {
        yield break;
    }

    // 1. DocumentName.jazor.cs
    yield return Path.Combine(documentDirectory, fileName + ".cs");

    // 2. ComponentName.cs
    yield return Path.Combine(documentDirectory, componentName + ".cs");
}

public static bool TryResolveOwningJazorPath(string codeBehindPath, out string jazorDocumentPath)
{
    jazorDocumentPath = string.Empty;
    if (string.IsNullOrWhiteSpace(codeBehindPath)
        || !codeBehindPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    var fullCodeBehindPath = Path.IsPathRooted(codeBehindPath)
        ? Path.GetFullPath(codeBehindPath)
        : codeBehindPath;

    // 1. ComponentName.jazor.cs → ComponentName.jazor
    if (fullCodeBehindPath.EndsWith(".jazor.cs", StringComparison.OrdinalIgnoreCase))
    {
        var candidate = fullCodeBehindPath[..^3];
        if (File.Exists(candidate))
        {
            jazorDocumentPath = Path.GetFullPath(candidate);
            return true;
        }
        return false;
    }

    // 2. ComponentName.cs → ComponentName.jazor
    var documentDirectory = Path.GetDirectoryName(fullCodeBehindPath);
    var componentName = Path.GetFileNameWithoutExtension(fullCodeBehindPath);
    if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(componentName))
    {
        return false;
    }

    var coLocatedJazorPath = Path.Combine(documentDirectory, componentName + ".jazor");
    if (!File.Exists(coLocatedJazorPath))
    {
        return false;
    }

    jazorDocumentPath = Path.GetFullPath(coLocatedJazorPath);
    return true;
}
```

## 4. 解决方案作用域与 owning project

### 4.1 解决方案发现

Jolt 在做项目级发现时，先向上查找 `.slnx`。找到以后，当前目录树才进入解决方案作用域。

如果向上查找后仍然找不到 `.slnx`，项目级发现必须停止，不得继续退回到 `*.csproj`、`*.sln` 或任意磁盘目录推断。

当前实现的用户错误为：

```text
No solution .slnx was found for '<documentPath>'. Open the project from a solution directory that contains a .slnx file.
```

### 4.2 项目归属

Owning project 由 `.slnx` 中的 project entries 决定。

这意味着：

- 文档不属于“最近的文件夹”
- 文档不属于“最近的 project 文件”
- 文档只属于解决方案图中实际声明它的项目

如果一个文件在多个项目中都可见，隐式路径仍然只绑定到当前文档的 owning project。

### 4.3 隐式发现边界

所有隐式发现都必须先拿到 owning project，再只在该项目的 document graph 内展开：

- import / component discovery
- related document discovery
- open document scan
- workspace symbol 的 project-local 解析

跨项目文件可以被显式引用，但不能被隐式发现逻辑自动跨过去。

### 4.4 HMR 和诊断刷新边界

当文件变化时，Jolt 只刷新 owning project 的受影响集合：

- HMR 只向 owning project 的依赖图传播
- 诊断刷新只重算 owning project 的相关文档
- sibling project 的诊断和更新保持不变，除非它们自己的文件也发生变化

这条规则的目的不是限制一个 Jolt 实例的能力，而是避免把局部变更误扩散成工作区级广播。

## 5. 线程安全模型

### 5.1 全局状态线程安全

**工作区文件缓存**：

```csharp
private static readonly ConcurrentDictionary<string, string[]> WorkspaceFileCache =
    new(StringComparer.OrdinalIgnoreCase);
```

- 使用 `ConcurrentDictionary` 确保线程安全
- 读操作无锁，写操作使用细粒度锁

**缓存年龄字典**：

```csharp
private static readonly object WorkspaceFileCacheSync = new();
private static readonly Dictionary<string, long> WorkspaceFileCacheAges =
    new(StringComparer.OrdinalIgnoreCase);
```

- 使用 `lock` 语句保护访问
- 与 `ConcurrentDictionary` 配合使用

### 5.2 AsyncLocal 状态

**工作区文件夹根**：

```csharp
private static readonly AsyncLocal<string[]?> WorkspaceFolderRoots = new();
```

- 使用 `AsyncLocal` 实现异步流控制
- 每个异步上下文有独立的值
- 自动清理，无需手动释放

### 5.3 无状态方法

**静态工具方法**：

- 所有解析方法都是静态的
- 不依赖实例状态
- 可以安全地并发调用

## 6. 错误处理

### 6.1 参数验证

**空值检查**：

```csharp
ArgumentNullException.ThrowIfNull(documentPath);
ArgumentNullException.ThrowIfNull(workspaceFolderRoots);
```

**取消检查**：

```csharp
cancellationToken.ThrowIfCancellationRequested();
```

### 6.2 文件系统异常处理

**安全枚举**：

```csharp
private static IEnumerable<string> SafeEnumerate(IEnumerable<string> values)
{
    IEnumerator<string>? enumerator = null;
    try
    {
        enumerator = values.GetEnumerator();
    }
    catch (DirectoryNotFoundException) { yield break; }
    catch (IOException) { yield break; }
    catch (UnauthorizedAccessException) { yield break; }

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
            catch (DirectoryNotFoundException) { yield break; }
            catch (IOException) { yield break; }
            catch (UnauthorizedAccessException) { yield break; }

            yield return current;
        }
    }
}
```

**安全文件存在检查**：

```csharp
private static bool SafeFileExists(string filePath)
{
    try
    {
        return File.Exists(filePath);
    }
    catch (IOException) { return false; }
    catch (UnauthorizedAccessException) { return false; }
}
```

### 6.3 文档解析失败报告

**写入警告到 stderr**：

```csharp
private static void WriteDocumentResolutionWarning(string documentPath, Exception exception)
{
    try
    {
        Console.Error.WriteLine(JsonSerializer.Serialize(new
        {
            eventType = "workspaceDocumentResolveFailed",
            documentPath,
            errorType = exception.GetType().FullName ?? exception.GetType().Name,
            message = exception.Message,
            timestamp = DateTimeOffset.UtcNow
        }));
    }
    catch (Exception)
    {
        // Resolution failure reporting must not change workspace lookup behavior.
    }
}
```

### 6.4 路径规范化异常

**路径段深度限制**：

```csharp
if (segments.Count > MaxPathSegmentDepth)
{
    throw new InvalidOperationException(
        $"Path normalization exceeded the safety limit of {MaxPathSegmentDepth} segments for '{documentPath}'.");
}
```

## 7. 配置选项

### 7.1 缓存大小限制

```csharp
private const int MaxWorkspaceCacheEntries = 1000;
```

**影响**：
- 最多缓存 1000 个搜索结果
- 超过限制时，使用 LRU 策略淘汰旧条目

### 7.2 路径段深度限制

```csharp
private const int MaxPathSegmentDepth = 256;
```

**影响**：
- 防止恶意路径导致栈溢出
- 超过限制时抛出 `InvalidOperationException`

### 7.3 工作区边界标记

**可配置的标记**：

- 版本控制目录：`.git`, `.hg`, `.svn`
- 项目配置文件：`jolt.config.json`, `package.json`, `global.json`, `Directory.Build.props`, `Directory.Build.targets`
- 解决方案文件：`.slnx`

**影响**：
- 停止向上枚举祖先目录
- 限制工作区搜索范围

### 7.4 目录跳过列表

```csharp
private static bool ShouldSkipWorkspaceDirectory(string directoryPath)
{
    var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    return directoryName switch
    {
        ".git" => true,
        ".hg" => true,
        ".svn" => true,
        ".vs" => true,
        ".idea" => true,
        "bin" => true,
        "obj" => true,
        "node_modules" => true,
        ".deno" => true,
        _ => false
    };
}
```

**影响**：
- 跳过不需要搜索的目录
- 提高文件枚举性能
- 减少缓存压力

## 8. 与其他子系统的交互

### 8.1 LSP 服务交互

**LspSession** 使用工作区解析器：

```
LspSession.Initialize
    ↓
PushWorkspaceFolderRoots(workspaceFolders)
    ↓
保存作用域

LspSession.Completion
    ↓
ResolveWorkspaceVueComponent
    ↓
返回组件候选项

LspSession.DidOpen/DidChange/DidClose
    ↓
InvalidatePath(documentPath)
    ↓
清除相关缓存
```

### 8.2 DevServer 交互

**OnDemandCompiler** 使用工作区解析器：

```
OnDemandCompiler.CompileRequest
    ↓
ResolveDocumentAsync
    ↓
获取文档和关联文档
    ↓
执行编译
```

### 8.3 编译器交互

**BuildOrchestrator** 使用工作区解析器：

```
BuildOrchestrator.IncrementalBuild
    ↓
EnumerateWorkspaceFiles
    ↓
获取所有相关源文件
    ↓
检测变更并触发编译
```

## 9. 设计权衡

### 9.1 缓存 vs 实时性

**当前选择**：LRU 缓存，路径失效时清除

**优点**：
- 显著提高重复查询性能
- 减少文件系统 I/O
- 合理的内存使用

**缺点**：
- 缓存失效逻辑复杂
- 可能返回过时结果（文件已删除但缓存未失效）

**适用场景**：
- 文件系统变化相对不频繁
- 重复查询相同路径
- 内存充足的环境

### 9.2 多策略组件解析

**当前选择**：4 种解析策略，按优先级依次尝试

**优点**：
- 支持未保存的文档（tracked 策略）
- 性能优化（nearby 优先）
- 灵活的解析范围

**缺点**：
- 多次尝试可能影响性能
- 逻辑复杂度较高

**适用场景**：
- 开发时体验优先（支持未保存文档）
- 大型工作区（nearby 优先避免全局扫描）
- 多种组件组织方式

### 9.3 工作区边界检测

**当前选择**：基于标记文件的启发式检测

**优点**：
- 自动适应项目结构
- 避免过度广泛的搜索
- 支持多种项目类型

**缺点**：
- 依赖约定的项目结构
- 可能误判边界

**适用场景**：
- 遵循标准项目结构的项目
- 需要自动配置的环境
- 不想手动配置工作区根

### 9.4 AsyncLocal 作用域

**当前选择**：使用 `AsyncLocal` 存储作用域工作区根

**优点**：
- 自动传播到异步调用
- 无需手动传递参数
- 支持嵌套作用域

**缺点**：
- 隐式依赖，调试困难
- 可能意外传播到不需要的调用

**适用场景**：
- 异步调用链较长
- 需要上下文传播
- 避免参数污染

### 9.5 静态类设计

**当前选择**：所有方法都是静态的

**优点**：
- 简单的 API
- 无实例化开销
- 易于调用

**缺点**：
- 难以 mock 测试
- 全局状态管理复杂
- 无法依赖注入

**适用场景**：
- 工具类性质的方法
- 无状态或共享状态
- 不需要替代实现的场景

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**参考源文件**：
- `src/Jolt/Workspace/JoltWorkspaceResolver.cs`
- `src/Jolt/Workspace/JazorRelatedDocumentResolver.cs`
