# Jero

Jero is a C# administration SPA. ASP.NET Core, Furion, and SqlSugar provide the APIs. Jolt components use Vue Router and Element Plus. C# browser modules implement navigation, page state, and same-origin API calls. [简体中文](README.md)

## Development

Open [src/Jero.slnx](src/Jero.slnx) in Visual Studio using the .NET 11 SDK selected by [global.json](src/global.json). Set **Jero.Web** as the startup project, choose the **Jero.Web** launch profile, and press **F5**. The default address is `http://localhost:5095`. The sample references Jazor and Jolt from this checkout. Its first build restores NuGet packages and locked npm dependencies.

To start the same profile from the command line, run this from `samples/Jero/src`.

```sh
dotnet run --project Jero.Web/Jero.Web.csproj --launch-profile Jero.Web
```

Debug builds check `.jazor` files and report component errors with line and column numbers. In Development the host starts Jolt and proxies Vite HTTP requests and hot updates. Access both the SPA and the same-origin API through the web host. [Startup](src/Jero.Web/Startup.cs)

Jazor Emit and Jolt share the project's `.jazor/` directory. For example, `AppBootstrap` declares the module `components/app-bootstrap.js`. The generated file is `.jazor/components/app-bootstrap.js`, and the HTML entry uses `src="/components/app-bootstrap.js"`. Setting the MSBuild property `JazorDir` moves the generated files. The same import paths work in components, C# and HTML. [Module declaration](src/Jero.Web/Composition/AppBootstrap.cs) · [HTML entry](src/Jero.Web/index.html)

SQLite connection and seed settings live in `Jero.Application/Configuration/Database.json`. The first development start generates any missing JWT signing key and SM2 key pair and saves them in the developer's user profile for subsequent starts.

## Layout

| Directory under `src` | Responsibility |
| --- | --- |
| `Jero.Web/` | ASP.NET Core host and frontend root, HTML entry, Vite configuration, npm manifest and lockfile |
| `Jero.Web/Pages/` | Feature-specific Jolt views and C# page state, with shared controls in `Components` |
| `Jero.Web/Application/` | Client session state, menus and route resolution |
| `Jero.Web/Infrastructure/` | HTTP transport and feature API clients, browser storage and real-time connections |
| `Jero.Web/Composition/` | Vue application entry, router and lazy page loaders |
| `Jero.Core/` | Backend entities, data access and system management APIs |
| `Jero.Application/Modules/` | Built-in ApprovalFlow, DingTalk, GoView, K3Cloud, ReZero and WorkWeixin modules |
| `Jero.Test/` | Service and HTTP contract tests |

For example, the user management page lives in `Jero.Web/Pages/User` and its API client in `Jero.Web/Infrastructure/Api/User`. The login page, administrative layout, Monaco editor, print designer and Office viewers have separate loading paths. Jolt reads Element Plus component, prop and slot metadata from the locked npm package.

## Publishing

Run this from `samples/Jero/src`.

```sh
dotnet publish Jero.Web/Jero.Web.csproj -c Release -o ../output/publish
```

Vite builds production assets under `.jazor/dist/`. Publishing places those files alongside the host's static files in `wwwroot/`. Deploy the complete publish directory and start the Jero.Web apphost from it. The host serves `wwwroot/index.html` for eligible SPA deep links. Static images support URL transformations such as `/images/logo.png?width=32&format=jpg`. APIs and SignalR share System.Text.Json settings and serialize long IDs as strings.

Supply production JWT and SM2 keys via configuration. See `Jero.Application/Configuration/JWT.json.example` and set `Cryptogram__PublicKey` and `Cryptogram__PrivateKey` for SM2. Configure external services such as OAuth, mail and messaging when enabling those integrations. Set `SPA:PathBase` when deploying under a subpath. See the [built-in module configuration](src/Jero.Application/Modules/README.md).

## Verification

From `samples/Jero/src`, run the test project. Its browser tests start an isolated host and require the Playwright Chromium build used by the project. The separate route verifier takes the publish directory and a local Chrome or Chromium executable. It exercises the production shell, image transformations and routes, using a mock session and API responses for business interactions.

Verification uses `test/jero/` at the repository root. Each route check has its own directory for the host copy, SQLite database, browser profile and logs, including when checks run concurrently.

On a new checkout, use PowerShell from `samples/Jero/src` to install the Chromium revision selected by the test project. The Playwright NuGet package supplies the installation script. Run tests in the same terminal to use the repository-local browser cache.

```powershell
dotnet build Jero.Test/Jero.Test.csproj
$env:PLAYWRIGHT_BROWSERS_PATH = [IO.Path]::GetFullPath('../../../test/jero/browser/browsers')
pwsh Jero.Test/bin/Debug/net11.0/playwright.ps1 install chromium --only-shell
```

```sh
dotnet test --project Jero.Test/Jero.Test.csproj
dotnet run --file ../scripts/csharp/verify-jolt-routes.cs -- ../output/publish "C:/Program Files/Google/Chrome/Application/chrome.exe"
```
