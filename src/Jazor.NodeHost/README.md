# Jazor.NodeHost

共享的进程内 JavaScript 宿主实现。WebIDL、编译器、CLR、Razor SG、Style、Devtools、Emit、Vite 和 Vue SSR 使用同一宿主。运行条件与旧配置迁移见[工具链指南](../../docs/03-guides/toolchain-migration.md)，平台和消费者验收入口见[当前状态](../../docs/04-roadmap/current-status.md)。

`NodeEnvironment` 管理单个 JS 线程与环境，全进程共享一次 Node platform 初始化。`PnpmBootstrap` 引导固定原生安装引擎；`NodeToolchain` 在内嵌环境调用安装和 Vite API。内部接口通过既有 Jazor 包交付，暂不建立新的公开宿主 API。

`NodeModuleRunner` 在独立 environment 中加载测试模块，按顺序等待注册的异步断言，并把异常传回 MSTest。测试专用 ESM loader 只映射 fixture 的本地替身；真实包由 pnpm 恢复。原生 pnpm 引擎位于调用者提供的共享 store，避免删除临时项目时遭遇 Windows 原生模块文件锁；当前 environment 销毁不会卸载进程级原生 addon。

并行 environment 关闭各自的 inspector 和 debug signal handler，避免重复初始化进程级 inspector 状态。Node.js 26 提供 CLR Half 切片所需的 `Math.f16round`，相关 CLR 运行时场景验证舍入、NaN、无穷大与有符号零。

第三方版本集中在仓库 `Directory.Build.props`。当前使用微软 LibNode 26.0.18（Node.js 26.13.1）和 NodeApi 0.9.39，原生资产覆盖 Windows 和 glibc Linux 的 x64、ARM64。Windows 宿主和 NuGet 消费者使用 `CETCompat=false` 的 apphost。平台前置条件见[工具链指南](../../docs/03-guides/toolchain-migration.md)。

宿主直接拥有 NodeApi runtime 与 JS 线程。SSR、测试模块与 Vite 生命周期取消通过 JS 线程调用 `TerminateEventLoop`，等待线程退出后才返回；已验证取消后定时器不再写文件、SSR 容量恢复和应用释放。它不能中断占满 JS 线程的同步无限循环。pnpm 原生安装 API 没有取消接口，恢复阶段等待安装实际结束后再传播取消，不能强行中止仍可能写 store 的 Rust 原生任务。

Rolldown 的 napi-rs loader 将 Windows `shared_library` 误判为 GNU 构建。当前桥接使用其显式 native library path，在该 environment 的私有 `process.env` 对象上选择已安装的 MSVC 绑定，加载完成立即恢复；不会修改 OS 环境或启动 CLI。此限制具有真实失败场景，不能机械套用到其他库。

Linux 上先加载系统 OpenSSL 3，再全局加载 LibNode，供原生 addon 解析 `napi_*`。LibNode 使用 `RTLD_DEEPBIND` 优先绑定内置 OpenSSL，系统库优先服务 .NET 的 TLS 调用。Vite 探针验证这两条路径在同一进程中工作。

Vite 的 CSS 压缩固定使用官方 `lightningcss-wasm`，版本与原生 Lightning CSS 一致，集中于 `Directory.Build.props`。其旧版 napi-rs Windows addon 只从主 executable 查找 Node-API，遇到真实 TDesign CSS 图会终止进程。恢复时将 `vite>lightningcss` 的固定 override 写入根 `package.json`，传给 pnpm API 并纳入 lock/增量身份；所有平台使用相同实现，不在失败后切换执行路线。

验证命令：

```text
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet run --file scripts/csharp/verify-vite-nodeapi.cs
dotnet run --file scripts/csharp/verify-nodeapi-pipeline.cs
```

Vite 探针验证 pnpm 恢复、原生绑定加载、开发服务启动/HTTP 响应/关闭和生产构建，并断言 JS PID 与 C# PID 一致。管线探针验证源码 SSR、浏览器/SSR Vite 构建及只复制 SSR runtime 的部署。WebIDL 默认比较固定 Deno 对照指纹和生成文件；EmitTest 覆盖代理、PathBase、WebSocket HMR 与 hydration。跨平台验证见 [NodeApi workflow](../../.github/workflows/nodeapi-verify.yml)。
