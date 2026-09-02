# RazorVue v0.27.0 之后的下一步开发计划

> 日期：2026-09-02
> 状态：下一阶段的决策与执行计划，不是新的 Support 声明。每项能力仍必须以当前
> ledger、实现、测试和适用运行 profile 的证据为准。
> 基线：`v0.27.0`，分支 `feature/unified-library-carriers`。

本文件回答一个比“还能支持什么”更实际的问题：在当前边界下，下一步支持什么最能提高
页面和组件作者的开发效率，同时不会把 RazorVue 变成一个无法兑现的“完整 Blazor 兼容层”。
已有的详细合同继续由[当前状态](./current-status.md)、[零摩擦执行计划](./razorvue-zero-friction-plan.md)、
[Blazor-first 开发者体验路线图](./razorvue-developer-experience.md)和[作者面诊断路线图](./razorvue-authoring-diagnostics.md)
维护；本文只负责排序、取舍和下一轮的交付顺序。

## 0. 结论先行

`v0.27.0` 已经越过“能不能生成和发布”的阶段。核心编译器、CLR 映射、官方 Razor Source
Generator、Vue 组件绑定、Emit carrier、SPA/SSR 发布消费者和真实浏览器门禁都有可复现证据。
下一阶段最有价值的工作不是再造一套产物管线，也不是盲目扩大 CLR 白名单，而是让常见页面在
三处更自然：

1. 作者第一次遇到限制时，在自己的 `.razor`/`.razor.cs` 位置得到明确且可复制的诊断。
2. 表单、校验、提交、导航这些高频业务动作拥有强类型且符合 Razor 习惯的公共契约。
3. 现有 adapter 和 binding 在 `Debug`、Release package、真实浏览器以及适用的 SSR/hydration
   中保持同一行为，不再由单页 bridge 掩盖缺口。

认证状态、SSR 状态交接、构造函数注入和浏览器后退拦截确实有价值，但它们需要新的宿主协议，
不应在下一轮以“先生成出来再观察浏览器结果”的方式实现。Microsoft/Blazor 内置 UI、
`IJSRuntime` 字符串互操作、server-only 服务和任意 CLR runtime identity 则不属于本产品的
下一步支持目标。

## 1. 当前基线与问题归因

### 1.1 已经足够可靠的部分

| 领域 | `v0.27.0` 结论 | 下一步处理 |
| --- | --- | --- |
| C# -> ESTree -> ECMAScript module | `Support`；求值顺序、导入、source origin、稳定命名和失败传播已有核心回归 | 只做回归保护和性能观测，不为 RazorVue 添加旁路 lowering |
| 普通 Razor、组件组合、fragment/slot、泛型、`@bind`、事件、循环、生命周期、`@key`、`@ref` | `Support`，部分复杂形状为 Compatibility Adapter | 继续扩展自然写法矩阵；不要以 JazorAdmin 的单页 bridge 作为新公共 API |
| `ParameterView`、可写 `[Inject]`、typed/named/nested cascading | 高频子集 `Support` | 保持 typed 参数优先；未物化的枚举、`TryGetValue`、`ToDictionary` 继续 Guidance |
| route host、同源内部 `NavigateTo`、LocationChanging | 已证明的子集 `Support`；replace/`LocationChanged` 小闭环已交付 | 继续固化 PathBase、history length、state 和事件顺序证据；复杂 browser history 另行设计 |
| 核心及七组扩展 DOM 事件、`ElementReference.FocusAsync` | getter-only browser interactive 子集 `Support` | 只按真实需求补 getter；constructor/setter/files/items 等不扩张 |
| TDesign、Vue Router、Pinia、Vuetify、Element Plus、Vue Data UI、Vu Icons、Style | 已声明 binding/resource closure `Support` | 先审查自然 authoring，再按两个独立消费者的需求补成员 |
| package carrier、manifest、Emit、SPA/SSR 发布链路 | 交付基线可靠；Emit 回归 `185/185` | 固化独立 consumer 证据，不引入第三种 carrier |

截至基线，Compiler、CLR 和 Razor SG 回归规模分别约为 `10,703`、`5,089` 和 `4,940` 个场景；
已发布的 Release workflow 还通过了包打包、Pinia、Windows SPA/SSR consumer、NuGet trusted
publishing 和 GitHub Release。数字用于说明基线，不代表可以跳过新能力自己的语义和浏览器证明。

