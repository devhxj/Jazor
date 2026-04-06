# Jazor 当前阶段评审（2026-04-04）

> Status: current status snapshot
> Positioning: Broad stage baseline for the whole repository before entering workstream-specific snapshots.

## 总结

Jazor 当前处于“核心编译器主线较成熟、外围集成线持续推进、文档体系进入正式治理期”的阶段。

## 四条判断线

### 1. 核心编译器主线
- 状态：接近稳定主干
- 依据：根 README 与 compiler README 都给出较强测试与功能完成信号
- 含义：`Jazor.Compiler` 与其测试体系是当前最成熟的资产

### 2. RazorVue / 新能力线
- 状态：执行推进中
- 依据：当前 team 任务集中在最小 OpenComponent lowering 与定向验证
- 含义：方向明确，但尚不应描述为全面收口

### 3. Team 协作线
- 状态：停留在 `team-exec`
- 依据：2026-04-03 的 team 执行快照显示当前 phase 为 `team-exec`，且 handoff 只记录到 `team-plan -> team-exec`
- 含义：当前没有 verify / complete 的闭环证据

### 4. 文档治理线
- 状态：从零散积累进入系统重组阶段
- 依据：顶层 README、`docs/` 报告、`src/Jazor.Compiler/doc/` 专题规范与既有整理方案并存
- 含义：当前需要的是信息架构治理，不是继续堆叠零散文档

## 当前维护建议
- 先通过 `docs/README.md` 收拢仓库级阅读路径
- 保留 `src/Jazor.Compiler/doc/README.md` 作为 compiler 专题入口
- 把阶段性报告与历史审计迁入 `docs/archive/`
- 显式排除第三方依赖、生成产物、缓存目录 markdown
