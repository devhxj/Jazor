# JS 资源库绑定指南

> 面向：为新的上游 JavaScript 库创建 `ECMAScript.<Name>` JS resource library 绑定的仓库维护者。

本指南沉淀自 `ECMAScript.DateFns` 的完整交付，覆盖从上游锁定、来源 metadata、入口设计、生成器、C# 契约到测试与门禁接线的全流程。纯函数库可直接照搬；组件库（含 props/slots 契约）在此基础上参考 `ECMAScript.VueRoute`、`ECMAScript.VuIcons` 和 `ECMAScript.VueDraggable` 的组件投影模式。分阶段选型与验收门槛见 [P3 Vue 应用生态绑定扩展计划](../04-roadmap/p3-vue-application-bindings-plan.md)。

一个 JS resource library 绑定由五部分组成：来源锁定（`npm`、`jsr` 或 `embedded-mjs`）、版本化 package metadata、C# 宿主契约（`[ECMAScript]` 映射）、可验证的 JS/CSS/worker/static 资源闭包与包接线（nuspec、buildTransitive targets、测试与门禁）。绑定保留上游运行时语义，Emit 将 metadata 和实际 import roots 投影为标准 `jazor/package.json`，在 MSBuild staging 中调用 DenoHost runtime 生成 `deno.lock` 和恢复 `node_modules`，NetPack/SSR 共用该工作根。

## 上游分析与锁定

上游以可审计的 registry/package metadata 锁定，绑定记录来源、版本、完整性、许可证和采集 fingerprint：

- 从 `https://registry.npmjs.org/<package>/<version>` 读取 `dist.integrity`（SHA-512）与 `dist.tarball`；下载后先校验 integrity 再解包。tarball 缓存于 `.tmp/packages/<package>-<version>.tgz`，`.tmp/` 不进版本库。
- 判定上游分发形态并记录可 tree shake 的边界：
  - 扁平 ESM（每函数一个文件，`index.js` barrel，如 date-fns）——保留上游 package exports，入口闭包即依赖图。
  - 预打包浏览器构建（`*.esm-browser.js` 与 `*.prod.js`，如 vue-router）——使用上游条件导出选择 dev/prod profile。
  - UMD-only（如 TDesign）——绑定生成经过验证的 ESM adapter，并把 Vue 等共享运行时标为 peer dependency；当上游提供细粒度 ESM 入口后，按入口迁移，先例见 `update-vue-binding-inputs.cs`。
- 上游运行时依赖进入 package metadata。date-fns 类零依赖库的闭包只含相对路径；存在包依赖时分别记录 package dependency、peer dependency 和 profile 条件，由 Emit 统一解析。
- 闭包计算保留真实模块边：先剥离块注释与行注释（JSDoc 示例中的裸 specifier不参与依赖图）；相对路径按词法解析 `.`/`..` 段，使同一模块保持稳定 identity。
- 许可证文件随包进入 `licenses/`，metadata 顶层 `files` 声明其哈希和来源。

上游版本、来源 URL、文档来源与许可证写入 `inventory.json` 并计算内容 fingerprint；升级上游时生成器重跑，fingerprint 变化即 contract drift，需人工复核。

### 绑定资源来源

每个绑定入口在 package metadata 中声明一个来源 identity，入口映射和资源闭包按来源进入同一张 package graph：

| 来源 | 绑定包记录 | Emit 交付 |
| --- | --- | --- |
| `npm` | npm package name、精确版本、integrity、`exports`、peer/optional dependency | 根 `package.json` 的精确 dependency，由 Emit 调用 DenoHost runtime 恢复到 `jazor/node_modules` |
| `jsr` | `jsr:` identity、精确版本或 integrity、入口和导出；需要时附本地 adapter fingerprint | JSR npm 兼容 package 或校验后的本地 package，供 Deno 与 NetPack 使用同一入口和字节 |
| `embedded-mjs` | 明确声明的本地 MJS carrier、版本、入口、导出、相对依赖、`sideEffects`、CSS/worker/static 资源 | `packages/<name>` 与 `node_modules/<name>` 下的标准本地 package，按 roots 物化资源闭包 |

