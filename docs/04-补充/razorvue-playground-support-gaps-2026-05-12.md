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
- `consumer` 读取 manifest 和 `.vue`
- Deno pipeline 编译并打包到 `wwwroot/jazor/client-entry.*`
- Deno pipeline 编译 `.vue` 后输出 named-export bridge module，例如 `export { _sfc_main as PlaygroundCatalogPage }`
- consumer 入口和组件间引用都使用 named import，例如 `import { PlaygroundCatalogPage } from "./pages/playground-catalog-page.mjs"`

### 后续提升方向

- 将当前 Playground 内的 SFC named-export bridge 标准化为 RazorVue 官方 build target / SDK 能力
- 若未来 authored Jazor module 需要引用 RazorVue 组件，也应引用 bridge module 的 named export，而不是直接引用 `.vue` default export

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

### 后续提升方向

- 若希望进一步降低接缝，未来可以考虑把 consumer pipeline 标准化为官方 build target / sdk 能力
- 或让 ASP.NET Core + RazorVue library mode 提供更完整的一体化 build 封装

## 5. ASP.NET Core fallback 不能使用 catch-all endpoint 抢占静态文件

### 现象

早期宿主验证时，`/assets/client-entry.js` 文件存在于 `wwwroot/assets`，但返回 404。后续按 Playground 的 `/jazor/*` 统一资源边界改为 `wwwroot/jazor/client-entry.*`，但该中间件顺序问题仍然成立。

根因是 endpoint routing 中的：

```csharp
app.MapMethods("/{**path}", ["GET", "HEAD"], ...)
```

会先为 `/assets/*` 选择 catch-all endpoint，导致 `StaticFileMiddleware` 因已有 endpoint 而不处理请求，最终落入 fallback 逻辑。

### 当前落地方式

`Playground` 改为和 `Wiki` 一样使用普通 middleware 处理 HTML shell fallback：

- 先挂载 `UseJazorDevelopmentAssets()`
- 再挂载 `UseStaticFiles()`
- 再用普通 `app.Use(...)` 判断非文件 HTML 路由
- fallback 显式排除 `/api`、`/assets`、`/health`、`/jazor`

### 后续提升方向

- 将该约束写入 ASP.NET Core 集成样例或模板
- 对 SPA fallback 提供官方 helper，避免用户误用 endpoint catch-all 破坏静态文件服务

## 6. 发布内容根不能固定到源码路径

### 现象

为了让 `dotnet run --project ...` 从仓库根启动时找到源码 `wwwroot`，一开始使用 `CallerFilePath` 固定 `ContentRootPath`。

该方式在发布包中有风险：发布后如果仍在同一台机器运行，宿主可能继续指向源码目录，而不是发布目录。

### 当前落地方式

`Playground` 的内容根解析改为：

- 若 `AppContext.BaseDirectory/wwwroot` 存在，优先使用发布/输出目录
- 否则回退到 `Program.cs` 所在源码目录

同时普通静态文件使用显式 `PhysicalFileProvider`，避免工作目录差异影响 `wwwroot/jazor`。

### 后续提升方向

- 在 `Jazor.AspNetCore` 或模板层提供标准的源码/发布双形态内容根策略
- 对 `dotnet run --project` 从仓库根启动的场景增加官方示例验证

## 7. 浏览器 bundle 与 RazorVue emit 复用 `/jazor/*` 需要明确合并语义

### 现象

`Playground` 需要让最终浏览器 bundle 也直接落在 `wwwroot/jazor`，而不是单独使用 `wwwroot/assets`。这会让发布目录中同一个 `/jazor/*` 路径同时包含：

- 根 `jazor` emit 复制出的 manifest、SFC 和 CLR runtime modules
- Deno consumer 生成的 `client-entry.js`、`client-entry.css` 与 sourcemap

### 当前落地方式

- 本地 build 阶段：根 `jazor` 仍是 RazorVue emit 源，`wwwroot/jazor` 只承载浏览器 bundle。
- 本地宿主阶段：先挂载 `wwwroot/jazor`，再挂载根 `jazor` 的 development assets，确保 `/jazor/client-entry.*` 和 `/jazor/jazor-manifest-razorvue.json` 都可访问。
- publish 阶段：先清空发布 `wwwroot/jazor`，复制根 `jazor` emit，再复制 `wwwroot/jazor/client-entry.*`，最终发布包只从 `wwwroot/jazor` 服务 `/jazor/*`。
- consumer 中间 build root 默认按进程隔离为 `.deno-build/pid-*`，避免 `smoke:ssr`、`smoke:browser` 或 CI 并行任务互相清理同一目录。

### 后续提升方向

- RazorVue / ASP.NET Core 集成层可以提供官方合并 target，避免真实项目手写“emit + consumer bundle”合并逻辑。
- `UseJazorDevelopmentAssets()` 可以考虑支持多个物理源的同一 request path 合并，减少中间件顺序敏感性。

## 8. 当前处理结论

这些问题没有阻断 `Playground` 落地，但都属于真实生产标准下必须正视的能力边界。

建议优先级：

1. default import/export 与 SFC bridge
2. library-mode 单项目 consumer pipeline 的官方化
3. ASP.NET Core fallback/static-file 官方化模板
4. `/jazor/*` 多来源合并能力官方化

已落地并由回归保护的项：

- Razor IR 对纯静态多 token attribute 的稳定接受
- library component raw `class=` / `style=` fallthrough authoring 体验修复
