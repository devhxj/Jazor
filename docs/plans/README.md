# Plans

本目录用于记录当前还在生效的行动方案和执行清单。要继续推进哪条工作流，先从这边进，不容易走岔。

## 仓库级执行导航

- [项目执行导航](./project-execution-index.md)
- [Project Program Roadmap](./project-program-roadmap.md)
- [Compiler 主线执行桥接](./compiler-mainline-execution-bridge.md)
- [Emit / Materialization 执行桥接](./emit-materialization-execution-bridge.md)
- [SourceMap 执行桥接](./sourcemap-execution-bridge.md)
- [RazorVue 执行桥接](./razorvue-execution-bridge.md)

用途：

- 识别当前活跃工作流
- 从状态文档跳到执行计划
- 从仓库级入口桥接到子系统文档

## 计划分层

### 1. 仓库级桥接

- `docs/plans/` 只保留仓库级执行导航
- 每条 lane 优先通过 bridge 接入，不在这儿重复展开子系统细节

### 2. 执行级细化计划

当前执行级细化计划主要放在 `docs/superpowers/plans/`：

- [2026-04-05-razorvue-layering-implementation.md](../superpowers/plans/2026-04-05-razorvue-layering-implementation.md)
- [2026-04-05-razorvue-lifecycle-safe-subset-implementation.md](../superpowers/plans/2026-04-05-razorvue-lifecycle-safe-subset-implementation.md)
- [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)
- [2026-04-06-razorvue-v1-authoring-roadmap.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-roadmap.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)

说明：

- `docs/plans/` 负责仓库级导航和收口
- `docs/superpowers/plans/` 负责更细的执行拆分
- 子系统内部如果已经有成熟总览，就通过桥接进去，莫在这边重复抄一遍

## 使用方式

如果你准备继续推进某条工作流，建议这样走：

1. 先读 [docs/status/README.md](../status/README.md)
2. 再读 [项目执行导航](./project-execution-index.md)
3. 再读 [Project Program Roadmap](./project-program-roadmap.md)
4. 最后再进入具体执行计划
