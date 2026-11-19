# Vue 应用生态绑定扩展计划

> 本计划定义 Jazor 七个已发布应用级绑定进入 Support 所需的后续消费者验收。

## 目标

提供可组合的强类型绑定，覆盖响应式工具、表单、国际化、日期、代码编辑器、服务端状态和浮动定位。

每个交付物都是独立的 `ECMAScript.<Name>` binding，拥有自己的 C# contract、npm/JSR identity、许可证、README、版本快照和测试项目。运行时模块与样式由上游 package 提供，编译器保持通用 lowering 语义。

## 当前绑定与上游选型

以下绑定已有源码、测试 lane 和 lockstep 发布。当前上游版本以各包 manifest/README 为准；后续调整需说明 Vue 兼容性、类型、维护、许可和映射成本，并同步迁移与消费者证据。

| 交付物 | C# 包 | 上游 npm 包 | 验收范围 |
| --- | --- | --- | --- |
| 响应式与浏览器工具 | `ECMAScript.VueUse` | `@vueuse/core` | 常用 composable、ref/computed 投影、浏览器事件、媒体查询、存储、剪贴板、网络状态 |
| 表单验证 | `ECMAScript.VeeValidate` | `vee-validate` | `useForm`、`useField`、字段状态、提交状态、错误消息、schema/规则适配 |
| 国际化 | `ECMAScript.VueI18n` | `vue-i18n` | `createI18n`、`useI18n`、消息查找、locale 切换、日期/数字格式化入口 |
| 日期时间 | `ECMAScript.DateFns` | `date-fns` | 解析、格式化、加减、比较、区间和 locale；保持纯函数导入 |
| 代码编辑器 | `ECMAScript.Monaco` | `monaco-editor` | editor/model、language、theme、markers、布局、生命周期与变更事件 |
| 服务端状态 | `ECMAScript.VueQuery` | `@tanstack/vue-query` | `VueQueryPlugin`、query/mutation、query key、缓存、失效、分页和加载/错误状态 |
| 浮动定位 | `ECMAScript.FloatingUi` | `@floating-ui/vue`（必要时补 `@floating-ui/dom`） | `useFloating`、middleware、placement、autoUpdate、箭头、工具提示/弹层定位 |

Monaco 直接绑定核心 `monaco-editor`。Floating UI 以 `@floating-ui/vue` 为作者入口，DOM 层函数按实际 authoring contract 补充。

## 共同边界

- 公开 C# API 使用具体类型、重载、命名 union 和显式 callback；不使用 `object`/`dynamic` 作为万能参数。
- Vue ref、computed、事件和组件 props 通过现有 `ECMAScript.Vue` 投影；第三方库自身的响应式语义不复制到 Jazor runtime。
- 每个库按需导入公开 ESM entry；`SemanticWalker` 负责表达式和成员翻译，`Jazor.Emit` 生成标准项目，进程内 Vite API 负责 bundle 与资源图。
- SSR 不可用的浏览器 API 必须在 contract 中标明生命周期和使用边界；不能静默退化为服务器实现。
- 上游包名、版本、export/component、prop、event、slot、文档来源、许可证和资源 hash 都写入 manifest/快照，并接受 contract drift 检查。

## 分阶段顺序

组件类绑定的策略见[npm/JSR 包绑定指南](../03-guides/js-resource-binding.md#c-契约)：组件数少时使用手写代理与 descriptor，组件数多且上游有机器可读 metadata 时生成代理与导出目录。两种形式都映射到上游公开 ESM specifier。

### P3-A：基础工具与确定性纯函数

先实现 `ECMAScript.DateFns`、`ECMAScript.VueUse` 和 `ECMAScript.FloatingUi`。date-fns 的纯函数导入最容易验证；VueUse 建立 composable/ref 约定；Floating UI 为 Tooltip、Popover、菜单和编辑器浮层提供共享定位能力。

退出条件：三个包可独立 pack，标准 package identity 与 ESM 入口可解析，编译器测试通过，每个包的独立消费者通过真实浏览器验证。

### P3-B：表单、国际化与服务端状态

实现 `ECMAScript.VeeValidate`、`ECMAScript.VueI18n` 和 `ECMAScript.VueQuery`。表单验证只负责浏览器侧字段/提交状态，服务端验证仍由 endpoint contract 负责；i18n 只负责 Vue locale/message runtime，不取代 ASP.NET request culture；Vue Query 只负责客户端 server-state，不取代 Pinia 的本地领域状态。

退出条件：覆盖校验成功/失败、locale 切换、query loading/error/cache invalidation/mutation 竞态，并有 Release package consumer 证据。

### P3-D：重量级编辑器

`ECMAScript.Monaco` 的核心 editor/model/language/marker 生命周期已经实现，worker URL 和 bundler 资源通过 manifest 明确声明。diff editor、completion、语义 token 和大文件策略仍属于后续扩展；该绑定已进入默认发布集合，真实浏览器和独立包消费者验证仍待补。

退出条件：编辑器可在独立 package consumer 中创建、更新、销毁；model 变更、language、theme、marker 和 layout 的事件/调用顺序有浏览器证据；SSR 明确拒绝直接创建浏览器 editor。

## 每个绑定的固定交付物

1. `src/ECMAScript.<Name>` 独立 net11.0 packable 项目，并加入 `Jazor.slnx`。
2. C# 类型契约、delegate、union、组件 proxy 和 `[ECMAScript]` 映射；命名空间不跨包复用。
3. 上游 runtime 的锁定版本、`manifest.json` schema 2、npm/JSR identity、公开入口、许可证和 `README.md`。
4. 生成或手工维护的 export/prop/event/slot inventory、文档来源和 contract fingerprint。
5. 对应 `src/ECMAScript.<Name>.Test`：metadata、compiler emission、标准项目/Vite 输出和适用的 browser smoke。
6. 真实消费者样例，证明安装包和源码引用得到相同 module/import 结果。
7. 作者指南、平台绑定表、current status 和 CHANGELOG 的同步条目。

## 验证门槛

每个包在声明 Support 前必须通过：

- C# API 能在独立消费者项目中编译，且错误输入产生稳定诊断；
- compiler 输出的 import、alias、临时变量、source origin 和 source map 稳定；
- Debug/Release/SSR（适用时）使用同一 `jazor/` 项目、`node_modules` 与 frozen lock；
- NodeApi runtime 测试覆盖正常、空值、错误、取消、重复挂载或竞态路径；
- 真实 Chromium browser smoke 覆盖至少一个用户可观察流程；
- 上游版本、文档和 contract fingerprint 与仓库 baseline 一致；漂移默认阻止发布；
- 适用的 compiler、Emit、Vue binding coverage 和 package consumer 门禁通过。

## Definition of Done

每个包的完成条件包括：独立包和测试项目进入 solution；提供锁定版本、manifest、许可证、README 和 inventory；通过编译器、Emit 回归及真实浏览器和独立包消费者验证；同步公开边界、SSR 约束与已知限制。

七个绑定已进入默认发布集合，当前保持 Guidance，等待浏览器和独立包消费者验收。

旧执行表、单轮测试计数和首次发布背景见[历史演进](../05-history/evolution.md#2026-09-28-api-与发布接线复核)。当前升级支持状态的依据是上述逐绑定验收，不是发布或生成成功。
