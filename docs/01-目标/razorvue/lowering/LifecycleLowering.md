# RazorVue Lifecycle Lowering

## 概述

RazorVue 的生命周期降级系统负责将 Blazor 的生命周期方法转换为 Vue 3 的组合式 API 钩子。该系统通过深度分析方法体，提取 `InvokeAsync` 调用模式，并生成对应的 Vue 代码。

**相关文件**:
- `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.ModuleBuilder.cs`（主要实现）
- `src/Jazor.RazorVue/Lowering/RazorVueExpressionEmitter.cs`（载荷表达式翻译）

## 完整生命周期映射

| Blazor 方法 | Vue Hook | 调用时机 | 异步支持 | 载荷参数 | HMR 边界 |
|------------|----------|---------|---------|---------|---------|
| `OnInitialized` | `onMounted` | 组件挂载后 | ❌ | 无 | LogicSafe |
| `OnInitializedAsync` | `onMounted` | 组件挂载后 | ✅ | 无 | LogicSafe |
| `OnParametersSet` | `watch(() => [...props], ...)` | Props 变化时 | ❌ | 无 | LogicSafe |
| `OnParametersSetAsync` | `watch(() => [...props], ...)` | Props 变化时 | ✅ | 无 | LogicSafe |
| `SetParametersAsync` | `watch(() => [...props], ...)` | Props 变化时 | ✅ | 无 | LogicSafe |
| `OnAfterRender` | `onMounted` + `onUpdated` | 首次和每次渲染后 | ❌ | `firstRender` | LogicSafe |
| `OnAfterRenderAsync` | `onMounted` + `onUpdated` | 首次和每次渲染后 | ✅ | `firstRender` | LogicSafe |
| `Dispose` | `onUnmounted` | 组件卸载前 | ❌ | 无 | LogicSafe |
| `DisposeAsync` | `onUnmounted` | 组件卸载前 | ✅ | 无 | LogicSafe |
| `ShouldRender` | （不支持） | N/A | N/A | N/A | FullReloadRequired |

## 核心概念

### 1. InvokeAsync 模式识别

RazorVue 仅支持通过 `InvokeAsync` 调用事件回调的生命周期方法：

```csharp
// ✅ 支持的模式
protected override void OnInitialized()
{
    InvokeAsync(() => OnInitialized.Invoke());
}

protected override async Task OnInitializedAsync()
{
    await InvokeAsync(async () => await OnInitializedAsync.Invoke());
}

// ❌ 不支持的模式
protected override void OnInitialized()
{
    OnInitialized.Invoke();  // 缺少 InvokeAsync
}

protected override void OnInitialized()
{
    Console.WriteLine("Init");  // 非 InvokeAsync 调用
}
```

### 2. 载荷表达式翻译

生命周期方法的参数会被翻译为 JavaScript 表达式：

**C# 代码**:
```csharp
protected override void OnAfterRender(bool firstRender)
{
    InvokeAsync(() => OnAfterRender.Invoke(firstRender));
}
```

**JavaScript 结果**:
```javascript
{
  let firstRender = true;
  onMounted(() => {
    const currentFirstRender = firstRender;
    firstRender = false;
    emit("onAfterRender", currentFirstRender);
  });
  onUpdated(() => {
    const currentFirstRender = firstRender;
    firstRender = false;
    emit("onAfterRender", currentFirstRender);
  });
}
```

### 3. Base 方法传递

允许基类方法的纯净传递，避免强制完全重载：

```csharp
// ✅ 支持的纯 base 传递
protected override void OnInitialized() => base.OnInitialized();

protected override async Task OnInitializedAsync()
{
    await base.OnInitializedAsync();
    await Task.CompletedTask;
}

// ❌ 不支持的混合逻辑
protected override void OnInitialized()
{
    Console.WriteLine("Before");
    base.OnInitialized();  // base 调用前后有其他逻辑
}
```

## 深度分析策略

### ExtractSupportedEmitCall 方法

```csharp
private static SupportedEmitCall? ExtractSupportedEmitCall(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    bool allowFirstRenderPayload)
    => ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));
```

**分析流程**:

#### 1. 循环检测

```csharp
if (!visitedMethods.Add(method))
    throw CreateUnsupportedLifecycleLoweringException(method);
```

**作用**: 检测生命周期方法链中的循环引用。

#### 2. 外部默认实现

```csharp
if (method.DeclaringSyntaxReferences.Length == 0)
{
    if (IsDefaultComponentBaseLifecycleMethod(snapshot.Compilation, method))
        return null;  // ComponentBase 的空实现
    throw CreateUnsupportedLifecycleLoweringException(method);
}
```