### 1.2 仍然影响作者效率的部分

| 症状 | 实际原因 | 正确 owner |
| --- | --- | --- |
| 作者要读 generated C# 或内部 lowering 才知道为什么失败 | 诊断位置和替代说明还需要产品化，部分边界只能在 final Compilation 判断 | RazorVue compatibility analyzer + final diagnostics |
| JazorAdmin 的 typed TDesign 页面仍可能暴露 callback 转发、命名后缀和 slot 类型摩擦 | 个别 binding 的 C# 公开形状仍不够自然，不能直接说明 compiler 缺陷 | `ECMAScript.*` binding API review，必要时才改 `Jazor.RazorVue`；不恢复通用 sample-local bridge |
| CRUD 页面需要统一的字段错误、提交中和异步校验表达 | 内置 `EditForm`/`InputBase` 协议被明确排除，第三方组件间没有共同的强类型表单契约 | TDesign/组件库 contract + RazorVue bind/runtime |
| `replace`、LocationChanged 和复杂 history 状态边界分散 | 同源内部 replace/`LocationChanged` 的公开矩阵已由 Authoring browser smoke 固化；复杂 history 仍未覆盖 | CLR navigation mapping + route host |
| 认证和 SSR 初始数据能工作，但没有稳定的状态 envelope | 需要版本、生命周期、失配和授权边界；不能靠全局 JS 状态补齐 | `Jazor.AspNetCore` + `Jazor.Emit` + typed endpoint |

## 2. 取舍标准

新条目同时按以下顺序评审，不能只因为某个 API 在 Blazor 中存在就进入计划：

1. **频率和收益**：是否直接影响列表、表单、详情、导航、登录等高频页面。
2. **C# 可表达性**：能否以明确参数、返回值、union、overload 或命名 contract 表达；不得用
   `object?`、开放泛型或裸字符串把问题推给运行时。
3. **官方 Razor SG 合法性**：标准 Razor 写法必须能被 SDK 正常绑定，不以 RazorVue 私有语法
   绕过 Razor 编译器。
4. **语义 owner 清晰**：C# 语义走 `Jazor.Compiler`，CLR/browser 映射走 `Jazor.CLR`，组件
   contract 走绑定包，宿主和 SSR 走 `Jazor.AspNetCore`/`Jazor.Emit`。
5. **证据成本可控**：必须能建立 reference、official SG、module/runtime、真实 browser、
   package/SSR（适用时）证据；无法证明的项目先保持 Guidance。
6. **不扩大固定边界**：不因“兼容”之名引入静默 fallback、反射、字符串 JS 调度、server
   对象泄漏或第二套组件框架。

计划中的状态含义如下：`Support` 是已经或将要完成四层证据的产品能力；`Guidance` 是在
作者源明确说明差异并给出强类型替代；`Reject` 是没有可靠浏览器语义或不属于产品范围的
固定边界。`Planned`/`In proof` 只是内部进度，不能对外当作可用。

## 3. P0：作者体验与小闭环

P0 的共同特点是收益高、可以沿现有架构实现，且不需要改变固定边界。预计形成下一次
`MINOR` 能力版本；纯文档和缺陷级诊断修复仍按 `PATCH` 规则处理。

### N0-01 作者第一成功路径与诊断产品化

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 首次使用者需要从 generated `BuildRenderTree`、模块 import 或运行时错误反推限制；当前已有 `JAZORVCA001`-`011` 与 `JAZORVGA020`-`026`，但 quickstart 入口、示例和失败分类需要统一。 |
| 推荐状态 | `Support`（已交付：作者正常写法零噪音）+ `Guidance`（确实无法保真的形状）；不新增 generated-code analyzer。 |
| owner | `Jazor.RazorVue` compatibility analyzer、final diagnostics、`docs/03-guides`、`samples/RazorVue.Authoring`。 |
| 实现路线 | 新增 `docs/03-guides/razorvue-quickstart.md`；所有片段从已通过的 Authoring sample 复制；将 server-only service、标准内置组件、SSR state/form handoff、IJSRuntime 等诊断链接到最小替代；保持 source analyzer 与 final Compilation 互斥归属。 |
| 依赖与风险 | 依赖现有 source span registry、HelpLink 和 no-partial-artifact 契约；不得为了提前报错分析 `.razor.g.cs`，不得让 analyzer 读取 generator 结果。 |
| 最低验收 | 已通过 quickstart、Authoring sample、作者源诊断和 analyzer scope 回归；正常写法 0 warning，失败 fixture 在作者源给出稳定 ID、span、HelpLink 和替代；`dotnet build` 与 isolated package consumer 结果一致，错误时无 descriptor、module 或坏 bundle。 |
| 不包含 | 不自动重写生命周期、认证、表单状态；不复制 `JAZORVGA` 到通用 `Jazor.Analyzer`。 |
| 版本 | 文档/诊断修复为 `PATCH`；若新增可执行作者面或公共 analyzer contract，随对应 `MINOR`。 |

