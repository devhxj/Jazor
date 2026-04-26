# Compiler 文档索引

> Status: 活跃参考
> Updated: 2026-04-26
> Positioning: 仓库级 `Jazor.Compiler` 入口，负责组织架构、转换、白名单、SourceMap 与实现原则的阅读路径。
> Note: 这份索引负责组织“长期有效的专题入口”；如果你要裁决当前实现路线和失败策略，优先回到 `src/Jazor.Compiler/ImplementationPrinciples.md`。

## 定位

`docs/01-目标/compiler/` 这组文档回答的是：

- 编译器为什么存在；
- 编译链路如何分层；
- 核心转换器分别解决什么问题；
- 白名单、宿主语义、SourceMap 和发射管线如何协作；
- 新增能力时应落在哪一层。

与之配套但不重复的另一份核心文档在源码目录：

- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

那份文档回答的是“实现路线与裁决原则”，更偏价值排序、失败策略、synthetic lowering 边界和扩展判据；本目录则更偏仓库级设计组织与专题索引。

## 推荐阅读顺序

### 路线 A：先建立整体图景

1. [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)
2. [ArchitectureOverview.md](./ArchitectureOverview.md)
3. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
4. [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

### 路线 B：准备实现或修改 compiler

1. [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)
2. [ArchitectureOverview.md](./ArchitectureOverview.md)
3. [WalkerExtensionSpec.md](./WalkerExtensionSpec.md)
4. [WhiteList.md](./WhiteList.md)
5. [OpCompileSpec.md](./OpCompileSpec.md)

### 路线 C：按专题深挖

- `AstConverter`：模块/类型级展开
- `SemanticWalker`：方法体与表达式 lowering
- `WhiteList` / `Compile`：宿主语义接缝
- `SourceMap`：调试锚点与生成映射
- `Emit`：产物物化与 SourceMap 输出

## 主题索引

### 总览与主链路

- [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md) - 快速理解输入域、宿主映射、转换核心与输出闭环
- [ArchitectureOverview.md](./ArchitectureOverview.md) - 完整架构总览、分层职责、扩展点和术语表
- [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md) - 端到端转换链路与阶段职责
- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md) - 实现路线、失败策略、行为保真顺序与扩展决策原则

### 模块级与语义级转换

- [AstConverter.md](./AstConverter.md) - 模块级转换、成员展开、导出结构
- [ModuleConversionSpec.md](./ModuleConversionSpec.md) - 类/成员到 ES module 的转换规范
- [WalkerExtensionSpec.md](./WalkerExtensionSpec.md) - `SemanticWalker` 扩展约定
- [semantic-walker/SemanticWalker.md](./semantic-walker/SemanticWalker.md) - `SemanticWalker` 总览

### 宿主语义与白名单

- [WhiteList.md](./WhiteList.md) - 白名单模型、生成来源与消费边界
- [OpCompileSpec.md](./OpCompileSpec.md) - `Compile` 路线的适用范围与规则
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md) - `Inline` AST 模板路径
- [InlineImportAudit.md](./InlineImportAudit.md) - `Inline` / `Import` 相关审计与收口
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md) - 运行时静态宿主解析策略

### 发射稳定性与辅助设施

- [StableUniqueNameAllocation.md](./StableUniqueNameAllocation.md) - 稳定命名与唯一名分配
- [Optimizer.md](./Optimizer.md) - AST 优化器的职责边界
- [InspectableStatementAnnotation.md](./InspectableStatementAnnotation.md) - 可检查语句标注

### SourceMap 与 Emit

- [sourcemap/SourceMap.Overview.md](./sourcemap/SourceMap.Overview.md) - SourceMap 总览
- [sourcemap/SourceMap.Design.md](./sourcemap/SourceMap.Design.md) - SourceMap 设计
- [emit/Emit.Pipeline.Overview.md](./emit/Emit.Pipeline.Overview.md) - 发射总览
- [emit/Emit.BundleAndSourceMap.Overview.md](./emit/Emit.BundleAndSourceMap.Overview.md) - 打包与 SourceMap 输出

## 使用约定

- 仓库级架构和专题入口优先看本目录。
- 实现路线、边界裁决、价值排序优先看 [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)。
- 如果某份旧文档与实现原则文档冲突，以 `ImplementationPrinciples.md` 记录的当前路线和现有测试约束为准，再回头修正文档漂移。

## 相关入口

- [../README.md](../README.md) - `01-目标` 总入口
- [../../README.md](../../README.md) - 仓库级文档中心
- [architecture.md](./architecture.md) - 编译器架构桥接页
