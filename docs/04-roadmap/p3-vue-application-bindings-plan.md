# Vue 应用生态绑定扩展计划

> 本计划定义 Jazor 在现有 Vue、Vue Router、Pinia、UI、图表和图标绑定之上的下一批应用级生态绑定。计划先冻结边界和证据要求，再逐个实现；未通过对应测试、资源闭包和真实消费者验证的条目保持 Guidance，不进入当前 Support 矩阵。

## 目标

为 RazorVue 应用提供一组可组合的、强类型的 Vue 应用基础库绑定，覆盖响应式工具、表单、国际化、日期、拖拽、编辑器、文件上传、服务端状态和浮动定位。

每个交付物都是独立的 `ECMAScript.<Name>` JS resource library，拥有自己的 C# contract、`manifest.json`、`dist/`、许可证、README、版本快照和测试项目。绑定只描述上游库的公开运行时能力；不会把第三方库的内部状态改写为 Jazor compiler 特判。

## 当前候选选型

以下是首选上游库。实现前允许根据 Vue 3 兼容性、TypeScript 类型质量、维护活跃度、资源许可和 Jazor 映射成本调整，但每次调整必须在本计划和对应 CHANGELOG 中记录原因。

| 交付物 | C# 包建议 | 上游 npm 包 | 首期范围 |
| --- | --- | --- | --- |
| 响应式与浏览器工具 | `ECMAScript.VueUse` | `@vueuse/core` | 常用 composable、ref/computed 投影、浏览器事件、媒体查询、存储、剪贴板、网络状态 |
| 表单验证 | `ECMAScript.VeeValidate` | `vee-validate` | `useForm`、`useField`、字段状态、提交状态、错误消息、schema/规则适配 |
| 国际化 | `ECMAScript.VueI18n` | `vue-i18n` | `createI18n`、`useI18n`、消息查找、locale 切换、日期/数字格式化入口 |
| 日期时间 | `ECMAScript.DateFns` | `date-fns` | 解析、格式化、加减、比较、区间和 locale；保持纯函数导入 |
| 拖拽排序 | `ECMAScript.VueDraggable` | `vue-draggable-plus` | `VueDraggable`、列表绑定、拖拽事件、group/sort/filter 配置 |
| 代码编辑器 | `ECMAScript.Monaco` | `monaco-editor` | editor/model、language、theme、markers、布局、生命周期与变更事件 |
| 富文本编辑器 | `ECMAScript.WangEditor` | `@wangeditor/editor-for-vue`（含 `@wangeditor/editor`） | Vue editor 组件、HTML/value、toolbar/config、change/focus/blur、destroy |
| 文件上传 | `ECMAScript.FilePond` | `vue-filepond`（含 `filepond`） | FilePond Vue 组件、server/process、files、上传进度、成功/失败、移除与取消 |
| 服务端状态 | `ECMAScript.VueQuery` | `@tanstack/vue-query` | `VueQueryPlugin`、query/mutation、query key、缓存、失效、分页和加载/错误状态 |
| 浮动定位 | `ECMAScript.FloatingUi` | `@floating-ui/vue`（必要时补 `@floating-ui/dom`） | `useFloating`、middleware、placement、autoUpdate、箭头、工具提示/弹层定位 |

Monaco 与 WangEditor 必须是两个独立包。Monaco 默认直接绑定核心 `monaco-editor`，不预先依赖第三方 Vue wrapper；WangEditor 使用官方 Vue 适配器，并将编辑器核心作为同一资源闭包的显式依赖。Floating UI 以 `@floating-ui/vue` 为作者入口，DOM 层函数只在 Vue 适配器无法表达时补充。

## 共同边界

- 公开 C# API 使用具体类型、重载、命名 union 和显式 callback；不使用 `object`/`dynamic` 作为万能参数。
- Vue ref、computed、事件和组件 props 通过现有 `ECMAScript.Vue`/`ECMAScript.VueContract` 投影；第三方库自身的响应式语义不复制到 Jazor runtime。
- 每个库按需导入独立 ESM entry；`SemanticWalker` 负责表达式和成员翻译，`Jazor.Emit` 负责资源闭包、manifest、bundle 和 source map。
- SSR 不可用的浏览器 API 必须在 contract 中标明生命周期和使用边界；不能静默退化为服务器实现。
- 上游包名、版本、export/component、prop、event、slot、文档来源、许可证和资源 hash 都写入 manifest/快照，并接受 contract drift 检查。

