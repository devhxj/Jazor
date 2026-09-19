# 绑定包标准化与细粒度 Tree Shaking 迁移计划

 > 状态：进行中（P0-5）。本文冻结迁移目标和验收边界；绑定包以 `manifest.json`、`inventory.json` 和上游 package metadata 描述来源，Emit 生成标准 package project，MSBuild 阶段完成 Deno restore、lock 和 consumer 验证。

## 目标

绑定库以强类型 API 映射上游模块，Emit 生成标准的 `jazor` 依赖项目并在 MSBuild staging 中调用 DenoHost runtime 完成恢复，NetPack 和 SSR 共同消费同一份 `node_modules`。最终结果必须满足：

- 使用一个组件或一个函数时，只解析其真实 ESM 依赖闭包；浏览器 bundle 仅包含已使用的导出、组件、方法和 CSS。
- 组件内部依赖、共享依赖、peer dependency、条件导出和模块副作用得到保留，tree shaking 前后运行时语义一致。
- 绑定可以映射外部 `npm` 包、外部 `JSR` 包，也可以随绑定包携带受控的 `mjs` 资源；三种来源都使用同一套入口、版本、资源和冲突规则。
- Deno 的 SSR 和 NetPack 的浏览器构建使用同一份恢复结果；失败路径显式报告，并保留上一份有效输出。

## 范围

本计划覆盖所有会进入 RazorVue/Emit 资源闭包的 ECMAScript.* 绑定和相关宿主路径：

- binding metadata、生成器 inventory、入口/export 映射和版本/integrity；
- npm、JSR、embedded-mjs 三类来源及其组件内部依赖；
- JS module、CSS、worker、字体、图片、wasm 和许可证等资源边；
- Emit 生成 jazor package project 并由 MSBuild 完成 Deno restore/lock，NetPack 与可选 SSR 消费同一 graph；
- 源码 ProjectReference、NuGet package consumer、Debug/Release、SSR/HMR 和离线 frozen-lock 验证。

计划聚焦依赖和资源交付方式；C# binding 的公共类型语义、Razor SG 约束和 Jazor.Compiler 的 C# lowering 继续沿用现有责任边界。

## 交付边界

- C# binding 继续提供强类型 authoring contract，并通过官方 Razor SG 进入编译链；本计划只调整其外部模块和资源的交付方式。
- npm、JSR 和 embedded-mjs 均通过版本化 metadata、精确入口和完整性信息进入标准 package project；每个来源都具备可审计的 package identity。
- 浏览器、Deno 和 SSR profile 共享同一份依赖图、版本选择和资源闭包，差异通过明确的 profile 条件表达。
- JS、CSS、worker、static 和 license 资源各自拥有可追踪的 owner、入口、hash 和依赖边；profile 输出闭包由选中的资源和这些边确定。
- NetPack、Deno 和第三方 package resolver 保持其标准模块语义，Jazor 负责提供稳定的工作根、入口映射、恢复和错误诊断。

## 前置依赖

实施前必须固定或确认：

1. NetPack 的 Platform.Web/Platform.Deno 能从显式工作根读取 package.json、exports、条件导出和 sideEffects，并能报告解析失败。
2. DenoHost 使用的 Deno 2.9.7 支持 `--package-json`、`--node-modules-dir=manual`、`--node-modules-linker=hoisted` 和 frozen lock；Emit 能在同一目录生成并复用 `deno.lock`。
3. Emit 能稳定收集应用实际 import roots，区分 package dependency、相对 module dependency 和 CSS/worker/static resource edge。
4. 各 binding 能提供上游版本、导出清单、许可证、peer dependency 和资源入口的可审计 metadata；上游 ESM 边界通过原生入口、验证后的 adapter 或明确声明的本地 carrier 记录在 profile 中。
5. 测试环境能隔离网络恢复与离线验证，并以独立 package consumer 路径验证产物。

## 当前问题与迁移动机

