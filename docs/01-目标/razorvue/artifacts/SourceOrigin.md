# RazorVueSourceOrigin - RazorVue 源码映射

## 1. 文档定位

`RazorVueSourceOrigin` 及相关枚举类型，这是 RazorVue 编译产物的源码映射信息。SourceOrigin 连接 C# 源码和生成的 JavaScript 代码，支持 SourceMap 生成、诊断信息定位和调试器映射。

**核心文件**：
- `src/Jazor.RazorVue/Artifacts/RazorVueSourceOrigin.cs`

## 2. 核心类型

### 2.1 RazorVueSourceOrigin Record

源码位置映射记录，包含源码位置、生成位置和映射质量信息。

```csharp
public sealed record RazorVueSourceOrigin(
    RazorVueOriginKind OriginKind,       // 映射种类（Component/Template/Logic 等）
    string SourceFilePath,                // 源码文件路径
    int SourceSpanStart,                  // 源码起始位置（字节偏移）
    int SourceSpanLength,                 // 源码长度（字节数）
    int StartLine,                        // 源码起始行（1-based）
    int StartColumn,                      // 源码起始列（1-based）
    string? GeneratedFilePath,            // 生成文件路径（可为 null）
    int? GeneratedSpanStart,              // 生成起始位置（可为 null）
    int? GeneratedSpanLength,             // 生成长度（可为 null）
    RazorVueMappingQuality MappingQuality,// 映射质量（ExactSource/GeneratedOnly 等）
    RazorVueOriginProvenance Provenance); // 来源证明（RazorSourceMap/GeneratedSyntaxLocation 等）
```

**字段说明**：
- **OriginKind**：标识映射的语义类型（Component/Template/Logic 等）
- **SourceFilePath**：C# 源码文件的绝对路径或相对路径
- **SourceSpanStart/Length**：源码在文件中的字节偏移和长度
- **StartLine/Column**：源码的行列位置（1-based，用于诊断显示）
- **GeneratedFilePath**：生成的 JavaScript 文件路径（通常与 SourceFilePath 相同）
- **GeneratedSpanStart/Length**：生成代码在文件中的位置（可为 null）
- **MappingQuality**：映射质量标识（精确源码映射 vs 生成代码映射）
- **Provenance**：映射来源证明（Razor 源码映射 vs 生成语法位置）

### 2.2 RazorVueOriginKind 枚举

映射的语义类型，标识代码段的用途。

```csharp
public enum RazorVueOriginKind
{
    Component,        // 组件类定义（class MyComponent : ComponentBase）
    Descriptor,       // 组件描述符（props/emits/slots 定义）
    Template,         // Razor 模板（RenderTreeBuilder 构建逻辑）
    Logic,            // setup() 逻辑（生命周期钩子、computed、watch）
    GeneratedRender   // 生成的渲染函数（Vue render 函数）
}
```

**使用场景**：
- **SourceMap 生成**：不同 OriginKind 可能使用不同的映射策略
- **诊断信息**：错误消息根据 OriginKind 显示不同上下文
- **HMR 边界检测**：Template 和 Logic 变更触发不同的 HMR 策略

### 2.3 RazorVueMappingQuality 枚举

映射质量标识，描述源码和生成代码的对应关系。

```csharp
public enum RazorVueMappingQuality
{
    ExactSource,           // 精确源码映射（1:1 对应，如 prop 定义）
    MappedFromGenerated,   // 从生成代码映射（经过 Source Generator 转换）
    GeneratedOnly          // 仅生成代码（无源码对应，如优化代码）
}
```

**映射示例**：

| C# 源码 | JavaScript 生成码 | MappingQuality |
|---------|------------------|----------------|
| `[Parameter] public string Title { get; set; }` | `props: { title: String }` | `ExactSource` |
| `@code { void Foo() { ... } }` | `function foo() { ... }` | `MappedFromGenerated` |
| （无源码） | `/* Optimized code */` | `GeneratedOnly` |

### 2.4 RazorVueOriginProvenance 枚举

映射来源证明，标识映射信息的获取方式。

```csharp
public enum RazorVueOriginProvenance
{
    RazorSourceMap,             // 来自 Razor 源码映射（.razor 文件）
    GeneratedSyntaxLocation,    // 来自生成语法位置（Source Generator 生成的 C# 代码）
    GeneratedFallback           // 生成代码回退（无法映射到源码）
}
```

**来源示例**：

| 场景 | Provenance |
|------|-----------|
| Razor 模板中的 `@Title` | `RazorSourceMap` |
| Source Generator 生成的 `BuildRenderTree` 方法 | `GeneratedSyntaxLocation` |
| 优化的内联代码（无源码对应） | `GeneratedFallback` |

## 3. 核心算法

### 3.1 FromLocation 工厂方法

从 Roslyn `Location` 对象创建 `RazorVueSourceOrigin`：

