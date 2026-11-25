# Jero

Jero is an administration application with an Admin.NET-based backend and a frontend developed with [Jazor](https://github.com/devhxj/Jazor).

The backend uses ASP.NET Core, Furion, and SqlSugar. The frontend uses native Jolt pages, Vue Router, and Element Plus. Ordinary C# browser modules manage page state, permissions, and API calls. ASP.NET Core builds, runs, and publishes the application.

[简体中文](README.md)

## Architecture

| Part | Technology and responsibility |
| --- | --- |
| Backend | ASP.NET Core, Furion, and SqlSugar for business services, data access, and APIs |
| Frontend | `.jazor` views and shared controls, C# routing and state, Vue Router, and Element Plus |
| Host | Serves APIs and compiled SPA assets, with a fallback for browser routes |
| Extensions | Plugin projects for approval workflows, integrations, and business modules |

Pages run at the host's root path, API requests use same-origin `/api/**` URLs, and generated frontend assets use `/assets/**`.

Features include user, role, menu, organization, tenant, configuration, dictionary, localization, notification, file, log, online user, and scheduled job management, together with form design, print design, and plugin code editing.

## Repository layout

The sample lives in `samples/Jero`. Open [src/Jero.slnx](src/Jero.slnx) to work with all projects.

| Directory | Contents |
| --- | --- |
| `src/Jero.Web` | Unified Web host, backend HTTP pipeline, and frontend build/publish entry |
| `src/Jero.Web/SPA` | ASP.NET Core middleware that serves bundled assets and the SPA entry page for deep links |
| `src/Jero.Web/App` | Feature directories containing `.jazor` components, C# page state, API clients, and shared contracts |
| `src/Jero.Web/App/Shell` | Browser-side application layout, navigation, and shared state |
| `src/Jero.Core` | Backend services, entities, and data access |
| `src/Jero.Application` | Application layer and shared configuration |
| `src/Plugins` | Business and integration plugins |
| `src/Jero.Test` | Automated tests |
| `scripts/csharp` | Verification scripts and tools |

## Development

Use the .NET 11 SDK selected by [`src/global.json`](src/global.json). Restore dependencies from the Jero `src` directory. Jazor, ECMAScript.Vue, ECMAScript.VueRoute, and Jazor.Jolt use version `2.7.2` from NuGet.org.

MVC and SignalR share their `System.Text.Json` configuration. Responses encode `long` IDs as strings and use `yyyy-MM-dd HH:mm:ss` for timestamps. Static images support URL transformations such as `/images/logo.png?width=32&format=jpg`.

`Jero.Web.csproj` compiles the server and the C# sources under `App`. Jazor projects Element Plus and `sm2` from the app's npm manifest and writes the browser modules to `App/.jazor/modules`. Jolt uses the same `App/pnpm-lock.yaml` to bundle those modules with the Vue pages. The host publishes `App/dist`. Public sign-in pages load their form dependencies first. The admin layout loads after sign-in. The code editor, print designer, and Word and Excel viewers load when their respective features open.

Copy the JWT example to a local configuration file and set your signing parameters. Configure database connections and initialization in `Jero.Application/Configuration/Database.json`.

Configure an SM2 key pair for password sign-in with `Cryptogram__PublicKey` and `Cryptogram__PrivateKey`. Supply local credentials for Redis, Elasticsearch, the message broker, or external file storage when using those services.

```sh
cp Jero.Application/Configuration/JWT.json.example Jero.Application/Configuration/JWT.json
```

For email, SMS, OAuth, WeChat, or Alipay, copy the corresponding `*.json.example` and configure the services you use. Copy `DeepSeek.example` to `DeepSeek.json` for DeepSeek. Supply Alipay certificates locally at the configured paths.

```sh
dotnet tool restore
dotnet restore Jero.Web/Jero.Web.csproj
dotnet build Jero.Web/Jero.Web.csproj -c Debug --no-restore
dotnet run --project Jero.Web/Jero.Web.csproj -c Debug --no-build --launch-profile Jero.Web
```

The default URL is `http://localhost:5095`. Configure the listening port in the `Kestrel` section of `Jero.Web/appsettings.json` and launch options in `Jero.Web/Properties/launchSettings.json`. Routes and navigation guards live in `App/AppBootstrap.cs`. The literal imports in `App/route-modules.mjs` let Vite split pages into lazy chunks. For example, find the user view, state, and API client together under `Jero.Web/App/User`. Generated browser modules live in `Jero.Web/App/.jazor/modules`, and bundled assets live in `Jero.Web/App/dist/assets`.

The first build generates the Element Plus slot declarations used by the Jolt CLI and editor under `Jero.Web/App/Components/element-plus/obj`. Before running Jolt directly, you can generate them from `src` with `dotnet msbuild Jero.Web/Jero.Web.csproj -t:GenerateElementPlusContract`.

## Publishing

```sh
dotnet publish Jero.Web/Jero.Web.csproj -c Release -o ../output/publish
```

Release publishing includes the frontend assets. Deploy the complete publish directory, start it with `dotnet Jero.Web.dll`, and configure the target environment's database and integrations.

## Route verification

From `samples/Jero`, run the [route verification procedure](README.md#jolt-路由验收) against a disposable Release publish copy. It starts the published app and uses an isolated Chromium profile to check navigation and mock API responses.

## Tracked files

The repository includes source code, configuration examples, automated tests, project documentation, and required static assets. Databases, logs, uploads, business import data, debug screenshots, temporary verification output, local configuration, and deployment materials are excluded through `.gitignore` and maintained per environment.
