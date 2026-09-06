# RazorVue 开发范式

> RazorVue 是一种使用 Razor 标记和 C# 表达式编写 Vue 组件的开发范式。它借用 Blazor 的作者语法和组件形状，但不实现 Blazor Server、Blazor WebAssembly 或完整 CLR。

## 一句话定义

RazorVue 把 Razor 组件当作一种 JSX-like 的声明式 UI 语言：Razor 标记声明组件树，属性和 `@(...)` 表达式提供值，事件和 `@bind` 提供交互，`@code`/`.razor.cs` 提供可编译的状态与行为；官方 Razor Source Generator 生成 C# 后，RazorVue 将最终 `Compilation` 降低为 Vue render-function 模块。

```text
Razor markup + C# component logic
    -> official Razor Source Generator
    -> final Roslyn Compilation
    -> RazorVue component binding / direct-render lowering
    -> Jazor.Compiler semantic lowering
    -> Vue render-function .mjs
```

Blazor 名称出现在作者代码中，只表示采用了熟悉的 Razor/C# 组件表达方式，不表示对应的 Blazor runtime、服务容器、内置 UI 组件或服务器 circuit 会被搬到浏览器。

## 范式的核心规则

### 1. 组件树优先

组件作者首先描述输出结构，而不是编写 DOM 操作。元素、组件、属性、子内容、条件和循环构成 VNode 树；`RenderTreeBuilder` 是 Razor SG 的中间表现和受控的手写入口，不是另一套公共作者协议。

组件必须同时具备 `ComponentBase` 身份、`IVueComponent`（或派生接口）契约，以及明确的 ECMAScript 模块或组件导入描述。导入描述不能单独把任意 .NET 类型变成组件。

### 2. Razor 标记与组件逻辑分域

- Razor 标记、组件标签、属性、children/slot 和产出 VNode 的 `BuildRenderTree` 属于 **direct render**。这里遵守 frame、metadata、fragment 和循环边界。
- `@code`/`.razor.cs` 的字段、属性、helper、事件处理器和生命周期方法属于 **component logic**。这里可以使用 compiler 已支持的局部变量、条件、循环、返回和方法调用。

同一段 C# 语法放在两个域中，支持结果可能不同。direct render 中的 `break`/`continue` 必须绑定到当前可证明的循环且不能跨越未关闭 frame；普通 helper 中的循环不受这条 RenderTree 协议限制。无法表达的 `goto`、跨 frame 跳转和动态 fragment 入口应明确失败。

### 3. C# 是作者契约，Vue 是运行时

参数、事件、slot、服务和第三方组件优先通过强类型 C# 契约表达。`object`、字符串拼接、反射和手写 JavaScript 不能作为未知运行时语义的逃生通道。

组件状态会映射到 Vue 的响应式模型，参数映射到 Vue props，children/`RenderFragment` 映射到 slot。该映射保持使用点可观察行为，但不承诺保留 CLR 对象身份、线程模型或完整引用语义。

### 4. 只承诺已经证明的语义

每项能力都必须同时通过以下适用证据：Razor SG 绑定、compiler/lowering、模块运行时、以及真实浏览器或 package consumer。涉及 SSR/hydration 时，还必须证明对应 profile 的资源闭包、状态所有权和失败传播。

支持矩阵使用四种决策：

- **Support**：作者可以按正常 Razor/C# 方式使用，且已有完整证据。
- **Support with constraints**：核心形状成立，但有明确的类型、位置、生命周期或运行时限制。
- **Guidance**：语法可能可见，但应改用范式内的强类型替代写法；诊断必须说明替代路径。
- **Reject**：无法在当前协议中保持语义，必须在作者源码或 final Compilation 使用点明确失败。

### 5. 边界属于产品能力

未映射的 .NET 类型、成员和运行时身份不自动可用；不支持的 direct-render 形状不静默生成近似 JavaScript；错误必须回到 `.razor`/`.razor.cs` 的源位置，并说明所属边界。生成失败时不得留下部分模块、catalog 或 bundle。

## 当前范式覆盖

当前已经形成稳定证据的作者形态包括：