**处理**: 外部库（如 ComponentBase）的默认实现被识别为无操作。

#### 3. 表达式体方法

```csharp
if (methodSyntax.ExpressionBody is not null)
{
    if (TryExtractBaseLifecycleEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload, visitedMethods, out var baseEmitCall))
        return baseEmitCall;
    return ExtractSupportedEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload);
}
```

**支持的形式**:
```csharp
// 单行 base 传递
protected override void OnInitialized() => base.OnInitialized();

// 单行 emit 调用
protected override void OnInitialized() => InvokeAsync(() => OnInitialized.Invoke());
```

#### 4. 块体方法

**空方法**:
```csharp
if (methodSyntax.Body.Statements.Count == 0)
    return null;
```

**单语句方法**:
```csharp
if (methodSyntax.Body.Statements.Count == 1 &&
    TryExtractBaseLifecycleEmitCall(snapshot, method, methodSyntax.Body.Statements[0], allowFirstRenderPayload, visitedMethods, out var passThroughEmitCall))
{
    return passThroughEmitCall;
}
```

**两语句方法**:
```csharp
if (methodSyntax.Body.Statements.Count == 2 &&
    methodSyntax.Body.Statements[0] is ExpressionStatementSyntax leadingExpression &&
    methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingReturn &&
    (trailingReturn.Expression is null || IsNoOpLifecycleExpression(trailingReturn.Expression)))
{
    return ExtractSupportedEmitCall(snapshot, method, leadingExpression.Expression, allowFirstRenderPayload);
}
```

**支持的形式**:
```csharp
// 前导 emit + 尾随 return
protected override async Task OnInitializedAsync()
{
    await InvokeAsync(() => OnInitializedAsync.Invoke());
    await Task.CompletedTask;
}
```

### 表达式级别提取

```csharp
private static SupportedEmitCall? ExtractSupportedEmitCall(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    ExpressionSyntax expression,
    bool allowFirstRenderPayload)
{
    expression = UnwrapLifecycleExpression(expression);
    if (expression is AwaitExpressionSyntax awaitExpression)
        expression = UnwrapLifecycleExpression(awaitExpression.Expression);
    if (TryUnwrapValueTaskCreation(expression, out var wrappedExpression))
        expression = wrappedExpression;

    if (IsNoOpLifecycleExpression(expression))
        return null;

    // 必须是 InvokeAsync 调用
    if (expression is not InvocationExpressionSyntax invocation ||
        invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
        !string.Equals(memberAccess.Name.Identifier.ValueText, "InvokeAsync", StringComparison.Ordinal) ||
        TryGetLifecycleCallbackName(memberAccess.Expression) is not string callbackName)
    {
        throw CreateUnsupportedLifecycleLoweringException(method);
    }

    var emitName = ToLifecycleEmitName(method, callbackName);
    if (invocation.ArgumentList.Arguments.Count == 0)
        return new SupportedEmitCall(emitName, null, false);

    if (invocation.ArgumentList.Arguments.Count != 1)
        throw CreateUnsupportedLifecycleLoweringException(method);

    var payloadSyntax = UnwrapLifecycleExpression(invocation.ArgumentList.Arguments[0].Expression);
    var semanticModel = snapshot.Compilation.GetSemanticModel(payloadSyntax.SyntaxTree);
    var payloadOperation = semanticModel.GetOperation(payloadSyntax);
    if (payloadOperation is null)
        throw CreateUnsupportedLifecycleLoweringException(method);

    try
    {
        var payload = RazorVueExpressionEmitter.EmitLifecyclePayload(method, payloadOperation, allowFirstRenderPayload);
        return new SupportedEmitCall(emitName, payload.Expression, payload.UsesFirstRender);
    }
    catch (NotSupportedException)
    {
        throw CreateUnsupportedLifecycleLoweringException(method);
    }
}
```

**必需模式**:
```
InvokeAsync(() => CallbackName(...))
InvokeAsync(async () => await CallbackNameAsync(...))
```

## 特殊生命周期方法

### SetParametersAsync 分析

**特点**: 允许在 base 调用前执行用户逻辑。

