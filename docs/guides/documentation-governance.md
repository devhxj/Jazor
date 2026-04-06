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

## 生命周期规则

### 1. 什么情况下仍应保留为 active

满足以下任一条件时，可继续保留在 `status/` 或 `plans/`：

- 它仍然是当前工作流的主入口
- 它仍然描述当前真实阶段，而不是旧阶段
- 相关 repo-level bridge 仍直接依赖它

### 2. 什么情况下应标记为 historical

满足以下任一条件时，至少应把状态语义改成 historical：

- 已有更新的同类快照取代它
- 它描述的是某一阶段的旧执行假设
- 现在只保留上下文价值，不再直接指导执行

### 3. 什么情况下迁入 archive

满足以下条件时，优先迁入 `docs/archive/`：

- 内容已被新的总览、状态页或执行桥接吸收
- 它不再是当前阅读路径的一部分
- 保留它的主要原因是历史审计或决策追溯

### 4. Active 文档完成后的最小动作

当一个 active plan 或 current status snapshot 结束生命周期时，至少做以下其中两项：

1. 更新其 `Status`
2. 从 repo-level 主入口移除或降级
3. 在新入口中保留一条 historical link
4. 需要时迁入 `docs/archive/`

### 5. 子系统局部文档的特殊规则

对子系统局部文档：

- 不要求为了 archive 而搬迁成熟局部文档集
- 若文件仍留在原位，也应通过 `Status` 或局部索引说明其 active / historical 性质
