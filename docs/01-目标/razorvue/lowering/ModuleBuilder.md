# RazorVue Module Builder

## 概述

**RazorVueArtifactFactory.ModuleBuilder** 负责生成完整的 Vue SFC（单文件组件）模块代码。它将 Blazor 组件的渲染树、生命周期方法和用户逻辑降级为 Vue 3 的 `defineComponent` 结构。

**核心文件**: `src/Jazor.RazorVue/Lowering/RazorVueArtifactFactory.ModuleBuilder.cs`（约 1100 行）

## 生成的模块结构

### 标准 Vue SFC 模块

```javascript
import { defineComponent, h, watch, onMounted, onUpdated, onUnmounted } from "vue";
import ButtonComponent from "./Button.mjs";
import CardComponent from "./Card.mjs";

export default defineComponent({
  name: "MyComponent",
  props: ["value", "onClick"],
  emits: ["update:value"],
  setup(props, { emit, slots, expose, attrs }) {
    // 用户逻辑降级
    const count = 0;
    const handleClick = () => { ... };

    // 生命周期降级
    onMounted(() => { ... });
    onUpdated(() => { ... });
    onUnmounted(() => { ... });

    // 渲染函数
    return () => h("div", { class: "container" }, [...]);
  }
});
```

## 核心方法：BuildModuleCode

```csharp
private static string BuildModuleCode(
    RazorVueSemanticSnapshot snapshot,
    RazorVueRenderFragment renderTree,
    RazorVueExpressionEmitter expressionEmitter,
    ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    var descriptor = snapshot.Descriptor;
    var builder = new StringBuilder();

    // 1. 追加 Vue API 导入
    AppendVueImports(builder, snapshot, resolvedComponents);

    // 2. 发射渲染表达式
    var renderExpression = expressionEmitter.EmitFragment(renderTree);

    // 3. 构建组件定义
    builder.AppendLine();
    builder.AppendLine("export default defineComponent({");
    builder.Append("  name: \"").Append(descriptor.Name).AppendLine("\",");
    builder.Append("  props: ").Append(FormatStringArray(descriptor.Props.Select(prop => prop.Name))).AppendLine(",");
    builder.Append("  emits: ").Append(FormatStringArray(descriptor.Emits.Select(emit => emit.Name))).AppendLine(",");

    // 4. Setup 函数
    builder.AppendLine("  setup(props, { emit, slots, expose, attrs }) {");

    // 5. 先规划 lifecycle，并在规划阶段收集 payload 触发的 setup 依赖
    var lifecyclePlan = CreateLifecyclePlan(snapshot, expressionEmitter);

    // 6. 先发射 setup 逻辑，确保 immediate watch/hook 引用的 binding 已经声明
    AppendSetupLogicLowering(
        builder,
        snapshot,
        expressionEmitter,
        lifecyclePlan.RequiredProperties,
        lifecyclePlan.RequiredFields,
        lifecyclePlan.RequiredMethods);

    // 7. 再发射 lifecycle hook / watch
    AppendLifecycleLowering(builder, lifecyclePlan);

    // 8. 返回渲染函数
    builder.Append("    return () => ").Append(renderExpression).AppendLine(";");
    builder.AppendLine("  }");
    builder.AppendLine("});");

    return builder.ToString();
}
```

## Vue API 导入

### AppendVueImports 方法

```csharp
private static void AppendVueImports(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot,
    ImmutableDictionary<string, VueComponentDescriptor> resolvedComponents)
{
    var vueImports = new List<string> { "defineComponent", "h" };

    // 条件导入：仅当实际使用时才导入
    var hasInitializedLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedMethod, false) ||
                                 HasSupportedLifecycleLowering(snapshot, snapshot.OnInitializedAsyncMethod, false);
    if (hasInitializedLowering)
        vueImports.Add("onMounted");

    var hasParametersSetLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetMethod, false) ||
                                   HasSupportedLifecycleLowering(snapshot, snapshot.OnParametersSetAsyncMethod, false) ||
                                   HasSupportedSetParametersAsyncLowering(snapshot);
    if (hasParametersSetLowering)
        vueImports.Add("watch");

    var hasAfterRenderLowering = HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderMethod, true) ||
                                 HasSupportedLifecycleLowering(snapshot, snapshot.OnAfterRenderAsyncMethod, true);
    if (hasAfterRenderLowering)
    {
        vueImports.Add("onMounted");
        vueImports.Add("onUpdated");
    }

    var hasDisposeLowering = HasSupportedLifecycleLowering(snapshot, snapshot.DisposeMethod, false) ||
                             HasSupportedLifecycleLowering(snapshot, snapshot.DisposeAsyncMethod, false);
    if (hasDisposeLowering)
        vueImports.Add("onUnmounted");

    builder.Append("import { ")
        .Append(string.Join(", ", vueImports.Distinct(StringComparer.Ordinal)))
        .AppendLine(" } from \"vue\";");

    AppendComponentImports(builder, resolvedComponents);
}
```

