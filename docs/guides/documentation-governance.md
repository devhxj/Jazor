# 文档治理规则

## 目标

Jazor 的文档组织优先服务维护者：快速判断当前状态、找到长期规范、区分历史材料与有效参考。

## 放置规则

- `docs/guides/`：维护规则、阅读路径、治理原则
- `docs/architecture/`：长期有效的架构、规范、专题入口
- `docs/status/`：当前阶段评审、成熟度判断、当前推进重点
- `docs/plans/`：当前仍生效的行动计划
- `docs/archive/`：历史报告、旧计划、一次性 review、被合并后的原文
- `docs/generated/`：生成产物说明
- `docs/external/`：第三方依赖、缓存、工作副本文档说明

## 状态标签

推荐在新文档中使用以下状态语义：
- `Status: active reference`
- `Status: current status snapshot`
- `Status: active plan`
- `Status: historical artifact`
- `Status: archived after merge`
- `Status: generated artifact`
- `Status: external dependency document`

## 排除规则

以下路径默认不进入主文档体系：
- `.dotnet/.nuget/packages/**/*.md`
- `src/**/node_modules/**/*.md`
- `.tmp/**/*.md`
- `.claude/worktrees/**/*.md`

## 编辑规则

1. 新增长期设计，优先进入 `docs/architecture/` 或现有专题索引
2. 新增当前评审，进入 `docs/status/`
3. 新增近期行动方案，进入 `docs/plans/`
4. 计划或报告完成后，迁入 `docs/archive/`
5. 子系统已经有强局部索引时，优先保留原位并在仓库级索引桥接，而不是盲目搬迁
