# RazorVuePipeline 设计文档

## 1. 概述

**RazorVuePipeline** 是 RazorVue 线路的核心编排器，负责将 C# 组件转换为 Vue artifacts。它位于 `Jazor.RazorVue` 项目中（而非 Roslyn generator 宿主层），体现了 RazorVue 语义所有权与编译器集成的分离。

**文件位置**: `src/Jazor.RazorVue/RazorVuePipeline.cs`

## 2. 架构设计

### 2.1 依赖注入点

Pipeline 通过三个扩展点实现可测试性和可扩展性：

```csharp
public sealed class RazorVuePipeline
{
    private readonly IRazorSemanticFrontend _semanticFrontend;
    private readonly IRazorVueArtifactLowerer _artifactLowerer;
    private readonly RazorVueCatalogBuilder _catalogBuilder = new();
}
```

| 依赖 | 职责 | 默认实现 |
|------|------|---------|
| `IRazorSemanticFrontend` | 从 Roslyn Compilation / 宿主绑定结果提取 RazorVue 语义快照 | `DefaultRazorSemanticFrontend.Instance`（Roslyn-only），Razor SDK 宿主默认使用 `RazorVueRazorDocumentSemanticFrontend.Instance` |
| `IRazorVueArtifactLowerer` | 将语义快照降级为 VueCompiledArtifact | `RazorVueArtifactFactory` |
| `RazorVueCatalogBuilder` | 构建最终组件目录 | 内部实现 |

### 2.2 构造函数重载

```csharp
// 单参构造：显式模板前端，使用默认语义前端和默认降级器
public RazorVuePipeline(IRazorVueTemplateFrontend templateFrontend)
    : this(DefaultRazorSemanticFrontend.Instance, templateFrontend)

// 双参构造：显式语义前端 + 显式模板前端
public RazorVuePipeline(IRazorSemanticFrontend semanticFrontend, IRazorVueTemplateFrontend templateFrontend)
    : this(semanticFrontend, new RazorVueArtifactFactory(templateFrontend))

// 双参构造：完全自定义
public RazorVuePipeline(
    IRazorSemanticFrontend semanticFrontend,
    IRazorVueArtifactLowerer artifactLowerer)
```

**设计原则**:
- `Jazor.Common` 不再隐式决定模板前端策略
- RazorVue 宿主必须显式传入模板前端（例如手写 `BuildRenderTree` 前端，或 Razor SDK / IR 前端）
- pipeline 只负责编排，不负责“默认选哪条模板语义路线”

## 3. Execute 重载方法

### 3.1 Execute(Compilation) - 完整管线入口

**签名**:
```csharp
public RazorVueCatalog Execute(Compilation compilation)
```

**执行流程**:

```
1. 参数验证
   └── compilation is null → throw ArgumentNullException

2. 前置检查
   └── !_semanticFrontend.CanHandle(compilation)
       → 返回空目录 (Build(compilation.AssemblyName, Empty))

3. 创建共享上下文
   └── RazorVueCompilationContext.TryCreate(compilation)
       → null → throw InvalidOperationException
       → 非null → 继续执行

4. 语义快照创建
   └── _semanticFrontend.CreateSemanticSnapshots(compilation)
       → ImmutableArray<RazorVueSemanticSnapshot>

5. Artifact 降级
   └── 对每个 snapshot 调用 _artifactLowerer.Lower(context, snapshot)
       → ImmutableArray<VueCompiledArtifact>

6. 目录构建
   └── _catalogBuilder.Build(assemblyName, artifacts)
       → RazorVueCatalog
```

**关键设计决策**:
- **共享上下文**: `RazorVueCompilationContext` 在整个执行过程中保持一致，确保前端和降级器使用相同的快照视图
- **早期退出**: 如果前端无法处理编译，立即返回空目录而非抛出异常
- **空安全**: 即使没有候选组件，也返回有效的 `RazorVueCatalog`（artifacts 为空）