**按需导入原则**: 避免导入未使用的 Vue API，减少包大小。

## 生命周期降级

### Blazor 到 Vue 生命周期映射

| Blazor 方法 | Vue Hook | 参数处理 | 异步支持 |
|------------|----------|---------|---------|
| `OnInitialized` | `onMounted` | 无 | ❌ |
| `OnInitializedAsync` | `onMounted` | 无 | ✅ |
| `OnParametersSet` | `watch(() => [...props], ...)` | Props 监听 | ❌ |
| `OnParametersSetAsync` | `watch(() => [...props], ...)` | Props 监听 | ✅ |
| `SetParametersAsync` | `watch(() => [...props], ...)` | Props 监听 | ✅ |
| `OnAfterRender` | `onMounted` + `onUpdated` | `firstRender` 参数 | ❌ |
| `OnAfterRenderAsync` | `onMounted` + `onUpdated` | `firstRender` 参数 | ✅ |
| `Dispose` | `onUnmounted` | 无 | ❌ |
| `DisposeAsync` | `onUnmounted` | 无 | ✅ |
| `ShouldRender` | 不生成 Vue hook，仅参与 HMR/边界分析 | N/A | 受控子集支持 |

### AppendLifecycleLowering 方法

```csharp
var lifecyclePlan = RazorVueSetupAndLifecycleLoweringSupport.CreateLifecyclePlan(snapshot, expressionEmitter);
AppendSetupLogicLowering(
    setupBodyBuilder,
    snapshot,
    expressionEmitter,
    lifecyclePlan.RequiredProperties,
    lifecyclePlan.RequiredFields,
    lifecyclePlan.RequiredMethods);
AppendLifecycleLowering(setupBodyBuilder, lifecyclePlan, "    ");
```

**当前 contract**:

1. 先创建 lifecycle plan
2. plan 创建阶段会先探测 lifecycle payload lowering，并顺带收集它触发的 setup property/field/method 依赖
3. 先发射这些 setup bindings/functions
4. 再注册 `watch(..., { immediate: true })`、`onMounted`、`onUpdated`、`onUnmounted`

这条顺序是生产级语义要求，不是简单重构：`OnParametersSet*` / `SetParametersAsync` 会 lower 到 `watch(..., { immediate: true })`；若 watch 先于 setup binding 发射，就可能在第一次 immediate 执行时触发 TDZ / 初始化顺序问题。

### 简单生命周期钩子

```csharp
private static void AppendLifecycleHook(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot,
    string hookName,
    IMethodSymbol? method,
    bool awaitResult)
{
    if (method is null)
        return;

    var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload: false);
    if (emitCall is null)
        return;

    builder.Append("    ").Append(hookName).Append("(");
    if (awaitResult)
        builder.Append("async ");
    builder.AppendLine("() => {");
    AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null);
    builder.AppendLine("    });");
}
```

**生成示例**:
```javascript
onMounted(() => {
  emit("onInitialized");
});

onMounted(async () => {
  await emit("onInitializedAsync", props.value);
});
```

### 参数设置钩子

```csharp
private static void AppendParametersSetHook(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol? method,
    bool awaitResult)
{
    if (method is null)
        return;

    var emitCall = ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload: false);
    if (emitCall is null)
        return;

    builder.Append("    watch(() => ").Append(BuildPropsWatchSource(snapshot.Descriptor)).Append(", ");
    if (awaitResult)
        builder.Append("async ");
    builder.AppendLine("() => {");
    AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride: null);
    builder.AppendLine("    }, { immediate: true });");
}
```

**生成示例**:
```javascript
watch(() => [props.value], () => {
  emit("onParametersSet");
}, { immediate: true });
```

### SetParametersAsync 钩子

