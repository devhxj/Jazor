# 下一阶段

> 本页安排下一轮投入的优先次序。能力状态以[当前状态](./current-status.md)、源码和可复现验证为准。

## 投入准则

下一阶段沿用编译产物、类库 carrier 和 Razor-to-Vue 主链路的既定设计。投入从高频页面和真实消费者出发，提升既有能力的作者体验、诊断质量和发布证据；C# 类型系统或浏览器语义出现明确缺口时，新增 runtime 协议。

每个条目必须遵守以下原则：

1. **C# 契约优先**：以明确参数、返回值、union 和 overload 表达作者面；运行时语义通过强类型契约进入模块。
2. **责任清晰**：C# 语义归 `Jazor.Compiler`，CLR/browser 映射归 `Jazor.CLR`，组件 API 归对应 binding，SSR 与宿主协议归 `Jazor.AspNetCore` / `Jazor.Emit`。
3. **显式语义路径**：原始 JavaScript、资源 carrier、组件协议和运行时行为均通过已声明契约进入产物。
4. **证据先于声明**：官方 Razor SG、模块运行时、真实浏览器、隔离的 package consumer，以及适用的 SSR/hydration 必须共同证明新增能力。

## P0：编写体验与交付闭环

具体执行顺序、交付物、测量协议和 Definition of Done 见 [RazorVue P0 执行计划](./p0-plan.md)。

优先处理已经进入真实页面、并直接影响作者体验和交付可信度的路径。

| 优先级 | 目标 | 主要责任层 | 完成条件 |
| --- | --- | --- | --- |
| P0-1 | 收敛 RazorVue 的编写体验与组件绑定。优先解决 TDesign 表格列、slot、表单及回调等高频自然写法，使真实页面不依赖应用侧类型转换、手写 builder 或通用桥接。 | 对应 `ECMAScript.*` 绑定；必要时 `Jazor.RazorVue` | 至少一个独立的 authoring fixture 和一个真实消费者页面通过 Razor SG、Release package 与浏览器验证；公共 API 仍保持强类型。 |
| P0-2 | 让边界明确呈现在作者源码中。继续完善 compatibility analyzer、final Compilation diagnostics、HelpLink 和最小替代路径，保证诊断路径维持完整 module、catalog 和 bundle。 | `Jazor.RazorVue`、指南与 Authoring sample | 正常写法保持 Razor SDK 诊断语义；缺少支持条件的形状具有稳定诊断 ID、源位置和替代说明；源码项目与 package consumer 表现一致。 |
| P0-3 | 巩固已声明 framework primitive 与宿主交付的真实证据。覆盖 Debug、Release、HMR、PathBase、SSR/hydration 的一致性，以及新 binding 或 framework slice 的独立消费者回归。 | `Jazor.Emit`、`Jazor.AspNetCore`、`Jazor.CLR`、RazorVue | 产物闭包、source map、浏览器交互和适用 SSR 行为可复现；失败显式传播，不以静默 CSR 或旧产物回退。 |
| P0-4 | 先测量，再优化 direct render 与 CLR runtime。性能候选必须先建立固定输入、warm-up、多轮测量、产物体积和行为基线，再决定是否实施。 | `Jazor.RazorVue`、`Jazor.Compiler`、`Jazor.CLR` | 改动前记录阈值和基线；优化在不改变求值顺序、值语义、导入稳定性和 source map 的前提下达到阈值，否则不实施。 |
| P0-5 | 将绑定资源迁移到标准 package project：Emit 生成 `jazor/package.json` 并调用 DenoHost runtime 恢复、冻结 `node_modules`，NetPack/SSR 复用同一依赖图，支持 npm、JSR 和 embedded `mjs` 的细粒度 JS/CSS tree shaking。 | `Jazor.Emit`、DenoHost、NetPack 集成层、各 `ECMAScript.*` binding | 通过[绑定包标准化与细粒度 tree shaking 迁移计划](./npm-jsr-binding-tree-shaking-plan.md)的 restore、offline/frozen lock、Web bundle、SSR、CSS/worker/static 和源码/NuGet consumer 矩阵；迁移阶段持续维护兼容 carrier。 |

## P1：需要先确立协议的能力

具体执行顺序、协议字段和 Definition of Done 见 [RazorVue P1 执行计划](./p1-plan.md)。

这些方向会触及状态所有权、生命周期或 browser history 语义，因此首先需要完整的诊断协议与真实消费者证据。证据闭环完成后，条目进入 `Support` 评审；在此之前使用 `Guidance` 或 `Reject` 状态。