### N0-02 强类型表单与校验 contract

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 管理页面普遍需要字段绑定、parse/format 错误、字段级提示、提交中状态、异步提交、重置和失败后保留输入；仅靠每个页面自写状态会产生重复状态逻辑。 |
| 推荐状态 | `Support`（已交付最小 Authoring slice：typed model、字段规则、bind、submit/reset/pending 与错误状态）；完整 CRUD/SSR form protocol 仍为后续候选，不实现 `EditForm` 兼容。 |
| owner | 首选 `ECMAScript.TDesign`（必要时同步 Vuetify/Element Plus contract），`Jazor.RazorVue` 负责 bind/event/lifecycle 语义；表单提交的服务端事实由应用 endpoint 负责。 |
| 实现路线 | 以强类型 model、字段/校验结果、`Value`/`ValueChanged`、submit/reset/pending 事件为核心；保留 union、overload 和 collection initializer 的自然 Razor 写法；只在 C# 无法表达的窄边界提供显式 factory。组件库负责 UI，RazorVue 负责单次求值、异步 callback 和错误传播。 |
| 依赖与风险 | 需要与 official Razor SG 的 generic component、typed slot、`@bind:get/set/after` 和 async lifecycle 矩阵对齐；不能靠 `object?` 或反射扫描 `DataAnnotations` 让任意模型“自动工作”。 |
| 最低验收 | 当前已由 `samples/RazorVue.Authoring` 证明 typed form 的初始值、空值校验、字段规则、reset、异步 submit、dialog 状态和 isolated Release browser/package closure；完整 create/edit/delete、取消/重试、重复提交、路由离开和 SSR 首屏副作用矩阵尚未交付，不得据此宣称完整 form protocol。 |
| 不包含 | `EditForm`、`InputBase<T>`、`Input*`、`ValidationMessage`、`InputFile`、antiforgery、enhanced form post、隐式 server validation protocol。 |
| 版本 | 新公共 contract 或新的 bind/runtime 能力进入 `MINOR`；仅修复 binding 名称/诊断进入 `PATCH`。 |

### N0-03 导航常用小闭环

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 现有 route host 和同源内部 `NavigateTo` 已支持，但登录跳转、向导、重复提交防止和页面状态恢复常需要替换历史记录或订阅 `LocationChanged`。 |
| 推荐状态 | `Support`（已交付：限定同一 base URI 的应用自有 route host 小闭环）。 |
| owner | `Jazor.CLR` navigation mapping、RazorVue route host、必要时 `Jazor.AspNetCore`。 |
| 实现路线 | 已补齐 `Microsoft.AspNetCore.Components.NavigationOptions.ReplaceHistoryEntry` 的内部映射、`LocationChanged` 订阅/注销/顺序和既有 `HistoryEntryState` 的行为矩阵；继续使用强类型 C# API。`Microsoft.AspNetCore.Components.NavigationOptions` 与 `ECMAScript.NavigationOptions` 分别属于 Blazor 与 Web Platform，不改名，通过命名空间或 alias 消歧。 |
| 依赖与风险 | 需要 reference oracle、URI 编码、重复导航、handler dispose 和 package browser consumer；浏览器后退发生在 history 改变之后，不能把 popstate cancellation 伪装成现有内部导航。 |
| 最低验收 | 已通过 official SG、CLR whitelist、Deno、HTTP-origin 真实浏览器和 isolated Release package；Authoring PathBase journey 覆盖 push/replace、`history.length`、`HistoryEntryState`、实际 URI、`LocationChanged` 非零计数、query/hash state、not-found 和浏览器后退路径，另有注销回归；快速连续导航及更复杂 history 仍按边界处理。无 console error。 |
| 不包含 | 外部 URI/`forceLoad`、server circuit、SSR route identity、复杂约束、完整 history state 序列化、`popstate`/`hashchange` 可取消拦截。 |
| 版本 | 新的导航行为属于 `MINOR`；纯错误修复按 `PATCH`。 |

