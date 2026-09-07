# RazorVue P2 执行计划

> P2 只推进能够由强类型 C# 契约、现有 Razor Source Generator 和最终 Vue 产物共同证明的能力。没有完整证据的条目保持 Guidance/Reject，不提前扩张 Blazor 兼容边界。

## 目标与范围

P2 的目标是完善 P1 之后的协议、渲染和生态能力，同时保持 RazorVue 作为支持 Razor/Blazor 语法的 JSX-like 范式，而不是完整 Blazor Server/WebAssembly 实现。

本阶段包含四条工作线：

| 工作线 | 责任层 | 当前决策 |
| --- | --- | --- |
| P2-A SSR 状态与表单交接 | `Jazor.AspNetCore`、`Jazor.Emit`、应用 endpoint | 先完善显式 bootstrap/错误边界；`PersistentComponentState`、enhanced form 保持 Guidance/Reject |
| P2-B 高级渲染评估 | `Jazor.RazorVue`、对应 binding | Microsoft Blazor 内置 UI 组件统一 Reject；`StreamRendering` 统一 Reject；localization、复杂验证另行评估 |
| P2-C JS 互操作边界 | `Jazor.Compiler`、`Jazor.Analyzer`、ECMAScript/WebIDL binding | `IJSRuntime` 家族继续 Reject；完善稳定诊断和 typed 替代路径，不引入字符串 fallback |
| P2-D 性能与交付质量 | `Jazor.Compiler`、`Jazor.Emit`、测试脚本 | 以固定 benchmark 为依据，只实施不改变语义、导入稳定性和 source map 的优化 |

Element Plus typed binding 已完成独立切片，后续 Vuetify、TDesign 和其它组件继续沿用同一证据门槛。

## Definition of Done

P2 的每个可交付切片都必须满足：

1. C# public surface 保持强类型，并能通过官方 Razor SG；不使用 `object?`、裸字符串或私有 wrapper 协议扩大边界。
2. compiler/lowering、runtime/binding 和失败诊断各有针对性回归；不支持的相邻形状在使用点稳定失败。
3. 有 Deno/runtime 证据和真实浏览器交互证据；涉及发布时必须增加隔离 Release package consumer。
4. 涉及 SSR 或 hydration 时，证明 envelope 所有权、错误传播、重复执行和资源闭包；不把该协议描述成 Blazor `PersistentComponentState`。
5. 作者指南、范式、当前状态、capability ledger、诊断矩阵和 CHANGELOG 同步。
6. 适用的 Compiler、CLR、Razor SG、Emit、coverage、SPA、SSR、HMR 门禁通过，并记录提交、SDK、Node、浏览器和生成物路径。

## 工作顺序

### P2-A：SSR 状态与表单交接

- 固定 `jazor-ssr-state` v1 的 props/providers/authentication 字段和错误分类。
- 增加过期 payload、重复 hydration、render-hook 异常和 endpoint 错误的 consumer 回归。
- 为表单场景提供显式 typed endpoint/bootstrap 示例；不得模拟 `PersistentComponentState`、`SupplyParameterFromForm`、antiforgery 或 enhanced post。

完成后只能把“显式 bootstrap DTO + envelope”标为 Support with constraints；内置表单协议仍为 Guidance/Reject。

### P2-B：高级渲染评估

Microsoft Blazor 内置 UI 组件统一不进入 RazorVue 组件契约：

- `CacheView`、`ConfigureBrowser`、`ImportMap`、`ResourcePreloader`、`AntiforgeryToken`、`FormMappingScope`、`DisplayName<T>`、`InputHidden`、`Label<T>`、`EnvironmentView`、`Virtualize`、`QuickGrid`、`SectionContent`、`SectionOutlet` 与其它内置 UI 组件由 `JAZORVGA021` Reject。
- 使用应用自有组件或已声明的 typed Vue component contract 替代。

以下非组件语义再分别建立最小 feasibility fixture：

- localization、复杂 validation：确认 SSR/浏览器语义和错误边界。

没有真实浏览器和 Release consumer 证据时，只更新 ledger 与指导文档，不添加公共 adapter。

### P2-C：JS 互操作边界

- 保持 `IJSRuntime`、`IJSObjectReference`、`JSInvokable` 的稳定 Reject。
- 确保诊断包含源位置、使用点和 typed ECMAScript/WebIDL 替代建议。
- 对常见误用补充 compiler/analyzer 回归，但不添加运行时猜测或原始 JS 注入。

### P2-D：性能与交付质量

