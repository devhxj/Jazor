# Jero

Jero 是一个用 C# 开发的后台管理 SPA。ASP.NET Core、Furion 和 SqlSugar 提供业务 API。浏览器端使用 Jolt 的 `.jazor` 组件、Vue Router 和 Element Plus，普通 C# 模块负责导航、页面状态和同源 API 调用。[English](README.en.md)

## 开发与运行

在本仓库中打开 [src/Jero.slnx](src/Jero.slnx)，使用 [global.json](src/global.json) 指定的 .NET 11 SDK。将 **Jero.Web** 设为启动项目，选择 **Jero.Web** 配置后按 **F5**。默认地址为 `http://localhost:5095`。该示例直接引用当前仓库的 Jazor 和 Jolt 源码，首次构建需要还原 NuGet 和锁文件中指定的 npm 依赖。

命令行从 `samples/Jero/src` 启动同一个开发配置：

```sh
dotnet run --project Jero.Web/Jero.Web.csproj --launch-profile Jero.Web
```

Debug 构建会检查 `.jazor`，错误定位到组件中的行和列。开发宿主启动 Jolt 并代理 Vite 的 HTTP 请求和热更新。打开宿主地址即可同时调试页面和同源 `/api`。[启动配置](src/Jero.Web/Startup.cs)

Jazor Emit 与 Jolt 共用项目下的 `.jazor/` 作为生成根。举例来说，`AppBootstrap` 声明的模块名是 `components/app-bootstrap.js`，磁盘文件是 `.jazor/components/app-bootstrap.js`，HTML 入口写 `src="/components/app-bootstrap.js"`。覆盖 MSBuild 的 `JazorDir` 即可改变磁盘位置，组件、C# 导入和 HTML 继续使用相同的模块路径。[模块声明](src/Jero.Web/Composition/AppBootstrap.cs) · [HTML 入口](src/Jero.Web/index.html)

数据库连接及种子设置在 `Jero.Application/Configuration/Database.json`，默认使用 SQLite。首次开发启动会为缺失的 JWT 签名密钥和 SM2 密钥对生成本机值并保存在用户配置目录，重启后复用。

## 目录和依赖

| 路径（相对 `src`） | 内容 |
| --- | --- |
| `Jero.Web/` | ASP.NET Core 启动项目，也是前端根目录。包含 HTML 入口、Vite 配置、npm 清单和锁文件 |
| `Jero.Web/Pages/` | 按功能组织的 `.jazor` 页面和 C# 页面状态，共享组件位于 `Components` |
| `Jero.Web/Application/` | 客户端会话、菜单、路由和业务状态 |
| `Jero.Web/Infrastructure/` | HTTP 请求、业务 API 客户端、浏览器存储和实时连接 |
| `Jero.Web/Composition/` | Vue 应用入口、路由表和页面懒加载 |
| `Jero.Core/` | 后端实体、基础服务、数据访问和系统管理 API |
| `Jero.Application/Modules/` | 随应用编译的审批流、钉钉、GoView、K3Cloud、ReZero 和企业微信模块 |
| `Jero.Test/` | 服务和 HTTP 契约测试 |

例如，用户页位于 `Jero.Web/Pages/User`，请求实现在 `Jero.Web/Infrastructure/Api/User`。公开登录页面独立加载，后台布局、Monaco 编辑器、打印设计器和 Office 预览按功能载入。Element Plus 的组件、属性和插槽信息来自锁定的上游 npm 包。

## 运行与发布

从 `samples/Jero/src` 构建发布目录：

```sh
dotnet publish Jero.Web/Jero.Web.csproj -c Release -o ../output/publish
```

Vite 将生产资源构建到 `.jazor/dist/`。`dotnet publish` 将其中的文件放进发布目录的 `wwwroot/`，与宿主原有静态文件一起部署。在完整发布目录启动 Jero.Web 后，SPA 深链接由 `wwwroot/index.html` 响应。图片支持 URL 变换，例如 `/images/logo.png?width=32&format=jpg`。API 与 SignalR 共用 System.Text.Json 配置，长整型 ID 输出为字符串。

生产环境需配置自己的 JWT 签名和 SM2 密钥。JWT 配置见 `Jero.Application/Configuration/JWT.json.example`，SM2 使用 `Cryptogram__PublicKey` 和 `Cryptogram__PrivateKey`。邮件、短信、OAuth、微信、支付宝等按实际启用的功能配置。模块的启用条件见[内置业务模块](src/Jero.Application/Modules/README.md)。若部署在子路径，使用 `SPA:PathBase` 设置路径前缀。

## 验证

从 `samples/Jero/src` 运行测试。`dotnet test` 会为浏览器测试启动独立宿主，运行前须安装该测试项目使用的 Playwright Chromium。单独执行路由验收脚本时，传入上一步的发布目录和 Chrome 或 Chromium 可执行文件。脚本验证生产入口、图片 URL 和浏览器路由，业务交互使用模拟会话和 API 响应。

验收使用仓库根目录的 `test/jero/`。路由脚本为每轮运行创建独立目录，保存宿主副本、SQLite、浏览器配置和日志。并行运行时各自使用独立目录。

首次安装测试使用的浏览器，可在 `samples/Jero/src` 下的 PowerShell 中运行以下命令。Playwright 安装脚本由测试项目的 NuGet 依赖在构建时提供。同一终端继续运行测试即可使用仓库内的浏览器缓存。

```powershell
dotnet build Jero.Test/Jero.Test.csproj
$env:PLAYWRIGHT_BROWSERS_PATH = [IO.Path]::GetFullPath('../../../test/jero/browser/browsers')
pwsh Jero.Test/bin/Debug/net11.0/playwright.ps1 install chromium --only-shell
```

```sh
dotnet test --project Jero.Test/Jero.Test.csproj
dotnet run --file ../scripts/csharp/verify-jolt-routes.cs -- ../output/publish "C:/Program Files/Google/Chrome/Application/chrome.exe"
```
