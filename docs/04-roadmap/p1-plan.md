# RazorVue P1 执行计划

> P1 处理状态所有权、SSR 生命周期、组件激活和浏览器 history 能力。本文固定协议、责任层和验收顺序，再逐项实现；证据闭环前的条目使用 Guidance/Reject 状态。

## 目标

P1 的目标是让 RazorVue 在既有 direct render 和 typed component contract 之上，拥有可版本化、可诊断、可回归的跨请求与跨生命周期协议：

```text
typed authoring
  -> versioned host contract
  -> official Razor SG + compiler lowering
  -> browser/SSR consumer evidence
  -> explicit Support or explicit Reject
```

P1 以版本化状态和生命周期协议为目标；JavaScript 语义通过 typed binding，CLR 服务通过显式宿主契约，组件通过既有组件协议进入模块。

## 工作线与顺序

| 顺序 | 工作线 | 首要责任层 | 本阶段结果 |
| --- | --- | --- | --- |
| P1-A | SSR bootstrap 与状态交接 | `Jazor.AspNetCore`、`Jazor.Emit` | 版本化 state envelope、schema/version 校验、错误传播和重复 hydration 证据 |
| P1-B | 强类型认证状态 | `Jazor.AspNetCore`、应用 endpoint | typed browser provider、匿名/登录/过期/登出/403 状态模型和 SSR 首屏契约 |
| P1-C | 构造函数注入与复杂 activation | `Jazor.RazorVue`、`Jazor.Compiler` | 有界的参数化 activation；保持 base/derived、字段初始化和生命周期顺序 |
| P1-D | 后退/前进与复杂 URI 状态 | `Jazor.CLR`、路由 host | URL 恢复、竞态、注册释放和用户确认协议；`popstate`/`hashchange` 使用独立事件模型 |

顺序固定为 A → B → C → D。B/C/D 以 A 的版本化状态和诊断传播契约为基础。

## P1-A：SSR bootstrap 与状态交接

当前已交付的协议基础：`JazorSsrStateEnvelope` 固定 `jazor-ssr-state` schema 和 v1 版本；`JazorSsrRequest` 保持 props/providers 强类型入口，并可携带显式 `JazorAuthenticationState` 快照。服务器 runner 与浏览器 hydration 对错误 schema、版本、provider 数组或空 provider key 提供显式诊断。该切片定义版本化 state envelope。

### 协议

- 浏览器和 SSR 共享一个 `jazor-ssr-state` envelope，字段为 `schema`、`version`、`props` 和 `providers`。
- `schema` 与 `version` 在 runner 和 hydration 入口同时校验；未知版本、缺失字段、错误 JSON 和 provider 形状必须显式失败。
- `JazorSsrRequest` 继续提供 `object? Props` 与强类型 `IReadOnlyList<JazorSsrProvider>` 作者入口；envelope 是宿主传输协议，不要求页面作者手写 JSON。
- SSR、反序列化和 hydration 失配通过显式失败状态传播；旧 artifact、HTML 与 CSR 各自遵循当前请求的产物协议。

### 验收

1. 官方 Razor SG、Deno runner、TestServer 和真实浏览器都能读取同一 envelope。
2. props/provider 值在 SSR 与 hydration 中保持一致，且 HTML 中不存在未版本化的第二份状态载体。
3. 重复 hydration、过期/错误 envelope 和 render-hook 异常都有可观察失败。
4. Release package consumer 能在隔离资源闭包下复现上述行为。

## P1-B：强类型认证状态

当前已交付协议：`JazorAuthenticationState` 是 host-owned closed snapshot；`JazorAuthenticationEnvelope` 固定 `jazor-auth-state` v1 endpoint 结果；显式 browser provider 覆盖登录、刷新、过期、403 和登出，异常保持可观察，状态按最新请求更新。

先定义应用可持有的 closed contract，例如 `Anonymous`、`Authenticated`、`Expired` 和 `Forbidden`，由 endpoint 返回版本化 envelope；组件通过 typed browser provider 消费该状态。服务端 endpoint 是授权事实来源；请求、Identity 和 token storage 由服务端宿主处理。

必须覆盖匿名、登录、刷新、过期、登出、403、SSR 首屏和 hydration；当前 provider 仍不注册隐式 `AuthenticationStateProvider`，也不提供 `AuthorizeView`。

## P1-C：构造函数注入与复杂 activation

只考虑可静态解析的强类型参数化 activation，并要求：

- selector 由 Roslyn 绑定符号提供；
- base constructor、字段初始化、派生 constructor 和 lifecycle 顺序在 browser/SSR 一致；
- 单一显式构造函数的普通引用类型服务参数通过既有 `jazor:service:<type>` provider key 解析；缺失 provider 必须在 setup 激活处失败；
- 外部 base type、`this(...)`、`ref/out/in/params` 驱动的 dispatch 和 module closure 条件不足的类型使用 Reject；
- 至少两个独立 consumer 和完整 activation 矩阵通过后，才能更新作者指南为 Support。

## P1-D：后退/前进与复杂 URI 状态

`NavigateTo` 的同源内部取消子集与浏览器 `popstate`/`hashchange` 分开建模；history 事件执行 handlers，取消时通过 `replaceState` 恢复已接受位置，后续事件 supersede 旧 dispatch，dispose 完成后停止通知。

## 诊断与证据

每个 P1 协议错误必须包含稳定 ID、`.razor`/`.razor.cs` 源位置、协议版本、失败阶段和最小替代写法。测试至少分为：

- compiler/lowering：求值顺序、单次副作用和导入稳定性；
- official Razor SG：正常绑定与作者错误；
- host/runtime：协议校验、生命周期和错误传播；
- isolated package consumer：Debug、Release、SSR、PathBase 和资源闭包；
- real browser：hydration、导航竞态和用户可观察状态。

## Definition of Done

P1 只有在以下条件全部满足时才可标记完成：

1. P1-A envelope 已在 SSR runner、hydration 和 package consumer 中通过 schema/version、错误和重复执行测试。
2. P1-B 认证状态拥有 typed C# contract、版本化 endpoint envelope 以及完整 browser/SSR 矩阵。
3. P1-C activation 拥有两个真实 consumer、构造器/生命周期矩阵和 selector 稳定性回归。
4. P1-D history 拥有 reference oracle、竞态/释放/恢复测试和真实浏览器证据。
5. 作者指南、当前状态、诊断矩阵、CHANGELOG 与实现同步；相邻形状保持明确 Guidance/Reject 状态。
6. Compiler、CLR、Razor SG、Emit、coverage、SPA、SSR、HMR 和新增 consumer 门禁全部通过。

P1-B/C/D 达到上述门槛后进入当前 Support 矩阵；当前状态记录 P1-A 已交付协议基础。
