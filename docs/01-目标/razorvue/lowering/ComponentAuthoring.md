# RazorVue Component Authoring

## 概述

**RazorVueExpressionEmitter.ComponentAuthoring** 负责将渲染树节点降级为 Vue 组件创作代码（使用 `h()` 函数）。它处理元素节点、组件节点、槽出口、条件渲染和循环渲染。

**核心文件**: `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ComponentAuthoring.cs`

## 核心方法

### EmitNode 分发器

```csharp
private string EmitNode(RazorVueRenderNode node)
    => node switch
    {
        RazorVueElementNode element => EmitElementNode(element),
        RazorVueComponentNode component => EmitComponentNode(component),
        RazorVueTextNode text => ToJavaScriptString(text.Text),
        RazorVueExpressionNode expression => EmitExpression(expression.Expression),
        RazorVueSlotOutletNode slot => EmitSlotOutlet(slot),
        RazorVueConditionalNode conditional => "(" + EmitExpression(conditional.Condition) + " ? " +
                                              EmitFragment(conditional.WhenTrue) + " : " +
                                              EmitFragment(conditional.WhenFalse) + ")",
        RazorVueForEachNode loop => EmitLoop(loop),
        _ => throw new NotSupportedException($"Unsupported RazorVue render node: {node.GetType().Name}.")
    };
```

## 元素节点降级

### EmitElementNode 方法

```csharp
private string EmitElementNode(RazorVueElementNode element)
    => "h(" + ToJavaScriptString(element.TagName) + ", " +
       EmitAttributes(element.Attributes) + ", " +
       EmitFragment(element.Children) + ")";
```

**C# 源码**:
```csharp
builder.OpenElement(0, "div");
builder.AddAttribute(1, "class", "container");
builder.AddContent(2, "Hello");
builder.CloseElement();
```

**JavaScript 结果**:
```javascript
h("div", { class: "container" }, "Hello")
```

**嵌套元素**:
```javascript
h("div", { class: "container" }, [
  h("h1", {}, "Title"),
  h("p", {}, "Content")
])
```

## 组件节点降级

### EmitComponentNode 方法

```csharp
private string EmitComponentNode(RazorVueComponentNode component)
{
    _resolvedComponents.TryGetValue(component.ComponentName, out var descriptor);
    _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);

    // 库组件验证：默认槽使用
    ValidateDefaultLibrarySlotUsage(component, descriptor, slotsByPublicName);

    // 库组件验证：重复槽使用
    ValidateDuplicateLibrarySlotUsage(component, descriptor, slotsByPublicName);

    var slotEntries = new List<string>();
    if (!component.Children.Children.IsDefaultOrEmpty)
        slotEntries.Add("default: () => " + EmitFragment(component.Children));

    var attributes = EmitAttributes(component.Attributes, component, slotEntries);
    var slots = slotEntries.Count == 0
        ? "null"
        : "{ " + string.Join(", ", slotEntries) + " }";

    return "h(" + ResolveComponentReference(component) + ", " + attributes + ", " + slots + ")";
}
```

**C# 源码**:
```csharp
builder.OpenComponent<MyComponent>(0);
builder.AddAttribute(1, "Value", 42);
builder.AddAttribute(2, "ValueChanged", handler);
builder.CloseComponent();
```

**JavaScript 结果**:
```javascript
h(MyComponent, { value: 42, "onUpdate:value": handler }, null)
```

**带槽的组件**:
```javascript
h(CardComponent, { title: "Title" }, {
  default: () => "Content",
  header: () => "Header",
  footer: () => "Footer"
})
```

### 库组件验证

#### ValidateDefaultLibrarySlotUsage 方法

```csharp
private void ValidateDefaultLibrarySlotUsage(
    RazorVueComponentNode component,
    VueComponentDescriptor? descriptor,
    ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName)
{
    var hasDefaultChildren = !component.Children.Children.IsDefaultOrEmpty;
    if (descriptor is null ||
        descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
        !hasDefaultChildren)
    {
        return;
    }

    var origin = CollectOrigins(component.Children).FirstOrDefault() ??
                 component.Origins.FirstOrDefault();

    if (slotsByPublicName is not null &&
        slotsByPublicName.TryGetValue("ChildContent", out var defaultSlotDescriptor))
    {
        if (defaultSlotDescriptor.Parameters.IsDefaultOrEmpty)
            return;

        // 隐式子内容无法满足类型化槽合约
        throw CreateAuthoringIssue(
            RazorVueIssueCode.SlotContextMisuse,
            $"Child content parameter 'ChildContent' on component '{descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(defaultSlotDescriptor)}'.",
            origin);
    }

    throw CreateAuthoringIssue(
        RazorVueIssueCode.UnknownSlot,
        $"Component '{descriptor.Name}' does not declare a child content parameter named 'ChildContent'.",
        origin);
}
```