当前 JS resource binding 以 `manifest.json`、`inventory.json` 和上游 package metadata 提供 package identity 与入口信息。仓库自有 ECMAScript 源码 carrier 的正式位置是 `src/ECMAScript/clr/**`；`Jazor.Vue/dist/**` 只承担独立 Vue runtime bridge。映射库的运行时代码由 npm/JSR package 提供，历史绑定目录中的 `dist/**` 不再是交付 carrier。协议仍支持经过明确声明、可校验的 `embedded-mjs` package，用于核心运行时或确实需要随包交付的独立桥接模块。迁移将入口和 package identity 投影到标准 package project，使上游包的导出边界、`package.json` 的 `exports`、`sideEffects`、peer dependency 和条件导出完整进入最终 bundler。迁移动机包括：

1. 入口需要从聚合 bundle 细化到组件、函数和共享 helper 的 ESM 边界，才能稳定移除未使用方法和组件。
2. CSS、注册模块、worker 和静态资源需要与 JS 入口建立可验证的资源边，形成按 root 选择的资源闭包。
3. DenoHost、NetPack 和浏览器 bundler 需要从同一份 package project 读取版本、条件导出和 peer dependency。
4. `LibraryPackageWriter` 需要把明确声明的 embedded-mjs carrier 物化为标准本地 package，外部 npm/JSR 依赖由 package project restore 记录版本、Deno lock 和 NetPack 的包解析结果。

本计划把“绑定元数据”和“依赖恢复”分开：绑定声明入口与资源边，Emit 生成项目并调用 DenoHost runtime 完成恢复和冻结，NetPack 按标准包语义 tree shake。

## 目标目录与固定命令

每次 Emit 的 staging/output 根目录为 `jazor/`：

~~~text
jazor/
  package.json          # Emit 生成，声明精确依赖和项目类型
  package-lock.json     # 可保留，作为 npm 生态兼容输入
  deno.lock             # Emit 调用 DenoHost runtime 生成并作为最终冻结锁
  node_modules/         # Emit 调用 DenoHost runtime 恢复，NetPack 和 SSR 共用
~~~

首次恢复由 MSBuild 调用的 Emit 执行，允许联网解析尚未缓存的依赖：

~~~text
deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=false
~~~

恢复成功后，`deno.lock` 是 Deno 的最终冻结依据。后续验证、测试和 SSR 使用：

~~~text
deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true
deno check --node-modules-dir=manual --no-remote --frozen-lockfile entry.mjs
~~~

SSR 运行时使用 npm/兼容包解析，命令组合为本地 `node_modules`、`--no-remote` 和 frozen lock，从而保证运行阶段不联网。Jazor 的输出契约是 `package.json`、可选的 `package-lock.json`、Emit 在 MSBuild staging 中生成的 `deno.lock` 以及恢复后的 `node_modules`。

NetPack 的入口必须位于 `jazor/` 下，或显式把工作根设置为 `jazor/`，使解析器能向上找到 `node_modules`：

- 浏览器构建使用 `Platform.Web`，按 `package.json` 的 `exports`、条件导出和 `sideEffects` 做 tree shaking。
- Deno/SSR 使用同一份 `jazor/node_modules`，依赖来源统一由该目录和对应 lock 文件追踪。
- `LibraryPackageWriter` 的职责是明确声明的 embedded-mjs carrier 的标准本地 package materialization；外部 npm/JSR 依赖由 package project restore。

## 来源模型

绑定元数据需要以版本化 schema 描述来源。建议的来源种类如下，具体字段在第一阶段冻结：

