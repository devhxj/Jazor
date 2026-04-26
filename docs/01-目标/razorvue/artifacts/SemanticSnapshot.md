# RazorVueSemanticSnapshot - RazorVue 语义快照
> Status: 活跃参考

## 1. 文档定位

本文档描述 `RazorVueSemanticSnapshot`，这是 RazorVue 编译主链路使用的最小语义快照。它在 Source Generator 分析阶段构建，同时保留两层信息：descriptor/flag 视图（供 HMR/hash/诊断使用）和 lifecycle method symbols（供 lowering 阶段生成 Vue hooks）。

**核心文件**：
- `src/Jazor.RazorVue/Artifacts/RazorVueSemanticSnapshot.cs`

## 2. 核心类型

### 2.1 RazorVueSemanticSnapshot 大型 Positional Record

```csharp
public sealed record RazorVueSemanticSnapshot(
    // ---- 编译上下文 ----
    Compilation Compilation,
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol? BuildRenderTreeMethod,

    // ---- Vue 语义描述符 ----
    VueLifecycleDescriptor Lifecycle,      // 生命周期标志（bool 字段）
    VueLogicDescriptor Logic,              // setup() 逻辑描述
    VueComponentDescriptor Descriptor,     // 组件描述符（props/emits/slots）

    // ---- 源码映射信息 ----
    ImmutableArray<RazorVueSourceOrigin> Origins,
    ImmutableArray<string> ImportedNamespaces,

    // ---- Lifecycle method symbol carriers (for lowering) ----
    // 只携带当前 lowering 支持的安全子集；其他 lifecycle 以 bool flag 为准。
    IMethodSymbol? OnInitializedMethod = null,
    IMethodSymbol? OnInitializedAsyncMethod = null,
    IMethodSymbol? OnParametersSetMethod = null,
    IMethodSymbol? OnParametersSetAsyncMethod = null,
    IMethodSymbol? ShouldRenderMethod = null,
    IMethodSymbol? SetParametersAsyncMethod = null,
    IMethodSymbol? OnAfterRenderMethod = null,
    IMethodSymbol? OnAfterRenderAsyncMethod = null,
    IMethodSymbol? DisposeMethod = null,
    IMethodSymbol? DisposeAsyncMethod = null
);
```

**设计特点**：
- **双层信息保留**：同时保留 descriptor/flag 视图和 symbol 视图
- **Lowering 优化**：避免 lowering 阶段重新发现生命周期方法
- **安全子集限制**：只携带当前 lowering 支持的方法符号

### 2.2 字段分组说明

#### 2.2.1 编译上下文字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `Compilation` | `Compilation` | Roslyn 编译上下文，用于类型查找和符号解析 |
| `ComponentSymbol` | `INamedTypeSymbol` | RazorVue 组件的类型符号（继承自 `ComponentBase`） |
| `BuildRenderTreeMethod` | `IMethodSymbol?` | `BuildRenderTree` 方法符号（可能为 null） |

#### 2.2.2 Vue 语义描述符字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `Lifecycle` | `VueLifecycleDescriptor` | 生命周期标志集合（如 HasOnMounted、HasOnUpdated 等 bool 字段） |
| `Logic` | `VueLogicDescriptor` | setup() 逻辑描述（computed/watch 方法引用） |
| `Descriptor` | `VueComponentDescriptor` | 组件描述符（props/emits/slots 的完整定义） |

#### 2.2.3 源码映射字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `Origins` | `ImmutableArray<RazorVueSourceOrigin>` | 源码位置映射（用于 SourceMap 和诊断） |
| `ImportedNamespaces` | `ImmutableArray<string>` | 导入的命名空间列表（用于 using 语句生成） |

#### 2.2.4 Lifecycle Method Symbol Carriers

10 个可选的生命周期方法符号载体，对应 Blazor 生命周期到 Vue hooks 的映射：