```csharp
private static SetParametersAsyncAnalysis AnalyzeSetParametersAsync(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol? method,
    HashSet<IMethodSymbol> visitedMethods)
{
    // ... 前置检查 ...

    if (methodSyntax.Body is null)
        return new SetParametersAsyncAnalysis(false, null);

    if (methodSyntax.Body.Statements.Count == 0)
        return new SetParametersAsyncAnalysis(true, null);

    var statements = methodSyntax.Body.Statements;
    var index = 0;
    var sawBaseCall = false;
    SetParametersAsyncAnalysis? baseAnalysis = null;

    // 前导 base 调用
    if (IsBaseSetParametersAsyncStatement(method, statements[0]))
    {
        sawBaseCall = true;
        baseAnalysis = AnalyzeBaseSetParametersAsync(snapshot, method, visitedMethods);
        if (!baseAnalysis.IsSupported)
            return new SetParametersAsyncAnalysis(false, null);
        index++;
    }

    if (index >= statements.Count)
        return sawBaseCall ? baseAnalysis! : new SetParametersAsyncAnalysis(true, null);

    // 用户 emit 调用
    if (TryGetSetParametersAsyncNoOpOrEmit(snapshot, method, statements[index], out var emitCall))
    {
        index++;
        if (index == statements.Count)
        {
            if (emitCall is null)
                return sawBaseCall ? baseAnalysis! : new SetParametersAsyncAnalysis(true, null);

            // 不能堆叠 emit
            return sawBaseCall
                ? baseAnalysis!.EmitCall is null
                    ? new SetParametersAsyncAnalysis(true, emitCall)
                    : new SetParametersAsyncAnalysis(false, null)
                : new SetParametersAsyncAnalysis(false, null);
        }

        // 尾随无操作
        if (index == statements.Count - 1 && IsNoOpSetParametersAsyncStatement(statements[index]))
        {
            if (emitCall is null)
                return sawBaseCall ? baseAnalysis! : new SetParametersAsyncAnalysis(true, null);

            return sawBaseCall
                ? baseAnalysis!.EmitCall is null
                    ? new SetParametersAsyncAnalysis(true, emitCall)
                    : new SetParametersAsyncAnalysis(false, null)
                : new SetParametersAsyncAnalysis(false, null);
        }
    }

    return new SetParametersAsyncAnalysis(false, null);
}
```

**支持的模式**:

```csharp
// 模式 1: 空
public override Task SetParametersAsync(ParameterView parameters)
    => base.SetParametersAsync(parameters);

// 模式 2: base + emit
public override async Task SetParametersAsync(ParameterView parameters)
{
    await base.SetParametersAsync(parameters);
    await InvokeAsync(() => OnParametersSet.Invoke());
}

// 模式 3: base + emit + no-op
public override async Task SetParametersAsync(ParameterView parameters)
{
    await base.SetParametersAsync(parameters);
    await InvokeAsync(() => OnParametersSet.Invoke());
    await Task.CompletedTask;
}
```

**生成的 JavaScript**:
```javascript
watch(() => [props.value], async () => {
  await emit("onParametersSet");
}, { immediate: true });
```

### ShouldRender 分析

**限制**: 仅支持两种形式，其他情况强制完全重载。

```csharp
private static ShouldRenderAnalysis AnalyzeShouldRender(Compilation compilation, IMethodSymbol? method)
{
    if (method is null || method.DeclaringSyntaxReferences.Length == 0)
        return new ShouldRenderAnalysis(false);

    if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
        return new ShouldRenderAnalysis(false);

    if (methodSyntax.ExpressionBody is not null)
        return new ShouldRenderAnalysis(IsSupportedShouldRenderExpression(compilation, methodSyntax.ExpressionBody.Expression));

    if (methodSyntax.Body?.Statements.Count != 1 ||
        methodSyntax.Body.Statements[0] is not ReturnStatementSyntax { Expression: not null } returnStatement)
    {
        return new ShouldRenderAnalysis(false);
    }

    // Constant true 和 base.ShouldRender() 都是显式的默认响应式渲染路径
    return new ShouldRenderAnalysis(IsSupportedShouldRenderExpression(compilation, returnStatement.Expression));
}
```

**支持的形式**:
```csharp
// 形式 1: 常量 true
protected override bool ShouldRender() => true;

// 形式 2: base.ShouldRender()
protected override bool ShouldRender() => base.ShouldRender();
```

## 载荷表达式翻译

### EmitLifecyclePayload 方法

