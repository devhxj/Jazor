# Jazor.NodeHost

共享的进程内 JavaScript 宿主实现。WebIDL、编译器、CLR、Razor SG、Style 和 Devtools 运行时测试使用同一宿主；Emit、Vite 和 SSR 的全局迁移及正式消费者验收仍在进行。

`NodeEnvironment` 管理单个 JS 线程与环境，全进程共享一次 Node platform 初始化。`PnpmBootstrap` 引导固定原生安装引擎；`NodeToolchain` 在内嵌环境调用安装和 Vite API。内部接口通过既有 Jazor 包交付，暂不建立新的公开宿主 API。

`NodeModuleRunner` 在独立 environment 中加载测试模块，按顺序等待注册的异步断言，并把异常传回 MSTest。测试专用 ESM loader 只映射 fixture 的本地替身；真实包由 pnpm 恢复。原生 pnpm 引擎位于调用者提供的共享 store，避免删除临时项目时遭遇 Windows 原生模块文件锁；当前 environment 销毁不会卸载进程级原生 addon。

并行 environment 关闭各自的 inspector 和 debug signal handler，避免旧 libnode 重复初始化进程级 inspector 状态。当前 CLR Half 切片需要 `Math.f16round`，宿主使用 .NET `Half` 转换补齐旧 Node 缺失的 binary16 运算；相关 CLR 运行时场景验证舍入、NaN、无穷大与有符号零。

第三方版本集中在仓库 `Directory.Build.props`。当前技术栈固定使用 `CETCompat=false` 的 apphost，仓库宿主和 NuGet 消费者由构建配置统一设置；此项是已接受的运行条件。当前 LibNode 为旧 Node 20 系列，维护版本、平台覆盖和即时取消仍待完成产品级验证。

Rolldown 的 napi-rs loader 将 Windows `shared_library` 误判为 GNU 构建。当前桥接使用其显式 native library path，在该 environment 的私有 `process.env` 对象上选择已安装的 MSVC 绑定，加载完成立即恢复；不会修改 OS 环境或启动 CLI。此限制具有真实失败场景，不能机械套用到其他库。

验证命令：

```text
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet run --file scripts/csharp/verify-vite-nodeapi.cs
```

Vite 探针验证 pnpm 恢复、原生绑定加载、开发服务启动/HTTP 响应/关闭和生产构建，并断言 JS PID 与 C# PID 一致。它不证明整个 ASP.NET Core/HMR/SSR 发布闭包已经切换。
