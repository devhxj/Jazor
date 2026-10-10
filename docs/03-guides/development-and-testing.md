# 开发与测试

> 面向：Jazor 仓库维护者与贡献者。

## 环境与构建

使用仓库 [global.json](../../global.json) 指定的 .NET SDK。在仓库根目录执行：

```bash
dotnet restore Jazor.slnx
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs
```

`Jazor.slnx` 是解决方案入口。仓库自动化采用 `scripts/csharp/` 下的单文件 C# 程序，不新增 PowerShell build、test、publish 或诊断 wrapper。

## 聚焦测试

| 领域 | 命令 |
| --- | --- |
| 编译器 | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj` |
| Razor-to-Vue 集成 | `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj` |
| Emit 与 bundle 快速回归 | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit` |
| Emit 完整 package consumer | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer` |
| CLR 映射 | `dotnet test src/Jazor.CLR.Test/Jazor.CLR.Test.csproj` |
| Vue Devtools binding | `dotnet test src/ECMAScript.Vue.Devtools.Test/ECMAScript.Vue.Devtools.Test.csproj` |
| Vue Data UI binding | `dotnet test src/ECMAScript.VueDataUi.Test/ECMAScript.VueDataUi.Test.csproj` |
| Lucide binding | `dotnet test src/ECMAScript.Lucide.Test/ECMAScript.Lucide.Test.csproj` |
| 单个编译器类别 | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"` |

`emit-consumer` 包含 48 个真实打包、SDK、Deno 与浏览器场景。日常修改应按 fixture 家族选择对应 lane；CI 也以同样的四条 lane 并行执行，避免一个测试进程串行占用全部门禁时间：

```bash
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-packages
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-style
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-core
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-release
```

并行运行多个 `dotnet test` lane 时，应使用独立 `BaseOutputPath`；聚焦回归构建成功后优先使用 `--no-build`。测试创建的临时目录、端口、管道和进程标识必须隔离并在结束时清理。

## 覆盖率门槛

