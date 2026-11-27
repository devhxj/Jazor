# Reviewed API removals: Jazor 1.1.0

The NodeApi toolchain refactor retains C# compiler and component authoring semantics.
Vite/SSR now invoke APIs directly, so JazorViteServerOptions and JazorSsrOptions no
longer expose TaskName. Remove these settings when upgrading. The maintainer has
selected 1.1.0 for this infrastructure migration; other breaking changes continue
to follow the MAJOR rule. See CHANGELOG.md and nodejs-pnpm-migration-plan.md.

The comparison tool deduplicates signatures across types. These three entries
cover the property and its accessors on both options types. The 1.0.0 snapshot
remains unchanged; all removed signatures stay visible in generated reports.

  - System.String TaskName
  - System.String get_TaskName()
  - Void set_TaskName(System.String)

## Reviewed API removals for Jazor 2.7.0

The maintainer approved removing the following two attributes in this MINOR
release. See the 2.7.0 migration instructions in CHANGELOG.md. The frozen 1.0.0
snapshot predates these types. The allowlist records their exact type entries.
The constructors and `Index` signatures also occur on other public types.
The comparison tool deduplicates those signatures across types.

- type ECMAScript.ECMAScriptUndefinedArgumentAttribute
- type ECMAScript.ECMAScriptUndefinedDefaultAttribute

## Reviewed removal of standalone ASP.NET Core reload

Remove the standalone reload registrations and options when upgrading. The
frozen 1.0.0 baseline remains unchanged. See CHANGELOG.md for the migration.

- type Jazor.AspNetCore.Dev.JazorHmrMapping
- type Jazor.AspNetCore.Dev.JazorReloadExtensions
  - Microsoft.AspNetCore.Builder.IApplicationBuilder UseJazorReload(Microsoft.AspNetCore.Builder.IApplicationBuilder)
  - Microsoft.Extensions.DependencyInjection.IServiceCollection AddJazorReload(Microsoft.Extensions.DependencyInjection.IServiceCollection)
  - Microsoft.Extensions.DependencyInjection.IServiceCollection AddJazorReload(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action`1[Jazor.AspNetCore.Dev.JazorReloadOptions])
- type Jazor.AspNetCore.Dev.JazorReloadOptions
  - Boolean get_InjectHtml()
  - Boolean get_SuppressExternalRefreshPaths()
  - Boolean get_SuppressReconnectReloadForExternalRefresh()
  - Microsoft.AspNetCore.Http.PathString get_ClientScriptPath()
  - Microsoft.AspNetCore.Http.PathString get_WebSocketPath()
  - System.Collections.Generic.IList`1[Jazor.AspNetCore.Dev.JazorHmrMapping] get_HmrMappings()
  - System.Collections.Generic.IList`1[System.String] get_WatchPaths()
  - System.TimeSpan get_DebounceInterval()
  - System.TimeSpan get_KeepAliveInterval()
  - System.TimeSpan get_PollingInterval()
  - Void set_ClientScriptPath(Microsoft.AspNetCore.Http.PathString)
  - Void set_DebounceInterval(System.TimeSpan)
  - Void set_InjectHtml(Boolean)
  - Void set_KeepAliveInterval(System.TimeSpan)
  - Void set_PollingInterval(System.TimeSpan)
  - Void set_SuppressExternalRefreshPaths(Boolean)
  - Void set_SuppressReconnectReloadForExternalRefresh(Boolean)
  - Void set_WebSocketPath(Microsoft.AspNetCore.Http.PathString)
  - Boolean InjectHtml
  - Boolean SuppressExternalRefreshPaths
  - Boolean SuppressReconnectReloadForExternalRefresh
  - Microsoft.AspNetCore.Http.PathString ClientScriptPath
  - Microsoft.AspNetCore.Http.PathString WebSocketPath
  - System.Collections.Generic.IList`1[Jazor.AspNetCore.Dev.JazorHmrMapping] HmrMappings
  - System.Collections.Generic.IList`1[System.String] WatchPaths
  - System.TimeSpan DebounceInterval
  - System.TimeSpan KeepAliveInterval
  - System.TimeSpan PollingInterval

## Reviewed rename of ASP.NET Core SPA hosting

The previous frontend type and method names were replaced by the JazorSPA API. See CHANGELOG.md for the migration.

- type Jazor.AspNetCore.Dev.JazorFrontendExtensions
  - Microsoft.AspNetCore.Builder.IApplicationBuilder UseJazorFrontend(Microsoft.AspNetCore.Builder.IApplicationBuilder, System.Action`1[Jazor.AspNetCore.JazorHostOptions])
  - Microsoft.AspNetCore.Builder.WebApplication UseJazorFrontend(Microsoft.AspNetCore.Builder.WebApplication, System.Action`1[Jazor.AspNetCore.JazorHostOptions])
  - Microsoft.AspNetCore.Builder.WebApplicationBuilder AddJazorFrontend(Microsoft.AspNetCore.Builder.WebApplicationBuilder, System.Action`1[Jazor.AspNetCore.Dev.JazorFrontendOptions])
  - Microsoft.Extensions.DependencyInjection.IServiceCollection AddJazorFrontend(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action`1[Jazor.AspNetCore.Dev.JazorFrontendOptions])
- type Jazor.AspNetCore.Dev.JazorFrontendOptions
- type Jazor.AspNetCore.Dev.JazorFrontendUrls
