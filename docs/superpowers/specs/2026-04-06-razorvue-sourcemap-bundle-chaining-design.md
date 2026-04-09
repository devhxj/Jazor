# RazorVue Sourcemap Bundle-Chaining Design

> Status: active reference
> Positioning: Execution-facing design reference for the current narrow SourceMap rollout tied to RazorVue output.

**Date:** 2026-04-06

## Goal

为 RazorVue 产物建立可落地的 sourcemap 闭环：

1. `Jazor.Emit` 输出的单模块 `.mjs` 生成外置 `.map`
2. `.map` 内嵌 `sourcesContent`
3. bundler 阶段保留并串联上游模块 map
4. 最终 bundle 生成可追溯到原始 `.razor/.cs` 的 chained sourcemap

本阶段不实现 HMR runtime，但会保持 sourcemap 所需的 module/source identity 稳定，以便后续 HMR 复用。

## Current State

根据现有代码、测试与文档，当前仓库已经具备以下基础：

- `RazorVueSourceOrigin` 已作为 source-origin carrier 存在
- `RazorVueArtifactFactory` 已汇聚 `snapshot.Origins` 与 lowering 阶段补充的 origins
- `RazorVueGenerator` 会把 `SourceOrigins` 进入 generated catalog
- `RazorVueCatalogReader` 在 emit 阶段可读回 `SourceOrigins`
- `RazorVueManifestModel` 已包含 HMR 相关 identity/hash/boundary 元数据
- `RazorVueModuleWriter` 当前只落 `.mjs`，尚不落 `.map`
- `ModuleBundler` 当前做 bundle，但尚无 sourcemap chaining 闭环

同时，文档现状存在一个需要显式更新的点：

- `src/Jazor.Compiler/doc/SourceMap.Overview.md` 里的旧共识写的是“第一阶段不做 bundle map chaining”
- 本设计明确把 bundler chaining 纳入当前阶段目标，因此后续实现时需要同步修正文档，避免设计与代码方向再次分叉

## Chosen Scope

本设计的边界由当前确认结论固定：

- 首先支持 RazorVue emitted modules
- API 与 builder/writer 结构按“未来可覆盖所有 emit 模块”设计
- sourcemap 采用外置 `.map` 文件
- emitted `.mjs` 与最终 bundle 都追加 `//# sourceMappingURL=`
- 映射目标优先回原始 `.razor/.cs`
- 第一阶段以行级稳定映射为主
- `.map` 包含 `sourcesContent`
- 缺失映射时局部降级，不中断 emit/bundle

## Non-Goals

本阶段明确不做：

- HMR runtime
- WebSocket/dev server/watch mode
- 完整 module graph
- 列级高精度 mapping
- 普通非 RazorVue 模块的完整 sourcemap 接入
- 为了 sourcemap 反向重构 RazorVue 主 lowering 架构

## Architecture Overview

整体链路固定为：

`RazorVue SourceOrigins -> module-level source map build -> emitted .mjs + .map -> bundle final map -> source map chaining/remap -> final bundle + final bundle.map`

按职责拆为四层：

### 1. Source map core

负责通用 sourcemap 内存模型、映射构建、JSON 写出、bundle chaining。

这一层不依赖 RazorVue 语义，只消费“生成文件 + origins/mappings”这类通用输入。

### 2. Emit integration

负责把 RazorVue artifact 上已有的 `SourceOrigins` 变成 emitted module `.map`，并把 `sourceMappingURL` 追加到输出模块末尾。

### 3. Bundle integration

负责在 bundle 阶段保留中间模块 map，并把 final bundle map 与 module maps 串起来，最终把 bundle 位置追溯回原始 `.razor/.cs`。

### 4. Future HMR contract

本阶段不实现热更新运行时，但会保持：

- `RelativeModulePath`
- generated file name
- source file path
- descriptor/Razor-markup/logic identity

这些键在 sourcemap 中的使用方式稳定，后续 HMR 可以直接复用它们做 diff/update payload 关联。

## File Ownership

建议新增/修改的主要文件如下。

### Source map core

新增到 `src/Jazor.Emit/SourceMaps/`：

- `SourceMapDocument.cs`
  - sourcemap 文档内存模型
- `SourceMapBuilder.cs`
  - 把 origin/mapping 输入转成 line-level mappings
- `SourceMapWriter.cs`
  - 输出标准 `.map` JSON
- `SourceMapChainBuilder.cs`
  - final bundle map 与 module maps 做 chaining/remap

