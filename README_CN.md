<div align="center">

[![今日诗词](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)](https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate)

<h1>Jazor</h1>

<p><strong>用 Jolt 和 C# 开发 Vue 应用，用 Jazor 将 C# 编译为 ECMAScript 模块。</strong></p>

<p>
  <a href="https://dotnet.microsoft.com/"><img alt=".NET 11" src="https://img.shields.io/badge/.NET-11-512BD4?logo=dotnet&amp;logoColor=white" /></a>
  <a href="https://www.nuget.org/packages/Jazor"><img alt="NuGet" src="https://img.shields.io/nuget/v/Jazor?logo=nuget&amp;label=NuGet" /></a>
  <a href="https://github.com/devhxj/Jazor/releases/tag/v2.7.4"><img alt="GitHub release" src="https://img.shields.io/github/v/tag/devhxj/Jazor?tag=v2.7.4&amp;display_name=tag&amp;label=release" /></a>
  <a href="LICENSE.txt"><img alt="MIT 许可证" src="https://img.shields.io/badge/license-MIT-2ea44f" /></a>
</p>

<p>
  <a href="docs/04-roadmap/current-status.md"><img alt="至少 10,000 项编译器测试通过" src="https://img.shields.io/badge/compiler%20tests-10%2C000%2B%20passing-2ea44f" /></a>
  <a href="docs/04-roadmap/current-status.md"><img alt="编译器行覆盖率至少 98%" src="https://img.shields.io/badge/compiler%20line%20coverage-98%25%2B-2ea44f" /></a>
  <a href="docs/04-roadmap/current-status.md"><img alt="编译器分支覆盖率至少 97%" src="https://img.shields.io/badge/compiler%20branch%20coverage-97%25%2B-2ea44f" /></a>
</p>

<p><a href="docs/03-guides/jolt-authoring.md">Jolt 快速开始</a> · <a href="samples/Jero/README.md">C# SPA 示例</a> · <a href="docs/README.md">文档</a> · <a href="CHANGELOG.md">更新日志</a></p>

<p><a href="README.md">English</a> · <strong>简体中文</strong></p>

</div>

## 运行 Jolt 应用

使用 .NET 11 SDK，在 Visual Studio 打开 `samples/Jolt.AspNetCore/Jolt.AspNetCore.csproj`，选择同名启动配置，按 **F5**。在终端也可以从仓库根目录进入项目后运行。

```sh
cd samples/Jolt.AspNetCore
dotnet run
```

打开 `http://localhost:5168/portal/frontend/index.html`，点击计数器，再修改项目根目录下 `Counter.jazor` 中的标题。页面会更新并保留计数。Web 宿主启动由本仓库源码构建的 Jolt 开发服务。Jero 管理应用也可以按 F5 或运行 `dotnet run` 启动。[计数器项目](samples/Jolt.AspNetCore/README.md) · [Jero 开发说明](samples/Jero/README.md)

在 `.jazor` 文件中编写标记和 C#。

```razor
<button @onclick="@Increment">Count: @Count</button>

@code {
    private int Count;
    private void Increment() => Count++;
}
```

每个 `.jazor` 文件生成一个 ES 模块，默认导出 Vue 组件，公开静态方法和字段以及公开常量生成具名导出。每个挂载的组件实例持有自己的状态。Jolt 读取 npm 包的公开 TypeScript 声明来检查组件用法，并导入包发布的 ESM 导出。 [组件写法](docs/03-guides/jolt-authoring.md)

### 编辑与构建

| 编辑器 | 扩展 |
| --- | --- |
| VS Code | 在扩展菜单中选择 **Install from VSIX**，安装 `jolt-2.7.4.vsix`。 [VS Code 配置](editors/jolt/README.md) |
| Visual Studio 2026（Windows x64） | 双击 `Jolt.VisualStudio.vsix`，通过 VSIX Installer 安装。 [Visual Studio 配置](editors/jolt-visual-studio/README.md) |
| JetBrains Rider 2026.3 | 在 **Settings → Plugins** 中从磁盘安装 `jolt-rider-2.7.4.zip`。 [Rider 配置](editors/jolt-rider/README.md) |