### 3.2 Execute(RazorVueCompilationContext) - 预构建上下文入口

**签名**:
```csharp
public RazorVueCatalog Execute(RazorVueCompilationContext context)
```

**执行流程**:

```
1. 参数验证
   └── context is null → throw ArgumentNullException

2. 语义快照创建
   └── _semanticFrontend.CreateSemanticSnapshots(context.Compilation)
       → DefaultOrEmpty → ImmutableArray<VueCompiledArtifact>.Empty
       → 非空 → 继续执行

3. Artifact 降级
   └── 对每个 snapshot 调用 _artifactLowerer.Lower(context, snapshot)
       → ImmutableArray<VueCompiledArtifact>

4. 目录构建
   └── _catalogBuilder.Build(assemblyName, artifacts)
       → RazorVueCatalog
```

**使用场景**:
- 外部已经创建了 `RazorVueCompilationContext`
- 需要跨多个 pipeline 执行共享同一个上下文
- 测试场景中需要注入预配置的上下文

### 3.3 Execute(string, ImmutableArray<RazorVueSemanticSnapshot>) - 快照入口

**签名**:
```csharp
public RazorVueCatalog Execute(
    string assemblyName,
    ImmutableArray<RazorVueSemanticSnapshot> snapshots)
```

**执行流程**:

```
1. 参数验证
   └── string.IsNullOrWhiteSpace(assemblyName)
       → throw ArgumentException

2. Artifact 降级
   └── snapshots.IsDefaultOrEmpty
       → ImmutableArray<VueCompiledArtifact>.Empty
       → 非空 → _artifactLowerer.Lower(snapshot)
           （注意：此重载不传入 context）

3. 目录构建
   └── _catalogBuilder.Build(assemblyName, artifacts)
       → RazorVueCatalog
```

**使用场景**:
- 组件发现和快照创建与 artifact 降级分离的场景
- 需要缓存或重用语义快照
- 测试特定快照的降级行为

**限制**: 此重载调用 `_artifactLowerer.Lower(snapshot)` 而非 `Lower(context, snapshot)`，因此无法访问编译上下文信息。

## 4. 完整管线数据流

```
┌─────────────────────────────────────────────────────────────────┐
│                      Roslyn Compilation                        │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│           IRazorSemanticFrontend.CanHandle()                    │
│           (DefaultRazorSemanticFrontend.Instance)               │
└────────────────────────┬────────────────────────────────────────┘
                         │ true
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│           RazorVueCompilationContext.TryCreate()                │
│           (创建共享编译上下文)                                   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│     IRazorSemanticFrontend.CreateSemanticSnapshots()            │
│     (发现候选组件 → 分类 → 创建 RazorVueSemanticSnapshot)       │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│          IRazorVueArtifactLowerer.Lower(context, snapshot)      │
│          (将语义快照降级为 VueCompiledArtifact)                  │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│              RazorVueCatalogBuilder.Build()                     │
│              (构建最终的 RazorVueCatalog)                       │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    RazorVueCatalog                              │
│              { AssemblyName, Artifacts[] }                      │
└─────────────────────────────────────────────────────────────────┘
```

## 5. 语义前端集成

### 5.1 DefaultRazorSemanticFrontend 实现

**文件位置**: `src/Jazor.RazorVue/Extensibility/DefaultRazorSemanticFrontend.cs`

```csharp
internal sealed class DefaultRazorSemanticFrontend : IRazorSemanticFrontend
{
    public static DefaultRazorSemanticFrontend Instance { get; } = new();

    public string Name => "Jazor.Compiler.DefaultRazorFrontend";

    public bool CanHandle(RazorVueCompilationContext context)
        => context is not null;

    public RazorVueEntryKind ClassifyEntry(RazorVueCompilationContext context, INamedTypeSymbol symbol)
        => GetRequiredContext(context).ClassifyEntry(symbol);

    public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(RazorVueCompilationContext context)
        => GetRequiredContext(context).CreateSemanticSnapshots();
}
```