### Emit integration

修改：

- `src/Jazor.Emit/RazorVueModuleWriter.cs`
  - 生成 `.mjs.map`
  - 追加 `//# sourceMappingURL=`

可选修改：

- `src/Jazor.Emit/RazorVueManifestModel.cs`
  - 若 host 需要显式发现 map 路径，可补 map 文件路径字段
  - 若阶段一采用同名约定，可先不改 manifest

### Bundle integration

修改：

- `src/Jazor.Emit/ModuleBundler.cs`
  - 收集中间模块 maps
  - 生成 final bundle map
  - 执行 chaining/remap
  - 给最终 bundle 追加 `//# sourceMappingURL=`

### Existing providers kept as-is initially

先直接消费已有结构，不先重做 RazorVue 主链路：

- `src/Jazor.RazorVue/RazorVue/Artifacts/RazorVueSourceOrigin.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- `src/Jazor.Emit/RazorVueCatalogReader.cs`

如果实际实现时发现 origin 粒度不足，再做第二轮定向补强，而不是在第一阶段预先扩 scope。

## Data Model

### 1. Origin layer

继续复用 `RazorVueSourceOrigin` 作为第一层来源数据。它表达：

- 原始源码路径
- 原始源码 span/line/column
- 生成文件路径
- 生成 span
- provenance / mapping quality

在第一阶段，builder 只把它视作“原始源码到 generated module 的候选映射记录”。

### 2. Document layer

新增 `SourceMapDocument`，表示一个输出文件对应的一份 sourcemap 文档。至少应包含：

- generated file name
- `sources`
- `sourcesContent`
- line mappings
- version
- 可选 `names`（第一阶段允许为空）

第一阶段不追求名字级别符号映射，避免为了 `names` 反向推高 builder 复杂度。

### 3. Chain layer

新增 `SourceMapChainBuilder` 使用的中间模型，表达：

- bundle 某段 generated 代码来自哪个 emitted module
- emitted module 的该段代码又来自哪个原始 source

其本质是两段映射的组合：

- `bundle -> module`
- `module -> original source`

最终输出：

- `bundle -> original source`

## Mapping Strategy

### 1. Precision

第一阶段使用行级稳定映射：

- 优先保证浏览器能跳回正确源文件与正确源码行
- 不强求复杂 lowering 的列级精确
- 对一源多目标情形允许多条 line mapping 指回同一原始行

### 2. Source preference

调试目标优先回原始 `.razor/.cs`，而不是 lowering 产生的中间代码。

中间阶段文件即便存在，也不应主导开发者调试体验。

### 3. Degradation

如果某段 generated code 没有可靠 origin：

- 对应模块 map 中该段可不映射
- chaining 遇到 unmapped 段时停止向上追溯
- 最终 `.map` 继续输出
- 不因为局部 unmapped 让整个 emit/bundle 失败

### 4. sourcesContent

所有能读到的原始 `.razor/.cs` 文件都写入 `sourcesContent`。

若源文件读取失败：

- 仍保留 source path
- 该 source 的 `sourcesContent` 允许为空
- 不阻断 `.map` 生成

## Emit Flow

对每个 RazorVue emitted module：

1. `RazorVueCatalogReader` 读回 artifact 与 `SourceOrigins`
2. `RazorVueModuleWriter` 在写 `.mjs` 前后构建 `SourceMapDocument`
3. `SourceMapBuilder` 把 origin 集合转换为 line-level mappings
4. `SourceMapWriter` 写出同名 `.mjs.map`
5. `RazorVueModuleWriter` 在 `.mjs` 尾部追加 `//# sourceMappingURL=<file>.map`

第一阶段的 emitted module sourcemap 是后续 bundle chaining 的上游输入，因此它必须是稳定可序列化的标准 `.map` 文件，而不是只停留在 manifest 元数据里。

## Bundle Flow

`ModuleBundler` 当前会把 emitted modules 放进 bundle workspace，然后生成最终 bundle。新增 sourcemap 后，bundle 阶段流程应扩展为：

1. 收集所有输入模块及其相邻 `.map`
2. bundle 过程中保留 final bundle 与输入模块之间的映射信息
3. 若 bundler 本身能提供 final map，则以其作为上层 map
4. `SourceMapChainBuilder` 把：
   - final bundle map
   - 各输入模块的 emitted module maps
   组合成最终 `bundle -> original source` map