- 元素和自定义组件组合、泛型组件、普通 children、`RenderFragment`、typed slot 和 slot forwarding；
- 静态/表达式属性、attribute splat、条件与循环渲染、`@key`、`@ref`；
- DOM/component `@bind`、`EventCallback`、事件 modifiers，以及已声明的核心和扩展 DOM 事件读取；
- `[Parameter]`、参数更新生命周期、`SetParametersAsync(ParameterView)` 的兼容子集、异步生命周期和异步事件；
- `[Inject]`/`@inject` 的 browser-capable typed service、typed/named cascading values；
- `@page`、`@layout`、路由参数、query 参数、应用自有 route host、push/replace history 和 `LocationChanged` 的已验证子集；
- TDesign、Vuetify、Element Plus 等已声明的 typed Vue component contracts；
- Debug 模块、Release bundle，以及已声明范围内的 SSR/hydration 交付。

这些能力的具体限制以[作者指南](../03-guides/razorvue-authoring.md)和[当前状态](../04-roadmap/current-status.md)为准；本页只定义共同范式，不复制逐项测试矩阵。

## 明确不属于范式的内容

以下项目不是“尚未自动兼容”的隐性欠账，而是当前范式明确排除的运行时模型：

- 完整 CLR、任意外部 .NET API、反射和依赖 CLR identity 的对象模型；
- `IJSRuntime` 字符串互操作、动态 JavaScript import 和未经 typed binding 描述的 JS 对象；
- DbContext、HttpContext、Identity manager、circuit、protected storage 等 server-only 服务；
- Microsoft Blazor 内置 UI 组件作为隐式 Vue 组件入口，例如 `Router`、`RouteView`、`EditForm`、`Input*`、`AuthorizeView` 和 `DynamicComponent`；
- 未定义版本化协议的认证状态、`PersistentComponentState`、`[PersistentState]`、enhanced form handoff；
- 完整 SSR/prerender route identity 和未经证明的 hydration side-effect parity；history 只支持已验证的 `popstate`/`hashchange` handler、取消恢复、竞态和 dispose 子集。

遇到这些形状时，作者应使用 typed endpoint、已声明的 Vue component contract、显式 route host 或强类型 ECMAScript binding。范式不通过弱化类型或增加隐式 fallback 来扩大边界。

## 作者决策顺序

编写组件时按以下顺序判断：

1. 这段代码是在声明 VNode，还是在执行 component logic？
2. 输入、参数、slot、事件和返回值能否用明确的 C# 类型表达？
3. 组件或服务是否有正式的导入/映射契约？
4. 该行为需要 CLR runtime identity，还是可以安全擦除为 Vue/JavaScript 值？
5. 是否会改变求值顺序、副作用次数、响应式更新或生命周期顺序？
6. 失败时能否给出源位置、稳定诊断 ID 和范式内替代写法？

如果最后一个问题无法回答，就不应把该形状标记为 Support。

## P0/P1 完成状态与后续完善工作

后续工作关注范式的自然度和证据闭环，而不是追求 Blazor API 数量：

