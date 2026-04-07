# 文档治理规则

## 目标

Jazor 的文档组织优先服务维护者：要能快点判断当前状态、找到长期规范，还要分得清历史材料和当前有效参考，莫把几层东西整混了。

## 放置规则

- `docs/workstream-dashboard.md`：唯一的工作流总览，当前状态和下一步行动的入口
- `docs/guides/`：维护规则、阅读路径、治理原则
- `docs/architecture/`：长期有效的架构、规范、专题入口
- `docs/status/`：每个工作流的详细状态快照
- `docs/archive/`：历史报告、旧计划、一次性 review、被合并后的原文
- `docs/generated/`：生成产物说明
- `docs/external/`：第三方依赖、缓存、工作副本文档说明
- `docs/superpowers/plans/`：执行级的详细计划文档

## 状态标签

推荐在新文档里使用下面这些状态语义：
- `Status: active reference`
- `Status: current status snapshot`
- `Status: active plan`
- `Status: historical artifact`
- `Status: archived after merge`
- `Status: generated artifact`
- `Status: external dependency document`

## 排除规则

以下路径默认不进主文档体系：
- `.dotnet/.nuget/packages/**/*.md`
- `src/**/node_modules/**/*.md`
- `.tmp/**/*.md`
- `.claude/worktrees/**/*.md`

## 编辑规则

1. 新增长期设计，优先放进 `docs/architecture/` 或已有专题索引
2. 新增当前评审，放进 `docs/status/`
3. 新增执行级行动方案，放进 `docs/superpowers/plans/`
4. 计划或报告结束后，迁进 `docs/archive/`
5. 子系统如果已经有强局部索引，优先保留原位，再在仓库级索引桥接，莫盲目搬家
6. 工作流状态和下一步行动的变更，优先更新 `docs/workstream-dashboard.md`

## 生命周期规则

### 1. 什么情况下仍应保留为 active

满足以下任一条件时，可以继续留在 `status/` 或 `superpowers/plans/`：

- 它仍然是当前工作流的主入口
- 它仍然描述的是当前真实阶段，不是旧阶段
- 相关 repo-level bridge 仍然直接依赖它

### 2. 什么情况下应标记为 historical

满足以下任一条件时，至少应该把状态语义改成 historical：

- 已经有更新的同类快照把它替代了
- 它描述的是某一阶段的旧执行假设
- 现在只剩上下文价值，不再直接指导执行

### 3. 什么情况下迁入 archive

满足以下条件时，优先迁进 `docs/archive/`：

- 内容已经被新的总览、状态页或执行桥接吸收
- 它已经不在当前阅读路径里头了
- 留它的主要原因是历史审计或决策追溯

### 4. Active 文档完成后的最小动作

当一个 active plan 或 current status snapshot 结束生命周期时，至少做下面两项中的任意两项：

1. 更新它的 `Status`
2. 从 `docs/workstream-dashboard.md` 移除或降级
3. 在新入口里保留一条 historical link
4. 需要时迁进 `docs/archive/`

### 5. 子系统局部文档的特殊规则

对子系统局部文档：

- 不要求为了 archive 而搬迁成熟局部文档集
- 如果文件仍留在原位，也要通过 `Status` 或局部索引说明它的 active / historical 性质
