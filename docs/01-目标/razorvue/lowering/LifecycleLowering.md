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
| `ShouldRender` | 不生成 Vue hook，仅参与 HMR/边界分析 | N/A | N/A | 受控子集支持 | `TemplateOnly` 或 `FullReloadRequired` |

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
        return new SupportedEmitCall(emitName, null, false, ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding>.Empty);

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
        return new SupportedEmitCall(emitName, payload.Expression, payload.UsesFirstRender, payload.PreludeBindings);
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

**限制**: 当前只接受受控显式响应式渲染路径；不生成 Vue hook，但会参与 HMR 边界分类。

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

// 形式 2: 直接透传到 ComponentBase
protected override bool ShouldRender() => base.ShouldRender();

// 形式 3: 递归安全 base-pass-through 链
protected override bool ShouldRender() => base.ShouldRender();
```

其中形式 3 的前提是：`base.ShouldRender()` 最终递归解析到另一个同样受支持的 `ShouldRender` 实现，例如抽象基类里的 `return true;`。如果 base 链最终落到动态条件、源码缺失、或出现环引用，仍按 unsupported 处理并要求 `FullReloadRequired`。

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
- **source-stable current-component value member**: 受控 declaration-initialized property / field，或受控 getter-bodied property
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

    if (IsCurrentComponentMember(method.ContainingType, property.Property, property.Instance) &&
        TryEmitLifecycleCurrentComponentPropertyReference(property.Property, out var lifecyclePropertyExpression))
    {
        return new LifecyclePayloadEmission(lifecyclePropertyExpression, false);
    }

    throw new NotSupportedException(
        $"RazorVue lifecycle payload only supports component [Parameter] properties or source-stable current-component value members. Unsupported member: '{property.Property.Name}'.");
}
```

**当前支持的 property 引用合同**:

- `[Parameter]` property 继续直接 lower 为 `props.xxx`
- current-component property 若已进入 logic/setup 主线，也可作为 lifecycle payload 原子

其中 current-component property 当前只开放受控子集：

- declaration-initialized value-like property，且源码可证明 source-stable
- expression-bodied property
- getter accessor 中单个 `return` 的 property
- 上述 getter property 的受控链式依赖

这些 current-component property 不会在 lifecycle lowering 内单独手拼 JS；它们会先沿 setup/property lowering 主线发射为 setup value binding 或 setup function，再在 payload 中以 `prefix` / `readyLabel()` 之类的最终 binding 形式被引用。

### 字段引用处理

current-component field 现也支持一个更窄的受控子集：

- `readonly` declaration-initialized field
- private mutable declaration-initialized field，但必须源码可证明无后续写入

一旦 field/property 后续出现可观察写入、需要更宽 dataflow 推理、或 payload 需要越出当前受控 helper lowering 合同，仍显式回到 unsupported。

### 当前仍不支持

- `async` helper-call payload
- `Task` / `ValueTask` 返回 helper-call payload
- 非精确 arity 的 current-component helper-call payload
- 越出当前 setup helper lowering 合同的一般 current-component method-call payload
- mutable/later-written property / field
- 超出当前 source-stable lifecycle prelude + compiler-owned lowering 合同的更宽动态 payload

### 当前已支持的 helper payload 子集

- 当前组件内 helper / method 调用
- 调用点参数个数与 helper 签名完全一致
- helper 本身继续满足 setup helper lowering 合同：同步、源码可分析、非 `Task` / `ValueTask` 返回、body 可收敛到单表达式 / 单返回
- helper 体内部对 declaration-initialized property / field、getter-bodied property、以及其他同步 helper 的依赖，继续沿同一 setup/property/field/method lowering 主线递归展开

### `firstRender` 的 compiler-owned fallback

当 lifecycle payload 实际引用 `OnAfterRender*` 的 `firstRender` 参数、且表达式形状仍落在当前受控子集内时，RazorVue 现在会在专用 payload 分支之外，再尝试一条 compiler-owned fallback：

- 先把 lifecycle `firstRender` 通过 scoped parameter alias 改写为 `currentFirstRender`
- 再把表达式交回 `EmitSetupExpression(...) -> SemanticWalker -> Jazor.Compiler`
- after-render hook 本身继续保留 `const currentFirstRender = firstRender; firstRender = false;` 的 snapshot 协议

