# `SemanticWalker.cs.Loop.cs`

## 定位

`SemanticWalker.cs.Loop.cs` 负责循环语句相关 lowering。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Loop.cs`

当前文件覆盖的循环类型很集中：

- `foreach`
- `for`
- `while`
- `do-while`

它的职责是把 Roslyn 的循环 `IOperation` 组织成对应的 JS AST，不负责循环体内部更细的表达式语义。

## 当前职责

### 1. `foreach` -> `for...of`

`VisitForEachLoop(...)` 当前直接把 C# `foreach` lower 成 JS `for...of`。

典型结果：

```csharp
foreach (var item in collection)
{
    Console.WriteLine(item);
}
```

```js
for (item of collection) {
  console.log(item);
}
```

当前实现特点：

- 循环变量直接来自 `LoopControlVariable`
- 集合表达式直接翻译为右侧 iterable
- 循环体继续交给通用 statement 转换
- `ForOfStatement` 的 `@await` 固定为 `false`

这说明当前文件处理的是普通 `foreach`，不是 `await foreach` 语义。

### 2. `for` 头部拆分

`VisitForLoop(...)` 会把 `for` 头部分成三段：

- `Before` -> `init`
- `Condition` -> `test`
- `AtLoopBottom` -> `update`

这和 JS `ForStatement` 的结构一一对应。

典型结果：

```csharp
for (int i = 0; i < 10; i++)
{
    Console.WriteLine(i);
}
```

```js
for (let i = 0; i < 10; i++) {
  console.log(i);
}
```

### 3. `AtLoopBottom` 顺序保留

当前实现里一个重要细节是：

- `IForLoopOperation.AtLoopBottom` 可能包含多条 operation

这不代表用户在 C# 源码里写了多个更新表达式，而是 Roslyn lowering 后把一个更新段拆成了多步。

当前策略是：

- 只有一条时，直接作为 update expression
- 多条时，按原顺序拼成 `SequenceExpression`

这样做的目标是保留 update 段的求值顺序，而不是强行把 Roslyn 中间结构重新还原成单一源码外形。

### 4. `while` / `do-while` 共享入口

`VisitWhileLoop(...)` 同时处理：

- `while`
- `do-while`

区分依据只有一条：

- `ConditionIsTop == true` -> `WhileStatement`
- `ConditionIsTop == false` -> `DoWhileStatement`

因此当前实现把这两类循环看作“同一语义族的两个布局版本”。

## 当前关键规则

### 1. `foreach` 当前不做 async iterable lowering

虽然 AST 节点是 `ForOfStatement`，但 `@await` 当前固定为 `false`。

这意味着：

- 普通可迭代集合可以直接工作
- `await foreach` 不属于当前文件已覆盖语义

### 2. `for` 的多个初始化声明会合并成一个 `let`

如果 `Before` 里翻译出了多个 `VariableDeclaration`，当前实现会抽出所有 `VariableDeclarator`，再组合成一个统一的：

```js
let a = ..., b = ...
```

这让 `for` 头部保持 JS 期望的单个 declaration 形态。

### 3. update 段优先保证顺序，而不是源码还原

`AtLoopBottom` 多节点时，当前输出更偏向：

- “语义顺序正确的 JS 表达式序列”

而不是：

- “尽量还原成用户原始写下的单个更新文本”

### 4. 循环体 block 结构不在这里额外重写

`Loop` 文件负责循环壳本身，循环体内部是否是：

- block
- 单语句
- break / continue
- 局部声明

都继续依赖通用 statement lowering。

## 现状与典型结果

### `foreach`

```csharp
var numbers = new[] { 1, 2, 3 };
foreach (var num in numbers)
{
    Console.WriteLine(num);
}
```

```js
let numbers = [1, 2, 3];
for (num of numbers) {
  console.log(num);
}
```

### 无初始化的 `for`

```csharp
int i = 0;
for (; i < 10; i++)
{
    Console.WriteLine(i);
}
```

```js
let i = 0;
for (; i < 10; i++) {
  console.log(i);
}
```

### 无条件的 `for`

```csharp
for (int i = 0; ; i++)
{
    if (i >= 10)
        break;
}
```

```js
for (let i = 0; ; i++) {
  if (i >= 10)
    break;
}
```

### `do-while`

```csharp
do
{
    Work();
} while (flag);
```

```js
do {
  Work();
} while (flag);
```

## 当前边界

这部分当前已经解决的是：

- 常规 `foreach`
- 常规 `for`
- `while`
- `do-while`
- Roslyn lowering 后 update 段的顺序保留

它没有试图做这些事情：

- `await foreach` 语义建模
- 为复杂 enumerator 协议额外插入运行时包装
- 还原 Roslyn 拆开的 update 段源码文本
- 在本文件中单独处理循环体内部的所有作用域细节

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerLoopTest.cs`

建议重点看这些场景：

- `Visit_ForEachLoop`
- `Visit_ForLoop_Simple`
- `Visit_ForLoop_NoInit`
- `Visit_ForLoop_NoCondition`
- `Visit_ForLoop_NoUpdate`
- `Visit_ForLoop_CompoundAssignment`

如果继续往下看，额外测试还覆盖了：

- 递减循环
- 步长变化
- 更复杂的更新表达式
- 空 `for`
- 多初始化变量

## 推荐阅读

建议按这个顺序看：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [SemanticWalker.Loop.md](./SemanticWalker.Loop.md)
3. [SemanticWalker.Ordinary.md](./SemanticWalker.Ordinary.md)
4. [SemanticWalker.Declaration.md](./SemanticWalker.Declaration.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Ordinary.md](./SemanticWalker.Ordinary.md)
- [SemanticWalker.Declaration.md](./SemanticWalker.Declaration.md)
- [SyntaxTransformationPipeline.md](../SyntaxTransformationPipeline.md)
