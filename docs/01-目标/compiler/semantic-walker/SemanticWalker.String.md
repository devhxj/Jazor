# `SemanticWalker.cs.String.cs`

## 目录

- [定位](#定位)
- [职责](#职责)
- [关键规则](#关键规则)
- [现状与典型结果](#现状与典型结果)
- [和其他字符串映射的边界](#和其他字符串映射的边界)
- [边界](#边界)
- [相关测试](#相关测试)
- [延伸阅读](#延伸阅读)

## 定位

`SemanticWalker.cs.String.cs` 当前主要负责“插值字符串”相关 lowering。

对应代码：

- `src/Jazor.Compiler/core/SemanticWalker.cs.String.cs`

这份文件的职责边界需要明确一下：

- 它负责把 Roslyn 的插值字符串 `IOperation` 转成 `TemplateLiteral` 或 `StringLiteral`
- 它不负责大多数 `string` 实例方法、静态方法、属性映射

后者更多是由 `Reference` / `WhiteList` 路径处理，所以不能把所有字符串 API 的行为都归到这份文件。

## 职责

### 1. 插值文本片段

`VisitInterpolatedStringText(...)` 处理插值字符串中的静态文本部分。

例如：

```csharp
$"Hello {name}!"
```

其中 `"Hello "` 和 `"!"` 会先各自落成文本值，再参与模板字符串拼装。

### 2. 插值表达式片段

`VisitInterpolation(...)` 当前只负责返回插值里的表达式本体。

当前事实是：

- 它不会额外处理格式说明符
- 它不会主动引入文化区格式化逻辑
- `{expr:F2}` 这类格式信息当前不会在这里被展开

换言之，这一层只保留“表达式插入模板字符串”的核心语义。

### 3. 自定义 handler 的插值拼接树

`IInterpolatedStringAdditionOperation` 只会出现在多个插值字符串转换到自定义 interpolated-string handler 的协议中。它不是普通字符串拼接，也不是标准插值字符串的另一种表示。

当前编译器不支持自定义 interpolated-string handler，因此 `VisitInterpolatedStringAddition(...)` 与 handler creation/append operation 保持同一边界：

- 抛出 `OperationTransformationException`
- 保留 `OperationKind.InterpolatedStringAddition`
- 通过既有报告回调提供源码位置和可操作诊断

不能把该 operation 直接转换为 JavaScript `TemplateLiteral`，因为自定义 handler 的构造、条件求值和 `AppendLiteral` / `AppendFormatted` 方法可能携带任意用户语义。

### 4. 标准插值字符串

`VisitInterpolatedString(...)` 处理 `IInterpolatedStringOperation` 的标准路径。

它会：

1. 顺序遍历 `operation.Parts`
2. 文本部分写入 `quasis`
3. 表达式部分写入 `expressions`
4. 在需要时补空 quasi
5. 最后修正尾部 quasi 的 `tail`

这保证了生成的模板字符串满足 JS AST 对 quasi / expression 数量关系的要求。

## 关键规则

### 1. 表达式前必须有 quasi

JS `TemplateLiteral` 要求每个表达式前面都有一个 quasi。

所以当前实现在遇到插值表达式时，如果发现：

- `quasis.Count == expressions.Count`

就会先补一个空 quasi。

这让下面这些边界都能稳定成立：

- 以表达式开头
- 连续多个表达式
- 整个字符串只有表达式

### 2. 尾部 quasi 必须显式存在

如果字符串以表达式结尾，当前实现会补一个空尾 quasi，并把它标为 `tail: true`。

例如：

```csharp
$"Value: {x}"
```

最终需要的是：

```js
`Value: ${x}`
```

而不是缺少结尾 quasi 的非法模板结构。

### 3. 纯文本插值会退化成普通字符串字面量

如果整个插值字符串最终没有任何表达式，并且只剩一个 quasi，当前实现会直接返回 `StringLiteral`。

例如：

```csharp
$"Hello World"
```

会输出：

```js
'Hello World'
```

这说明当前设计并不执着于“源代码写了 `$""` 就必须保留模板字符串外形”，而是优先落成更直接的 JS 结果。

### 4. 标准插值与自定义 handler 必须分开

`VisitInterpolatedString(...)` 负责可直接落成 JavaScript 模板字符串的标准 C# 插值字符串。

`VisitInterpolatedStringAddition(...)` 属于自定义 handler conversion，当前显式拒绝。两者虽然都含有插值语法，但运行时合同不同，不能共享模板字符串 lowering。

## 现状与典型结果

### 普通插值

```csharp
string message = $"Hello {name}!";
```

```js
let message = `Hello ${name}!`;
```

### 以表达式开头

```csharp
string message = $"{count} items";
```

```js
let message = `${count} items`;
```

### 以表达式结尾

```csharp
string message = $"Value: {x}";
```

```js
let message = `Value: ${x}`;
```

### 连续表达式

```csharp
string message = $"{x}{y}{z}";
```

```js
let message = `${x}${y}${z}`;
```

### 纯文本插值

```csharp
string message = $"Hello World";
```

```js
let message = 'Hello World';
```

### 插值格式说明符当前会被忽略

```csharp
string formatted = $"Pi: {pi:F2}";
```

```js
let formatted = `Pi: ${pi}`;
```

这不是文档上的假设，而是当前实现的直接结果。

## 和其他字符串映射的边界

虽然 `SemanticWalkerStringTest` 里有大量 `string` API 测试，但这些行为并不都来自 `SemanticWalker.cs.String.cs`。

例如下列映射主要依赖宿主映射 / 白名单消费：

- `value.Length` -> `value.length`
- `value.Contains(...)` -> `value.includes(...)`
- `value.Substring(...)` -> `value.substring(...)`
- `string.Join(...)` -> `Array.from(parts).join(...)`
- `string.IsNullOrWhiteSpace(...)` -> `!value?.trim()`

阅读提示：

- “插值字符串 lowering 文档”

而不是：

- “整个 `string` 类型映射总文档”

如果关心字符串 API 如何落地，应同时看 `Reference` 和 `WhiteList` 文档。

## 边界

- 插值格式说明符的运行时实现
- `CultureInfo` / 本地化格式化
- 自定义 interpolated-string handler creation、addition 和 append 协议
- 全部 `string` 实例方法和静态方法映射

## 相关测试

主要测试在：

- `src/Jazor.CompilerTest/SemanticWalkerStringTest.cs`

建议重点关注以下场景：

- `Visit_InterpolatedString_Simple`
- `Visit_InterpolatedString_TextOnly`
- `Visit_InterpolatedString_StartsWithExpression`
- `Visit_InterpolatedString_EndsWithExpression`
- `Visit_InterpolatedString_ConsecutiveExpressions`
- `Visit_InterpolatedString_WithEscapes`
- `Visit_InterpolatedString_Multiline`
- `Visit_InterpolatedString_Format`

如果要看字符串 API 宿主映射，可以再对照：

- `Visit_String_Length`
- `Visit_String_Contains`
- `Visit_String_Substring`
- `Visit_String_Join`
- `Visit_String_IsNullOrWhiteSpace`

但这些测试不应被误读为全部由本文件独立负责。

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [InlineAstTemplateSpec.md](../InlineAstTemplateSpec.md)
