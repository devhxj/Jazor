# RazorVueCompilationContext - 编译上下文

**文件路径**: `src/Jazor.RazorVue/RazorVueCompilationContext.cs`

## 为什么需要

RazorVue 编译时分析需要一个中心化的上下文对象来协调以下职责：

1. **编译信息持有**：持有 Roslyn `Compilation` 和 `RazorVueCompilationSymbols`
2. **组件发现**：从编译中枚举并分类所有组件候选
3. **语义快照创建**：为组件候选构建完整的语义描述
4. **库组件发现**：发现并注册 `IVueLibraryComponent` 实现
5. **组件注册表创建**：生成最终的 `VueComponentRegistry`

`RazorVueCompilationContext` 作为编译时分析的统一入口，简化了上层调用者的使用复杂度。

## 实现思路

### 核心结构

`RazorVueCompilationContext` 是一个 `public sealed` 类，包含两个核心属性：

```csharp
public sealed class RazorVueCompilationContext
{
    public RazorVueCompilationContext(Compilation compilation, RazorVueCompilationSymbols symbols)
    {
        Compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
    }

    public Compilation Compilation { get; }
    public RazorVueCompilationSymbols Symbols { get; }
}
```

**设计原则**：
- 不可变性：构造后 `Compilation` 和 `Symbols` 不可更改
- 必需性：两个属性都是必需的，不允许 `null`
- 验证性：构造时进行参数验证

### 工厂方法

#### `TryCreate(Compilation)`

```csharp
public static RazorVueCompilationContext? TryCreate(Compilation compilation)
{
    if (compilation is null)
        throw new ArgumentNullException(nameof(compilation));

    var symbols = RazorVueCompilationSymbols.TryCreate(compilation);
    return symbols is null ? null : new RazorVueCompilationContext(compilation, symbols);
}
```

**工作流程**：
1. 验证 `compilation` 参数
2. 尝试创建 `RazorVueCompilationSymbols`（可能失败）
3. 如果符号表创建成功，返回 `RazorVueCompilationContext`
4. 如果符号表创建失败，返回 `null`

**失败场景**：
- 缺少必需符号（`ECMAScriptModuleAttribute`、`JazorComponent`、`VueComponent`、`ComponentBase`）
- 引用程序集未正确配置

### 组件分类

#### `ClassifyEntry(INamedTypeSymbol)`

```csharp
public RazorVueEntryKind ClassifyEntry(INamedTypeSymbol symbol)
{
    if (symbol is null)
        throw new ArgumentNullException(nameof(symbol));

    return RazorVueEntryClassifier.Classify(symbol, Symbols);
}
```

**职责**：委托到 `RazorVueEntryClassifier.Classify`，使用上下文持有的符号表。

**用途**：快速判断一个类型是否是 RazorVue 组件，无需完整发现流程。

### 组件候选发现

#### `DiscoverComponentCandidates()`

```csharp
public ImmutableArray<RazorVueComponentCandidate> DiscoverComponentCandidates()
{
    var builder = ImmutableArray.CreateBuilder<RazorVueComponentCandidate>();
    foreach (var symbol in EnumerateNamedTypes(Compilation.GlobalNamespace))
    {
        if (RazorVueEntryClassifier.Classify(symbol, Symbols) != RazorVueEntryKind.RazorVueComponent)
            continue;

        builder.Add(new RazorVueComponentCandidate(
            symbol,
            RazorVueEntryClassifier.FindBuildRenderTreeMethod(symbol),
            RazorVueEntryClassifier.FindOnInitializedMethod(symbol),
            RazorVueEntryClassifier.FindOnInitializedAsyncMethod(symbol),
            RazorVueEntryClassifier.FindOnParametersSetMethod(symbol),
            RazorVueEntryClassifier.FindOnParametersSetAsyncMethod(symbol),
            RazorVueEntryClassifier.FindOnAfterRenderMethod(symbol),
            RazorVueEntryClassifier.FindOnAfterRenderAsyncMethod(symbol),
            RazorVueEntryClassifier.FindShouldRenderMethod(symbol),
            RazorVueEntryClassifier.FindSetParametersAsyncMethod(symbol),
            RazorVueEntryClassifier.FindDisposeMethod(symbol),
            RazorVueEntryClassifier.FindDisposeAsyncMethod(symbol),
            RazorVueEntryClassifier.FindLogicMethods(symbol),
            RazorVueEntryClassifier.FindLogicFields(symbol),
            RazorVueEntryKind.RazorVueComponent));
    }

    return builder.ToImmutable();
}
```

**发现流程**：
1. 遍历 `GlobalNamespace` 中所有命名类型（包括嵌套类型）
2. 使用 `EntryClassifier.Classify` 过滤出 `RazorVueComponent` 类型
3. 使用 `EntryClassifier` 的生命周期方法发现器查找所有生命周期方法
4. 使用 `EntryClassifier` 的逻辑成员发现器查找逻辑方法和字段
5. 构建 `RazorVueComponentCandidate` 对象

