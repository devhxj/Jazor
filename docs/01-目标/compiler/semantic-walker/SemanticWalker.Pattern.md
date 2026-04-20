# `SemanticWalker.cs.Pattern.cs`

## 定位

`SemanticWalker.cs.Pattern.cs` 负责模式匹配相关语义节点的 lowering。

它覆盖的范围不只是 `expr is pattern`，还包括：

- `is` 模式
- `switch` 语句里的模式 case
- `switch` 表达式里的模式 arm
- 常量、类型、声明、关系、逻辑、递归、列表、切片等模式

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Pattern.cs`

## 当前核心前提

这部分逻辑现在建立在一个明确前提上：

> pattern 本身不再靠向上遍历父节点去找输入表达式，而是通过 `SenseArgument.PatternInput` 显式传入。

这也是当前模式域和旧文档最重要的差异。

当前典型入口：

- `VisitIsPattern(...)`
  先求值被匹配对象，再把它放进 `PatternInput`
- `VisitSwitchExpression(...)`
  先把输入表达式缓存到临时变量，再把该变量作为 `PatternInput` 传给所有 arm
- `VisitSwitchPatternMatching(...)`
  对模式 `switch` 语句也采用同样思路

这让 pattern lowering 的关键依赖变成显式上下文，而不是“必须拿到完整父操作树才能工作”。

## 当前职责

从现状看，这部分逻辑可以分成五条主线。

### 1. 基础模式表达式

包括：

- 常量模式
- 类型模式
- 关系模式
- 丢弃模式

这些模式都会基于当前 `PatternInput` 直接生成布尔条件表达式。

例如：

```csharp
value is 42
```

```js
value === 42
```

```csharp
value is > 0
```

```js
value > 0
```

### 2. 逻辑组合模式

包括：

- `not`
- `and`
- `or`

当前直接 lower 成 JS 逻辑表达式：

- `not` -> `!`
- `and` -> `&&`
- `or` -> `||`

这里的核心不是语法映射本身，而是保证左右子模式都共享同一个 `PatternInput`，从而保持 C# 模式组合语义。

### 3. 声明模式与变量捕获

声明模式不只是“类型检查”，还会引入变量绑定。

例如：

```csharp
obj is string s
```

当前会生成：

- 类型匹配条件
- 以及对 `s` 的绑定赋值

这部分由 `BuildDeclarationPattern(...)` 负责。

它的重点不是创建新的运行时模式对象，而是：

- 用当前 `PatternInput` 作为绑定来源
- 在合适位置插入声明/赋值
- 让后续 guard、case body、arm value 能复用这个绑定结果

### 4. 递归 / 属性 / 位置模式

递归模式是模式域里最复杂的一类。

它统一承载：

- 属性模式
- 位置模式
- 记录类型位置解构模式
- 类型 + 子模式的组合

例如：

```csharp
obj is Person { Name: "John", Age: > 18 }
```

或：

```csharp
value is Person("John", 18)
```

当前实现会：

1. 先决定是否需要类型检查
2. 再为每个属性子模式构造新的访问表达式
3. 再把这个访问表达式作为新的 `PatternInput` 传入子模式
4. 最终把所有条件用 `&&` 组合起来

也就是说，递归模式不是特殊语法糖层，而是一层“递归构造访问路径 + 递归传递 `PatternInput`”的组合逻辑。

### 5. 列表与切片模式

列表模式当前已经是完整的一条单独 lowering 路径。

它会处理：

- 空列表模式
- 固定元素列表模式
- 带切片的列表模式
- 列表中的声明模式
- 嵌套列表模式

整体思路是：

1. 先确认输入是可按索引访问的列表形态
2. 再检查长度条件
3. 再为每个元素构造索引访问表达式
4. 把索引访问结果作为新的 `PatternInput` 传给子模式

切片模式本身不是单独生成最终判定链，而是服务于列表模式中的“剩余片段”匹配。

## `PatternInput` 的作用

这是当前模式域最关键的设计点。

### 为什么必须显式传

因为一个 pattern 只有和“当前正在匹配谁”绑定起来，才有意义。

例如：

- 常量模式要比较谁是否等于常量
- 关系模式要比较谁是否大于/小于某值
- 属性模式要从谁身上继续取属性
- 列表模式要从谁身上继续按索引取元素

这个“谁”现在统一由 `SenseArgument.PatternInput` 提供。

### 当前直接依赖它的路径

最典型的有：

- `VisitConstantPattern(...)`
- `VisitRelationalPattern(...)`
- `VisitTypePattern(...)`
- `VisitPropertySubpattern(...)`
- `VisitListPattern(...)`
- `BuildDeclarationPattern(...)`
- `GetPatternRefrence(...)`

如果没有 `PatternInput`，这些 lowering 本身就没有稳定的目标对象可用。

### 当前实现收益

这样做的收益很直接：

- 模式本身更容易独立测试
- `switch expression` / `switch statement` / `is pattern` 可以共用同一套 lowering
- 不需要再把“输入表达式是谁”藏在父节点结构里

## switch 表达式与 switch 语句

当前模式域不只是处理单个 pattern，还负责两类“模式驱动控制流”。

### 1. `switch` 表达式

当前模式 `switch` 表达式会：

1. 先把输入表达式缓存到临时变量
2. 用 IIFE 包起来
3. 逐个 arm 生成 `if (...) return ...`

这样做的原因是：

- 输入表达式可能有副作用
- 每个 arm 可能依赖同一个 pattern 绑定
- 最终要保持 C# `switch` 表达式的单值返回语义

### 2. 模式 `switch` 语句

模式 `switch` 语句也采用类似思路：

- 先缓存输入
- 再把每个 case lowering 成显式条件分支

这里的重点不是把它尽量还原成 JS 原生 `switch`，而是保证模式语义和 guard 顺序正确。

## 当前模式类型概览

当前文档层面可以把支持面概括为：

- 常量模式
- 类型模式
- 声明模式
- 关系模式
- `not` 模式
- `and` / `or` 模式
- 递归模式
- 属性子模式
- 列表模式
- 切片模式
- 丢弃模式

这也是 `SemanticWalkerPatternTest` 当前主要覆盖的测试面。

## 当前生成风格

模式域当前生成代码的偏好非常明确：

- 优先生成显式布尔条件链
- 需要时插入临时变量
- 不为模式匹配额外引入运行时模式对象

这和整个编译器当前风格保持一致：

- 结果等价优先
- 求值顺序正确优先
- 运行时额外抽象最少

## 当前边界

这份文件当前不承诺这些事情：

- 把所有模式匹配都压缩成最短 JS
- 用统一运行时 helper 封装所有模式逻辑
- 完整复刻 CLR 内部模式匹配实现细节

它当前更偏向：

- 把 pattern lowering 展开成显式、可验证的 AST 条件结构
- 通过 `PatternInput` 统一上下文模型
- 让声明模式、guard、列表模式、递归模式共用同一套基础规则

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerPatternTest.cs`

建议重点关注这些测试面：

- `PatternInput` 直接传入的单模式测试
- 声明模式变量捕获
- 复杂递归属性模式
- 列表 + 切片 + 嵌套列表模式
- 模式 `switch` 表达式
- 模式 `switch` 语句

## 推荐阅读

建议按这个顺序看：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
3. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
4. [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [Optimizer.md](./Optimizer.md)