`embedded-mjs` 是标准 package graph 支持的本地来源。当前仓库自有 ECMAScript 源码位于 `src/ECMAScript/clr/**`；映射库运行时入口使用上游 npm/JSR package，独立运行时桥接按需声明为 `embedded-mjs` carrier。metadata 记录逻辑 specifier 到文件的映射、哈希与资源边；Emit 生成该 package 的 `package.json`/`exports`，再把它和同一绑定声明的 npm/JSR 依赖连接起来。作者代码始终引用逻辑 package specifier，carrier 路径只由 metadata 和 materializer 管理。

## 入口设计

C# 代码使用的 import specifier 就是 package metadata 的入口 key，也是 `[ECMAScript("<specifier>")]` 的参数。该特性参数是绑定入口的唯一真源，编译器会把它原样作为最终 ESM import specifier；metadata 负责校验它对应的 package、`exports`、版本、条件和资源闭包。参数采用标准 package specifier/subpath，例如 `tdesign-vue-next/button/Button` 或 `date-fns/addDays`；物理资源路径、临时文件名和完整性信息由 metadata 管理。

入口声明与 package identity 分开维护：

- `[ECMAScript]` 的第一个参数表达模块入口；`Transform` 表达普通导入或组件导入；组件的 `ExportName` 表达 named/default export。
- package metadata 的 `libraryId`、`version`、来源、integrity、`exports`、`sideEffects`、peer dependency、条件导出和 CSS/worker/static 资源表达包身份和依赖闭包。
- 当一个宿主类型需要多个 ESM 入口时，在具体成员上标注 `[ECMAScript]`；成员入口优先于类型入口。类型级入口只作为未细分声明的默认入口。

入口按上游形态选择：

| 上游形态 | 入口策略 | 先例 |
| --- | --- | --- |
| 干净 barrel 且全量可接受 | specifier 直接指向上游 barrel，由 package graph 保留完整闭包 | `date-fns` → 上游 `dist/index.js` |
| barrel 过大或需策展子集 | 生成 re-export 桥模块，只导出策展清单 | 使用 package 的 `exports` 子路径（例如 `date-fns/locale`），或在 metadata 中声明明确的 embedded carrier |
| 子树天然独立 | 拆为多个 specifier，各自闭包 | `date-fns` 与 `date-fns/locale` |
| dev/prod 分离构建 | 同一 specifier 的 development/production 指向不同文件与哈希 | `vue-router` |
| 运行时强耦合 | 一个 specifier 绑定组件、composable、指令和共享运行时的完整闭包 | `vue-draggable-plus` |

策展桥（如 locale 桥）属于资源框架代码，由生成器维护并标注生成来源；它承载稳定的 ESM re-export 边界，重新生成即可更新。策展清单只保留在生成器一处，桥模块、package metadata 闭包与 C# 契约由同一次生成保持一致。

package metadata 的模块依赖规则：entry 的 `developmentModuleDependencies`/`productionModuleDependencies` 列出闭包内除入口外的每个模块路径，且每个路径必须有同值的 `files` 条目（`type: "module"`、`moduleId` 即该路径）供 `LibraryManifest.FindModule` 解析。package dependency 通过包名、版本和来源 identity 连接到另一份 metadata；CSS、worker、static 资源通过对应 resource edge 连接。两个入口闭包共享的文件允许在两侧都声明，但 metadata 必须完全一致。上游无 dev/prod 差异时，两个 profile 指向同一文件与哈希。

## 生成器脚本

生成器是 `scripts/csharp/` 下的单文件 C# 脚本（遵循仓库脚本策略，不新增 PowerShell）：

