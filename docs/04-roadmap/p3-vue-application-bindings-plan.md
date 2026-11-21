# Vue 应用生态 npm 投影验收计划

## 目标

Vue、Vue Router、Pinia、Pinia Testing 和 Vue Devtools 的手写绑定长期维护并随 SDK 发布。其他 npm 应用库由 Jolt 从锁定版本的公开 TypeScript 声明推导 C# 类型，并按原包的公开入口生成 ESM 导入。以下七个应用级 NuGet 包移出后续默认发布清单。它们的完整手写 API、类型定义、调用样例和测试作为投影器的参考实现。开发目标是让首次接入的 npm 包遇到相同声明结构时，也能自动得到同等质量的 C# 调用体验。[平台与绑定](../02-architecture/platform-and-bindings.md)

## 现有绑定与业务用例

以下绑定已有源码、测试和 lockstep 发布记录。现有绑定使用的上游版本见各包的 manifest 或 README。迁移样例在自己的 npm 锁文件中声明版本，使用该版本的公开声明检验调用。

| 现有 C# 包 | 上游 npm 包 | 自动投影验收用例 |
| --- | --- | --- |
| `ECMAScript.DateFns` | `date-fns` | 解析、格式化、加减、比较、区间、集合和 locale。把日期与区间结果继续传给其他导出。 |
| `ECMAScript.VueUse` | `@vueuse/core` | composable、ref/computed 读写、浏览器事件、媒体查询、存储、剪贴板和网络状态。验证卸载后的清理。 |
| `ECMAScript.FloatingUi` | `@floating-ui/vue`、`@floating-ui/dom` | `useFloating`、middleware、placement、`autoUpdate` 和箭头定位。把异步坐标用于真实弹层。 |
| `ECMAScript.VeeValidate` | `vee-validate` | `useForm`、`useField`、schema/规则、字段状态、错误消息及提交成功和失败。 |
| `ECMAScript.VueI18n` | `vue-i18n` | `createI18n`、`useI18n`、消息查找、locale 切换和日期/数字格式化。 |
| `ECMAScript.VueQuery` | `@tanstack/vue-query` | `VueQueryPlugin`、query/mutation、query key、分页、缓存、失效、加载和错误状态。验证失效后重取及相关竞态。 |
| `ECMAScript.Monaco` | `monaco-editor` | 浏览器中创建编辑器与模型，读取和更新内容，处理事件，切换语言与主题，设置标记，执行布局并释放资源。 |

对照脚本列出七库手写 API 的重载、公开类型成员及源码位置，匹配锁定版本的导出。它用 Roslyn 编译合并后的 C# 投影，再把公开投影编译成 DLL，让同一组消费代码仅凭程序集元数据重新绑定。报告列出实际选中的方法、属性、枚举成员和剩余未使用成员。一个导出只要有一种可生成的调用形式就计入顶层统计。运行结果由各库的消费夹具单独验证。 [七库对照脚本](../../scripts/csharp/verify-jolt-binding-projection.cs) · [已验证范围](./current-status.md#生态与参考应用)