| 优先级 | 工作 | 完成标准 | 状态 |
| --- | --- | --- | --- |
| P0 | 收敛组件库 authoring contract | TDesign/Vuetify/Element Plus 的参数、事件、union、slot 和 splat 命名保持一致；真实页面不需要应用侧转换或手写 builder。 | 已完成并由组件 binding/authoring 测试与 Release consumer 覆盖 |
| P0 | 提升失败诊断和修改反馈 | 每个 Reject/Guidance 都有稳定 ID、原始源位置、原因和最小替代写法；源码项目与独立 package consumer 行为一致。 | 已完成；诊断排序、源位置和失败传播有 SG 回归 |
| P0 | 固化真实开发闭环 | Debug、HMR、Release、PathBase、浏览器交互、SSR/hydration 的资源闭包和错误传播可重复验证。 | 已完成；命令与实跑结果见[验收证据入口](#p0p1-验收证据入口) |
| P1 | 完善响应式与生命周期语义 | 继续验证参数替换、slot 捕获、`@key` identity、异步事件、异步 lifecycle、卸载竞态和 SSR side effect；明确哪些是 Vue 语义而非 CLR parity。 | 当前声明子集已完成；完整 CLR reference parity 和复杂 SSR side effect 仍是边界 |
| P1 | 提供范式级调试工具 | 让作者能从 `.razor` 位置追踪到 generated C#、lowered module、source map 和最终组件边界，不要求阅读内部 AST。 | 已完成；使用 `inspect-razorvue-chain.cs` |
| P1 | 建立中型应用体验基线 | 用多个组件、多层 slot、表单、路由和状态组合的真实页面测量首次构建、增量构建、HMR、产物体积和诊断耗时，再决定优化。 | 基线已完成；后续优化需保持同一 benchmark 参数 |
| P2 | 扩展 typed 生态绑定 | **已完成本轮 Element Plus 切片**：`ElButton`/`ElInput` 覆盖枚举 prop、事件、`@bind-ModelValue`、default/prefix slot、class/style 与 attribute splat，并通过官方 SG、Deno 模块运行时、Release package consumer 和真实浏览器证据。后续组件仍按同一门槛逐切片加入。 |
| P2 | 评估有限的协议扩展 | **本轮无需扩展协议**：现有 `ECMAScript` metadata、Razor SG 参数绑定、slot descriptor 与 Vue module pipeline 足以表达 Element Plus 切片；保持协议不变，避免引入 wrapper-JS marker 或弱类型 fallback。 |

任何后续能力只有在实现、测试、作者指南和当前状态同步后，才能从 Guidance/Reject 升级为 Support。

## 相关文档

- [Razor-to-Vue 架构](./razor-to-vue.md)
- [RazorVue 作者指南](../03-guides/razorvue-authoring.md)
- [当前状态](../04-roadmap/current-status.md)
- [下一阶段](../04-roadmap/next-development.md)

## P0/P1 验收证据入口

以下命令是范式交付的可复现证据入口；命令成功后，结果应连同提交版本、SDK 与浏览器版本保存到 CI artifact 或发布记录中。

| 交付场景 | 验收命令 | 必须观察的结果 |
| --- | --- | --- |
| Debug authoring smoke | `dotnet run --file samples/RazorVue.Authoring/verify-smoke.cs` | 官方 Razor SG 编译通过；生成 `.mjs`、`.mjs.map`、manifest；浏览器路由、表单、slot 与状态交互通过。 |
| Development HMR | `dotnet run --file scripts/csharp/verify-development-hmr.cs` | 同一页面在文件变更后收到 HMR 更新；浏览器状态与错误 overlay 行为符合脚本断言。 |
| Windows SPA Release | `dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs` | 本地 package consumer 使用 Release bundle；`/docs` 下资源、路由和刷新均成功。 |
| Windows SSR Release | `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo` | SSR HTML、部署资源解析和已声明 hydration 子集通过；资源闭包缺失时发布失败且无 partial artifact。 |
| RazorVue quality gate | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` | 测试数量、行覆盖率和分支覆盖率达到 `current-status.md` 门槛。 |
| Render performance baseline | `dotnet run --file scripts/csharp/benchmark-razorvue-g2.cs -- --measure-runtime --samples 3 --iterations 3` | 输出 render/update 吞吐与 gzip 体积；优化前后使用相同 Node、样本和迭代参数比较。 |

### 证据记录格式

每次能力升级至少记录：`git rev-parse HEAD`、`dotnet --info`、Node 版本、验收命令、退出码、测试计数、覆盖率和生成物路径。性能记录还要包含样本数、迭代数、冷启动/热更新区分以及 gzip 字节数。没有真实 consumer 或浏览器证据的实现只能标记为 `Support with constraints`，不能升级为 `Support`。

### P2 Element Plus 验收入口

Element Plus 的 typed binding 以生成源 `src/ECMAScript.Vue.Generator/ElementPlusGenerator.cs` 和上游
`2.14.4` metadata 为单一来源。生成器 `elementplus --check` 必须报告 `111 components and 2 directives`
且工作区无生成漂移。官方 SG 回归
`RazorSgOfficialElementPlusNaturalAuthoringRuntimeTests` 验证 `ElButton` 的枚举与 click、`ElInput`
的 `VueStringNumberValue` 双向绑定、named slot、属性 splat 以及最终 `element-plus` import；Emit 的
`Build_LocalReleasePackages_WithExternalNativeElementPlusRazorConsumer_MaterializesAssetsInRealBrowser`
验证隔离 package consumer、Release bundle、CSS/ESM 资源闭包以及真实浏览器可读取发布资源；组件
交互语义由上面的官方 SG + Deno 运行时测试覆盖。该切片没有新增
runtime protocol；如果后续组件需要协议能力，必须先增加明确的失败测试和迁移说明。