| 来源 | 绑定声明 | Emit 行为 | 解析边界 |
| --- | --- | --- | --- |
| `npm` | 包名、精确版本、integrity、`exports` 子路径、命名导出、peer/optional 约束 | 写入根 `package.json` 的精确 dependency，按选定入口恢复所需文件 | Deno 和 NetPack 按 npm package resolution 解析 |
| `jsr` | `jsr:` specifier、精确版本或 integrity、入口和导出 | 优先归一化为 JSR npm 兼容包的精确 identity（例如 `@jsr/<scope>__<package>@<version>`）；需要适配时物化为经过校验的本地标准 package，让 NetPack 和 Deno 共用该 package | Deno 与 NetPack 解析同一入口和字节，JSR 来源由 metadata 和 hash 追踪 |
| `embedded-mjs` | 明确声明的本地 MJS carrier、package identity、版本、入口、导出、相对依赖、`sideEffects`、CSS/worker/static 资源 | 从声明的 carrier 读取选中的模块和资源，物化为标准本地 package，并参与同一解析图 | 标准 `package.json`、`exports` 和可解析的 ESM 图共同提供入口；本地 carrier 与外部 npm/JSR 包使用同一 package graph |

`npm` 和 `jsr` 是外部依赖来源，绑定包携带 metadata、许可证来源和 contract fingerprint。Emit 统一生成根 `package.json`，可保留兼容 npm 生态的 `package-lock.json`；MSBuild 调用 Emit 在该目录生成并维护 `deno.lock`。JSR 来源通过 JSR npm 兼容 identity 或经过校验的本地 package adapter 进入同一依赖图。`embedded-mjs` 是 `ECMAScript` 类库的受控来源，按入口物化为可解析的本地 package。

`embedded-mjs` 的资源由明确声明的本地 carrier 提供：`manifest.json`/package metadata 记录逻辑入口到 carrier 文件的映射、哈希和资源边，`[ECMAScript("...")]` 继续使用逻辑 package specifier。Emit 将选中的 carrier 投影为 `packages/<name>` 下的标准本地 package，并生成对应的 `exports`；模块内部的相对 import、共享 helper、CSS、worker 和 static 资源沿显式边加入闭包。当前源码型 carrier 只有 `src/ECMAScript/clr/**`；映射库历史 `dist/**` 不进入新 package graph，独立运行时桥接才可以按 `embedded-mjs` 规则单独声明 carrier。

### 入口映射

C# 的 `[ECMAScript("specifier")]` 是绑定声明使用哪个 ESM 入口的唯一真源。编译器应把该值作为最终 import specifier 保留下来，Emit 从实际生成代码收集这些 roots，再把它们交给标准 package resolver。组件 descriptor 和生成器 inventory 用来校验该声明并补充资源信息，不再替 C# 声明选择另一个隐藏入口。

导入路径的契约分为四个彼此独立的部分：

| C# 声明 | 表达的内容 | 示例 |
| --- | --- | --- |
| `ECMAScript` 的第一个参数 | 标准 ESM import specifier，作为逻辑入口 | `tdesign-vue-next/button/Button` |
| `Transform` | 普通导入或组件导入的 lowering 类别 | `Transform.Import` / `Transform.Component` |
| `ExportName` | 该入口的 named export；省略时按默认导出处理 | `Button` |
| 绑定库级 package metadata | 来源、包名、版本、integrity、条件、依赖和资源边 | `npm:tdesign-vue-next@1.20.7` |

推荐的绑定形态如下：

```csharp
[ECMAScript("tdesign-vue-next/button/Button", Transform.Component, "Button")]
public sealed class TButton : IUIComponent
{
}

[ECMAScript("date-fns/addDays")]
public static class AddDays
{
}
```

普通函数、组件、directive 和 composable 都遵循同一条规则。需要不同细粒度入口时，在实际声明该导出的类型或成员上分别写对应的特性；成员级路径优先于宿主类型级路径，未声明成员级路径时继承宿主类型级路径。这样可以让一个 C# 宿主类型承载多个 ESM entry，同时保持每个调用点的 import root 可追踪。

特性中的 specifier 只承担模块寻址。绑定库级 metadata 必须同时记录：

- 上游 package identity、版本、来源类型和完整性信息；
- `specifier` 到 `exports` 子路径以及命名/默认导出的精确映射；
- development、production、browser、deno 等条件选择；
- peer dependency、optional dependency 和共享运行时（例如 Vue）的唯一版本约束；
- CSS、worker、字体、图片和其他静态资源的显式资源入口；
- 上游许可证、文档来源、采集版本和 fingerprint。

