# `VisitInvalid` 现状说明

## 定位

`IInvalidOperation` 的处理入口目前不在单独的 `SemanticWalker.cs.Invalid.cs` 文件中，而是在：

- `src/Jazor.Compiler/core/SemanticWalker.cs.NotSupport.cs`

对应实现是：

- `VisitInvalid(IInvalidOperation operation, SenseArgument argument)`

这和旧文档描述的“语法节点级回退转换器”已经不一致，当前文档需要按真实代码修正。

## 职责

当前 `VisitInvalid(...)` 的职责非常简单：

- 遇到 `IInvalidOperation` 时直接失败
- 通过 `HandleTransformationFailure(...)` 抛出不支持转换错误

换言之，当前实现把 `IInvalidOperation` 视为：

- 不应进入正常 lowering 主线的异常输入

而不是：

- 需要额外兜底支持的一类常规节点

## 关键规则

### 1. 没有语法级兜底转换

旧文档里提到的这套策略当前并不存在：

- 从 `IInvalidOperation` 回退到 `SyntaxNode`
- 再按语法节点类型手工构造 AST

当前真实实现没有这条路径。

### 2. `IInvalidOperation` 被视为异常状态

从测试注释和实现现状看，当前假设是：

- 在诊断正常、语义分析正常的输入下，理论上不应依赖 `VisitInvalid(...)`

因此这里选择的是：

- 快速失败

而不是：

- 带有较强猜测性的容错转换

### 3. `VisitInvalid` 属于不支持策略的一部分

它当前与 dynamic、函数指针、UTF-8 字符串等不支持节点放在同一文件中，不是偶然。

这表达了当前设计态度：

- `IInvalidOperation` 不是一个“弱支持分支”
- 而是与其他无法稳定映射到 JS 的输入一样，直接拒绝

## 现状与测试

当前仓库里关于 `Invalid` 的测试文件是：

- `src/Jazor.CompilerTest/SemanticWalkerInvalidTest.cs`

但该文件中的直接测试目前处于注释状态，并且文件开头已经明确写了：

- 理论上在没有诊断错误的情况下，不应该出现 `InvalidOperation`

这和当前实现是一致的。

## 边界

这部分当前没有提供这些能力：

- 对 `IInvalidOperation` 的语法级 fallback
- 基于 `SyntaxNode` 的手工推断式 AST 恢复
- 把语义异常节点继续“尽力转换”

它当前提供的只有：

- 一个明确的失败点
- 一个清晰的设计信号：`IInvalidOperation` 不在受支持 lowering 面内

## 延伸阅读

- [SemanticWalker.md](./SemanticWalker.md)
- [SemanticWalker.NotSupport.md](./SemanticWalker.NotSupport.md)
- [SyntaxTransformationPipeline.md](../SyntaxTransformationPipeline.md)
