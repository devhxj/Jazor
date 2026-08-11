# Jazor SSR 实施计划

> 状态：SSR 基线已接入；DenoHost 是固定的运行时执行器。
> 目标：由 ASP.NET Core 提供 SSR 宿主能力，DenoHost 执行物化 ESM 图，Netpack 只构建浏览器生产包。

## 1. 决策与原因

SSR 需要两类职责，不能混为一层：

| 职责 | 当前所有者 | 原因 |
| --- | --- | --- |
| 路由、请求 props、HTTP 响应、HTML 文档、静态资源和中间件顺序 | ASP.NET Core | 这些是 .NET Web 宿主语义，必须保留现有 endpoint、`PathBase` 和 static-file 行为。 |
| 执行 Vue `.mjs`、调用 `@vue/server-renderer`、得到组件 HTML | SSR renderer | Vue SSR 是 JavaScript 运行时语义，ASP.NET Core 不能直接执行 ESM。 |
| 物化 Vue/runtime 资源、生成 browser/SSR import map | `Jazor.Emit` | 资源必须来自 NuGet-owned manifest，不依赖应用 `node_modules`、CDN 或 remote import。 |

`Netpack` 是 browser bundler，不承担 JavaScript 执行。DenoHost 则执行 SSR 所需的 materialized ESM graph，并通过 NuGet 提供本地 Deno runtime。两者不交换职责，也不引入 SSR server bundle。

DenoHost SSR 的运行时约束如下：

- 不依赖 PATH、环境变量、`node_modules`、CDN 或网络下载的 Deno；
- 每个渲染请求使用隔离进程，且只授予 artifact root 的读权限；
- SSR 直接消费 materialized module graph 与 `ssr-importmap.json`，不消费 Netpack browser bundle；
- 应用集成只依赖 `IJazorSsrRenderer`、`AddJazorSsr` 和 `UseJazorSsr`，不暴露 Deno 可执行文件路径。

## 2. 当前产物合同

调试构建的 artifact root 为 `wwwroot/jazor/`。发布构建默认只保留浏览器 bundle；配置 `<JazorSsrEnabled>true</JazorSsrEnabled>` 后，还会生成独立的 SSR 原始模块图：

```text
wwwroot/jazor/
  bundle.js
  bundle.js.map
  ssr/
    jazor-manifest.json
    components/*.mjs
    importmap.json
    ssr-importmap.json
    manifest.json
    vendor/...
```

`importmap.json` 面向浏览器，URL 根为 `/jazor/...`；`ssr-importmap.json` 面向本地 ESM 执行，目标为 artifact root 内的 `./vendor/...`。`manifest.json` 只描述浏览器 CSS 资源。`jazor-manifest.json` 始终是应用模块 manifest，不能与资源 manifest 混用。

SSR runtime 的传输 ABI 固定为 `modulePath` 与 `props` 两个 JavaScript 名称。它们独立于 CLR 成员命名规则，也不受 DenoHost 进程边界影响。

## 3. 应用集成

```csharp
builder.Services.AddJazorSsr();

var app = builder.Build();
app.UseStaticFiles();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

`UseJazorSsr` 复用 `UseJazorSpaFallback` 的接受条件和排除规则。静态文件、已映射 endpoint 和非 HTML 请求必须先于 SSR fallback 处理；`HEAD` 只返回响应头，不启动 JavaScript renderer。

服务端输出包含：rendered HTML、相同的 JSON props、浏览器 import map、样式链接以及 `createSSRApp(...).mount(...)` hydration 脚本。`onServerPrefetch` 可以影响本次服务端 HTML，但 Vue 不会自动把该异步状态注入浏览器；应用必须把需要复用的状态显式放入 props 或自己定义的状态载荷中。

## 4. 固定边界

1. Netpack 只生成 browser bundle 与 source map；SSR 不使用其输出作为执行输入。
2. DenoHost 直接执行 materialized SSR module graph，并以 `ssr-importmap.json` 解析 Vue 与 `@vue/server-renderer`。
3. `JazorSsrRenderer` 为每次请求创建独立 payload 文件和 DenoHost 进程，保留 props 序列化、HTML 结果与取消语义。
4. ASP.NET Core 中间件、artifact locator、浏览器 hydration 和 `PathBase` URL 重写不因执行器边界而变化。

## 5. 验收

- `Jazor.Emit` 能从 NuGet-owned Vue manifest 物化 `vue` 与 `@vue/server-renderer`，不出现 `node_modules`、CDN 或 remote import。
- `UseJazorSsr` 实际执行 `onServerPrefetch`，保留 endpoint/static-file/`PathBase`，并为 `HEAD` 跳过 renderer。
- 真实浏览器能 hydrate 服务端 HTML，且无 hydration diagnostic。
- `JazorMode=release` 加 `JazorSsrEnabled=true` 时，浏览器 bundle 与 raw SSR graph 都存在，raw graph 包含 server renderer、两个 import map 和许可证。
- `AddJazorSsr()` 不配置外部可执行文件时，仍由 NuGet 分发的 DenoHost runtime 完成 SSR。
