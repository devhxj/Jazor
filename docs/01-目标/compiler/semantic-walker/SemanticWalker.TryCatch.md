# `SemanticWalker.cs.TryCatch.cs`

## 目录

- [定位](#定位)
- [职责](#职责)
- [关键规则](#关键规则)
- [现状与典型结果](#现状与典型结果)
- [边界](#边界)
- [相关测试](#相关测试)
- [延伸阅读](#延伸阅读)

## 定位

`SemanticWalker.cs.TryCatch.cs` 负责把 `try` / `catch` / `finally` / `throw` 相关 `IOperation` lower 成 JavaScript AST。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.TryCatch.cs`

这部分不是简单做语法直译。当前实现需要同时处理：

- C# 多 `catch` 到 JavaScript 单 `catch` 的结构收敛
- `when` 过滤器
- `catch` 是否需要绑定异常变量
- 裸 `throw;` 的重新抛出来源
- `try` / `catch` / `finally` 各自的作用域隔离

## 职责

### 1. `try` / `finally` 主体翻译

`VisitTry(...)` 会先把 `try`、每个 `catch`、`finally` 分别放进独立 scope。

当前规则是：

- `try` 体变量声明不会泄漏到外层
- 每个 `catch` 体变量声明不会互相泄漏
- `finally` 体变量声明也独立处理
- 每个局部 scope 内收集到的声明会先 flush 成 `let`

这和当前 `SenseArgument.WithNewScope()` 的用法一致，目标是让生成结果保持 JS block scope 语义。

### 2. 多 `catch` 合并

JavaScript 只有一个 `catch`，所以多个 C# `catch` 不会直接一一对应输出。

当前实现会：

1. 生成一个共享的 `catch (v$N)` 参数
2. 把 C# `catch` 按映射后的 JS 运行时异常类型分组
3. 在 `catch` 内构造 `if` / `else if` / fallback 链
4. 未命中任何分支时 `throw v$N`

典型结果：

```csharp
try
{
    Work();
}
catch (ArgumentNullException ex)
{
    HandleArg(ex);
}
catch (Exception ex)
{
    HandleAny(ex);
}
```

```js
try {
  Work();
} catch (v$0) {
  if (v$0 instanceof TypeError) {
    const ex = v$0;
    HandleArg(ex);
  } else if (v$0 instanceof Error) {
    const ex = v$0;
    HandleAny(ex);
  } else
    throw v$0;
}
```

### 3. 同运行时类型 `catch` 分组

这是当前实现里最重要的细节之一。

多个 `catch` 即使在 C# 类型上不同，只要映射到同一个 JS 运行时类型，就不能简单拆成并列 `else if`。

原因是：

- 这些 `catch` 在 JS 侧看到的是同一个 `instanceof` 结果
- 如果前一个同组 `catch` 带 `when`，过滤失败后必须继续尝试同组后续分支
- 不能因为第一个 `when` 失败就提前 `throw`

所以当前实现先按 `typeName` 做相邻分组，再在组内继续构造链式判断。

这正是 `BuildGroupChain(...)` / `BuildGroupBody(...)` 存在的原因。

### 4. `when` 过滤器

`when` 不会单独变成新的 `catch`。当前策略是把过滤条件插进 `catch` 体内。

典型结果：

```csharp
catch (Exception ex) when (ex.Message.Contains("test"))
{
    Log(ex);
}
```

```js
catch (ex) {
  if (!ex.message.includes("test"))
    throw ex;
  Log(ex);
}
```

这里的关键点是：

- `when` 本身仍在已捕获异常的上下文里求值
- 过滤失败时重新抛出当前异常
- 在多 `catch` 同组场景里，过滤失败会继续落到同组后续分支，而不是直接结束整个 `catch`

### 5. `catch` 参数按需绑定

并不是所有 `catch` 都必须生成 `catch (ex)`。

`RequiresCatchBinding(...)` 当前只在这些情况下要求绑定异常变量：

- 存在 `when`
- `catch` 显式声明了变量
- `catch` 体内包含裸 `throw;`

因此下面这种代码会直接生成无参 `catch`：

```csharp
catch
{
    Console.WriteLine("caught");
}
```

```js
catch {
  console.log("caught");
}
```

这样做的目的明确：不在不需要的时候额外制造 JS 参数名。

### 6. 裸 `throw;`

`VisitThrow(...)` 对裸 `throw;` 不会凭空构造异常对象。

当前实现依赖 `SenseArgument.CatchExceptionVar`：

- 如果当前位于有异常变量上下文的 `catch` 中，输出 `throw ex;`
- 如果当前上下文拿不到捕获变量，则直接报 transformation failure

这条规则保证了 rethrow 只在语义成立的地方被允许。

## 关键规则

### 1. `try`、每个 `catch`、`finally` 都单独建 scope

当前实现不是整条 `try` 语句共用一个变量收集器，而是分块处理。

这样可以避免：

- `try` 内局部声明泄漏到 `catch`
- 某个 `catch` 的临时变量跑到其他 `catch`
- `finally` 中的局部声明污染外层

### 2. 多 `catch` 的 fallback 永远是重新抛出

只要是多 `catch` 合并场景，最终链尾都会保留：

```js
throw v$N;
```

这保证 JS 单 `catch` 不会吞掉未命中的异常。

### 3. 同组多个 `catch` 会尝试共享参数名

如果同一运行时类型组内多个分支使用相同异常变量名，当前实现会先：

```js
const ex = v$0;
```

再在组内复用它。

如果变量名不一致，则按分支单独声明。这让生成结果更接近源代码的绑定关系，也避免无意义重复声明。

### 4. `throw` 的转换不区分“try 内抛出”和“普通位置抛出”

有显式异常表达式时：

- 直接翻译该表达式

没有异常表达式时：

- 只能从当前 `catch` 上下文取异常变量

这条规则让 `throw new Exception(...)` 和 `throw ex;` 走统一路径，而 `throw;` 作为特例由上下文补足。

## 现状与典型结果

### 单个 `catch`

```csharp
try
{
    int x = 1;
}
catch (Exception ex)
{
    int y = 2;
}
```

```js
try {
  let x = 1;
} catch (ex) {
  let y = 2;
}
```

### `catch` 无变量

```csharp
try
{
    throw new Exception();
}
catch
{
    Console.WriteLine("caught");
}
```

```js
try {
  throw new Error;
} catch {
  console.log("caught");
}
```

### 带 `when`

```csharp
try
{
    throw new Exception("test");
}
catch (Exception ex) when (ex.Message.Contains("test"))
{
    Console.WriteLine("ok");
}
```

```js
try {
  throw new Error("test");
} catch (ex) {
  if (!ex.message.includes("test"))
    throw ex;
  console.log("ok");
}
```

### `catch` 中 rethrow

```csharp
try
{
    Work();
}
catch (Exception ex)
{
    Log(ex);
    throw;
}
```

```js
try {
  Work();
} catch (ex) {
  Log(ex);
  throw ex;
}
```

### `finally` 保持原始控制流位置

当前实现不会重写 `finally` 的控制流语义。像 `return`、`throw`、`break`、`continue` 仍然直接出现在 `finally` 体内，由后续 JS 运行时行为决定最终效果。

## 边界

这部分当前已经解决的是：

- `try` / `catch` / `finally` 的基础 lowering
- 多 `catch` 收敛
- `when` 过滤
- 裸 `throw;` 在 `catch` 内的重抛

它并未承担以下职责：

- 建立独立的异常运行时层
- 在 JS 侧模拟 CLR 精确异常类型体系
- 让 `catch` 保留 C# 的多子句语法外形
- 支持脱离 `catch` 上下文的裸 `throw;`

另外，异常类型命中依赖当前类型映射结果，所以“哪个 C# 异常最终映射到哪个 JS 宿主类型”并不在这份文件里决定，而是复用全局类型映射结果。

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerTryCatchTest.cs`

建议重点关注以下场景：

- `VisitTry_SingleCatch`
- `VisitTry_MultipleCatches`
- `VisitTry_MultipleCatchWithWhen`
- `VisitTry_CatchWithoutVariable`
- `VisitCatch_NoVariable`
- `VisitCatch_Rethrow`
- `VisitCatch_WhenCondition`
- `VisitTry_ReturnInFinally`
- `VisitTry_LoopInFinally`
- `VisitTry_TryInFinally`

这些测试基本覆盖了当前文件最重要的结构性行为。

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [SyntaxTransformationPipeline.md](../SyntaxTransformationPipeline.md)
