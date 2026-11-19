# RazorVue 开发范式

> RazorVue 是一种使用 Razor 标记和 C# 表达式编写 Vue 组件的开发范式。它借用 Blazor 的作者语法和组件形状，但不实现 Blazor Server、Blazor WebAssembly 或完整 CLR。

## 一句话定义

RazorVue 视 Razor 组件为一种 JSX-like 的声明式 UI 语言：Razor 标记声明组件树，属性和 `@(...)` 表达式提供值，事件和 `@bind` 提供交互，`@code`/`.razor.cs` 提供可编译的状态与行为；官方 Razor Source Generator 生成 C# 后，RazorVue 降低最终 `Compilation`，产出 Vue render-function 模块。

```text
Razor markup + C# component logic
    -> official Razor Source Generator
    -> final Roslyn Compilation
    -> RazorVue component binding / direct-render lowering
    -> Jazor.Compiler semantic lowering
    -> Vue render-function .mjs
```

Blazor 名称出现在作者代码中，仅表示沿用熟悉的 Razor/C# 组件表达方式；对应的 Blazor runtime、服务容器、内置 UI 组件或服务器 circuit 并不随之进入浏览器。

## 范式的核心规则

### 1. 组件树优先

组件作者首先描述输出结构。元素、组件、属性、子内容、条件和循环构成 VNode 树；`RenderTreeBuilder` 作为 Razor SG 中间表现和受控手写入口服务于同一作者协议。

组件必须同时具备 `ComponentBase` 身份、`IVueComponent`（或派生接口）契约，以及明确的 ECMAScript 模块或组件导入描述。仅有导入描述，不足以让任意 .NET 类型成为组件。

### 2. Razor 标记与组件逻辑分域

- Razor 标记、组件标签、属性、children/slot 和产出 VNode 的 `BuildRenderTree` 属于 **direct render**。这里遵守 frame、metadata、fragment 和循环边界。
- `@code`/`.razor.cs` 的字段、属性、helper、事件处理器和生命周期方法属于 **component logic**。这里可以使用 compiler 已支持的局部变量、条件、循环、返回和方法调用。

同一段 C# 语法位于两个域时，适用的 lowering 规则由所在域决定。direct render 中的 `break`/`continue` 绑定当前可证明循环并位于已关闭 frame 之后；普通 helper 中的循环遵循 component logic 规则。`goto`、跨 frame 跳转和动态 fragment 入口由诊断说明当前范围。

### 3. C# 是作者契约，Vue 是运行时

参数、事件、slot、服务和第三方组件通过强类型 C# 契约表达。运行时语义经已声明的 host mapping、binding 或组件契约进入模块。

组件状态会映射到 Vue 的响应式模型，参数映射到 Vue props，children/`RenderFragment` 映射到 slot。该映射保持使用点可观察行为，但不承诺保留 CLR 对象身份、线程模型或完整引用语义。

### 4. 只承诺已经证明的语义

每项能力都必须同时通过以下适用证据：Razor SG 绑定、compiler/lowering、模块运行时、以及真实浏览器或 package consumer。涉及 SSR/hydration 时，还必须证明对应 profile 的资源闭包、状态所有权和失败传播。

支持矩阵使用四种决策：

- **Support**：作者可以按正常 Razor/C# 方式使用，且已有完整证据。
- **Support with constraints**：核心形状成立，但有明确的类型、位置、生命周期或运行时限制。
- **Guidance**：语法可能可见，但应改用范式内的强类型替代写法；诊断必须说明替代路径。
- **Reject**：当前协议在作者源码或 final Compilation 使用点提供稳定诊断与替代路径。

### 5. 边界属于产品能力

外部 .NET 类型、成员和运行时身份通过显式 mapping 进入模块；direct-render 形状依据已实现协议生成 JavaScript。诊断回到 `.razor`/`.razor.cs` 源位置并说明所属边界；失败路径维持上一份完整模块、catalog 或 bundle。

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

这些能力的具体限制以[作者指南](https://github.com/devhxj/RazorVue/blob/main/docs/03-guides/razorvue-authoring.md)和[当前状态](../04-roadmap/current-status.md)为准；本页只定义共同范式，不复制逐项测试矩阵。

## 明确不属于范式的内容

以下运行时模型由当前范式明确排除；此类边界出自有意的设计取舍，属于产品能力的确定部分：

- 完整 CLR、任意外部 .NET API、反射和依赖 CLR identity 的对象模型；
- `IJSRuntime` 字符串互操作、动态 JavaScript import 和未经 typed binding 描述的 JS 对象；
- DbContext、HttpContext、Identity manager、circuit、protected storage 等 server-only 服务；
- Microsoft Blazor 内置 UI 组件作为隐式 Vue 组件入口，例如 `Router`、`RouteView`、`EditForm`、`Input*`、`AuthorizeView`、`DynamicComponent`、`CacheView`、`ConfigureBrowser`、`ImportMap`、`ResourcePreloader`、`BasePath`、`AntiforgeryToken`、`FormMappingScope`、`DisplayName<T>`、`InputHidden`、`Label<T>`、`EnvironmentView`、`Virtualize`、`QuickGrid`、`SectionContent` 和 `SectionOutlet`；
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

能够完整回答上述问题的形状进入 Support 评审。

## 支持范围与持续验收

组件、生命周期、诊断和交付的当前支持切片统一在[当前状态](../04-roadmap/current-status.md)维护。后续作者体验、协议扩展与性能工作按[下一阶段](../04-roadmap/next-development.md)的责任层和验收条件推进。新增形状只有在实现、测试、指南与真实消费者证据同步后，才从 Guidance/Reject 进入 Support 评审。

## 相关文档

- [Razor-to-Vue 架构](./razor-to-vue.md)
- [RazorVue 作者指南](https://github.com/devhxj/RazorVue/blob/main/docs/03-guides/razorvue-authoring.md)
- [当前状态](../04-roadmap/current-status.md)
- [下一阶段](../04-roadmap/next-development.md)

## 验收证据入口

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

每次能力升级至少记录：`git rev-parse HEAD`、`dotnet --info`、Node 版本、验收命令、退出码、测试计数、覆盖率和生成物路径。性能记录还包含样本数、迭代数、冷启动/热更新区分以及 gzip 字节数。完整 Support 证据由真实 consumer 或浏览器验证组成。

### P2 Element Plus 验收入口

Element Plus 的 typed binding 以生成源 `src/ECMAScript.Vue.Generator/ElementPlusGenerator.cs` 和上游 `2.14.5` metadata 为单一来源。生成器 `elementplus --check` 报告 `111 components and 2 directives`，工作区保持生成一致性。组件绑定现在按 `element-plus/<component>/<export>` 逻辑入口生成，root 入口只承载插件、版本和指令等全局 API；每个组件的 JS/CSS 通过 manifest 入口闭包按需物化。官方 SG 回归 `RazorSgOfficialElementPlusNaturalAuthoringRuntimeTests` 验证 `ElButton` 的枚举与 click、`ElInput` 的 `VueStringNumberValue` 双向绑定、named slot、属性 splat 以及组件级 import；Emit 的 `LibraryMaterializerTests.Materialize_ElementPlusButtonEntry_DoesNotMaterializeAlertOrRootAssets` 验证只请求 Button 时不会复制 Alert、root bundle 或全局样式。组件交互语义由上面的官方 SG + NodeApi 运行时测试覆盖。