## 分阶段顺序

组件类绑定的策略见[JS 资源库绑定指南](../03-guides/js-resource-binding.md#组件绑定)：组件数少时用手写双表示（`ECMAScript.VueRoute` 的 `RouterLink`/`RouterView` 范式，Razor 代理 + `IVueComponent<TProps, TSlots>` 描述符），组件数多且上游有机器可读 metadata 时用全量代理 + 生成描述符（`ECMAScript.Vuetify` 范式，生成 `VuetifyCatalog.g.cs` 导出目录与 `dist/components.mjs` shim）。P3-A 三包均为函数/composable，无组件代理；P3-C 的 `VueDraggable`、`FilePond`、`WangEditor` 组件数少，按手写双表示落地；P3-D 的 Monaco 是指令式 editor API 而非 Vue 组件，按函数/Hook 绑定。

### P3-A：基础工具与确定性纯函数

先实现 `ECMAScript.DateFns`、`ECMAScript.VueUse` 和 `ECMAScript.FloatingUi`。date-fns 的纯函数导入最容易验证；VueUse 建立 composable/ref 约定；Floating UI 为 Tooltip、Popover、菜单和编辑器浮层提供共享定位能力。

退出条件：三个包可独立 pack，manifest 资源闭包可解析，compiler/SG 测试通过，至少一个真实 RazorVue 页面和浏览器 smoke 使用每个包。

### P3-B：表单、国际化与服务端状态

实现 `ECMAScript.VeeValidate`、`ECMAScript.VueI18n` 和 `ECMAScript.VueQuery`。表单验证只负责浏览器侧字段/提交状态，服务端验证仍由 endpoint contract 负责；i18n 只负责 Vue locale/message runtime，不取代 ASP.NET request culture；Vue Query 只负责客户端 server-state，不取代 Pinia 的本地领域状态。

退出条件：覆盖校验成功/失败、locale 切换、query loading/error/cache invalidation/mutation 竞态，并有 Release package consumer 证据。

### P3-C：交互组件与媒体资源

实现 `ECMAScript.VueDraggable`、`ECMAScript.FilePond` 和 `ECMAScript.WangEditor`。这些库包含 DOM 生命周期、事件序列和资源上传/编辑状态，必须先固定 dispose、取消、错误和 SSR 边界，再扩充选项。

退出条件：拖拽排序保持 key 和列表顺序；上传覆盖进度、取消、失败和移除；富文本覆盖 HTML 双向更新、焦点事件、销毁和重复挂载。

### P3-D：重量级编辑器

最后实现 `ECMAScript.Monaco`。先交付核心 editor/model/language/marker 生命周期，再评估 worker、diff editor、completion、语义 token 和大文件策略；worker URL 和 bundler 资源必须通过 manifest 明确声明。

退出条件：编辑器可在独立 package consumer 中创建、更新、销毁；model 变更、language、theme、marker 和 layout 的事件/调用顺序有浏览器证据；SSR 明确拒绝直接创建浏览器 editor。

## 每个绑定的固定交付物

1. `src/ECMAScript.<Name>` 独立 net11.0 packable 项目，并加入 `Jazor.slnx`。
2. C# 类型契约、delegate、union、组件 proxy 和 `[ECMAScript]` 映射；命名空间不跨包复用。
3. 上游 runtime 的锁定版本、`manifest.json` schema 2、`dist/` 资源、许可证和 `README.md`。
4. 生成或手工维护的 export/prop/event/slot inventory、文档来源和 contract fingerprint。
5. 对应 `src/ECMAScript.<Name>.Test`：metadata/manifest、compiler emission、Razor SG、Emit/resource closure 和适用的 browser smoke。
6. 真实消费者样例，证明安装包和源码引用得到相同 module/import 结果。
7. 作者指南、平台绑定表、current status 和 CHANGELOG 的同步条目。

## 验证门槛

每个包在声明 Support 前必须通过：

- C# API 能在官方 Razor Source Generator 下编译，且错误输入产生稳定诊断；
- compiler 输出的 import、alias、临时变量、source origin 和 source map 稳定；
- Debug/Release/SSR（适用时）资源闭包只包含 manifest 声明的模块、样式、worker 和许可证；
- Deno/runtime 测试覆盖正常、空值、错误、取消、重复挂载或竞态路径；
- 真实 Chromium browser smoke 覆盖至少一个用户可观察流程；
- 上游版本、文档和 contract fingerprint 与仓库 baseline 一致；漂移默认阻止发布；
- 适用的 compiler、Razor SG、Emit、Vue binding coverage 和 package consumer 门禁通过。

## 非目标

本计划不实现新的 axios/fetch 通用库、不把所有第三方库合并成一个聚合包、不复制第三方完整内部类型、不提供任意 JavaScript 字符串互操作，也不宣称这些浏览器库天然具备 SSR 等价行为。TanStack Query 不替代 Pinia；VeeValidate 不替代服务端模型验证；WangEditor 不替代 Monaco；Floating UI 不替代 UI 组件库自身的视觉组件。

## Definition of Done

本计划只有在十个包均满足以下条件后才算完成：独立包和测试项目已加入 solution；每个包都有锁定版本、manifest、许可证、README、inventory/fingerprint；核心 C# contract、compiler/SG/Emit 回归和真实 browser/package consumer 证据齐全；公共边界、SSR 约束、选型变更和已知限制已同步到架构文档、作者指南、current status 与 CHANGELOG；全量质量门禁通过。

计划状态：**进行中（P3-A 已启动）**。已存在的 TDesign、Element Plus、Vuetify、图表、图标、Pinia、Vue Router 和 Fetch binding 不计入本计划交付物，也不因本计划改变其公共 API。

执行记录：

| 日期 | 条目 | 状态 | 证据 |
| --- | --- | --- | --- |
| 2026-09-17 | `ECMAScript.DateFns`（date-fns 4.4.0） | 包、测试与门禁已交付；browser smoke 与真实 RazorVue consumer 待补 | `src/ECMAScript.DateFns`（manifest schema 2、367 模块闭包、inventory fingerprint、MIT 许可证）、`ECMAScript.DateFns.Test` 16/16、`Jazor.EmitTest.Materialize_DateFnsClosure_IsSelfContainedAndResolvesBothEntries`、`test-dotnet.cs --project date-fns` |
| 2026-09-17 | `ECMAScript.VueUse`（VueUse 15.0.0） | 包、测试与门禁已交付；browser smoke 与真实 RazorVue consumer 待补 | `src/ECMAScript.VueUse`（`@vueuse/core` + `@vueuse/shared` 闭包、69 策展 composable、MIT 许可证）、`ECMAScript.VueUse.Test` 16/16、`Jazor.EmitTest.Materialize_VueUseClosure_ResolvesSharedFromTheCoreEntry`、`test-dotnet.cs --project vueuse` |
| 2026-09-17 | `ECMAScript.FloatingUi`（`@floating-ui/vue` 2.0.1） | 包、测试与门禁已交付；browser smoke 与真实 RazorVue consumer 待补 | `src/ECMAScript.FloatingUi`（4 包跨包闭包 + 分包版本、`useFloating` 与 9 个中间件、MIT 许可证）、`ECMAScript.FloatingUi.Test` 13/13、`Jazor.EmitTest.Materialize_FloatingUiClosure_ResolvesSiblingPackagesFromTheAuthorEntry`、`test-dotnet.cs --project floating-ui` |
| 2026-09-17 | `ECMAScript.VeeValidate`（vee-validate 4.15.1） | 包、测试与门禁已交付；browser smoke 与真实 RazorVue consumer 待补 | `src/ECMAScript.VeeValidate`（单模块闭包、泛型表单/字段返回、MIT）、`ECMAScript.VeeValidate.Test` 14/14、`test-dotnet.cs --project vee-validate` |
| 2026-09-17 | `ECMAScript.VueI18n`（vue-i18n 11.4.12） | 包、测试与门禁已交付；browser smoke 与真实 RazorVue consumer 待补 | `src/ECMAScript.VueI18n`（`@intlify/*` 三包闭包、Composer 表面、MIT）、`ECMAScript.VueI18n.Test` 8/8、`test-dotnet.cs --project vue-i18n` |
| 2026-09-17 | `ECMAScript.VueQuery`（`@tanstack/vue-query` 5.103.1） | 包、测试与门禁已交付；browser smoke 与真实 RazorVue consumer 待补 | `src/ECMAScript.VueQuery`（46 模块闭包含 `vue-demi`、query/mutation 返回、MIT）、`ECMAScript.VueQuery.Test` 9/9、`Jazor.EmitTest.Materialize_P3BClosures_ResolveTheirEntryGraphs`、`test-dotnet.cs --project vue-query` |

未通过对应测试、资源闭包和真实消费者验证的条目保持 Guidance，不进入当前 Support 矩阵。
