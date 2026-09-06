# 下一阶段

> 本页安排下一轮投入的优先次序，而不提前新增 `Support` 声明。能力是否可用，始终以[当前状态](./current-status.md)、源码和可复现验证为准。

## 投入准则

下一阶段不重开编译产物、类库 carrier 或 Razor-to-Vue 主链路的设计。投入从高频页面和真实消费者出发：先让既有能力更自然、更易诊断，也更容易在发布环境中证明；只有 C# 类型系统无法表达，或浏览器语义确有缺口时，才新增 runtime 协议。

每个条目必须遵守以下原则：

1. **C# 契约优先**：以明确参数、返回值、union 和 overload 表达作者面；不以 `object?`、裸字符串或示例局部桥接转移问题。
2. **责任清晰**：C# 语义归 `Jazor.Compiler`，CLR/browser 映射归 `Jazor.CLR`，组件 API 归对应 binding，SSR 与宿主协议归 `Jazor.AspNetCore` / `Jazor.Emit`。
3. **拒绝静默降级**：不引入原始 JavaScript fallback、第三种 carrier、第二套组件协议或未经证明的运行时猜测。
4. **证据先于声明**：官方 Razor SG、模块运行时、真实浏览器、隔离的 package consumer，以及适用的 SSR/hydration 必须共同证明新增能力。

## P0：编写体验与交付闭环

具体执行顺序、交付物、测量协议和 Definition of Done 见 [RazorVue P0 执行计划](./p0-plan.md)。

优先处理已经进入真实页面、并直接影响作者体验和交付可信度的路径。

| 优先级 | 目标 | 主要责任层 | 完成条件 |
| --- | --- | --- | --- |
| P0-1 | 收敛 RazorVue 的编写体验与组件绑定。优先解决 TDesign 表格列、slot、表单及回调等高频自然写法，使真实页面不依赖应用侧类型转换、手写 builder 或通用桥接。 | 对应 `ECMAScript.*` 绑定；必要时 `Jazor.RazorVue` | 至少一个独立的 authoring fixture 和一个真实消费者页面通过 Razor SG、Release package 与浏览器验证；公共 API 仍保持强类型。 |
| P0-2 | 让限制明确呈现在作者源码中。继续完善 compatibility analyzer、final Compilation diagnostics、HelpLink 和最小替代路径，保证失败时不留下 partial module、catalog 或 bundle。 | `Jazor.RazorVue`、指南与 Authoring sample | 正常写法无额外警告；不支持形状具有稳定诊断 ID、源位置和替代说明；源码项目与 package consumer 表现一致。 |
| P0-3 | 巩固已声明 framework primitive 与宿主交付的真实证据。覆盖 Debug、Release、HMR、PathBase、SSR/hydration 的一致性，以及新 binding 或 framework slice 的独立消费者回归。 | `Jazor.Emit`、`Jazor.AspNetCore`、`Jazor.CLR`、RazorVue | 产物闭包、source map、浏览器交互和适用 SSR 行为可复现；失败显式传播，不以静默 CSR 或旧产物回退。 |
| P0-4 | 先测量，再优化 direct render 与 CLR runtime。性能候选必须先建立固定输入、warm-up、多轮测量、产物体积和行为基线，再决定是否实施。 | `Jazor.RazorVue`、`Jazor.Compiler`、`Jazor.CLR` | 改动前记录阈值和基线；优化在不改变求值顺序、值语义、导入稳定性和 source map 的前提下达到阈值，否则不实施。 |

## P1：需要先确立协议的能力

具体执行顺序、协议字段和 Definition of Done 见 [RazorVue P1 执行计划](./p1-plan.md)。

这些方向的价值已经明确，但会触及状态所有权、生命周期或 browser history 语义，因此首先需要完整的失败协议与真实消费者证据。在这些条件具备之前，它们保持 `Guidance` 或 `Reject`；局部实现不能被提前宣传为 `Support`。

| 方向 | 预期边界 | 前置条件 |
| --- | --- | --- |
| 强类型认证状态 | 显式 typed browser provider 与版本化 endpoint envelope；服务端 endpoint 始终是授权事实来源。 | 匿名、登录、过期、登出、403、刷新、SSR 首屏和 hydration 的完整 browser/package 证据。 |
| SSR bootstrap 与状态交接 | 版本化 payload，明确请求/组件所有权、反序列化失败、失配和一次性副作用。 | 不模拟 `PersistentComponentState` 或 enhanced form；先证明 packaged SSR consumer 的重复 hydration、错误传播和过期 payload 行为。 |
| 构造函数注入与复杂 activation | 只考虑有界的强类型子集，保持 base/derived、字段初始化、生命周期和 SSR/browser lifetime 的一致性。 | 至少两个真实消费者、完整 activation 矩阵与所有 profile 验证；不使用 selector 猜测或 `arguments.length` fallback。 |
| 后退/前进与复杂 URI 状态 | 不把已发生的 `popstate`/`hashchange` 伪装成可取消内部导航。 | URL 恢复、竞态、注册释放与用户确认行为先由 reference 和真实浏览器定义。 |

## P2：协议边界、评估与可测量优化

P2 的执行顺序、Definition of Done 和当前证据见 [RazorVue P2 执行计划](./p2-plan.md)。当前优先级如下：

| 方向 | 预期边界 | 当前状态 |
| --- | --- | --- |
| SSR 状态与表单交接 | 继续使用显式 `jazor-ssr-state` envelope；不模拟 `PersistentComponentState` 或 enhanced form。 | provider key 唯一性已硬化；表单协议仍 Guidance |
| 高级渲染 | Microsoft Blazor 内置 UI 组件统一 Reject；StreamRendering、localization 和复杂 validation 单独评估。 | 当前 SDK 内置 UI 组件已由 `JAZORVGA021` Reject；其余语义保持 Guidance |
| JS 互操作 | `IJSRuntime` 家族保持 Reject，使用 typed ECMAScript/WebIDL binding 替代。 | 已有稳定诊断与作者面回归 |
| 性能与交付 | 以固定 benchmark 比较 render/update、gzip、clean/incremental/HMR/Release。 | 已完成运行时基线，尚未宣称优化收益 |

## 不扩张的边界

P2 的具体执行顺序、Definition of Done 和证据门槛见 [RazorVue P2 执行计划](./p2-plan.md)。

下列边界在本阶段继续保持稳定，避免局部便利稀释整体契约。

- 不实现完整 CLR、任意外部 .NET API 或未经映射的运行时类型。
- 不将 Microsoft/Blazor 内置 UI 组件、`IJSRuntime` 字符串互操作或仅服务器端服务包装为 Vue 的兼容层。
- 不因 JavaScript 端接受 `any` 而弱化 C# binding 的类型契约。
- 不将 JazorAdmin 的单页临时绕行上升为公共平台 API。

## 写入当前状态的门槛

计划只有完成证据闭环，才可以成为读者能够依赖的当前能力。

一个计划条目完成后，必须同步更新实现、测试、作者指南和[当前状态](./current-status.md)。写入“当前状态”前，至少满足：

1. C# API 通过正常类型检查和官方 Razor SG 绑定，不依赖私有作者语法。
2. compiler lowering、CLR/runtime 或 binding 行为具有针对性的回归测试。
3. 最终模块在真实浏览器中完成目标交互，Release package consumer 可独立复现。
4. 涉及 SSR、hydration 或宿主状态时，额外证明该 profile 的所有权、失败传播和资源闭包。
5. 不支持的相邻形状仍有明确 `Guidance` 或 `Reject`，没有兼容 fallback。
