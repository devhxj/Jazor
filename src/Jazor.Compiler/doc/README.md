# Jazor.Compiler 文档索引

## 概述

本文档汇总 `Jazor.Compiler/doc` 当前仍有参考价值的设计说明、实现说明、专题文档与阶段性材料。

整理原则：

- 优先指向“当前实现事实”和“长期有效的设计约束”
- 将专题文档通过专题入口聚合，而不是在根索引中平铺全部子文档
- 将阶段性计划、实施清单、复核记录与主规范分层，降低过时信息干扰

---

## 一、入口文档

如果你是第一次进入该目录，建议先读这些文档：

1. [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)
2. [ArchitectureOverview.md](./ArchitectureOverview.md)
3. [TransformationRoadmap.md](./TransformationRoadmap.md)
4. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)

这四份文档分别负责：

- 一页版整体认知
- 完整架构边界
- 当前闭环状态与下一阶段动作
- 编译主链路与转换流程

---

## 二、长期有效规范

这部分文档更偏“长期有效的设计/实现说明”，适合作为稳定参考。

### 2.1 转换核心

- [AstConverter.md](./AstConverter.md)
- [SemanticWalker.md](./SemanticWalker.md)
- [WalkerArgument.md](./WalkerArgument.md)
- [ModuleConversionSpec.md](./ModuleConversionSpec.md)
- [WalkerExtensionSpec.md](./WalkerExtensionSpec.md)

`SemanticWalker` 分部文档请从主入口进入：

- [SemanticWalker.Pattern.md](./SemanticWalker.Pattern.md)
- [SemanticWalker.Reference.md](./SemanticWalker.Reference.md)
- [SemanticWalker.Loop.md](./SemanticWalker.Loop.md)
- [SemanticWalker.Switch.md](./SemanticWalker.Switch.md)
- [SemanticWalker.String.md](./SemanticWalker.String.md)
- [SemanticWalker.TryCatch.md](./SemanticWalker.TryCatch.md)
- [SemanticWalker.Creation.md](./SemanticWalker.Creation.md)
- [SemanticWalker.Tuple.md](./SemanticWalker.Tuple.md)
- [SemanticWalker.Declaration.md](./SemanticWalker.Declaration.md)
- [SemanticWalker.Invalid.md](./SemanticWalker.Invalid.md)
- [SemanticWalker.NotSupport.md](./SemanticWalker.NotSupport.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)

### 2.2 白名单 / 宿主映射 / 运行时边界

- [WhiteList.md](./WhiteList.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)
- [InlineAstTemplateSpec.md](./InlineAstTemplateSpec.md)
- [OpCompileSpec.md](./OpCompileSpec.md)

### 2.3 生成与输出

- [ESGenerator.md](./ESGenerator.md)
- [Optimizer.md](./Optimizer.md)

---

## 三、专题文档

专题文档采用“专题入口 -> 详细设计/规则/清单”的结构。根索引只保留专题入口，避免信息平铺。

### 3.1 SourceMap 专题

入口：

- [SourceMap.Overview.md](./SourceMap.Overview.md)

状态：

- 设计已明确
- 当前策略是暂缓实现，等待编译器主体进一步稳定

专题内包含：决策摘要、完整设计、硬约束、实施清单、坑点说明。

### 3.2 RazorVue 专题

入口：

- [RazorVue.Overview.md](./RazorVue.Overview.md)

状态：

- 主方向已定
- 已有部分实现落地
- phase one 范围刻意收敛

专题内包含：决策摘要、完整设计、项目职责拆分、组件描述符规范、`DenoHost` 契约、硬约束、坑点与实施材料。

### 3.3 RazorVue HMR 子专题

入口：

- [RazorVue.Hmr.Overview.md](./RazorVue.Hmr.Overview.md)

状态：

- 架构已预留
- runtime HMR 尚未完整实现
- 当前以 compiler-owned identity / metadata 设计为主
- 当前不应把它理解为 active runtime implementation lane

专题内包含：决策摘要、完整设计、硬约束、实施清单、坑点说明。

