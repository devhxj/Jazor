# `SemanticWalker.cs.Creation.cs`

## 目录

- [定位](#定位)
- [当前职责](#当前职责)
- [当前关键规则](#当前关键规则)
- [现状与典型结果](#现状与典型结果)
- [初始化器展开方式](#初始化器展开方式)
- [何时直接变成对象字面量](#何时直接变成对象字面量)
- [当前边界](#当前边界)
- [延伸阅读](#延伸阅读)

## 定位

`SemanticWalker.cs.Creation.cs` 负责“创建类”语义节点的 lowering。

它处理的不只是 `new Type()`，还包括：

- 对象创建
- 对象/集合初始化器
- `record` 创建
- 匿名对象
- 数组创建
- 泛型类型参数对象创建
- 委托创建

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Creation.cs`

## 当前职责

这部分逻辑可以分成六条主线。

### 1. 普通对象创建

最基础的路径是 `BuildObjectCreation(...)`。

它先处理参数，再确定目标类型在当前映射里应该落成什么宿主表达式，然后再决定最终是：

- `new Type(...)`
- `Type(...)`
- 数组字面量
- 白名单映射表达式

换言之，`new` 在这里不是机械保留，而是会根据类型映射和构造器映射进一步改写。

### 2. 带初始化器的对象创建

如果对象创建后还跟着初始化器，当前通常有两种路径：

- 能直接落成集合字面量时，优先直接落字面量
- 否则使用 IIFE 包装初始化过程

典型例子：

```csharp
var obj = new TestClass { Name = "Test", Value = 42 };
```

当前会生成：

```js
let obj = (() => {
  let v$0 = new TestClass;
  v$0.Name = "Test";
  v$0.Value = 42;
  return v$0;
})();
```

这条路径的目标不是“生成最短代码”，而是：

- 保持求值顺序正确
- 允许嵌套初始化器继续展开
- 让对象创建和成员赋值共享已有 lowering 规则

### 3. 集合初始化器与集合字面宿主

当前实现不是把所有集合初始化器都降成 IIFE。

对于已经有明确 JS 集合宿主的类型，会优先直接落成字面宿主：

- `List<T>` / 数组映射 -> `[...]`
- `HashSet<T>` / `Set<T>` 映射 -> `new Set([...])`
- `Dictionary<TKey, TValue>` / `Map<TKey, TValue>` 映射 -> `new Map([[k, v], ...])`

这条路径由 `TryBuildCollectionLiteral(...)` 处理。

它的意义明确：

- 避免把本来就能直接表达的 JS 集合再包成初始化器流程
- 让 CLR 集合桥接类型尽量对齐真实 JS 容器 shape

### 4. `record` 创建

如果目标类型是 `record`，当前不会走普通 `new` 对象创建路径，而是直接落成对象字面量。

这条路径由：

- `ShouldLowerRecordStructurally(...)`
- `BuildRecordStructuralLiteral(...)`

负责。

它会：

- 读取构造参数
- 按 record 属性名恢复 key
- 合并初始化器成员
- 最终产出 `ObjectExpression`

这意味着 `record` 在当前语义里更接近“带静态协议的 JS 对象字面量”，而不是普通 CLR class 构造实例。

这条规则是全局约定，不依赖：

- record 是否位于 `ECMAScript` 程序集
- record 是否声明了 `Description` / `ECMAScriptName`
- record 是否有显式名称

如果想要普通类语义，必须显式写 `class`。

### 5. 数组创建

数组创建现在覆盖的情况比旧文档写得更完整。

当前主要分三类：

- 一维带初始化器数组 -> `ArrayExpression`
- 一维按大小创建 -> `new Array(size)`
- 多维/嵌套数组 -> 递归数组字面量或递归 `Array(...).fill().map(...)`

换言之，多维数组不再只是“明确不支持”。

当前实现里：

- 如果多维数组有初始化器，走 `BuildNestedArrayInitializer(...)`
- 如果多维数组只有维度大小，走 `BuildMultiDimensionalArray(...)`

这条路径的目标是给出可运行的 JS 结构，而不是强行保持 CLR 数组运行时模型。

### 6. 委托创建

当前 `VisitDelegateCreation(...)` 非常直接：

- 直接回落到 `Visit(operation.Target, argument)`

这意味着委托创建是否需要额外包装，主要取决于目标表达式本身怎么 lower，而不是在 `Creation` 路径里单独造一层委托对象。

## 当前关键规则

### 1. tuple 边界在创建路径里同样有效

`Creation` 不是 tuple remap 的例外区。

当前这些位置都会显式走 `TranslateTupleForTarget(...)`：

- 构造函数参数
- 对象初始化器赋值
- 集合初始化器元素
- 数组元素
- `record` 构造参数

换言之，创建路径不会因为“这是 `new`”就绕过 tuple 视图/对象协议规则。

### 2. 初始化器成员名优先走 setter / 字段映射

对象初始化器里的成员名不是简单取属性名。

当前通过 `GetInitializerMemberName(...)` 决定最终 key / member name。

这让初始化器可以正确复用：

- setter 对应的白名单别名
- 字段自身的白名单别名

所以创建路径和普通引用路径在名字规则上是一致的。

### 3. 创建路径也消费白名单

这部分不是“只看类型映射，不看成员映射”。

当前会消费白名单的点包括：

- 构造函数白名单映射
- 初始化器里的 setter / 成员白名单映射
- 集合初始化器里的调用映射

换言之，一个创建表达式里的构造、赋值、Add 调用，仍然可能分别落到不同的白名单规则上。

### 4. `record` 的语义边界

当前 `record` 的 lowering 约定是 structural lowering：

- `new Record(...)` -> 对象字面量
- `record with { ... }` -> 对象 spread
- record 位置/属性模式 -> 基于结构属性键匹配

当前不承诺：

- `record` 的 runtime class identity
- `instanceof Record`
- 模块/成员层 record runtime declaration

## 现状与典型结果

### 普通对象

```csharp
var obj = new object();
```

```js
let obj = new Object;
```

### 构造映射

```csharp
var exception = new System.Exception("Error message");
```

```js
let exception = new Error("Error message");
```

### 集合初始化器

```csharp
var list = new List<int> { 1, 2, 3 };
var set = new HashSet<int> { 1, 2, 3 };
```

```js
let list = [1, 2, 3];
let set = new Set([1, 2, 3]);
```

### 字典初始化器

```csharp
var dict = new Dictionary<string, int>
{
    { "one", 1 },
    { "two", 2 }
};
```

```js
let dict = new Map([["one", 1], ["two", 2]]);
```

### 匿名对象

```csharp
var anonymous = new { Name = "Test", Value = 42 };
```

```js
let anonymous = { Name: "Test", Value: 42 };
```

注意这里的现状是：匿名对象当前保留当前视图键名，不自动做额外 camel 化。

## 初始化器展开方式

`BuildObjectCreationInitializer(...)` 是当前初始化器展开的主入口。

它会处理三类 initializer：

- `ISimpleAssignmentOperation`
- `IMemberInitializerOperation`
- `IInvocationOperation`

分别对应：

- 直接成员赋值
- 嵌套对象/集合初始化器
- 集合 `Add(...)` 一类调用

这让对象初始化器、集合初始化器和嵌套初始化器都可以共用同一条展开主线。

## 何时直接变成对象字面量

当前不是所有创建都必须保留构造语义。

会直接落成对象字面量的典型情况有：

- 匿名对象
- `record`
- 递归对象/集合初始化器生成的嵌套字面量

这和整体设计保持一致：

- 能直接用稳定 JS shape 表达时，优先直接表达
- 不为了保留 CLR 外形而额外制造宿主层

## 当前边界

- 不保证所有集合类型都能落成“最优” JS 宿主；优先保证语义正确和 lowering 稳定
- 不保证所有初始化器都绕开 IIFE；当求值顺序或初始化协议需要时，IIFE 仍然是合法实现手段
- 不追求保持 CLR 匿名类型 / `record` 的 runtime identity、构造协议或实例细节完全一致

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