| C# 方法 | Vue Hook | 字段名 |
|---------|----------|--------|
| `OnInitialized()` | `onMounted()` | `OnInitializedMethod` |
| `OnInitializedAsync()` | `onMounted()` | `OnInitializedAsyncMethod` |
| `OnParametersSet()` | `watch(() => props.xxx, ...)` | `OnParametersSetMethod` |
| `OnParametersSetAsync()` | `watch(() => props.xxx, ...)` | `OnParametersSetAsyncMethod` |
| `ShouldRender()` | 无直接映射（优化提示） | `ShouldRenderMethod` |
| `SetParametersAsync()` | `watch(() => props, ...)` | `SetParametersAsyncMethod` |
| `OnAfterRender()` | `onUpdated()` | `OnAfterRenderMethod` |
| `OnAfterRenderAsync()` | `onUpdated()` | `OnAfterRenderAsyncMethod` |
| `Dispose()` | `onUnmounted()` | `DisposeMethod` |
| `DisposeAsync()` | `onUnmounted()` | `DisposeAsyncMethod` |

**为什么是可选字段**：
- 组件可能不实现所有生命周期方法
- 未实现的方法为 `null`，lowering 阶段跳过生成对应 Vue hook

## 3. 核心算法

### 3.1 语义快照构建流程

```
RazorVue 组件类 (C#)
       ↓
Symbol 发现（遍历 ComponentSymbol 成员）
       ↓
VueLifecycleDescriptor 构建（bool flags）
       ↓
VueLogicDescriptor 构建（computed/watch 引用）
       ↓
VueComponentDescriptor 构建（props/emits/slots）
       ↓
Lifecycle methods 符号提取
       ↓
RazorVueSemanticSnapshot 生成
```

### 3.2 Lifecycle Method Symbol 提取规则

Source Generator 分析阶段按以下规则提取生命周期方法符号：

```csharp
// 伪代码示例
IMethodSymbol? FindLifecycleMethod(INamedTypeSymbol componentSymbol, string methodName)
{
    var method = componentSymbol.GetMembers(methodName)
        .OfType<IMethodSymbol>()
        .FirstOrDefault(m => m.IsOverride && m.MethodKind == MethodKind.Ordinary);

    // 验证方法签名是否安全
    if (method is null || !IsSignatureSafeForLowering(method))
        return null;

    return method;
}
```

**签名安全检查**：
- 方法必须是 `override`（避免捕获用户自定义方法）
- 参数类型必须在白名单中（如 `ParameterView`）
- 返回类型必须是 `Task` 或 `void`

### 3.3 Lowering 阶段使用

Lowering 阶段（生成 `setup()` 函数）使用符号载体直接生成 Vue hooks：

```csharp
// 伪代码示例
FunctionBody GenerateSetupBody(RazorVueSemanticSnapshot snapshot)
{
    var statements = new List<Statement>();

    // 生成 onMounted(() => { ... })
    if (snapshot.OnInitializedMethod is { } onInitialized)
    {
        statements.Add(new CallExpression(
            new Identifier("onMounted"),
            new[] { GenerateCallbackFromMethod(onInitialized) }
        ));
    }

    // 生成 watch(() => props.xxx, async (newVal, oldVal) => { ... })
    if (snapshot.OnParametersSetMethod is { } onParametersSet)
    {
        statements.Add(new CallExpression(
            new Identifier("watch"),
            new[] { GeneratePropsGetter(onParametersSet), GenerateCallbackFromMethod(onParametersSet) }
        ));
    }

    return new FunctionBody(statements);
}
```

**优势**：
- 避免重新遍历 `ComponentSymbol` 成员
- 符号已在分析阶段验证安全性
- 支持方法体优化（no-op 或 emit 调用时展开实际 Vue hook 表达式）

## 4. 线程安全模型

`RazorVueSemanticSnapshot` 是不可变 record 类型，天然线程安全。

- **构建阶段**：Source Generator 单线程构建（编译时）
- **读取阶段**：Lowering 阶段只读访问（无状态修改）

## 5. 错误处理

### 5.1 符号发现失败

如果生命周期方法符号发现失败（如方法签名不安全），对应字段为 `null`：

```csharp
// 不安全的 OnParametersSet 签名
public void OnParametersSet(Dictionary<string, object> parameters)  // ❌ 参数类型不在白名单
{
    // ...
}

// 结果：OnParametersSetMethod = null（lowering 跳过生成）
```

