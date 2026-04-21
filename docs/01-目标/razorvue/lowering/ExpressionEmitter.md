# RazorVue Expression Emitter

## 概述

**RazorVueExpressionEmitter** 负责将 Roslyn 操作树（`IOperation`）翻译为 JavaScript 表达式。它支持两种翻译模式：
- **渲染时表达式** (`EmitExpression`): 在渲染函数中执行，访问 props 和 setup 状态
- **Setup 时表达式** (`EmitSetupExpression`): 在 setup 函数中执行，仅访问 props

**核心文件**:
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.ExpressionLowering.cs`

## 核心架构

### 构造函数

```csharp
public RazorVueExpressionEmitter(
    RazorVueSemanticSnapshot snapshot,
    ImmutableDictionary<string, string>? componentReferences = null,
    ImmutableDictionary<string, VueComponentDescriptor>? resolvedComponents = null,
    ImmutableDictionary<string, ImmutableDictionary<string, string>>? componentEmitsByRazorAlias = null)
{
    _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

    // 当前组件的 Props、Slots、Emits
    _propsByPublicName = snapshot.Descriptor.Props.ToDictionary(
        prop => prop.PublicName,
        prop => prop,
        StringComparer.Ordinal);
    _slotsByPublicName = snapshot.Descriptor.Slots.ToDictionary(
        slot => slot.PublicName,
        slot => StringComparer.Ordinal);
    _emitsByRazorAlias = snapshot.Descriptor.Emits
        .Where(emit => !string.IsNullOrWhiteSpace(emit.RazorAlias))
        .ToDictionary(
            emit => emit.RazorAlias!,
            emit => emit,
            StringComparer.Ordinal);

    // 子组件的元数据
    _resolvedComponents = resolvedComponents ?? ImmutableDictionary<string, VueComponentDescriptor>.Empty;
    _componentReferences = componentReferences ?? ImmutableDictionary<string, string>.Empty;
    _componentPropsByPublicName = BuildComponentPropsByPublicName(_resolvedComponents);
    _componentSlotsByPublicName = BuildComponentSlotsByPublicName(_resolvedComponents);
    _componentEmitDescriptorsByRazorAlias = BuildComponentEmitDescriptorsByRazorAlias(_resolvedComponents);
    _componentEmitsByRazorAlias = componentEmitsByRazorAlias ?? ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;

    // 用户逻辑（字段和方法）
    _logicFieldsByName = snapshot.Logic.Fields.ToImmutableDictionary(
        field => field.Name,
        field => field,
        StringComparer.Ordinal);
    _logicMethodsByName = snapshot.Logic.Methods
        .GroupBy(method => method.Name, StringComparer.Ordinal)
        .ToImmutableDictionary(
            group => group.Key,
            group => group.ToImmutableArray(),
            StringComparer.Ordinal);

    _requiredSetupFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
    _requiredSetupMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
}
```

**关键数据结构**:
- `_propsByPublicName`: 当前组件的参数映射
- `_componentReferences`: 子组件的 JavaScript 引用名称
- `_requiredSetupFields`: 渲染时引用的字段（需要在 setup 中降级）
- `_requiredSetupMethods`: 渲染时调用的方法（需要在 setup 中降级）

## 渲染时表达式翻译

### EmitExpression 方法

```csharp
private string EmitExpression(IOperation operation)
{
    var current = Unwrap(operation);
    if (current is null)
        return "undefined";

    return current switch
    {
        ILiteralOperation literal => EmitLiteral(literal),
        ILocalReferenceOperation local => local.Local.Name,
        IParameterReferenceOperation parameter => parameter.Parameter.Name,
        IPropertyReferenceOperation property => EmitPropertyReference(property),
        IFieldReferenceOperation field => EmitFieldReference(field),
        IBinaryOperation binary => "(" + EmitExpression(binary.LeftOperand) + " " +
                                   GetBinaryOperator(binary.OperatorKind) + " " +
                                   EmitExpression(binary.RightOperand) + ")",
        IUnaryOperation unary => GetUnaryOperator(unary.OperatorKind) + EmitExpression(unary.Operand),
        IInvocationOperation invocation => EmitInvocation(invocation),
        IInterpolatedStringOperation interpolated => EmitInterpolatedString(interpolated),
        IConditionalOperation conditional when conditional.WhenTrue is not null && conditional.WhenFalse is not null =>
            "(" + EmitExpression(conditional.Condition) + " ? " +
            EmitExpression(conditional.WhenTrue) + " : " +
            EmitExpression(conditional.WhenFalse) + ")",
        IObjectCreationOperation creation when creation.Arguments.Length == 0 => "{}",
        IDefaultValueOperation => "null",
        _ => throw new NotSupportedException(
            $"RazorVue render currently does not support expression '{current.Kind}' in component '{_snapshot.Descriptor.FullName}'.")
    };
}
```

**支持的操作类型**:
1. **字面量**: 数字、字符串、布尔值、null
2. **局部变量**: 循环变量、临时变量
3. **参数**: 方法参数
4. **属性引用**: Props 和对象属性
5. **字段引用**: 组件字段（需要在 setup 中降级）
6. **二元运算**: 算术、比较、逻辑运算
7. **一元运算**: 取反、负号
8. **方法调用**: 组件方法和对象方法
9. **插值字符串**: 模板字符串
10. **条件表达式**: 三元运算符

### 字面量发射

```csharp
private static string EmitLiteral(ILiteralOperation literal)
{
    if (!literal.ConstantValue.HasValue || literal.ConstantValue.Value is null)
        return "null";

    return literal.ConstantValue.Value switch
    {
        string text => ToJavaScriptString(text),
        char ch => ToJavaScriptString(ch.ToString()),
        bool value => value ? "true" : "false",
        float value => value.ToString("R", CultureInfo.InvariantCulture),
        double value => value.ToString("R", CultureInfo.InvariantCulture),
        decimal value => value.ToString(CultureInfo.InvariantCulture),
        sbyte value => value.ToString(CultureInfo.InvariantCulture),
        byte value => value.ToString(CultureInfo.InvariantCulture),
        short value => value.ToString(CultureInfo.InvariantCulture),
        ushort value => value.ToString(CultureInfo.InvariantCulture),
        int value => value.ToString(CultureInfo.InvariantCulture),
        uint value => value.ToString(CultureInfo.InvariantCulture),
        long value => value.ToString(CultureInfo.InvariantCulture),
        ulong value => value.ToString(CultureInfo.InvariantCulture),
        _ => Convert.ToString(literal.ConstantValue.Value, CultureInfo.InvariantCulture) ?? "null"
    };
}
```

**转换示例**:
- C#: `"Hello"` → JS: `"Hello"`
- C#: `'A'` → JS: `"A"`
- C#: `true` → JS: `true`
- C#: `3.14m` → JS: `"3.14"`
- C#: `42L` → JS: `"42"`

### 属性引用发射

```csharp
private string EmitPropertyReference(IPropertyReferenceOperation property)
{
    if (IsCurrentComponentMember(property.Property, property.Instance))
    {
        // 当前组件的参数属性
        if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
            return "props." + prop.Name;

        // 当前组件的槽属性
        if (_slotsByPublicName.TryGetValue(property.Property.Name, out var slot))
        {
            if (slot.IsDefault)
                return "slots.default ? slots.default() : null";
            return "props." + ToLowerCamelCase(property.Property.Name);
        }

        // 当前组件的事件属性
        if (_emitsByRazorAlias.TryGetValue(property.Property.Name, out _))
            return "props." + ToLowerCamelCase(property.Property.Name);

        throw new NotSupportedException(
            $"RazorVue render currently only supports parameter properties in template expressions. Unsupported member: '{property.Property.Name}'.");
    }

    // 其他对象的属性
    return EmitMemberTarget(property.Instance) + "." + property.Property.Name;
}
```

**访问规则**:
- 当前组件的 `[Parameter]` 属性 → `props.propName`
- `ChildContent` 属性 → `slots.default ? slots.default() : null`
- 其他槽属性 → `props.slotName`
- 其他对象的属性 → `instance.propertyName`

### 字段引用发射

```csharp
private string EmitFieldReference(IFieldReferenceOperation field)
{
    if (IsCurrentComponentMember(field.Field, field.Instance))
    {
        if (_logicFieldsByName.ContainsKey(field.Field.Name))
        {
            // 标记为需要在 setup 中降级
            _requiredSetupFields.Add(field.Field);
            // 转换为小驼峰命名
            return ToLowerCamelCase(field.Field.Name);
        }

        throw new NotSupportedException(
            $"RazorVue render currently does not support component field '{field.Field.Name}' in component '{_snapshot.Descriptor.FullName}'.");
    }

    return EmitMemberTarget(field.Instance) + "." + field.Field.Name;
}
```

**关键逻辑**:
1. 检查字段是否在 `Logic.Fields` 列表中
2. 如果是，标记为必需并返回小驼峰命名的变量名
3. 如果不是，抛出异常（不支持其他字段）

### 方法调用发射

```csharp
private string EmitInvocation(IInvocationOperation invocation)
{
    // 委托 Invoke 调用
    if (invocation.Instance is not null && invocation.TargetMethod.Name == "Invoke")
    {
        return EmitExpression(invocation.Instance) + "(" +
               string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
    }

    // 当前组件的方法
    if (IsCurrentComponentMember(invocation.TargetMethod, invocation.Instance))
    {
        // 验证参数数量匹配
        if (invocation.Arguments.Length != invocation.TargetMethod.Parameters.Length)
            throw CreateUnsupportedSetupLogicException(invocation.TargetMethod);

        // 标记为需要在 setup 中降级
        _requiredSetupMethods.Add(invocation.TargetMethod);
        return ToLowerCamelCase(invocation.TargetMethod.Name) + "(" +
               string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
    }

    // 其他对象的方法
    var targetMethodName = GetEmittedMethodName(invocation.TargetMethod);
    var target = invocation.Instance is not null
        ? EmitMemberInvocationTarget(invocation.Instance, targetMethodName, useSetupEmitter: false)
        : targetMethodName;

    return target + "(" + string.Join(", ", invocation.Arguments.Select(argument => EmitExpression(argument.Value))) + ")";
}
```

**调用类型**:
1. **委托 Invoke**: `callback(arg1, arg2)`
2. **组件方法**: `handleClick(arg1, arg2)`（需要在 setup 中降级）
3. **对象方法**: `array.push(item)`, `console.log(message)`

### 插值字符串发射

```csharp
private string EmitInterpolatedString(IInterpolatedStringOperation interpolated)
{
    var builder = new StringBuilder();
    builder.Append('`');
    foreach (var part in interpolated.Parts)
    {
        switch (part)
        {
            case IInterpolatedStringTextOperation text:
                builder.Append(EscapeTemplateText(text.Text.ConstantValue.HasValue && text.Text.ConstantValue.Value is string value ? value : string.Empty));
                break;
            case IInterpolationOperation interpolation:
                builder.Append("${").Append(EmitExpression(interpolation.Expression)).Append('}');
                break;
        }
    }

    builder.Append('`');
    return builder.ToString();
}
```

**C# 示例**:
```csharp
$"Hello, {name}!"
```

**JavaScript 结果**:
```javascript
`Hello, ${name}!`
```

## Setup 时表达式翻译

### EmitSetupExpression 方法

```csharp
internal string EmitSetupExpression(IOperation operation)
{
    var current = Unwrap(operation);
    if (current is null)
        return "undefined";

    return current switch
    {
        ILiteralOperation literal => EmitLiteral(literal),
        ILocalReferenceOperation local => local.Local.Name,
        IParameterReferenceOperation parameter => parameter.Parameter.Name,
        IPropertyReferenceOperation property => EmitSetupPropertyReference(property),
        IFieldReferenceOperation field => EmitSetupFieldReference(field),
        IBinaryOperation binary => "(" + EmitSetupExpression(binary.LeftOperand) + " " +
                                   GetBinaryOperator(binary.OperatorKind) + " " +
                                   EmitSetupExpression(binary.RightOperand) + ")",
        IUnaryOperation unary => GetUnaryOperator(unary.OperatorKind) + EmitSetupExpression(unary.Operand),
        IInvocationOperation invocation => EmitSetupInvocation(invocation),
        IInterpolatedStringOperation interpolated => EmitSetupInterpolatedString(interpolated),
        IConditionalOperation conditional when conditional.WhenTrue is not null && conditional.WhenFalse is not null =>
            "(" + EmitSetupExpression(conditional.Condition) + " ? " +
            EmitSetupExpression(conditional.WhenTrue) + " : " +
            EmitSetupExpression(conditional.WhenFalse) + ")",
        IDefaultValueOperation => "null",
        _ => throw new NotSupportedException(
            $"RazorVue setup-side logic does not support expression '{current.Kind}' in component '{_snapshot.Descriptor.FullName}'.")
    };
}
```

**与渲染时表达式的区别**:
- **更严格的验证**: 仅支持 `[Parameter]` 属性，不支持槽属性
- **不追踪依赖**: 不会修改 `_requiredSetupFields` 或 `_requiredSetupMethods`
- **不同的错误消息**: 明确指出是 setup 逻辑错误

### Setup 属性引用

```csharp
private string EmitSetupPropertyReference(IPropertyReferenceOperation property)
{
    if (IsCurrentComponentMember(property.Property, property.Instance))
    {
        // 仅支持参数属性
        if (_propsByPublicName.TryGetValue(property.Property.Name, out var prop))
            return "props." + prop.Name;

        throw CreateUnsupportedSetupLogicException(
            property.Property,
            $"RazorVue setup-side logic only supports component [Parameter] properties. Unsupported member: '{property.Property.Name}'.");
    }

    return EmitMemberTarget(property.Instance) + "." + property.Property.Name;
}
```

**限制**: Setup 逻辑中只能访问组件参数，不能访问槽属性。

### Setup 字段引用

```csharp
private string EmitSetupFieldReference(IFieldReferenceOperation field)
{
    if (IsCurrentComponentMember(field.Field, field.Instance))
    {
        if (_logicFieldsByName.ContainsKey(field.Field.Name))
        {
            _requiredSetupFields.Add(field.Field);
            return ToLowerCamelCase(field.Field.Name);
        }

        throw CreateUnsupportedSetupLogicException(
            field.Field,
            $"RazorVue setup-side logic does not support component field '{field.Field.Name}'.");
    }

    return EmitMemberTarget(field.Instance) + "." + field.Field.Name;
}
```

## 运算符映射

### 二元运算符

```csharp
private static string GetBinaryOperator(BinaryOperatorKind kind)
    => kind switch
    {
        BinaryOperatorKind.Add => "+",
        BinaryOperatorKind.Subtract => "-",
        BinaryOperatorKind.Multiply => "*",
        BinaryOperatorKind.Divide => "/",
        BinaryOperatorKind.Remainder => "%",
        BinaryOperatorKind.Equals => "===",
        BinaryOperatorKind.NotEquals => "!==",
        BinaryOperatorKind.LessThan => "<",
        BinaryOperatorKind.LessThanOrEqual => "<=",
        BinaryOperatorKind.GreaterThan => ">",
        BinaryOperatorKind.GreaterThanOrEqual => ">=",
        BinaryOperatorKind.ConditionalAnd => "&&",
        BinaryOperatorKind.ConditionalOr => "||",
        BinaryOperatorKind.And => "&",
        BinaryOperatorKind.Or => "|",
        BinaryOperatorKind.ExclusiveOr => "^",
        _ => throw new NotSupportedException($"Unsupported RazorVue binary operator: {kind}.")
    };