| 库 | 手写绑定的调用链 | 投影证据与缺口 |
| --- | --- | --- |
| date-fns | `Add(Date, DateFnsDuration)` 和 `FormatDuration(DateFnsDuration, DateFnsFormatDurationOptions?)` 共享时长。把同一个区间用于 `Clamp` 和 `EachDayOfInterval` 时，还要传递分步选项并读取日期数组。 [手写 API](../../src/ECMAScript.DateFns/Api/DateFns.Api.cs) | 同一个 `DateFns.Interval` 已能传给 `IsWithinInterval` 和 `IntervalToDuration`，两天的区间返回 `Days == 2`。普通 C# 消费者把返回对象直接传给 `FormatDuration` 得到 `2 days`。对照验收还编译共享时长和日期集合。独立类库消费者实际用 `Delimiter` 格式化时长。`FormatISO` 的 `Format` 与 `Representation` 从两组字符串字面量推导为不同的 C# 枚举，SSR 得到 `20260115`。独立类库将 `date-fns/locale` 的 `Fr` 传给 `FormatDistance` 得到 `2 jours`，类型错误的 locale 会产生编译诊断。继续核对日期子类和上下文选项。锁定的 date-fns 4.4.0 将 `Date`、时间戳和字符串纳入泛型日期输入，部分结果还受上下文类型影响。 [对照验收](../../scripts/csharp/verify-jolt-binding-projection.cs) · [独立类库验收](../../scripts/csharp/verify-jolt-public-projection.cs) |
| VueUse | `UseStorage` 接收 key、初值和 options 并返回可写 ref。`UseMediaQuery` 返回 computed。`UseClipboard` 的返回对象组合状态和动作。`UseEventListener` 接受 DOM 目标及事件回调并返回清理函数。 [手写 API](../../src/ECMAScript.VueUse/Api/VueUse.Api.cs) | 对照验收检查存储、计数器、鼠标和网络状态的 C# 使用，运行夹具验证网络状态。JSDOM 中已验证 `UseEventListener(target, "click", callback)` 接收 `EventRef`，并分别验证主动清理和卸载后停止监听。锁定的 `HTMLElementEventMap` 还可推导 `keydown` 的 `KeyboardEvent.Key` 和 `dblclick` 的 `MouseEvent.ClientX`。后续检查其他事件目标、存储选项与可写 ref 的联动，以及剪贴板授权。 [对照验收](../../scripts/csharp/verify-jolt-binding-projection.cs) · [运行夹具](../../scripts/csharp/fixtures/jolt-npm-runtime/App.jazor) |
| Floating UI | `Arrow`、`Shift` 等产生共享 `FloatingMiddleware`，交给 `ComputePosition` 或 `UseFloating`。异步定位返回坐标和 middleware data。`AutoUpdate` 返回清理函数。 [手写 API](../../src/ECMAScript.FloatingUi/Api/FloatingUi.Api.cs) | 运行夹具将 DOM 入口的 `Offset` 和 `Arrow` 结果交给 `ComputePosition`，从 Promise 读取实际箭头坐标。C# 选项 `Placement.BottomStart` 传入后，结果中的 `Placement` 为 `bottom-start`。上游可以向 middleware 数组写入 `false`，投影复制数组并保留其中每个 middleware 的对象身份。DOM 入口的 `AutoUpdate` 在安装时和窗口 resize 时各回调一次，执行返回的清理函数后再次 resize，次数仍为两次。Vue 入口的 `UseFloating` 已接收两个可空元素的只读 ref，替换锚点后重新定位，卸载后停止布局读取。在 `WhileElementsMounted` 中接入 DOM 的 `AutoUpdate` 后，换锚点会移除原滚动容器的监听器，卸载会移除新容器的监听器。Vue 的 `UseFloating` 已接收 DOM 的 `Offset`、Vue 的 `Arrow` 和带 limiter 的 `Shift`，误将 limiter 当作 middleware 会产生 C# 诊断。 [定位夹具](../../scripts/csharp/fixtures/jolt-npm-runtime/Floating.jazor) |
| VeeValidate | `UseField<TValue>` 贯穿字段名称、规则、初值和可写值。表单端从 `UseForm<TValues>` 配置走到字段错误、校验结果与提交回调。 [手写 API](../../src/ECMAScript.VeeValidate/Api/VeeValidate.Api.cs) | 运行夹具对记录字段执行同步验证，对数值字段等待异步规则，改值后重新验证。表单实例的 `Values` 保留输入类型，成功回调读取转换后的记录。实例的 `Errors` 和注入上下文的 `UseFormErrors()` 都已验证能读到失败消息，并在校验通过后读到 `null`。字段数组、成功与失败提交、字段写入和重置已通过 SSR 验收。`UseFormValues<T>()` 的只读选择器保留记录属性的实际 JS 名称，缺失的数值字段读为 `null`。`UseFieldValue<T>()` 已验证字符串、记录、数值和布尔字段。SSR 和客户端挂载都能区分 `0`、`false` 与缺失字段的 `null`。嵌套记录可通过属性选择器读取，缺少中间记录和缺少末端字段均返回 `null`。嵌套写入、数组索引和缺少输入输出类型的 schema 仍待覆盖。 [字段与表单运行夹具](../../scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor) |
| Vue I18n | `CreateI18n` 的实例公开 `Global` Composer 和 `Dispose`。`UseI18n` 取得 Composer，`Locale` 可写，`T` 支持 key、复数和命名值等调用形式。 [手写类型](../../src/ECMAScript.VueI18n/Types/VueI18n.Types.cs) | SSR 夹具从法语切换到英语，调用 `T("items", 1)` 和 `T("items", 2)` 得到单数与复数文本。另一个 SSR 应用从 C# 创建插件、设置法语字符串消息、调用 `app.Use(plugin)`，使用字符串字典向同一条消息传入两个名字，SSR 渲染出 `Bonjour Ada / Bonjour Lin` 后释放插件。独立的 SSR 应用还验证了一层嵌套消息和本地作用域。创建插件时指定布尔字面量模式，组合式 `Global.Locale.Value` 和 Legacy 的 `Global.Locale` 都可在切换后译出新消息。日期和数字的基本格式化已通过 SSR。后续覆盖按消息约束插值键及切换后的客户端重渲染。锁定声明中的 Composer 重载应逐个确认，尤其是泛型 key 推导和参数组合。 [对照验收](../../scripts/csharp/verify-jolt-binding-projection.cs) |
| Vue Query | `UseQuery<TData>` 连接查询 key、回调和数据 ref。`UseMutation<TData, TVariables>` 连接变量、异步结果与状态。`UseQueryClient` 的实例方法用于缓存管理。 [手写 API](../../src/ECMAScript.VueQuery/Api/VueQuery.Api.cs) | 运行夹具覆盖记录结果查询、类型化 mutation 和成功后的 `0`、`false`。独立类库消费者还验证了查询缺省值和异步获取记录。C# 在 SSR 和客户端调用 `app.Use(Query.VueQueryPlugin)`。另一个用例同时保留三个活跃查询，执行精确匹配、前缀失效及主动重取，再执行无参失效和刷新，检查每个缓存的最终值。查询键支持字符串、数字、布尔和 `VueProps` 记录，`ECMAScript.Number` 变量可直接用作数值元素。并发用例交错刷新两个页号，让同页旧请求最后返回，并核对取消信号与两页各自的缓存。无限查询已用数字游标连续加载两页，C# 读取记录字段并通过 `null` 停止。锁定声明的默认结果只提供 `unknown[]` 类型的 `pageParams`，已在真实缓存上核对游标序列。后续验证自定义结果类型与标量 mutation 的 `Variables` ref。 [运行夹具](../../scripts/csharp/fixtures/jolt-npm-runtime/Query.jazor) · [独立类库验收](../../scripts/csharp/verify-jolt-public-projection.cs) |
| Monaco | `CreateModel` 的结果传给编辑器 `SetModel`、marker API 或 worker。模型和编辑器事件返回可释放的订阅，worker 使用应用指定的代理接口。 [手写 API](../../src/ECMAScript.Monaco/Api/Monaco.Api.cs) · [手写类型](../../src/ECMAScript.Monaco/Types/Monaco.Types.cs) | 浏览器夹具创建编辑器与模型、同步模型到 worker、读取结果并释放资源。模型修改后从回调读取实际 `VersionId`，释放订阅后再修改不触发回调。自定义主题用 `BuiltinTheme.VsDark`、token 规则数组和颜色字典调用 `DefineTheme`。切换后 Chromium 读到配置的背景色 `rgb(18, 52, 86)`。C# 可向两个 owner 写入强类型 marker 数组，并按 owner 和模型 URI 读取。Chromium 已核对消息、严重度、范围和按 owner 清理后保留的标记。锁定 Monaco 的 `getProxy()` 和 `withSyncedResources(...)` 返回 Promise，调用按异步契约执行。 [浏览器夹具](../../scripts/csharp/fixtures/jolt-monaco-browser/) |