### N0-04 现有 Support 的稳定性与故障可见性

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 产物链路已经可靠，但新 lowering 最容易回归为缺失 import、未定义 helper、错误 alias、部分 catalog 或 runtime-first failure。 |
| 推荐状态 | 维护现有 `Support`，优先级与 P0 同等重要；不把它包装成新功能。 |
| owner | `Jazor.Compiler`、`Jazor.RazorVue`、`Jazor.Emit`。 |
| 实现路线 | 固化 ESTree 层自由标识符/声明/导入完整性检查；继续锁定 temp/import/source-map 稳定性、错误时无 partial artifact、manifest selected closure 和 source-origin；把失败映射到已有 typed diagnostics。 |
| 依赖与风险 | 检查必须识别 property key、member property、label、binding declaration 和允许的 ECMAScript global；禁止 regex 扫描 `.mjs` 或宽泛 global allow-list。 |
| 最低验收 | Compiler/Razor SG/Emit 主线全过；最终 ESTree 组合通过 `VueModuleIntegrityValidator` 的 lexical binding 回归（含 official Razor SG 生成模块）；每个新 framework slice 仍有独立 package consumer；Release browser console errors=`0`；稳定输出在并行和不同工作树下不漂移。 |
| 不包含 | 第三种 carrier、运行时 fallback、无证据的优化、把 `Jazor.Emit` 重新合并进 compiler。 |
| 版本 | 回归修复和诊断改善为 `PATCH`；影响产物契约的新增能力另走 `MINOR`。 |

### N0-05 组件 binding 与 bridge 收敛

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | TDesign 的主要自然 Razor 场景已通过，但 JazorAdmin 仍可用来发现重复的 callback 转发、命名后缀、generic/non-generic 入口和 slot 类型摩擦。 |
| 推荐状态 | `Support` 按需扩展；单页面 workaround 不升级为公共 API。 |
| owner | 对应 `ECMAScript.TDesign`/`ECMAScript.Vuetify`/`ECMAScript.ElementPlus` binding，必要时 `Jazor.RazorVue`。 |
| 实现路线 | 先建立两个独立消费者或一个自然 authoring fixture + 一个真实页面的证据；再决定删除 bridge、调整 binding 类型、增加窄 overload，或保留领域 wrapper。保持 native union、required 参数、typed slot 和 attribute splat，不引入弱类型逃生口。 |
| 依赖与风险 | API 变更要同时检查 official Razor SG、隐式 union 构造、重载解析、包资源 closure 和已有页面迁移；binding 包不承载 CLR whitelist。 |
| 最低验收 | Authoring sample 和 JazorAdmin 的目标页面不需要应用侧 cast/手写 builder；包 consumer 0 warning/0 error；真实浏览器覆盖输入、表格、弹窗、slot、回调和错误状态。 |
| 不包含 | 一次性映射所有第三方长尾组件；不因为 JS 端接受 `any` 而把 C# API 改成 `object?`。 |
| 版本 | 新 binding/公共 API 为 `MINOR`；兼容性修复为 `PATCH`，破坏性命名迁移必须写 CHANGELOG。 |

## 4. P1：值得做，但先完成协议

P1 的共同特点是用户价值明确，却会改变宿主生命周期或安全边界。只有协议、reference
行为、失败矩阵和真实 consumer 都冻结后，才能从 `Guidance`/`In proof` 提升为 `Support`。

### N1-01 Typed authentication state

认证状态对后台、门户和下游客户端很有价值，**可以支持，但不应直接兼容 `AuthorizeView`**。
推荐设计是显式 typed browser provider，加服务端 endpoint 返回版本化的匿名/登录/过期/拒绝
状态 envelope；endpoint 授权始终是安全事实来源，provider 只负责 UI 可观察状态。必须覆盖
SSR 首屏、hydration、刷新、登出、过期和 403，不把 claims 放进无版本全局变量。

当前保持 `Guidance`：缺少已证明 provider 时由 `JAZORVCA007` 说明注册和 endpoint 替代，
`AuthorizeView`、`AuthorizeRouteView`、`CascadingAuthenticationState` 保持 `Reject`。owner
为 `Jazor.AspNetCore`、RazorVue host 和 `Jazor.CLR`；最低验收是 reference oracle、匿名/认证/
过期/403 的真实 browser/package consumer，以及适用 SSR 的同一 envelope 证明。完成后属于
`MINOR`，在此之前不把历史 adapter 记为支持。