- 命名 `generate-<library>.cs` 或 `update-<library>-inputs.cs`；参数 `--version <v>`（首次必填，之后复用 manifest 版本）与 `--source <dir>`（本地解包目录，用于离线复跑）。
- 生成器读取并校验上游包，生成 package metadata（`manifest.json`/`inventory.json`）、许可证副本，以及需要生成式维护的 C# 契约（如 locale 属性表）。确实需要自有 MJS 时，将其写入 manifest 声明的 embedded carrier，并为每个入口记录哈希和资源边。
- npm/JSR 入口记录上游 package identity、exports、sideEffects 和版本完整性；embedded carrier 的哈希取自项目树中刚写入的文件，确保本地 package projection 可复现。
- 输出统计与“复核 contract drift”提示；生成器不猜 C# 表面，人工编写的 API 契约永远手写并评审。

## C# 契约

所有绑定类型位于 `ECMAScript` 命名空间（`RootNamespace`），类型名加库名前缀（`DateFnsDuration`）避免跨包冲突。核心模式：

- 模块宿主：`[ECMAScript("<specifier>")]` + `[Description("@#")]` 的 `static partial class`；成员 `[Description("@#<exportName>")]` 的 `extern static` 方法/属性。`@#` 前缀表示逐字 JavaScript 标识符。
- 选项对象：`[ECMAScript]` + `[Description("@#")]` 的 `record`，`init` 属性用 `[Description("@#<propName>")]` 映射为对象字面量成员；可选属性用可空类型。
- 枚举：数值域直接声明数值成员（发射为数字字面量）；字符串域标注 `[String]` 且每个成员显式 `[Description("@#<js字符串>")]`（发射为字符串字面量）。
- 不透明宿主值（如 locale 对象）：`sealed class` + 私有构造器，不给成员。
- 标量与日期参数优先复用核心宿主类型（`Number`、`string`、`Date`）；`Number` 自带从 CLR 数值的隐式转换，`AddDays(date, 3)` 形态可直接编写。
- **`object` 的判定标准是「能否更精确表达」，不是「是否出现」**：当值域本身开放、C# 无法给出更精确类型时，`object?` 是正当用法（仓库先例：`Global.TypeOf(object? value)`、`NumberValue(object?)`；绑定侧的 `isDate(value: unknown)`）。当值域可被具体类型、命名 record、不透明宿主类型或泛型表达时，禁止降级为 `object`。可精确表达却用了 `object` 才算过度降级。
- 上游扩展缝（如 date-fns 的 `ContextOptions`/TZDate）可整体不绑定，但必须在包 README 的未绑定清单中写明。
- 上游 `Object`/`Any` 域优先用泛型（`CreateEventHook<T>`、`UseAsyncState<T>`）或**不透明宿主类型**（`sealed class` + 私有构造器，如 `FloatingMiddleware`、`FilePondPlugin`）表达，这两种写法都比 `object?` 精确。类型擦除不是弱化 C# 作者面的理由；泛型参数不参与编译器特判时不会要求具体 runtime 语义。

## 组件绑定

当上游导出 **Vue 组件**（而非纯函数或 composable）时，绑定按上游组件数量选择两种范型。两者共享同一套参数映射规则，区别只在描述符如何产生。

| 范型 | 适用 | 参考实现 | 描述符来源 |
| --- | --- | --- | --- |
| A：手写双表示 | 组件数少（个位数），且需要逐组件强类型 props/slots | `ECMAScript.VueRoute`（`RouterLink`/`RouterView`） | 手写 `record XxxProps`/`XxxSlots` + `IVueComponent<TProps, TSlots>` |
| B：全量代理 + 生成描述符 | 组件数多（几十到上百），上游提供机器可读 metadata | `ECMAScript.Vuetify`（114 组件）、`ECMAScript.ElementPlus`、`ECMAScript.TDesign` | 生成器从上游 contracts/web-types 产出导出目录与 shim |

无论哪种范型，组件代理类都作为 Razor 编译期契约容器；发射时由 RazorVue 映射到上游同名组件，并把组件声明的 ESM root 交给 Emit 的 package graph。组件库的 JS 运行时由上游 package 或明确声明的 embedded carrier 提供。