## 投影边界

- 投影使用具体类型、重载、闭合 union 和显式回调。TypeScript checker 解析实际使用的导出与可具现泛型。生成的 C# 保留值的方向、可写性、可选参数和显式 null，复用宿主 Date、DOM 以及 Vue ref/computed 契约。
- 同一上游命名类型及相同泛型实参在兼容的使用方向上共享 C# 类型，结果可直接传给接收该类型的下一项 API。包身份、公开声明和泛型实参决定生成类型的身份。
- 支持真实使用的默认导出、具名导出及公开子路径，保留对象方法的 JavaScript 接收者。项目锁文件确定依赖解析。Vite 处理样式和 worker 资源，应用负责插件注册、浏览器挂载、卸载及适用的 SSR 生命周期。
- 上游声明缺失或无法完整描述动态接口时，使用类型包、项目内的窄 `.d.ts` 或带类型的 ESM 适配模块补足契约。仍无法可靠表达的使用点给出包含包、导出和声明位置的诊断。

## 从手写设计提取映射规则

对照时沿着一段作者代码追踪类型。从构造选项、选择重载、传入回调，一直跟到读取结果或调用下一个 API。手写绑定说明开发者需要表达什么，锁定的上游声明决定当前版本实际允许什么。导出计数用于寻找遗漏，一个函数能生成短参数重载时，还要检查手写调用用到的其他参数、返回成员和组合方式。

