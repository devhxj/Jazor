# 产物管线

> 适用范围：`Jazor.Emit`、MSBuild 输出模式、source map、manifest、Netpack bundle 与 SSR 模块图。

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

`JazorDir` 默认指向 `$(MSBuildProjectDirectory)\wwwroot\jazor\`。应用启用 `JazorSSR=true` 时，release 产物还会保留用于服务器渲染的原始模块图；该图与优化后的浏览器 bundle 分开维护。

## 确定性与清理

模块收集、导入别名、产物路径、manifest 条目和 source map 产出必须稳定。清理只针对 Emit 已拥有的输出范围，避免把编译语义或应用资产的责任混入物化层。

`jazor-manifest.json` 是模块与本地资源的共享清单。它避免将机器绝对路径或墙钟时间作为新产物的必要内容，从而支持可复现构建。

## SSR

ASP.NET Core 负责请求管线、静态文件、响应文档和 hydration；DenoHost 负责执行生成的 Vue 服务器模块；Netpack 只负责浏览器构建。应用不需要全局 Deno、项目 `node_modules`、CDN 或远程 import 才能使用受支持的 SSR 路径。

使用方式见 [安装与配置](../03-guides/installation-and-configuration.md)，实现级 API 见 [Jazor.Emit README](../../src/Jazor.Emit/README.md)。
