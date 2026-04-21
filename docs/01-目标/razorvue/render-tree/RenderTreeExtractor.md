# RazorVue Render Tree 提取器

## 概述

**RazorVueRenderTreeExtractor** 负责从 Blazor 组件的 `BuildRenderTree` 方法中提取框架无关的渲染树表示。它通过解析 Roslyn 操作树（`IOperation`），将 `RenderTreeBuilder` 的方法调用转换为结构化的 `RazorVueRenderNode` 节点。

**核心文件**: `src/Jazor.RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs`

## 核心职责

1. **方法体解析**: 定位并解析 `BuildRenderTree` 方法体
2. **操作树遍历**: 深度遍历 `IBlockOperation` 树
3. **模式识别**: 识别 `RenderTreeBuilder` 的 API 调用模式
4. **节点构建**: 栈式构建元素和组件节点树
5. **源码映射**: 追踪每个节点的源码位置

## 提取流程

### 1. 入口点：Extract 方法

```csharp
public RazorVueRenderFragment Extract(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
{
    var method = snapshot.BuildRenderTreeMethod;
    if (method is null)
        return RazorVueRenderFragment.Empty;

    // 获取 RenderTreeBuilder 参数
    var builderParameters = method.Parameters
        .Where(p => p.Name == "builder" || p.Type.Name == "RenderTreeBuilder")
        .ToImmutableHashSet<IParameterSymbol>();

    // 解析方法体
    var operation = model.GetOperation(methodSyntax.Body);
    if (operation is IBlockOperation block)
        return new Parser(snapshot, context.Symbols, builderParameters).Parse(block.Operations);

    return RazorVueRenderFragment.Empty;
}
```

**关键步骤**:
1. 获取 `BuildRenderTree` 方法符号
2. 识别 `RenderTreeBuilder` 参数（用于验证调用目标）
3. 获取方法体的语义模型（`IOperation`）
4. 委托给内部 `Parser` 类进行解析

### 2. 内部 Parser 类

**Parser** 是提取器的核心，使用状态机模式管理解析状态：

```csharp
private sealed class Parser
{
    private readonly RazorVueSemanticSnapshot _snapshot;
    private readonly RazorVueCompilationSymbols _symbols;
    private readonly ImmutableHashSet<IParameterSymbol> _builderParameters;
    private readonly List<RazorVueRenderNode> _rootChildren = [];
    private readonly Stack<OpenNodeBuilder> _openNodes = new();
}
```

**状态字段**:
- `_rootChildren`: 根级子节点列表
- `_openNodes`: 当前打开的节点栈（用于嵌套结构）

### 3. 操作树遍历

```csharp
public RazorVueRenderFragment Parse(IEnumerable<IOperation> operations)
{
    foreach (var operation in operations)
        ParseOperation(operation);

    // 关闭所有未关闭的节点
    while (_openNodes.Count > 0)
        AddNode(_openNodes.Pop().Build());

    return new RazorVueRenderFragment(_rootChildren.ToImmutableArray());
}
```

**遍历策略**:
- 深度优先遍历操作树
- 自动关闭未闭合的节点（容错处理）
- 构建不可变的渲染片段

## 模式识别

### 1. 语句级别识别

```csharp
private void ParseOperation(IOperation? operation)
{
    var current = Unwrap(operation);
    switch (current)
    {
        case IExpressionStatementOperation expressionStatement:
            ParseExpressionStatement(expressionStatement);
            break;
        case IConditionalOperation conditional:
            // 提取 @if 语句
            AddNode(new RazorVueConditionalNode(
                conditional.Condition,
                ParseNestedBranch(conditional.WhenTrue),
                ParseNestedBranch(conditional.WhenFalse),
                CreateOrigins(current, RazorVueOriginKind.Template)));
            break;
        case IForEachLoopOperation foreachLoop:
            // 提取 @foreach 循环
            AddNode(new RazorVueForEachNode(
                foreachLoop.Locals[0].Name,
                foreachLoop.Collection,
                ParseNestedBranch(foreachLoop.Body),
                CreateOrigins(current, RazorVueOriginKind.Template)));
            break;
        case IBlockOperation block:
            // 递归遍历块语句
            foreach (var child in block.Operations)
                ParseOperation(child);
            break;
    }
}
```

