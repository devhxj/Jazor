# Jazor SSR 实施计划

> 状态：SSR 基线已接入；Deno 仅为显式的过渡执行后端。
> 目标：由 ASP.NET Core 提供 SSR 宿主能力，后续以 `Jint + Netpack` 替换 Deno，而不改变应用集成契约。

## 1. 决策与原因

SSR 需要两类职责，不能混为一层：

| 职责 | 当前所有者 | 原因 |
| --- | --- | --- |
| 路由、请求 props、HTTP 响应、HTML 文档、静态资源和中间件顺序 | ASP.NET Core | 这些是 .NET Web 宿主语义，必须保留现有 endpoint、`PathBase` 和 static-file 行为。 |
| 执行 Vue `.mjs`、调用 `@vue/server-renderer`、得到组件 HTML | SSR renderer | Vue SSR 是 JavaScript 运行时语义，ASP.NET Core 不能直接执行 ESM。 |
| 物化 Vue/runtime 资源、生成 browser/SSR import map | `Jazor.Emit` | 资源必须来自 NuGet-owned manifest，不依赖应用 `node_modules`、CDN 或 remote import。 |

`Netpack` 是 bundler，不是 JavaScript 执行器；`Jint` 是执行器，不负责解析整个应用的模块图。因此最终路线是 Netpack 先把 SSR 入口及依赖变成 Jint 可执行的服务器 bundle，再由 Jint 执行该 bundle。

当前为了验证完整 SSR 主线，使用 Deno 执行 materialized ESM graph。Deno 体积和启动成本不适合作为长期 ASP.NET Core 运行时，因此它必须满足以下限制：

- 只由 `JazorSsrOptions.DenoExecutablePath` 显式配置；
- 不从 `Jazor.Emit` 的构建工具资产复制到 ASP.NET Core 应用或 publish 输出；
- 不从 PATH、环境变量、`node_modules` 或网络下载中隐式发现；
- 每个渲染请求使用隔离进程，且只授予 artifact root 的读权限；
- 后续删除 Deno 后端时，应用仍只依赖 `IJazorSsrRenderer`、`AddJazorSsr` 和 `UseJazorSsr`。

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

SSR runtime 的传输 ABI 固定为 `modulePath` 与 `props` 两个 JavaScript 名称。它们独立于 CLR 成员命名规则，因此未来更换执行器不受编译器命名迁移影响。

## 3. 应用集成

```csharp
builder.Services.AddJazorSsr(options =>
{
    options.DenoExecutablePath = @"C:\tools\deno\deno.exe";
});

var app = builder.Build();
app.UseStaticFiles();
app.UseJazorSsr("components/app.mjs", new { Title = "Jazor" });
```

`UseJazorSsr` 复用 `UseJazorSpaFallback` 的接受条件和排除规则。静态文件、已映射 endpoint 和非 HTML 请求必须先于 SSR fallback 处理；`HEAD` 只返回响应头，不启动 JavaScript renderer。

服务端输出包含：rendered HTML、相同的 JSON props、浏览器 import map、样式链接以及 `createSSRApp(...).mount(...)` hydration 脚本。`onServerPrefetch` 可以影响本次服务端 HTML，但 Vue 不会自动把该异步状态注入浏览器；应用必须把需要复用的状态显式放入 props 或自己定义的状态载荷中。

## 4. Jint + Netpack 替换门槛

替换不是向现有 Deno runner 添加兼容 fallback，而是新增一个 `IJazorSsrRenderer` 实现并在默认 DI 注册中切换。实现前必须满足：

1. Netpack 有独立 SSR bundle contract，入口、依赖、Vue runtime 和 source map 均来自 materialized package graph。
2. bundle 不包含 Jint 不支持的动态 ESM 解析、Node builtin 或运行时 remote import；不支持时在 build 阶段给出明确诊断。
3. Jint renderer 接收并返回与当前 renderer 相同的 `JazorSsrRequest` / `JazorSsrRenderResult` 语义，保留 props 序列化和 HTML 结果。
4. ASP.NET Core 中间件、artifact locator、浏览器 hydration 和 `PathBase` URL 重写不因替换而变化。
5. 删除 `DenoExecutablePath` 后，Deno 不再是应用运行前置条件，也不保留隐式 fallback。

## 5. 验收

- `Jazor.Emit` 能从 NuGet-owned Vue manifest 物化 `vue` 与 `@vue/server-renderer`，不出现 `node_modules`、CDN 或 remote import。
- `UseJazorSsr` 实际执行 `onServerPrefetch`，保留 endpoint/static-file/`PathBase`，并为 `HEAD` 跳过 renderer。
- 真实浏览器能 hydrate 服务端 HTML，且无 hydration diagnostic。
- `JazorMode=release` 加 `JazorSsrEnabled=true` 时，浏览器 bundle 与 raw SSR graph 都存在，raw graph 包含 server renderer、两个 import map 和许可证。
- Jint 替换完成时，以上测试不依赖 Deno 可执行文件，额外验证 Netpack SSR bundle 与 Jint 的 source-map/debug 边界。