### N1-02 版本化 SSR bootstrap 与状态交接

显式 typed endpoint DTO 已能解决很多 SSR 首屏数据需求；真正值得支持的是一个版本化的
bootstrap/state contract，而不是把 `PersistentComponentState` 静默模拟成浏览器全局状态。
该 contract 至少要规定 payload version、组件/请求边界、序列化失败、失配处理、一次性副作用
和 hydration 后的所有权。owner 为 `Jazor.AspNetCore` + `Jazor.Emit`，依赖 N1-01 的 provider
边界和已有 SSR runner/hydration envelope。

当前 `PersistentComponentState`、`[PersistentState]`、`[SupplyParameterFromForm]` 继续由
`JAZORVCA011` 提供 `Guidance`。只有 packaged SSR consumer 能证明首屏、刷新、重复 hydration、
过期 payload 和错误传播后，才考虑对一个明确子集标为 `Compatibility Adapter`；不承诺完整
Blazor prerender identity 或 enhanced form protocol。

### N1-03 构造函数注入与 activation protocol

构造函数注入对迁移现有 Blazor 组件很方便，但它不是属性注入的简单别名：必须同时定义
service catalog、构造函数选择、参数化 component activation、字段初始化、base-to-derived
顺序、生命周期和 SSR/浏览器实例 lifetime。当前 `JAZORVGA024` 拒绝 constructor injection、
primary-constructor 参数、`this(...)` 和 `base(args)` 是正确的行为。

本阶段只做有界 feasibility spike 和更好的 authored-source Guidance；只有至少两个真实
消费者、完整 activation 矩阵和 browser/package/SSR 证据都成立，才实现一个强类型的受限子集。
禁止使用“没有 selector 就猜 constructor”或 `arguments.length` fallback。owner 为 RazorVue、
Compiler、browser service host；成功后是 `MINOR`，失败则保留明确 Reject，不影响当前属性注入。

### N1-04 后退/前进与复杂 URI 状态

`popstate`/`hashchange` 的可取消拦截看似是 NavigationManager 的自然补齐，实际发生在浏览器
history 已变化之后，需要恢复 URL、处理竞态和避免页面闪烁。它只有在用户确实需要“离开页面
确认”时才值得做。先记录 browser history state、fragment、编码、replace 和快速连续导航的
协议；不要把内部 `NavigateTo` 的 cancellation 结果外推到 popstate。

推荐状态为 `P2 design`，当前保持 `Guidance/Reject` 边界。owner 为 route host + ASP.NET Core
host；最低验收需要真实浏览器的 back/forward、取消、supersede、dispose、SSR profile 和
Release package。没有完整协议前，应用可使用页面级显式 guard，不增加隐藏 fallback。

## 5. P2：真实需求驱动，不提前承诺

### N2-01 高需求 binding 和 DOM projection

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 真实页面遇到的组件成员或 DOM getter 仍可能缺失，作者被迫绕过 typed binding。 |
| 推荐状态 | `Support` 按需扩展；没有重复消费者时保持现状。 |
| owner | 对应 `ECMAScript.TDesign`/`ECMAScript.Vuetify`/`ECMAScript.ElementPlus` binding、`Jazor.CLR` generator/module；必要时 `Jazor.RazorVue`。 |
| 实现路线 | 只有两个独立应用或一个组件库 fixture + 一个真实页面重复遇到时才补成员；保留 getter-only native carrier，并让资源 manifest 自动收集选中 entry。 |
| 依赖与风险 | 需要生成器、CLR module、compiler、official SG、Deno、真实 browser 和 package consumer 全链路；DOM constructor/setter/identity、synthetic payload、file input、`DataTransfer.files/items`、非 getter TouchList 操作和任意 DOM method 不因对称性自动放行。 |
| 最低验收 | reference metadata（适用时）、module/source map、真实事件或组件 journey、isolated Release package 和 console errors=`0`。 |
| 不包含 | 一次性映射全部第三方长尾 API；不以 `object?`、裸字符串或 runtime reflection 代替 binding。 |
| 版本 | 新 binding/member 为 `MINOR`；映射缺陷为 `PATCH`。 |

