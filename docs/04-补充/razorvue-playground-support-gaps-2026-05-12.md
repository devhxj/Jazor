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
- 强类型 C# authoring 入口统一使用 `CssClass` / `CssStyle`，通过 `[VueLibraryProp(..., Name = "class" / "style")]` 映射到 Vue runtime prop。
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

- 进一步把 `razorvue-consumer-entry` 封装为 `Jazor` SDK/MSBuild target，减少项目手写 `Exec`。
- 提供 ASP.NET Core + RazorVue library mode 的标准模板，明确 colocated consumer 目录、runtime entry、bundle 输出和 publish 合并策略。

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
- 再挂载 `UseJazorSpaFallback(PlaygroundHostPage.WriteHtmlAsync)`
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

- 对 `dotnet run --project` 从仓库根启动的场景增加官方示例验证

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

- RazorVue / ASP.NET Core 集成层可以提供官方合并 target，避免真实项目手写“emit + consumer bundle”合并逻辑。
- `UseJazorWebAssets(...)` 后续可以继续扩展为更完整的 RazorVue library-mode 宿主模板入口。

## 8. 待执行：统一 manifest 与宿主 API 收敛

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
- MSBuild target 移除公开的 `JazorRazorVueManifestPath` 默认值和 `jazor-manifest-razorvue.json` 引用；previous snapshot 使用统一 manifest 的快照路径。
- `razorvue-diff`、bundle update plan、host asset sidecar 仍可复用 RazorVue diff 模型，但输入源必须是统一 manifest 投影后的 component 集合。

### Consumer 与 SFC bridge 工作项

- `razorvue-consumer-entry` 默认 manifest 路径改为 `<hostJazorRoot>/jazor-manifest.json`。
- `razorvue-sfc-bridge` 默认 manifest 路径改为 `<hostJazorRoot>/jazor-manifest.json`。
- Deno bridge 读取统一 manifest shape，并只处理 `kind = "vue"` 且 `component.model = "sfc"` 的 module。
- consumer entry 组件选择逻辑只在 component entries 中匹配，普通 `mjs` module 不参与 `id:` / `name:` / `path:` component selector。
- 错误信息应继续明确区分“manifest 不存在”“manifest 没有组件”“selector 无匹配”“selector 匹配多个组件”，不能因为统一 manifest 降低诊断质量。
- Playground colocated `consumer` 目录继续保留；它是单 .NET 项目中的前端消费构建层，不是第二个运行时 host。命名上使用 `consumer`，不再使用 `playground-consumer` 这类项目特化名称。

### ASP.NET Core 宿主工作项

- `UseJazorWebAssets()` 默认只依赖标准 `jazor-manifest.json` 即可挂载开发期 `/jazor/*` 输出。
- `DevelopmentEntryModuleRelativePath = "jazor-manifest-razorvue.json"` 这类样例必需配置应移除；高级项目仍可通过 option 覆盖 readiness probe。
- `JazorDevelopmentAssetOptions` 默认探测列表移除旧 manifest 文件名，避免继续把废弃文件作为隐式契约。
- `wwwroot` 静态文件使用 ASP.NET Core 默认 web root 机制，不在 Playground/Wiki 私有代码里手写特殊处理。
- `JazorWebApplication.CreateBuilder(args)` 作为源码运行与发布运行的内容根 helper，可供 Playground 和 Wiki 共用。
- `UseJazorWebAssets(...)` 负责标准静态文件、source map content type、开发期 Jazor 输出挂载；项目只配置安全头、cache header 或是否启用 default files。
- `UseJazorSpaFallback(...)` 继续负责 SPA navigation fallback；Wiki 如果需要 SEO shell 和 discovery document，可以保留项目特定 HTML shell 逻辑，但不应复制静态资源挂载和 Jazor output 探测逻辑。

### Playground / Wiki 一致性工作项

- `Playground/Program.cs` 精简为 builder、服务注册、安全头、`UseJazorWebAssets(...)`、SPA fallback、API endpoint，不再包含 manifest 文件名或 Jazor output 细节。
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
- ASP.NET Core SPA fallback/static-file 官方 helper
- ASP.NET Core 源码/发布双形态 content root helper
- `/jazor/*` 本地 webroot bundle + development emit 标准挂载 helper