组件库优先使用上游的 per-component ESM entry 或合法的 `exports` 子路径。细粒度 tree shaking 的 Support 条件是上游 ESM 入口或经过验证的 ESM adapter；只有全量 UMD/browser bundle 的库进入 `Guidance/Reject` 分流。

### 组件内部依赖

组件入口作为 root，并沿真实模块边递归处理：

~~~text
selected component entry
  -> component implementation
  -> shared utilities / runtime helpers
  -> peer dependency (for example vue)
  -> selected CSS / worker / static resource edges
~~~

同一个模块在多个入口闭包中只保留一个 identity；不同版本的同名依赖按 package manager 和 peer 规则确定。版本选择无法形成唯一结果时，物化阶段报告 owner、版本和 profile；依赖图保持单一运行时实例和原始组件 import 关系。

## 生成的 package 项目契约

Emit 生成的根 `package.json` 是本次选中闭包的依赖项目，不是绑定库的静态文件。它至少包含：

- `"private": true`、`"type": "module"` 和稳定的 package manager metadata；
- 所有 `npm`/`jsr` 外部依赖采用经过审查的精确版本或精确 specifier，并记录 integrity；
- 经过规范化的 peer dependency 选择和条件 profile 信息；
- embedded package 的标准本地依赖/入口声明；具体是 local package 或等价的标准 Node resolution 形式由第一阶段验证；
- ESM roots 由应用和绑定共同选定，package graph 从这些 roots 建立。

`package-lock.json` 的职责是 npm 生态兼容输入。Emit 在能够写出完整 npm 图时生成它；外部 npm/JSR 图直接由 `package.json` 交给 Deno 解析，避免写入不完整的手工 lock。`deno.lock` 是 Deno 的 frozen lock，由 Emit 调用 DenoHost runtime 生成，并由后续 `deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true`、`deno check --frozen-lockfile` 和 SSR 使用。

所有 package、lock 和 `node_modules` 写入同卷 staging，验证成功后原子替换；恢复失败、lock 不一致、包解析失败、路径越界、integrity 错误或输出冲突时保留上一份有效目录并输出诊断。

## CSS、worker 与静态资源闭包

JavaScript tree shaking 与 CSS tree shaking分别建立资源边和验收：

1. `module`：通过 `exports` 和 ESM import 图解析。只保留从选定 roots 可达的模块。
2. `style`：绑定元数据或上游 package 明确声明 CSS 入口；组件样式和全局基础样式分开建边。未选组件的 CSS 不进入输出。
3. `worker`：worker URL、worker entry 和构造参数记录为显式资源关系；动态字符串使用绑定提供的静态资源映射或进入诊断分流。
4. `static`：字体、图片、主题、wasm 等资源记录 owner、相对路径、hash 和引用关系；profile 闭包只包含这些关系可达的文件。
5. `sideEffects`：依据上游 `package.json` 或绑定 inventory 的明确声明计算；含 CSS 导入、全局注册、polyfill 或运行时初始化的文件进入副作用集合。

NetPack 的 tree shaking 删除未使用且属于无副作用集合的模块；Emit 按组件和全局基础样式的资源边生成 CSS 闭包，保留必要的副作用模块。

## 责任分层

| 层 | 责任 | 交付边界 |
| --- | --- | --- |
| `ECMAScript.*` binding/generator | 强类型 API、来源/入口/资源 metadata、版本和 fingerprint | 维护 binding contract 和上游 inventory，提供 Emit 可消费的 package identity |
| `Jazor.Compiler` | C# 语义、import 收集、最终逻辑 specifier 和 export 使用 | 提供稳定的 C# import roots 与 export 使用信息 |
| `Jazor.Emit` | 收集实际 roots、生成 `package.json`、校验 lock/来源/闭包、物化 embedded package、输出稳定 profile | 交付入口映射、资源边、package project 和确定性诊断；第三方 ESM 语义由标准 resolver 保持 |
| `Jazor.Emit`（由 MSBuild 调用） | 生成 package project，执行首次或 frozen `deno install`，生成/验证 `deno.lock` 和 `node_modules` | 在同一 staging 事务内完成恢复、NetPack 输入准备和提交 |
| NetPack | 在 `jazor` 工作根下解析 package exports/conditions/sideEffects，并生成 Web/Deno bundle；入口由 Emit roots 提供 | 复用标准 package graph 和显式资源闭包 |
| `DenoHost` / `Jazor.AspNetCore` | 读取已恢复的 `jazor/node_modules`，以 `--no-remote`、`--frozen-lockfile` 执行 SSR | 使用 Emit 已验证的依赖图完成服务端渲染 |