- 使用 `benchmark-razorvue-g2.cs` 和 `benchmark-razorvue-build.cs` 的固定参数记录 clean/incremental/HMR/Release、render/update 和 gzip 基线。
- 优化前后比较相同输入、样本数和 SDK；若不能证明收益或破坏 source map/导入稳定性，则不改主链路。
- 对资源闭包、manifest hash、source map 和诊断耗时增加可重复检查。

## 当前状态

### 剩余工作验收清单

本轮 goal 以以下可执行验收为完成条件；`Reject` 是已确定的产品边界，不计为待实现功能。此前“P2 全部完成”的表述不能替代这些证据。

| 工作 | 本轮验收 | 状态 |
| --- | --- | --- |
| SSR 交接正确性 | 真实浏览器执行生成的 bootstrap，验证无效 envelope、并发/重复 hydration、失败传播；SSR render-hook 和 endpoint 错误保持显式失败 | 已完成：23 项 hosting + 4 个真实浏览器场景 |
| 显式表单与状态新鲜度 | 应用自有 typed DTO/endpoint 示例覆盖失败保留草稿和过期数据刷新；解释 snapshot 与业务版本的所有权 | Guidance：继续由应用 endpoint 管理；不新增通用 TTL/replay 认证协议 |
| 绑定与原始注释 | Element Plus/Vuetify/TDesign 生成检查、覆盖门禁及表单/表格/输入等现有高频契约审查；仅对实证缺口补生成源 | 已完成当前快照审查：三套 contract gate 通过 |
| 应用和交付 | Authoring 与 JazorAdmin 现有中型页面、隔离 Release package、SPA/SSR/PathBase/HMR 浏览器门禁 | 已有门禁通过；后续按发布矩阵持续回归 |
| 调试诊断 | source → SG → module → map 工具及失败定位回归，更新可复现使用指南 | 已完成现有链路与失败定位工具 |
| 性能 | 固定参数记录构建全流程与 runtime 数据，按已约定阈值验证优化候选；无稳定收益的候选明确否决 | 已完成基线；本轮无证据支持修改主链路，明确保留现状 |
| 收口 | 同步指南/范式/ledger/状态/CHANGELOG，适用测试通过并推送 main | 已完成本轮改动；推送以网络可用性为准 |

| 条目 | 状态 |
| --- | --- |
| Element Plus typed binding | 已完成独立切片 |
| SSR 显式 envelope/bootstrap | P1 基础已完成；P2 已增加 provider key 唯一性、空白 key 及认证保留 key 冲突校验，表单协议仍为 Guidance |
| Microsoft Blazor 内置 UI 组件 | Reject（`JAZORVGA021`），包括 `CacheView`、`ConfigureBrowser`、`ImportMap`、`ResourcePreloader`、`AntiforgeryToken`、`FormMappingScope`、`DisplayName<T>`、`InputHidden`、`Label<T>`、`EnvironmentView`、`Virtualize`、`QuickGrid`、`SectionContent/SectionOutlet` |
| StreamRendering | Reject（`JAZORVCA012`）；其 renderer-owned streaming SSR 语义不属于当前 contract |
| localization | Guidance：使用应用自有 typed resource/locale contract 或 ECMAScript Intl；request-culture middleware、IStringLocalizer 注入和 SSR locale handoff 尚未形成 RazorVue primitive |
| 复杂 validation | Guidance：使用组件库 typed rules/callbacks 或应用自有 validation contract；EditContext/InputBase/完整服务器验证 parity 不进入范式 |
| IJSRuntime 等 JS 互操作 | Reject；作者面回归确认注入本身不误报，实际成员使用仍由 usage-site/compiler 边界裁决 |
| 性能与交付优化 | 已完成 `benchmark-razorvue-g2.cs --measure-runtime --samples 3 --iterations 3` 基线；当前固定参数复测仍未证明需要主链路优化，因此保留现状 |

最近一次已记录运行时基线（Node `v24.14.1`）为：`counter` generated render/update `476190.48/2500000`、gzip `118`；`keyed-list-100` generated render/update `136363.64/188679.25`、gzip `99`。该数据只用于同参数比较，不构成性能承诺；本轮未实施未经证明的优化。

SSR 状态输入边界由 `Jazor.EmitTest.JazorSsrHostingTests` 锁定：空 provider key、空白 provider key、重复 provider key 和认证保留 key 冲突均在启动 Deno worker 前失败；失败不会留下 runner 产物。

## 证据入口

- `dotnet run --file scripts/csharp/test-dotnet.cs`
- `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs`
- `dotnet run --file scripts/csharp/verify-vue-binding-contracts.cs`
- `dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- --measure-runtime --samples 3 --iterations 3`
- 适用的 `verify-windows-spa-release.cs`、`verify-windows-ssr-release.cs` 和 development HMR 门禁