**作用**: 确保库组件的默认槽使用符合契约。

**错误场景**:
1. 组件未声明 `ChildContent` 槽
2. `ChildContent` 需要上下文参数，但用户传递的是隐式子内容

#### ValidateDuplicateLibrarySlotUsage 方法

```csharp
private void ValidateDuplicateLibrarySlotUsage(
    RazorVueComponentNode component,
    VueComponentDescriptor descriptor,
    ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName)
{
    if (descriptor is null ||
        descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
        slotsByPublicName is null)
    {
        return;
    }

    // 库槽是单赋值创作合约
    var assignedSlots = new HashSet<string>(StringComparer.Ordinal);
    if (!component.Children.Children.IsDefaultOrEmpty &&
        slotsByPublicName.ContainsKey("ChildContent"))
    {
        assignedSlots.Add("ChildContent");
    }

    foreach (var attribute in component.Attributes)
    {
        if (!slotsByPublicName.ContainsKey(attribute.Name))
            continue;

        if (assignedSlots.Add(attribute.Name))
            continue;

        throw CreateAuthoringIssue(
            RazorVueIssueCode.DuplicateSlotValue,
            $"Component '{descriptor.Name}' receives child content parameter '{attribute.Name}' more than once.",
            attribute);
    }
}
```

**作用**: 检测重复的槽赋值。

**错误示例**:
```csharp
// 错误：ChildContent 被赋值两次
builder.OpenComponent<MyComponent>(0);
builder.AddAttribute(1, ChildContent);  // 第一次
builder.CloseComponent();
// 隐式子内容又传递了一次
```

## 槽出口降级

### EmitSlotOutlet 方法

```csharp
private string EmitSlotOutlet(RazorVueSlotOutletNode slot)
{
    if (slot.Argument is null)
        return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "() : null";

    return "slots." + slot.SlotName + " ? slots." + slot.SlotName + "(" + EmitExpression(slot.Argument) + ") : null";
}
```

**C# 源码**:
```csharp
// 无参数槽
builder.AddContent(0, Header);

// 带参数槽
builder.AddContent(1, Row(context));
```

**JavaScript 结果**:
```javascript
// 无参数
slots.header ? slots.header() : null

// 带参数
slots.row ? slots.row(context) : null
```

**设计理念**:
- **安全调用**: 使用可选链避免运行时错误
- **null 回退**: 槽不存在时返回 `null`

## 条件渲染降级

### 条件表达式

```csharp
RazorVueConditionalNode conditional => "(" + EmitExpression(conditional.Condition) + " ? " +
                                      EmitFragment(conditional.WhenTrue) + " : " +
                                      EmitFragment(conditional.WhenFalse) + ")"
```

**C# 源码**:
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

**JavaScript 结果**:
```javascript
(isVisible ? h("div", {}, "Shown") : h("div", {}, "Hidden"))
```

**嵌套条件**:
```javascript
(condition1
  ? (condition2 ? whenTrueTrue : whenTrueFalse)
  : whenFalse)
```

## 循环降级

### EmitLoop 方法

```csharp
private string EmitLoop(RazorVueForEachNode loop)
    => EmitExpression(loop.Source) + ".map((" + loop.ItemName + ") => " + EmitFragment(loop.Body) + ")";
```

**C# 源码**:
```csharp
@foreach (var item in items)
{
    <div>@item.Name</div>
}
```

**JavaScript 结果**:
```javascript
items.map((item) => h("div", {}, item.Name))
```

**嵌套循环**:
```javascript
items.map((item) =>
  item.SubItems.map((subItem) =>
    h("div", {}, subItem.Name)
  )
)
```

**注意**: RazorVue 使用 `.map()` 而不是 `v-for`，因为目标是渲染函数而不是模板。