## 分阶段迁移

### 阶段 A：基线和 metadata schema

责任 owner：`Jazor.Emit`、`Jazor.Common`、绑定生成器维护者。

- 盘点所有 `ECMAScript.*` binding 的 JS、CSS、worker、static、peer 和当前 manifest；逐包标记 `npm`、`jsr`、`embedded-mjs`。
- 冻结来源、入口、export、条件、sideEffects、resource edge、版本和 integrity 的 schema；schema 带版本，并在未知必需字段出现时产生稳定诊断。
- 增加“一个逻辑入口只对应一个来源 identity”的冲突诊断，generator、metadata 和 package writer 对同一入口保持一致定义。
- 建立 npm、JSR、embedded mjs、组件内部依赖和 CSS 的最小 fixture，先证明 graph/identity/diagnostics，再改实际绑定。

退出条件：metadata 可以表达三个来源和资源闭包；对缺入口、重复版本、peer 冲突、动态 worker 和全量 UMD 入口有稳定失败原因。

### 阶段 B：Emit 生成项目和 MSBuild restore

责任 owner：`Jazor.Emit`、MSBuild targets 和 `DenoHost` runtime。

- 新增 package project writer，生成 `jazor/package.json`，保留兼容的 `package-lock.json`，外部 npm/JSR 依赖通过 package project restore 进入 `node_modules`。
- 实现首次 restore：`deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=false`；成功后记录 `deno.lock` 的来源、版本和 hash。
- 实现后续 frozen restore：`deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true` 与 `deno check --node-modules-dir=manual --no-remote --frozen-lockfile`，所有 SSR 入口复用同一工作根。
- MSBuild 调用 Emit 完成恢复；SSR 使用本地 `node_modules`、`--no-remote` 和 frozen lock，不承担 restore。

退出条件：fresh restore、已有 lock 的 offline restore、错误 lock、缺包和 SSR 失败都能在写最终输出前明确报告；同一输入重复生成 package/lock staging 的内容稳定。

### 阶段 C：NetPack 直接消费 `jazor/node_modules`

责任 owner：`Jazor.Emit`、NetPack 集成层和发布脚本维护者。

- 把 NetPack 的 project/work root 固定到 `jazor`，或通过显式 root 参数保证从入口向上找到 `node_modules`。
- `Platform.Web` 统一解析 `package.json` 的 `exports`、条件导出、`sideEffects` 和 peer dependency，所有入口解析结果记录在 bundle 诊断中。
- 以 fixture 证明命名导入、子路径导入、共享依赖和 package 内部相对导入都得到稳定 bundle；`Platform.Deno` 复用同一包根和条件选择。
- `LibraryPackageWriter` 处理 embedded mjs 的标准本地 package materialization，外部 npm/JSR 依赖由 package project restore 处理；两条路径共享 identity/冲突校验。

退出条件：浏览器和 SSR 从同一个 `jazor/node_modules` 得到一致的包版本和入口；改变工作根、删除 node_modules 或绕过 exports 的行为都有明确诊断。

### 阶段 D：三类 binding 逐包迁移

责任 owner：对应 `ECMAScript.*` binding owner，Emit 和 DenoHost 提供共同门禁。迁移顺序按风险递增：

