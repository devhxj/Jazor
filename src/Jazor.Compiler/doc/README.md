# Jazor.Compiler 文档索引

## 概述

本文档汇总 Jazor.Compiler 当前仍然有参考价值的设计说明、实现说明和专题文档。

这里优先指向“当前实现事实”和“已确认的设计约束”，不再把历史分析、阶段性判断和已过时的缺陷列表作为主入口。

## 推荐阅读路径

如果你是第一次进入该目录，建议按以下顺序阅读：

1. [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)
2. [ArchitectureOverview.md](./ArchitectureOverview.md)
3. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
4. [TransformationRoadmap.md](./TransformationRoadmap.md)
5. [WhiteList.md](./WhiteList.md)
6. [SemanticWalker.md](./SemanticWalker.md)
7. [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
8. [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

---

## 核心文件列表

| 文件 | 职责 | 文档 |
|------|------|------|
| `AstConverter.cs` | 类级别转换器 | [AstConverter.md](./AstConverter.md) |
| `core/SemanticWalker.cs` | 操作级别转换器（主文件） | [SemanticWalker.md](./SemanticWalker.md) |
| `WalkerArgument.cs` | 转换上下文参数 | [WalkerArgument.md](./WalkerArgument.md) |
| `Sense.cs` | 语义上下文枚举 | - |
| `SenseArgument.cs` | 语义上下文参数 | - |
| `TypeMapper.cs` | 类型映射枚举 | - |
| `WhiteList.cs` | 白名单核心 | [WhiteList.md](./WhiteList.md) |
| `Optimizer.cs` | AST 优化器 | [Optimizer.md](./Optimizer.md) |
| `ESGenerator.cs` | 增量源生成器 | [ESGenerator.md](./ESGenerator.md) |
| `Transformation Pipeline` | 端到端语法转化总说明 | [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md) |
| `Runtime Static Host Resolution` | 运行时静态成员宿主选择与继承兼容规则 | [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md) |
| `Architecture Overview` | 编译器整体方案架构图与职责边界 | [ArchitectureOverview.md](./ArchitectureOverview.md) |
| `Architecture Simplified` | 面向新成员和汇报的一页版架构说明 | [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md) |
| `Transformation Roadmap` | 闭环状态、欠账和下一阶段动作 | [TransformationRoadmap.md](./TransformationRoadmap.md) |
| `Module Conversion Spec` | 模块层转换规范 | [ModuleConversionSpec.md](./ModuleConversionSpec.md) |
| `Walker Extension Spec` | `SemanticWalker` 扩展规范 | [WalkerExtensionSpec.md](./WalkerExtensionSpec.md) |
| `Inline AST Template Spec` | `Op.Inline` 的 AST 模板规范 | [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md) |
| `OpCompile Spec` | `Op.Compile` 的分发语义与边界约定 | [OpCompileSpec.md](./OpCompileSpec.md) |
| `OpCompile Checklist` | `Op.Compile` 的分阶段实施清单 | [OpCompileImplementationChecklist.md](./OpCompileImplementationChecklist.md) |
| `Closure Checklist` | 转化链路闭环清单 | [TransformationClosureChecklist.md](./TransformationClosureChecklist.md) |
| `SourceMap Decision Summary` | sourcemap 简版决策摘要 | [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md) |
| `SourceMap Overview` | sourcemap 文档总览与阅读顺序 | [SourceMap.Overview.md](./SourceMap.Overview.md) |
| `SourceMap Design` | sourcemap 完整设计方案 | [SourceMap.Design.md](./SourceMap.Design.md) |
| `SourceMap Checklist` | sourcemap 后续实施清单 | [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md) |
| `SourceMap Pitfalls` | sourcemap 实施注意事项与易踩坑清单 | [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md) |
| `SourceMap Hard Rules` | sourcemap 第一阶段必须遵守的硬约束 | [SourceMap.HardRules.md](./SourceMap.HardRules.md) |

---

## SemanticWalker 分部文件索引

| 文件 | 职责 | 行数 | 文档 |
|------|------|------|------|
| `SemanticWalker.cs` | 主入口、类型映射、Translate 方法族 | ~470 | [SemanticWalker.md](./SemanticWalker.md) |
| `SemanticWalker.cs.Pattern.cs` | 模式匹配 | ~800+ | [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md) |
| `SemanticWalker.cs.Reference.cs` | 字段/属性/方法引用 | ~585 | [SemanticWalker.Reference.md](./SemanticWalker.Reference.md) |
| `SemanticWalker.cs.Loop.cs` | 循环语句 | ~145 | [SemanticWalker.Loop.md](./SemanticWalker.Loop.md) |
| `SemanticWalker.cs.Switch.cs` | Switch 语句/表达式 | ~170 | [SemanticWalker.Switch.md](./SemanticWalker.Switch.md) |
| `SemanticWalker.cs.String.cs` | 字符串插值 | ~189 | [SemanticWalker.String.md](./SemanticWalker.String.md) |
| `SemanticWalker.cs.TryCatch.cs` | 异常处理 | ~240 | [SemanticWalker.TryCatch.md](./SemanticWalker.TryCatch.md) |
| `SemanticWalker.cs.Creation.cs` | 对象/数组创建 | ~422 | [SemanticWalker.Creation.md](./SemanticWalker.Creation.md) |
| `SemanticWalker.cs.Tuple.cs` | 元组和解构 | ~560 | [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md) |
| `SemanticWalker.cs.Declaration.cs` | 变量声明 | ~140 | [SemanticWalker.Declaration.md](./SemanticWalker.Declaration.md) |
| `SemanticWalker.cs.Ordinary.cs` | 二元/一元运算 | ~800+ | [SemanticWalker.Ordinary.md](./SemanticWalker.Ordinary.md) |
| `SemanticWalker.cs.Invalid.cs` | IInvalidOperation 处理 | ~152 | [SemanticWalker.Invalid.md](./SemanticWalker.Invalid.md) |
| `SemanticWalker.cs.NotSupport.cs` | 不支持的操作 | ~525 | [SemanticWalker.NotSupport.md](./SemanticWalker.NotSupport.md) |
| `SemanticWalker.cs.WhiteList.cs` | 白名单处理 | ~130 | [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md) |
| `SemanticWalker.cs.Generate.cs` | 白名单生成 | 自动生成 | - |

---

## 设计决策说明

### 关于 Parser 的使用

对于白名单中的内联代码模板（`Op.Inline`），使用 Parser 解析是**必要的设计选择**，而非缺陷。原因如下：

1. **模板复杂性**：内联代码模板可能包含任意复杂的 JavaScript 表达式
2. **维护成本**：为每种可能的 AST 结构编写直接构造代码会导致代码量爆炸
3. **运行时安全**：Parser 提供了语法验证和标准 AST 生成

详见 [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md#43-使用-parser-解析内联代码的设计决策)。

### 关于变量声明与作用域隔离

**核心原则**：凡是对应 JS 函数边界或独立作用域的地方，必须隔离 `_declarators`，共享 `_specifiers`。

| 数据 | 传播方向 | 函数边界行为 |
|------|---------|------------|
| `_declarators`（变量声明） | 向上冒泡到最近的块 | **不能**穿越函数边界 |
| `_specifiers`（import 声明） | 向上冒泡到模块顶层 | **必须**穿越函数边界 |

`WalkerArgument.WithNewDeclarators()` 实现了这个隔离策略。

---

## 使用建议

如果只是想快速建立当前实现认知，推荐按这个顺序阅读：

1. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
2. [SemanticWalker.md](./SemanticWalker.md)
3. [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
4. [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
5. [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

如果问题落在宿主 API 映射、运行时 shape 或命名边界，优先看：

- [WhiteList.md](./WhiteList.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [OpCompileSpec.md](./OpCompileSpec.md)
- [OpCompileImplementationChecklist.md](./OpCompileImplementationChecklist.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

如果问题落在 sourcemap，优先看：

- [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
- [SourceMap.Design.md](./SourceMap.Design.md)
- [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md)

---

## 相关文档

- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
- [SemanticWalker.md](./SemanticWalker.md)
- [WhiteList.md](./WhiteList.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

---