## 属性降级

### 元素属性

```csharp
private string EmitAttributes(ImmutableArray<RazorVueAttributeNode> attributes)
{
    if (attributes.IsDefaultOrEmpty)
        return "null";

    var entries = attributes.Select(attribute =>
        ToJavaScriptString(attribute.Name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
    return "{ " + string.Join(", ", entries) + " }";
}
```

**C# 源码**:
```csharp
builder.AddAttribute(0, "class", "container");
builder.AddAttribute(1, "disabled", true);
builder.AddAttribute(2, "onclick", handler);
```

**JavaScript 结果**:
```javascript
{
  "class": "container",
  "disabled": true,
  "onclick": handler
}
```

### 组件属性

```csharp
private string EmitAttributes(
    ImmutableArray<RazorVueAttributeNode> attributes,
    RazorVueComponentNode component,
    List<string> slotEntries)
{
    if (attributes.IsDefaultOrEmpty)
        return "null";

    _componentEmitsByRazorAlias.TryGetValue(component.ComponentName, out var emitsByAlias);
    _componentEmitDescriptorsByRazorAlias.TryGetValue(component.ComponentName, out var emitDescriptorsByAlias);
    _componentPropsByPublicName.TryGetValue(component.ComponentName, out var propsByPublicName);
    _componentSlotsByPublicName.TryGetValue(component.ComponentName, out var slotsByPublicName);

    // 库组件是显式创作合约
    ValidateComponentAuthoringAttributes(component, propsByPublicName, slotsByPublicName, emitDescriptorsByAlias);

    var entries = new List<string>();
    foreach (var attribute in attributes)
    {
        // 槽属性
        if (slotsByPublicName is not null &&
            slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor) &&
            attribute.Value is not null)
        {
            var slotName = slotDescriptor.Name;
            var slotExpression = EmitExpression(attribute.Value!);
            if (slotDescriptor.Parameters.IsDefaultOrEmpty || !IsCallableSlotExpression(attribute.Value!))
            {
                slotEntries.Add(slotName + ": () => " + slotExpression);
            }
            else
            {
                // 保留声明的槽上下文名称
                var slotParameterName = slotDescriptor.Parameters[0].Name;
                slotEntries.Add(slotName + ": (" + slotParameterName + ") => " + slotExpression + "(" + slotParameterName + ")");
            }

            continue;
        }

        // 事件处理程序映射
        var name = attribute.Name;
        if (emitsByAlias is not null && emitsByAlias.TryGetValue(name, out var vueEventName))
            name = vueEventName;
        else if (propsByPublicName is not null && propsByPublicName.TryGetValue(name, out var propDescriptor))
            name = propDescriptor.Name;

        entries.Add(ToJavaScriptString(name) + ": " + (attribute.Value is null ? "true" : EmitExpression(attribute.Value!)));
    }

    return entries.Count == 0
        ? "null"
        : "{ " + string.Join(", ", entries) + " }";
}
```

**处理逻辑**:
1. **槽属性**: 转换为 `slotEntries` 列表
2. **事件处理程序**: 映射到 Vue 事件名称（如 `ValueChanged` → `update:value`）
3. **参数属性**: 映射到 Vue prop 名称
4. **布尔属性**: 无值时映射为 `true`

### 组件属性验证

#### ValidateComponentAuthoringAttributes 方法