1. 纯 ESM、无 CSS 的函数/composable 包，验证 named export 和共享依赖。
2. TDesign、Element Plus、Vuetify 等组件包，验证 per-component `exports`、组件内部依赖、Vue peer、组件 CSS 和全局基础 CSS。
3. VueDataUi、图表、编辑器和上传类包，验证多入口、worker、字体、图片、wasm 和动态资源边。
4. 仍需要随包携带 `mjs` 的库，迁移为 `embedded-mjs` 标准本地 package；保留按入口解析的 carrier 模块。
5. 新增 binding 统一提供可验证的 ESM 入口；上游只有全量 UMD/browser bundle 时，为其登记兼容 runtime profile，并记录后续 ESM 入口计划。

迁移完成后，包目录保留 package metadata、许可证和明确声明的 carrier；历史绑定 `dist` 产物从当前交付物中移除。包级完成后继续通过独立 package consumer 和适用的浏览器/SSR 证据。

### 阶段 E：清理旧路径和更新契约

责任 owner：`Jazor.Emit`、各 binding、文档与发布维护者。

- 所有绑定迁移并通过矩阵后，收束只服务于旧全量复制路径的实现分支和历史字段。
- 持续同步[类库资源与引用契约](../02-architecture/library-artifact-contract.md)、[产物管线](../02-architecture/artifact-pipeline.md)、[JS 资源库绑定指南](../03-guides/js-resource-binding.md)、`current-status.md` 和 CHANGELOG；每个来源的 package metadata、integrity、入口和资源边保持可审计。
- 更新 NuGet、源码 ProjectReference、Release、SSR、HMR 和 NetPack consumer 的包布局与故障诊断。
- 保留迁移前后的字节、入口、版本和资源闭包报告，便于回归和发布审计。

## 验收矩阵

以下矩阵是每个 binding 和最终主线的最低验收集合：

| 场景 | 必须证明 |
| --- | --- |
| 源码 ProjectReference | Emit 生成 package project，Deno restore 成功，NetPack/SSR 使用相同 roots |
| NuGet package consumer | 在独立 package consumer 路径发现 manifest/metadata、package project 和 embedded carrier |
| npm binding | 精确版本、integrity、exports、peer dependency 和 `node_modules` 入口一致 |
| JSR binding | Deno 与 NetPack 使用同一规范化入口和可审计字节，offline/frozen 行为一致 |
| embedded mjs | 从明确声明的本地 carrier 物化选中模块和资源，标准 package exports 可解析，输出闭包由入口和资源边确定，并能连接 npm/JSR 依赖 |
| Tree shaking | fixture 同时导入已使用和未使用 export；bundle 中只保留可达代码，组件内部共享依赖仍只保留一份 |
| CSS | 已使用组件和必要基础 CSS 存在，未使用组件 CSS 不存在；CSS side effect 不被错误删除 |
| worker/static | 声明的 worker、字体、图片、wasm 能解析；动态且不可证明的资源显式失败 |
| Deno restore | 首次 `deno install ... --frozen=false` 生成可复现 `deno.lock` 和 `node_modules` |
| Deno offline | frozen `deno install`、`deno check ... --no-remote --frozen-lockfile` 和 SSR 不联网通过 |
| SSR | 使用同一 `jazor/node_modules` 运行；依赖缺失/版本冲突进入稳定诊断 |
| NetPack Web | `Platform.Web` 从 `jazor` 根解析 exports/sideEffects，产物可运行且 tree shaking 结果稳定 |
| 条件导出 | browser、deno、development、production 条件选择明确，并在不同 profile 中保持确定性 |
| 冲突与失败 | 重复 identity、peer 版本冲突、integrity/hash、路径越界、缺入口/依赖在物化前失败 |
| 确定性 | 相同输入、SDK、Deno 和 NetPack 版本下 package、lock 投影、manifest、bundle 和 source map 字节稳定 |

应覆盖绑定专属测试、`Jazor.EmitTest` 的 package/closure/materialization 回归、MSBuild Emit restore/check/SSR 回归、NetPack Web bundle fixture、真实 RazorVue package consumer 和适用 Chromium smoke。测试脚本继续使用仓库规定的单文件 C# 入口，不新增 PowerShell 自动化。

