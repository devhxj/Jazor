# Jazor Component Runtime 工程计划

> Status: Active mainline
> Date: 2026-07-06
> Size rule: 本文保持在 10KB 内，作为工程主计划入口；实现细节按阶段拆到后续规格。

## 1. 目标

Jazor Component Runtime 的目标是让显式标注 `[ECMAScriptModule]` 的标准 Razor Component 源码尽量无缝切换到 Jazor：`.razor` 仍由官方 Razor Source Generator 生成 C# 组件代码，Jazor.Compiler 将这些 opt-in 组件类编译为浏览器 ES module，`@jazor/runtime` 在浏览器中执行 Razor render tree、组件生命周期、事件、参数和 DOM 更新。未标注组件不进入 Jazor Component Runtime 主线。

一句话边界：

```text
Razor SG 负责语法与生成代码；[ECMAScriptModule] 负责入口 opt-in；ASP.NET Core Components 源码定义兼容语义；Jazor 负责 ES module 编译、runtime 移植、DOM 宿主和打包。
```

主线定位：

- Jazor Component Runtime 是当前前端 authoring/runtime 主线。
- RazorVue 降级为 Vue artifact 旁路与历史探索线，不再定义 Jazor 的主要组件执行模型。
- Jolt 降级为开发期宿主、LSP、DevServer、HMR、build/debug 经验旁路；只有 Runtime 合同稳定后才评估重新集成。

## 2. 设计原则

- 不重写 Razor 语法解析器，不绕过官方 Razor SG。
- 只处理显式标注 `[ECMAScriptModule]` 的组件；未标注 Razor component 继续由原宿主处理或保持普通组件身份。
- 不把 Vue/React/Solid 作为核心语义来源；它们只作为调度、响应式、devtools 或 DOM 工程参考。
- 以 ASP.NET Core Components 的固定版本源码作为行为基线，记录 upstream path、commit/tag、license 和本地改造点。
- 编译器支持必须走 WhiteList/Compile/Import 等显式宿主缝，不允许 raw JS 或 object catch-all 伪兼容。
- Runtime 首版可用 TypeScript/JavaScript 手写移植；不要先尝试用 Jazor 自举编译官方 C# runtime。
- 每个阶段必须可运行、可测试、可回滚；不以“看起来能跑”为验收标准。

## 3. 目标架构

```text
.razor
  -> official Razor Source Generator
.razor.g.cs / component partial class
  -> Jazor.Compiler host mapping
component .mjs
  -> @jazor/runtime
Jazor DOM renderer
  -> browser DOM
```

主要模块：

- `src/Jazor.ComponentRuntime` 或等价 npm/runtime 目录：`ComponentBase`、`RenderTreeBuilder`、`RenderTreeFrame`、`Renderer`、`EventCallback`、`ParameterView`、DOM renderer。
- `src/Jazor.Compiler`：识别 `Microsoft.AspNetCore.Components.*` 宿主类型，支持外部宿主基类继承、render-tree 调用 lowering、稳定 runtime import。
- `src/Jazor.Emit`：物化组件 `.mjs`、runtime manifest、sourcemap 和 bundle 入口。
- `src/Jazor.ComponentRuntime.Test` 与 browser 测试：验证 runtime 行为与真实 DOM 交互。

## 4. Upstream 基线

第一项工程任务必须锁定 ASP.NET Core 源码基线，不追 `main`。

每个移植文件头部或伴随清单记录：

```text
Upstream: dotnet/aspnetcore
Path: src/Components/...
CommitOrTag: <locked-sdk-tag>
License: MIT
LocalAdaptation: browser ES module runtime, no server circuit, no WASM host
```

优先参考：

- `Components/src/ComponentBase.cs`
- `Components/src/Rendering/RenderTreeBuilder.cs`
- `Components/src/RenderTree/*`
- `Components/src/ParameterView.cs`
- `Components/src/EventCallback*.cs`
- `Components.Web.JS/src/Rendering/BrowserRenderer.ts`
- `Components.Web.JS/src/Rendering/Events/*`

## 5. 阶段计划

### Phase 0: 基线与兼容矩阵

目标：冻结路线，建立可执行的工程边界。

任务：

- 锁定 ASP.NET Core Components upstream tag。
- 建立 `[ECMAScriptModule]` 组件入口分类规则，区分静态模块、Runtime 组件和非入口组件。
- 建立 Razor feature 兼容矩阵：P0/P1/P2/P3/P4。
- 建立 runtime public surface 清单和 compiler host mapping 表。
- 选定第一批 fixture：Counter、父子组件、ChildContent、列表、表单雏形。

验收：

- 文档列明每个功能属于 supported / planned / out-of-scope。
- 每个 P0 feature 都有对应源码 fixture 和测试入口。
- 无实现代码依赖 `main` 分支不稳定行为。

### Phase 1: Counter 最小闭环

目标：不改 `.razor`，运行官方 Razor SG 生成的 Counter。

任务：

- 实现最小 `ComponentBase`、`RenderHandle`、`RenderTreeBuilder`、`RenderTreeFrame`。
- 支持 `OpenElement`、`CloseElement`、`AddContent`、`AddAttribute`、事件属性。
- Jazor.Compiler 支持组件类继承 `ComponentBase` 并稳定导入 runtime。
- 实现 render queue、`StateHasChanged` 和最小 DOM mount。

