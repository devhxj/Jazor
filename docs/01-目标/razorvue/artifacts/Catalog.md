# RazorVueCatalog - RazorVue 组件目录
> Status: 活跃参考

## 1. 文档定位

本文档描述 `RazorVueCatalog` 和 `RazorVueCatalogBuilder`，这是 RazorVue 编译产物的聚合和索引层。Catalog 在程序集级别聚合所有 `VueCompiledArtifact`，提供 DevServer、HMR 和 Build 阶段的统一查询接口。

**核心文件**：
- `src/Jazor.RazorVue/Artifacts/RazorVueCatalog.cs`
- `src/Jazor.RazorVue/Artifacts/RazorVueCatalogBuilder.cs`

## 2. 核心类型

### 2.1 RazorVueCatalog

程序集级别的编译产物容器，包含所有 RazorVue 组件的编译结果。

```csharp
public sealed record RazorVueCatalog(
    string AssemblyName,                         // 程序集名称（如 "MyApp.Components"）
    ImmutableArray<VueCompiledArtifact> Artifacts); // 该程序集中的所有组件编译产物
```

**字段说明**：
- **AssemblyName**：程序集的简单名称（不含版本、文化和公钥标记）
- **Artifacts**：不可变数组，包含该程序集内所有 RazorVue 组件的编译产物

**使用场景**：
- **DevServer**：从 Catalog 中提取所有组件模块，写入磁盘
- **HMR**：根据 `AssemblyName` 和 `ComponentId` 快速查找变更组件
- **Build**：聚合多个程序集的 Catalog，生成最终 bundle

### 2.2 RazorVueCatalogBuilder

Catalog 的构建器，负责路径规范化和排序优化。

```csharp
public sealed class RazorVueCatalogBuilder
{
    public RazorVueCatalog Build(
        string assemblyName,
        ImmutableArray<VueCompiledArtifact> artifacts)
    {
        // 实现见下文
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        // 实现见下文
    }
}
```

**核心职责**：
1. **路径规范化**：统一相对路径格式（反斜杠转正斜杠，移除前导 `./`）
2. **排序优化**：按路径和名称排序，加速 DevServer 查找
3. **安全验证**：拒绝路径逃逸和绝对路径

## 3. 核心算法

### 3.1 Catalog 构建流程

```
VueCompiledArtifact 集合
       ↓
路径规范化（NormalizeRelativePath）
       ↓
按路径 + 名称排序
       ↓
生成 RazorVueCatalog
```

### 3.2 路径规范化算法

`NormalizeRelativePath` 方法确保所有相对路径符合统一格式：

```csharp
private static string NormalizeRelativePath(string relativePath)
{
    if (string.IsNullOrWhiteSpace(relativePath))
        throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

    // 1. 反斜杠转正斜杠
    var normalized = relativePath.Replace('\\', '/').TrimStart('/');

    // 2. 移除前导 "./"
    while (normalized.StartsWith("./", StringComparison.Ordinal))
        normalized = normalized.Substring(2);

    // 3. 拒绝绝对路径
    if (Path.IsPathRooted(normalized))
        throw new InvalidOperationException(
            $"RazorVue artifact relative path must be relative: '{relativePath}'.");

    // 4. 拒绝路径逃逸（".."）
    var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
    if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
        throw new InvalidOperationException(
            $"RazorVue artifact relative path cannot escape output directory: '{relativePath}'.");

    // 5. 重新拼接（移除空段）
    return string.Join("/", segments);
}
```

**规范化示例**：

| 输入路径 | 规范化后 | 说明 |
|---------|---------|------|
| `components/MyComponent.js` | `components/MyComponent.js` | 无变化 |
| `components\\MyComponent.js` | `components/MyComponent.js` | 反斜杠转正斜杠 |
| `./components/MyComponent.js` | `components/MyComponent.js` | 移除前导 `./` |
| `/components/MyComponent.js` | `components/MyComponent.js` | 移除前导 `/` |
| `components/../MyComponent.js` | ❌ 抛出异常 | 路径逃逸 |
| `C:/components/MyComponent.js` | ❌ 抛出异常 | 绝对路径 |