### N2-02 性能、缓存和预加载

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 组件数量和更新频率增长后，首次构建、bundle 体积或更新耗时可能成为实际瓶颈。 |
| 推荐状态 | `Support` 维护 + `P2` 需求驱动优化；没有 benchmark 证据不排期。 |
| owner | `Jazor.Compiler`/`Jazor.RazorVue`（lowering 和生成吞吐）、`Jazor.Emit`（资源物化）；宿主负责部署配置。 |
| 实现路线 | 以 `RazorVue.Authoring`、JazorAdmin 和一个 Release consumer 建立基线，再评估 block/patch 粒度、handler cache、生成并行、缓存或 preload；先保留可观察语义和 deterministic output。 |
| 依赖与风险 | 优化可能改变单次求值、identity、source map 或 hydration 时机；必须能从 benchmark 区分编译、网络、mount 和 update 成本。 |
| 最低验收 | 浏览器 heap、首屏/更新耗时、产物大小、生成耗时和 hydration 回归均改善或不退化；Compiler/Razor SG/Emit 和真实 browser/package gate 全过。 |
| 不包含 | 以“更像手写 Vue”作为目标、无证据的全局 memoization、默认 immutable cache 或第三种 artifact carrier。 |
| 版本 | 纯优化/回归修复按 `PATCH`；若新增配置或改变产物契约按 `MINOR`，并记录迁移。 |

### N2-03 CLR 长尾与集合 API

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 少量业务页面可能需要尚未映射的 BCL 成员或集合操作。 |
| 推荐状态 | `Support` 仅按真实需求扩展；长尾默认 `Guidance`/使用点 Reject。 |
| owner | `Jazor.CLR` module/source、`Jazor.Compiler` whitelist consumer 和对应 CLR tests。 |
| 实现路线 | 先确认两个真实消费者、现有 carrier 能表达语义且不需要 CLR runtime identity，再补 generator metadata、runtime helper、compiler emission 和文档；容量、反射等长尾继续 `Op.Discard`。 |
| 依赖与风险 | 必须保持 comparer/null/NaN/数值精度、迭代顺序和异常语义；`List<Unsupported>`、`Task<Unsupported>`、`Dictionary<TKey, Unsupported>` 等 erased generic shape 不应被提前拒绝，也不建立开放泛型 fallback。 |
| 最低验收 | CLR whitelist metadata、Compiler emission、runtime behavior、Deno/必要时真实 browser 和 package closure 全部有回归。 |
| 不包含 | 完整 CLR 对象身份、反射/线程/文件系统/网络 runtime 或只为 API 数量而增加的成员。 |
| 版本 | 新 BCL/runtime support 为 `MINOR`；错误映射/诊断修复为 `PATCH`。 |

### N2-04 暂不进入的框架方向

| 项目 | 决策 |
| --- | --- |
| 用户痛点 | 这些方向可能被误读为“只差几个 adapter”，导致 roadmap 无限膨胀。 |
| 推荐状态 | `Reject` 或另立产品计划；本路线不排期。 |
| owner | 无当前实现 owner；若未来改变范围，由产品/架构评审建立独立 owner 和证据合同。 |
| 实现路线 | 保持 `Jazor.React`/`Jazor.RazorReact` 未接受；完整 Blazor UI、server circuit、任意反射/线程/文件系统/网络 runtime 和脱离 typed binding 的动态 JavaScript 不进入 P2 backlog。 |
| 最低验收 | 本路线只要求作者源或 final pipeline 有稳定 Reject/Guidance、无 partial artifact 和替代方向；不为它们建立伪 Support 测试。 |
| 不包含 | 任何隐式兼容 adapter、运行时字符串调度、server 对象注入或“先生成再看浏览器”的实验性公共 API。 |
| 版本 | 不进入本计划版本；未来若改变产品方向，另立范围、架构和版本计划。 |

## 6. 明确“应该支持 / 只需指导 / 应拒绝”