```

**C# 到 JavaScript 映射**:
| C# 运算符 | JavaScript 运算符 |
|-----------|------------------|
| `==` | `===` |
| `!=` | `!==` |
| `&&` | `&&` |
| `\|\|` | `\|\|` |
| `+` | `+` |
| `-` | `-` |
| `*` | `*` |
| `/` | `/` |
| `%` | `%` |

### 一元运算符

```csharp
private static string GetUnaryOperator(UnaryOperatorKind kind)
    => kind switch
    {
        UnaryOperatorKind.Not => "!",
        UnaryOperatorKind.Minus => "-",
        UnaryOperatorKind.Plus => "+",
        _ => throw new NotSupportedException($"Unsupported RazorVue unary operator: {kind}.")
    };
```

## 片段发射

### EmitFragment 方法

```csharp
public string EmitFragment(RazorVueRenderFragment fragment)
{
    if (fragment.Children.IsDefaultOrEmpty)
    {
        return _snapshot.Descriptor.Slots.Any(slot => slot.IsDefault)
            ? "slots.default ? slots.default() : null"
            : "null";
    }

    if (fragment.Children.Length == 1)
        return EmitNode(fragment.Children[0]);

    return "[" + string.Join(", ", fragment.Children.Select(EmitNode)) + "]";
}
```

**策略**:
- **空片段**: 返回 `null` 或默认槽调用
- **单节点**: 直接返回节点表达式
- **多节点**: 返回数组表达式

### EmitNode 分发

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

**节点类型映射**:
- **元素节点**: `h("tagName", attrs, children)`
- **组件节点**: `h(ComponentRef, attrs, slots)`
- **文本节点**: `"text"`
- **表达式节点**: 翻译后的表达式
- **槽出口**: `slots.slotName ? slots.slotName() : null`
- **条件节点**: `condition ? whenTrue : whenFalse`
- **循环节点**: `source.map((item) => body)`

## 源码映射

### CollectOrigins 方法

```csharp
public IEnumerable<RazorVueSourceOrigin> CollectOrigins(RazorVueRenderFragment fragment)
{
    foreach (var child in fragment.Children)
    {
        foreach (var origin in CollectOrigins(child))
            yield return origin;
    }
}

