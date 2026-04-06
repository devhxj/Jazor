# Emit / Materialization 执行桥接

> Status: active plan
> Positioning: Repository-level execution bridge for emit and host-facing materialization work.

## 目标

把 emit lane 从“只有薄模块入口与相邻专题可见”提升为 repo-level 与 module-local 都可读的执行入口。

## 当前职责块

### 1. Catalog reading

当前重点：

- 从 compiler-owned catalog 读取普通模块与 RazorVue carriers
- 保持 collector/read path 与上游 carrier shape 对齐

对应实现面：

- `ModuleCollector`
- `CatalogReader`
- `RazorVueCatalogReader`

入口：

- [Emit / Host Materialization 状态（2026-04-06）](../status/2026-04-06-emit-host-materialization-status.md)
- [Jazor.Emit README](../../src/Jazor.Emit/README.md)
- [Jazor.Emit Docs](../../src/Jazor.Emit/doc/README.md)
- [Jazor.EmitTest README](../../src/Jazor.EmitTest/README.md)

### 2. Manifest persistence

当前重点：

- 保持 manifest shape 稳定
- 维持普通模块 manifest 与 RazorVue manifest 的并行演进

对应实现面：

- `ManifestModel`
- `RazorVueManifestModel`

### 3. Writer / materialization

当前重点：

- 保持 output writer 的职责清晰
- 维持普通模块与 RazorVue 模块的并行物化路径

对应实现面：

- `ModuleWriter`
- `RazorVueModuleWriter`

### 4. Bundler / host-facing output assembly

当前重点：

- 维持 bundle path 稳定
- 不让 bundler 职责回流进 compiler 语义层

对应实现面：

- `ModuleBundler`

### 5. RazorVue and SourceMap handoff continuation

当前重点：

- 承接 RazorVue catalog / manifest / materialization
- 为 emitted module `.map`、writer 扩展和 bundle chaining 预留清晰承接点

入口：

- [RazorVue 阶段评估（2026-04-06）](../status/2026-04-06-razorvue-stage-assessment.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](../superpowers/plans/2026-04-06-razorvue-v1-authoring-pr-breakdown.md)
- [SourceMap.Overview.md](../../src/Jazor.Compiler/doc/SourceMap.Overview.md)
- [2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md](../superpowers/plans/2026-04-06-razorvue-sourcemap-bundle-chaining-implementation.md)

## 建议阅读顺序

1. [Emit / Host Materialization 状态（2026-04-06）](../status/2026-04-06-emit-host-materialization-status.md)
2. 本文档
3. [Modules Bridge](../architecture/modules/README.md)
4. [Jazor.Emit README](../../src/Jazor.Emit/README.md)
5. [Jazor.Emit Docs](../../src/Jazor.Emit/doc/README.md)
6. [Emit.Materialization.Overview.md](../../src/Jazor.Emit/doc/Emit.Materialization.Overview.md)
7. [Emit.BundleAndSourceMap.Overview.md](../../src/Jazor.Emit/doc/Emit.BundleAndSourceMap.Overview.md)
8. [Jazor.EmitTest README](../../src/Jazor.EmitTest/README.md)
9. 再按具体职责块进入相关上游专题或执行计划

## 当前非目标

- 不把 emit lane 描述成一个独立于 compiler 的完整平台专题
- 不在 repo-level bridge 中重复 `src/Jazor.Emit` 实现细节

## 下一步维护要求

如果 emit lane 的职责进一步扩大，至少同步更新：

1. [Emit / Host Materialization 状态（2026-04-06）](../status/2026-04-06-emit-host-materialization-status.md)
2. [项目执行导航](./project-execution-index.md)