| 能力 | 能否实现 | 用户价值 | 本计划决策 | 原因 |
| --- | --- | --- | --- | --- |
| 作者源码诊断、quickstart、失败替代 | 可以 | 很高 | **已交付 Support** | 成本低，能消除 runtime-first 试错，不扩大运行时 |
| 强类型表单、字段校验、提交状态 | 可以 | 很高 | **最小 Authoring slice 已交付；完整 contract 仍为候选** | CRUD 高频，能复用现有 bind/event；不需要内置 `EditForm` |
| `NavigationOptions.ReplaceHistoryEntry`、`LocationChanged` | 可以 | 高 | **已交付 Support（同源内部 route host 子集）** | 浏览器 API 简单且对登录/向导有直接收益，范围可控 |
| TDesign 等组件的重复 binding 摩擦 | 可以 | 高 | **按证据支持** | 先 API review，避免 bridge 演化成第二框架 |
| typed AuthenticationStateProvider | 可以但需要宿主协议 | 高 | **P1 先 Guidance/设计** | claims、SSR 和授权边界必须可验证 |
| 版本化 SSR bootstrap/state | 可以但需要协议 | 中高 | **P1 先 Guidance/设计** | 必须解决版本、失配和 hydration 副作用 |
| 构造函数注入 | 部分可行，成本高 | 中高 | **P1 feasibility；当前 Reject** | activation/lifetime/继承顺序未闭合，属性注入已足够覆盖常见页面 |
| popstate/hashchange cancellation | 可行但复杂 | 中 | **P2 需求驱动** | history 已改变后的恢复和竞态不能靠现有内部导航协议冒充 |
| `ParameterView` 枚举、`TryGetValue`、`ToDictionary` | 可实现但收益低 | 低 | **Guidance** | typed 参数更稳定，动态 snapshot 会扩大运行时协议 |
| Microsoft 内置 UI 组件 | 不属于当前产品入口 | 视组件而定 | **Reject** | 由 TDesign/Vuetify/Element Plus 或应用自定义组件承担 |
| `IJSRuntime`/字符串 JS interop/动态 import | 不应实现 | 表面高、风险极高 | **Reject** | 破坏静态 import、资源闭包和类型边界，改用 typed ECMAScript/WebIDL |
| `DbContext`、`HttpContext`、Identity manager 等 server-only 服务 | 浏览器不可等价 | 视业务而定 | **Reject/Guidance** | 使用 typed endpoint 和 browser client，服务器仍是事实来源 |
| 任意 DOM method、完整 CLR runtime identity、反射/线程/文件系统 | 不适合当前模型 | 长尾 | **Reject** | 无稳定 carrier 或会引入不可确定的运行时语义 |

## 7. 执行顺序与版本拆分

### Phase A：下一次 minor 前

1. N0-01 已完成：quickstart、作者诊断入口、失败矩阵和正常用法零噪音回归。
2. N0-02 已完成最小 typed form contract；完整 create/edit/delete、SSR form protocol 和第二个真实页面消费者仍需单独验收。
3. N0-03 已完成 replace/LocationChanged 小闭环，并冻结同源内部 route host 行为矩阵。
4. 以 N0-04 作为所有改动的共同门禁；同步执行 N0-05 的 binding/API review，不因时间压力添加
   `object?`、字符串路由或页面专用公共类型。

### Phase B：下一次 minor 之后

1. 先完成 N1-01/N1-02 的协议和 reference fixture，再决定是否实现一个受限 Support 子集。
2. 对 N1-03 只交付 activation 设计和 authored-source Guidance；没有完整证据就不改变 Reject。
3. N1-04 只有在真实应用提出可复现的 back/forward guard 需求时进入实现。

### Phase C：持续维护

按两个独立消费者、一个可复现最小 fixture 和四层证据门禁决定 N2 binding、DOM、性能和 CLR
长尾。没有真实需求的项目不占用公共 API、测试预算和发布版本号。

## 8. 统一验收门禁

### 8.1 Support 的四层证据

| 层 | 必须证明 |
| --- | --- |
| L1 作者源/official SG | 标准 Razor/C# 能绑定，正常写法无额外内部符号 |
| L2 语义/产物 | `SemanticWalker`、RenderEmitter、imports、source map、module closure 保留求值顺序、单次求值和 identity |
| L3 reference/真实浏览器 | 与适用的 Blazor reference oracle 在作者可观察行为上相符；浏览器 mount、更新、错误和 dispose 正确 |
| L4 Release package/SSR | 从 NuGet/manifest 选定闭包可交付；触及 SSR 时证明首屏、hydration、状态和副作用边界 |

`Deno` 和静态 snapshot 是快速回归，不单独构成 Support 证据。每个新条目必须同步更新
ledger、实现、测试、作者指南和[当前状态](./current-status.md)。

### 8.2 Guidance/Reject 的验收

