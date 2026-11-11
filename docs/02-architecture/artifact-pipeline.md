# 产物管线

> 适用范围：当前源码候选的编译模块、ECMAScript 源码、npm/JSR 绑定、Emit、进程内 NodeApi、Vite、SSR 与 HMR。完整迁移与平台验收状态见[迁移方案](../04-roadmap/nodejs-pnpm-migration-plan.md)，类库边界见[类库与标准前端项目契约](./library-artifact-contract.md)。

## 一个项目根

最终宿主的前端项目统一位于 `jazor/`：

```text
ModuleCatalog + ECMAScript 源码 + binding declarations
                         |
                     Jazor.Emit
                         |
           jazor/（源码、入口、package.json）
                         |
          +--------------+---------------+
          |              |               |
    pnpm API 恢复    Vite API 构建    Vue SSR / Vite HMR
          +--------------+---------------+
              .NET 进程内 NodeApi + libnode
```

Emit 在 MSBuild 阶段将 carrier 物化为标准项目，再经共享 `Jazor.NodeHost` 调用 API。包恢复使用固定 `@pnpm/napi`，构建和开发服务使用 Vite API，SSR 直接调用生成入口。整个执行链不启动 Node、pnpm、tsc、Deno 或 shell 工具子进程，不需要消费者安装全局 JavaScript 工具。

## 输入与所有权

| 输入 | 产生方 | Emit 的处理 |
| --- | --- | --- |
| `Jazor.Generated.ModuleCatalog` | Compiler / RazorVue | 写入生成模块、Entries、source map、相对依赖与 package imports |
| ECMAScript 源码 carrier | ECMAScript 源码库 | 按声明路径写入 `clr/**` 等项目源码 |
| binding declaration | `ECMAScript.*` | 收集 npm/JSR 身份、精确版本、specifier 与显式样式入口 |
| 应用资源 | 最终宿主 / ModuleCatalog | 按源码 import 或 URL 的目标路径写出 CSS、worker、字体、图片和 wasm |

`embedded-mjs` 是源码 carrier；外部绑定的运行时代码由恢复后的包提供。manifest 和 inventory 保存声明与诊断证据，运行时使用标准 package exports 和源码 import。

## 固定数据流

1. 收集最终宿主根程序集、参与程序集、Entries、ModuleCatalog、源码 carrier 和绑定声明。
2. 就地准备最终 `jazor/` 项目。单文件以临时文件加 rename 发布，内容不变时保留时间戳。
3. 写入源码、source map 和资源。项目内模块按最终位置写 `./` 或 `../`；外部 import 保留声明的 bare specifier。
4. 生成浏览器 `entry.js`；启用 SSR 时生成 `ssr-entry.js`，按请求选择组件，不提前执行浏览器启动代码。
5. 合并根 `package.json` 的 dependencies、exports 和默认 Vite 配置。
6. 经 pnpm API 生成或验证 `pnpm-lock.yaml` 和 hoisted `node_modules`。依赖声明与现有 lock 一致时使用 frozen restore。
7. Acornima 检查作者模块语法，Vite 检查模块图解析；检查不执行模块初始化，不重复实现 C# 类型检查。
8. Release 经 Vite API 生成 `dist/`，启用 SSR 时再生成 `ssr/`。发布复制运行闭包；source map 仅在 `JazorPublishSourceMaps=true` 时交付。

## 恢复与检查

NodeApi、LibNode、pnpm 引擎版本集中在 `Directory.Build.props`。当前固定组合为 NodeApi 0.9.36、LibNode 20.1800.215、`@pnpm/napi` 12.5.1；Windows apphost 使用已接受的 `CETCompat=false`。LibNode 仍属于 Node 20 系列。

原生 pnpm 引擎由宿主引导，并校验包 integrity。恢复通过其发布 API 读取 registry、认证、代理和 TLS 配置；安装固定使用 hoisted linker、自动 peer 安装和 `ignoreScripts=true`。不执行生命周期 shell scripts，不自动回退到 CLI。当前契约不承诺 pnpm CLI 的全部配置、workspace 或 lifecycle 行为。