### 3.3 排序优化算法

`Build` 方法对 artifacts 进行排序，优化 DevServer 查找性能：

```csharp
public RazorVueCatalog Build(string assemblyName, ImmutableArray<VueCompiledArtifact> artifacts)
{
    if (string.IsNullOrWhiteSpace(assemblyName))
        throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));

    var normalizedArtifacts = artifacts.IsDefault
        ? ImmutableArray<VueCompiledArtifact>.Empty
        : artifacts
            .Select(static artifact => artifact with
            {
                RelativeModulePath = NormalizeRelativePath(artifact.RelativeModulePath)
            })
            .OrderBy(static artifact => artifact.RelativeModulePath, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static artifact => artifact.ComponentName, StringComparer.Ordinal)
            .ToImmutableArray();

    return new RazorVueCatalog(assemblyName, normalizedArtifacts);
}
```

**排序规则**：
1. **主排序**：按 `RelativeModulePath`（不区分大小写）
2. **次排序**：按 `ComponentName`（区分大小写）

**性能优势**：
- DevServer 使用二分查找快速定位组件（O(log n)）
- HMR 批量更新时保持输出顺序一致（diff 优化）

### 3.4 Catalog 查询算法（DevServer 使用）

DevServer 使用 LINQ 进行高效查询：

```csharp
// 查询单个组件
var artifact = catalog.Artifacts.FirstOrDefault(
    a => a.Identity.ComponentId == componentId);

// 查询路径下的所有组件
var artifacts = catalog.Artifacts.Where(
    a => a.RelativeModulePath.StartsWith("components/", StringComparison.OrdinalIgnoreCase));
```

**优化建议**（未来）：
- 构建 `Dictionary<string, VueCompiledArtifact>` 索引（按 `ComponentId`）
- 构建 `SortedDictionary<string, ImmutableArray<VueCompiledArtifact>>` 前缀树（按路径）

## 4. 线程安全模型

### 4.1 构建阶段

`RazorVueCatalogBuilder.Build` 是纯函数，无共享状态，可并行调用。

### 4.2 读取阶段

`RazorVueCatalog` 是不可变 record 类型，天然线程安全：

- **DevServer**：多线程并发读取（无锁）
- **HMR**：替换整个 Catalog（原子操作）
- **Build**：聚合多个 Catalog（只读访问）

### 4.3 更新阶段

HMR 更新时采用 Copy-on-Write 策略：

```csharp
// 伪代码示例
var updatedArtifacts = catalog.Artifacts
    .Where(a => a.Identity.ComponentId != changedComponentId)
    .Append(newArtifact)
    .OrderBy(a => a.RelativeModulePath)
    .ThenBy(a => a.ComponentName)
    .ToImmutableArray();

var newCatalog = catalog with { Artifacts = updatedArtifacts };
```

## 5. 错误处理

### 5.1 程序集名称验证

```csharp
if (string.IsNullOrWhiteSpace(assemblyName))
    throw new ArgumentException("Assembly name cannot be empty.", nameof(assemblyName));
```

**错误场景**：
- Source Generator 传入空程序集名
- DevServer 加载损坏的 Catalog

### 5.2 路径验证失败

```csharp
// 空路径
throw new InvalidOperationException("RazorVue artifact relative path cannot be empty.");

// 绝对路径
throw new InvalidOperationException($"RazorVue artifact relative path must be relative: '{relativePath}'.");

// 路径逃逸
throw new InvalidOperationException($"RazorVue artifact relative path cannot escape output directory: '{relativePath}'.");
```

**错误场景**：
- Source Generator 生成错误的相对路径
- 用户自定义输出路径配置错误

### 5.3 Artifacts 为空

允许 `artifacts.IsDefault` 或 `artifacts.IsEmpty`，返回空 Catalog：

```csharp
var normalizedArtifacts = artifacts.IsDefault
    ? ImmutableArray<VueCompiledArtifact>.Empty
    : /* ... */;
```

**使用场景**：程序集中无 RazorVue 组件时，返回空 Catalog 而非抛出异常。

## 6. 配置选项

无直接配置选项。行为由 `RazorVueCatalogBuilder` 的规则固定：