```csharp
private static void AppendSetParametersAsyncHook(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot)
{
    var analysis = AnalyzeSetParametersAsync(snapshot, snapshot.SetParametersAsyncMethod);
    if (!analysis.IsSupported || analysis.EmitCall is null)
        return;

    builder.Append("    watch(() => ").AppendBuildPropsWatchSource(snapshot.Descriptor)).Append(", async () => {").AppendLine();
    AppendEmitStatement(builder, analysis.EmitCall, awaitResult: true, payloadOverride: null);
    builder.AppendLine("    }, { immediate: true });");
}
```

**特殊之处**: `SetParametersAsync` 总是异步的，且允许在 base 调用前执行逻辑。

**顺序要求**: 若 `SetParametersAsync` / `OnParametersSet*` payload 依赖当前组件 setup member，这些 binding 必须先于 `watch(..., { immediate: true })` 注册发射；当前 module builder 已通过 lifecycle planning 固定这一顺序。

### AfterRender 双钩子模式

```csharp
private static void AppendAfterRenderHook(
    StringBuilder builder,
    SupportedEmitCall? emitCall,
    bool awaitResult)
{
    if (emitCall is null)
        return;

    var snapshotsFirstRender = emitCall.UsesFirstRender;
    var payloadOverride = snapshotsFirstRender
        ? emitCall.PayloadExpression?.Replace(RazorVueExpressionEmitter.LifecycleFirstRenderPlaceholder, "currentFirstRender")
        : null;

    // onMounted 钩子
    builder.Append("    onMounted(");
    if (awaitResult)
        builder.Append("async ");
    builder.AppendLine("() => {");
    if (snapshotsFirstRender)
        builder.AppendLine("      const currentFirstRender = firstRender;");
    if (awaitResult && snapshotsFirstRender)
        builder.AppendLine("      firstRender = false;");
    AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride);
    if (!awaitResult && snapshotsFirstRender)
        builder.AppendLine("      firstRender = false;");
    builder.AppendLine("    });");

    // onUpdated 钩子
    builder.Append("    onUpdated(");
    if (awaitResult)
        builder.Append("async ");
    builder.AppendLine("() => {");
    if (snapshotsFirstRender)
        builder.AppendLine("      const currentFirstRender = firstRender;");
    if (awaitResult && snapshotsFirstRender)
        builder.AppendLine("      firstRender = false;");
    AppendEmitStatement(builder, emitCall, awaitResult, payloadOverride);
    if (!awaitResult && snapshotsFirstRender)
        builder.AppendLine("      firstRender = false;");
    builder.AppendLine("    });");
}
```

**生成示例**:
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

## 用户逻辑降级

### AppendSetupLogicLowering 方法

```csharp
private static void AppendSetupLogicLowering(
    StringBuilder builder,
    RazorVueSemanticSnapshot snapshot,
    RazorVueExpressionEmitter expressionEmitter)
{
    // 当前实现按 symbol identity 递归展开 property / field / method 依赖，
    // 直到没有新依赖为止；循环依赖会显式 fail-fast。
}
```

**依赖解析策略**:
1. 收集所有被渲染树引用的字段和方法
2. 同时把 lifecycle payload 触发的 setup property/field/method 依赖并入同一依赖图
3. 递归解析依赖链（property -> field -> helper、helper -> helper、lifecycle payload -> setup binding）
4. 若成员之间形成循环依赖，显式 fail-fast

**当前 contract**:

- 旧的“helper 最多两层组合”人工深度上限已经移除
- 当前支持的是“源码可分析、同步、单表达式 / 单返回”的 helper/property 链递归展开
- lifecycle payload 命中同一受控 helper/property/field 子集时，会把这些依赖并入同一 setup 依赖图，并保证 setup binding / function 仍先于 watch/hook 发射
- `async` helper、`Task` / `ValueTask` 返回 helper、或超出当前受控 body 形状的方法体，仍显式 unsupported

### 字段降级

```csharp
private static string BuildSetupFieldLowering(
    RazorVueSemanticSnapshot snapshot,
    RazorVueExpressionEmitter expressionEmitter,
    VueLogicFieldDescriptor field)
{
    if (field.FieldSymbol.DeclaringSyntaxReferences.Length == 0)
        throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);

    var syntax = field.FieldSymbol.DeclaringSyntaxReferences[0].GetSyntax();
    if (syntax is not VariableDeclaratorSyntax declarator || declarator.Initializer is null)
        throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);

    var semanticModel = snapshot.Compilation.GetSemanticModel(declarator.SyntaxTree);
    var operation = semanticModel.GetOperation(declarator.Initializer.Value);
    if (operation is null)
        throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);

    try
    {
        var expression = expressionEmitter.EmitSetupExpression(operation);
        var fieldBuilder = new StringBuilder();
        fieldBuilder.Append("    ")
            .Append(field.IsReadOnly ? "const " : "let ")
            .Append(ToLowerCamelCase(field.Name))
            .Append(" = ")
            .Append(expression)
            .AppendLine(";");
        return fieldBuilder.ToString();
    }
    catch (NotSupportedException)
    {
        throw CreateUnsupportedSetupLoweringException(field.FieldSymbol);
    }
}
```