**嵌套类型处理**：`EnumerateNamedTypes` 递归遍历命名空间和类型的嵌套层次。

### 命名空间枚举

#### `EnumerateNamedTypes(INamespaceSymbol)`

```csharp
private static IEnumerable<INamedTypeSymbol> EnumerateNamedTypes(INamespaceSymbol namespaceSymbol)
{
    // 1. 枚举当前命名空间的直接类型成员
    foreach (var typeSymbol in namespaceSymbol.GetTypeMembers())
    {
        yield return typeSymbol;
        // 2. 递归枚举嵌套类型
        foreach (var nestedType in EnumerateNestedTypes(typeSymbol))
            yield return nestedType;
    }

    // 3. 递归枚举子命名空间
    foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
    {
        foreach (var childType in EnumerateNamedTypes(childNamespace))
            yield return childType;
    }
}
```

**枚举顺序**：
1. 当前命名空间的类型成员
2. 每个类型的嵌套类型
3. 子命名空间的类型成员（递归）

**嵌套类型枚举**：

```csharp
private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol typeSymbol)
{
    foreach (var nestedType in typeSymbol.GetTypeMembers())
    {
        yield return nestedType;
        foreach (var nestedChild in EnumerateNestedTypes(nestedType))
            yield return nestedChild;
    }
}
```

### 语义快照创建

#### `CreateSemanticSnapshot(RazorVueComponentCandidate)`

```csharp
public RazorVueSemanticSnapshot CreateSemanticSnapshot(RazorVueComponentCandidate candidate)
{
    if (candidate is null)
        throw new ArgumentNullException(nameof(candidate));

    if (candidate.EntryKind != RazorVueEntryKind.RazorVueComponent)
        throw new InvalidOperationException($"Only {nameof(RazorVueEntryKind.RazorVueComponent)} candidates can become semantic snapshots.");

    // 1. 创建组件描述符（参数、插槽、事件等）
    var descriptor = VueComponentDescriptorFactory.Create(candidate, this);

    // 2. 创建生命周期描述符
    var lifecycle = new VueLifecycleDescriptor(
        HasOnInitialized: candidate.OnInitializedMethod is not null,
        HasOnInitializedAsync: candidate.OnInitializedAsyncMethod is not null,
        HasOnParametersSet: candidate.OnParametersSetMethod is not null,
        HasOnParametersSetAsync: candidate.OnParametersSetAsyncMethod is not null,
        HasOnAfterRender: candidate.OnAfterRenderMethod is not null,
        HasOnAfterRenderAsync: candidate.OnAfterRenderAsyncMethod is not null,
        HasShouldRender: candidate.ShouldRenderMethod is not null,
        HasSetParametersAsync: candidate.SetParametersAsyncMethod is not null,
        HasDispose: candidate.DisposeMethod is not null,
        HasDisposeAsync: candidate.DisposeAsyncMethod is not null);

    // 3. 创建逻辑方法描述符
    var logicMethods = candidate.LogicMethods
        .Select(static method => new VueLogicMethodDescriptor(method.Name, method.Parameters.Length, method.IsAsync, method))
        .ToImmutableArray();

    // 4. 创建逻辑字段描述符（保留 Roslyn 符号用于后续 lowering）
    var logicFields = candidate.LogicFields
        .Select(static field => new VueLogicFieldDescriptor(field.Name, field.IsReadOnly, field))
        .ToImmutableArray();

    // 5. 组合逻辑描述符
    var logic = logicMethods.IsDefaultOrEmpty && logicFields.IsDefaultOrEmpty
        ? VueLogicDescriptor.Empty
        : new VueLogicDescriptor(logicFields, logicMethods);

    // 6. 提取源码位置（用于 sourcemap/HMR）
    var origins = candidate.ComponentSymbol.Locations
        .Where(static location => location.IsInSource)
        .Select(static location => RazorVueSourceOrigin.FromLocation(location, RazorVueOriginKind.Component))
        .ToImmutableArray();

    // 7. 提取导入命名空间
    var importedNamespaces = candidate.ComponentSymbol.DeclaringSyntaxReferences
        .Select(static reference => reference.GetSyntax())
        .OfType<TypeDeclarationSyntax>()
        .SelectMany(static declaration =>
            declaration.SyntaxTree.GetRoot() is CompilationUnitSyntax compilationUnit
                ? compilationUnit.Usings
                : Enumerable.Empty<UsingDirectiveSyntax>())
        .Where(static directive => directive.Alias is null && directive.Name is not null)
        .Select(static directive => directive.Name!.ToString())
        .Distinct(StringComparer.Ordinal)
        .ToImmutableArray();

    // 8. 构建语义快照
    return new RazorVueSemanticSnapshot(
        Compilation,
        candidate.ComponentSymbol,
        candidate.BuildRenderTreeMethod,
        lifecycle,
        logic,
        descriptor,
        origins,
        importedNamespaces,
        candidate.OnInitializedMethod,
        candidate.OnInitializedAsyncMethod,
        candidate.OnParametersSetMethod,
        candidate.OnParametersSetAsyncMethod,
        candidate.ShouldRenderMethod,
        candidate.SetParametersAsyncMethod,
        candidate.OnAfterRenderMethod,
        candidate.OnAfterRenderAsyncMethod,
        candidate.DisposeMethod,
        candidate.DisposeAsyncMethod);
}
```

