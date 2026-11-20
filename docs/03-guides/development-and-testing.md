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

## 清理本地输出

历次隔离测试、发布验证和样例构建会累积临时产物。先预览占用，再按需清理：

```bash
dotnet run --file scripts/csharp/clean-workspace.cs
dotnet run --file scripts/csharp/clean-workspace.cs -- --apply
```

清理前结束正在运行的构建和样例。脚本清理 `.tmp/`、`artifacts/`、`test/` 等忽略目录，以及 `src/`、`samples/` 中的 `bin/`、`obj/`、`node_modules/` 和生成输出；保留 Git 管理的文件、未忽略的新文件、NuGet 缓存、worktree 和 IDE 设置。清理后下一次构建会重新生成产物，npm 验证输入按对应脚本恢复。

## 聚焦测试

| 领域 | 命令 |
| --- | --- |
| 编译器 | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj` |
| Jolt 语法与编译 | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt` |
| Jolt SSR、路由与 hydration | `dotnet run --file scripts/csharp/verify-jolt-ssr.cs -- <Jolt.Cli-apphost> <Chromium-executable>` |
| Emit 与 bundle 快速回归 | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit` |
| Emit 完整 package consumer | `dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer` |
| CLR 映射 | `dotnet test src/Jazor.CLR.Test/Jazor.CLR.Test.csproj` |
| Vue Devtools binding | `dotnet test src/ECMAScript.Vue.Devtools.Test/ECMAScript.Vue.Devtools.Test.csproj` |
| 单个编译器类别 | `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"` |

`emit-consumer` 包含真实打包、SDK、NodeApi 与浏览器场景。日常修改应按 fixture 家族选择对应 lane；CI 也以同样的四条 lane 并行执行，避免一个测试进程串行占用全部门禁时间：

```bash
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-packages
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-style
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-core
dotnet run --file scripts/csharp/test-dotnet.cs -- --project emit-consumer-release
```

并行运行多个 `dotnet test` lane 时，应使用独立 `BaseOutputPath`；聚焦回归构建成功后优先使用 `--no-build`。测试创建的临时目录、端口、管道和进程标识必须隔离并在结束时清理。

### 复测 Jolt 编辑与渲染性能

先运行 `dotnet build src/Jolt.Cli/Jolt.Cli.csproj -c Release`。将以下占位符替换为该构建的 apphost 和本机 Chromium 可执行文件的绝对路径，逐项运行。

| 测量 | 命令 |
| --- | --- |
| 混编项目交替保存两种组件到浏览器更新 | `dotnet run --file scripts/csharp/verify-jolt-mixed.cs -- <Jolt-apphost> <Chromium> --benchmark 20` |
| C# 表达式或 Vue prop 增量编辑到 LSP 诊断 | `dotnet run --file src/Jolt.LanguageServer/verify.cs -- --benchmark-mixed 10 <Jolt-apphost>` |
| 生产千行列表与 SFC 同批对照 | `dotnet run --file samples/Jolt.Counter/benchmark-keyed-rows.cs -- <Jolt-apphost> <Chromium> .tmp/keyed-rows.json` |
| 独立声明文件变更引发编译错误及修复 | `dotnet run --file scripts/csharp/verify-jolt-dev-dependencies.cs -- <Jolt-apphost>` |

列表命令可在最后追加旧版 apphost，同一标签页会轮换旧版、候选版和 SFC 三份产物。混编 HMR 的计时包含前一次保存尚未完成的检查，LSP 的计时包含防抖。两项均在检查编辑结果后才接收下一次编辑。 [编辑反馈基线](../../samples/Jolt.Mixed/EditFeedback-baseline.md) · [列表基线](../../samples/Jolt.Counter/KeyedRows-baseline.md)

## 覆盖率门槛

| 范围 | 最低要求 | 验证入口 |
| --- | --- | --- |
| 核心编译器 | 10,000 个通过场景、98% 行覆盖率、97% 分支覆盖率 | `dotnet run --file scripts/csharp/verify-compiler-coverage.cs` |
| Vue 生态绑定 | 每个目标 90% 已审计公共绑定契约 | `dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs` |
| 绑定 XML 文档 | 公开声明、枚举值、XML 输出、上游快照和 nuspec 文档文件一致；Release 构建后由质量工作流上传报告 | `dotnet run --file scripts/csharp/verify-binding-documentation.cs -- --no-build --configuration Release` |

门槛是验证规则，不等同于任一历史报告中的固定通过数量。对当前结果的判断应运行相应脚本或测试命令。

## Emit 阶段输出与取消

正常构建会显示 `[Jazor Emit]` 阶段的开始、完成和耗时，包括模块收集、项目写入、依赖 restore/check、浏览器 bundle 与 SSR bundle。NodeApi 中的工具进度即时输出；失败保留 JS 异常、工具诊断、阶段与项目目录。

Emit 接收 Ctrl+C 后不再启动后续阶段。pnpm 原生 API 没有取消入口，恢复等待实际安装结束再传播取消；Vite/SSR/测试环境在 JS 线程可调度时终止事件循环并等待退出，不留下仍在执行的自有任务。同步无限循环尚不能强制打断。就地写入可能留下部分产物，再次构建会收敛，不承诺回滚。

## Emit 增量输出

Emit 在 `node_modules/.jazor-restore-state` 记录成功恢复的 package、lock 和工具链身份。外部依赖图不变时跳过重装，继续执行语法与 Vite 模块图检查。`file:` 包每次恢复时重新检查。Windows 上更换依赖前应退出使用该目录的开发宿主。

库资源按 manifest 内容 hash 保留未变化的文件和时间戳。内容更新、缺失或损坏时原子替换目标文件，错误由 Emit 上报。

## 改动边界

- 修改 `Jazor.CLR` 白名单来源后，运行 `Jazor.Compiler.Generator` 并提交重新生成的 `WhiteList.cs.Generate.cs`；生成器维护该文件。
- 新增编译语义时，优先保持求值顺序、副作用次数和最终结果，随后补充聚焦回归。
- Jolt 的 C# 语义使用 `Jazor.Compiler` 的翻译入口，模板与组件绑定由 Jolt 负责。

## 发布消费者验证

发布门禁运行隔离包消费者、NodeApi 工具链和浏览器验证。[NodeApi workflow](../../.github/workflows/nodeapi-verify.yml) 记录平台矩阵，[发布工作流](../../.github/workflows/nuget-publish-ref.yml) 验证发布 ref。

`.NET 11` GA SDK 及其 Roslyn 版本由 `global.json` 和 `Directory.Build.props` 固定。单文件脚本的 `#:package` 版本和 `Basic.Reference.Assemblies.Net110` 应同步升级。原生 `union` 仍需 C# preview。

项目内脚本、测试与验证入口见 [scripts/csharp README](../../scripts/csharp/README.md)。