- 路径规范化规则不可配置
- 排序规则不可配置
- 安全验证规则不可配置

**扩展方向**：未来可考虑通过 `RazorVueCatalogBuilderOptions` 配置排序规则（如按哈希排序）。

## 7. 与其他子系统的交互

### 7.1 与 Source Generator 的交互

Source Generator 生成阶段调用 `RazorVueCatalogBuilder.Build`：

```csharp
// Source Generator 伪代码
var artifacts = ImmutableArray.CreateBuilder<VueCompiledArtifact>();
foreach (var component in components)
{
    var artifact = GenerateArtifact(component);
    artifacts.Add(artifact);
}

var catalog = new RazorVueCatalogBuilder()
    .Build(assemblyName, artifacts.ToImmutable());

// 嵌入程序集
EmbedCatalog(catalog);
```

### 7.2 与 DevServer 的交互

DevServer 启动时从程序集加载 Catalog：

```csharp
// DevServer 伪代码
var catalog = LoadCatalogFromAssembly(assembly);
foreach (var artifact in catalog.Artifacts)
{
    var outputPath = Path.Combine(outputRoot, artifact.RelativeModulePath);
    File.WriteAllText(outputPath, artifact.ModuleCode);
}
```

### 7.3 与 HMR 的交互

HMR 阶段根据 `AssemblyName` 和 `ComponentId` 更新 Catalog：

```csharp
// HMR 伪代码
var oldArtifact = catalog.Artifacts
    .FirstOrDefault(a => a.Identity.ComponentId == componentId);

if (oldArtifact is null)
    throw new InvalidOperationException($"Component not found: {componentId}");

var newCatalog = new RazorVueCatalogBuilder()
    .Build(catalog.AssemblyName,
        catalog.Artifacts
            .Remove(oldArtifact)
            .Append(newArtifact)
            .ToImmutableArray());
```

### 7.4 与 Build 的交互

Build 阶段聚合多个程序集的 Catalog：

```csharp
// Build 伪代码
var allArtifacts = assemblies
    .SelectMany(asm => LoadCatalog(asm).Artifacts)
    .ToImmutableArray();

var mergedCatalog = new RazorVueCatalogBuilder()
    .Build("MergedApplication", allArtifacts);
```

## 8. 设计权衡

### 8.1 为什么使用不可变数组而非列表

**问题**：为什么使用 `ImmutableArray<VueCompiledArtifact>` 而非 `List<VueCompiledArtifact>`？

**答案**：
- **线程安全**：不可变数组天然支持并发读取（无需锁）
- **内存效率**：`ImmutableArray` 比不可变列表更紧凑（无额外开销）
- **语义清晰**：Catalog 是编译时快照，不应运行时修改

**权衡**：构建时需要 `ToImmutableArray()` 转换（一次性成本）。

### 8.2 为什么在 Builder 中排序而非外部排序

**问题**：为什么排序逻辑在 `RazorVueCatalogBuilder` 内部，而非调用方排序？

**答案**：
- **封装规则**：排序规则是 Catalog 的内在属性，不应暴露给外部
- **一致性保证**：所有 Catalog 都遵循相同排序规则（避免外部排序不一致）
- **优化机会**：未来可替换为更高效的排序算法（如并行排序）

### 8.3 为什么拒绝路径逃逸而非自动修复

**问题**：为什么 `components/../MyComponent.js` 抛出异常而非自动修复为 `MyComponent.js`？

**答案**：
- **安全优先**：路径逃逸可能是 Source Generator bug，不应静默修复
- **明确意图**：强制用户明确指定正确的相对路径
- **避免意外**：自动修复可能导致错误的文件覆盖

**用户指导**：Source Generator 应确保生成的相对路径始终在输出目录内。

### 8.4 为什么使用 Record 而非 Class

**问题**：为什么 `RazorVueCatalog` 是 record 而非 class？

**答案**：
- **值语义**：Catalog 是不可变快照，天然支持值比较
- **模式匹配**：方便解构和 `with` 表达式（HMR 更新场景）
- **简洁性**：record 自动生成 `Equals`/`GetHashCode`（基于所有字段）