**C# 源码**:
```csharp
private int count = 0;
private readonly string title = "Hello";
```

**降级结果**:
```javascript
const count = 0;
const title = "Hello";
```

### 方法降级

```csharp
private static string BuildSetupMethodLowering(
    RazorVueSemanticSnapshot snapshot,
    RazorVueExpressionEmitter expressionEmitter,
    VueLogicMethodDescriptor method)
{
    if (method.IsAsync || method.MethodSymbol.DeclaringSyntaxReferences.Length == 0)
        throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);

    var syntax = method.MethodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
    if (syntax is not MethodDeclarationSyntax methodSyntax)
        throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);

    ExpressionSyntax expressionSyntax = methodSyntax.ExpressionBody?.Expression
        ?? (methodSyntax.Body?.Statements.Count == 1 &&
            methodSyntax.Body.Statements[0] is ReturnStatementSyntax returnStatement &&
            returnStatement.Expression is not null
                ? returnStatement.Expression
                : throw CreateUnsupportedSetupLoweringException(method.MethodSymbol));

    var semanticModel = snapshot.Compilation.GetSemanticModel(expressionSyntax.SyntaxTree);
    var operation = semanticModel.GetOperation(expressionSyntax);
    if (operation is null)
        throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);

    try
    {
        var expression = expressionEmitter.EmitSetupExpression(operation);
        var methodBuilder = new StringBuilder();
        methodBuilder.Append("    function ")
            .Append(ToLowerCamelCase(method.Name))
            .Append('(')
            .Append(string.Join(", ", method.MethodSymbol.Parameters.Select(parameter => parameter.Name)))
            .AppendLine(") {");
        methodBuilder.Append("      return ")
            .Append(expression)
            .AppendLine(";");
        methodBuilder.AppendLine("    }");
        return methodBuilder.ToString();
    }
    catch (NotSupportedException)
    {
        throw CreateUnsupportedSetupLoweringException(method.MethodSymbol);
    }
}
```

**C# 源码**:
```csharp
private int Increment() => count + 1;
private string Greet(string name) => $"Hello, {name}";
```

**降级结果**:
```javascript
function increment() {
  return count + 1;
}
function greet(name) {
  return `Hello, ${name}`;
}
```

## 生命周期方法分析

### ExtractSupportedEmitCall 方法