## 实施路径与诊断

- **NetPack 的 JSR 解析条件**：JSR binding 进入 Support 时，NetPack 原生解析或经过 hash/exports 校验的标准本地 JSR adapter 与 DenoHost 指向同一份可审计字节；该结果写入 JSR 包级验收证据。
- **上游模块形态**：绑定 metadata 记录上游 ESM export 边界。上游提供 UMD 或完整 browser bundle 时，绑定交付兼容运行时闭包；上游或 adapter 提供 ESM 入口后，迁移到入口级 tree shaking，并在 inventory 中记录两种形态的来源。
- **CSS/worker 动态资源**：绑定为动态资源建立稳定映射，声明入口、owner、hash 与 profile 关系；物化器按照这些关系生成资源闭包，并在映射缺失时输出带 specifier 的诊断。
- **peer dependency 版本选择**：package resolver 为每个 peer 选择唯一版本，记录 owner、版本与 profile，并将共享运行时纳入同一依赖图。
- **Lock 文件协作**：完整的 `package-lock.json` 记录 npm 兼容输入；外部依赖图由 `deno.lock` 记录 Deno frozen graph。Emit 在 restore 前决定是否生成 npm lock，restore 完成后校验 `deno.lock` 与 `package.json` 的依赖 identity，再执行 SSR/NetPack。
- **迁移 profile**：每个 binding 通过明确的 profile 选择兼容 carrier 或标准 package carrier；profile 状态、版本、入口和诊断保留在迁移记录中，便于逐包切换和回滚。

## 迁移与恢复边界

迁移以 binding 和 profile 为单位可回滚：

1. 任一包的新 metadata、Deno restore 或 NetPack consumer 进入诊断状态时，保留该包的 package metadata、上游快照和可复现的本地 carrier，并在迁移记录中标记待处理 profile。
2. Emit 输出采用 staging + 原子提交；package/lock/node_modules 每个阶段都在同一事务中完成，失败时继续提供上一份有效 `jazor` 目录。
3. carrier materializer 和文档契约在源码 consumer、NuGet consumer、browser/SSR profile 与 release 验证完成后统一收束，过程版本和 fingerprint 保留在迁移记录中。
4. 每个新 profile 绑定一份对应的 `package.json`、`deno.lock` 和 `node_modules`，发布与运行时从同一 profile identity 读取依赖。

## Definition of Done

本计划完成必须同时满足：

- 所有纳入范围的 binding 都有来源类型、精确版本、入口/export、条件、peer、CSS/worker/static 和 license metadata；npm、JSR、embedded mjs 均至少有一个真实包级实现。
- Emit 能生成上述 `jazor` 项目并调用 DenoHost runtime 完成首次 restore、`deno.lock` 冻结和 offline check；SSR 使用同一 frozen graph。
- NetPack 能以 `jazor` 为工作根消费同一份 `node_modules`，`Platform.Web` 的 exports/sideEffects tree shaking 经过可观察 bundle 断言；组件内部依赖和共享依赖没有语义回归。
- 未使用 JS 方法、组件和 CSS 在 release bundle 中被删除；必要的 side effect、CSS、worker 和 static 资源仍存在且来源可追踪。
- 源码 ProjectReference、NuGet consumer、Debug/Release、SSR/HMR、离线/frozen lock 和真实浏览器路径通过验收矩阵；失败诊断稳定且不静默回退。
- 架构文档、作者指南、测试门禁、发布脚本、CHANGELOG 和 `current-status.md` 已同步；新 binding 默认携带标准 package metadata，`manifest.json`、`inventory.json` 与显式来源 carrier 通过对应 profile 保持可复现。

本计划状态保持 **进行中（P0-5）**，细粒度 tree shaking、JSR 跨工具支持和 embedded-mjs 标准 package carrier 按验收矩阵逐项转入 Support 能力。