- 诊断落在作者 `.razor`/`.razor.cs` 或 final Compilation 可映射的源位置，具有稳定 ID、severity、
  HelpLink、原因和最小强类型替代。
- 不产生 partial `ModuleCatalog`、module、manifest 或看似可运行的 undefined bundle。
- source analyzer 不分析 generated C#，也不复制 `JAZORVGA020`-`026`；同一形状只由一个 owner
  报告。
- 文档不能把 `Guidance` 写成“基本支持”，不能把历史 adapter 或单页 workaround 写成 Support。

### 8.3 发布与量化指标

每个阶段至少记录以下指标，并与干净工作树和隔离 consumer 一起复现：

- Authoring sample 作者源码中的 RazorVue 内部符号、手写 builder、无意 cast/bridge 数量；目标为
  页面作者面 `0`。
- 首次成功构建耗时、package consumer warning/error 数量、浏览器 console error 数量；目标分别
  是可接受的一次构建、`0/0`、`0`。
- 生成模块自由标识符完整性、source-map source origin、selected resource closure 和输出稳定性。
- 适用 SSR 的首屏副作用次数、hydration mismatch、状态 envelope 版本/失配结果。

新增 lowering、binding、host contract 或支持面走 `MINOR`；文档、诊断和纯修复走 `PATCH`；
破坏性 API 迁移按[发版与版本规则](../03-guides/release-and-versioning.md)写明迁移路径。

## 9. 归属与不变边界

| 责任 | owner | 不应做的事 |
| --- | --- | --- |
| C# 语义、类型、import、临时值、source origin | `Jazor.Compiler` | 不为 RazorVue 产品协议添加框架特判或 raw-JS fallback |
| CLR/browser carrier 与白名单 | `Jazor.CLR` + generator | 不在 `ECMAScript.Blazor` 维护第二份 mapping/resource catalog |
| RenderTree、component closure、diagnostics、route host | `Jazor.RazorVue` | 不要求页面作者手写 builder、Vue AST、模块字符串或 marker protocol |
| SSR provider/state、宿主生命周期 | `Jazor.AspNetCore` + `Jazor.Emit` | 不把服务器对象、隐式全局状态或未版本化 payload 带进 hydration |
| TDesign/Vuetify/Element Plus 等组件 contract | 对应 `ECMAScript.*` binding | 不用 `object?`、裸字符串 component name 或通用应用专用 bridge 扩大公共面 |
| 真实消费者和参考页面 | `samples/RazorVue.Authoring`、JazorAdmin | 不以单页 workaround 定义平台能力 |

以下边界在本计划期间保持不变：Microsoft/Blazor 内置 UI 组件、`IJSRuntime` 家族、server-only
服务、server circuit、完整 SSR/prerender identity、动态 runtime `Type`、任意 DOM method、
完整 CLR runtime identity、反射/线程/文件系统和未声明的外部 API。它们只有在产品范围另行
评审并提出可证明的宿主协议后，才可能建立独立计划。

## 10. 完成标准

下一阶段完成，不以“支持了多少 API”计数，而以以下结果为准：

1. clean checkout 的 quickstart 能在不阅读 compiler 内部文档的情况下完成一个 typed CRUD 页面。
2. 表单和导航 P0 场景在 official SG、真实浏览器和 isolated Release package 中可观察一致。
3. 已知不支持形状在作者源或 final pipeline 首次构建被解释，没有 runtime-first 失败或 partial artifact。
4. JazorAdmin 直接消费 typed binding；只有有领域理由的 wrapper 才可保留，重复的 bridge/cast
   不回流为公共 API，或必须有明确 API review 记录。
5. 认证、SSR 状态、构造函数注入和 history cancellation 要么有完整协议和证据，要么继续保持
   清晰的 Guidance/Reject，而不是模糊的“部分兼容”。
6. 当前支持边界、ledger、作者指南、测试入口和版本说明彼此一致。

## 11. 相关文档与验证入口

- [当前状态](./current-status.md)
- [RazorVue “零摩擦”执行计划](./razorvue-zero-friction-plan.md)
- [RazorVue Blazor-first 兼容与开发者体验路线图](./razorvue-developer-experience.md)
- [RazorVue 作者面诊断路线图](./razorvue-authoring-diagnostics.md)
- [Blazor CLR 类型支持计划](./blazor-clr-support-plan.md)
- [JazorAdmin 生产级参考应用路线图](./admin-reference-app.md)
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj`
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj`
- `dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs`
- `dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo`