依赖声明、引擎、Node、平台、架构、linker 与 lock 内容参与恢复状态。已成功恢复且身份不变时跳过重装；`file:` 依赖每次重新交给安装引擎处理。安装前清除成功状态，只有成功后才写回，失败不能伪装为可复用结果。

通过 `JazorPnpmStoreDirectory` / Emit `--pnpm-store` 选择共享 store。pnpm 原生安装 API 没有取消入口，恢复必须等待实际安装结束后传播取消，避免原生任务继续写 store。SSR、Vite 与测试环境在可调度的 JS 线程上终止事件循环并等待退出；同步无限循环的强制中断尚不支持。

## 构建与开发服务

当前支持的构建入口是 `vite build` 与 SSR 配置的 Vite build，通过 API 执行。应用定制放入 Vite 配置；任意 package scripts 或 shell 命令不是替代执行协议。

Vite 沿标准 ESM 图解析 package exports、conditions、sideEffects、CSS、worker 与静态资源。共享 helper 和必要副作用保留，未到达的导出与资源由构建器裁剪。

开发服务器由 ASP.NET Core 生命周期拥有，在进程内 NodeApi environment 中启动与关闭。ASP.NET Core 代理 HTTP/WebSocket，Vite 负责模块解析、source map 与 HMR；不要求另开终端执行 dev 命令。

## SSR 环境池

SSR 复用有界的 NodeApi environment 池。`WorkerCount` 为单应用实例的并发环境数，默认 `min(Environment.ProcessorCount, 4)`。每次渲染创建独立 Vue app，直接调用入口并等待异步生命周期；环境复用 ESM 模块缓存。

Development 使用根 `ssr-entry.js`、package 声明和 lock；Release 使用 `ssr/ssr-entry.js`、`ssr/package.json` 与本地 bundled chunks。发布目录不需要源码、根 lock、node_modules 或包恢复。桥接从程序集资源加载，不在部署目录写临时工具文件。

入口、依赖声明、开发 lock 和源码模块内容变化时轮换 generation，以清除旧模块缓存。内容不变只改时间戳时复用环境。取消或失败的租用环境退出后恢复池容量；与 generation 切换重叠的错误显式传播，不自动重放有副作用的渲染。

SSR 不再通过 loopback HTTP 或 package task 启动，`TaskName` 设置已移除。state 仍通过强类型请求、props 和显式 provider 传递。

## 确定性与交付

`JazorMode=debug` 保留源码与 maps；`release` 从同一项目根生成浏览器运行闭包；`JazorSSR=true` 增加 SSR 输出。最终可执行项目拥有这些配置，类库只传播自己的 carrier。

相同源码、SDK、固定运行时、依赖身份和构建选项应产生稳定路径、入口、声明、lock 与 bundle 元数据。模块按稳定路径与内容去重；外部包版本或资源冲突在恢复前失败。相同外部包可由多个不同绑定声明，但其身份与资源边必须一致。

写入就地生效，失败或中断后由下一次构建收敛，不对整个项目根做快照或回滚。诊断 manifest 位于 obj，是构建证据；浏览器、Vite 与 SSR 不依赖它解析模块。

`dotnet publish` 仅交付 `dist/**` 和启用后的 `ssr/**`，运行时所需 NodeApi/libnode 资产随宿主发布。Windows 应启动该构建的 apphost，以保留 `CETCompat=false`，不要用另一个工具宿主替代。

当前 Windows x64 的真实证据包含 pnpm/JSR 解析、Vite/HMR、Pinia/VueRoute DOM、JazorAdmin、Wiki PathBase，以及从 NuGet 发布到干净目录的 SPA/SSR 和 hydration。跨平台、覆盖率与全仓库门禁仍须按迁移方案完成，不能据此扩大支持声明。
