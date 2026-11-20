# 下一阶段

> 已交付能力以[当前状态](./current-status.md)为准。本页只安排尚需推进的工作，不把已完成的迁移或既有支持切片重复列为待开发能力。

## 投入准则

从真实消费者复现、高频作者写法和明确的行为缺口选择任务。每个切片先定义 C# 契约、责任层和验收，再实现、回归并更新指南；缺少需求或可复现收益时不增加运行时协议。

## 主线投入

| 优先级 | 提案 | 排序依据 | 首个交付门槛 |
| --- | --- | --- | --- |
| **P0：首批切片已交付** | [#4：ILSpy 按需导入普通 C# / NuGet DLL](https://github.com/devhxj/Jazor/issues/4) | 解除普通托管类库只有签名、没有可编译实现的核心采用障碍，直接扩大现有 C# 代码的复用范围；1.4.2 已实现普通 DLL/NuGet 自动导入、成员闭包与共享模块 | 实现资产与 ref/lib 配对、原始/恢复符号对应、静态直接调用的成员闭包；无关不支持成员不阻断；真实 DLL/NuGet 的 JS 与 .NET 结果对照及隔离 consumer 验证 |
| **当前主线** | [#138：Jolt 组件和语义编辑](../03-guides/jolt-authoring.md) | 以顶层标记和 C# 编写 Vue 组件，直接使用项目依赖中的 npm 组件 | 用同一应用验证组件组合、编辑器、浏览器和隔离工具包消费者 |

## 作者体验与交付证据

| 任务 | 责任层 | 完成条件 |
| --- | --- | --- |
| npm 组件组合、插槽和回调 | Jolt.Compiler、Jolt.Npm | 真实组件包的类型检查、渲染和浏览器交互一致 |
| 诊断与修改反馈 | Jolt.LanguageServer、编辑器扩展 | 构建和编辑器定位一致，补全和导航使用同一份组件声明 |
| Debug/HMR/SPA/SSR 一致性 | Emit、ASP.NET Core、NodeHost | 同一资源闭包上的模块、source map、PathBase、浏览器交互和适用 hydration 行为可复现，错误明确传播 |
| 真实应用联调 | 参考应用与消费者 | 补齐真实后端保存、插件启用后的业务流程和修改到可见的 HMR 长尾证据 |
| 可测量优化 | Compiler、Jolt、CLR、Emit | 固定机器、提交、输入、warm-up 和多轮参数；比较耗时、p50/p95、吞吐、原始/gzip 体积与行为 |

复现入口见 [Jolt 作者指南](../03-guides/jolt-authoring.md) 和 [开发与测试](../03-guides/development-and-testing.md)。

## 协议与支持范围扩展

SSR state envelope、认证快照/browser provider、单一显式构造函数的普通引用服务参数，以及 history handler 的已验证子集已经实现。扩展必须在现有协议基础上明确新边界：

| 方向 | 待证明的扩展 | 责任层与验收 |
| --- | --- | --- |
| SSR 状态交接 | 更复杂的请求/组件所有权、过期 payload、一次性副作用与 hydration identity | ASP.NET Core / Emit；独立 SSR/package/browser 场景验证失配、重复入口与失败传播 |
| 认证与应用 bootstrap | 新 endpoint 状态和应用生命周期的组合 | 应用 endpoint / ASP.NET Core；服务端保持授权事实来源，typed consumer 验证匿名、刷新、403、过期、错误和竞态 |
| 构造与 activation | 多构造、继承和更复杂服务生命周期 | Compiler、应用集成；验证 Roslyn 绑定、初始化顺序和 SSR/browser lifetime |
| 复杂 browser history | 超出现有 handler、取消恢复和释放切片的完整历史语义 | CLR / 应用 route host；以真实浏览器和 reference 明确 URL、竞态、用户确认与释放协议 |
| localization 与复杂 validation | 有明确需求的 typed 应用/组件协议 | 对应应用或 binding；当前保持 Guidance，取得独立作者面和运行时证据后再评审 |

## 编译器与普通类库

35 个语法 bug 的运行行为和诊断修复已随 1.3.0 交付。[语法 issue 索引](./csharp-syntax-gap-audit.md)中的开放 enhancement 优先选择真实消费者及 P0 普通类库导入所需的切片，再扩展构造链、参数绑定、源码运算符、接口分派和自定义解构/枚举；不把关闭 bug 等同于实现完整运行时协议。

普通 NuGet DLL 的自动实现恢复已形成[有界交付切片](./ordinary-nuget-compilation-research.md)，对应 [issue #4](https://github.com/devhxj/Jazor/issues/4)。后续按真实库需求扩展初始化、运行时类型和分派协议。

每个新增 lowering 先明确求值顺序、副作用、结果、使用点语义、确定性与失败边界，再由 Compiler / CLR / Analyzer 按职责实现。仅为擦除类型注解增加 blanket rejection 不属于实现路线。

## Vue 应用生态绑定

DateFns、VueUse、FloatingUi、VeeValidate、VueI18n、VueQuery 和 Monaco 已有源码、测试 lane 和 lockstep 发布。各绑定需要补齐浏览器交互、真实包消费者，以及适用的 SSR、worker 和资源验证。交付清单见 [P3 验收计划](./p3-vue-application-bindings-plan.md)。

## Jolt 开发顺序

Jolt 2.3.0 交付了可编辑列表、泛型表格、npm 函数与 composable、本地 Vue 混编和组件包分发，以及应用组合与 SSR 所需的功能。下表保留各批的验收范围。 [目标语法与示例](../02-architecture/jolt-authoring-comparison.md#目标特性与写法) · [已发布能力](./current-status.md#jolt-作者面)

| 顺序 | 交付与主要改动 | 完成条件 |
| --- | --- | --- |
| **1 · 指令、表单与节点身份** | Syntax 区分普通属性、事件、指令及修饰符；Projection、ComponentContract 和 VueLowering 接通 `@key`、文本和 checkbox 的 `@bind`、`:event`、已声明的 npm 命名 model、DOM 事件参数及 `preventDefault` / `stopPropagation`。LSP 复用定点诊断。 | 浏览器验证输入法组合、复选框、父状态回显、Reka UI 双向更新各写回一次；带焦点的一行排序后仍持有原 DOM。错误的 model 类型、只读目标和未实现指令在作者区间报错，产物中没有残留指令属性。 |
| **2 · Razor 模板和本地组件契约** | 在第一批的指令节点上扩展局部代码、`@for` / `@switch`、`@attributes`、模板引用、具名片段、多泛型与共享 imports。投影明确本地 callback prop、Vue emit、model 和 exposed ref 的对外名称。逐项扩展其他表单类型与 `:get` / `:set` / `:after`。 | 一个泛型表格能由父组件提供带类型的行模板；局部变量单次求值，属性合并顺序正确；挂载和卸载得到正确 ref。改错约束、回调或插槽字段，LSP 与 CLI 指向同一处；现有 `Action<T>` 调用方能继续工作。 |
| **3 · npm 普通值与组合式函数** | Jolt.Npm 增加按导出查询的类型投影，Project 将合成声明或已有宿主绑定提供给 Roslyn，VueLowering 提供每实例同步 setup。先接 `es-toolkit/string` 的具名函数，再用 VueUse `useCounter` 验证结构化返回值、ref 和回调。此批的标量导出工作可在第一批之后独立开展。 | 隔离消费者只声明 npm 依赖。公开子路径和默认/具名导出解析正确；生成模块使用原 specifier，错误参数定位到 C#，定义跳到锁文件版本的声明。两个计数器彼此独立；卸载后相关 watcher 停止。升级依赖后类型缓存失效。 |
| **4 · Vue 混编与组件库分发** | 先从本地契约生成可供 TS/SFC 使用的声明和 ESM 包，再让 Vite 与语言工具支持源码 `.vue` ↔ `.jazor`。以第二批的模型、slot 和外部名称契约为前置条件。接入已有 Vue 插件与 scoped CSS 链路，增量失效覆盖依赖者和样式。 | 同一 Vite 应用里，SFC 用 `v-model` 调用 Jolt，Jolt 调用本地 SFC，Vue TSX 消费同一组件声明。两端各有负例；模板和 CSS 热更后输入值保留。新建一个普通 Vue 项目安装构建出的库，能类型检查、生产打包并在浏览器交互。 |
| **5 · Vue 应用组合与 SSR** | 以上一批的混合应用为消费者，先验收 Teleport、Transition、动态组件、Router、Pinia 和 typed provide/inject，再接 `@page` / `@layout` 等可选适配与 SSR。Vite/NodeHost 负责两端构建，应用宿主负责每请求的 app、router、store 和初始状态。 | 同一路由直达和客户端导航都得到同一视图。两个不同请求各自渲染和回填状态；hydration 无失配，挂载后一次交互触发一次回调；服务端预取错误可见。Teleport、scoped CSS 和卸载清理分别覆盖浏览器与适用的 SSR 情况。 |

### 已交付能力与下一步

可编辑列表验证 `@key`、输入法组合、DOM 事件和 npm 命名模型。泛型表格通过父级行模板验证 `@for`、模式分支、单次求值和键控重排，编辑表单使用本地模型、引用和属性展开。同一批的编译回归还覆盖了类型化内联片段、嵌套插槽类型转发以及错误定位。 [作者指南](../03-guides/jolt-authoring.md#泛型表格内联模板与目录导入) · [可运行的表格](../../samples/Jolt.Counter/.fixtures/TableDemo.jazor) · [浏览器与 HMR](../../samples/Jolt.Counter/README.md) · [安装包验收](../../scripts/csharp/verify-jolt-package.cs)

第三批接通 `es-toolkit/string` 的 `camelCase` 和 `@vueuse/core` 的 `useCounter`。Roslyn 检查调用、回调和 ref 的读写权限，生成模块保留原始导出和公开子路径。浏览器以两个组件实例验证更新、卸载清理与重新挂载；编辑器在依赖升级后重新定位声明。元数据范围和可运行的调用示例见[作者指南](../03-guides/jolt-authoring.md#直接调用-npm-函数)和[npm 声明验证](../../src/Jolt.Npm/verify.cs)。

第四批的 `jolt library` 生成包根和组件子路径、ESM、类型声明与 CSS。独立 Vue 工程从包入口导入组件，在 SFC 和 TSX 中检查参数、model、泛型插槽和公开实例方法。混编样例沿 `.vue → .jazor → .vue` 更新同一字符串，并在两边的模板和样式热更时保留输入框的草稿与焦点。 [组件包验收](../../scripts/csharp/verify-jolt-library.cs) · [混编验收](../../scripts/csharp/verify-jolt-mixed.cs)

第五批使用 Vue Router、Pinia 和 ASP.NET Core 运行同一个混合应用。每个请求创建 app、router 和 store，预取后的状态与 Teleport 片段随 HTML 返回。Chromium 检查 DOM 复用、首次点击、预取次数、导航和退出清理。可选的 `@page` / `@layout` 生成客户端与服务端共用的路由记录。 [应用样例](../../samples/Jolt.Mixed.Ssr/README.md) · [端到端验收](../../scripts/csharp/verify-jolt-ssr.cs)

混编样例已测量交替保存 `.vue` 和 `.jazor` 到浏览器可见的耗时，也测量了编辑 SFC prop 到 Jolt 调用方收到、清除诊断的耗时。SFC 改完文案后，Vue 会先完成热更新，随后的 Jolt 保存可能还在等待前一次契约检查。工作区在 Vue 解析器确认仅有文字变化、公开定义位置仍有效时复用检查结果。两批各 20 轮的交替保存中，Jolt 保存到 DOM 更新的 p50 从 2.33–2.85 秒降至 305–314 毫秒，Vue 保存为 9.3–9.5 毫秒。真正的 prop 类型变化仍走 TypeScript 检查。 [混编编辑反馈与原始样本](../../samples/Jolt.Mixed/EditFeedback-baseline.md)

下一步按两个已测到的瓶颈推进。

1. **千行列表的首部插入。** 当前 Jolt 的插入 p50 约为同批 SFC 的三倍。Vue block / patch flag 原型保留空白和行身份，两批插入 p50 分别从 2.6 ms 降到 1.7 和 1.8 ms。先在 Jolt.Compiler 中为子节点形状固定的原生行定义可静态判定的条件，再验证 C# 求值次数和顺序、动态片段、ref、空文本、SSR 与 hydration。候选产物、旧版和 SFC 须同批复测插入和倒序。直接生成字符串子节点的另一个实验会改变 Text 节点身份，已撤回。 [千行列表对照](../../samples/Jolt.Counter/KeyedRows-baseline.md)
2. **真实 Vue 契约编辑。** 修改或修复 SFC 的 prop 类型仍需要约 1.3 秒才能在 Jolt 调用方看到诊断更新。下一轮先分别测量 TypeScript program 创建、类型查询和 Roslyn 重分析，再按占比评估能否为同一项目复用增量 program。验收覆盖未保存缓冲区、独立 `.d.ts` 变更、关闭文档恢复磁盘版本，以及已打开调用方诊断的源码范围。 [混编编辑反馈](../../samples/Jolt.Mixed/EditFeedback-baseline.md)

SSR 的生产基线分别记录了回环响应、Vue 接管服务端 DOM 和首次点击，用于复核渲染改动。 [SSR 基线](../../samples/Jolt.Mixed.Ssr/README.md#ssr-benchmark)

### 性能与开发反馈的验收

易用性用真实修改来检验。在编辑器输入 npm 组件名后能补全必填 prop，在 slot 中能补全行类型，在错误的 model 上能跳到定义。再从一个空白 Vue 项目安装 Jolt 组件库，记录完成上述操作需要手写的类型声明、按包包装的组件和修改的配置。对已经支持的公开导出，目标是零手写声明和零包装组件。

从第一批起保留两份相同行为的页面，用同一 Vue/Vite 锁定版本、相同 CSS 和依赖、相同浏览器与机器测量。生产测量关闭 HMR；开发测量明确区分首次依赖准备与热缓存。在同一批次交替运行 Jolt 与 SFC，预热后重复采样，报告样本数、p50、p95 和两者比值。先记录多轮基线的波动，再用该波动判断回退和可重复的收益。

| 场景 | 指标及语义检查 | 可尝试的优化 |
| --- | --- | --- |
| 生产列表含 1,000 个带 key 的行，更新单行、插入、倒序 | 首次挂载、触发更新到 DOM 提交的耗时、实际更新的行数和原始/gzip JS 大小。先断言焦点、顺序、事件次数和 DOM 结果一致。 | 先消除每次 render 重建的纯静态 props 和与循环无关的处理器，再测每实例静态子树缓存。仅当能证明动态边界且锁定 Vue 版本有契约测试时采用 block / patch flag。动态展开及捕获行变量的处理器保留完整更新语义。 |
| 混合工程中改一个叶组件或 npm 声明 | 记录冷构建、热重建、写盘到浏览器可见、键入到本地/依赖诊断可见的耗时，以及重新分析的组件数和缓存命中数。 | 按源码、公开契约和样式分别失效；只改实现时重建该组件，契约变化时重检依赖者。修复模板后保留输入和焦点，改实例结构时覆盖重新挂载。 |
| 同一路由首屏 SSR 和 hydration | 分开记录服务器渲染、客户端 hydration 及首个有效交互耗时，检查双请求状态、失配和副作用次数。 | 先消除重复取数和无效模块载入，再评估更细的 SSR 发射。模块级仅共享不可变的静态数据。 |

现有 `test-dotnet.cs -- --project jolt` 验证语法和编译，npm、双向类型检查、LSP、浏览器、SSR 及安装包各有独立入口。性能对照需分别保留机器与提交信息、样本数和分布。 [Vue 渲染机制](https://vuejs.org/guide/extras/rendering-mechanism.html) · [Jolt #138](https://github.com/devhxj/Jazor/issues/138)

## 写入当前状态的门槛

1. C# API 通过正常类型检查，组件绑定通过 Jolt 编译。
2. compiler lowering、CLR/runtime 或 binding 有针对性的行为回归与相邻失败诊断。
3. 隔离 Release package consumer 与真实浏览器完成目标交互。
4. 涉及 SSR、hydration、生命周期或宿主状态时，证明对应 profile 的所有权和错误传播。
5. 同步架构、指南和适用发布说明，使用正式门禁保留提交与环境对应的报告。

已完成计划的过程和旧测试快照只保留在[历史演进](../05-history/evolution.md)，详细原始记录通过 Git 历史追溯。
