# `SemanticWalker.cs.Tuple.cs`

## 定位

`SemanticWalker.cs.Tuple.cs` 负责把 C# tuple 相关语法糖 lower 成 JavaScript AST。

当前实现不追求 CLR 级语义等价，而追求两点：

1. C# tuple 的代码结果等价。
2. JavaScript 侧运行时 shape 对业务和第三方可预期。

换句话说，这里的 tuple 不是运行时类型设计问题，而是编译期解糖问题。

## 当前规则

### 1. tuple 只是语法糖

编译器不会引入 `JTuple`、`Array` 子类、`freeze` 或其他新的运行时 tuple 类型。

tuple 最终只会落成普通对象：

```csharp
(1, 2)
```

```js
{ Item1: 1, Item2: 2 }
```

```csharp
(name: "John", age: 30)
```

```js
{ name: "John", age: 30 }
```

### 2. 位置负责语义

解构、比较、swap、本质匹配都按槽位位置处理，而不是按名字处理。

例如：

```csharp
(a, b) = (b, a);
```

lower 后会先缓存右值，再回写左值，避免自引用覆盖：

```js
let v$0, v$1;
v$0 = b, v$1 = a, a = v$0, b = v$1;
```

### 3. 名字负责运行时协议

虽然 C# tuple 名字不是强约束的一部分，但当前编译器把“当前静态视图名字”视为 JS 侧运行时协议。

这意味着：

- 相同位置但不同名字的 tuple，在 JS 侧不能直接透传。
- 业务代码和第三方代码看到的是对象 key，不是 CLR tuple metadata。

### 4. 边界上需要 remap

当 tuple 穿过静态视图边界，且目标 tuple 名字与源名字不同，编译器会显式生成一个新对象，而不是直接复用原对象。

例如：

```csharp
(string name, int age) source = ("John", 30);
(string first, int years) target = source;
```

```js
let source = { name: "John", age: 30 };
let target = { first: source.name, years: source.age };
```

## 已覆盖的 tuple 边界

当前统一通过 `TranslateTupleForTarget(...)` 和 `TryTranslateTupleConversion(...)` 处理 remap，已接入这些边界：

- 显式/隐式 conversion
- 简单赋值
- 方法调用参数
- 构造函数参数
- 对象初始化器赋值
- `return`

如果边界两边 runtime shape 一致，则直接透传，不额外投影。

## 核心实现点

### `VisitTuple`

只负责“当前 tuple 视图”的对象字面量落地，不负责跨边界 remap。

### `HasSameTupleRuntimeShape`

递归比较 tuple 各槽位名字和嵌套 tuple shape。

只要任意一层名字不同，就认为 runtime shape 不同，不能直接透传。

### `BuildTupleProjection`

按目标 tuple 视图重新构造对象。

例如：

```csharp
(name, age) -> (first, years)
```

会生成：

```js
{ first: source.name, years: source.age }
```

嵌套 tuple 也会递归 remap。

### `ShouldCacheTupleSource`

如果 tuple 源值来自调用、属性、复杂表达式等，projection / compare / deconstruct 不能重复求值。

这时会先缓存到临时变量，再按字段读取，保证：

- getter 次数不变
- 调用次数不变
- swap / compare / deconstruct 的结果稳定

## 解构规则

tuple 解构仍按位置展开。

支持：

- tuple 解构
- 嵌套 tuple 解构
- 丢弃
- 带临时缓存的复杂右值
- 自定义 `Deconstruct`

其中 tuple 解构和自定义 `Deconstruct` 是两套路径：

- tuple 解构直接按字段访问对象
- 自定义 `Deconstruct` 按当前编译器约定转成一次方法调用，再从返回数组中取出 out 值

## 比较规则

tuple `==` / `!=` 会递归 lower 成逐槽位比较：

- `==` -> 各元素严格相等并且用 `&&` 连接
- `!=` -> 任一元素严格不等并且用 `||` 连接

复杂操作数会先缓存，避免重复调用。

## 当前实现的明确取舍

下面这些方向当前都不采用：

- 运行时 `JTuple`
- `Array`/数组子类模拟 tuple
- `Object.freeze`
- 同时保留 `ItemN` 与别名双写 shape

原因很直接：

- 会让 `toString` / `toJSON` / 序列化产生额外歧义
- 会把一个编译期语法糖问题膨胀成运行时类型设计问题
- 对当前业务最关键的“稳定对象协议”没有额外收益

## 测试关注点

相关测试主要分布在：

- `src/Jazor.CompilerTest/SemanticWalkerTupleTest.cs`
- `src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs`
- `src/Jazor.CompilerTest/SemanticWalkerCreationTest.cs`

当前测试重点覆盖：

- tuple 字面量生成
- 命名/未命名混合 tuple
- 解构与嵌套解构
- swap 顺序正确性
- tuple 比较
- 参数传递 remap
- 赋值 remap
- 返回值 remap
- 对象初始化器 remap
- 复杂源值缓存

## 结论

当前 tuple lowering 的核心标准是：

- 语义按位置保持等价
- 运行时按当前静态名字暴露协议
- 一旦跨视图就显式 remap
- 一旦源值复杂就先缓存

这套实现更适合当前编译器目标，也比引入额外运行时 tuple 类型更可控。
