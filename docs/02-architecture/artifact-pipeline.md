# 产物管线

> 适用范围：C# 编译模块、ECMAScript 源码、npm/JSR 绑定、Jazor.Emit、Deno、所选构建工具、SSR 与 HMR。标准项目契约见[类库与标准前端项目契约](./library-artifact-contract.md)。

## 一个项目根

最终宿主的前端产物统一位于 `jazor/`：

```text
ModuleCatalog + ECMAScript 源码 + binding declarations
                         |
                         v
                     Jazor.Emit
                         |
                         v
             jazor/（源码、入口、package.json）
                         |
             +-----------+-----------+
             |                       |
             v                       v
       Deno restore/check       Vite / DenoHost
```

Emit 是 MSBuild 阶段的项目适配器。它把 Jazor carrier 转换为标准项目文件，再调用标准工具。项目形成后，包解析、条件选择、模块可达性和资源裁剪遵循 Deno、npm/JSR package 与 所选构建工具 的规则。

## 输入与所有权

| 输入 | 产生方 | Emit 的处理 | 项目结果 |
| --- | --- | --- | --- |
| `Jazor.Generated.ModuleCatalog` | `Jazor.Compiler` / RazorVue | 读取生成模块、Entries、source map、相对依赖与 package imports | 生成的 `.mjs`、`.map` 与相对 import |
| ECMAScript 源码 carrier | ECMAScript 源码库 | 按声明路径写入源码树 | `clr/**` 与其他项目源码 |
| binding declaration | `ECMAScript.*` binding | 收集 dependency key、版本、integrity、specifier 与显式样式入口 | 根 `package.json` 的 `dependencies` 与源码 bare import |
| 应用本地资源 | 最终宿主 / `ModuleCatalog` | 按源码 import 或 URL 的目标路径写出 | 项目内 CSS、worker、字体、图片和 wasm |

现有 metadata 中的 `embedded-mjs` 对应 ECMAScript 源码 carrier，写入项目源码树。npm/JSR binding 的运行时代码由依赖恢复提供。`manifest.json`、inventory 和 source map 保存声明与诊断证据，项目运行时解析使用标准 `package.json` 和源码 import。

## 固定数据流

1. **收集**：最终宿主提供根程序集、参与程序集、应用 Entries、`ModuleCatalog`、源码 carrier 和 binding declarations。
2. **就地准备项目**：Emit 直接在最终 `jazor/` 写入，单文件使用临时文件加 rename 以避免半写文件，并复用仍与 `package.json` 一致的锁文件。
3. **写源码**：写入 `clr/**`、其他 ECMAScript 源码、编译生成模块、source map 与本地资源。
4. **归一化 import**：项目内模块按最终文件位置写 `./` 或 `../`；npm/JSR import 保留绑定声明中的 bare specifier。
5. **生成入口**：`entry.js` 连接浏览器 roots；启用 SSR 时生成 `ssr-entry.js`，通过渲染请求加载选定组件。SSR 入口不提前执行浏览器启动代码。
6. **生成项目声明**：合并实际 package imports，写根 `package.json` 的标准字段、`dependencies` 和 `exports`。
7. **恢复与检查**：调用 Deno 2.9.7 生成或验证 `deno.lock` 和 `node_modules`，再以 frozen lock 检查可见入口。
8. **消费与交付**：Release 交给选定的标准前端工具构建 `dist/`；SSR 交给 DenoHost 使用同一项目；写入就地生效，失败显式返回并由下一次构建收敛。

## 源码与入口路径

源码 carrier 的路径在项目内保持稳定：

```text
src/ECMAScript/clr/**         -> jazor/clr/**
ECMAScriptModule.RelativePath -> jazor/<RelativePath>
```

`ModuleCatalog` 中的相对依赖在写出时根据 importer 的最终路径计算。package import 直接由 Deno 恢复的 `node_modules` 解析。入口导出保持 C# binding 声明的 named/default 形式。

应用可以拥有浏览器、SSR 和 HMR 等可见入口；这些入口共享一棵项目源码与 package 依赖图。`obj/.../jazor-manifest.json`和 source map 只属于 Emit 的诊断/增量证据，运行时与开发服务不读取它；package resolution 始终读取根 `package.json`。

## Deno 项目恢复

Deno 版本固定为 2.9.7，由 `DenoHost.Runtime` 包提供可执行文件。Emit 的 restore/check 与 `deno task build` 统一经 `DenoHost.Core` 的 `DenoProcess` 启动（输出捕获、退出等待与取消终止都由它托管）；`--deno`/`JazorDenoPath`/`JAZOR_DENO_PATH` 保留为仅做存在性校验的兼容入口。Emit 根据根项目的 dependency identity 选择标准命令。

首次生成或依赖 identity 变化：

```text
deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=false
```

锁文件与 `package.json` 对齐（按 lock 确定性恢复）：

```text
deno install --package-json --node-modules-dir=manual --node-modules-linker=hoisted --frozen=true
```

恢复后的入口检查：

```text
deno check --node-modules-dir=manual --no-remote --no-config --frozen-lockfile entry.js
```