```csharp
private void ValidateComponentAuthoringAttributes(
    RazorVueComponentNode component,
    ImmutableDictionary<string, VuePropDescriptor>? propsByPublicName,
    ImmutableDictionary<string, VueSlotDescriptor>? slotsByPublicName,
    ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias)
{
    if (!_resolvedComponents.TryGetValue(component.ComponentName, out var descriptor) ||
        descriptor.SourceKind != VueComponentSourceKind.LibraryComponent ||
        component.Attributes.IsDefaultOrEmpty)
    {
        return;
    }

    var attributeNames = component.Attributes
        .Select(attribute => attribute.Name)
        .ToImmutableHashSet(StringComparer.Ordinal);

    // 验证绑定目标
    ValidateInvalidBindTargets(component, descriptor, propsByPublicName, emitsByAlias, attributeNames);

    foreach (var attribute in component.Attributes)
    {
        // 槽上下文验证
        if (slotsByPublicName is not null &&
            slotsByPublicName.TryGetValue(attribute.Name, out var slotDescriptor))
        {
            if (!slotDescriptor.Parameters.IsDefaultOrEmpty &&
                attribute.Value is not null &&
                !IsCallableSlotExpression(attribute.Value))
            {
                throw CreateAuthoringIssue(
                    RazorVueIssueCode.SlotContextMisuse,
                    $"Child content parameter '{attribute.Name}' on component '{descriptor.Name}' expects a callable template that accepts '{DescribeSlotContext(slotDescriptor)}'.",
                    attribute);
            }

            continue;
        }

        // 参数验证
        if (propsByPublicName is not null && propsByPublicName.ContainsKey(attribute.Name))
            continue;

        // 事件验证
        if (emitsByAlias is not null && emitsByAlias.ContainsKey(attribute.Name))
            continue;

        // 槽类型验证
        if (attribute.Value is not null && IsRenderFragmentLike(attribute.Value))
        {
            throw CreateAuthoringIssue(
                RazorVueIssueCode.UnknownSlot,
                $"Component '{descriptor.Name}' does not declare a child content parameter named '{attribute.Name}'.",
                attribute);
        }

        throw CreateAuthoringIssue(
            RazorVueIssueCode.UnknownParameter,
            $"Component '{descriptor.Name}' does not declare a parameter named '{attribute.Name}'.",
            attribute);
    }
}
```

**验证规则**:
1. **绑定目标**: `ValueChanged` 形式的属性需要对应支持双向绑定的参数
2. **槽上下文**: 类型化槽需要可调用的模板表达式
3. **未知参数**: 库组件不允许未声明的参数
4. **未知槽**: `RenderFragment` 类型的值必须是已声明的槽

#### ValidateInvalidBindTargets 方法

```csharp
private void ValidateInvalidBindTargets(
    RazorVueComponentNode component,
    VueComponentDescriptor descriptor,
    ImmutableDictionary<string, VuePropDescriptor>? propsByPublicName,
    ImmutableDictionary<string, VueEmitDescriptor>? emitsByAlias,
    ImmutableHashSet<string> attributeNames)
{
    foreach (var attribute in component.Attributes)
    {
        if (!TryGetBindTargetName(attribute.Name, out var parameterName) ||
            !attributeNames.Contains(parameterName))
        {
            continue;
        }

        var hasBindableProp = propsByPublicName is not null &&
                              propsByPublicName.TryGetValue(parameterName, out var propDescriptor) &&
                              propDescriptor.AcceptsBinding;
        var hasModelUpdateEmit = emitsByAlias is not null &&
                                 emitsByAlias.TryGetValue(attribute.Name, out var emitDescriptor) &&
                                 emitDescriptor.Kind == VueEmitKind.ModelUpdate;

        if (hasBindableProp && hasModelUpdateEmit)
            continue;

        throw CreateAuthoringIssue(
            RazorVueIssueCode.InvalidBindTarget,
            $"Component '{descriptor.Name}' does not support two-way binding for parameter '{parameterName}'.",
            attribute);
    }
}

private static bool TryGetBindTargetName(string attributeName, out string parameterName)
{
    parameterName = string.Empty;
    if (string.IsNullOrWhiteSpace(attributeName) ||
        !attributeName.EndsWith("Changed", StringComparison.Ordinal) ||
        attributeName.Length <= "Changed".Length)
    {
        return false;
    }

    parameterName = attributeName.Substring(0, attributeName.Length - "Changed".Length);
    return !string.IsNullOrWhiteSpace(parameterName);
}
```

**作用**: 验证双向绑定语法（如 `@bind-Value`）是否受支持。

**C# 示例**:
```csharp
// 错误：Value 不支持双向绑定
builder.AddComponent<MyComponent>(0, "Value", 42);
builder.AddComponent<MyComponent>(0, "ValueChanged", handler);
```

## 错误处理

### CreateAuthoringIssue 方法

```csharp
private RazorVueCompilationIssueException CreateAuthoringIssue(
    RazorVueIssueCode code,
    string message,
    RazorVueSourceOrigin? origin)
{
    var issue = new RazorVueCompilationIssue(
        code,
        RazorVueIssueSeverity.Error,
        message,
        ImmutableArray<string>.Empty);
    return new RazorVueCompilationIssueException(issue, _snapshot.Descriptor.FullName, origin);
}
```