**语义快照内容**：

| 部分 | 来源 | 用途 |
|------|------|------|
| 组件描述符 | `VueComponentDescriptorFactory.Create` | 参数、插槽、事件等组件元数据 |
| 生命周期描述符 | 候选的生命周期方法 | 标识哪些生命周期钩子存在 |
| 逻辑描述符 | 候选的逻辑方法和字段 | 方法签名、字段可读性 |
| 源码位置 | `ComponentSymbol.Locations` | sourcemap 和 HMR 的源码锚点 |
| 导入命名空间 | `UsingDirectiveSyntax` | JavaScript 模块导入生成 |

**关键设计决策**：
- **逻辑字段保留 Roslyn 符号**：注释说明 "Preserve Roslyn field carriers for upcoming setup-side lowering"，为后续 setup 语法降级预留
- **源码位置作为身份锚点**：注释说明 "Keep the first snapshot carrier tied to Roslyn locations"，确保 sourcemap/HMR 有稳定的源码身份

#### `CreateSemanticSnapshots()`

```csharp
public ImmutableArray<RazorVueSemanticSnapshot> CreateSemanticSnapshots()
{
    var builder = ImmutableArray.CreateBuilder<RazorVueSemanticSnapshot>();
    foreach (var candidate in DiscoverComponentCandidates())
        builder.Add(CreateSemanticSnapshot(candidate));

    return builder.ToImmutable();
}
```

**批量创建**：组合 `DiscoverComponentCandidates()` 和 `CreateSemanticSnapshot()`，一次性创建所有组件的语义快照。

### 库组件发现

#### `DiscoverLibraryComponents()`

```csharp
public ImmutableArray<VueComponentDescriptor> DiscoverLibraryComponents()
{
    var builder = ImmutableArray.CreateBuilder<VueComponentDescriptor>();
    foreach (var symbol in EnumerateNamedTypes(Compilation.GlobalNamespace))
    {
        if (!RazorVueEntryClassifier.IsLibraryComponent(symbol, Symbols))
            continue;

        // 库组件存根是仅描述符的编写表面，从编译中发现，不是 [ECMAScriptModule] 运行时入口
        builder.Add(VueComponentDescriptorFactory.CreateLibraryComponent(symbol, this));
    }

    return builder.ToImmutable();
}
```

**库组件特征**：
- 实现 `IVueLibraryComponent` 接口
- 非静态、非抽象
- 不是 `IVueLibraryComponent` 接口本身
- **不需要** `[ECMAScriptModule]` 特性

**注释说明**：
```csharp
// Library stubs are descriptor-only authoring surfaces discovered
// from the compilation, not [ECMAScriptModule] runtime entries.
```

### 组件注册表创建

#### `CreateComponentRegistry(ImmutableArray<VueComponentDescriptor>)`

```csharp
public VueComponentRegistry CreateComponentRegistry(ImmutableArray<VueComponentDescriptor> libraryComponents = default(ImmutableArray<VueComponentDescriptor>))
{
    var discoveredLibraryComponents = DiscoverLibraryComponents();
    if (!libraryComponents.IsDefaultOrEmpty)
        discoveredLibraryComponents = MergeLibraryComponents(discoveredLibraryComponents, libraryComponents);

    return VueComponentRegistry.Create(CreateSemanticSnapshots(), discoveredLibraryComponents);
}
```

**创建流程**：
1. 发现编译中的库组件
2. 合并外部提供的库组件（可选）
3. 创建所有组件的语义快照
4. 构建 `VueComponentRegistry`

#### `MergeLibraryComponents(...)`