**支持的模式**:
- `IExpressionStatementOperation`: 表达式语句（可能包含 `RenderTreeBuilder` 调用）
- `IConditionalOperation`: `@if` 条件语句
- `IForEachLoopOperation`: `@foreach` 循环语句
- `IBlockOperation`: 块语句（递归遍历）

### 2. RenderTreeBuilder 调用识别

```csharp
private void ParseExpressionStatement(IExpressionStatementOperation expressionStatement)
{
    if (Unwrap(expressionStatement.Operation) is not IInvocationOperation invocation)
        return;

    if (!IsRenderTreeBuilderInvocation(invocation))
        return;

    switch (invocation.TargetMethod.Name)
    {
        case "OpenElement":
            OpenElement(invocation);
            break;
        case "CloseElement":
            CloseCurrentNode(expectedComponent: false);
            break;
        case "OpenComponent":
            OpenComponent(invocation);
            break;
        case "CloseComponent":
            CloseCurrentNode(expectedComponent: true);
            break;
        case "AddAttribute":
            AddAttribute(invocation);
            break;
        case "AddContent":
            AddContent(invocation);
            break;
        case "AddMarkupContent":
            AddMarkupContent(invocation);
            break;
    }
}
```

**验证机制**:
```csharp
private bool IsRenderTreeBuilderInvocation(IInvocationOperation invocation)
{
    return invocation.Instance is IParameterReferenceOperation parameterReference &&
           _builderParameters.Contains(parameterReference.Parameter);
}
```

确保只处理 `RenderTreeBuilder` 实例上的方法调用。

### 3. 元素节点构建

```csharp
private void OpenElement(IInvocationOperation invocation)
{
    var tagName = GetConstantStringArgument(invocation, 1);
    if (!string.IsNullOrWhiteSpace(tagName))
        _openNodes.Push(new ElementBuilder(tagName!, CreateOrigins(invocation, RazorVueOriginKind.Template)));
}
```

**对应 Blazor 代码**:
```csharp
builder.OpenElement("div");
```

**转换过程**:
1. 提取标签名字符串（参数 1）
2. 创建 `ElementBuilder` 并压入栈
3. 等待 `CloseElement` 调用完成构建

### 4. 组件节点构建

```csharp
private void OpenComponent(IInvocationOperation invocation)
{
    if (invocation.TargetMethod.TypeArguments.Length != 1)
        return;

    var componentType = invocation.TargetMethod.TypeArguments[0];
    var resolutionName = GetComponentResolutionName(invocation, componentType.ToDisplayString());
    _openNodes.Push(new ComponentBuilder(
        componentType.Name,
        componentType.ToDisplayString(),
        resolutionName,
        CreateOrigins(invocation, RazorVueOriginKind.Template)));
}
```

**对应 Blazor 代码**:
```csharp
builder.OpenComponent<MyComponent>(0);
```

**关键信息**:
- `ComponentName`: 类型名称（"MyComponent"）
- `ComponentFullName`: 完全限定名（"App.Pages.MyComponent"）
- `ResolutionName`: 从泛型参数语法提取的解析名称

### 5. 属性添加

```csharp
private void AddAttribute(IInvocationOperation invocation)
{
    if (_openNodes.Count == 0)
        return;

    var name = GetConstantStringArgument(invocation, 1);
    if (string.IsNullOrWhiteSpace(name))
        return;

    _openNodes.Peek().AddAttribute(new RazorVueAttributeNode(
        name!,
        GetInvocationArgument(invocation, 2),
        CreateOrigins(invocation, RazorVueOriginKind.Template)));
}
```

**对应 Blazor 代码**:
```csharp
builder.AddAttribute(0, "class", "container");
builder.AddAttribute(1, "onclick", handler);
```

**处理逻辑**:
- 将属性添加到当前打开节点的属性列表
- 属性值可能是常量或表达式（`IOperation`）