```csharp
public static RazorVueSourceOrigin FromLocation(
    Location location,
    RazorVueOriginKind originKind)
{
    if (location is null)
        throw new ArgumentNullException(nameof(location));

    var lineSpan = location.GetLineSpan();
    return new RazorVueSourceOrigin(
        OriginKind: originKind,
        SourceFilePath: lineSpan.Path ?? string.Empty,
        SourceSpanStart: location.SourceSpan.Start,
        SourceSpanLength: location.SourceSpan.Length,
        StartLine: lineSpan.StartLinePosition.Line + 1,    // 转换为 1-based
        StartColumn: lineSpan.StartLinePosition.Character + 1,
        GeneratedFilePath: lineSpan.Path,
        GeneratedSpanStart: location.SourceSpan.Start,
        GeneratedSpanLength: location.SourceSpan.Length,
        MappingQuality: RazorVueMappingQuality.MappedFromGenerated,
        Provenance: RazorVueOriginProvenance.GeneratedSyntaxLocation
    );
}
```

**转换规则**：
- **行列转换**：Roslyn 返回 0-based，转换为 1-based（符合 IDE 习惯）
- **路径处理**：`lineSpan.Path` 可能为 null，使用空字符串回退
- **质量推断**：默认 `MappedFromGenerated`（因为是 Location 对象）

### 3.2 SourceMap 生成流程

```
RazorVue 组件 (C#)
       ↓
Source Generator 分析（提取 Location 信息）
       ↓
RazorVueSourceOrigin 集合（存储在 VueCompiledArtifact.SourceOrigins）
       ↓
SourceMap 发射器（遍历 SourceOrigins）
       ↓
生成 .js.map 文件（VLQ 编码）
```

**SourceMap 结构**（简化）：

```json
{
  "version": 3,
  "sources": ["Components/MyComponent.razor"],
  "names": ["Title", "OnInitialized"],
  "mappings": "AAAA,GAAG,GAAG,CAAC;AACJ,GAAG,GAAG,CAAC",
  "sourcesContent": ["@code { [Parameter] public string Title { get; set; } }"]
}
```

### 3.3 映射质量推断算法

Source Generator 根据 `OriginKind` 和转换复杂度推断 `MappingQuality`：

```csharp
// 伪代码示例
RazorVueMappingQuality InferMappingQuality(RazorVueOriginKind originKind, IOperation operation)
{
    return originKind switch
    {
        RazorVueOriginKind.Descriptor when IsSimpleProperty(operation) =>
            RazorVueMappingQuality.ExactSource,

        RazorVueOriginKind.Template when IsSimpleTemplate(operation) =>
            RazorVueMappingQuality.MappedFromGenerated,

        RazorVueOriginKind.Logic when IsComplexLogic(operation) =>
            RazorVueMappingQuality.MappedFromGenerated,

        RazorVueOriginKind.GeneratedRender =>
            RazorVueMappingQuality.GeneratedOnly,

        _ => RazorVueMappingQuality.MappedFromGenerated
    };
}
```

**推断规则**：
- **ExactSource**：1:1 映射（如 prop 定义 → props 字段）
- **MappedFromGenerated**：经过转换（如 C# 方法 → JS 函数）
- **GeneratedOnly**：无源码对应（如优化代码、runtime 辅助函数）

## 4. 线程安全模型

`RazorVueSourceOrigin` 是不可变 record 类型，天然线程安全。

- **构建阶段**：Source Generator 单线程构建（编译时）
- **读取阶段**：SourceMap 发射器并发读取（无状态访问）

## 5. 错误处理

### 5.1 FromLocation 参数验证

```csharp
if (location is null)
    throw new ArgumentNullException(nameof(location));
```

**错误场景**：
- Source Generator 传入 null Location
- 语法节点无位置信息（如合成代码）

### 5.2 路径为空处理

```csharp
SourceFilePath: lineSpan.Path ?? string.Empty
```

**原因**：某些合成代码（如生成的 `BuildRenderTree` 方法）可能无文件路径。

**处理方式**：使用空字符串而非 null，避免下游空引用异常。

### 5.3 GeneratedSpan 为 null 的处理

`GeneratedSpanStart/Length` 是可空字段（`int?`），允许为 null：

```csharp
var origin = new RazorVueSourceOrigin(
    // ...
    GeneratedSpanStart: null,  // 无生成位置信息
    GeneratedSpanLength: null,
    // ...
);
```

**使用场景**：
- 源码尚未生成（分析阶段）
- 生成代码位置未知（动态生成）

## 6. 配置选项

无直接配置选项。行为由 `FromLocation` 工厂方法固定。

## 7. 与其他子系统的交互

### 7.1 与 Roslyn Location 的交互

`FromLocation` 方法直接从 Roslyn `Location` 对象提取信息：

```csharp
var lineSpan = location.GetLineSpan();
SourceSpanStart = location.SourceSpan.Start,
StartLine = lineSpan.StartLinePosition.Line + 1,
```