```csharp
private static ImmutableArray<VueComponentDescriptor> MergeLibraryComponents(
    ImmutableArray<VueComponentDescriptor> discoveredLibraryComponents,
    ImmutableArray<VueComponentDescriptor> additionalLibraryComponents)
{
    if (discoveredLibraryComponents.IsDefaultOrEmpty)
        return additionalLibraryComponents;
    if (additionalLibraryComponents.IsDefaultOrEmpty)
        return discoveredLibraryComponents;

    var builder = ImmutableArray.CreateBuilder<VueComponentDescriptor>();
    var seenFullNames = new HashSet<string>(StringComparer.Ordinal);

    foreach (var component in discoveredLibraryComponents)
    {
        if (seenFullNames.Add(component.FullName))
            builder.Add(component);
    }

    foreach (var component in additionalLibraryComponents)
    {
        if (seenFullNames.Add(component.FullName))
            builder.Add(component);
    }

    return builder.ToImmutable();
}
```

**合并策略**：
- 使用 `FullName` 去重
- 优先保留发现的组件（外部组件作为补充）
- 确保每个全名只出现一次

## 使用示例

### 创建编译上下文

```csharp
var compilation = ...; // 获取 Roslyn Compilation
var context = RazorVueCompilationContext.TryCreate(compilation);

if (context is null)
{
    Console.WriteLine("Failed to create RazorVue compilation context");
    return;
}
```

### 发现所有组件

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation);
var candidates = context.DiscoverComponentCandidates();

foreach (var candidate in candidates)
{
    Console.WriteLine($"Found component: {candidate.ComponentSymbol.Name}");
    Console.WriteLine($"  BuildRenderTree: {candidate.BuildRenderTreeMethod is not null}");
    Console.WriteLine($"  OnInitialized: {candidate.OnInitializedMethod is not null}");
    Console.WriteLine($"  Logic methods: {candidate.LogicMethods.Length}");
}
```

### 创建语义快照

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation);
var snapshots = context.CreateSemanticSnapshots();

foreach (var snapshot in snapshots)
{
    Console.WriteLine($"Component: {snapshot.ComponentSymbol.Name}");
    Console.WriteLine($"  Parameters: {snapshot.Descriptor.Parameters.Length}");
    Console.WriteLine($"  Lifecycle hooks: {snapshot.Lifecycle}");
    Console.WriteLine($"  Logic methods: {snapshot.Logic.Methods.Length}");
}
```

### 创建组件注册表

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation);
var registry = context.CreateComponentRegistry();

Console.WriteLine($"Total components: {registry.Components.Length}");
Console.WriteLine($"Library components: {registry.LibraryComponents.Length}");
```

### 合并外部库组件

```csharp
var context = RazorVueCompilationContext.TryCreate(compilation);
var externalLibraryComponents = ...; // 从外部加载的库组件

var registry = context.CreateComponentRegistry(externalLibraryComponents);
```

## 设计权衡

### 1. 组件候选 vs 语义快照

**两阶段设计**：
- **候选阶段**：轻量级发现，仅持有 Roslyn 符号
- **快照阶段**：重量级描述，构建完整的语义模型

**优点**：
- 延迟重计算：只有需要的组件才构建快照
- 灵活性：可以在候选阶段过滤
- 内存效率：避免为所有组件构建完整描述符

### 2. 库组件的描述符特性

库组件不需要 `[ECMAScriptModule]` 特性，因为它们是"仅描述符的编写表面"（descriptor-only authoring surfaces），不是运行时入口。

**含义**：
- 库组件不生成 JavaScript 模块
- 库组件仅提供类型信息和元数据
- 库组件用于类型检查和 IDE 智能提示

### 3. 导入命名空间提取

当前实现从 `TypeDeclarationSyntax` 的 `CompilationUnitSyntax` 提取 `UsingDirectiveSyntax`。

**限制**：
- 只处理类型声明所在编译单元的 using
- 不处理文件范围的 using（C# 10 `global using`）
- 可能重复提取（多个类型声明在同一编译单元）

**改进空间**：
- 使用 `Compilation.GlobalImports` 处理 global using
- 缓存编译单元级别的 using 集合

### 4. 源码位置作为身份锚点

语义快照保留 Roslyn `Location` 对象，用于 sourcemap 和 HMR。

**设计原因**：
- sourcemap 需要源文件路径和行列号
- HMR 需要稳定的源码身份来跟踪变更
- Roslyn `Location` 提供了这些信息的权威来源

## 相关文件

- `src/Jazor.RazorVue/Discovery/RazorVueEntryClassifier.cs` - 组件分类和成员发现
- `src/Jazor.RazorVue/RazorVueCompilationSymbols.cs` - 编译符号表
- `src/Jazor.RazorVue/RazorVueComponentCandidate.cs` - 组件候选数据结构
- `src/Jazor.RazorVue/Artifacts/RazorVueSemanticSnapshot.cs` - 语义快照
- `src/Jazor.RazorVue/Descriptor/VueComponentDescriptorFactory.cs` - 组件描述符工厂
- `src/Jazor.RazorVue/Descriptor/VueComponentRegistry.cs` - 组件注册表
