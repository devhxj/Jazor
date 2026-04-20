# `SenseArgument`（原 `WalkerArgument`）

## 定位

文档文件名仍叫 `WalkerArgument.md`，但当前真实实现已经是：

- `src/Jazor.Compiler/SenseArgument.cs`

类型名是：

- `SenseArgument`

它不是旧文档里的独立 `WalkerArgument` 类，而是把旧的上下文传递和依赖收集职责直接内联进了新的语义上下文结构。

## 当前职责

`SenseArgument` 当前同时承担两类职责：

### 1. 语义上下文传递

它携带的显式语义字段包括：

- `Sense`
- `PatternInput`
- `CatchExceptionVar`
- `SwitchExpressionVar`

这些字段分别服务于：

- 当前访问语义场景
- 模式匹配输入对象
- 裸 `throw;` 的 catch 变量恢复
- switch expression 的共享输入变量

### 2. 依赖收集

旧 `WalkerArgument` 的收集职责现在被直接内联到了 `SenseArgument`：

- `_declarators`
- `_specifiers`
- `_importBindings`

也就是说，它既是“语义上下文”，也是“依赖缓冲区”。

## 当前关键规则

### 1. `WithNewScope()` 只隔离变量声明，不隔离 import

这是当前设计里最关键的一条。

`WithNewScope()` 会：

- 创建新的 `_declarators`
- 复用已有 `_specifiers`
- 复用已有 `_importBindings`

这意味着：

- block 级局部变量声明不会泄漏
- import 依赖仍能继续向外汇总到模块级

### 2. `With...` 系列方法主要做轻量上下文复制

当前支持：

- `With(Sense sense)`
- `WithPatternInput(...)`
- `WithCatchVar(...)`
- `WithSwitchVar(...)`
- `With(Sense sense, Expression patternInput)`

这些方法不会重置已有依赖收集状态，而是复用当前收集器引用。

### 3. 变量声明以 `depth:name` 去重

`AddVarDeclarator(...)` 当前使用：

- `{depth}:{name}`

作为键。

这样可以在同一作用域层避免重复登记，同时允许不同深度出现同名变量的独立声明。

### 4. import 绑定名会稳定别名化

`BindImportSpecifier(...)` 不直接把外部导出名裸露进当前命名空间，而是生成：

- `i$...`

形式的稳定内部名。

这样做的目的很明确：

- 避免和用户代码顶层名冲突
- 避免不同模块相同导出名冲突

### 5. `FlushImportSpecifiers()` 会去重并清空状态

当前 flush import 时：

1. 先按 `specifier.ToECMAScript()` 去重
2. 返回按模块路径分组的结果
3. 清空 `_specifiers`
4. 同时清空 `_importBindings`

这说明 import 收集器是“单轮消费型缓冲区”。

### 6. `FlushVarDeclarator()` 也是消费型缓冲区

变量声明 flush 后会直接清空 `_declarators`。

因此外层 block / function body 必须在正确时机消费，否则声明不会自动再次出现。

## 现状与典型用法

### 模式匹配输入

```csharp
var patternArg = argument.WithPatternInput(inputValue);
```

随后 pattern 路径里的访问会从 `PatternInput` 读取共享输入对象。

### catch 变量恢复

```csharp
var catchContext = argument.WithCatchVar(exceptionParam.Name);
```

这样 `throw;` 才能在后续被还原成：

```js
throw ex;
```

### `out` 变量收集

```csharp
argument.AddVarDeclarator(declarator, _recursionDepth);
```

后续由外层：

```csharp
var declarators = argument.FlushVarDeclarator();
```

统一生成：

```js
let result, temp;
```

### 模块 import 收集

walker 会调用：

```csharp
argument.BindImportSpecifier(modulePath, importedName)
```

或：

```csharp
argument.MergeImportSpecifier(modulePath, specifier)
```

之后由 `AstConverter` 统一：

- `FlushImportSpecifiers()`
- `MergeImports(...)`
- 生成模块级 `ImportDeclaration`

## 当前边界

这部分当前已经解决的是：

- 语义上下文传递
- 模式输入传递
- catch 变量传递
- switch expression 共享输入传递
- 变量声明收集
- import specifier 收集与稳定别名绑定

它没有试图做这些事情：

- 提供完整不可变持久化集合语义
- 自动在任意层级隐式 flush
- 作为独立公共 API 面对外暴露复杂契约

## 与旧 `WalkerArgument` 的关系

如果看到旧文档、旧测试说明或历史笔记里提到 `WalkerArgument`，当前应按下面方式理解：

- 旧名字：`WalkerArgument`
- 当前名字：`SenseArgument`
- 旧职责：上下文 + 收集器
- 当前状态：这些职责已合并到 `SenseArgument`

也就是说，现在不是“`WalkerArgument` 被另一个类型取代后废弃不用”，而是它的核心职责被直接吸收了。

## 相关测试与使用面

当前没有独立的 `SenseArgument` 单测文件，但它被大量语义测试间接覆盖。

建议重点看：

- `src/Jazor.CompilerTest/SemanticWalkerDeclarationTest.cs`
  - `Visit_DeclarationExpression_OutVar`
- `src/Jazor.CompilerTest/SemanticWalkerPatternTest.cs`
  - 多个 `new SenseArgument(PatternInput: ...)` 场景
- `src/Jazor.CompilerTest/AstConverterTests.cs`
  - 间接覆盖 import / flush / 方法体上下文传播

## 推荐阅读

建议按这个顺序看：

1. [SemanticWalker.md](./SemanticWalker.md)
2. [WalkerArgument.md](./WalkerArgument.md)
3. [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
4. [AstConverter.md](./AstConverter.md)

## 相关文档

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [AstConverter.md](./AstConverter.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)