private IEnumerable<RazorVueSourceOrigin> CollectOrigins(RazorVueRenderNode node)
{
    // 当前节点的源码位置
    foreach (var origin in node.Origins)
        yield return origin;

    // 递归子节点
    switch (node)
    {
        case RazorVueElementNode element:
            foreach (var attribute in element.Attributes)
            {
                foreach (var origin in attribute.Origins)
                    yield return origin;
            }

            foreach (var childOrigin in CollectOrigins(element.Children))
                yield return childOrigin;
            break;
        case RazorVueComponentNode component:
            foreach (var attribute in component.Attributes)
            {
                foreach (var origin in attribute.Origins)
                    yield return origin;
            }

            foreach (var childOrigin in CollectOrigins(component.Children))
                yield return childOrigin;
            break;
        case RazorVueConditionalNode conditional:
            foreach (var childOrigin in CollectOrigins(conditional.WhenTrue))
                yield return childOrigin;
            foreach (var childOrigin in CollectOrigins(conditional.WhenFalse))
                yield return childOrigin;
            break;
        case RazorVueForEachNode loop:
            foreach (var childOrigin in CollectOrigins(loop.Body))
                yield return childOrigin;
            break;
    }
}
```

**用途**: 收集所有渲染节点的源码位置，用于错误报告和源码映射。

## 依赖追踪

### GetRequiredSetupFields 方法

```csharp
internal ImmutableArray<VueLogicFieldDescriptor> GetRequiredSetupFields()
    => _requiredSetupFields
        .SelectMany(field => _logicFieldsByName.TryGetValue(field.Name, out var candidate) &&
                             SymbolEqualityComparer.Default.Equals(candidate.FieldSymbol, field)
            ? [candidate]
            : ImmutableArray<VueLogicFieldDescriptor>.Empty)
        .Distinct()
        .ToImmutableArray();
