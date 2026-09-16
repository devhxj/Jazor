---
name: jazor
description: Guide developers using Jazor 1.0 preview to install lockstep packages, author typed C# to ECMAScript modules, build RazorVue and SSR applications, use Vue bindings, and diagnose Emit output. Use for Jazor setup, authoring, configuration, migration, or troubleshooting; do not use for unrelated generic ASP.NET Core or Vue work.
---

# Jazor Usage Guide

Help the user reach a working Jazor build with the smallest change that fits the product contract. Answer in the user's language. Prefer the repository's current guides and source over remembered API names.

## Route the request

Identify the workflow before suggesting code:

- **Core modules:** a class library marked with `ECMAScriptModule` and a final executable or web host that runs Emit.
- **RazorVue:** a Razor SDK project that authors components and references both `Jazor` and `Jazor.Vue`; the official Razor Source Generator is the input.
- **SSR/host integration:** an ASP.NET Core host that configures `JazorMode`, `JazorDir`, and, when needed, `JazorSSR` and the Jazor ASP.NET Core services.
- **Bindings:** an explicit `ECMAScript.*` package selected for the Vue ecosystem or another JavaScript library. Keep all Jazor and ECMAScript package versions lockstep.
- **Troubleshooting:** distinguish package/reference problems, compiler support errors, Razor SG errors, and Emit/output-directory errors before proposing a fix.

Inspect the project's `.csproj`, `global.json`, package references, and existing Jazor properties first. For a repository checkout, use these authoritative entries when relevant:

- `docs/03-guides/quick-start.md`
- `docs/03-guides/installation-and-configuration.md`
- `docs/03-guides/razorvue-authoring.md`
- `docs/03-guides/razorvue-quickstart.md`
- `docs/02-architecture/compiler.md`
- `docs/02-architecture/artifact-pipeline.md`
- `docs/02-architecture/library-artifact-contract.md`
- `docs/04-roadmap/current-status.md`

## Current contract

- The current release is `1.0.0-preview.3`; use `v1.0.0-preview.3` examples unless the user's repository or package source explicitly targets another version.
- Jazor compiles a supported, typed C# semantic subset to deterministic ECMAScript modules. It is not a full CLR implementation and it is not a general string-based JavaScript interop layer.
- A pure Jazor authoring project directly references `Jazor`. A RazorVue authoring project directly references `Jazor` and `Jazor.Vue`. The final host directly references `Jazor` and owns Emit output. Add binding packages explicitly where their types are authored.
- JavaScript resource libraries carry `manifest.json` and `dist/**`; pure Jazor libraries carry the generated `Jazor.Generated.ModuleCatalog`. Do not invent a third carrier or manually stitch dependency paths.
- RazorVue lowers the final official Razor SG generated C# and reuses Jazor compiler semantics. Do not introduce a parallel Razor IR, SFC fallback, or marker-JS protocol when the final render-function shape can express the behavior directly.
- Use strong C# contracts and typed binding APIs. Do not weaken host-facing APIs to `object` merely to imitate JavaScript `any`; use closed unions, overloads, enums, callbacks, and collection builders.
- Respect documented unsupported boundaries: unmapped external members, arbitrary CLR reflection/runtime identity, string-based `IJSRuntime` interop, and unrelated Microsoft/Blazor UI components are not made valid by a raw-JavaScript fallback.

## Standard workflow

1. Establish the path and version. Confirm the SDK selected by `global.json`, the target framework, and that all Jazor/ECMAScript package references use one version.
2. Choose the direct package set. Add `Jazor` to a pure module library or final host; add `Jazor.Vue` only for RazorVue authoring; add ecosystem packages only for APIs the project actually uses.
3. Configure the final host, not an intermediate library, with `JazorMode` and `JazorDir`. Use `debug` while inspecting generated modules and source maps; use `release` for the production bundle. Enable `JazorSSR` only with the ASP.NET Core SSR setup.
4. Author through the public C# surface. For core modules, put `[ECMAScriptModule("path/name.mjs")]` on the module type. For RazorVue, make parameters and component usage valid for both Razor SG binding and Jazor lowering.
5. Build the final host and inspect `JazorDir`, `jazor-manifest.json`, imports, and source maps. The build should materialize the dependency closure once.
6. Validate at the narrowest useful scope: `dotnet build` first, then the focused compiler/RazorVue/Emit or binding test suite. If generated output is wrong, diagnose the C# semantic boundary before editing emitted JavaScript.

Use the runnable patterns in [usage-patterns.md](references/usage-patterns.md) when the request needs a project file, module, RazorVue component, SSR host, or binding example. Use [troubleshooting.md](references/troubleshooting.md) when a build or generated artifact fails.

## Response rules

- Give concrete package commands, XML, and C# that match the user's target project.
- Explain which project owns a package or output directory when a multi-project setup is involved.
- Preserve upstream binding comments and point users to XML documentation in the installed package when they ask about API meaning.
- For unsupported APIs, name the unsupported symbol and the supported typed alternative. Do not suggest bypasses that hide the compiler diagnostic.
- Keep fixes scoped. Do not change version, public API, output mode, or release state unless the user asks for that change.
