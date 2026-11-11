# Jazor.NodeHost

共享的进程内 JavaScript 宿主实现。WebIDL、编译器、CLR、Razor SG、Style 和 Devtools 运行时测试使用同一宿主；Emit、Vite 和 SSR 的全局迁移及正式消费者验收仍在进行。

`NodeEnvironment` 管理单个 JS 线程与环境，全进程共享一次 Node platform 初始化。`PnpmBootstrap` 引导固定原生安装引擎；`NodeToolchain` 在内嵌环境调用安装和 Vite API。内部接口通过既有 Jazor 包交付，暂不建立新的公开宿主 API。

`NodeModuleRunner` 在独立 environment 中加载测试模块，按顺序等待注册的异步断言，并把异常传回 MSTest。测试专用 ESM loader 只映射 fixture 的本地替身；真实包由 pnpm 恢复。原生 pnpm 引擎位于调用者提供的共享 store，避免删除临时项目时遭遇 Windows 原生模块文件锁；当前 environment 销毁不会卸载进程级原生 addon。

并行 environment 关闭各自的 inspector 和 debug signal handler，避免旧 libnode 重复初始化进程级 inspector 状态。当前 CLR Half 切片需要 `Math.f16round`，宿主使用 .NET `Half` 转换补齐旧 Node 缺失的 binary16 运算；相关 CLR 运行时场景验证舍入、NaN、无穷大与有符号零。

第三方版本集中在仓库 `Directory.Build.props`，当前 LibNode 为 `20.1800.215`。技术栈固定使用 `CETCompat=false` 的 apphost，仓库宿主和 NuGet 消费者由构建配置统一设置；此项是已接受的运行条件。该 LibNode 仍基于旧 Node 20 系列，升级此包不代表已经升级到新的 Node 主版本，维护版本与平台覆盖仍待完成产品级验证。

宿主直接拥有 NodeApi runtime 与 JS 线程。SSR、测试模块与 Vite 生命周期取消通过 JS 线程调用 `TerminateEventLoop`，等待线程退出后才返回；已验证取消后定时器不再写文件、SSR 容量恢复和应用释放。它不能中断占满 JS 线程的同步无限循环。pnpm 原生安装 API 没有取消接口，恢复阶段等待安装实际结束后再传播取消，不能强行中止仍可能写 store 的 Rust 原生任务。

Rolldown 的 napi-rs loader 将 Windows `shared_library` 误判为 GNU 构建。当前桥接使用其显式 native library path，在该 environment 的私有 `process.env` 对象上选择已安装的 MSVC 绑定，加载完成立即恢复；不会修改 OS 环境或启动 CLI。此限制具有真实失败场景，不能机械套用到其他库。

验证命令：

```text
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet run --file scripts/csharp/verify-vite-nodeapi.cs
dotnet run --file scripts/csharp/verify-nodeapi-pipeline.cs
```

Vite 探针验证 pnpm 恢复、原生绑定加载、开发服务启动/HTTP 响应/关闭和生产构建，并断言 JS PID 与 C# PID 一致。管线探针进一步验证真实物化入口、源码 SSR、浏览器/SSR Vite 构建及只复制 SSR runtime 的部署。EmitTest 覆盖 ASP.NET Core 代理、PathBase、WebSocket HMR 与浏览器 hydration；NuGet/MSBuild、全仓库脚本与跨平台交付仍待完成。