```csharp
private static SupportedEmitCall? ExtractSupportedEmitCall(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    bool allowFirstRenderPayload)
    => ExtractSupportedEmitCall(snapshot, method, allowFirstRenderPayload, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

private static SupportedEmitCall? ExtractSupportedEmitCall(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    bool allowFirstRenderPayload,
    HashSet<IMethodSymbol> visitedMethods)
{
    // 1. 循环检测
    if (!visitedMethods.Add(method))
        throw CreateUnsupportedLifecycleLoweringException(method);

    // 2. 外部默认实现
    if (method.DeclaringSyntaxReferences.Length == 0)
    {
        if (IsDefaultComponentBaseLifecycleMethod(snapshot.Compilation, method))
            return null;
        throw CreateUnsupportedLifecycleLoweringException(method);
    }

    // 3. 解析方法体
    var reference = method.DeclaringSyntaxReferences[0];
    if (reference.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
        throw CreateUnsupportedLifecycleLoweringException(method);

    // 4. 表达式体
    if (methodSyntax.ExpressionBody is not null)
    {
        if (TryExtractBaseLifecycleEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload, visitedMethods, out var baseEmitCall))
            return baseEmitCall;
        return ExtractSupportedEmitCall(snapshot, method, methodSyntax.ExpressionBody.Expression, allowFirstRenderPayload);
    }

    // 5. 块体
    if (methodSyntax.Body is null)
        throw CreateUnsupportedLifecycleLoweringException(method);

    // 空方法
    if (methodSyntax.Body.Statements.Count == 0)
        return null;

    // 单语句纯 base 传递
    if (methodSyntax.Body.Statements.Count == 1 &&
        TryExtractBaseLifecycleEmitCall(snapshot, method, methodSyntax.Body.Statements[0], allowFirstRenderPayload, visitedMethods, out var passThroughEmitCall))
    {
        return passThroughEmitCall;
    }

    // 两语句：前导表达式 + 尾随 return/default
    if (methodSyntax.Body.Statements.Count == 2 &&
        methodSyntax.Body.Statements[0] is ExpressionStatementSyntax leadingExpression &&
        methodSyntax.Body.Statements[1] is ReturnStatementSyntax trailingReturn &&
        (trailingReturn.Expression is null || IsNoOpLifecycleExpression(trailingReturn.Expression)))
    {
        return ExtractSupportedEmitCall(snapshot, method, leadingExpression.Expression, allowFirstRenderPayload);
    }

    // 单语句
    if (methodSyntax.Body.Statements.Count != 1)
        throw CreateUnsupportedLifecycleLoweringException(method);

    return methodSyntax.Body.Statements[0] switch
    {
        ExpressionStatementSyntax expressionStatement => ExtractSupportedEmitCall(snapshot, method, expressionStatement.Expression, allowFirstRenderPayload),
        ReturnStatementSyntax returnStatement when returnStatement.Expression is null || IsNoOpLifecycleExpression(returnStatement.Expression) => null,
        ReturnStatementSyntax returnStatement when returnStatement.Expression is not null => ExtractSupportedEmitCall(snapshot, method, returnStatement.Expression, allowFirstRenderPayload),
        _ => throw CreateUnsupportedLifecycleLoweringException(method)
    };
}
```

**支持的代码模式**:

1. **空方法**:
   ```csharp
   protected override void OnInitialized() { }
   ```

2. **纯 base 传递**:
   ```csharp
   protected override void OnInitialized() => base.OnInitialized();
   ```

3. **单 emit 调用**:
   ```csharp
   protected override void OnInitialized()
   {
       InvokeAsync(() => OnInitialized.Invoke());
   }
   ```