这条 fallback 当前已锁定的典型形态包括：

- `(bool)firstRender`
- `object.Equals(firstRender, true)`
- `object.Equals((bool)firstRender, true)`
- `firstRender.Equals(true)`
- `firstRender == true`
- `bool? alias = firstRender; alias ?? false`
- `firstRender is true`
- `firstRender is false`
- `firstRender is not true`
- `firstRender is not false`
- `firstRender is true or false`
- `firstRender is true and not false`
- `firstRender is bool`
- `firstRender is object`
- `firstRender is bool ready && ready`
- `firstRender switch { ... }`
- 继续满足 setup helper lowering 合同的受控 helper-call payload，例如 `Normalize(firstRender)`
- source-stable tuple deconstruction payload，例如 `var pair = (firstRender, new ReadyState(firstRender)); var (_, readyState) = pair; readyState.Value`
- source-stable local function payload，例如 `bool NormalizeReady(bool value) => value; NormalizeReady(firstRender)`
- source-stable local lambda / delegate payload，例如 `Func<bool, bool> normalizeReady = static value => value; normalizeReady(firstRender)`
- direct structural source-data-carrier member payload，例如 `new ReadyState(firstRender).Value`
- 受控 structural source-data-carrier 深链 payload，例如 `new ReadyEnvelope(new ReadyState(firstRender)).State.Value`
- object-initializer structural source-data-carrier 深链 payload，例如 `new ReadyEnvelope { State = new ReadyState(firstRender) }.State.Value`
- source-stable structural source-data-carrier local/list carrier，例如 `readyEnvelopes[1].State.Value`
- helper-returned structural source-data-carrier 深链 payload，例如 `BuildEnvelope(firstRender).State.Value`
- tuple-carried structural source-data-carrier 深链 payload，例如 `(firstRender, new ReadyState(firstRender)).Item2.Value`
- direct structural source-data-carrier property pattern，例如 `new ReadyEnvelope(new ReadyState(firstRender)) is { State.Value: true }`
- helper-returned structural source-data-carrier property pattern，例如 `BuildEnvelope(firstRender) is { State.Value: true }`
- structural deep-member equals payload，例如 `new ReadyEnvelope(new ReadyState(firstRender)).State.Value.Equals(true)`
- helper-returned equals payload，例如 `BuildReady(firstRender).Value.Equals(true)`
- null-conditional + coalesced structural payload，例如 `(new ReadyEnvelope { State = new ReadyState(firstRender) }.State?.Value) ?? false`

这里不是在 RazorVue 内继续扩手写 CLR/调用拼接；RazorVue 只提供 lifecycle snapshot、source-stable prelude alias，以及参数别名，具体表达式 lowering 仍以现有 `Jazor.Compiler` / whitelist / CLR 模块能力为准。

当前已锁定的典型发射结果包括：

- `object.Equals(firstRender, true)` -> `currentFirstRender === true`
- `object.Equals((bool)firstRender, true)` -> `currentFirstRender === true`
- `firstRender.Equals(true)` -> `currentFirstRender === true`
- `firstRender == true` -> `(currentFirstRender === true)`
- `alias ?? false` -> `currentFirstRender ?? false`
- `firstRender is true` -> `currentFirstRender === true`
- `firstRender is false` -> `currentFirstRender === false`
- `firstRender is bool` -> `typeof currentFirstRender === "boolean"`
- `firstRender is bool ready && ready` -> compiler-owned declaration-pattern lowering with pattern local binding
- `var pair = (firstRender, new ReadyState(firstRender)); var (_, readyState) = pair; readyState.Value` -> source-stable tuple deconstruction prelude + compiler-owned tuple/local lowering
- `bool NormalizeReady(bool value) => value; NormalizeReady(firstRender)` -> prelude local-function alias + compiler-lowered invocation
- `Func<bool, bool> normalizeReady = static value => value; normalizeReady(firstRender)` -> prelude delegate alias + compiler-lowered invocation
- `new ReadyState(firstRender).Value` -> `{ value: currentFirstRender }.value`
- `new ReadyEnvelope(new ReadyState(firstRender)).State.Value` -> `{ state: { value: currentFirstRender } }.state.value`
- `new ReadyEnvelope { State = new ReadyState(firstRender) }.State.Value` -> `{ state: { value: currentFirstRender } }.state.value`
- `readyEnvelopes[1].State.Value` -> structural carrier literal + existing `List<T>.this[int].get` helper + `.state.value`
- `BuildEnvelope(firstRender).State.Value` -> `buildEnvelope(currentFirstRender).state.value`
- `(firstRender, new ReadyState(firstRender)).Item2.Value` -> tuple-view literal + `.item2.value` based on current static tuple element names
- `new ReadyEnvelope(new ReadyState(firstRender)) is { State.Value: true }` -> compiler-owned single-evaluation structural property-pattern lowering with `__patin$...` temp
- `BuildEnvelope(firstRender) is { State.Value: true }` -> compiler-owned single-evaluation structural property-pattern lowering against helper result temp

