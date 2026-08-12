# Deployment Guide

## Publish Command

```powershell
dotnet publish samples/Wiki/Wiki.csproj -c Release -p:JazorMode=release -o <deploy-dir>
```

Optional subpath deployment:

```powershell
$env:Wiki__PathBase = "/docs"
dotnet <deploy-dir>\Wiki.dll --urls http://0.0.0.0:8080
```

`Wiki__PathBase` / `Wiki:PathBase` enables ASP.NET Core `UsePathBase(...)` so the same publish output can be mounted below a reverse-proxy prefix such as `/docs`.

## Directory Structure Contract

Published output must follow this layout:

```
<deploy-dir>/
  Wiki.dll
  Wiki.exe
  jazor/
    bundle.js
    bundle.js.map
    __jazor_netpack_entry__.js
    vendor/
      <Netpack runtime dependencies>
  wwwroot/
    site.css
    favicon.svg
    vendor/
      vue@3.5.16.mjs
```

## Key Invariants

The following invariants are enforced by `wiki-verify-smoke.cs --publish`:

1. `/jazor/*` must resolve from `<deploy-dir>/jazor/` through the explicit Jazor artifact mount
2. `bundle.js` and `bundle.js.map` must exist under `<deploy-dir>/jazor/`
3. Debug module graph files (`main.mjs`, `jazor-manifest.json`, `components/`, `style.mjs`) must not leak into a non-SSR release publish
4. `wwwroot/jazor/` must not be used as a fallback or shadow the generated artifact graph
5. `/vendor/vue@3.5.16.mjs` must exist and be servable
6. Registered docs routes must return HTTP 200 with route-correct first-response metadata and the SPA shell
7. Search routes are utility surfaces, so they must emit `noindex, nofollow` and must not appear in `sitemap.xml`
8. Unknown docs routes must return HTTP 404 with the recoverable shell and `X-Robots-Tag: noindex, nofollow`
9. HTML responses must carry `Referrer-Policy: strict-origin-when-cross-origin`, `X-Content-Type-Options: nosniff`, and `X-Frame-Options: DENY`
10. `host/index.template.html` must not reference any external CDN URL
11. When `Wiki__PathBase` / `Wiki:PathBase` is configured, first-response HTML, discovery documents, static assets, and SPA navigation must all stay correct beneath that subpath

## Verification

Run before every deployment:

```powershell
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --publish
dotnet run --file .\scripts\csharp\wiki-verify-browser.cs -- --publish
dotnet run --file .\scripts\csharp\wiki-verify-smoke.cs -- --publish --path-base /docs
dotnet run --file .\scripts\csharp\wiki-verify-browser.cs -- --publish --path-base /docs
```

This checks structural invariants, discovery docs, route metadata and headers, all registered docs routes, the search/404 indexing contract, release bundle/source-map content, generated `ECMAScript.Style` registration, and real browser runtime behavior.

## Rollback Procedure

1. Keep the previous publish output as `<deploy-dir>.previous/`
2. Deploy the new version by renaming the current directory to `.previous` and extracting the new output
3. If verification fails, roll back by renaming `.previous` back

## Health Check

`/health` returns HTTP 200 with body `ok`. Use this endpoint for load balancer or monitoring probes.
