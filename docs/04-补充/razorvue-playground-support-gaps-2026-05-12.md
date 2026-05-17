# RazorVue Playground 支持缺口记录（2026-05-12）

## 背景

`src/Playground` 是一个真实案例，不是演示玩具。它按如下路线落地：

- 单 ASP.NET Core 项目作为唯一运行时宿主
- RazorVue library mode 产出 `.vue` SFC
- consumer 使用 DenoHost 路线消费生成产物
- UI 技术栈为 `Vuetify + Pinia + Vue Router`

这个过程暴露出若干当前不支持点或高摩擦点，需要明确记录，作为后续能力提升项。

## 1. RazorVue SFC default export 不能进入 Jazor 编译器边界

### 现象

Vue SFC 生态默认以 `default export` 表达组件，但 Jazor authored C# module 路线明确不支持：

- default export emit
- default import consume

这个边界是刻意保留的，不计划通过扩展 Jazor 编译器来支持 default export/import。

### 当前影响

`.vue` 不能作为 Jazor authored module 直接消费的模块边界。真实项目需要一个 build-time bridge，把 Vue SFC 的 default component 语义转换成 Jazor 可接受的 named export/import 语义。

### 当前落地方式

`Playground` 采用：

- ASP.NET Core 项目开发/测试阶段输出根 `jazor/*.vue`
- 发布阶段将根 `jazor` 物化到 `wwwroot/jazor`
- `Jazor.Emit razorvue-consumer-entry` 读取 manifest 和 `.vue`，生成 browser/SSR entry modules
- Deno pipeline 打包生成的 browser entry 到 `wwwroot/jazor/client-entry.*`
- `Jazor.Emit razorvue-sfc-bridge` 编译 `.vue` 后输出 named-export bridge module，例如 `export { _sfc_main as PlaygroundCatalogPage }`
- consumer 入口和组件间引用都使用 named import，例如 `import { PlaygroundCatalogPage } from "./pages/playground-catalog-page.mjs"`

### 当前保护

- SFC named-export bridge 已收敛为 `Jazor.Emit` 的官方 host-facing build target，而不是 Playground 私有 workaround。
- `src/Jazor.Emit/Deno/razorvue-sfc-bridge.ts` 负责 Vue SFC 编译、default export 转 named export、相对 `.vue` import 转 `.mjs` named import、CSS 输出，以及 browser/SSR 模式差异。
- `src/Jazor.Emit/RazorVueSfcBridgeCompiler.cs` 通过 DenoHost 在隔离 workspace 中执行 bridge，避免依赖调用方目录中的 `deno.json` 或全局 Deno。
- `src/Jazor.EmitTest/RazorVueSfcBridgeCompilerTests.cs` 覆盖 named export 输出、相对 `.vue` default import 改写、SSR 模式不注入 CSS import、非法 component export name 和 manifest 缺失错误。
- `Playground` consumer 只调用官方 consumer entry 生成命令，并通过 `JAZOR_EMIT_TOOL_PATH` 在 MSBuild 中复用当前 `Jazor.Emit.dll`，避免维护本地 SFC 编译和 manifest/entry 拼接副本。

若未来 authored Jazor module 需要引用 RazorVue 组件，也应引用 bridge module 的 named export，而不是直接引用 `.vue` default export。

## 2. RazorVue Razor IR frontend 对某些静态 HTML attribute 形态仍然脆弱

### 现象

真实案例中遇到：

- 静态多 token `class` 值在 Razor IR frontend 中被识别为 mixed attribute content
- 从而触发 `ResolveAttributeValue(...)` 路径拒绝

典型现象是本来语义上完全静态的：

```razor
class="playground-page playground-page--catalog"
```

该问题已在 Razor IR frontend 中修复：当 attribute value 的多个 Razor IR child 都能证明为静态 literal 时，前端会按 Razor IR 的 `Prefix`/token 内容拼接成一个静态字符串，而不是直接判为 mixed content。

### 当前影响

真实项目不再需要把纯静态 class 设计从多 token 写法改成单 token 规避，例如：

- `playground-page playground-page--catalog`
- 不再需要改成 `playground-page-catalog`

动态 mixed attribute content 仍然不在当前支持边界内，例如静态 literal 与 `@Title` 表达式混写的 `class="todo-card @Title"` 仍会明确失败。这个边界是有意保留的，避免把真实动态内容误降级为静态字符串。

### 相关代码