这些 structural property-pattern 路径的合同仍然是保守的：

- 单次求值和求值顺序由 compiler-owned temp 保证，helper 返回值不会被重复调用
- 这里只开放可诚实擦除为 structural value 的 source-data-carrier；不是重新引入 nominal/runtime type 语义
- tuple payload 仍遵循当前编译器的 tuple runtime-shape 合同，字段名取当前静态视图而不是强行重写成另一套 RazorVue 私有命名
- bare nominal type pattern、runtime type token、以及超出当前 structural carrier 合同的更宽 runtime type 路径仍显式 unsupported

对 source-stable lifecycle prelude 这条线，同样保持受控合同：

- local function 不会被误判成 current-component helper；它们会在 lifecycle prelude 中以稳定 alias 发射真实 compiler-lowered function declaration
- local lambda / delegate local 只接受源码可恢复初始化器的 source-stable callable local，并在 prelude 中发射 compiler-lowered `const` alias
- tuple deconstruction 不是 RazorVue 私造投影协议；它继续复用编译器现有 tuple / deconstruction lowering 语义
- 一旦 local function / lambda / tuple local 的依赖越出 source-stable + compiler-owned lowering 合同，仍显式回到 unsupported

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
private sealed record SupportedEmitCall(
    string EmitName,
    string? PayloadExpression,
    bool UsesFirstRender,
    ImmutableArray<RazorVueExpressionEmitter.LifecyclePayloadPreludeBinding> PreludeBindings);
```

**字段说明**:
- `EmitName`: Vue 事件名称（如 "onInitialized", "update:value"）
- `PayloadExpression`: JavaScript 载荷表达式
- `UsesFirstRender`: 是否使用 `firstRender` 参数
- `PreludeBindings`: 需要在 emit 前先发射的 source-stable lifecycle prelude 绑定（例如 local function / delegate alias / deconstruction local）

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

当前只支持以下受控子集：

- `return true;`
- `return base.ShouldRender();`，且最终解析到 `ComponentBase.ShouldRender()`
- `return base.ShouldRender();`，且最终递归解析到另一个同样受支持的 base `ShouldRender` 实现

以下情况仍显式 unsupported：

- 动态条件，例如 `return Value > 0;`
- base 链形成环
- base 实现缺少可分析源码
- 超出单返回表达式受控子集的 body 形状

### 4. SetParametersAsync 限制

当前仅支持受控子集：

- no-op
- expression-bodied 或 statement-bodied 的 `base.SetParametersAsync(parameters)` pass-through
- expression-bodied no-op，例如 `=> Task.CompletedTask`
- `base.SetParametersAsync(parameters)` 后接单个受支持 `InvokeAsync` emit

并且 no-op 会按真实返回类型判定：

- `Task` 返回方法只接受 `Task.CompletedTask` 这类真实 completed-task 形态
- non-generic `ValueTask` 返回方法可接受 `default` / `default(ValueTask)` / `new ValueTask(...)` 的等价 no-op
- `Task` 返回的 `=> default` 不再被视为 no-op

当前仍不支持：

- pass-through 最终落到外部无源码 override（`ComponentBase.SetParametersAsync(...)` 默认实现除外）
- 在 base 调用和 emit 调用之间插入额外 mutation/控制流
- 重复 emit
- 更一般的方法体执行模型

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
**最后更新**: 2026-05-21
**版本**: v1.0