**字段映射**：

| Roslyn 字段 | RazorVueSourceOrigin 字段 |
|-------------|--------------------------|
| `Location.SourceSpan.Start` | `SourceSpanStart` |
| `Location.SourceSpan.Length` | `SourceSpanLength` |
| `FileLinePositionSpan.StartLinePosition.Line` | `StartLine` (+1) |
| `FileLinePositionSpan.StartLinePosition.Character` | `StartColumn` (+1) |

### 7.2 与 VueCompiledArtifact 的交互

`VueCompiledArtifact.SourceOrigins` 存储所有映射信息：

```csharp
public sealed record VueCompiledArtifact(
    // ...
    ImmutableArray<RazorVueSourceOrigin> SourceOrigins
);
```

**关联方式**：
- 每个 `VueCompiledArtifact` 包含多个 `RazorVueSourceOrigin`（对应不同代码段）
- 通过 `OriginKind` 区分组件定义、模板、逻辑等不同部分

### 7.3 与 SourceMap 发射器的交互

SourceMap 发射器遍历 `SourceOrigins` 生成 `.js.map` 文件：

```csharp
// SourceMap 发射器伪代码
var mappings = new StringBuilder();
foreach (var origin in artifact.SourceOrigins)
{
    var entry = new SourceMapEntry
    {
        GeneratedLine = origin.GeneratedSpanStart ?? 0,
        GeneratedColumn = 0,
        SourceIndex = 0,
        OriginalLine = origin.StartLine,
        OriginalColumn = origin.StartColumn,
        NameIndex = GetNameIndex(origin)  // 可选
    };
    mappings.Append(entry.ToVLQ());
}
```

**质量过滤**：
- `GeneratedOnly` 映射不进入 SourceMap（无源码对应）
- `ExactSource` 和 `MappedFromGenerated` 进入 SourceMap

### 7.4 与诊断系统的交互

诊断系统使用 `SourceOrigin` 显示错误位置：

```csharp
// 诊断伪代码
foreach (var origin in artifact.SourceOrigins)
{
    if (origin.OriginKind == RazorVueOriginKind.Template)
    {
        diagnostics.Add(new Diagnostic
        {
            Message = "Template syntax error",
            FilePath = origin.SourceFilePath,
            Line = origin.StartLine,
            Column = origin.StartColumn
        });
    }
}
```

## 8. 设计权衡

### 8.1 为什么使用 Record 而非 Class

**问题**：为什么 `RazorVueSourceOrigin` 是 record 而非 class？

**答案**：
- **值语义**：源码映射是不可变事实，天然支持值比较
- **模式匹配**：方便解构和 `with` 表达式（修改质量标记）
- **简洁性**：record 自动生成 `Equals`/`GetHashCode`（用于去重和哈希）

### 8.2 为什么分离 MappingQuality 和 Provenance

**问题**：为什么需要两个枚举描述映射属性？

**答案**：
- **MappingQuality**：描述映射的精确度（用于 SourceMap 生成策略）
- **Provenance**：描述映射的来源（用于调试和审计）

**示例场景**：
- 同一 `MappedFromGenerated` 质量的映射，可能来自 `RazorSourceMap` 或 `GeneratedSyntaxLocation`
- 诊断系统根据 Provenance 决定是否显示警告（如 `GeneratedFallback`）

### 8.3 为什么 GeneratedSpan 是可空字段

**问题**：为什么 `GeneratedSpanStart/Length` 是 `int?` 而非 `int`？

**答案**：
- **分析阶段**：Source Generator 分析时生成代码尚未存在，位置未知
- **动态生成**：某些代码（如 runtime 辅助函数）动态生成，无固定位置

**处理方式**：SourceMap 发射器跳过 `GeneratedSpanStart == null` 的映射。

### 8.4 为什么 FromLocation 默认 MappedFromGenerated

**问题**：为什么 `FromLocation` 不推断 `MappingQuality`，而是默认 `MappedFromGenerated`？

**答案**：
- **保守策略**：Location 对象来自生成代码（如 `BuildRenderTree`），默认 `MappedFromGenerated`
- **精确推断复杂**：需要分析 IOperation 才能确定是否 `ExactSource`（性能开销）
- **显式覆盖**：调用方可在创建后用 `with` 修改质量：

```csharp
var origin = RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Descriptor);
if (IsSimpleProperty(location))
    origin = origin with { MappingQuality = RazorVueMappingQuality.ExactSource };
```

### 8.5 为什么行列使用 1-based

**问题**：为什么 `StartLine/Column` 使用 1-based 而非 0-based？

**答案**：
- **IDE 习惯**：大多数编辑器和诊断显示使用 1-based（如 VS Code）
- **SourceMap 标准**：SourceMap 规范要求 1-based 行列
- **用户友好**：错误消息 "Line 1" 比 "Line 0" 更直观

**转换开销**：从 Roslyn 的 0-based 转换为 1-based（+1 操作）。