5. 写出最终 `bundle.map`
6. 给最终 bundle 追加 `//# sourceMappingURL=<bundle>.map`

### Bundle failure fallback

如果 bundler chaining 失败：

- 首选保留 bundler 原生 final map（若可用）
- 若 final chaining 做不到，也不应影响 bundle 主产物生成
- 模块级 `.map` 仍然应该保留

这保证即使 final bundle 无法完整 remap，开发者至少还能在 emitted module 层获得有效调试信息。

## Error Handling

本阶段采用“局部降级但不中断主流程”的策略。

### Case 1: source file missing

- `sourcesContent` 为空
- source path 仍写入 `.map`
- 继续输出 `.map`

### Case 2: invalid origin span

- 跳过该条 mapping
- 不让整个 module map 失败

### Case 3: mixed mapped/unmapped generated ranges

- mapped 段正常输出
- unmapped 段留空
- 不强行伪造 synthetic 映射

### Case 4: chain remap failure

- 继续保留 emitted module maps
- final bundle 使用退化 map 或无 chain map
- 不阻断 bundling

## Testing Strategy

测试主要放在 `src/Jazor.EmitTest/`。

### Unit tests

新增：

- `SourceMapBuilderTests.cs`
  - origin -> line mappings
  - 多 source 合并
  - 行级映射边界
  - unmapped/invalid origin 降级

- `SourceMapWriterTests.cs`
  - `.map` JSON 结构
  - `sourcesContent` 输出
  - `sourceMappingURL` 文件名一致性

- `SourceMapChainBuilderTests.cs`
  - `bundle -> module` 与 `module -> source` chaining
  - partial/unmapped 情况
  - source 丢失时的退化行为

### Integration tests

扩展现有测试：

- `RazorVueEmitIntegrationTests.cs`
  - emitted `.mjs.map` 存在
  - `.mjs` 尾部包含 `sourceMappingURL`
  - `.map` 含原始 `.razor/.cs`
  - `.map` 含 `sourcesContent`

- `ModuleBundlerTests.cs`
  - final bundle map 存在
  - final bundle 尾部包含 `sourceMappingURL`
  - chained map 能追溯到原始源码路径

- `SdkIntegrationTests.cs`
  - sample 多项目真实产物中 bundle map 被落盘
  - bundle map 至少能指回 RazorVue 原始 source 文件

## Compatibility and Evolution

为了后续支持普通 emit 模块，本设计约束如下：

1. `SourceMapBuilder` 不绑定 `RazorVueSourceOrigin` 具体类型名，可以通过 adapter/input DTO 接入
2. `SourceMapDocument` 与 `SourceMapWriter` 不应假设输入一定来自 RazorVue
3. `SourceMapChainBuilder` 只关心标准 map 与 generated-file identity，不关心上游是否是 RazorVue

这样后续普通模块若补上 origin provider，可以直接复用当前 sourcemap core，而无需推翻阶段一实现。

## Relationship with Future HMR

本阶段不实现 HMR runtime，但 sourcemap 设计会保留以下对 HMR 有利的约束：

- emitted module path 稳定
- source path 稳定
- generated file identity 稳定
- 与 artifact identity/hash 语义不冲突

后续 HMR 若要做：

- boundary 决策仍来自 `DescriptorHash` / `TemplateHash` / `LogicHash`
- patch/reload payload 可以引用同一 module/source identity
- 调试与热更新不需要各自维护一套 source provenance 体系

## Implementation Notes

实现时需要同步更新文档，至少包括：

- `src/Jazor.Compiler/doc/SourceMap.Overview.md`
  - 移除“第一阶段不做 bundle map chaining”的旧结论
- 相关 SourceMap checklist/decision 文档
  - 统一为当前阶段目标：模块 map + bundle chaining

否则文档会继续落后于实际代码方向，重复制造误导。

## Success Criteria

本设计落地完成时，应满足：

1. RazorVue emitted `.mjs` 都能产出同名 `.map`
2. emitted `.mjs` 都追加 `sourceMappingURL`
3. `.map` 包含原始 `.razor/.cs` 路径与 `sourcesContent`
4. final bundle 产出 chained `bundle.map`
5. final bundle 追加 `sourceMappingURL`
6. 浏览器调试可从 final bundle 稳定跳回原始 `.razor/.cs` 行
7. 局部缺失映射不会导致 emit/bundle 失败
8. sourcemap core API 后续可扩展到非 RazorVue 模块