---

## 四、阶段性材料

这一组文档更偏“阶段计划 / 实施清单 / 复核记录”。

为了降低 active 与 historical 混杂，本节拆成两类：

- 当前仍可能直接指导执行的材料
- 更偏历史上下文或阶段性复核的材料

### 4.1 当前 active implementation materials

#### 编译器主线

- [TransformationClosureChecklist.md](./TransformationClosureChecklist.md)
- [OpCompileImplementationChecklist.md](./OpCompileImplementationChecklist.md)

#### SourceMap

- [SourceMap.DecisionSummary.md](./SourceMap.DecisionSummary.md)
- [SourceMap.HardRules.md](./SourceMap.HardRules.md)
- [SourceMap.ImplementationChecklist.md](./SourceMap.ImplementationChecklist.md)
- [SourceMap.Pitfalls.md](./SourceMap.Pitfalls.md)
- [SourceMap.Design.md](./SourceMap.Design.md)

#### RazorVue

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ProjectResponsibilities.md](./RazorVue.ProjectResponsibilities.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.Pitfalls.md](./RazorVue.Pitfalls.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)

#### RazorVue HMR

- [RazorVue.Hmr.DecisionSummary.md](./RazorVue.Hmr.DecisionSummary.md)
- [RazorVue.Hmr.Design.md](./RazorVue.Hmr.Design.md)
- [RazorVue.Hmr.HardRules.md](./RazorVue.Hmr.HardRules.md)
- [RazorVue.Hmr.ImplementationChecklist.md](./RazorVue.Hmr.ImplementationChecklist.md)
- [RazorVue.Hmr.Pitfalls.md](./RazorVue.Hmr.Pitfalls.md)

说明：

- 这一组当前更接近 reserved architecture lane
- 若未来进入 active implementation，应先更新各文档 `Status` 与 repo-level bridge

### 4.2 历史或阶段性上下文材料

- [RazorVue.FirstPrPlan.md](./RazorVue.FirstPrPlan.md)
- [RazorVue.Review.md](./RazorVue.Review.md)

说明：

- 是否 active，优先看文档开头的 `Status`
- repo-level 当前执行入口请看 [docs/plans/project-execution-index.md](../../../docs/plans/project-execution-index.md)

---

## 五、核心文件索引

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
| `Runtime Static Host Resolution` | 运行时静态成员宿主选择与继承兼容规则 | [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md) |

---

## 六、使用建议

### 6.1 想快速建立整体认知

按这个顺序读：

1. [ArchitectureOverview.Simplified.md](./ArchitectureOverview.Simplified.md)
2. [ArchitectureOverview.md](./ArchitectureOverview.md)
3. [TransformationRoadmap.md](./TransformationRoadmap.md)
4. [SyntaxTransformationPipeline.md](./SyntaxTransformationPipeline.md)
5. [SemanticWalker.md](./SemanticWalker.md)

### 6.2 问题落在宿主 API 映射 / 运行时 shape / 命名边界

优先看：

- [WhiteList.md](./WhiteList.md)
- [SemanticWalker.WhiteList.md](./SemanticWalker.WhiteList.md)
- [OpCompileSpec.md](./OpCompileSpec.md)
- [RuntimeStaticHostResolution.md](./RuntimeStaticHostResolution.md)

### 6.3 问题落在 SourceMap

先从专题入口进入：

- [SourceMap.Overview.md](./SourceMap.Overview.md)

### 6.4 问题落在 RazorVue

先从专题入口进入：

- [RazorVue.Overview.md](./RazorVue.Overview.md)
- [RazorVue.Hmr.Overview.md](./RazorVue.Hmr.Overview.md)

---

## 七、备注

当前目录中有一部分文档属于“阶段性实施材料”，例如首批 PR 计划、实现骨架、复核纪要、专题实施清单等。

这些文档不是无效文档，但在阅读时应优先把它们视为：

- 阶段方案
- 实施辅助材料
- 历史上下文

而不是默认视为当前唯一权威规范。
