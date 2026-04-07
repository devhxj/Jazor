# Jazor 文档中心

## 阅读顺序
1. [当前状态](./status/README.md) - 先看哈项目现在走到哪一步了
2. [架构与规范](./architecture/README.md) - 再把整体架构和规矩理清楚
3. [维护规则](./guides/README.md) - 晓得文档平时该咋个维护
4. [活动计划](./plans/README.md) - 看接下来沿哪条路子往前推
5. [历史归档](./archive/README.md) - 最后再去翻历史材料

如果你是来恢复项目推进，建议按这条顺序走：

1. [项目阶段评审](./status/2026-04-04-project-stage-assessment.md)
2. [工作流状态面板](./status/2026-04-06-project-workstream-dashboard.md)
3. [项目执行导航](./plans/project-execution-index.md)
4. [Project Program Roadmap](./plans/project-program-roadmap.md)
5. [架构与规范](./architecture/README.md)

## 文档分层
- `guides/`：维护者使用说明与治理规则，主要讲文档该咋个维护
- `architecture/`：长期有效的架构、规范和专题入口，适合当稳定参考
- `status/`：当前阶段判断、成熟度评审和推进重点，先看现状就来这儿
- `plans/`：当前还在生效的行动方案，主要回答下一步咋个走
- `archive/`：历史材料和被合并后的原始文档，查老账就看这坨
- `generated/`：生成产物文档的来源说明，不当主规范用
- `external/`：第三方依赖、缓存和工作副本文档说明，不进主阅读路径

## 当前范围说明
本目录只收纳或索引 Jazor 自己的文档资产。第三方依赖 README、生成产物 markdown、缓存目录 markdown 这些东西，都不算主阅读路径。

## 当前执行入口

- [工作流状态面板](./status/2026-04-06-project-workstream-dashboard.md)
- [项目执行导航](./plans/project-execution-index.md)
- [Project Program Roadmap](./plans/project-program-roadmap.md)

说明：

- `docs/status/` 回答“现在项目在哪个阶段”
- `docs/plans/` 回答“现在应该沿哪条执行线推进”
- `docs/architecture/` 回答“稳定参考和局部文档入口在哪儿”
- `docs/superpowers/plans/` 保留更细的执行级计划文档

## 子系统入口

### 1. Compiler / RazorVue / SourceMap 深度文档

- [Compiler 文档桥接入口](./architecture/compiler/README.md)
- [Jazor.Compiler 文档索引](../src/Jazor.Compiler/doc/README.md)

### 2. 模块级 operational docs

- [Modules Bridge](./architecture/modules/README.md)
- [Jazor.Emit Docs](../src/Jazor.Emit/doc/README.md)

原则：

- repo-level 文档负责桥接和收口
- 子系统局部文档负责深度内容
- 已经成熟的局部文档集，不在顶层重复抄正文

## 历史材料说明
`docs/archive/testing/` 保存 2026-01-27 的历史测试审计材料。顶层 `docs/` 这边不再保留这类一次性报告。
