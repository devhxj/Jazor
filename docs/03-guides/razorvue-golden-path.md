# RazorVue Golden Path

本文是新项目采用 RazorVue 的推荐入口。它定义一条可复制的作者路径：使用标准 Razor 和强类型 Vue binding 编写页面，经官方 Razor Source Generator 生成 C#，再由 RazorVue 输出 Vue render-function 模块，并在独立 package consumer 与真实浏览器中验收。

## 从哪个样本开始

先阅读并运行 [RazorVue.Authoring](../../samples/RazorVue.Authoring/README.md)。它是最小的完整样本，包含 TDesign 表单、表格、typed slot、`@bind`、路由、query、`NavigationManager` 以及 package Release/browser smoke。需要参考更大规模管理应用时，再阅读 [JazorAdmin](../../samples/JazorAdmin/README.md)。

样本中的 `TaskBoard`、`TaskDetails` 和 `TaskTable` 展示了推荐的组件拆分方式：页面负责状态和业务流程，表格负责展示与用户操作，API client 负责 endpoint 边界，组件库负责控件行为。

## 推荐的项目结构

```text
MyApp/
  Components/
    Layout/
    Pages/
    Shared/
  Contracts/          # 请求、响应和表单 DTO
  Clients/            # 强类型 browser API client
  Services/           # 仅浏览器可执行的 adapter
  Bootstrap.cs        # Vue mount、资源和 route host framing
```

页面和组件使用 `.razor`/`.razor.cs`，公共数据模型使用明确的 record/class。服务端数据库、请求上下文和 Identity 操作留在 endpoint；组件注入的是有明确返回类型的 browser client。不要把服务端 service、`object` catch-all 或字符串 JavaScript 互操作带入组件。

## 最小 CRUD 页面

一个可交付页面至少应明确以下状态：

- `Loading`、空数据、错误和成功状态；
- 查询条件、分页和当前页数据；
- 新增/编辑 draft，校验失败时保留用户输入；
- 提交中防重复提交，完成后刷新列表；
- 至少一个 typed event、一个 `@bind` 和一个 slot；
- 同一页面的 `@page` route 与应用自有 route host。

推荐用组件库提供的具体类型表达这些状态。例如 TDesign 的 `TForm<T>`、`TInput<T>`、`TTable<T>`、`EventCallback` 和 `RenderFragment<T>` 可以直接由 official Razor SG 绑定；页面不需要手写 `RenderTreeBuilder`、桥接组件或 cast。

## 验证顺序

从仓库根目录执行以下命令。所有输出都写到 `.tmp` 隔离目录，不会覆盖默认 `bin/` 产物：

```text
dotnet run --file samples/RazorVue.Authoring/build-local.cs -- --source-only --configuration Debug --work-root .tmp/authoring-local-build-debug
dotnet run --file samples/RazorVue.Authoring/build-local.cs -- --configuration Release --work-root .tmp/authoring-local-build
dotnet run --file samples/RazorVue.Authoring/verify-smoke.cs -- --skip-build --work-root .tmp/authoring-local-build --package-output .tmp/nupkg-sample/RazorVue.Authoring
```

第一条命令确认源码引用和 official Razor SG；第二条命令构建本地包、独立 package consumer 以及 Release artifact；第三条命令检查模块、source map、manifest、资源闭包、PathBase 和浏览器交互。没有 Edge/Chrome/Chromium 时可使用 `--skip-browser`，但这只能证明静态产物，不能把浏览器路径标记为完成。

正式提交前，再运行适用的完整门禁：

```text
dotnet run --file scripts/csharp/test-dotnet.cs -- --project razor-sg
dotnet run --file scripts/csharp/verify-razorvue-coverage.cs
dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs
```

需要比较编译优化前后的构建时间时，使用固定输入运行基线脚本。脚本默认执行 3 轮，在 `.tmp` 下隔离输出，并记录 clean、incremental、HMR 与 Release 的每轮耗时和中位数；`--skip-hmr` 或 `--skip-release` 只适合没有对应运行环境的本地探查，不能作为完整 P0 证据。

```text
dotnet run --file scripts/csharp/benchmark-razorvue-build.cs -- --work-root .tmp/razorvue-build-benchmark
```

## 支持边界

这条路径支持“使用 Blazor/Razor 语法表达 Vue 组件”的开发范式。它不承诺完整 Blazor Server/WebAssembly 或 CLR parity。Microsoft 内置 UI 组件、`IJSRuntime` 字符串互操作、未映射外部成员、server-only service、未经版本化的 SSR state handoff 和复杂 `popstate`/`hashchange` cancellation 仍按作者指南报告为 Guidance/Reject。

遇到限制时，先查看 [RazorVue 诊断矩阵](./razorvue-diagnostic-matrix.md)，再回到 [作者指南](./razorvue-authoring.md) 的对应章节；不要通过弱化 C# 类型或拼接 JavaScript 绕过诊断。
