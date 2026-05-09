# `SemanticWalker.cs.Switch.cs`

## 目录

- [定位](#定位)
- [职责](#职责)
- [关键规则](#关键规则)
- [现状与典型结果](#现状与典型结果)
- [和 Pattern 路径的边界](#和-pattern-路径的边界)
- [边界](#边界)
- [相关测试](#相关测试)
- [延伸阅读](#延伸阅读)

## 定位

`SemanticWalker.cs.Switch.cs` 负责 `switch` 语句的分流和传统 `switch` case lowering。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.Switch.cs`

这份文件的边界需要说清楚：

- 传统常量 `switch` 的主体在这里
- 模式匹配 `switch` 的主体实现不在这里，而是转交给 `Pattern` 路径
- `switch expression` 也不在这里，而在 `SemanticWalker.cs.Pattern.cs`

阅读提示：

- “`switch` 语句入口和传统 case lowering 文档”

而不是：

- “所有 switch 相关语义的总文档”

## 职责

### 1. 入口分流

`VisitSwitch(...)` 当前先判断 `switch` 是否包含 pattern case：

- 包含 pattern case -> 调用 `VisitSwitchPatternMatching(...)`
- 不包含 pattern case -> 调用 `VisitSwitchTraditional(...)`

这意味着当前 `Switch` 文件本身既是实现文件，也是一个调度边界。

### 2. 传统 `switch` 生成

`VisitSwitchTraditional(...)` 负责最普通的 case/value 形式。

典型结果：

```csharp
switch (value)
{
    case 1:
        DoOne();
        break;
    case 2:
        DoTwo();
        break;
    default:
        DoDefault();
        break;
}
```

```js
switch (value) {
  case 1:
    DoOne();
    break;
  case 2:
    DoTwo();
    break;
  default:
    DoDefault();
    break;
}
```

### 3. case label 收敛

每个 `ISwitchCaseOperation` 可能包含多个 clause。

当前实现会：

1. 先把所有 clause 转成 `tests`
2. 再翻译 case body
3. 为每个 test 都生成一个 `SwitchCase`

但有一个关键规则：

- 共享 body 的多个 label 中，真正的 body 只挂在最后一个 label 上

这样 `case 1: case 2: ...` 命中 `case 2` 时，仍然能落到真实语句体，而不是变成空穿透。

### 4. `default` 的处理

`VisitDefaultCaseClause(...)` 本身不生成节点。

当前做法是：

- 在 `VisitSwitchTraditional(...)` 中看到 `CaseKind.Default`
- 直接往 `tests` 里放 `null`
- 再由 `SwitchCase(test: null, ...)` 表示 `default`

所以 `VisitDefaultCaseClause(...)` 更像一个保留入口，而不是实际构造点。

### 5. 单值 case 子句

`VisitSingleValueCaseClause(...)` 当前非常直接：

- 把 case value 翻译成一个普通表达式

它不在这里额外做模式语义处理。

## 关键规则

### 1. 模式 `switch` 不在本文件展开

当前如果 `switch` 里出现 pattern case，这里只负责识别并分流。

真正的模式匹配实现位于：

- `SemanticWalker.cs.Pattern.cs`

这点和旧文档里“模式 switch 尚未实现”的说法已经不一致，当前文档必须按实际代码纠正。

### 2. 多个 case label 共享 body 时，body 挂到最后一个 label

这是传统 `switch` 转换里最重要的结构规则。

例如：

```csharp
case 1:
case 2:
    break;
```

会生成：

```js
case 1:
case 2:
  break;
```

而不是把 `break` 错挂到第一个 label 上。

### 3. `default` 通过 `test = null` 表示

这与 Acornima/ESTree 的 `SwitchCase` 约定一致，不需要额外造特殊节点。

### 4. case body 允许 statement / expression 混合

当前翻译 case body 时：

- `Statement` 直接加入 consequent
- `Expression` 包装成 `NonSpecialExpressionStatement`

这保证普通表达式也能稳定出现在 case 体内。

## 现状与典型结果

### 单个 case

```csharp
switch (value)
{
    case 1:
        break;
}
```

```js
switch (value) {
  case 1:
    break;
}
```

### 多个 case

```csharp
switch (value)
{
    case 1:
        break;
    case 2:
        break;
}
```

```js
switch (value) {
  case 1:
    break;
  case 2:
    break;
}
```

### fallthrough

```csharp
switch (value)
{
    case 1:
    case 2:
        break;
}
```

```js
switch (value) {
  case 1:
  case 2:
    break;
}
```

### `default`

```csharp
switch (value)
{
    default:
        break;
}
```

```js
switch (value) {
  default:
    break;
}
```

## 和 Pattern 路径的边界

当前这些能力不在本文件内完成：

- pattern case
- `when` 守卫参与的模式分支
- `switch expression`
- property pattern / relational pattern / tuple pattern

它们属于：

- `SemanticWalker.cs.Pattern.cs`

所以阅读 `SemanticWalkerSwitchTest` 时，需要区分：

- 传统 switch 测试用于验证本文件
- pattern / switch expression 测试更多是在验证 `Pattern` 路径

## 边界

这部分当前已经解决的是：

- `switch` 入口分流
- 传统常量 `switch`
- `default`
- 多 label 共享 body

它并未承担以下职责：

- 在本文件内部实现完整模式匹配 switch
- 在本文件内部实现 `switch expression`
- 把所有 `switch` 语义集中到一个 partial 文件里

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerSwitchTest.cs`

建议重点关注以下场景：

- `VisitSwitch_SingleCase`
- `VisitSwitch_MultipleCases`
- `VisitSwitch_WithDefault`
- `VisitSwitch_Fallthrough`
- `VisitSwitch_MultipleFallthrough`
- `VisitSwitch_MultipleCasesSameCode`

如果要看模式路径，再对照：

- `VisitSwitch_PatternMatching_TypePattern`
- `VisitSwitch_PatternMatching_RelationalPattern`
- `VisitSwitch_WithWhenClause`
- `VisitSwitchExpression_SimpleConstants`

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [SemanticWalker.Ordinary.md](./SemanticWalker.Ordinary.md)
- [SyntaxTransformationPipeline.md](../SyntaxTransformationPipeline.md)
