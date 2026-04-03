# `SemanticWalker.cs.NotSupport.cs`

## 定位

`SemanticWalker.cs.NotSupport.cs` 集中定义“不进入 JS lowering 面”的 `IOperation`。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.NotSupport.cs`

这份文件不是零散的异常集合，而是编译器语义边界的明确出口：

- 某些 C# 能力无法稳定映射到 JS
- 某些 Roslyn / VB / FlowAnalysis 节点本来就不是目标输入面
- 某些特性会显著放大 C# / JS 运行时割裂，当前选择直接拒绝

## 当前职责

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

- 资源管理：`using` / `using declaration`
- 事件系统：raise / event reference / event assignment
- dynamic：动态创建、动态成员访问、动态调用、动态索引
- CLR / unsafe：`sizeof`、取地址、函数指针
- 查询 / 高级运行时：translated query、插值字符串处理器、UTF-8 字符串
- 编译器内部 / flow analysis：`Stop`、`End`、`FlowCapture`、`CaughtException` 等
- VB 特有节点：`ForToLoop`、`RangeCaseClause`、`ReDim` 等
- 其他明确拒绝的输入：独立 `RangeOperation`、`InlineArrayAccess`、`IInvalidOperation`

### 3. 文档化当前设计边界

这份文件在实际作用上还承担一个工程角色：

- 告诉后续维护者“哪些语义当前是明确不做的”

这比让 unsupported 逻辑散落在各个 partial 文件里更清楚。

## 当前关键规则

### 1. 不支持不等于“暂时没写”

当前许多拒绝分支不是单纯实现空缺，而是设计边界。

例如：

- `using`
- dynamic
- 事件系统
- 函数指针
- 独立 `RangeOperation`

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

原因很直接：

- 它们不是面向最终 JS 输出的稳定语言语义
- 而是 Roslyn 内部或分析阶段节点

### 3. `RangeOperation` 只允许作为别处 lowering 的组成部分

当前 `VisitRangeOperation(...)` 明确拒绝“独立 range”。

这说明当前设计只接受：

- range 在索引器 / 切片语义中被上层专门消费

而不接受：

- 把 range 当成一个独立 JS 运行时对象直接输出

### 4. `VisitInvalid(...)` 已归并到不支持路径

这也是当前现状的一个重要信号：

- `IInvalidOperation` 没有单独 fallback 转换器
- 它现在就是不支持路径的一部分

## 现状与典型边界

### `using`

```csharp
using var file = File.OpenRead("data.txt");
```

当前结果：

- 直接 transformation failure

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

- 直接 transformation failure

### 插值字符串处理器

```csharp
WriteInterpolated($"Hello {name}");
```

如果语义依赖自定义 interpolated string handler，当前结果也是：

- 直接 transformation failure

## 当前边界

这份文件当前并不提供：

- 自动 polyfill
- 运行时仿真层
- “尽量翻译”的降级输出
- 为 unsupported 特性偷偷改写成语义近似物

当前提供的是：

- 明确拒绝
- 失败点集中化
- 边界可读性

这和整个编译器当前方向一致：宁可清晰失败，也不制造运行时语义错配。

## 相关测试

这部分没有形成一个完整独立的 `NotSupport` 测试文件，但有多个测试从不同语法域覆盖了失败行为。

当前可直接关注：

- `src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs`
  - `Visit_UsingDeclaration_Basic`
- `src/Jazor.CompilerTest/SemanticWalkerTryCatchTest.cs`
  - `VisitTry_UsingInTry`
- `src/Jazor.CompilerTest/SemanticWalkerInvalidTest.cs`
  - 当前以说明性注释为主，反映 `IInvalidOperation` 不应进入正常路径

## 推荐阅读

建议按这个顺序看：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [SemanticWalker.NotSupport.md](./SemanticWalker.NotSupport.md)
3. [SemanticWalker.Invalid.md](./SemanticWalker.Invalid.md)
4. [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Invalid.md](./SemanticWalker.Invalid.md)
- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
