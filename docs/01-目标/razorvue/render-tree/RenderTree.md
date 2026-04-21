# RazorVue Render Tree 模型

## 概述

RazorVue Render Tree 是一个框架无关的中间表示（IR），用于表示 Razor 组件的渲染树结构。它通过解析 Blazor 的 `BuildRenderTree` 方法生成，为后续降级到 Vue SFC 提供结构化的渲染逻辑。

**核心文件**: `src/Jazor.RazorVue/RenderTree/RazorVueRenderTree.cs`

## 设计目标

1. **框架无关性**: 与 Blazor 和 Vue 实现解耦，提供纯净的渲染树表示
2. **类型安全**: 使用 C# record 类型确保不可变性和模式匹配支持
3. **源码追溯**: 每个节点携带 `Origins` 信息，支持源码映射和错误定位
4. **语义完整性**: 支持所有 Blazor 渲染构造（元素、组件、片段、条件、循环）

## 核心类型

### RazorVueRenderFragment

```csharp
internal sealed record RazorVueRenderFragment(
    ImmutableArray<RazorVueRenderNode> Children)
{
    public static RazorVueRenderFragment Empty { get; } =
        new(ImmutableArray<RazorVueRenderNode>.Empty);
}
```

**用途**: 表示渲染片段的容器，对应 Blazor 的 `RenderFragment`。

**特性**:
- 不可变的子节点数组
- 提供 `Empty` 静态实例表示空片段
- 作为组件渲染输出的根节点

### RazorVueRenderNode (抽象基类)

```csharp
internal abstract record RazorVueRenderNode(
    ImmutableArray<RazorVueSourceOrigin> Origins);
```

**用途**: 所有渲染节点的抽象基类。

**核心字段**:
- `Origins`: 源码位置追踪数组，用于错误报告和源码映射

## 具体节点类型

### 1. RazorVueElementNode - 元素节点

