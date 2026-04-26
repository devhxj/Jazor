# `SemanticWalker.cs.Ordinary.cs`

## 定位

`SemanticWalker.cs.Ordinary.cs` 负责一组“基础但分布很广”的语义 lowering。

它并不只是处理二元/一元运算符。当前这份文件实际上覆盖：

- block 与函数体
- `return`
- 标签、分支、空语句
- 字面量与常量值
- 转换
- 条件访问
- 一元/二元/三元表达式
- 赋值、复合赋值、`??=`
- lambda / 局部函数 / `await`
- `nameof` / `default`

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Ordinary.cs`

## 当前职责

从现状看，这部分逻辑可以分成六条主线。

### 1. block 与函数体基础设施

这部分文件首先承担的是“把一串 operation 组织成 JS statement/block”的职责。

典型入口有：

- `VisitBlock(...)`
- `VisitMethodBodyOperation(...)`
- `VisitConstructorBodyOperation(...)`
- `VisitLocalFunction(...)`
- `VisitAnonymousFunction(...)`

这里的关键规则不是运算符映射，而是：

- 新作用域如何建立
- `_declarators` 如何在块顶提升
- 函数边界如何隔离变量声明但保留 import specifier 传播

换言之，这份文件里相当一部分代码是在支撑“普通表达式应该落在哪个 block 里”，而不是直接处理表达式本身。

### 2. 值与字面量归一化

当前 `BuildValueLiteral(...)` / `VisitLiteral(...)` 负责把 Roslyn 常量值转换成稳定的 JS 字面量。

覆盖面包括：

- `null`
- 布尔值
- 字符串与字符
- 数值
- `BigInt`
- 特殊浮点值

这里的重点不是“字面量直接照抄”，而是：

- 使用 JS 实际可接受的字面量形式
- 保留必要的数值边界行为
- 对特殊值显式映射，例如 `NaN` / `Infinity`

### 3. 转换与空值相关表达式

这部分包括：

- `VisitConversion(...)`
- `VisitConditionalAccess(...)`
- `VisitConditionalAccessInstance(...)`
- `VisitCoalesce(...)`
- `VisitCoalesceAssignment(...)`
- `VisitDefaultValue(...)`

当前几个重要事实：

- 普通引用类型转换、装箱/拆箱、`as` 转换大多直接退化为操作数本身
- `Number <-> BigInt` 的显式转换会保留为 `Number(...)` / `BigInt(...)`
- tuple conversion 是一条显式边界，会先尝试 `TryTranslateTupleConversion(...)`
- 条件访问通过 `PatternInput` 传递可选链左侧对象
- `??` 和 `??=` 直接落为 nullish 相关表达式

所以“转换”在当前实现里不是 CLR cast 仿真，而是“只保留 JS 侧真正需要显式表达的那部分差异”。

### 4. 普通运算表达式

这一组包括：

- `VisitUnaryOperator(...)`
- `VisitBinaryOperator(...)`
- `VisitConditional(...)`
- `VisitIncrementOrDecrement(...)`

整体风格直接：

- 一元运算 -> `NonUpdateUnaryExpression`
- `++/--` -> `UpdateExpression`
- 逻辑与/或 -> `LogicalExpression`
- 其他二元运算 -> `NonLogicalBinaryExpression`
- 三元表达式 -> `ConditionalExpression`

同时会保留一条重要扩展点：

- 如果二元运算绑定到了 `OperatorMethod`，会优先尝试白名单映射

这意味着普通运算虽然默认是“直接运算符映射”，但并不排斥通过宿主映射接管特定运算符语义。

### 5. 赋值与副作用表达式

包括：

- `VisitSimpleAssignment(...)`
- `VisitCompoundAssignment(...)`
- `VisitCoalesceAssignment(...)`

当前这条路径有两个重点：

1. tuple 赋值边界会主动 remap，不依赖 Roslyn 恰好插入 conversion
2. 属性 setter 赋值会优先尝试 setter 对应的白名单映射

换言之，`x = y` 这类代码在当前实现里并不总是直接产出 `AssignmentExpression`。

如果命中了：

- tuple 视图/对象协议切换
- setter 宿主映射

它会先走更高优先级的语义修正路径。

### 6. 语句级基础节点

这部分还包括一些容易被忽略、但很基础的语句节点：

- `VisitReturn(...)`
- `VisitBranch(...)`
- `VisitExpressionStatement(...)`
- `VisitEmpty(...)`
- `VisitLabeled(...)`
- `VisitAwait(...)`
- `VisitNameOf(...)`
- `VisitOmittedArgument(...)`

这些节点虽然不复杂，但它们共同决定了“普通方法体里剩下的大部分基础语义”如何落到 JS。

## 当前几个关键规则

### 1. return 也是 tuple 边界

`return` 不是简单返回表达式。

当前如果返回值的 tuple 当前视图和函数声明返回类型不一致，会在 `VisitReturn(...)` 里显式 remap。

换言之，tuple 视图/对象协议的边界不只出现在：

- 赋值
- 参数传递
- 初始化器

也出现在 `return`。

### 2. 条件访问复用 `PatternInput`

`PatternInput` 不只服务于模式匹配。

当前 `VisitConditionalAccess(...)` 也使用同一机制：

- 先求值左侧对象
- 再把它放进 `PatternInput`
- 让 `VisitConditionalAccessInstance(...)` 从上下文中拿到同一个目标对象

这说明 `PatternInput` 现在已经更广泛地承担“当前隐式输入对象”的传递职责。

### 3. 函数边界隔离变量声明

`VisitBlock(...)`、`VisitLocalFunction(...)`、`VisitAnonymousFunction(...)` 当前都在坚持一个统一原则：

- 变量声明不能跨函数边界泄漏
- import specifier 可以继续向外传播

这条规则和总文档里关于 `SenseArgument` 的说明保持一致。

### 4. 当前优先保留 JS 运行时语义，而不是 CLR 外形

最典型的例子是 conversion：

- CLR 里的很多 cast 在 JS 里没有必要单独表示
- 只有 `Number/BigInt` 这类真正影响 JS runtime 的转换，才会保留为显式调用

这和整个编译器当前方向一致：

- 保留真正影响 JS 的部分
- 忽略只属于 CLR 静态类型系统的外形差异

## 现状与典型结果

### 空合并与空合并赋值

```csharp
string? nullableStr = null;
string finalStr = nullableStr ?? "default";
name ??= "Default";
```

```js
let nullableStr = null;
let finalStr = nullableStr ?? "default";
name ??= "Default";
```

### 条件访问

```csharp
string? testStr = null;
int? length = testStr?.Length;
```

```js
let testStr = null;
let length = testStr?.length;
```

### 类型转换

```csharp
double d = 3.14;
int i = (int)d;
long x = (long)1;
```

当前现状是：

- `int` <- `double` 这类在 JS 里通常直接保留操作数
- `Number` -> `BigInt` 会显式生成 `BigInt(...)`

### lambda

```csharp
var func = (int a, int b) => a + b;
```

```js
let func = (a, b) => {
  return a + b;
};
```

### `nameof`

```csharp
string methodName = nameof(TestMethod);
```

```js
let methodName = 'TestMethod';
```

## 当前边界

这份文件当前不承诺这些事情：

- 模拟 CLR 的完整 conversion 语义
- 精确复刻 `checked/unchecked` 溢出行为
- 最小化所有括号或生成最短表达式
- 让所有普通语义都只依赖一个统一 helper 层

它当前更偏向：

- 让基础语义节点稳定落成 JS AST
- 在 tuple / setter / runtime numeric conversion 这些关键边界上补足语义差异
- 为其他语法域提供公共 block / return / lambda / literal 支撑

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerOrdinaryTest.cs`

建议重点关注这些测试面：

- conversion 与 `Number/BigInt`
- 条件访问
- `??` / `??=`
- tuple 赋值边界
- setter 映射赋值
- lambda / await / return

## 推荐阅读

建议按以下顺序阅读：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [SemanticWalker.Ordinary.md](./SemanticWalker.Ordinary.md)
3. [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
4. [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
5. [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [Optimizer.md](../Optimizer.md)