### 共同约定

- **命名空间隔离**：组件库使用独立命名空间（`ECMAScript.Vuetify`、`ECMAScript.ElementPlus`、`ECMAScript.TDesign`），不复用函数库的 `ECMAScript` 命名空间；组件类型加库前缀（`V*`/`T*`/`El*`）避免跨库冲突。
- **显式命名优先**：生成组件对每个参数显式标注 `[ECMAScriptName("<运行时名>")]`（`Vuetify` 生成部分 2460 处），不依赖命名推断。手写基类在 C# 名经 camelCase 降低后已等于 JS 名时省略该属性（`VInputComponentBase.cs` 的 `Id`/`Name`/`PersistentHint` → `id`/`name`/`persistentHint`，45 个参数零标注）。C# 保留字或冲突必须还原：`CssClass` → `class`、`CssStyle` → `style`。
- **组件声明**：`[ECMAScript("<specifier>", Transform.Component, "<ExportName>")]`；`Transform.Component` 专用于组件，函数/Hook 用 `Transform.Import`，不可混用。
- **组件代理**：`sealed class : ComponentBase, IVueComponent`；必填参数标 `[Parameter] [EditorRequired]`。
- **插槽**：无作用域用 `[Parameter] RenderFragment? ChildContent` 并标 `[ECMAScriptName("default")]`；有作用域用 `RenderFragment<TSlotContext>`，上下文是 `[ECMAScript] [Description("@#")] record`（需要作为对象传给宿主 API 时才继承 `Vue.VueProps`）。
- **事件**：`[Parameter] EventCallback<TEvent>`；无参事件用 `EventCallback`。
- **透传属性**：`[Parameter(CaptureUnmatchedValues = true)] IReadOnlyDictionary<string, object?>? AdditionalAttributes`，键使用 Vue/HTML 实际属性名，使 `data-*`、`aria-*`、`role` 原样落到宿主元素。

### 范型 A：手写双表示

代理面向 `.razor` 标签作者，描述符面向 `H()` 渲染函数作者，二者映射同一上游组件。

```razor
<VueRouterLink CssClass="@CssClassValue" To="@Target.Route" data-action-key="@Action.Key">@Text</VueRouterLink>
```

描述符固定形态：

- Props 用 `record XxxProps : Vue.VueProps`；公共选项抽到基类 `record XxxOptions : Vue.VueProps` 再由完整 Props 继承（`RouterLinkOptions` → `RouterLinkProps`），避免重复。
- Slots 用 `record XxxSlots : Vue.VueSlots`，默认槽是委托属性 `XxxSlotCallback? Default`。
- 描述符类型按需要选接口：props+slots 用 `IVueComponent<TProps, TSlots>`，仅 props 用 `IVueComponent<TProps>`，仅 slots 用 `IVueSlotComponent<TSlots>`——接口选择让 `H` 重载解析到正确形状。
- 调用形态：`H(RouterLink, new RouterLinkProps { ... }, new RouterLinkSlots { Default = scope => new IVNode[] { ... } })`。
- 两套表示的参数集、插槽名、事件名必须同步，否则 Razor 标签与 `H()` 调用会漂移；代理测试与发射测试分别锁定两侧。

### 范型 B：全量代理 + 生成描述符

组件数大时不在 C# 侧手写每个组件的 props 记录，改由生成器维护：

