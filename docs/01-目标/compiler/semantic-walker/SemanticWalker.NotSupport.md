# `SemanticWalker.cs.NotSupport.cs`

## 定位

`SemanticWalker.cs.NotSupport.cs` 集中定义“不进入 JS lowering 面”的 `IOperation`。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.NotSupport.cs`

这份文件不是零散的异常集合，而是编译器语义边界的明确出口：

- 某些 C# 能力无法稳定映射到 JS
- 某些 Roslyn / VB / FlowAnalysis 节点本来就不是目标输入面
- 某些特性会显著放大 C# / JS 运行时割裂，当前选择直接拒绝

## 职责

### 1. 快速失败

当前大多数入口都是：

```csharp
=> HandleTransformationFailure<Node>(operation, "...");
```

也就是：

- 一旦命中这些 operation
- 立即停止当前转换
- 返回带明确原因的 transformation failure

### 2. 归档“不支持语义”

当前这份文件记录的不是单一类别，而是整个“不支持面”。

主要包括：

- 资源管理：当前未承接 `await using` 之外的更复杂 async render/resource-runtime 语义边界
- 事件系统：raise / event reference / event assignment
- dynamic：动态创建、动态成员访问、动态调用、动态索引
- CLR / unsafe：取地址、函数指针
- 查询 / 高级运行时：插值字符串处理器、UTF-8 字符串
- 编译器内部 / flow analysis：`Stop`、`End`、`FlowCapture`、`CaughtException` 等
- VB 特有节点：`ForToLoop`、`RangeCaseClause`、`ReDim` 等
- 其他明确拒绝的输入：`InlineArrayAccess`、`IInvalidOperation`

已经从这份“不支持面”中移出、进入正式 lowering 主线的切片包括：

- `using` / `using declaration` / `await using`
- `lock`
- 窄语义 `typeof(T)` 运行时类型令牌
- `ITranslatedQueryOperation`：仅移除 Roslyn query wrapper，复用已绑定的 invocation/lambda lowering
- `System.Index` / `System.Range` 值：通过 CLR `JIndex` / `JRange` carrier 和白名单成员映射传递
- 窄语义 `sizeof(T)`：仅编译期 primitive scalar 或 enum underlying size，输出数值常量

`Enumerable` query 继续走普通 delegate callback；`System.Linq.Expressions.Expression<TDelegate>`
和 `IQueryable<T>` query 的 lambda 则在 conversion 使用点明确拒绝。它们要求保留供 provider
检查和改写的表达式树，而箭头函数只能表示可执行 delegate，不能作为近似替代。

### 29 个剩余 visitor 的分类决策

当前文件实际保留 29 个不支持 visitor。这个数字是当前代码结果，不是早期“约 31 个”的估算值。

| 分类 | visitor 数 | 代表节点 | 决策 |
| --- | ---: | --- | --- |
| Roslyn / FlowAnalysis 内部节点 | 8 | `Stop` 、`End` 、`FlowCapture` 、`CaughtException` 、collection placeholder | 不是稳定源语言输入，不建立 lowering |
| VB 专有语法 | 5 | `ForToLoop` 、`RangeCaseClause` 、`ReDim` | Razor/C# 产品输入面不需要 |
| C# dynamic 与事件 | 7 | dynamic create/member/invoke/indexer，event raise/reference/assignment | 需要 DLR 和多播委托运行时模型，不以 JS 属性访问冒充 |
| unsafe 语义 | 2 | `AddressOf` 、function pointer invoke | JavaScript 无 CLR 地址模型，明确拒绝 |
| custom interpolated-string handler | 4 | handler creation/addition/append/placeholder | 需要完整 handler 协议与参数传递模型，不以模板字符串降级 |
| 其他明确边界 | 3 | UTF-8 literal，inline-array access，`IInvalidOperation` | 分别缺少字节 / 栈布局语义，或表示 Roslyn 已无法绑定 |

结论：当前 29 个条目中没有可以只补一个 ESTree 节点就获得正确 C# 语义的候选。后续如果要开放 dynamic、event 或 handler，必须先提出独立的宿主运行时协议、类型/求值顺序合约与 Deno.host 端到端测试；不属于普通 compiler visitor 补全。

### 3. 文档化当前设计边界

这份文件在实际作用上还承担一个工程角色：

- 告诉后续维护者“哪些语义当前是明确不做的”

这比让 unsupported 逻辑散落在各个 partial 文件里更清楚。

## 关键规则

### 1. 不支持不等于“暂时没写”

当前许多拒绝分支不是单纯实现空缺，而是设计边界。

例如：

- dynamic
- 事件系统
- 函数指针

这些都不只是“以后补个 AST 节点”就能解决的问题，而是涉及 C# / JS 运行时模型差异。

### 2. 编译器内部节点直接拒绝

像这些 operation：

- `IStopOperation`
- `IEndOperation`
- `ICaughtExceptionOperation`
- `IFlowCaptureOperation`
- `IFlowCaptureReferenceOperation`
- `IFlowAnonymousFunctionOperation`
- `IStaticLocalInitializationSemaphoreOperation`

当前都直接失败。

原因在于：

- 它们不是面向最终 JS 输出的稳定语言语义
- 而是 Roslyn 内部或分析阶段节点

### 3. `RangeOperation` 是映射值，不是 array-only 语法糖

当前 `VisitRangeOperation(...)` 将 range 通过已绑定的 `System.Range(Index, Index)`、
`Index.Start` / `Index.End` 和 `System.Index` 工厂成员转换为 `JRange` carrier。

- 直接 array/indexer range 仍走数字 offset fast path，避免不必要的 carrier
- 传过 local、argument 或 return 边界的 `Index` / `Range` 值，会在消费点调用
  `GetOffset(int)` / `GetOffsetAndLength(int)`，保持范围校验和单次求值
- 不为普通 JavaScript 对象猜测 Range/Index 结构；缺少 CLR mapping 时仍明确失败

### 4. `sizeof` 仅保留可证明的编译期常量

`sizeof(bool)`、数值 primitive、`decimal` 和 enum underlying type 会直接输出其 Roslyn 已计算的数值。
这不是 JavaScript memory-layout lowering：`DateTime` 等 carrier-backed CLR 类型和用户 struct 仍拒绝，
因为它们的 JS 表示不承诺 CLR storage layout。

### 5. `VisitInvalid(...)` 已归并到不支持路径

这也是当前现状的一个重要信号：

- `IInvalidOperation` 没有单独 fallback 转换器
- 它现在就是不支持路径的一部分

### 5. `typeof(T)` 不是完整反射能力

当前 `typeof(T)` 已不再由 `NotSupport` 统一拒绝，但它支持的是“稳定运行时类型令牌”，不是完整 CLR `System.Type` 反射对象。

当前允许：

- `typeof(int)` 这类稳定映射到 JS 构造器的类型
- `typeof(Person)` 这类稳定映射到运行时 class/constructor 的类型
- `typeof(Person).Name` 这类最小后续消费

当前继续拒绝：

- record 的 structural lowering 类型
- `System.DateTime` / `System.DateOnly` / `System.DateTimeOffset` / `System.TimeOnly` / `System.TimeSpan` 这类 shaped carrier
- tuple / anonymous type / erased interface

## 现状与典型边界

### dynamic

```csharp
dynamic obj = GetObject();
obj.Run();
```

当前结果：

- 直接 transformation failure

### 独立 `Range`

```csharp
var range = 1..5;
```

当前结果：

- 通过 `JRange` carrier 输出；传递后可由 array、string 或具备 `Length`/`Slice(int,int)` 协议的目标消费

### 插值字符串处理器

```csharp
WriteInterpolated($"Hello {name}");
```

如果语义依赖自定义 interpolated string handler，当前结果也是：

- 直接 transformation failure

### `typeof(record)`

```csharp
record Person(string Name);
var type = typeof(Person);
```

当前结果：

- 直接 transformation failure

原因：

- record 当前走 structural lowering，不承诺稳定 nominal runtime type token

## 边界

当前提供的是：

- 明确拒绝
- 失败点集中化
- 边界可读性

## 相关测试

这部分没有形成一个完整独立的 `NotSupport` 测试文件，但有多个测试从不同语法域覆盖了失败行为。

当前可直接关注：

- `src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs`
  - `Visit_UsingDeclaration_Basic`
  - `Visit_AwaitUsingDeclaration_Basic`
- `src/Jazor.CompilerTest/SemanticWalkerTryCatchTest.cs`
  - `VisitTry_UsingInTry`
- `src/Jazor.CompilerTest/SemanticWalkerInvalidTest.cs`
  - 当前以说明性注释为主，反映 `IInvalidOperation` 不应进入正常路径

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Invalid.md](./SemanticWalker.Invalid.md)
- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [SyntaxTransformationPipeline.md](../SyntaxTransformationPipeline.md)