### 错误代码

| 错误代码 | 说明 | 示例 |
|---------|------|------|
| `SlotContextMisuse` | 槽上下文参数不匹配 | 传递 `RenderFragment` 给需要 `RenderFragment<T>` 的槽 |
| `UnknownSlot` | 未声明的槽 | 传递 `RenderFragment` 给未声明的参数 |
| `DuplicateSlotValue` | 重复的槽赋值 | 同时使用隐式子内容和显式 `ChildContent` 参数 |
| `UnknownParameter` | 未声明的参数 | 向库组件传递未声明的参数 |
| `InvalidBindTarget` | 无效的双向绑定目标 | 使用 `@bind` 不支持的参数 |

## 辅助方法

### ResolveComponentReference

```csharp
private string ResolveComponentReference(RazorVueComponentNode component)
{
    if (_componentReferences.TryGetValue(component.ComponentName, out var reference))
        return reference;

    throw new NotSupportedException(
        $"RazorVue render could not resolve component node '{component.ComponentName}' in component '{_snapshot.Descriptor.FullName}'.");
}
```

**返回值**:
- Vue 内置组件: 直接名称（如 `"Teleport"`）
- 其他组件: 别名（如 `"MyComponentComponent"`）

### IsRenderFragmentLike

```csharp
private static bool IsRenderFragmentLike(IOperation operation)
{
    if (Unwrap(operation)?.Type is not INamedTypeSymbol namedType)
        return false;

    var definition = namedType.OriginalDefinition;
    var metadataName = definition.ToDisplayString();
    return string.Equals(metadataName, "Microsoft.AspNetCore.Components.RenderFragment", StringComparison.Ordinal) ||
           string.Equals(metadataName, "Microsoft.AspNetCore.Components.RenderFragment<T>", StringComparison.Ordinal);
}
```

**作用**: 检测表达式是否为 `RenderFragment` 或 `RenderFragment<T>` 类型。

## 转换示例

### 示例 1: 简单元素

**C# 代码**:
```csharp
builder.OpenElement(0, "div");
builder.AddAttribute(1, "class", "container");
builder.AddContent(2, "Hello, World!");
builder.CloseElement();
```

**JavaScript 结果**:
```javascript
h("div", { "class": "container" }, "Hello, World!")
```

### 示例 2: 带属性的组件

**C# 代码**:
```csharp
builder.OpenComponent<ButtonComponent>(0);
builder.AddAttribute(1, "Text", "Click me");
builder.AddAttribute(2, "OnClick", handler);
builder.CloseComponent();
```

**JavaScript 结果**:
```javascript
h(ButtonComponent, { "text": "Click me", "onClick": handler }, null)
```

### 示例 3: 条件渲染

**C# 代码**:
```csharp
@if (isLoggedIn)
{
    <div>Welcome</div>
}
else
{
    <div>Please log in</div>
}
```

**JavaScript 结果**:
```javascript
(isLoggedIn ? h("div", {}, "Welcome") : h("div", {}, "Please log in"))
```

### 示例 4: 列表渲染

**C# 代码**:
```csharp
@foreach (var item in items)
{
    <div>@item.Name</div>
}
```

**JavaScript 结果**:
```javascript
items.map((item) => h("div", {}, item.Name))
```

### 示例 5: 带槽的组件

**C# 代码**:
```csharp
builder.OpenComponent<CardComponent>(0);
builder.AddAttribute(1, "Title", "Card Title");
builder.AddAttribute(2, Header, headerContent);
builder.AddAttribute(3, Footer, footerContent);
builder.CloseComponent();
```

**JavaScript 结果**:
```javascript
h(CardComponent, { "title": "Card Title" }, {
  "header": () => headerContent(),
  "footer": () => footerContent()
})
```

## 相关文档

- **表达式发射器**: `docs/01-目标/razorvue/lowering/ExpressionEmitter.md`
- **模块构建器**: `docs/01-目标/razorvue/lowering/ModuleBuilder.md`
- **生命周期降级**: `docs/01-目标/razorvue/lowering/LifecycleLowering.md`

---

**维护者**: developerhan
**最后更新**: 2026-04-21
**版本**: v1.0