- **一个组件一个文件**：`VBtn.cs`、`VAlert.cs`，各自是完整代理（`VBtnSlotContexts.cs` 等单独承载插槽上下文）。
- **标记接口替代泛型描述符**：`public interface IVuetifyComponent : IVueComponent { }`，代理实现它；不再为每个组件生成 `TProps`/`TSlots`。
- **生成导出目录**：生成器产出 `VuetifyCatalog.g.cs`，即 `static class VuetifyComponents` 上的 `extern static IVuetifyComponent VAlert { get; }`（带 `[ECMAScriptName("VAlert")]`），供 `H()` 按导出名引用。
- **入口目录**：`VuetifyCatalog.g.cs` 的每个组件入口直接对应 Vuetify `lib/components/*/index.js`、`lib/labs/*/index.js` 或 `lib/directives/*/index.js` 的上游 package path；稳定版与 labs 使用 `vuetify/components/<Name>`、`vuetify/labs/components/<Name>` 等逻辑 specifier。入口的命名导出由 metadata 校验并交给 package resolver，保持上游 package 的导出边界。
- **基类复用**：多个组件共享的插槽/属性抽到基类，例如 `ElComponentBase` 承载 `CssClass`/`CssStyle`/`AdditionalAttributes`，`ElContentComponentBase` 再补 `ChildContent`。
- **上游输入锁定**：`src/ECMAScript.Vue.Generator/upstream/<lib>/<version>/` 冻结 `contracts.json`、`web-types.json`、`package.json`；生成器不得用 `object`、`VueValue` 或占位类型伪造组件覆盖率。
- **受控产物**：`V*.cs`、`VuetifyCatalog.g.cs` 和 package metadata 由生成器维护；改契约要先改生成器或上游输入，再运行生成并用 `--check` 校验，生成文件保持可重复构建。运行时模块与样式继续由上游 npm package 提供。
- **样式资源**：组件库在 package metadata 中声明组件样式和必要的全局 stylesheet edge；Emit 按使用的组件 roots 选择 CSS 闭包，NetPack 从恢复后的上游 package 读取 CSS 与 sideEffects 信息。

组件库的额外门禁（相对函数库）：

- 契约漂移：`verify-vue-binding-contracts.cs` 跑各生成器 `--check`，并校验 manifest 版本、上游快照与文档来源；可输出 schema 1.0 fingerprint 报告，用 `--baseline` + `--fail-on-baseline-drift` 阻断 inventory 漂移。
- 覆盖：`verify-vue-binding-coverage.cs` 要求每个目标 ≥90% 的已审计契约单元有作者面。
- 文档：`verify-binding-documentation.cs` 要求公开声明有双语 XML，并保留上游 JSDoc/web-types/MDN 来源与版本、许可证、采集日期。

## 测试

包专属测试项目 `src/ECMAScript.<Name>.Test` 覆盖六类，文件定位用 `[CallerFilePath]`（并行 lane 的 `BaseOutputPath` 差异不影响资源解析）：

1. package metadata：schema/libraryId/版本/来源/inventory 一致、moduleDependencies 与 files moduleId 集合相等、全部声明文件哈希逐一复算，并校验入口到 npm/JSR/embedded-mjs identity 的映射。
2. embedded carrier（存在时）的实际文件集合与 metadata 声明闭包精确相等（双向差集为空）；npm/JSR 入口校验版本、integrity、exports 与 peer 约束。
3. 上游 drift：静态提取上游 barrel 的命名导出（`export function x` 与 `export { a, b as c }`，排除 default），断言 C# 绑定的每个 `@#` 导出名都存在；策展清单做生成文件、manifest 闭包、inventory 与 C# 契约的四方一致断言。
4. inventory：fingerprint 复算覆盖除自身外的全部 payload。
5. proxy 与编译边界：import 宿主与 Transform.Import、首期表面按参数类型逐位锁定、`object` 降级扫描（**递归检查泛型实参**，捕获 `IVueRef<object>` 这类降级；对值域确实开放的参数显式列入例外并注释理由，见 `ECMAScript.VueRoute.Test/EcmaScriptVueRouteProxyTests.cs` 的 `AssertNotObject`）、枚举值域；编译器发射断言覆盖命名导入、选项对象字面量、枚举字面量与策展桥导入。
6. 组件（有组件时）：
   - 范型 A：代理与描述符两侧分别断言 `Transform.Component` 与 ExportName、必填参数的 `EditorRequired`、`[ECMAScriptName]` 还原名、默认槽的 `RenderFragment<TSlotScope>`、`EventCallback<T>`、`CaptureUnmatchedValues` 透传；发射测试锁定 `H(Component, new XxxProps { ... }, new XxxSlots { ... })` 与作用域槽回调的 VNode 数组返回。
   - 范型 B：断言生成导出目录的组件数与代理类数一致、每个 `VuetifyComponents.X` 都有对应代理与 `[ECMAScriptName]`、shim 模块的导出与 manifest 入口对齐，并运行生成器 `--check` 作为陈旧检测。
   - 两者都加布局守卫测试锁定目录结构与 shell 文件只保留属性入口，防止组件代理被误并入 API 分片（见 `ECMAScript.VueRoute.Test/EcmaScriptVueRouteLayoutGuardTests.cs`）。

