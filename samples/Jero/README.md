# Jero

Jero 是一个后端基于 Admin.NET、前端使用 [Jazor](https://github.com/devhxj/Jazor) 开发的后台管理项目。

后端使用 ASP.NET Core、Furion 和 SqlSugar。前端使用原生 Jolt 页面、Vue Router 和 Element Plus，普通 C# 浏览器模块负责页面状态、权限判断和业务请求。ASP.NET Core 宿主统一构建、运行和发布前后端。

[English](README.en.md)

## 项目架构

| 部分 | 技术与职责 |
| --- | --- |
| 后端 | ASP.NET Core、Furion、SqlSugar，提供业务服务、数据访问和管理 API |
| 前端 | `.jazor` 页面与共享控件、C# 路由和状态、Vue Router、Element Plus |
| 宿主 | 托管 API 和构建后的 SPA，业务深链接由 SPA 回退处理 |
| 扩展 | 通过插件工程组织审批流、第三方集成和业务模块 |

浏览器通过宿主根路径访问后台页面，使用同源 `/api/**` 请求后端，前端生成资源位于 `/assets/**`。

## 主要功能

- 用户、角色、菜单、组织、职位与租户管理。
- 系统配置、字典、多语言、通知公告与文件管理。
- 操作日志、异常日志、在线用户与任务调度。
- 表单设计、打印模板设计与动态插件代码编辑。
- 审批流及业务插件扩展。

## 目录结构

样例位于 `samples/Jero`。解决方案入口是 [src/Jero.slnx](src/Jero.slnx)。

| 目录 | 内容 |
| --- | --- |
| `src/Jero.Web` | 统一 Web 启动宿主、后端 HTTP 管线与前端生成发布 |
| `src/Jero.Web/SPA` | ASP.NET Core 托管打包后的静态资源，向直接访问页面路由的请求返回 SPA 入口 |
| `src/Jero.Web/App` | 按功能组织的 `.jazor` 组件、C# 页面状态、API 客户端和共享契约 |
| `src/Jero.Web/App/Shell` | 浏览器端的应用布局、导航和共享状态 |
| `src/Jero.Core` | 后端基础服务、实体与数据访问 |
| `src/Jero.Application` | 应用层与共享配置 |
| `src/Plugins` | 业务与集成插件 |
| `src/Jero.Test` | 自动化测试 |
| `scripts/csharp` | 验证脚本与工具 |

## 开发与运行

使用 [`src/global.json`](src/global.json) 指定的 .NET 11 SDK。从 Jero 的 `src` 目录还原依赖。Jazor、ECMAScript.Vue、ECMAScript.VueRoute 和 Jazor.Jolt 均使用 NuGet.org 上发布的 `2.7.2`。

MVC 与 SignalR 共用 `System.Text.Json` 配置。响应中的 `long` ID 写为字符串，日期使用 `yyyy-MM-dd HH:mm:ss`。静态图片仍支持 URL 变换，例如 `/images/logo.png?width=32&format=jpg`。

`Jero.Web.csproj` 编译服务端和 `App` 下的 C#，Jazor 从应用的 `package.json` 投影 Element Plus 和 `sm2`，并将浏览器模块写入 `App/.jazor/modules`。Jolt 使用同一份 `App/pnpm-lock.yaml` 将这些模块与 Vue 页面打包，宿主发布 `App/dist`。公开登录页只加载表单依赖，登录后再加载后台布局。代码编辑器、打印设计器以及 Word 和 Excel 预览器在打开对应功能时加载。

首次运行前，将 JWT 配置示例复制为本机配置，并填写自己的签名参数；数据库连接与初始化选项在 `Jero.Application/Configuration/Database.json` 中配置。

密码登录需要为当前环境配置自己的 SM2 密钥对（`Cryptogram__PublicKey`、`Cryptogram__PrivateKey`）。使用 Redis、Elasticsearch、消息队列或外部文件存储时，填写相应服务的本机凭据。

```sh
cp Jero.Application/Configuration/JWT.json.example Jero.Application/Configuration/JWT.json
```

邮件、短信、OAuth、微信和支付宝按所需功能复制相应的 `*.json.example`，填写本机配置；DeepSeek 使用 `DeepSeek.example` 复制为 `DeepSeek.json`。支付宝证书按配置路径在本机准备。

```powershell
dotnet tool restore
dotnet restore Jero.Web/Jero.Web.csproj
dotnet build Jero.Web/Jero.Web.csproj -c Debug --no-restore
dotnet run --project Jero.Web/Jero.Web.csproj -c Debug --no-build --launch-profile Jero.Web
```

默认访问地址为 `http://localhost:5095`，监听端口在 `Jero.Web/appsettings.json` 的 `Kestrel` 中配置，开发启动选项见 `Jero.Web/Properties/launchSettings.json`。路由和导航守卫在 `App/AppBootstrap.cs` 中声明，`App/route-modules.mjs` 为 Vite 提供可静态识别的懒加载入口。例如，用户列表的视图、页面状态和 API 客户端均位于 `Jero.Web/App/User`。构建生成的浏览器模块位于 `Jero.Web/App/.jazor/modules`，打包资产位于 `Jero.Web/App/dist/assets`。

首次构建会在 `Jero.Web/App/Components/element-plus/obj` 生成 Jolt CLI 和编辑器使用的 Element Plus 插槽声明。从 `src` 单独运行 Jolt 前，可用 `dotnet msbuild Jero.Web/Jero.Web.csproj -t:GenerateElementPlusContract` 生成这份声明。

## 发布

```powershell
dotnet publish Jero.Web/Jero.Web.csproj -c Release -o ../output/publish
```

Release 发布会同时生成前端资源。部署时使用完整发布目录，以 `dotnet Jero.Web.dll` 启动，并配置目标环境的数据库及所需集成服务。

## Jolt 路由验收

从 `samples/Jero` 目录运行以下 PowerShell 命令。验收启动真实的 Release 发布宿主，并在 Chromium 中模拟会话与 API 响应。请使用一次性的发布副本。以下配置使用该副本里的空 SQLite 文件，关闭数据库初始化，并用仅供本地验收的值替换 OAuth 和 JWT 配置。

```powershell
$fixture = ".tmp/jero-routes-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Force .tmp | Out-Null
Copy-Item output/publish $fixture -Recurse
$db = (New-Item -ItemType File "$fixture/Smoke.db").FullName
@{
    DbConnection = @{ ConnectionConfigs = @(@{
        DbType = "Sqlite"
        ConnectionString = "Data Source=$db"
        DbSettings = @{ EnableInitDb = $false; EnableInitView = $false }
        TableSettings = @{ EnableInitTable = $false }
        SeedSettings = @{ EnableInitSeed = $false }
    }) }
} | ConvertTo-Json -Depth 8 | Set-Content "$fixture/Configuration/Database.json" -Encoding utf8
'{"OAuth":{"Gitee":{"ClientId":"browser-smoke","ClientSecret":"browser-smoke"},"Weixin":{"ClientId":"browser-smoke","ClientSecret":"browser-smoke"}}}' |
    Set-Content "$fixture/Configuration/OAuth.json" -Encoding utf8
'{"JWTSettings":{"IssuerSigningKey":"local-jero-route-smoke-only-dummy-signing-key"}}' |
    Set-Content "$fixture/Configuration/JWT.json" -Encoding utf8
dotnet run --file scripts/csharp/verify-jolt-routes.cs -- $fixture "C:/Program Files/Google/Chrome/Application/chrome.exe"
```

脚本在回环地址启动宿主和 Chromium 调试端口，使用独立的浏览器配置，并输出宿主日志所在的临时目录。

## 仓库文件范围

仓库保留源码、配置示例、自动化测试、项目说明及运行所需的静态资源。数据库、日志、上传文件、业务导入数据、调试截图、临时验收输出、本机配置和部署材料通过 `.gitignore` 排除，由各环境自行维护。