```

**返回**: 所有在渲染时引用的字段描述符。

### GetRequiredSetupMethods 方法

```csharp
internal ImmutableArray<VueLogicMethodDescriptor> GetRequiredSetupMethods()
    => _requiredSetupMethods
        .SelectMany(method => _logicMethodsByName.TryGetValue(method.Name, out var candidates)
            ? candidates.Where(candidate => SymbolEqualityComparer.Default.Equals(candidate.MethodSymbol, method))
            : ImmutableArray<VueLogicMethodDescriptor>.Empty)
        .Distinct()
        .ToImmutableArray();
```

**返回**: 所有在渲染时调用的方法描述符。

## 辅助方法

### Unwrap（隐式转换解包）

```csharp
private static IOperation? Unwrap(IOperation? operation)
{
    var current = operation;
    while (current is IConversionOperation conversion && conversion.IsImplicit)
        current = conversion.Operand;

    return current;
}
```

**作用**: 移除编译器插入的隐式转换操作。

### IsCurrentComponentMember

```csharp
private bool IsCurrentComponentMember(ISymbol symbol, IOperation? instance)
{
    for (var current = _snapshot.ComponentSymbol; current is not null; current = current.BaseType)
    {
        if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, current))
            return instance is null || Unwrap(instance) is IInstanceReferenceOperation;
    }

    return false;
}
```

**检查**: 符号是否属于当前组件或其基类。

### IsCallableSlotExpression

```csharp
private static bool IsCallableSlotExpression(IOperation operation)
    => Unwrap(operation)?.Type?.TypeKind == TypeKind.Delegate;