### 6. 内容添加

```csharp
private void AddContent(IInvocationOperation invocation)
{
    var value = GetInvocationArgument(invocation, 1);
    if (value is null)
        return;

    var origins = CreateOrigins(invocation, RazorVueOriginKind.Template);

    // 常量字符串 -> 文本节点
    if (TryGetConstantString(value) is string text)
    {
        AddNode(new RazorVueTextNode(text, origins));
        return;
    }

    // RenderFragment 属性引用 -> 槽出口
    if (TryResolveSlotOutlet(value) is string slotName)
    {
        AddNode(new RazorVueSlotOutletNode(slotName, null, origins));
        return;
    }

    // 其他 -> 表达式节点
    AddNode(new RazorVueExpressionNode(value, origins));
}
```

**智能分发**:
- 常量字符串 → `RazorVueTextNode`
- `RenderFragment` 属性 → `RazorVueSlotOutletNode`
- 表达式 → `RazorVueExpressionNode`

### 7. 槽出口解析

```csharp
private string? TryResolveSlotOutlet(IOperation operation)
{
    if (Unwrap(operation) is not IPropertyReferenceOperation propertyReference)
        return null;

    if (!IsCurrentComponentMember(propertyReference.Property, propertyReference.Instance))
        return null;

    if (!IsRenderFragment(propertyReference.Property.Type))
        return null;

    // ChildContent -> "default"
    // HeaderContent -> "header"
    return string.Equals(propertyReference.Property.Name, "ChildContent", StringComparison.Ordinal)
        ? "default"
        : ToLowerCamelCase(propertyReference.Property.Name);
}
```

**对应 Blazor 代码**:
```csharp
builder.AddContent(0, Header);  // -> 槽出口 "header"
builder.AddContent(1, ChildContent);  // -> 槽出口 "default"
```

## 栈式树构建

### OpenNodeBuilder 抽象类

```csharp
private abstract class OpenNodeBuilder
{
    private readonly List<RazorVueAttributeNode> _attributes = [];
    private readonly List<RazorVueRenderNode> _children = [];

    public void AddAttribute(RazorVueAttributeNode attribute)
        => _attributes.Add(attribute);

    public void AddChild(RazorVueRenderNode child)
        => _children.Add(child);

    protected ImmutableArray<RazorVueAttributeNode> BuildAttributes()
        => _attributes.ToImmutableArray();

    protected RazorVueRenderFragment BuildChildren()
        => new(_children.ToImmutableArray());

    public abstract RazorVueRenderNode Build();
}
```

**设计模式**:
- **构建器模式**: 累积属性和子节点
- **模板方法模式**: 子类实现 `Build()` 方法

### ElementBuilder 实现

```csharp
private sealed class ElementBuilder(string tagName, ImmutableArray<RazorVueSourceOrigin> origins)
    : OpenNodeBuilder(origins)
{
    public override RazorVueRenderNode Build()
        => new RazorVueElementNode(tagName, BuildAttributes(), BuildChildren(), Origins);
}
```

### ComponentBuilder 实现

```csharp
private sealed class ComponentBuilder(
    string componentName,
    string componentFullName,
    string resolutionName,
    ImmutableArray<RazorVueSourceOrigin> origins
) : OpenNodeBuilder(origins)
{
    public override RazorVueRenderNode Build()
        => new RazorVueComponentNode(
            componentName,
            componentFullName,
            resolutionName,
            BuildAttributes(),
            BuildChildren(),
            Origins);
}
```

## 嵌套分支处理

```csharp
private RazorVueRenderFragment ParseNestedBranch(IOperation? operation)
{
    var current = Unwrap(operation);
    if (current is null)
        return RazorVueRenderFragment.Empty;

    return current switch
    {
        IBlockOperation block =>
            new Parser(_snapshot, _symbols, _builderParameters).Parse(block.Operations),
        _ =>
            new Parser(_snapshot, _symbols, _builderParameters).Parse([current])
    };
}
```