```csharp
internal static LifecyclePayloadEmission EmitLifecyclePayload(IMethodSymbol method, IOperation operation, bool allowFirstRenderPayload)
{
    var current = Unwrap(operation);
    if (current is null)
        throw new NotSupportedException($"RazorVue lifecycle payload is missing an operation in component '{method.ContainingType.ToDisplayString()'.");

    return current switch
    {
        ILiteralOperation literal => new LifecyclePayloadEmission(EmitLiteral(literal), false),
        IDefaultValueOperation defaultValue when IsNullDefaultValue(defaultValue) => new LifecyclePayloadEmission("null", false),
        IParameterReferenceOperation parameter when IsFirstRenderPayloadParameter(method, parameter, allowFirstRenderPayload) =>
            new LifecyclePayloadEmission(LifecycleFirstRenderPlaceholder, true),
        IPropertyReferenceOperation property => EmitLifecyclePayloadPropertyReference(method, property),
        IUnaryOperation unary => EmitLifecyclePayloadUnary(method, unary, allowFirstRenderPayload),
        IBinaryOperation binary => EmitLifecyclePayloadBinary(method, binary, allowFirstRenderPayload),
        IConditionalOperation conditional => EmitLifecyclePayloadConditional(method, conditional, allowFirstRenderPayload),
        IInterpolatedStringOperation interpolated => EmitLifecyclePayloadInterpolatedString(method, interpolated, allowFirstRenderPayload),
        _ => throw new NotSupportedException(
            $"RazorVue lifecycle payload does not support expression '{current.Kind}' in component '{method.ContainingType.ToDisplayString()'.")
    };
}
```

**支持的表达式**:
- **字面量**: `"Hello"`, `42`, `true`
- **null**: `null`
- **firstRender 参数**: `__jazorVueLifecycleFirstRender__`（占位符）
- **组件参数**: `props.propName`
- **一元运算**: `!value`, `-count`
- **二元运算**: `value + 1`, `isActive && isVisible`
- **条件表达式**: `condition ? whenTrue : whenFalse`
- **插值字符串**: `` `Value: ${props.value}` ``

### 属性引用处理

```csharp
private static LifecyclePayloadEmission EmitLifecyclePayloadPropertyReference(
    IMethodSymbol method,
    IPropertyReferenceOperation property)
{
    if (IsCurrentComponentMember(method.ContainingType, property.Property, property.Instance) &&
        IsComponentParameterProperty(property.Property))
    {
        return new LifecyclePayloadEmission("props." + ToLifecyclePropName(property.Property.Name), false);
    }

    throw new NotSupportedException(
        $"RazorVue lifecycle payload only supports component [Parameter] properties. Unsupported member: '{property.Property.Name}'.");
}
```

**限制**: 仅支持 `[Parameter]` 属性引用。

### 占位符替换

在 `AppendAfterRenderHook` 中，占位符被替换为实际变量：

```csharp
var payloadOverride = snapshotsFirstRender
    ? emitCall.PayloadExpression?.Replace(RazorVueExpressionEmitter.LifecycleFirstRenderPlaceholder, "currentFirstRender")
    : null;
```

**转换示例**:
- 原始: `"__jazorVueLifecycleFirstRender__"`
- 替换后: `"currentFirstRender"`

## 内部记录类型

### SupportedEmitCall

```csharp
private sealed record SupportedEmitCall(string EmitName, string? PayloadExpression, bool UsesFirstRender);
```

**字段说明**:
- `EmitName`: Vue 事件名称（如 "onInitialized", "update:value"）
- `PayloadExpression`: JavaScript 载荷表达式
- `UsesFirstRender`: 是否使用 `firstRender` 参数

### SetParametersAsyncAnalysis

```csharp
private sealed record SetParametersAsyncAnalysis(bool IsSupported, SupportedEmitCall? EmitCall);
```

**组合状态**:
- `(false, null)`: 不支持
- `(true, null)`: 支持，但无 emit 调用
- `(true, emitCall)`: 支持，有 emit 调用

### ShouldRenderAnalysis

```csharp
private sealed record ShouldRenderAnalysis(bool IsSupported);
```

## 代码生成示例

### 示例 1: 简单初始化

**C# 代码**:
```csharp
protected override void OnInitialized()
{
    InvokeAsync(() => OnInitialized.Invoke());
}
```

**JavaScript 结果**:
```javascript
onMounted(() => {
  emit("onInitialized");
});
```

### 示例 2: 异步初始化

**C# 代码**:
```csharp
protected override async Task OnInitializedAsync()
{
    await InvokeAsync(async () => await OnInitializedAsync.Invoke());
}
```

**JavaScript 结果**:
```javascript
onMounted(async () => {
  await emit("onInitializedAsync");
});
```

### 示例 3: 参数传递

**C# 代码**:
```csharp
protected override void OnParametersSet()
{
    InvokeAsync(() => OnParametersSet.Invoke());
}
```

**JavaScript 结果**:
```javascript
watch(() => [props.value], () => {
  emit("onParametersSet");
}, { immediate: true });
```