```

**作用**: 检查表达式是否为委托类型（用于槽上下文）。

## 错误处理

### CreateUnsupportedSetupLogicException

```csharp
private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(IMethodSymbol method)
    => CreateUnsupportedSetupLogicException(
        method,
        $"RazorVue setup lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.");

private RazorVueCompilationIssueException CreateUnsupportedSetupLogicException(ISymbol symbol, string message)
{
    var originLocation = symbol.Locations.FirstOrDefault(location => location.IsInSource);
    var origin = originLocation is null
        ? null
        : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
    var ownerComponent = symbol.ContainingType?.ToDisplayString() ?? _snapshot.Descriptor.FullName;
    var issue = new RazorVueCompilationIssue(
        RazorVueIssueCode.UnsupportedSetupLogicLowering,
        RazorVueIssueSeverity.Error,
        message,
        ImmutableArray<string>.Empty);
    return new RazorVueCompilationIssueException(issue, ownerComponent, origin);
}
```

**特点**: 包含源码位置，便于 IDE 显示错误。

## 相关文档

- **组件创作**: `docs/01-目标/razorvue/lowering/ComponentAuthoring.md`
- **形状和映射**: `docs/01-目标/razorvue/lowering/ShapeAndMaps.md`
- **生命周期降级**: `docs/01-目标/razorvue/lowering/LifecycleLowering.md`

---

**维护者**: developerhan
**最后更新**: 2026-04-21
**版本**: v1.0