4. **前导 emit + 尾随 return**:
   ```csharp
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

**必需模式**: `InvokeAsync(() => CallbackName(...))`

### 无操作表达式检测

```csharp
private static bool IsNoOpLifecycleExpression(ExpressionSyntax syntax)
{
    syntax = UnwrapLifecycleExpression(syntax);
    if (syntax is AwaitExpressionSyntax awaitExpression)
        syntax = UnwrapLifecycleExpression(awaitExpression.Expression);

    var expressionText = syntax.ToString().Trim();
    return string.Equals(expressionText, "Task.CompletedTask", StringComparison.Ordinal) ||
           string.Equals(expressionText, "ValueTask.CompletedTask", StringComparison.Ordinal) ||
           string.Equals(expressionText, "default", StringComparison.Ordinal) ||
           string.Equals(expressionText, "default(ValueTask)", StringComparison.Ordinal) ||
           string.Equals(expressionText, "default(System.Threading.Tasks.ValueTask)", StringComparison.Ordinal);
}
```

### Base 传递提取

```csharp
private static bool TryExtractBaseLifecycleEmitCall(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol method,
    ExpressionSyntax expression,
    bool allowFirstRenderPayload,
    HashSet<IMethodSymbol> visitedMethods,
    out SupportedEmitCall? emitCall)
{
    emitCall = null;
    if (!IsBaseLifecyclePassThroughCall(method, expression))
        return false;

    var baseMethod = FindBaseLifecycleMethod(method);
    if (baseMethod is null)
        throw CreateUnsupportedLifecycleLoweringException(method);

    if (baseMethod.DeclaringSyntaxReferences.Length == 0)
    {
        if (IsComponentBaseNoOpLifecycle(snapshot.Compilation, baseMethod))
        {
            emitCall = null;
            return true;
        }
        throw CreateUnsupportedLifecycleLoweringException(method);
    }

    emitCall = ExtractSupportedEmitCall(snapshot, baseMethod, allowFirstRenderPayload, visitedMethods);
    return true;
}
```

**作用**: 递归提取基类方法的 emit 调用，避免强制完全重载。

### SetParametersAsync 分析

```csharp
private static SetParametersAsyncAnalysis AnalyzeSetParametersAsync(
    RazorVueSemanticSnapshot snapshot,
    IMethodSymbol? method,
    HashSet<IMethodSymbol> visitedMethods)
{
    if (method is null || !visitedMethods.Add(method))
        return new SetParametersAsyncAnalysis(false, null);

    if (method.DeclaringSyntaxReferences.Length == 0)
        return new SetParametersAsyncAnalysis(false, null);

    if (method.DeclaringSyntaxReferences[0].GetSyntax() is not MethodDeclarationSyntax methodSyntax)
        return new SetParametersAsyncAnalysis(false, null);

    // 表达式体
    if (methodSyntax.ExpressionBody is not null)
    {
        return IsBaseSetParametersAsyncCall(method, methodSyntax.ExpressionBody.Expression)
            ? AnalyzeBaseSetParametersAsync(snapshot, method, visitedMethods)
            : new SetParametersAsyncAnalysis(false, null);
    }

    // 块体
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

### ShouldRender 分析

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

private static bool IsSupportedShouldRenderExpression(Compilation compilation, ExpressionSyntax expression)
{
    expression = UnwrapLifecycleExpression(expression);
    if (IsConstantTrueShouldRenderExpression(expression))
        return true;

    if (expression is not InvocationExpressionSyntax invocationExpression ||
        invocationExpression.Expression is not MemberAccessExpressionSyntax
        {
            Expression: BaseExpressionSyntax,
            Name.Identifier.ValueText: "ShouldRender"
        } ||
        invocationExpression.ArgumentList.Arguments.Count != 0)
    {
        return false;
    }

    var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
    if (componentBase is null)
        return false;

    var semanticModel = compilation.GetSemanticModel(invocationExpression.SyntaxTree);
    return semanticModel.GetOperation(invocationExpression) is IInvocationOperation invocation &&
           SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType.OriginalDefinition, componentBase);
}
```

**当前支持的受控子集**:
1. `return true;`
2. `return base.ShouldRender();`，且最终解析到 `ComponentBase.ShouldRender()`
3. `return base.ShouldRender();`，且最终递归解析到另一个同样受支持的 base `ShouldRender` 实现

若 base 链最终落到动态条件、源码缺失或形成环引用，则继续按 unsupported 处理。

## 辅助方法

### BuildPropsWatchSource

```csharp
private static string BuildPropsWatchSource(VueComponentDescriptor descriptor)
{
    if (descriptor.Props.IsDefaultOrEmpty)
        return "[]";

    return "[" + string.Join(", ", descriptor.Props.Select(prop => "props." + prop.Name)) + "]";
}
```

**生成示例**:
```javascript
[props.value, props.onClick]
```

### FormatStringArray

```csharp
private static string FormatStringArray(IEnumerable<string> values)
    => "[" + string.Join(", ", values.Select(ToJavaScriptString)) + "]";
```

**生成示例**:
```javascript
["value", "onClick"]
```

### ToLowerCamelCase

```csharp
private static string ToLowerCamelCase(string value)
{
    if (string.IsNullOrEmpty(value))
        return value;

    if (value.Length == 1)
        return char.ToLowerInvariant(value[0]).ToString();

    if (char.IsUpper(value[0]) && char.IsUpper(value[1]))
        return value;

    return char.ToLowerInvariant(value[0]) + value.Substring(1);
}
```

**示例**:
- `MyComponent` → `myComponent`
- `HTMLContent` → `HTMLContent`（缩写词保护）

## 内部记录类型

### SupportedEmitCall

```csharp
private sealed record SupportedEmitCall(string EmitName, string? PayloadExpression, bool UsesFirstRender);
```

**字段说明**:
- `EmitName`: Vue 事件名称（如 "onInitialized", "update:value"）
- `PayloadExpression`: 事件载荷表达式（JavaScript 代码）
- `UsesFirstRender`: 是否使用 `firstRender` 参数

### SetParametersAsyncAnalysis

```csharp
private sealed record SetParametersAsyncAnalysis(bool IsSupported, SupportedEmitCall? EmitCall);
```

### ShouldRenderAnalysis

```csharp
private sealed record ShouldRenderAnalysis(bool IsSupported);
```

## 相关文档

- **表达式降级**: `docs/01-目标/razorvue/lowering/ExpressionEmitter.md`
- **组件创作**: `docs/01-目标/razorvue/lowering/ComponentAuthoring.md`
- **生命周期映射**: `docs/01-目标/razorvue/lowering/LifecycleLowering.md`

---

**维护者**: developerhan
**最后更新**: 2026-05-21
**版本**: v1.0