### 5.2 委托模式

`DefaultRazorSemanticFrontend` 现在只是 `RazorVueCompilationContext` 的 Roslyn-only 薄包装：

| 方法 | 委托目标 |
|------|---------|
| `CanHandle()` | `context != null` |
| `ClassifyEntry()` | `context.ClassifyEntry()` |
| `CreateSemanticSnapshots()` | `context.CreateSemanticSnapshots()` |

**设计意图**:
- `Jazor.Common` 默认语义前端不再承担 `.razor` / `_Imports.razor` 文档定位
- 保持 `IRazorSemanticFrontend` 接口稳定，让 Razor SDK 宿主可以在外层替换成文档感知前端
- 单例模式避免重复初始化开销
- 需要 Razor 文档绑定时，由 `Jazor.RazorVue.RazorSdk.RazorVueRazorDocumentSemanticFrontend` 在宿主层补齐

## 6. 错误处理策略

### 6.1 参数验证

所有 `Execute` 方法都进行严格的参数验证：

| 参数 | 验证规则 | 异常类型 |
|------|---------|---------|
| `Compilation compilation` | `compilation is null` | `ArgumentNullException` |
| `RazorVueCompilationContext context` | `context is null` | `ArgumentNullException` |
| `string assemblyName` | `string.IsNullOrWhiteSpace(assemblyName)` | `ArgumentException` |

### 6.2 上下文创建失败

当 `CanHandle()` 返回 `true` 但 `TryCreate()` 返回 `null` 时：

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation)
    ?? throw new InvalidOperationException(
        "RazorVue compilation context was expected once the semantic frontend accepted the compilation.");
```

**设计决策**: 这是一个编程错误（前端与上下文创建器不一致），应该快速失败而非返回空目录。

### 6.3 空结果处理

Pipeline 不抛出"没有组件"的异常，而是返回有效的空目录：

```csharp
if (!_semanticFrontend.CanHandle(compilation))
    return _catalogBuilder.Build(
        compilation.AssemblyName ?? "Jazor.Assembly",
        ImmutableArray<VueCompiledArtifact>.Empty);
```

**好处**: 调用者可以统一处理 `RazorVueCatalog`，无需额外检查"是否有组件"。

## 7. 扩展点说明

### 7.1 IRazorSemanticFrontend - 语义提取接口

**文件位置**: `src/Jazor.RazorVue/Extensibility/IRazorSemanticFrontend.cs`

```csharp
public interface IRazorSemanticFrontend
{
    string Name { get; }

    bool CanHandle(Compilation compilation);

    RazorVueEntryKind ClassifyEntry(Compilation compilation, INamedTypeSymbol symbol);

    ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots(Compilation compilation);
}
```

**设计目标**: 长期来看，Razor 所有的项目应该实现此接口，而不强制 `Jazor.Compiler` 永久拥有每个前端细节。

### 7.2 IRazorVueArtifactLowerer - Artifact 降级接口

**文件位置**: `src/Jazor.RazorVue/Extensibility/IRazorVueArtifactLowerer.cs`

```csharp
public interface IRazorVueArtifactLowerer
{
    VueCompiledArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot);

    VueCompiledArtifact Lower(RazorVueSemanticSnapshot snapshot);
}
```

**设计目标**: 保持此契约明确可以防止 pipeline 退化为直接字符串生成。降级器负责：
- 将语义快照转换为 VueCompiledArtifact
- 处理模板、逻辑、描述符的 AST 转换
- 生成源码映射和 HMR 边界信息

## 8. 使用示例

### 8.1 基本使用（显式模板前端）

```csharp
var pipeline = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance);
var catalog = pipeline.Execute(compilation);

