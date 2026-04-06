# Jazor 文档中心

## 阅读顺序
1. [当前状态](./status/README.md)
2. [架构与规范](./architecture/README.md)
3. [维护规则](./guides/README.md)
4. [活动计划](./plans/README.md)
5. [历史归档](./archive/README.md)

如果你是在恢复项目推进，建议改用这条顺序：

1. [项目阶段评审](./status/2026-04-04-project-stage-assessment.md)
2. [工作流状态面板](./status/2026-04-06-project-workstream-dashboard.md)
3. [项目执行导航](./plans/project-execution-index.md)
4. [Project Program Roadmap](./plans/project-program-roadmap.md)
5. [架构与规范](./architecture/README.md)

## 文档分层
- `guides/`：维护者使用说明与治理规则
- `architecture/`：长期有效的架构、规范、专题入口
- `status/`：当前阶段判断、成熟度评审、当前推进重点
- `plans/`：当前仍在生效的行动方案
- `archive/`：历史材料与被合并后的原始文档
- `generated/`：生成产物文档说明
- `external/`：第三方依赖、缓存、工作副本文档说明

## 当前范围说明
本目录只收纳或索引 Jazor 自有文档资产。第三方依赖 README、生成产物 markdown、缓存目录 markdown 不进入主阅读路径。

## 当前执行入口

- [工作流状态面板](./status/2026-04-06-project-workstream-dashboard.md)
- [项目执行导航](./plans/project-execution-index.md)
- [Project Program Roadmap](./plans/project-program-roadmap.md)

说明：

- `docs/status/` 回答“现在项目在哪个阶段”
- `docs/plans/` 回答“现在应该沿哪条执行线推进”
- `docs/architecture/` 回答“稳定参考与局部文档入口在哪里”
- `docs/superpowers/plans/` 保留更细的执行级计划文档

## 子系统入口

### 1. Compiler / RazorVue / SourceMap 深度文档

- [Compiler 文档桥接入口](./architecture/compiler/README.md)
- [Jazor.Compiler 文档索引](../src/Jazor.Compiler/doc/README.md)

### 2. 模块级 operational docs

- [Modules Bridge](./architecture/modules/README.md)
- [Jazor.Emit Docs](../src/Jazor.Emit/doc/README.md)

原则：

- repo-level 文档负责桥接
- 子系统局部文档负责深度内容
- 不复制成熟局部文档集正文

## 历史材料说明
`docs/archive/testing/` 保存 2026-01-27 的历史测试审计材料。顶层 `docs/` 不再保留这些一次性报告。
