# Compiler 文档索引

> Updated: 2026-04-26

这组文档覆盖编译器的架构、转换管线、白名单、宿主语义、SourceMap 和发射。实现路线和裁决原则在源码目录的另一份文档里：

- [src/Jazor.Compiler/ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md) — 实现路线、失败策略、行为保真顺序、扩展判据

本目录偏仓库级设计组织和专题索引；`ImplementationPrinciples.md` 偏价值排序和裁决。两者不重复。

## 阅读路线

**先建立整体图景：** [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md) → [ArchitectureOverview.md](./ArchitectureOverview.md) → [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md) → [ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)

**准备动手改 compiler：** [ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md) → [ArchitectureOverview.md](./ArchitectureOverview.md) → [WalkerExtensionSpec.md](./WalkerExtensionSpec.md) → [WhiteList.md](./WhiteList.md) → [OpCompileSpec.md](./OpCompileSpec.md)

**按专题深挖：** `AstConverter`（模块/类型级展开）· `SemanticWalker`（方法体与表达式 lowering）· `WhiteList` / `Compile`（宿主语义接缝）· `SourceMap`（调试锚点与生成映射）· `Emit`（产物物化与 SourceMap 输出）

## 主题索引

### 总览与主链路

- [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md) - 快速理解输入域、宿主映射、转换核心与输出闭环
- [ArchitectureOverview.md](./ArchitectureOverview.md) - 完整架构总览、分层职责、扩展点和术语表
- [Compiler.HardRules.md](./Compiler.HardRules.md) - 已收口 compiler 语义与输出边界的一页硬规则摘要
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

## 约定

- 仓库级架构和专题入口看本目录；实现路线和边界裁决看 [ImplementationPrinciples.md](../../../src/Jazor.Compiler/ImplementationPrinciples.md)。
- 如果旧文档与实现原则冲突，以 `ImplementationPrinciples.md` 和现有测试为准，回头修正文档。

## 相关入口

- [../README.md](../README.md) — `01-目标` 总入口
- [../../README.md](../../README.md) — 仓库级文档中心
- [architecture.md](./architecture.md) — 编译器架构桥接页