foreach (var artifact in catalog.Artifacts)
{
    Console.WriteLine($"Component: {artifact.ComponentName}");
    Console.WriteLine($"Module: {artifact.RelativeModulePath}");
}
```

### 8.2 自定义模板前端

```csharp
var customFrontend = new MyCustomTemplateFrontend();
var pipeline = new RazorVuePipeline(customFrontend);
var catalog = pipeline.Execute(compilation);
```

### 8.3 使用预构建上下文

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation);
if (context is not null)
{
    var pipeline = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance);
    var catalog = pipeline.Execute(context);
}
```

### 8.4 使用预构建快照

```csharp
var snapshots = ImmutableArray.Create(
    new RazorVueSemanticSnapshot(/* ... */),
    new RazorVueSemanticSnapshot(/* ... */)
);

var pipeline = new RazorVuePipeline(BuildRenderTreeTemplateFrontend.Instance);
var catalog = pipeline.Execute("MyAssembly", snapshots);
```

## 9. 性能考虑

### 9.1 上下文缓存

`RazorVueCompilationContext` 在 pipeline 执行期间保持不变，避免重复创建：

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation);
var snapshots = _semanticFrontend.CreateSemanticSnapshots(context);
var artifacts = snapshots.Select(s => _artifactLowerer.Lower(context, s));
```

### 9.2 不可变集合

所有中间结果都使用 `ImmutableArray<T>`：
- 线程安全
- 避免不必要的复制
- 支持高效的集合操作（如 `.Select()` 和 `.ToImmutableArray()`）

### 9.3 延迟执行

虽然当前实现立即执行所有步骤，但设计允许未来优化为延迟执行：

```csharp
// 当前：立即执行
var artifacts = snapshots.Select(s => _artifactLowerer.Lower(context, s))
                         .ToImmutableArray();

// 未来可能的优化：延迟执行
IEnumerable<VueCompiledArtifact> artifacts =
    snapshots.Select(s => _artifactLowerer.Lower(context, s));
```

## 10. 测试指南

### 10.1 单元测试场景

| 场景 | 测试方法 |
|------|---------|
| 空编译 | `Execute(EmptyCompilation)` 返回空目录 |
| 无候选组件 | `Execute(CompilationWithoutCandidates)` 返回空目录 |
| 有效组件 | `Execute(ValidCompilation)` 返回包含 artifacts 的目录 |
| 上下文创建失败 | `Execute(CompilationWithInvalidContext)` 抛出 `InvalidOperationException` |
| 自定义前端 | 注入 mock `IRazorSemanticFrontend` 验证交互 |

### 10.2 集成测试场景

| 场景 | 测试方法 |
|------|---------|
| 端到端管线 | 从真实 Compilation 到完整 RazorVueCatalog |
| 多组件项目 | 验证所有候选组件都被正确处理 |
| 错误恢复 | 验证单个组件失败不影响其他组件 |

## 11. 相关文件

| 文件 | 职责 |
|------|------|
| `src/Jazor.RazorVue/RazorVuePipeline.cs` | Pipeline 主类 |
| `src/Jazor.RazorVue/Extensibility/IRazorSemanticFrontend.cs` | 语义前端接口 |
| `src/Jazor.RazorVue/Extensibility/IRazorVueArtifactLowerer.cs` | Artifact 降级接口 |
| `src/Jazor.RazorVue/Extensibility/DefaultRazorSemanticFrontend.cs` | 默认语义前端实现 |
| `src/Jazor.Analyzer/RazorVue/Generation/RazorVueGenerator.cs` | Roslyn generator 宿主（调用 pipeline） |
| `src/Jazor.RazorVue/Artifacts/RazorVueCatalog.cs` | 目录数据结构 |
| `src/Jazor.RazorVue/Artifacts/RazorVueCompilationContext.cs` | 编译上下文 |

---

**文档维护者**: developerhan
**最后更新**: 2026-04-21
**文档版本**: v1.0