**用途**:
- 处理 `@if` 和 `@foreach` 的嵌套体
- 为每个分支创建独立的解析器实例
- 保持解析状态的隔离

## 辅助功能

### 1. 隐式转换解包

```csharp
private static IOperation? Unwrap(IOperation? operation)
{
    var current = operation;
    while (current is IConversionOperation conversion && conversion.IsImplicit)
        current = conversion.Operand;

    return current;
}
```

**作用**: 移除编译器插入的隐式转换，获取底层操作。

### 2. 源码映射创建

```csharp
private static ImmutableArray<RazorVueSourceOrigin> CreateOrigins(
    IOperation operation,
    RazorVueOriginKind originKind)
{
    return operation.Syntax is null
        ? ImmutableArray<RazorVueSourceOrigin>.Empty
        : ImmutableArray.Create(RazorVueSourceOrigin.FromLocation(operation.Syntax.GetLocation(), originKind));
}
```

**作用**: 为每个节点关联源码位置，支持错误定位。

### 3. 常量字符串提取

```csharp
private static string? TryGetConstantString(IOperation? operation)
{
    var current = Unwrap(operation);
    if (current?.ConstantValue.HasValue == true &&
        current.ConstantValue.Value is string text)
        return text;

    return null;
}
```

## 错误处理策略

### 1. 容错解析

- 空操作（`IVariableDeclarationGroupOperation`）静默跳过
- 未知的节点类型返回 `RazorVueRenderFragment.Empty`
- 未闭合的节点在遍历结束时自动关闭

### 2. 参数验证

```csharp
private void AddAttribute(IInvocationOperation invocation)
{
    if (_openNodes.Count == 0)  // 没有打开的节点
        return;

    var name = GetConstantStringArgument(invocation, 1);
    if (string.IsNullOrWhiteSpace(name))  // 无效属性名
        return;

    // ...
}
```

### 3. 类型检查

```csharp
private bool IsRenderFragment(ITypeSymbol typeSymbol)
    => typeSymbol is INamedTypeSymbol namedType &&
       ((_symbols.RenderFragment is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragment)) ||
        (_symbols.RenderFragmentOfT is not null && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, _symbols.RenderFragmentOfT)));
```

确保只处理 `RenderFragment` 和 `RenderFragment<T>` 类型。

## 性能考虑

### 1. 不可变构建

使用 `ImmutableArray<T>` 和 `record` 类型确保线程安全。

### 2. 栈式状态管理

使用 `Stack<OpenNodeBuilder>` 避免递归深度限制。

### 3. 惰性求值

只在需要时创建 `RazorVueSourceOrigin` 实例。

## 限制与约束

### 1. 仅支持 BuildRenderTree 模式

- 不支持直接返回 JSX/HTML 的组件
- 不支持部分视图或视图组件

### 2. 表达式限制

- 仅支持编译时可分析的表达式
- 不支持 `dynamic` 类型
- 不支持 LINQ 查询

### 3. 生命周期方法

提取器不处理生命周期逻辑（由 `RazorVueArtifactFactory.ModuleBuilder` 处理）。

## 使用示例

```csharp
// Blazor 组件
@code {
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (isVisible)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "container");
            builder.AddContent(2, $"Count: {count}");
            builder.CloseElement();
        }
    }
}

// 提取结果
RazorVueConditionalNode(
    Condition: [isVisible expression],
    WhenTrue: RazorVueRenderFragment([
        RazorVueElementNode(
            TagName: "div",
            Attributes: [
                RazorVueAttributeNode("class", "container")
            ],
            Children: [
                RazorVueExpressionNode([interpolated string])
            ]
        )
    ]),
    WhenFalse: Empty
)
```

## 相关文档

- **渲染树模型**: `docs/01-目标/razorvue/render-tree/RenderTree.md`
- **降级器**: `docs/01-目标/razorvue/lowering/ArtifactFactory.md`
- **模块构建器**: `docs/01-目标/razorvue/lowering/ModuleBuilder.md`

---

**维护者**: developerhan
**最后更新**: 2026-04-21
**版本**: v1.0