| 方向 | 预期边界 | 前置条件 |
| --- | --- | --- |
| 强类型认证状态 | 显式 typed browser provider 与版本化 endpoint envelope；服务端 endpoint 始终是授权事实来源。 | 匿名、登录、过期、登出、403、刷新、SSR 首屏和 hydration 的完整 browser/package 证据。 |
| SSR bootstrap 与状态交接 | 版本化 payload，明确请求/组件所有权、反序列化结果、失配和一次性副作用。 | packaged SSR consumer 覆盖重复 hydration、错误传播和过期 payload 行为。 |
| 构造函数注入与复杂 activation | 考虑有界的强类型子集，保持 base/derived、字段初始化、生命周期和 SSR/browser lifetime 的一致性。 | 至少两个真实消费者、完整 activation 矩阵与所有 profile 验证；selector 由 Roslyn 绑定符号提供。 |
| 后退/前进与复杂 URI 状态 | 已发生的 `popstate`/`hashchange` 按独立事件协议处理。 | URL 恢复、竞态、注册释放与用户确认行为先由 reference 和真实浏览器定义。 |

## P2：协议边界、评估与可测量优化

P2 的执行顺序、Definition of Done 和当前证据见 [RazorVue P2 执行计划](./p2-plan.md)。当前优先级如下：

| 方向 | 预期边界 | 当前状态 |
| --- | --- | --- |
| SSR 状态与表单交接 | 继续使用显式 `jazor-ssr-state` envelope；表单使用应用自有 endpoint 协议。 | provider key、重复 hydration 和失败传播已硬化；表单协议使用 Guidance |
| 高级渲染 | Microsoft Blazor 内置 UI 组件和 `StreamRendering` 使用 Reject；localization 和复杂 validation 使用 Guidance，等待独立 typed 语义证据。 | 当前 SDK 内置 UI 组件由 `JAZORVGA021` 覆盖，`StreamRendering` 由 `JAZORVCA012` 覆盖；localization 与复杂 validation 已给出应用自有/组件库 typed 替代路径 |
| JS 互操作 | `IJSRuntime` 家族使用 Reject，JavaScript 能力通过 typed ECMAScript/WebIDL binding 表达。 | 已有稳定诊断与作者面回归 |
| 性能与交付 | 以固定 benchmark 比较 render/update、gzip、clean/incremental/HMR/Release。 | 已完成运行时基线；主链路优化以可复现收益为准 |

## P3：Vue 应用生态绑定扩展

P3 的分阶段顺序、每个绑定的固定交付物与验证门槛见 [P3 Vue 应用生态绑定扩展计划](./p3-vue-application-bindings-plan.md)。

| 阶段 | 目标 | 当前状态 |
| --- | --- | --- |
| P3-A | `ECMAScript.DateFns`、`ECMAScript.VueUse`、`ECMAScript.FloatingUi` | 三包均已交付包、测试与主线门禁；browser smoke 与真实 RazorVue consumer 待补 |
| P3-B | `ECMAScript.VeeValidate`、`ECMAScript.VueI18n`、`ECMAScript.VueQuery` | 三包均已交付包、测试与主线门禁；browser smoke 与真实 RazorVue consumer 待补 |
| P3-C | `ECMAScript.VueDraggable`、`ECMAScript.FilePond`、`ECMAScript.WangEditor` | 三包均已交付包、测试与主线门禁；browser smoke 与真实 RazorVue consumer 待补 |
| P3-D | `ECMAScript.Monaco` | 未开始 |

## 稳定边界

P2 的具体执行顺序、Definition of Done 和证据门槛见 [RazorVue P2 执行计划](./p2-plan.md)。

下列边界在本阶段保持稳定，确保整体契约清晰一致。

- 浏览器运行时通过受支持的 C# 语义和显式宿主映射执行。
- Microsoft/Blazor 内置 UI 组件、`IJSRuntime` 字符串互操作和服务器端服务使用各自的运行时模型。
- C# binding 保持强类型契约。
- 公共平台 API 以跨应用契约和验收证据定义。

## 写入当前状态的门槛

计划只有完成证据闭环，才可以成为读者能够依赖的当前能力。

一个计划条目完成后，必须同步更新实现、测试、作者指南和[当前状态](./current-status.md)。写入“当前状态”前，至少满足：

1. C# API 通过正常类型检查和官方 Razor SG 绑定，不依赖私有作者语法。
2. compiler lowering、CLR/runtime 或 binding 行为具有针对性的回归测试。
3. 最终模块在真实浏览器中完成目标交互，Release package consumer 可独立复现。
4. 涉及 SSR、hydration 或宿主状态时，额外证明该 profile 的所有权、失败传播和资源闭包。
5. 相邻形状具有明确 `Guidance` 或 `Reject` 状态与替代路径。