### 示例 4: AfterRender 首次渲染追踪

**C# 代码**:
```csharp
protected override void OnAfterRender(bool firstRender)
{
    InvokeAsync(() => OnAfterRender.Invoke(firstRender));
}
```

**JavaScript 结果**:
```javascript
{
  let firstRender = true;
  onMounted(() => {
    const currentFirstRender = firstRender;
    firstRender = false;
    emit("onAfterRender", currentFirstRender);
  });
  onUpdated(() => {
    const currentFirstRender = firstRender;
    firstRender = false;
    emit("onAfterRender", currentFirstRender);
  });
}
```

### 示例 5: 复杂载荷表达式

**C# 代码**:
```csharp
protected override void OnAfterRender(bool firstRender)
{
    InvokeAsync(() => OnAfterRender.Invoke($"Rendered at {DateTime.Now}, firstRender={firstRender}"));
}
```

**JavaScript 结果**:
```javascript
{
  let firstRender = true;
  onMounted(() => {
    const currentFirstRender = firstRender;
    firstRender = false;
    emit("onAfterRender", `Rendered at ${new Date()}, firstRender=${currentFirstRender}`);
  });
  onUpdated(() => {
    const currentFirstRender = firstRender;
    firstRender = false;
    emit("onAfterRender", `Rendered at ${new Date()}, firstRender=${currentFirstRender}`);
  });
}
```

### 示例 6: SetParametersAsync

**C# 代码**:
```csharp
public override async Task SetParametersAsync(ParameterView parameters)
{
    await base.SetParametersAsync(parameters);
    await InvokeAsync(() => OnParametersSet.Invoke());
}
```

**JavaScript 结果**:
```javascript
watch(() => [props.value], async () => {
  await emit("onParametersSet");
}, { immediate: true });
```

### 示例 7: Dispose

**C# 代码**:
```csharp
protected override void Dispose()
{
    _timer.Dispose();
    InvokeAsync(() => OnDispose.Invoke());
}
```

**JavaScript 结果**:
```javascript
onUnmounted(() => {
  timer.dispose();
  emit("onDispose");
});
```

## 限制与约束

### 1. 仅支持 InvokeAsync 模式

所有生命周期方法必须通过 `InvokeAsync` 调用事件回调：

```csharp
// ❌ 不支持
protected override void OnInitialized()
{
    Console.WriteLine("Init");
    OnInitialized.Invoke();
}

// ✅ 支持
protected override void OnInitialized()
{
    InvokeAsync(() => OnInitialized.Invoke());
}
```

### 2. 不支持复杂逻辑

仅支持简单的 emit 调用，不支持复杂的控制流或业务逻辑：

```csharp
// ❌ 不支持
protected override void OnInitialized()
{
    if (condition)
    {
        InvokeAsync(() => OnInitialized.Invoke());
    }
}

// ✅ 支持
protected override void OnInitialized()
{
    InvokeAsync(() => OnInitialized.Invoke());
}
```

### 3. ShouldRender 限制

仅支持 `return true;` 或 `return base.ShouldRender();`，其他形式强制完全重载。

### 4. SetParametersAsync 限制

不支持在 base 调用和 emit 调用之间插入其他逻辑。

## 错误处理

### 不支持的生命周期降级异常

```csharp
private static RazorVueCompilationIssueException CreateUnsupportedLifecycleLoweringException(IMethodSymbol method)
{
    var originLocation = method.Locations.FirstOrDefault(location => location.IsInSource);
    var origin = originLocation is null
        ? null
        : RazorVueSourceOrigin.FromLocation(originLocation, RazorVueOriginKind.Logic);
    var issue = new RazorVueCompilationIssue(
        RazorVueIssueCode.UnsupportedLifecycleLowering,
        RazorVueIssueSeverity.Error,
        $"RazorVue lifecycle lowering does not support method '{method.Name}' in component '{method.ContainingType.ToDisplayString()}'.",
        ImmutableArray<string>.Empty);
    return new RazorVueCompilationIssueException(issue, method.ContainingType.ToDisplayString(), origin);
}
```

**错误消息**: 明确指出不支持的方法名称和组件名称。

## 相关文档

- **模块构建器**: `docs/01-目标/razorvue/lowering/ModuleBuilder.md`
- **表达式发射器**: `docs/01-目标/razorvue/lowering/ExpressionEmitter.md`
- **组件降级**: `docs/01-目标/razorvue/lowering/ArtifactFactory.md`

---

**维护者**: developerhan
**最后更新**: 2026-04-21
**版本**: v1.0