| 映射问题 | 手写参考 | 在新包上应得到的行为 |
| --- | --- | --- |
| 同一个类型如何跨调用复用 | date-fns 的共享时长与区间，Monaco 的模型，Floating UI 的 middleware | 依照声明身份与泛型实参生成兼容的 C# 类型。前一个导出的结果可以直接交给后一个导出，类型不兼容的实参由 C# 定位。 |
| 如何从 JS 的多种输入形式得到好用的重载 | VueUse 的普通值、ref 与 getter，date-fns 的日期与集合 | 利用调用方向、只读性和 TypeScript 重载顺序决定宿主类型、闭合 union 或重载。普通 C# 调用可以传值、lambda 或集合，并得到该重载对应的结果。 |
| 有限选项如何保留原始值 | date-fns 的 ISO 格式、Floating UI 的 placement、Monaco 的 marker severity | 输入字段的纯字符串字面量联合生成枚举，编译时检查选项，发射时保留大小写和标点。具名数字枚举按声明区分类型，保留上游数值。 |
| 一个泛型如何贯穿配置、回调和异步状态 | VeeValidate 的字段初值与验证器，Vue Query 的查询和提交，Floating UI 的异步定位结果 | 跟踪类型参数在声明中的每次使用。应用自己的记录可以进入回调并从 ref 或 Promise 读回，缺省值、零和假值可区分。 |
| 哪些对象应复用宿主协议 | Vue I18n 的 locale 与 Composer 方法，VueUse 的 computed，Floating UI 的 DOM 元素，Monaco 的 worker | 通过声明识别宿主类型、读写方向和实例接收者。开发者可用已有 Vue 与 DOM 类型，实例方法提取为委托后仍调用原对象。 |

七库盘点已能定位手写重载和生成成员，后续按上表从未覆盖的调用选择用例。优先处理多个库共用、且会阻断下一次调用的类型关系。每次改动选一个真实手写用法复验，再用另一个库或独立的最小 `.d.ts` 改换包名、导出名和成员名，检查相同结构能否得到同样的强类型行为。近似声明则用反例验证边界，例如普通元组参数必须保留为一个实参。仓库现有独立声明夹具已覆盖跨函数共享输入类型、非对称 ref 读写和元组参数，可继续承载这些用例。[独立声明回归](../../src/Jolt.Npm/verify.cs)

## 后续开发顺序

1. **扩展元素 ref 与配置回调的组合。** Floating UI 的双 ref 定位已验证挂载、换元素和卸载。`whileElementsMounted` 的 `AutoUpdate` 清理已经过换锚点和卸载验证。DOM middleware 已能传给 Vue，且 limiter 误用会在编译时被拒绝。后续对照 VueUse 尚未执行业务用例的 getter、ref 和浏览器事件签名。
2. **从键推导值。** 从已验证的 `HTMLElementEventMap` 键盘和鼠标事件，扩展到其他 DOM 目标以及 VeeValidate 的嵌套写入和数组索引。合法键应选中对应的字段值或事件参数，错误类型应定位到 C# 实参。上游 schema 缺少输入、输出类型时，通过带类型的适配器提供这部分信息。
3. **检查依赖选项的返回类型和异步状态。** 在已验证的 Vue I18n 模式签名上，继续检查嵌套消息的插值键。Vue Query 继续检查显式结果类型能否保留 `PageParams`，以及标量 mutation 的 `Variables` ref。每个扩展都在另一个包或改名声明上复验，并把真实消费代码加入七库对照。

## 迁移验收

1. 独立消费者直接声明 npm 依赖，使用锁文件和生成的 C# 契约。移除对应旧绑定引用后，表中约定的用例全部通过类型检查和运行验证。错误参数、无效写入和无法表达的类型有可定位的诊断。
2. CLI 和编辑器看到相同的 C# 签名及定义。相同的锁定输入生成稳定的类型名和 ESM 导入。依赖声明变动后重新检查受影响的调用。
3. 编译和 Vite 产物保留正确的导出名、实参、接收者和运行时资源。浏览器复现实际交互，并按用例验证状态更新、事件次数、卸载、worker 及适用的 SSR 行为。
4. 迁移说明展示 npm 依赖声明和 C# 调用的新写法。若名称、类型或导出形态发生破坏性变更，在发布说明记录旧写法及替换方式。已通过的场景进入对应的支持清单。

七个样本的实际可用状态见[当前状态](./current-status.md)。旧执行表和首次发布背景见[历史演进](../05-history/evolution.md#2026-09-28-api-与发布接线复核)。
