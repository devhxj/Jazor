# Jazor.NodeHost

共享的进程内 JavaScript 宿主实现。当前供 WebIDL 试点使用；Emit、Vite 和 SSR 的全局迁移正在进行，尚未切换产品主线。

`NodeEnvironment` 管理单个 JS 线程与环境，全进程共享一次 Node platform 初始化。`PnpmBootstrap` 引导固定原生安装引擎；`NodeToolchain` 在内嵌环境调用安装和 Vite API。内部接口通过既有 Jazor 包交付，暂不建立新的公开宿主 API。

第三方版本集中在仓库 `Directory.Build.props`。Windows 调用方必须使用 `CETCompat=false` 的 apphost；当前 LibNode 为旧 Node 20 系列，维护版本、平台覆盖和即时取消仍待完成产品级验证。

Rolldown 的 napi-rs loader 将 Windows `shared_library` 误判为 GNU 构建。当前桥接使用其显式 native library path，在该 environment 的私有 `process.env` 对象上选择已安装的 MSVC 绑定，加载完成立即恢复；不会修改 OS 环境或启动 CLI。此限制具有真实失败场景，不能机械套用到其他库。

验证命令：

```text
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet run --file scripts/csharp/verify-vite-nodeapi.cs
```

Vite 探针验证 pnpm 恢复、原生绑定加载、开发服务启动/HTTP 响应/关闭和生产构建，并断言 JS PID 与 C# PID 一致。它不证明整个 ASP.NET Core/HMR/SSR 发布闭包已经切换。