2.7.4 的编辑器扩展通过同版 .NET 工具启动语言服务。按照上表的编辑器配置安装后，打开示例项目的 `Counter.jazor` 即可使用补全和转到定义。[2.7.4 发布页](https://github.com/devhxj/Jazor/releases/tag/v2.7.4)

在 Web 项目目录运行 `dotnet build` 构建项目，`dotnet publish -c Release` 将宿主、前端入口和生产资源一起发布。[计数器项目的发布目标](samples/Jolt.AspNetCore/Jolt.AspNetCore.csproj)

## 编译 C# 模块

在 .NET 项目中引用 `Jazor` SDK，即可编译带 `[ECMAScriptModule]` 的类。该包包含编译器、分析器、产物工具和 ASP.NET Core 集成。 [C# 模块快速开始](docs/03-guides/quick-start.md)

```sh
dotnet add package Jazor --version 2.7.4
```

| 主题 | 文档 |
| --- | --- |
| SDK 配置、产物模式和 ASP.NET Core SSR | [安装与配置](docs/03-guides/installation-and-configuration.md) |
| 复用 C# 类库和 NuGet 包 | [普通类库导入](docs/04-roadmap/ordinary-nuget-compilation-research.md) |
| 编译器和产物管线 | [系统架构](docs/01-overview/system-architecture.md) |
| 支持范围和测试要求 | [当前状态](docs/04-roadmap/current-status.md) |

## 开发 Jazor

在仓库根目录用 [global.json](global.json) 选定的 SDK 执行以下命令。

```sh
dotnet build Jazor.slnx
dotnet run --file scripts/csharp/test-dotnet.cs -- --project jolt
dotnet run --file scripts/csharp/test-dotnet.cs
```

发布工作流验证 Jolt CLI 的隔离安装、语言服务和浏览器交互，并构建两套 VSIX。 [开发与测试](docs/03-guides/development-and-testing.md)

## 最新发布

Jazor 2.7.4 让 Emit 和 Jolt 共用 `.jazor/` 产物目录。ASP.NET Core 宿主通过 `AddJazorSPA` 和 `UseJazorSPA` 接入 Jolt，按 F5 或执行 `dotnet run` 即可启动。Jolt 会跟踪 C# 源码依赖的变化。已有项目升级前请查看[发布说明](CHANGELOG.md#jazor-274)中的破坏性变更及迁移步骤。

## AI 协作开发

Jazor 核心编译器和 CLR 支持的第一版，以及首批 500 个测试，均由人工手写完成，前后投入近两年时间，奠定了项目的技术基础。

在此基础上，AI 主要协助推进文档完善、测试覆盖、外部库封装、迭代优化，以及 CI 和版本管理。后续协作主要使用智谱 GLM-5 系列和 GPT-5 系列；项目方向、代码审查和发版决策仍由维护者负责。

## 致谢

Jazor 使用了 [Roslyn](https://github.com/dotnet/roslyn)、[Acornima](https://github.com/adams85/acornima)、[node-api-dotnet](https://github.com/microsoft/node-api-dotnet)、[pnpm](https://github.com/pnpm/pnpm)、[Vite](https://github.com/vitejs/vite) 与 [WebRef](https://github.com/w3c/webref)。早期工作也受益于 Netpack、DenoHost，以及 [WootzJs](https://github.com/kswoll/WootzJs)、[h5](https://github.com/curiosity-ai/h5)、[SharpKit](https://github.com/SharpKit/SharpKit) 等 C# 到 JavaScript 项目。

## 许可证与反馈

Jazor 使用 [MIT 许可证](LICENSE.txt)。安全问题请通过 [GitHub Security Advisories](https://github.com/devhxj/Jazor/security/advisories/new) 私下报告；其他问题可使用 [Issues](https://github.com/devhxj/Jazor/issues) 或 [Discussions](https://github.com/devhxj/Jazor/discussions)。