编译器发射的稳定格式（断言按此书写）：同一模块的命名导入合并为单条语句并按字母排序；三个及以上属性的对象字面量多行展开。Emit 层在 `Jazor.EmitTest.LibraryMaterializerTests` 增加真实 package materialization 测试：以 `requiredImports` 选择入口，断言 embedded mjs 的 import path 指向标准 local package 的 `exports` 目标，外部 npm/JSR identity 进入根 `package.json`，并对每个 embedded 模块的相对导入逐一验证可解析。完整 npm 输入存在时再断言 `package-lock.json`；外部图由 MSBuild 调用 Emit 生成 `deno.lock`，并由 `jazor/node_modules/<package>/package.json` 与 frozen graph 验证。`Load_AllRepositoryResourceManifests` 会自动加载 `src/` 下所有 manifest，新包接入后该项即自动获得 schema 与哈希校验。

## 仓库接线

新包需要同步的接线点，遗漏任何一处都会有测试或门禁拦住：

| 位置 | 内容 | 拦截者 |
| --- | --- | --- |
| `Jazor.slnx` | 主项目（顶层，2 空格缩进）与测试项目（`/test/` folder，4 空格缩进） | 解决方案构建 |
| `scripts/csharp/test-dotnet.cs` | 项目变量、`switch` lane、默认 all 数组、`NormalizeProject` 集合、usage 字符串 | lane 入口 |
| `scripts/csharp/publish-nuget.cs` | `DefaultPublicPackageIds`、`PackageAliases`、catalog 条目、usage 与错误信息 | 本地打包 |
| `scripts/csharp/verify-binding-documentation.cs` | `libraries` 数组纳入新包；csproj 开启 `GenerateDocumentationFile`，全部公开声明有非空双语 `summary` | 文档门禁 |
| `src/ECMAScript.Pinia.Test/EcmaScriptPiniaLayoutGuardTests.cs` | 锁定 publish 脚本默认包集合字符串，新增包必须同步 | Pinia lane |
| `src/ECMAScript.Vue.Generator/`（仅组件库） | 生成器命令、`upstream/<lib>/<version>/` 上游输入快照、`Program.cs` 分派 | 契约生成 |
| `scripts/csharp/verify-vue-binding-contracts.cs` 的 `checks`/`targets`（仅组件库） | 生成器 `--check` 命令、`BindingTarget`（id/版本/包目录/上游目录/显示名） | 绑定契约门禁 |
| `scripts/csharp/verify-vue-binding-coverage.cs` 的目标清单（仅组件库） | 每个目标的 ≥90% 已审计契约单元 | Vue 绑定覆盖门禁 |
| 包目录 | `nuspec`（`jazor/<libraryId>/` 布局，携带 package metadata、明确声明的 embedded carrier（如有）、licenses 与 `lib/<tfm>/<包名>.xml`）、`buildTransitive/*.targets`（`JazorLibraryManifest`）、README | pack 与 consumer |
| 文档 | CHANGELOG 日期章节、current-status 主线 lane、next-development、路线图计划状态 | 发版与评审 |

## 验收链

按顺序执行，全部通过才可声明包级交付；进入 Support 矩阵还需真实 RazorVue 页面与 Chromium browser smoke：

