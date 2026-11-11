# npm/JSR 绑定与标准 Jazor 项目

> 当前契约：`jazor/` 是由 Emit 物化、供进程内 pnpm、Vite 与 Vue SSR API 共同消费的标准前端项目。NodeApi 切换的验收与限制见[迁移方案](./nodejs-pnpm-migration-plan.md)；稳定边界见[类库契约](../02-architecture/library-artifact-contract.md)和[产物管线](../02-architecture/artifact-pipeline.md)。

## 项目与所有权

最终宿主拥有一个标准项目根。编译器负责 C# 语义与 import 收集，Emit 负责项目物化，`Jazor.NodeHost` 负责进程内工具调用和环境生命周期。pnpm 解析依赖，Vite 处理标准模块图、CSS、资源、HMR 与 bundle，Vue SSR 执行渲染。

```text
jazor/
  package.json
  pnpm-lock.yaml
  node_modules/
  entry.js
  ssr-entry.js          # 启用 SSR 时生成
  ssr-bundle-entry.js   # SSR 构建入口
  vite.config.js
  ssr.vite.config.js
  clr/
  runtime/vue/
  <generated-modules>
  dist/
```

根 `package.json` 声明 `"type": "module"`。默认项目路径使用 `.js`；作者显式声明的 `.mjs` 与上游 specifier 保持原样。RazorVue 的模块语义、catalog 和 source map 契约不因包管理器变化而改变。

Emit 只管理其生成文件。源码与 map 使用单文件写入，过期生成文件通过清单差异清理；不复制或替换整个含 `node_modules` 的项目根。失败显式传播，后续构建重新收敛。作者配置、scripts 与额外依赖不能被普通构建任意删除。

## 两类输入

| 输入 | 项目中的表示 |
| --- | --- |
| ECMAScript 源码 carrier | 按声明路径写入 `clr/**`、runtime 或其他项目源码目录，使用相对 ESM import |
| npm/JSR 绑定 | C# 强类型 API、package identity、公开 ESM specifier 与样式 side-effect import；运行时代码由包管理器恢复 |

绑定声明包括 `[ECMAScript]` 模块入口、名字机制给出的导出名、`[Style]` 样式边、精确包身份，以及上游许可证和 inventory。成员级模块声明优先于类型级声明。编译器在使用处收集这些声明，Emit 不重新分析 C# 表达式或绑定包内部结构。

组件优先使用上游公开的细粒度入口；上游只有根入口时保留根入口。样式由普通 side-effect import 表达，工具链依据上游 `exports`、`conditions`、`sideEffects` 和依赖图处理，不引入 Jazor 私有 CSS 拼接、import map 或合成 npm 包投影。

## 包身份与冲突

| 来源 | 安装声明 | 生成源码 |
| --- | --- | --- |
| npm | dependency key 对应精确版本或明确 npm alias | 保留公开 bare specifier |
| JSR | 保留绑定中的 JSR 身份，通过 npm 兼容注册表安装 | 使用 `@jsr/...` 公开入口 |
| 项目源码 | 不创建外部 package identity | 根据最终文件位置使用相对 import |

JSR 项目配置 `@jsr:registry=https://npm.jsr.io`。例如 `@std/path@1.0.8` 对应 npm 兼容名 `@jsr/std__path`，源码可导入 `@jsr/std__path/posix`。绑定身份与安装表示之间的转换由统一的 package identity 逻辑负责。

多个绑定可以共同声明同一精确外部包身份；无关的绑定自身名称或版本不构成冲突。同一 dependency key 指向不同版本或不兼容声明时，恢复前失败。bare import 缺少所需包声明时同样显式失败。

## 依赖恢复与检查

固定版本集中在 `Directory.Build.props`。恢复通过内嵌 Node 中的 `@pnpm/napi` 完成，采用固定 hoisted linker、自动 peer 安装和 `ignoreScripts=true`；不执行 shell lifecycle script。依赖包含 file/workspace 安装需求时，必须按当前已验证的 API 能力处理，不能默默调用 CLI。

`pnpm-lock.yaml` 是唯一恢复锁。根依赖与 override 一致时执行 frozen restore；安装成功状态同时记录依赖图、引擎、Node、平台、架构和 lock 身份，以跳过未变化的重复安装。失败不会留下成功状态。旧包管理器 lock 不参与判断，升级时由作者审阅后清理。

Vite 使用与原生版本一致的官方 `lightningcss-wasm`，固定 override 写入 `package.json` 并纳入恢复身份；所有平台一致。普通 pnpm 复现也能读取该选择。

模块检查通过 Vite 的实际模块转换与解析图执行，不启动 HTTP/WebSocket 监听。它覆盖入口语法、导入解析和工具转换错误；C# 类型检查仍由 Roslyn 负责，不能把此检查描述为完整 TypeScript 类型检查。

## 开发、构建与 SSR

开发服务经 Vite API 在进程内启动，ASP.NET Core 代理 HTTP/WebSocket 并管理 ready、失败、重建和关闭。Release 经 Vite API 构建 browser 与启用后的 SSR bundle。自定义构建行为放入 Vite 配置或 plugin，shell scripts 不再是执行入口。

开发 SSR 读取已恢复的同一标准项目。生产 SSR 只消费已发布的 bundle 与所需 runtime 资产，不在部署机器安装工具依赖或写入临时 JS runner。请求仍使用明确的 state/provider 协议；失败不能静默回退到旧产物或 CSR。

环境取消等待 JS 线程退出后返回。同步无限循环无法被该协议中断；pnpm 原生安装没有取消接口，必须等待实际安装结束再传播取消，以免 native store 写任务遗留。Windows 必须启动当前构建的 `CETCompat=false` apphost。

## 验收入口

| 契约 | 可复现证据 |
| --- | --- |
| npm/JSR、alias、版本变化与 frozen lock | `StandardPackageProjectTests` |
| package identity 合并与资源闭包 | `LibraryMaterializerTests` |
| 增量与干净构建确定性 | `SdkIntegrationTests`；比较源码、map、配置和 lock，排除安装缓存 |
| Vite 生命周期与同进程执行 | `verify-vite-nodeapi.cs` |
| restore/build/部署 SSR | `verify-nodeapi-pipeline.cs` |
| proxy、HMR、SSR 池与取消 | `JazorViteProxyTests`、`JazorSsrHostingTests`、`EmitProgressTests` |
| 干净 NuGet SPA/SSR 消费者 | `verify-windows-spa-release.cs`、`verify-windows-ssr-release.cs` |
| 四平台组合与包内资产 | `nodeapi-verify.yml` |

tree shaking 必须通过实际 bundle 证明：只使用某组件时不拉入无关组件源码或 CSS；组件所需内部依赖、动态 import、字体、worker 和 wasm 仍完整。源码与 NuGet consumer 使用同一声明及标准模块图。

编译语义、覆盖率、绑定契约和真实浏览器门槛继续以[当前状态](./current-status.md)为准。工具迁移不得降低它们。正式版后移除旧配置、任务或锁文件契约按 MAJOR 处理，见[发版规则](../03-guides/release-and-versioning.md)。
