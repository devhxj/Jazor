# NodeApi 工具链与旧项目迁移

当前构建、依赖恢复和 SSR 使用进程内 `Microsoft.JavaScript.NodeApi`、LibNode、固定 pnpm API 与 Vite/Vue API。所选版本集中在 [Directory.Build.props](../../Directory.Build.props)，运行时约束见 [Jazor.NodeHost](../../src/Jazor.NodeHost/README.md)。

## 当前运行条件

- 使用 [global.json](../../global.json) 选择的 .NET 正式 SDK；原生 union 编写仍使用 C# preview 语言模式。
- Windows 必须运行当前输出中 `CETCompat=false` 的 apphost。仓库和 NuGet 消费者的构建配置负责设置，不修改系统设置。
- Jazor 维护的安装、转译、开发服务器和 SSR 路径直接调用 API，不启动 Node、npm、pnpm、tsc 或 Deno CLI，也不在 API 失败时切换到命令行。
- `pnpm-lock.yaml` 是依赖锁；共享 store 由 `JazorPnpmStoreDirectory` 或 Emit `--pnpm-store` 指定。应用配置和锁文件保持作者所有权。
- 源码中的 `Jazor.LibNode 0.2.0` 使用 Node.js 26.13.1，提供 `win-x64`、`win-arm64`、`linux-x64` 和 `linux-arm64`。Windows 需要对应架构的 Microsoft Visual C++ 2015–2022 Redistributable。Linux 需要 glibc 2.39 和提供 `GLIBCXX_3.4.32` 的 libstdc++。

## 从微软 LibNode 迁移

按 lockstep 升级 Jazor 系列包。如果项目直接引用 `Microsoft.JavaScript.LibNode` 或其 RID 子包，移除此引用，由 `Jazor` 统一引入新的原生运行库。用切换后的版本重新构建并发布到干净目录。

使用 macOS 或 Windows x86 的项目，可以把 Emit 和 SSR 迁至上述平台，或继续使用迁移前的 Jazor 版本。静态浏览器产物仍可部署到任意能托管静态文件的平台。

交叉发布时，Emit 选用构建机的原生库，NuGet 选用部署目标的原生库。跨 CPU 架构发布时，使用 `PlatformTarget=AnyCPU` 干净重建，确保 Emit 可以加载应用的托管程序集。

pnpm 安装 API 不提供即时终止：进入原生安装后，取消会等待实际操作完成再传播。依赖恢复不执行 lifecycle shell scripts。`package.json` 中的标准 scripts 可描述前端项目用途，但 Jazor 不把任意 shell script 解释为可执行的内嵌 API。

## 从 Deno 工具链升级

该切换随 1.1.0 交付。使用旧版本时，应阅读相应 tag 的文档；新项目直接按[安装与配置](./installation-and-configuration.md)创建。

| 旧配置 | 迁移方式 |
| --- | --- |
| `JazorDenoPath`、`JAZOR_DENO_PATH`、Emit `--deno` | 移除旧 runtime 路径配置，使用随包 NodeApi/LibNode 资产 |
| Vite / SSR 的 `TaskName` | 将定制放入明确的 Vite/SSR 配置和受支持 API；任意 shell 任务不由内嵌宿主执行 |
| WebIDL `--deno-config` | 使用 `--node-project <package.json>`，同目录提供 collector 的 `host.cjs` |
| Deno 安装结果与 `deno.lock` | 审阅项目后恢复 pnpm 依赖，生成并提交 `pnpm-lock.yaml`；旧锁按迁移步骤处理 |
| 依赖 Deno 的自定义 scripts | 改为目标工具支持的配置/API，并验证 Debug、Release 与适用 SSR 行为 |

升级步骤：

1. 将所有 Jazor/ECMAScript 包统一到同一发布版本。
2. 审阅生成目录中的作者配置、旧 scripts、运行时路径与锁文件；保留需要合并的定制。
3. 清理项目拥有的旧安装结果，使用新工具链重新恢复依赖，审阅锁文件解析差异。普通构建不删除其他包管理器的作者文件。
4. 验证模块、source map、开发服务器、HMR、PathBase、Release 资源和适用 SSR/hydration。
5. 向干净发布目录重新交付。生产 SSR 消费已发布 bundle 和 runtime 资产，不在目标机器恢复工具依赖。

完整删除生成目录只适用于确认目录可再生的项目；含作者配置的目录先备份并合并。

## 验证入口

```bash
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet run --file scripts/csharp/verify-nodeapi-pipeline.cs
dotnet run --file scripts/csharp/verify-windows-spa-release.cs -- --path-base /docs
dotnet run --file scripts/csharp/verify-windows-ssr-release.cs -- --path-base /todo
```

仓库工具项目通过 `dotnet run --file scripts/csharp/restore-node-project.cs -- <directory>` 恢复依赖。跨平台验证由 [NodeApi workflow](../../.github/workflows/nodeapi-verify.yml)记录实际组合，Windows 发布消费者由[官方发布工作流](../../.github/workflows/nuget-publish-ref.yml)在上传前执行。

版本例外与兼容性要求见[发版规则](./release-and-versioning.md)，候选比较与早期探针结论见[历史演进](../05-history/evolution.md)。