### 5.2 Lowering 阶段处理

Lowering 阶段遇到非 no-op 方法体时抛出异常（不静默降级）：

```csharp
if (snapshot.OnInitializedMethod is { } onInitialized)
{
    var methodBody = GetMethodBody(onInitialized);
    if (!IsNoOp(methodBody) && !IsEmitCall(methodBody))
        throw new InvalidOperationException(
            $"Lifecycle method '{onInitialized.Name}' has unsupported logic. " +
            $"Only no-op or emit calls are allowed during lowering.");
}
```

**错误示例**：
```csharp
protected override void OnInitialized()
{
    Console.WriteLine("Direct console call");  // ❌ 不支持，抛出异常
}
```

**正确示例**：
```csharp
protected override void OnInitialized()
{
    // no-op（支持，lowering 跳过）
}

protected override void OnInitialized()
{
    ExecuteJavaScript("console.log('Hello')");  // ✅ 支持，lowering 展开 Vue hook 表达式
}
```

## 6. 配置选项

无直接配置选项。行为由 Source Generator 的分析阶段规则决定。

## 7. 与其他子系统的交互

### 7.1 与 VueComponentDescriptor 的交互

- `Descriptor` 字段包含组件的完整定义（props/emits/slots）
- `Lifecycle` 和 `Logic` 字段是 `Descriptor` 的补充信息

### 7.2 与 VueCompiledArtifact 的交互

- `RazorVueSemanticSnapshot` 是编译前的语义信息
- `VueCompiledArtifact` 是编译后的输出，两者通过 `ComponentSymbol` 关联
- `Origins` 字段传递到 `VueCompiledArtifact.SourceOrigins`

### 7.3 与 Lowering 的交互

Lowering 阶段直接使用符号载体生成 Vue hooks，避免重新发现方法：

```csharp
// Lowering 伪代码
if (snapshot.OnInitializedMethod is { } onInitialized)
{
    var statements = LowerMethodBody(onInitialized);
    setupBody.Add(new CallExpression(
        new Identifier("onMounted"),
        new[] { new ArrowFunctionExpression(statements) }
    ));
}
```

## 8. 设计权衡

### 8.1 为什么保留双层信息（Descriptor + Symbols）

**问题**：为什么不同时使用 `VueLifecycleDescriptor` 的 bool 标志和 method symbols？

**答案**：
- **Descriptor/Flag 视图**：供 HMR/hash/诊断使用，不依赖 Roslyn symbol（运行时不可用）
- **Symbol 视图**：供 lowering 阶段使用，直接访问方法体进行代码生成

如果只用符号，HMR 阶段无法访问 Roslyn symbol（编译后丢失）。如果只用 flag，lowering 阶段需要重新发现方法（性能损失）。

### 8.2 为什么只携带安全子集的方法符号

**问题**：为什么不是所有生命周期方法都有对应的 symbol 字段？

**答案**：
- 当前 lowering 只支持 10 个方法的代码生成
- 其他方法（如 `OnAfterRenderRender`）未实现映射，不需要携带符号
- 减少快照大小，避免不必要的符号保留

**扩展方向**：未来 lowering 支持更多方法时，可扩展新的 symbol 字段。

### 8.3 为什么使用 Positional Record 而非 Property Record

**问题**：为什么使用大型 positional record（17 个参数）而非属性 record？

**答案**：
- **解构便利**：支持模式匹配和位置解构
- **不可变性**：positional record 的 `with` 表达式更简洁
- **性能**：positional record 的构造函数比属性 record 更快

**权衡**：参数过多可能降低可读性，但通过注释分组缓解。

### 8.4 为什么 Lowering 不做静默降级

**问题**：为什么遇到不支持的方法体逻辑时抛出异常而非跳过？

**答案**：
- **语义一致性**：静默跳过可能导致运行时行为不符合预期
- **早期失败**：编译时报错比运行时故障更容易调试
- **安全策略**：RazorVue 只支持明确定义的转换模式，不支持任意 C# 代码

**用户指导**：文档明确说明哪些逻辑支持（no-op、emit 调用），哪些不支持（直接 C# 代码）。