1. `node` 直接 import 恢复后的 npm/JSR package 入口，或 import manifest 声明的 embedded carrier 入口，确认运行时可用（导出数量、一次真实调用）。
2. 包专属 lane：`dotnet run --file scripts/csharp/test-dotnet.cs -- --project <lane>`。
3. `dotnet build Jazor.slnx` 零错误。
4. `Jazor.EmitTest` package graph、materialization 和资源闭包测试通过。
5. `dotnet run --file scripts/csharp/verify-binding-documentation.cs -- --no-build --library ECMAScript.<Name>` 零失败（声明数与 XML 成员数一致）。
6. `dotnet pack` 后检查 nupkg：package metadata、embedded carrier（如有）、licenses/buildTransitive/lib 布局与条目数，含 `lib/<tfm>/<包名>.xml`；外部依赖 identity 记录在 metadata 中。
7. 生成 `jazor/package.json`；存在完整 npm 输入时保留 `package-lock.json`，外部图由 Emit 调用 Deno 生成 `deno.lock`；随后使用 frozen `deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true` 与离线 `deno check --node-modules-dir=manual --no-remote --frozen-lockfile`。
8. `dotnet run --file scripts/csharp/test-dotnet.cs` 全量主线门禁全绿。

## 踩坑清单

以下是 DateFns 交付中实际踩到或验证过的坑，新绑定时先对照：

- 测试项目命名空间不能是 `ECMAScript.<Name>.Test`——外层命名空间会让 `DateFns` 解析到命名空间而非类型；用无点形式 `ECMAScript<Name>Test`。
- `typeof(NullableType)` 非法；反射中 `T?` 就是 `typeof(T)`，直接比较即可。
- 闭包扫描不剥注释会把 JSDoc 示例当依赖；不词法解析 `..` 会重复解析同一 package 文件。
- package metadata 中 `moduleDependencies` 的每个值必须能通过 `files` 的 `moduleId` 或入口 key 解析，否则 materialization 报 `JAZOR_LIBRARY_MODULE_DEPENDENCY_MISSING`；embedded carrier 与外部 package dependency 都要沿同一资源图验证。
- `slnx` 顶层项目与 folder 内项目缩进不同，字符串替换会静默失败；改完必须 `grep` 验证条目存在。
- `String` 枚举成员必须逐个给 `[Description("@#...")]`，不要依赖成员名的大小写推断。
- 新增包进 `publish-nuget.cs` 默认集合时，Pinia 布局守卫的断言字符串要同步更新，否则主线门禁在 Pinia lane 失败。
- 上游有额外构建产物（`.cjs`、`.d.ts`、`fp/` 等）时由 package exports/条件选择实际入口；只有明确声明的 embedded carrier 文件进入本地 package projection。

## 相关入口

- 架构契约：[类库资源与引用契约](../02-architecture/library-artifact-contract.md)、[产物管线](../02-architecture/artifact-pipeline.md)
- 参考实现（函数/composable）：`src/ECMAScript.DateFns`（纯函数单包）、`src/ECMAScript.FloatingUi`（跨包闭包 + 分包版本）、`src/ECMAScript.VueUse`（多入口 bundle + 泛型 composable）
- 参考实现（组件）：`src/ECMAScript.VueRoute`（范型 A 手写双表示）、`src/ECMAScript.Vuetify`（范型 B 全量代理 + 生成描述符，114 组件）、`src/ECMAScript.ElementPlus`（范型 B + 基类复用）、`src/ECMAScript.TDesign`、`src/ECMAScript.VuIcons`（图标式生成组件）
- 生成器先例：`scripts/csharp/generate-date-fns.cs`、`scripts/csharp/generate-vueuse.cs`、`scripts/csharp/generate-floating-ui.cs`、`scripts/csharp/generate-vu-icons.cs`、`scripts/csharp/update-vue-binding-inputs.cs`
- 计划与门槛：[P3 Vue 应用生态绑定扩展计划](../04-roadmap/p3-vue-application-bindings-plan.md)、[开发与测试](./development-and-testing.md)