| 范围 | 最低要求 | 验证入口 |
| --- | --- | --- |
| 核心编译器 | 10,000 个通过场景、98% 行覆盖率、97% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Razor-to-Vue | 4,000 个通过场景、90% 行覆盖率、94% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-razorvue-coverage.cs` |
| Vue 生态绑定 | 每个目标 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| Vue 绑定版本漂移 | 三套生成器 `--check`、上游快照版本、原始注释说明和 manifest identity；可归档 schema `1.0` JSON/Markdown 报告，并可用 baseline + `--fail-on-baseline-drift` 阻断 inventory 漂移 | `dotnet run --file scripts/csharp/verify-vue-binding-contracts.cs -- --report .tmp/binding-contracts.json` |
| 绑定 XML 文档 | 公开声明、枚举值、XML 输出、上游快照和 nuspec 文档文件一致；Release 构建后由质量工作流上传报告 | `dotnet run --file scripts/csharp/verify-binding-documentation.cs -- --no-build --configuration Release` |
| RazorVue 构建性能 | clean、incremental、HMR、Release 的可重复时间基线；clean/incremental 另记录模块/source map/manifest/Emit 输出的原始与 gzip 体积和增量产物变化；JSON 旁生成 Markdown 摘要，质量工作流按周归档 3 轮 clean/incremental 趋势 | `dotnet run --file scripts/csharp/benchmark-razorvue-build.cs -- --samples 3` |
| 样例业务回归矩阵 | 定时/手动质量门禁串行运行 `RazorVue.Authoring` 与 `JazorAdmin` 的 Release smoke，记录每个场景的退出码、耗时、命令、提交和工具链上下文，并归档 JSON/Markdown 报告；浏览器验证默认启用，本地快速迭代可显式 `--skip-browser` | `dotnet run --file scripts/csharp/verify-sample-regression-matrix.cs -- --report artifacts/quality/sample-regression/report.json` |

门槛是验证规则，不等同于任一历史报告中的固定通过数量。对当前结果的判断应运行相应脚本或测试命令。

## Emit 阶段输出与取消（未发布源码）

正常构建会显示 `[Jazor Emit]` 阶段的开始、完成和耗时，依次包括模块收集、工程写入、依赖 restore/check、Release 浏览器 bundle，以及启用时的 SSR bundle。Deno 的 stdout/stderr 即时输出；失败的最终诊断仍保留原始输出和退出码，并指出失败阶段与工程目录。

Emit CLI 接收 Ctrl+C 后取消当前工作并终止自身 Deno 进程树。DenoHost 2.9.7 的启动后等待先完成，再处理取消，避免启动过程中丢失已创建进程的所有权。Emit 就地写入，取消可能留下部分产物；再次构建会按现有契约收敛，不承诺回滚。

## Release 资源报告

`benchmark-razorvue-build.cs` 的 Release 采样附带资源报告；也可只读分析已经构建或发布的消费者 `dist`，不会重建消费者或清理 benchmark 工作区：

```powershell
dotnet run --file scripts/csharp/benchmark-razorvue-build.cs -- --release-artifacts D:/consumer/publish/jazor/dist --out .tmp/consumer-assets.json
```

JSON 与 Markdown 分别记录入口、静态依赖、lazy、source map、metadata 和其它文件的逐文件原始/gzip 体积。共享资源只计一次；入口的静态闭包优先于动态引用。manifest 声明的资源缺失会失败，报告输出必须位于输入 `dist` 之外。

schema `razorvue-build-v3` 增加 SDK、源码提交、操作系统、架构及采样参数。Debug 产物不计恢复得到的 `node_modules`，模块数和 manifest 体积从当前样例的真实隔离 `obj` 读取；这一统计口径与旧 v2 不同。gzip 使用 .NET `SmallestSize` 逐文件估算，不能视作 HTTP 压缩或首屏实际请求。`artifact-scan` 的耗时仅表示报告扫描，不能作为构建耗时。

当前 schema `razorvue-build-v4` 还支持真实消费者的构建与浏览器观测。消费者保持自己的 NuGet/cache 配置；工具与消费者提交分别记录。先在消费者准备好同版本包并 restore，再从上游执行：

```powershell
dotnet run --file scripts/csharp/benchmark-razorvue-build.cs -- --consumer-project D:/consumer/src/Host/Host.csproj --skip-hmr --samples 3 --work-root .tmp/consumer-benchmark
```

每轮使用新的前端目录和输出目录；clean 是已 restore 的 C# `Rebuild`，下载缓存保持温热，incremental 是同目录的无改动 build，Release 是空目录 publish。这不是清空全机缓存的冷启动指标。应用 manifest 从消费者实际 `IntermediateOutputPath` 读取。`--skip-release` 可省略发布采样。

消费者 HMR 应在自己的运行浏览器中测量。默认样例的 `hmr` 行表示整个 HMR 验证脚本的墙钟时间，不能当作消费者更新延迟。浏览器采集器可通过 `--browser-observations observations.json` 附入当前报告，也可与 `--release-artifacts` 合用。输入是 `BrowserObservation[]`，字段为 `Scenario`、`Sample`、`Browser`、`Url`、`CacheDisabled`、`ReadyMilliseconds` 与 `Resources`；每个资源记录 `Path`、`DurationMilliseconds`、`DecodedBodySize`、`EncodedBodySize`、`TransferSize`。

`ReadyMilliseconds` 的起点和可见条件必须随采集器记录。资源体积来自原生 Resource Timing，同一资源的多次请求保留，缓存响应可能有 0 transfer；这些数值与逐文件 gzip 估算分开呈现。首屏的请求列表据浏览器实际观察填写，不能用 manifest 静态闭包代替。

当前源码的 Emit 会在 `node_modules/.jazor-restore-state` 记录成功 restore/check 的 package/lock 身份。外部依赖图没有变化时只运行 Deno check，避免 HMR 构建覆写运行中的 Vite 原生模块。删除 node_modules 后正常恢复；更改 package 或锁文件会重新安装，file: 包不使用这个根锁复用条件。依赖确有变化时，Windows 上先退出使用该目录的开发宿主再构建。

库资源物化也按 manifest 内容 hash 保留未变化的目标文件和时间戳，不因作者修改而触发无关 Vite 更新或替换 Windows 正在读取的文件。源资源先按既有契约校验；内容更新、缺失或损坏的目标仍走原子写入，错误继续传播。

## 改动边界

- 修改 `Jazor.CLR` 白名单来源后，运行 `Jazor.Compiler.Generator` 并提交重新生成的 `WhiteList.cs.Generate.cs`；生成器维护该文件。
- 新增编译语义时，优先保持求值顺序、副作用次数和最终结果，随后补充聚焦回归。
- RazorVue 的 C# 语义必须使用 `Jazor.Compiler` translation hooks；不要在集成层拼接 JavaScript 或重建 AST 语义。
- `Jazor.Admin` 是库，`samples/JazorAdmin` 是示例；测试与文档应分别说明它们的职责。

## 发布消费者兼容矩阵

发布工作流和本地消费者门禁使用以下基线；升级其中任一项时，应先在独立 consumer 中重跑 SPA、SSR、PathBase、HMR 和浏览器验证：

| 层 | 当前基线 | 验证入口 |
| --- | --- | --- |
| .NET SDK | `11.0.100-rc.1.26425.128`（由 `global.json` 固定） | `dotnet build Jazor.slnx`、质量门禁 |
| Node.js | `20`、`22`（Wiki Chrome browser consumer 矩阵）；发布 workflow 使用 `22`，本地 benchmark 应记录实际版本 | `wiki-verify.yml`、发布 workflow、`benchmark-razorvue-build.cs` |
| 浏览器 | Windows SPA/Wiki/SSR 统一使用 Google Chrome headless | SPA/SSR、Wiki browser scripts |
| 发布配置 | Release、NuGet 本地源、`JazorMode=release` | SPA/SSR consumer scripts |
| 部署路径 | `/docs`（SPA）、`/todo`（SSR） | 对应 Windows 发布消费者门禁 |

Node 与浏览器的绝对版本由运行器提供；`wiki-verify.yml` 的 Wiki 与发布浏览器 consumer 使用 Node 22（浏览器脚本依赖 Node 22 提供的 WebSocket 客户端），`quality-gates.yml` 与 `wiki-verify.yml` 会上传 `toolchain-matrix.md`，记录每次运行的实际 SDK、Node、Chrome、操作系统和 commit。本地运行结果不构成跨版本保证。组件库升级还需先通过 `verify-vue-binding-contracts.cs`，再运行发布消费者验证。

项目内脚本、测试说明和特殊验证路径见 [scripts/csharp README](../../scripts/csharp/README.md)。