启用 SSR 时对 `ssr-entry.js` 追加相同的 frozen check。`deno.lock` 是 Deno 的冻结依据；完整 npm `package-lock.json` 作为生态兼容输入随项目保留，并在生成时验证根依赖一致。

## 标准消费者

### 项目构建脚本

所选构建工具 接收项目根、可见 entry、构建模式与输出目录，并沿普通 ESM 图读取：

```text
entry.js
  -> 项目源码
  -> npm/JSR 公开入口
  -> 包内实现与共享 helper
  -> peer / optional dependency
  -> side-effect module
  -> CSS / worker / static asset
```

所选构建工具 使用上游 `package.json` 的 `exports`、conditions 和 `sideEffects`。公开子路径与 named export 提供细粒度入口；组件内部 import、共享模块和必要副作用在可达图中保持，未到达的导出与资源由构建器裁剪。

### DenoHost

DenoHost 以 `jazor/` 为工作目录，加载 Emit 生成的 `ssr-entry.js`，复用已恢复的 `node_modules` 与 `deno.lock`。浏览器构建与 SSR 共享 package identity 和源码；条件选择由各自的标准运行目标决定。

SSR 项目定位要求 `ssr-entry.js`、`package.json` 和 `deno.lock`，不要求 `jazor-manifest.json`、旧的资源 `manifest.json` 或 import map。宿主不再根据样式清单注入 `<link>`。worker 在 SSR 入口、package 声明或 lock 内容变化后轮换；仅时间戳变化时复用现有 worker。入口和 package 文件以单文件临时写入加 rename 发布，内容不变时保留时间戳。

worker 通过标准项目任务启动，`TaskName` 默认 `ssr`；开发可选择 `ssr:dev`，由静态脚本中的 Deno `--watch=.` 更新子模块（排除依赖目录与构建输出）。渲染通过 Deno loopback HTTP 服务处理，重启后的监听地址经 ready 通知更新；宿主不实现文件监听或 import 图扫描。与重启重叠的请求允许失败并传播错误，不自动重放。

### HMR

Vite 开发服务器读取当前项目的源码入口、source map 和变更文件，ASP.NET Core 代理 HTTP/WebSocket，不默认静态托管或观察 jazor/。更新消息引用项目内稳定路径；依赖恢复和包解析继续由同一 `package.json`、`node_modules` 与 lock 提供。

## 写入与确定性

Emit 在最终 `jazor/` 中就地完成写入、恢复、检查和构建。写入就地生效：中断或失败时项目可能处于未收敛状态，下一次构建按同一规则收敛；HMR 需要模块随时变更，因此不保留整目录快照，也不做整目录回滚。相同源码、SDK、Deno、所选构建工具、依赖 identity 和构建选项产生稳定的源码路径、入口、项目声明、lock 与 bundle 元数据。

项目内重复模块按稳定路径和内容 hash 去重。dependency key、版本或入口冲突在 Deno restore 前报告；缺失源码、无法解析的相对 import、Deno frozen check 失败或 所选构建工具 失败会使本次构建显式失败。

## 配置与交付

最终宿主配置 `JazorMode` 与 `JazorDir`，Emit 在 MSBuild 中运行：

```xml
<PropertyGroup>
  <JazorMode>debug</JazorMode>
  <JazorDir>$(MSBuildProjectDirectory)/jazor/</JazorDir>
</PropertyGroup>
```

- `debug`：保留项目源码、source map 和诊断 manifest，交由标准开发服务提供模块解析与 HMR。
- `release`：从同一项目根调用 所选构建工具，输出 `dist/` 与 bundle metafile。
- `JazorSSR=true`：在同一次项目生成与恢复中增加 `ssr-entry.js`，供 DenoHost 直接消费。

### 单一项目根（已定）

消费入口只接受**一个项目根**：`jazor/` 同时是模块输入、依赖恢复根、package 解析根与构建输出根。标准构建从 `entry.js` 和根 `package.json` 读取图，输出位置与文件名由选定工具配置；宿主侧的就绪探测以 `entry.js` 为主，并保留旧 bundle 路径兼容。`jazor-manifest.json` 若存在，只是 Emit 的诊断/增量证据。

早期契约把同一个目录拆成 `ArtifactRoot`/`SourceRoot`/`OutputRoot`/`PackageRoot` 四个参数传入；生产路径上它们始终指向同一处，拆开只把"必须一致"的责任推给调用方。区分"调用方是否已恢复依赖"的判据改为是否传入物化结果（`MaterializedLibraries`）：Emit 先在自己的项目根完成 restore 再传结果，直接调用 Toolchain 的调用方没有恢复过，需要在临时工作区补一次同样的 restore。

## 验收证据

- 生成的 `jazor/` 可由 Deno 2.9.7 在 `--no-remote` 与 frozen lock 下检查。
- `package.json` 的每个 dependency key 都对应一个精确 binding identity，源码中的每个 bare import 都能解析。
- 所选构建工具 metafile 展示入口可达的 JS、CSS、worker 和 static 资源；组件内部依赖保持完整，共享模块只出现一个实例。
- DenoHost SSR 使用同一 `node_modules` 与 `deno.lock`，浏览器与 SSR 的 package identity 一致。
- 重复 MSBuild 生成得到稳定项目文件、路径、source map 和 bundle 输出。