- `src/Jazor.RazorVue/RazorSdk/RazorVueRazorIrTemplateFrontend.cs`
- `src/Jazor.RazorVue/RazorSdk/RazorVueReflectedRazorIrReader.cs`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`

### 当前落地方式

- `RazorVueReflectedRazorIrReader` 读取 Razor IR attribute value 的 `Prefix`/`Suffix` 元数据。
- `ResolveAttributeValue(...)` 在多 child 场景下先尝试静态 literal 拼接。
- 只有 `HtmlContent`、`HtmlAttributeValue` 和非 C# `IntermediateToken` 会参与静态拼接。
- 一旦出现 C# expression、C# token 或未知节点，仍按 mixed attribute content 抛出显式错误。

## 3. library component 上原样 authoring `class=` / `style=` 已落地稳定契约

### 现象

按设计，带 `[Parameter(CaptureUnmatchedValues = true)]` 的 library component 应该支持 fallthrough attributes。

仓库中已有测试和文档也说明：

- `class`
- `style`
- `data-*`
- `aria-*`

应当可以透传。

`Playground` 的真实 authoring 过程中曾经遇到：在组件标签上写 lowercase raw attribute：

```razor
<VChip class="playground-category-chip" ... />
```

会被官方 Razor Source Generator 绑定到组件的 `Class` 参数，而不是作为 unmatched fallthrough attribute 进入 `AdditionalAttributes`。由于 `Class` 参数类型是 `VueClassValue?`，HTML-style 字符串 literal 不能按 Razor SG 规则直接绑定到该非字符串参数，最终会生成错误的 C#。

### 当前落地方式

该问题已在 authoring surface 层修复：

- 组件标签上的 lowercase `class` / `style` 走 `AdditionalAttributes` fallthrough，保持 Razor SG 原生可编译。
- 强类型 C# authoring 入口统一使用 `CssClass` / `CssStyle`，通过 `[VueProp(..., Name = "class" / "style")]` 映射到 Vue runtime prop。
- 不再在 top-level Vuetify authoring component 上暴露 `[Parameter] Class` / `[Parameter] Style`，避免与 Razor SG 的 lowercase attribute 绑定规则冲突。

因此真实项目可以恢复自然写法：

```razor
<VChip class="playground-category-chip" ... />
```

需要强类型表达式时则使用：

```razor
<VChip CssClass='@("playground-category-chip")' CssStyle='@("margin-inline: 1rem")' ... />
```

`CssClass` / `CssStyle` 仍会输出到 Vue 的 `class` / `style`，不会改变运行时语义。

### 相关参考

- `src/Jazor.RazorVue/Lowering/RazorVueCaptureUnmatchedAttributePolicy.cs`
- `src/ECMAScript.Vuetify/README.md`
- `src/Jazor.RazorVue.RazorIr.Test/RazorVueRazorIrTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/VuetifyAuthoringSurfaceTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`

### 当前保护

- Razor IR frontend 覆盖 raw `class=` fallthrough 与 `CssClass` / `CssStyle` 强类型映射。
- Vuetify authoring surface 测试禁止组件参数重新暴露 `Class` / `Style`。
- Playground 已使用自然 raw `class="playground-category-chip"` 作为真实集成验证点。

## 4. 单项目 library-mode 实际上仍需要 consumer 构建层

### 现象

从产品形态上看，`Playground` 已经满足“不要拆成 app 和 host”的要求，因为运行时只有一个 .NET 项目。

但从构建链角度，仍然必须存在一个 consumer 层去：

- 读取 RazorVue manifest
- 编译 `.vue`
- 组装 `Pinia` / `Vue Router` / `Vuetify`
- 输出浏览器 bundle

### 当前影响

“单项目”目前能做到的是：

- 单 .NET 项目
- 同仓库内 colocated consumer
- MSBuild target 自动调用 Deno consumer 构建浏览器资产

而不是“完全不需要任何前端 consumer”。

### 当前保护

- `Jazor.Emit razorvue-consumer-entry` 已成为官方 build-time consumer entry 生成入口。
- 该命令统一负责读取 `jazor-manifest.json` 中的 RazorVue component metadata、选择组件、调用 SFC named-export bridge、生成 browser/SSR bridge modules，以及写出 `client-entry.mjs` / `ssr-entry.mjs` / `vue-feature-flags.mjs`。
- 组件选择使用显式 `--component Alias=selector` 契约，selector 支持 `id:`、`name:`、`path:`；模糊匹配会失败并要求显式 selector，避免真实项目在组件重名时静默选错。
- consumer runtime 不再需要解析 manifest 或知道 `.vue` default export 转换细节，只接收 `razorVueConsumerComponents` 和 `razorVueHostRequirements`。
- `src/Jazor.EmitTest/RazorVueConsumerEntryCompilerTests.cs` 覆盖 browser/SSR entry 生成、CLI 参数解析、组件选择歧义错误和 clean 模式误删保护。
- `Playground` 的 `consumer/scripts/lib/pipeline.ts` 已改为调用官方 entry 生成命令，私有脚本只保留最终 Deno bundle、HTML dist 和 smoke verification。

### 后续提升方向

- 已收敛为 `Jazor` SDK/MSBuild contract：项目通过 `JazorConsumerRoot`、`JazorConsumerRunScriptPath`、`JazorConsumerBuildTask`、`JazorConsumerBrowserAssetRoot` 等声明式属性启用 colocated consumer build，不再手写项目私有 `Exec` target。
- `src/Jazor/buildTransitive/Jazor.targets` 现已官方提供 consumer build 与 publish materialization 组合能力；`Playground` 只保留配置，`SdkIntegrationTests` 覆盖 package consumer 场景下的 build/publish merge 行为。
- 后续仍需要提供 ASP.NET Core + RazorVue library mode 标准模板，明确 colocated consumer 目录、runtime entry、bundle 输出和 publish 合并策略。

## 5. ASP.NET Core fallback 不能使用 catch-all endpoint 抢占静态文件

### 现象

早期宿主验证时，`/assets/client-entry.js` 文件存在于 `wwwroot/assets`，但返回 404。后续按 Playground 的 `/jazor/*` 统一资源边界改为 `wwwroot/jazor/client-entry.*`，但该中间件顺序问题仍然成立。

根因是 endpoint routing 中的：

```csharp
app.MapMethods("/{**path}", ["GET", "HEAD"], ...)
```

会先为 `/assets/*` 选择 catch-all endpoint，导致 `StaticFileMiddleware` 因已有 endpoint 而不处理请求，最终落入 fallback 逻辑。

### 当前落地方式

`Playground` 已改为使用 `Jazor.AspNetCore` 官方 `UseJazorSpaFallback(...)` middleware 处理 HTML shell fallback：

- 先通过 `UseJazorWebAssets(...)` 挂载标准静态资源和开发期 `/jazor/*` 资产
- 再挂载 `UseJazorSpaFallback("/index.html")`
- 不使用 `MapMethods("/{**path}", ...)`、`MapFallbackToFile(...)` 等 endpoint catch-all 作为 SPA fallback

### 当前保护

`UseJazorSpaFallback(...)` 的默认行为面向生产宿主安全边界收窄：

- 只处理 `GET` / `HEAD`
- 默认要求 `Accept` 包含 `text/html` 或 `application/xhtml+xml`
- 所有带文件扩展名的路径都不 fallback，避免缺失静态文件被改写成 HTML
- 默认排除 `/api`、`/assets`、`/health`、`/jazor`
- 支持通过 `JazorSpaFallbackOptions.ExcludedPathPrefixes` 添加项目自定义排除前缀
- 在调用后续 pipeline 后只对未被 endpoint 选中、未开始响应、最终仍为 404 的导航请求写入 shell
- endpoint 自己返回的 404 保持 404，不会被 SPA shell 覆盖

### 后续提升方向

- 将 `UseJazorSpaFallback(...)` 纳入 ASP.NET Core + RazorVue library mode 标准模板
- Wiki 等已有站点后续可以按同一契约迁移，减少宿主私有 fallback 分类逻辑

## 6. 发布内容根不能固定到源码路径

### 现象

为了让 `dotnet run --project ...` 从仓库根启动时找到源码 `wwwroot`，一开始使用 `CallerFilePath` 固定 `ContentRootPath`。

该方式在发布包中有风险：发布后如果仍在同一台机器运行，宿主可能继续指向源码目录，而不是发布目录。

### 当前落地方式

`Playground` 已改为使用 `Jazor.AspNetCore` 官方 `JazorWebApplication.CreateBuilder(args)` 创建宿主。该 helper 的内容根解析策略是：

- 若 `AppContext.BaseDirectory/wwwroot` 存在，优先使用发布/输出目录
- 否则回退到 `Program.cs` 所在源码目录

因此 `Program.cs` 不再手写 `WebRootPath`、`PhysicalFileProvider` 或 `CallerFilePath` 内容根解析，普通 `wwwroot` 静态文件继续交给 ASP.NET Core 默认 web root 机制处理。

### 后续提升方向

- 已完成（2026-05-14 本轮）：`JazorWebApplication.ResolveContentRootPath(...)` 已由 `JazorAspNetCoreHostingTests` 独立锁定“发布/输出目录存在 `wwwroot` 时优先使用 `AppContext.BaseDirectory`，否则回退源码目录”的双分支语义，避免后续把内容根解析悄悄漂回固定源码路径。

## 7. 浏览器 bundle 与 RazorVue emit 复用 `/jazor/*` 需要明确合并语义

### 现象

`Playground` 需要让最终浏览器 bundle 也直接落在 `wwwroot/jazor`，而不是单独使用 `wwwroot/assets`。这会让发布目录中同一个 `/jazor/*` 路径同时包含：

- 根 `jazor` emit 复制出的 manifest、SFC 和 CLR runtime modules
- Deno consumer 生成的 `client-entry.js`、`client-entry.css` 与 sourcemap

### 当前落地方式

- 本地 build 阶段：根 `jazor` 仍是 RazorVue emit 源，`wwwroot/jazor` 只承载浏览器 bundle。
- 本地宿主阶段：`UseJazorWebAssets(...)` 先服务 `wwwroot` 静态文件，再服务根 `jazor` development assets，确保 `/jazor/client-entry.*` 和 `/jazor/jazor-manifest.json` 都可访问。
- publish 阶段：先清空发布 `wwwroot/jazor`，复制根 `jazor` emit，再复制 `wwwroot/jazor/client-entry.*`，最终发布包只从 `wwwroot/jazor` 服务 `/jazor/*`。
- consumer 中间 build root 默认按进程隔离为 `.deno-build/pid-*`，避免 `smoke:ssr`、`smoke:browser` 或 CI 并行任务互相清理同一目录。

### 后续提升方向

- RazorVue / ASP.NET Core 集成层已提供官方 publish 合并 target：`JazorPublishMaterializeEnabled=true` 负责将开发输出根物化到发布 `wwwroot/jazor`，`JazorPublishConsumerBrowserAssets` 负责把 colocated consumer browser bundle 合并到同一路径并清理影子目录。
- `UseJazorWebAssets(...)` 后续可以继续扩展为更完整的 RazorVue library-mode 宿主模板入口。

## 8. 已完成：统一 manifest 与宿主 API 收敛

### 目标

`Playground` 和 `Wiki` 只是两个当前验证项目，真实项目可能同时包含普通 Jazor H 函数模块、RazorVue H 组件、RazorVue SFC 组件、浏览器 bundle、SSR bridge 和自定义 host shell。

因此不能继续通过项目私有约定、文件名分裂或样例特化 option 来区分产物类型。需要把输出契约收敛为一个默认可运行、可组合、可扩展的标准宿主模型：

- 默认配置即可启动标准 Jazor 输出
- 高级场景通过 option 扩展，不要求每个项目手写必需配置
- `Playground` 与 `Wiki` 使用同一组 `Jazor.AspNetCore` helper
- manifest 只保留一个公开文件名：`jazor-manifest.json`
- 组件语义写入 manifest module metadata，而不是另起 `jazor-manifest-razorvue.json`

### 已确认的 manifest 契约

统一 manifest 文件名固定为：

```text
jazor-manifest.json
```

旧文件名废除，不再作为默认探测、默认输出或公开文档入口：

```text
jazor-manifest-razorvue.json
```

`Modules` 中每个 module 使用两层判别：

- `kind` 表示实际产物文件形态，当前取值为 `mjs` 或 `vue`
- `component.model` 表示组件 authoring/runtime 模型，当前取值为 `h` 或 `sfc`
- 没有 `component` 的 `mjs` 是普通 Jazor/ECMAScript module，不应被当作 RazorVue component

约定示例：

```json
{
  "kind": "mjs",
  "relativePath": "components/wiki-home.mjs"
}
```

```json
{
  "kind": "mjs",
  "relativePath": "components/counter-card.mjs",
  "component": {
    "model": "h"
  }
}
```

```json
{
  "kind": "vue",
  "relativePath": "components/counter-card.vue",
  "component": {
    "model": "sfc"
  }
}
```

`kind` 不再使用 `ecmascript` / `razorvue` 这类来源或技术线命名。文件形态用 `mjs` / `vue`，组件模型用 `h` / `sfc`，避免把普通 H 函数模块、H 组件和 SFC 组件混在同一个维度。

### Emit 侧工作项

- 修正 `ManifestModel`，将当前半迁移状态中的 `RazorVue` metadata 改为通用 `Component` metadata。
- 增加 `ManifestComponentModel.H` / `ManifestComponentModel.Sfc` 常量，并保持 `ManifestModuleKind.Mjs` / `ManifestModuleKind.Vue` 只表达文件形态。
- `ModuleWriter` 写普通 `.mjs` manifest 时必须保留已有 component entries，clean 只清理自己负责的普通 module，不能误删 RazorVue component manifest 项。
- `RazorVueModuleWriter` 写 H 组件时合并到统一 `jazor-manifest.json`，产物为 `kind = "mjs"`、`component.model = "h"`。
- `RazorVueSfcModuleWriter` 写 SFC 组件时合并到统一 `jazor-manifest.json`，产物为 `kind = "vue"`、`component.model = "sfc"`。
- `RazorVueManifestSerializer.TryLoad(...)` 需要支持从统一 manifest 中投影 `modules[].component` 到现有 `RazorVueManifestModel`，让 diff、bundle、consumer entry 在内部模型未完全替换前仍可稳定复用。
- `ModuleBundler` 不再读取 `RazorVueModuleWriter.GetManifestPath(...)` 产生的第二 manifest，而是从统一 manifest 投影 RazorVue component metadata。
- 已完成（2026-05-13 本轮）：`ModuleCollector` 允许同一 emit run 同时收集 RazorVue H catalog 与 SFC catalog，并保留“单程序集只能暴露一种 catalog shape”保护；跨模型 `componentId` / `relativePath` 冲突会显式失败。
- 已完成（2026-05-13 本轮）：`Jazor.Emit` 主流程不再在 H 与 SFC 之间二选一；clean/write 会分别处理 H 与 SFC，并在最终统一 manifest 基础上收敛生成单一 `__jazor/razorvue-host.mjs`。
- 已完成（2026-05-13 本轮）：`RazorVueModuleWriter` / `RazorVueSfcModuleWriter` 的宿主 metadata 生成逻辑收敛为共享 `RazorVueHostRequirementsModuleWriter`，避免 mixed 场景下 host requirements 被最后一个 writer 覆盖。
- 已完成（2026-05-13 本轮）：`ModuleBundler` 仅将非 component entries 视为 bundle 输入模块，RazorVue host requirements 则直接从统一 manifest 生成，不再依赖输入目录中预先存在的 `__jazor/razorvue-host.mjs`。
- 已完成（2026-05-14 本轮）：`ModuleWriter` 在统一 manifest 下已收敛为“普通 `.mjs` 仅保留未被同路径 plain module 接管的 component entries”；当 plain module 接管旧 RazorVue H 路径时，会同步移除旧 `component` metadata 与 `.origins.json` sidecar，避免后续 RazorVue clean 把新 plain module 误删。
- 已完成（2026-05-14 本轮）：MSBuild target 已移除公开的 `JazorRazorVueManifestPath` 默认值和 `jazor-manifest-razorvue.json` 引用；bundle 前快照统一收敛为 `JazorPreviousManifestSnapshotPath -> previous-jazor-manifest.json`，并由 `Jazor.targets` 直接传给 `bundle --previous-manifest`。
- 已完成（2026-05-14 本轮）：`razorvue-diff`、bundle update plan 与 host asset sidecar 现已统一从 `jazor-manifest.json` 的 component projection 读取输入；`ModuleBundlerTests` / `RazorVueManifestDifferTests` / `SdkIntegrationTests` 已锁定“projection missing / invalid”语义，避免回退到第二 manifest 契约。

### Consumer 与 SFC bridge 工作项

- `razorvue-consumer-entry` 默认 manifest 路径改为 `<hostJazorRoot>/jazor-manifest.json`。
- `razorvue-sfc-bridge` 默认 manifest 路径改为 `<hostJazorRoot>/jazor-manifest.json`。
- Deno bridge 读取统一 manifest shape，并只处理 `kind = "vue"` 且 `component.model = "sfc"` 的 module。
- consumer entry 组件选择逻辑只在 component entries 中匹配，普通 `mjs` module 不参与 `id:` / `name:` / `path:` component selector。
- consumer entry 生成 `razorVueConsumerRoutes`，其数据源是 selected component 的 `routeTemplates`；Playground 这类 consumer runtime 不再把 `router.js` 中的手写 path table 作为路由真相源。
- 当前 route template 到 Vue Router path 的官方支持边界已收窄并显式化：支持纯 literal segment、纯 `{parameter}` segment 和 `{parameter?}`；constraint、catch-all、default value、mixed segment 会在 consumer entry 生成阶段直接失败。
- 错误信息应继续明确区分“manifest 不存在”“manifest 没有组件”“selector 无匹配”“selector 匹配多个组件”，不能因为统一 manifest 降低诊断质量。
- Playground colocated `consumer` 目录继续保留；它是单 .NET 项目中的前端消费构建层，不是第二个运行时 host。命名上使用 `consumer`，不再使用 `playground-consumer` 这类项目特化名称。
- 已完成（2026-05-13 本轮）：`razorvue-consumer-entry` 在 mixed H/SFC 场景下按 `component.model` 分流，H 组件直接 default import host `.mjs`，SFC 组件才进入 bridge。
- 已完成（2026-05-13 本轮）：`razorvue-consumer-entry` 只把“被选择的 SFC 组件集合”传给 `razorvue-sfc-bridge`，不再因为 manifest 中未选中的坏 SFC 而整体失败。
- 已完成（2026-05-13 本轮）：`razorvue-sfc-bridge` 支持显式 entry module path 过滤，并保留相对 `.vue` 依赖闭包编译，保证选中的 SFC 组件间引用仍能稳定工作。
- 已完成（2026-05-14 本轮）：统一 manifest 的 RazorVue component projection 诊断已细分为“manifest 不存在”“manifest 存在但没有 RazorVue component entries”“selector 无匹配”“selector 匹配多个组件”；`razorvue-diff` 的缺失原因也改为统一 Jazor manifest projection 语义，避免继续泄漏废弃的第二 manifest 公共契约。
- 已完成（2026-05-14 本轮）：`Playground` consumer runtime 已移除缺失 `routeDefinitions` 时的 legacy 手写路由回退；运行时现在必须消费 `razorvue-consumer-entry` 生成的 `razorVueConsumerRoutes`，确保 Razor `@page -> unified manifest routeTemplates -> consumer runtime` 是唯一路由真相源。
- 已完成（2026-05-14 本轮）：sample / pure Deno consumer runtime 已与官方 `razorvue-consumer-entry` 的三参调用契约对齐；运行时会把第 3 个参数识别为 route metadata，而不是误当成 `app.mount(...)` selector，修复了浏览器 smoke 中的 `parent.insertBefore is not a function` 挂载错误。
- 已完成（2026-05-14 本轮）：SDK colocated consumer 模板运行时也已对齐同一三参契约；`Publish_LocalJazorPackage_WebSdkHost_WithColocatedConsumer_UsesSdkConsumerBuildAndUnifiedJazorPublishRoot` 现会回归锁定 `razorVueConsumerRoutes` 调用与 `Array.isArray(routesOrSelector)` 兼容逻辑，防止模板再次漂移回旧 selector-only 签名。
- 已完成（2026-05-14 本轮）：sample 与 external pure Deno 的 SSR runtime export 也已显式声明并透传第 3 个 `razorVueConsumerRoutes` 参数，避免“浏览器 runtime 已升级、SSR runtime 仍停留在旧双参签名”的模板契约漂移。
- 已完成（2026-05-14 本轮）：Playground consumer 的 browser/SSR runtime 入口已显式校验 `CatalogPage` / `DetailPage` 必需组件导出；当 selector 配置或生成入口退化时，会在入口层给出稳定错误，而不是等到更深层渲染/SSR 过程里以模糊异常失败。

### ASP.NET Core 宿主工作项

- `UseJazorWebAssets()` 默认只依赖标准 `jazor-manifest.json` 即可挂载开发期 `/jazor/*` 输出。
- `DevelopmentEntryModuleRelativePath = "jazor-manifest-razorvue.json"` 这类样例必需配置应移除；高级项目仍可通过 option 覆盖 readiness probe。
- `JazorDevelopmentAssetOptions` 默认探测列表移除旧 manifest 文件名，避免继续把废弃文件作为隐式契约。
- `wwwroot` 静态文件使用 ASP.NET Core 默认 web root 机制，不在 Playground/Wiki 私有代码里手写特殊处理。
- `JazorWebApplication.CreateBuilder(args)` 作为源码运行与发布运行的内容根 helper，可供 Playground 和 Wiki 共用。
- `UseJazorHost(...)` 作为默认宿主入口，统一挂载通用安全头、标准静态文件、source map content type、开发期 Jazor 输出；项目仅在需要时覆盖站点特有 cache/header 策略。
- `UseJazorWebAssets(...)` 继续作为更细粒度的低层挂载 API 存在，但不再要求 Playground/Wiki 这类标准宿主手写组合样板。
- `UseJazorSpaFallback(...)` 继续负责 SPA navigation fallback；Wiki 如果需要 SEO shell 和 discovery document，可以保留项目特定 HTML shell 逻辑，但不应复制静态资源挂载和 Jazor output 探测逻辑。
- `UseJazorSpaFallback("/index.html")` 这类静态页面回退应作为官方宿主 API 提供：标准 SPA 宿主可以直接复用 `wwwroot/index.html`，而不必总是手写 `HttpContext -> WriteHtmlAsync` 委托。
- 已完成（2026-05-13 本轮）：`JazorDevelopmentAssetOptions` / `JazorWebAssetOptions` 默认探测仅保留 `jazor-manifest.json`，`Playground` smoke 不再显式依赖旧 manifest 文件名是否 404。
- 已完成（2026-05-13 本轮）：Wiki 的 `/vendor/*` 长缓存策略已从项目私有 `OnPrepareResponse` delegate 收敛为 `UseJazorHost(...).WebAssets.ImmutableCachePathPrefixes` 声明式 option；标准宿主不再手写基础静态资源 header 逻辑。
- 已完成（2026-05-14 本轮）：`Playground` 与 `samples/RazorVue.TodoList/Todo.Host` 均已收敛到 `UseJazorHost()` + `UseJazorSpaFallback("/index.html")` 默认宿主契约；`JazorAspNetCoreHostingTests` 新增默认单宿主组合回归，防止样例重新退回私有 `SendFileAsync` fallback。
- 已完成（2026-05-14 本轮）：开发期输出探针的公共宿主 API 已收敛为 `DevelopmentOutputProbeRelativePath` / `DevelopmentOutputProbeRelativePaths`，默认即为统一 `jazor-manifest.json`；旧 `EntryModuleRelativePath` 仅保留显式弃用的兼容别名，不再作为推荐语义。
- 已完成（2026-05-14 本轮）：高层 `UseJazorHost(...).WebAssets` 入口现已补齐 `DevelopmentOutputProbeRelativePath` 单值配置，与低层 `UseJazorDevelopmentAssets(...)` 保持同一公共 probe 语义；`JazorAspNetCoreHostingTests` 已锁定该高层入口不会退回到只能手改 list 的半收敛状态。

### Playground / Wiki 一致性工作项

- `Playground/Program.cs` 精简为 builder、服务注册、安全头、`UseJazorWebAssets(...)`、SPA fallback、API endpoint，不再包含 manifest 文件名或 Jazor output 细节。
- `Playground/Program.cs` 现已进一步收敛为 builder、服务注册、`UseJazorHost()`、SPA fallback、API endpoint，不再维护项目私有静态资产/安全头样板。
- `Wiki/Program.cs` 迁移到同一组 `Jazor.AspNetCore` helper，避免与 Playground 使用不同 API 设计。
- Wiki 可保留 host-rendered HTML shell、robots/sitemap、路径基址和目录完整性校验；这些是站点语义，不是 Jazor web asset 基础设施。
- 两个项目都应以默认配置跑起来，差异只体现在项目语义 option，而不是基础 Jazor host 契约。

### 验收标准

- 全仓库不再有默认输出、默认探测、测试断言或 smoke 脚本依赖 `jazor-manifest-razorvue.json`。
- `jazor-manifest.json` 同时覆盖普通 `mjs` module、H component module、SFC `vue` module。
- manifest clean 不误删不同 writer 负责的 module entries 或文件。
- SFC bridge、consumer entry、bundle、update plan 都从统一 manifest 工作。
- Playground smoke 访问 `/jazor/jazor-manifest.json` 成功，且不需要显式配置旧 manifest 路径。
- Wiki 和 Playground 使用一致的 ASP.NET Core helper API。
- 相关测试覆盖 manifest schema、merge/clean、consumer selection、SFC bridge filtering、ASP.NET Core default hosting。

### 当前状态

该项现已完成并由回归锁定：

- 统一 manifest 公开契约已稳定为 `jazor-manifest.json`
- `jazor-manifest-razorvue.json` 仅作为文档中的废弃历史名称保留，不再参与默认输出、默认探测或默认宿主运行
- `UseJazorDevelopmentAssets()` 默认探针与 `UseJazorHost()` 默认宿主契约现都显式拒绝把旧文件名当成 development readiness probe
- `Playground` / `Wiki` 已收敛到同一组 `Jazor.AspNetCore` helper，只在站点语义层保留差异化 option

## 9. 当前处理结论

这些问题没有阻断 `Playground` 落地，但都属于真实生产标准下必须正视的能力边界。

建议优先级：

1. default import/export 与 SFC bridge
2. library-mode 单项目 consumer pipeline 的 SDK/MSBuild/template 封装
3. ASP.NET Core + RazorVue library mode 标准模板
4. `/jazor/*` 多来源合并能力官方化

已落地并由回归保护的项：

- Razor IR 对纯静态多 token attribute 的稳定接受
- library component raw `class=` / `style=` fallthrough authoring 体验修复
- RazorVue SFC named-export bridge 官方化
- RazorVue consumer entry generation 官方化切片
- colocated consumer MSBuild build/publish contract 官方化
- ASP.NET Core SPA fallback/static-file 官方 helper
- ASP.NET Core 源码/发布双形态 content root helper
- `/jazor/*` 本地 webroot bundle + development emit 标准挂载 helper

## 10. 已完成：handwritten `BuildRenderTree` 模板局部变量支持

### 现象

此前 handwritten `BuildRenderTree` 中只允许 `RenderTreeBuilder` 别名局部变量。真实 authoring 中常见的模板内局部缓存/别名声明会失败，例如：

```csharp
var localTitle = Title;
```

```csharp
foreach (var item in Items!)
{
    var decorated = item + "!";
    builder.OpenElement(0, "span");
    builder.AddContent(1, decorated);
    builder.CloseElement();
}
```

```csharp
builder.AddAttribute(1, nameof(ChildCard.ItemTemplate), (RenderFragment<int>)((item) => (slotBuilder) =>
{
    var decorated = item + 1;
    slotBuilder.OpenElement(2, "span");
    slotBuilder.AddContent(3, decorated);
    slotBuilder.CloseElement();
}));
```

### 当前落地方式

该缺口已在 RazorVue handwritten `BuildRenderTree` frontend 与后续 lowering 链路中收口：

- render tree 增加 template-scoped local declaration 节点
- render tree 增加局部 template scope 节点，用于“立即调用的 typed fragment”
- canonical model 显式保留“声明后生效”的局部作用域顺序
- H lowering 使用片段级局部作用域/IIFE 保证单次求值与节点顺序
- SFC lowering 使用局部 template scope wrapper 保留同一顺序语义

因此以下场景现已稳定支持：

- 顶层片段局部值缓存/别名
- `for` / `foreach` body 中基于迭代变量的局部缓存
- typed slot template 中基于 slot 参数的局部缓存
- `AddContent(sequence, RenderFragment<T>, value)` 这种“立即调用 typed fragment + 实参”的局部模板作用域

### 当前支持边界

支持边界仍然刻意收窄为“带初始化器的不可变模板局部声明/局部模板作用域”：

- 支持：`var decorated = item + "!";`
- 支持：`builder.AddContent(0, (RenderFragment<int>)(item => itemBuilder => { ... }), 42);`
- 不支持：先声明后赋值
- 不支持：`++` / `--` / 其他写入型模板局部状态
- 不支持：把模板局部声明当作匿名函数/委托状态载体继续扩散
- 不支持：把任意 delegate 值、动态 callable、外部 fragment 变量都放宽成可立即模板执行的 `AddContent(RenderFragment<T>, value)` 形态；当前要求源码 inline 且可分析

### 当前保护

- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`

当前回归同时覆盖：

- 顶层局部声明成功
- loop body 局部声明成功
- typed slot template 局部声明成功
- typed `AddContent(RenderFragment<T>, value)` 模板作用域成功
- “无初始化器后续赋值”仍明确失败

## 11. 已完成：handwritten `BuildRenderTree` render helper 额外值参数支持

### 现象

此前 handwritten `BuildRenderTree` 对当前组件/local render helper 只支持“单个 `RenderTreeBuilder` 参数”形态。下面这些真实 authoring 都会失败：

```csharp
protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    RenderBody(builder, Title);
}

private void RenderBody(RenderTreeBuilder builder, string? title)
{
    builder.OpenElement(0, "section");
    builder.AddContent(1, title);
    builder.CloseElement();
}
```

```csharp
protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    void RenderBody(RenderTreeBuilder localBuilder, string? title)
    {
        localBuilder.OpenElement(0, "section");
        localBuilder.AddContent(1, title);
        localBuilder.CloseElement();
    }

    RenderBody(builder, Title);
}
```

如果简单把 helper 参数直接替换成调用点实参，会破坏单次求值、副作用顺序和参数作用域边界；如果继续沿用共享 open-frame 解析，又会把“依赖调用方已打开节点/component frame”的 helper 一并放开，边界不安全。

### 当前落地方式

该缺口已在 RazorVue handwritten `BuildRenderTree` frontend 与后续 lowering 链路中收口：

- current-component/local render helper 现支持“恰好一个 `RenderTreeBuilder` 参数 + 额外普通按值参数”
- helper body 在 extra-parameter 场景下按独立片段解析，避免把调用方 open-frame 状态隐式透传进 helper
- render tree / canonical model 使用局部 template scope node 显式保留“helper 形参 <- 调用点实参”绑定
- H lowering 将 helper 参数编码为一次性立即调用作用域，保证单次求值与参数不泄漏
- SFC lowering 将 helper 参数编码为局部 `<template v-for="(...) in [...]">` scope wrapper，并修正了根级 template-scope close-tag 重复输出
- template-scoped local declaration 现在也允许在 helper body 中基于 helper 参数建立局部缓存/别名

因此以下场景现已稳定支持：

- `RenderBody(builder, Title)` 这种当前组件 helper 参数绑定
- `void RenderBody(RenderTreeBuilder localBuilder, string? title) { ... }` + `RenderBody(builder, Title)` 这种 local function helper 参数绑定
- `RenderBody(builder, Title, Subtitle)` / `void RenderBody(RenderTreeBuilder localBuilder, string? title, string? subtitle) { ... }` 这类 multiple extra parameter 绑定
- `RenderBody(title: Title, builder: builder)` / `RenderBody(title: Title, localBuilder: builder)` 这类 named argument 绑定
- `RenderBody(builder)` + helper optional default value 绑定
- helper body 中对参数的 element child / interpolation 使用
- helper body 中基于参数的模板局部缓存/别名
- helper body 中“额外参数 -> 模板局部缓存/别名 -> 后续节点引用”这类组合 authoring
- `for` / `foreach` body 中“loop 变量 -> helper 额外参数 -> helper 内模板局部缓存/别名 -> 后续节点引用”这类组合 authoring
- canonical / H / SFC 三条 lowering 链路对 helper 参数作用域的一致保留

### 当前支持边界

支持边界仍然刻意收窄为“源码可分析、按值参数、helper 自身可独立 canonicalize”的 render helper：

- 支持：`private void RenderBody(RenderTreeBuilder builder, string? title) { ... }`
- 支持：`void RenderBody(RenderTreeBuilder localBuilder, string? title) { ... }`
- 支持：`private void RenderBody(RenderTreeBuilder builder, string? title, string? subtitle) { ... }`
- 支持：named argument / builder 参数不在第一个位置，只要调用点参数与声明一一对应
- 支持：省略 optional parameter 且默认值可安全投影到当前 template/canonical 边界
- 支持：多个额外值参数按调用点实参求值顺序形成嵌套 template scope / 嵌套 IIFE，同时保持 helper 形参与实参的正确绑定；named argument 打乱声明顺序时不会退化成错误重排
- 支持：`for` / `foreach` body 中使用 loop 变量调用 helper 时，loop 变量可作为 helper 实参稳定进入后续 helper parameter scope；不会因为 loop/template scope 叠加而丢失绑定或错误提升
- 不支持：`ref` / `out` / `in` / `params`
- 不支持：helper body 依赖调用方已打开 element/component frame 的 `AddAttribute` / `SetKey` / `CloseElement` / `CloseComponent` 等协议
- 不支持：递归 render helper

### 当前保护

- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`

当前回归同时覆盖：

- frontend 产出 helper 参数 template scope node
- frontend 产出 current-component / local function helper 参数 template scope node
- frontend / canonical / H / SFC 对 multiple extra parameter 的嵌套作用域与调用点实参求值顺序保持一致
- frontend / canonical / H / SFC 对“helper 参数作用域 + helper body 内模板局部声明”组合语义保持一致
- frontend / canonical / H / SFC 对“loop scope + helper 参数作用域 + helper body 内模板局部声明”组合语义保持一致
- canonical model 保留 `title <- props.title` 绑定
- named argument 绑定稳定工作
- omitted optional default value 绑定稳定工作
- H lowering 输出 helper 立即调用作用域
- SFC lowering 输出局部 template scope wrapper，且不再重复闭合 `</template>`
- “helper 依赖调用方 open frame 做 attribute mutation” 仍明确失败

## 13. handwritten `AddContent(RenderFragment<T>, value)` 的 typed fragment carrier 需要稳定边界

### 现象

此前 handwritten `BuildRenderTree` 对 typed fragment 只稳定支持“调用点直接内联匿名模板”：

```csharp
builder.AddContent(0, (RenderFragment<int>)(item => itemBuilder =>
{
    itemBuilder.AddContent(1, item);
}), 42);
```

但真实 authoring 很自然会写成局部 carrier：

```csharp
RenderFragment<int> template = item => itemBuilder =>
{
    itemBuilder.AddContent(1, item);
};

builder.AddContent(0, template, 42);
```

旧实现会把 `template` 误判为普通 template-scoped local，然后因为“callable template state”保护在声明阶段直接失败。

### 当前落地方式

该缺口已在 handwritten `BuildRenderTree` extractor 中收口：

- `RenderFragment` / `RenderFragment<T>` 局部变量会先按“局部 fragment carrier”单独识别，而不是落入普通 template-scoped local 规则
- carrier 只接受源码可分析 initializer：
  - inline anonymous fragment
  - 或引用先前已解析的本地 fragment carrier
- `AddContent(sequence, RenderFragment<T>, value)`、slot template 等后续解析会优先消费该 carrier 映射
- 普通 template-scoped local 仍保持“不允许 callable template state”保护，不会因为这次支持而被整体放宽

### 当前支持边界

- 支持：inline typed fragment
- 支持：同一可分析作用域内、初始化即为可分析匿名模板的局部 `RenderFragment<T>` carrier
- 支持：该局部 carrier 既可用于 `AddContent(sequence, RenderFragment<T>, value)`，也可用于组件 typed slot/template 参数
- 支持：frontend / canonical / H / SFC 对局部 carrier 与 inline 形态保持相同 lowering 结果
- 不支持：任意 delegate 值流分析
- 不支持：current-component property / field 承载的 `RenderFragment<T>`
- 不支持：动态重赋值后的 carrier
- 不支持：无法静态还原到匿名模板 body 的 callable 形态

### 当前保护

- `src/Jazor.RazorVue.Test/BuildRenderTreeTemplateFrontendTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueCanonicalSfcSemanticTests.cs`
- `src/Jazor.RazorVue.Test/RazorVueSfcArtifactFactoryTests.cs`
- `src/Jazor.RazorVue.Test/RazorVuePipelineTests.cs`

当前回归同时锁定：

- typed fragment local carrier frontend 产出 template scope node
- canonical model 与 inline typed fragment 保持相同 `item <- 42` 作用域语义
- H lowering 与 inline typed fragment 保持相同立即调用输出
- SFC lowering 与 inline typed fragment 保持相同局部 template scope wrapper
