# Emit / Host Materialization 状态（2026-07-23）

> Status: 当前状态快照
> Positioning: 仓库级状态快照，涵盖 emit、manifest、bundle 和 runtime asset 物化工作

## 总结

`Jazor.Emit` 现在是构建后的 generic module materialization 层。它读取 compiler-produced ECMAScript module catalog、`Jazor.Generated.VueRenderCatalog`、source-map carrier、CLR runtime catalog 与 RazorVue embedded runtime assets，写出 `.mjs`、可选 `.mjs.map` 和 canonical schema-v1 `jazor-manifest.json`，并在 bundle 模式下通过 Deno-backed path 组装浏览器入口。

RazorVue catalog、SFC bridge、consumer-entry、host sidecar、update-plan 和 Jolt handoff 合同已随 Razor SG G0 后清理退役，不再属于当前 emit CLI 或 manifest model。

## 当前状态判断

### 1. Emit 是 active dependency lane

当前 emit 线承接的不是单一功能，而是一组跨工作流职责：

- assembly loading
- compiler module catalog collection
- deterministic `.mjs` / `.mjs.map` writing
- generic manifest persistence and cleanup
- runtime asset copy
- Deno-backed bundle workspace assembly

### 2. 当前 repo-level 入口已经补齐第一层

当前仓库级导航里，emit 已经可以通过以下入口直接进入：

- `docs/03-完成/emit/status.md`（本文件）
- `src/Jazor.Emit/README.md`
- `docs/01-目标/compiler/emit/Emit.Pipeline.Overview.md`
- `src/Jazor.EmitTest/README.md`

这些入口必须保持一致：emit 拥有文件物化与 bundling，不拥有 compiler lowering、Razor SG hook、RazorVue render-context lowering 或 toolchain dev-server/HMR 协议。

### 3. 当前收口重点

当前最重要的是保持旧 RazorVue/Jolt manifest 形状退役后的收敛：

- `ManifestModel` 只描述 generic module output；新写 manifest 使用 `schemaVersion`、`runtimeProtocolVersion`、`rootAssemblyName`、`entries`、module `path/contentHash` 字段，不写入 wall-clock 或机器绝对 root path。
- `ModuleWriter` 负责 module、map、canonical manifest 和 stale cleanup；Vue render component 从下一次 manifest 移除时，旧 `.mjs` 与 `.mjs.map` 的删除已有独立 writer 覆盖，并已补外部 Razor SG package consumer 的二次 build 删除回归；同一 Vue render catalog 重复物化时，component `.mjs`、`.mjs.map`、runtime assets 与 manifest 的 byte-for-byte 稳定已有 writer 覆盖；Counter 外部 Razor SG consumer 的连续 clean build 产物相对路径 + SHA256 清单一致性已有 SDK integration 覆盖，且第二轮前会显式删除 `wwwroot/jazor`，避免依赖旧产物残留。
- `CatalogReader` 负责将 RazorVue embedded render-context runtime 资源映射为 `@jazor/vue-runtime/*.mjs` 模块。
- `ModuleBundler` 只消费 manifest 中声明的 module graph。
- 未知、重复、逃逸或 hash 不一致的输入必须 fail-fast，不能猜测旧 RazorVue catalog shape。

## 下一步行动

### 1. 显式化 materialization / source-map 承接职责

**目标**：让 emit 在整体架构中的职责更清晰

**具体行动**：
- 明确 module catalog reading 和 manifest persistence 的边界。
- 保持模块 README、目标文档和测试 README 同步。
- 让 writer / bundler 的演进方向和上游 compiler output 对齐。

**参考文档**：
- [Jazor.Emit README](../../../src/Jazor.Emit/README.md)
- [Emit.Materialization.Overview.md](../../01-目标/compiler/emit/Emit.Materialization.Overview.md)

### 2. 维持 emit test 和真实输出链路的一致性

**目标**：避免测试和真实执行继续分裂

**具体行动**：
- 确保 emit test 覆盖真实的 materialisation 场景
- 让测试链路和构建产物路径保持一致

**参考文档**：
- [Jazor.EmitTest README](../../../src/Jazor.EmitTest/README.md)

### 3. 支撑 RazorVue 和 SourceMap 的活跃需求

**目标**：确保 emit 能够稳定承接上游工作流

**具体行动**：
- 在已接入 RazorVue generated carrier 的基础上，`.mjs.map` 的表达式级首切片已经串起 wrapper map、compiler origin map 与 Razor SG source mappings；Vue render component stale cleanup 已覆盖 writer 与真实 package consumer 增量删除路径；materialization repeat-write determinism 已覆盖 writer 层；Counter 外部 consumer clean-build artifact hash determinism 已覆盖 SDK integration 层；Counter official SG generated artifact size/hash 已进入 benchmark partial report；后续重点转向多 fixture determinism、diagnostic 边界和完整 performance evidence。
- 保留真实 browser Counter smoke 作为 regression gate；继续跟进 bundle source-map chaining 和 runtime dependency 裁剪。

**参考文档**：
- [Emit.BundleAndSourceMap.Overview.md](../../01-目标/compiler/emit/Emit.BundleAndSourceMap.Overview.md)

## 深度文档

- [Jazor.Emit README](../../../src/Jazor.Emit/README.md)
- [Emit.Pipeline.Overview.md](../../01-目标/compiler/emit/Emit.Pipeline.Overview.md)
- [Emit.Materialization.Overview.md](../../01-目标/compiler/emit/Emit.Materialization.Overview.md)
- [Jazor.EmitTest README](../../../src/Jazor.EmitTest/README.md)

## 当前缺口

- RazorVue generated carrier 到 emit generic manifest 的首个正式切片已完成；`.mjs.map` 已接上 wrapper/compiler/SG source mappings 的表达式级首切片，stale cleanup 已覆盖独立 writer 与 package consumer 增量删除路径，writer 层 repeat-write determinism、Counter 外部 consumer clean-build artifact hash determinism 与 Counter generated artifact size/hash benchmark partial report 已覆盖，但仍缺 G2 级多 fixture determinism/performance 证据。
- 真实 browser Counter 首切片已通过；后续 browser 工作应作为 regression/hardening，而不是继续扩大临时 G1 harness。
- Runtime assets 当前随 `Jazor.RazorVue` assembly 扫描物化，后续可基于 manifest dependency metadata 精确裁剪。
- 如果 source-map 或 host bundling 明显扩张，需要继续细化 emit 内部文档分层。