```csharp
internal sealed record RazorVueElementNode(
    string TagName,
    ImmutableArray<RazorVueAttributeNode> Attributes,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示 HTML 元素。

**对应 Blazor**:
```csharp
builder.OpenElement("div");
builder.AddAttribute("class", "container");
builder.CloseElement();
```

**字段说明**:
- `TagName`: HTML 标签名（如 "div", "span"）
- `Attributes`: 元素属性集合
- `Children`: 子节点片段

**降级到 Vue**:
```javascript
h("div", { class: "container" }, [...children])
```

### 2. RazorVueComponentNode - 组件节点

```csharp
internal sealed record RazorVueComponentNode(
    string ComponentName,
    string ComponentFullName,
    string ResolutionName,
    ImmutableArray<RazorVueAttributeNode> Attributes,
    RazorVueRenderFragment Children,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示子组件实例。

**对应 Blazor**:
```csharp
builder.OpenComponent<MyComponent>(0);
builder.AddAttribute(1, "Value", 42);
builder.CloseComponent();
```

**字段说明**:
- `ComponentName`: 组件类型名称（如 "MyComponent"）
- `ComponentFullName`: 完全限定类型名（如 "App.Pages.MyComponent"）
- `ResolutionName`: 组件解析名称（可能来自泛型参数）
- `Attributes`: 组件参数和事件处理程序
- `Children`: 默认槽内容（对应 `ChildContent`）

**降级到 Vue**:
```javascript
h(MyComponent, { value: 42 }, { default: () => [...] })
```

### 3. RazorVueTextNode - 文本节点

```csharp
internal sealed record RazorVueTextNode(
    string Text,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示纯文本内容。

**对应 Blazor**:
```csharp
builder.AddContent("Hello, World!");
```

**降级到 Vue**: 直接嵌入字符串或作为 `h()` 函数的子节点。

### 4. RazorVueExpressionNode - 表达式节点

```csharp
internal sealed record RazorVueExpressionNode(
    IOperation Expression,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示 C# 表达式求值结果。

**对应 Blazor**:
```csharp
builder.AddContent(@$"Count is {count}");
```

**核心特性**:
- `Expression`: Roslyn `IOperation` 表示编译时表达式树
- 降级时通过 `RazorVueExpressionEmitter` 翻译为 JavaScript 表达式

**降级到 Vue**:
```javascript
`Count is ${count}`
```

### 5. RazorVueSlotOutletNode - 槽出口节点

```csharp
internal sealed record RazorVueSlotOutletNode(
    string SlotName,
    IOperation? Argument,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示命名槽的渲染点。

**对应 Blazor**:
```csharp
builder.AddContent(RenderFragment, Header);
```

**字段说明**:
- `SlotName`: 槽名称（"default", "header", "footer"）
- `Argument`: 传递给槽的上下文参数（可选）

**降级到 Vue**:
```javascript
slots.header ? slots.header(context) : null
```

### 6. RazorVueConditionalNode - 条件节点

```csharp
internal sealed record RazorVueConditionalNode(
    IOperation Condition,
    RazorVueRenderFragment WhenTrue,
    RazorVueRenderFragment WhenFalse,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示条件渲染逻辑。

**对应 Blazor**:
```csharp
@if (isVisible)
{
    <div>Shown</div>
}
else
{
    <div>Hidden</div>
}
```

**字段说明**:
- `Condition`: Roslyn `IOperation` 表示条件表达式
- `WhenTrue`: 条件为真时的渲染片段
- `WhenFalse`: 条件为假时的渲染片段（可为空）

**降级到 Vue**:
```javascript
condition ? whenTrueFragment : whenFalseFragment
```

### 7. RazorVueForEachNode - 循环节点

```csharp
internal sealed record RazorVueForEachNode(
    string ItemName,
    IOperation Source,
    RazorVueRenderFragment Body,
    ImmutableArray<RazorVueSourceOrigin> Origins) : RazorVueRenderNode(Origins);
```

**用途**: 表示列表渲染逻辑。

**对应 Blazor**:
```csharp
@foreach (var item in items)
{
    <div>@item.Name</div>
}
```

**字段说明**:
- `ItemName`: 迭代变量名称（如 "item"）
- `Source`: Roslyn `IOperation` 表示集合源
- `Body`: 每个迭代项的渲染片段

**降级到 Vue**:
```javascript
source.map((item) => bodyFragment)
```

### 8. RazorVueAttributeNode - 属性节点

```csharp
internal sealed record RazorVueAttributeNode(
    string Name,
    IOperation? Value,
    ImmutableArray<RazorVueSourceOrigin> Origins);
```

**用途**: 表示元素或组件的属性/参数。

**字段说明**:
- `Name`: 属性名称（如 "class", "onclick", "Value"）
- `Value`: 属性值表达式（可选，布尔属性可省略值）

**示例**:
```csharp
// 元素属性
new RazorVueAttributeNode("disabled", null, origins)  // disabled=true
new RazorVueAttributeNode("class", "container", origins)

// 组件参数
new RazorVueAttributeNode("Value", expression, origins)
new RazorVueAttributeNode("ValueChanged", handler, origins)
```

## 使用场景

### 1. 渲染树提取

`RazorVueRenderTreeExtractor` 从 `BuildRenderTree` 方法体提取结构化数据：

```csharp
var renderTree = _renderTreeExtractor.Extract(context, snapshot);
```

### 2. 模板形状分析

用于 HMR（热模块替换）的模板哈希计算：

```csharp
var templateShape = expressionEmitter.DescribeFragment(renderTree);
var templateHash = ComputeSha256Hex(templateShape);
```

### 3. Vue 代码生成

通过 `RazorVueExpressionEmitter` 降级到 Vue 渲染函数：

```csharp
var renderExpression = expressionEmitter.EmitFragment(renderTree);
```

## 设计优势

### 1. 编译时验证

所有节点类型在编译时验证，避免运行时类型错误。

### 2. 不可变性

使用 `record` 类型确保节点不可变，支持安全的树遍历和转换。

### 3. 模式匹配

C# 模式匹配简化节点处理：

```csharp
switch (node)
{
    case RazorVueElementNode element:
        // 处理元素
        break;
    case RazorVueComponentNode component:
        // 处理组件
        break;
    // ...
}
```

### 4. 源码映射

`Origins` 字段支持精确的错误定位：

```csharp
throw CreateAuthoringIssue(
    RazorVueIssueCode.UnknownParameter,
    $"Component '{descriptor.Name}' does not declare parameter '{attribute.Name}'.",
    attribute.Origins.FirstOrDefault());
```

## 限制与约束

### 1. 仅支持 Blazor 渲染模式

Render Tree 基于 `BuildRenderTree` 方法，不支持：
- 基于控制器的视图
- Razor Pages 页面
- 纯 HTML 视图

### 2. 表达式限制

`RazorVueExpressionNode` 只支持编译时可分析的表达式：
- ✅ 字面量、变量引用、属性访问
- ✅ 二元/一元运算符
- ✅ 方法调用（仅限白名单）
- ❌ 动态类型 (`dynamic`)
- ❌ LINQ 查询表达式

### 3. 生命周期隔离

Render Tree 是纯数据结构，不包含：
- 状态管理逻辑
- 生命周期钩子
- 副作用操作

这些由 `RazorVueArtifactFactory.ModuleBuilder` 在降级阶段处理。

## 相关文档

- **提取器**: `docs/01-目标/razorvue/render-tree/RenderTreeExtractor.md`
- **降级器**: `docs/01-目标/razorvue/lowering/ArtifactFactory.md`
- **表达式发射器**: `docs/01-目标/razorvue/lowering/ExpressionEmitter.md`

---

**维护者**: developerhan
**最后更新**: 2026-04-21
**版本**: v1.0
