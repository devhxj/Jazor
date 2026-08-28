# 产物管线

> 适用范围：`Jazor.Emit`、MSBuild 输出模式、source map、manifest、Netpack bundle 与 SSR 模块图。

类库如何直接引用工具、如何让 catalog 和上游资源到达最终宿主，见[类库产物与引用契约](./library-artifact-contract.md)。本文只描述物化层的职责，不把中间类库的工具依赖当作 artifact 依赖。

## 交付职责

`Jazor.Emit` 消费编译器和 adapter 生成的中性 catalog，不参与 C# 或 Razor 语义降低。它负责程序集读取、模块收集、确定性文件物化、manifest 维护、本地库资源复制与浏览器打包。

| 输出 | 所属组件 | 说明 |
| --- | --- | --- |
| ESTree、模块文本与 source-map carrier | `Jazor.Compiler` | 编译语义和来源锚点 |
| Artifact catalog | adapter（当前为 `Jazor.RazorVue`） | `Jazor.Generated.ArtifactCatalog`，模块、source map、资产和 opaque HMR payload |
| Runtime provider catalog | adapter（当前为 `Jazor.RazorVue`） | `Jazor.Artifacts.RuntimeProviderCatalog`，嵌入模块、依赖闭包和 import-map contribution |
| `.mjs`、`.mjs.map`、manifest | `Jazor.Emit` | debug 物化与资源目录维护 |
| 浏览器 bundle | `Jazor.Emit` + Netpack | 生产环境浏览器交付 |
| SSR 模块图 | `Jazor.Emit` + ASP.NET Core | 服务器运行的原始模块图 |

## MSBuild 输出模式

| `JazorMode` | 行为 |
| --- | --- |
| `none` | 默认值，不写入 Jazor 产物 |
| `debug` | 写入可检查的模块、外部 source map 与 `jazor-manifest.json` |
| `release` | 先在中间目录物化，再通过固定 Netpack 路径生成浏览器 bundle |

`JazorDir` 默认指向 `$(MSBuildProjectDirectory)\jazor\`。Web 宿主通过 `UseJazorHost()` 将该目录挂载到浏览器 `/jazor/*`；发布目标显式复制它到 `<publish>/jazor/`，因此 Web 与非 Web SDK 宿主得到相同的发布布局。应用启用 `JazorSSR=true` 时，release 产物还会保留用于服务器渲染的原始模块图；该图与优化后的浏览器 bundle 分开维护。

## 确定性与清理

模块收集、导入别名、产物路径、manifest 条目和 source map 产出必须稳定。清理只针对 Emit 已拥有的输出范围，避免把编译语义或应用资产的责任混入物化层。

`jazor-manifest.json` 是模块与本地资源的共享清单。它避免将机器绝对路径或墙钟时间作为新产物的必要内容，从而支持可复现构建。

## Package Asset Closure

生成模块的 `PackageImports` 是 library ESM 的唯一应用根。`LibraryMaterializer` 依据这些 logical specifier 选择 package manifest 中的 entry，而不是把每个已引用 NuGet 包的全部 `dist/` 目录复制到应用输出。manifest schema-v1 的 import entry 可声明：

- `developmentDependencies` / `productionDependencies`：该 entry 所需的其他 logical package imports；
- `files`：该 entry 的 relative ESM closure 或 entry-specific license；
- 根 `files`：仅在该 library 至少有一个 selected entry 时复制的 shared metadata/license。

选择顺序和 copy order 都按 library/version/specifier 稳定排序。debug/release 有应用 manifest 时，空 `PackageImports` 表示不物化 package asset；直接 CLI 调用没有 application manifest 或 explicit import root 时，保留历史的 all-entry materialization 语义。package author 必须随 vendor entry 更新这些声明，Emit 不扫描 `node_modules`，也不把第三方 JS parser 变成运行时依赖。

release browser graph 和 SSR graph 分开选择。浏览器只跟随 generated imports；SSR runner 额外显式请求 `vue` 与 `@vue/server-renderer`，因此不会依赖浏览器图碰巧全量复制 runtime。该机制减少 publish/deployment 文件集合和本地 asset footprint；它不自动等同于首屏 network saving，因为未被 import 的旧文件本来也不会由浏览器请求。

## SSR

ASP.NET Core 负责请求管线、静态文件、响应文档和 hydration；DenoHost 负责执行生成的 Vue 服务器模块；Netpack 只负责浏览器构建。应用不需要全局 Deno、项目 `node_modules`、CDN 或远程 import 才能使用受支持的 SSR 路径。

SSR executor 是有界 persistent Deno worker pool。每个 worker 通过 line-delimited stdin/stdout protocol 串行处理请求，应用级 `WorkerCount` 限制跨 generation 的总并发。`jazor-manifest.json`、`ssr-importmap.json` 与 packaged runner 的内容哈希共同标识 generation；新 generation 不复用旧 ESM module cache，旧请求则允许完成。请求取消或 worker crash 会丢弃对应进程，后续租约按需补 worker；应用关闭时先停止接收请求并 drain in-flight render，再关闭 stdin 和进程。协议不落 per-request temporary JSON。

可复现的 cold/warm/concurrent 测量入口是：

```bash
dotnet run --file scripts/csharp/benchmark-razorvue-ssr.cs -- --samples 5 --iterations 50 --workers 4
```

使用方式见 [安装与配置](../03-guides/installation-and-configuration.md)，实现级 API 见 [Jazor.Emit README](../../src/Jazor.Emit/README.md)。