验收：

- `[ECMAScriptModule] Counter.razor` 经 Razor SG + Jazor.Compiler 产出浏览器可运行 ES module。
- 点击按钮后计数更新，事件处理只执行一次，DOM 不整页重载。
- 编译器测试覆盖 runtime import、外部宿主基类、builder 调用 lowering。

### Phase 2: 组件参数与内容组合

目标：普通组件组合可用。

任务：

- 支持 `OpenComponent`、`CloseComponent`、`AddComponentParameter`。
- 实现 `[Parameter]`、`SetParametersAsync`、`ParameterView`。
- 实现 `EventCallback`、`EventCallback<T>`、父子组件回调。
- 支持 `RenderFragment`、`RenderFragment<T>`、`ChildContent`。
- 支持生命周期：`OnInitialized{Async}`、`OnParametersSet{Async}`、`OnAfterRender{Async}`。

验收：

- 父组件传参、子组件触发回调、模板组件均可运行。
- async lifecycle 和 async event 完成后触发正确批次渲染。
- 参数缺失、重复、类型不匹配有明确失败或诊断路径。

### Phase 3: RenderTree 与 DOM diff 正式化

目标：从可运行升级为可维护的 render-tree runtime。

任务：

- 实现 sequence、region、markup content、conditional attribute、splat attribute。
- 实现 `@key`、element/component ref capture、组件 disposal。
- 建立 render batch 数据结构，DOM renderer 只应用差异。
- 参考 Blazor `BrowserRenderer.ts` 和事件委托模型，稳定 event handler id 与解绑。

验收：

- `@if`、`@foreach`、列表 reorder、ref、事件保留都有 browser 回归。
- 删除组件会调用 dispose，事件监听不会泄露。
- diff 行为不依赖遍历偶然顺序，输出和 sourcemap 稳定。

### Phase 4: Razor Components 常用生态

目标：小型 Blazor-style app 基本无改造迁移。

任务：

- 实现 `CascadingValue`、`[CascadingParameter]`、级联更新。
- 实现 `EditForm`、`InputBase<T>`、基础 validation 链路。
- 实现 `LayoutComponentBase`、router、`NavigationManager`。
- 实现最小 JS interop：同步/异步调用、错误传播、对象引用释放。

验收：

- 一个含 layout、router、form、validation、嵌套组件的样例应用可运行。
- 表单事件、验证状态、导航状态在浏览器刷新前后行为可解释。
- 不支持的 Blazor Server/WASM 专属能力必须 fail-fast。

### Phase 5: 高兼容与生态验证

目标：进入真实 Razor 生态兼容验证。

任务：

- 支持 `ErrorBoundary`、`DynamicComponent`、`HeadOutlet`。
- 完善 async render batching、异常边界、取消与 disposal。
- 建立 differential fixtures：同一 `.razor` 在官方 Blazor 与 Jazor 下比较关键 DOM、事件和生命周期日志。
- 选择 1-2 个真实 Razor component library 做兼容报告。

验收：

- 兼容报告列出 pass/fail/unsupported，并能定位到 compiler、runtime、DOM 或生态缺口。
- 每个 unsupported 项都有明确原因，不以静默降级通过。
- 生成包可被样例应用直接通过 ES module mount 使用。

## 6. 测试与质量门

- Compiler tests：验证 Razor SG 生成代码、host type mapping、runtime import、diagnostic。
- Runtime unit tests：验证 builder frame、参数、callback、生命周期、batch。
- Browser tests：验证真实点击、DOM diff、ref、disposal、导航和表单。
- Differential tests：阶段 5 起对照官方 Blazor 的 DOM 与日志。
- 每个阶段完成前运行相关 focused suite；跨阶段里程碑运行 full solution build。

最低质量门：

- 无静默 fallback；unsupported 必须有清晰错误。
- 无全局共享测试状态；browser 测试使用独立容器和端口。
- runtime import、handler id、temp name、batch 顺序保持稳定。
- 移植源码保留 MIT 许可与 upstream 追踪信息。

## 7. 风险与应对

| 风险 | 影响 | 应对 |
| --- | --- | --- |
| 官方源码细节随 SDK 变化 | 高 | 锁定 tag，建立 upstream diff 更新流程 |
| Runtime 与 compiler 互相牵制 | 高 | 先 JS runtime，compiler 只接显式 host seam |
| Blazor 生态期望过大 | 高 | 用兼容矩阵分级，不支持项 fail-fast |
| DOM diff 行为难以复刻 | 中 | 参考 Blazor Web.JS renderer，先 batch 再优化 |
| async/lifecycle 边界复杂 | 中 | 生命周期日志 fixture + differential tests |
| 第三方库依赖 DI/reflection | 中 | 先提供最小 DI/interop 策略，超界明确拒绝 |

## 8. 完成定义

项目完成不是“全部 Blazor 都可跑”，而是达到以下条件：

- 标注 `[ECMAScriptModule]` 的标准 Razor SG 组件无需改语法即可进入 Jazor 编译链。
- P0-P4 兼容矩阵项有实现、测试和文档状态。
- 一个真实小型 Razor app 可通过 Jazor ES module runtime 运行。
- 高级或专属能力有兼容报告和明确边界。
- 后续升级 ASP.NET Core Components 基线时，有可重复的 diff、测试和迁移流程。
