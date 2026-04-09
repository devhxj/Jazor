# Jazor.VueHost

> Status: active development-time boundary
> Positioning: standalone .NET host for `.jazor` authoring, workspace coordination, and Deno-backed frontend intelligence.

`Jazor.VueHost` is the only long-term host boundary in the current architecture.

Authoring model:

- `.jazor` follows Razor syntax.
- Editor intelligence is Razor-first and should resolve nearby `.vue`, `.css`, `.js`, and `.ts` files from the `.jazor` document graph.
- Legacy `@vueimport` / `@jsimport` are compatibility inputs only; they are not the target authoring model.
- Virtual `.vue` / `.cs` artifacts may still be produced for projection and tooling, but they are implementation details.

Implementation model:

- `.jazor` is the only authoring document and remains Razor-first.
- IntelliSense and build/materialization are separate stages.
- Razor/Roslyn semantics and frontend semantics are lane-based internals inside VueHost.
- nearby `.vue`, `.css`, `.js`, and `.ts` are part of the workspace graph, not separate host boundaries.
- `.vue` and `.jazor` should participate in the same workspace navigation graph for definition, references, and rename.
- workspace-open `.vue` documents should immediately affect `.jazor` diagnostics and navigation without waiting for file materialization.
- component rename/reference should stay on Razor/Vue markup symbols and must not spill into `@code` C# identifiers.
- cross-file navigation may expand from nearby lookup into bounded workspace disk scans, but this remains a host-internal heuristic rather than a generated-artifact dependency.
- the current implementation uses a shared workspace resolver for LSP, frontend-context derivation, and hot-update impact so `.jazor <-> .vue` lookup rules stay aligned.
- Deno is the only frontend/runtime host path.

Current scope:

- executable host project targeting `net10.0`
- stdio RPC and LSP serving for `.jazor`
- workspace store abstraction for `.jazor` / `.vue` / `.js` / `.ts`
- Deno frontend worker integration for Vue/TS/CSS/HTML semantics
- host-local analysis fallback for virtual artifact generation
- protocol and projection orchestration
- shared workspace resolver for nearby lookup, bounded workspace scans, and cache invalidation
- projection metadata plumbed through routing, while current lane handlers still consume source-document coordinates

Out of scope:

- Bun/Vite split-host workflows
- reintroducing separate analysis-route projects
- external dev-server wrappers
- Roslyn semantics or `.jazor` compilation logic

Compatibility note:

- transport-based analysis is kept only for migration compatibility. It is not the target architecture.
- when an external analysis process is still required during migration, point it at `Jazor.VueHost --analysis-stdio`.
- do not add new dependencies on legacy analysis host/runtime projects.

Runtime modes:

- default mode: start without redirected stdin and without explicit mode flags to print a startup banner and a `vuehost/getHostInfo` response envelope
- stdio RPC mode: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --stdio`
- stdio RPC mode is also entered automatically when stdin is redirected
- LSP mode: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --lsp`
- analysis compatibility mode: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --analysis-stdio`
- language-server catalog inspection: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --inspect-language-servers`
- language-server probe: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --probe-language-servers`
- Razor SDK toolset inspection: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --inspect-razor-toolset`
- in-proc Razor projection probe: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --probe-inproc-razor=<absolute-or-relative-jazor-path>`

LSP mode:

- current LSP surface: `initialize`, `initialized`, `textDocument/didOpen`, `textDocument/didChange`, `textDocument/didClose`, `textDocument/hover`, `textDocument/completion`, `textDocument/documentSymbol`, `textDocument/definition`, `textDocument/references`, `textDocument/rename`, `textDocument/codeAction`, `shutdown`, `exit`
- current focus: `.jazor` diagnostics plus workspace-aware `.jazor <-> .vue` hover, completion, definition, references, rename, and code actions
- current IntelliSense contract: nearby/open `.vue` can suppress unresolved component diagnostics for open `.jazor`, and `.vue -> .jazor` rename/reference stays markup-only
- the host always wires the Jazor lane, an in-proc Roslyn-backed code lane, and a frontend lane
- passing `--external-roslyn` lets the code lane use an external Roslyn language server when one is discovered from the catalog
- Volar and TypeScript are discovered through the language-server catalog when available; otherwise frontend behavior falls back to the bundled Deno worker path

Frontend runtime path:

- Deno is the only frontend/runtime host path
- the bundled frontend worker lives at `Frontend/Deno/Worker/frontend-worker.ts`
- `Jazor.Vite`, Bun, and the old split-host route are migration leftovers

Analysis compatibility mode:

- this exposes `vueanalysis/analyzeJazor` over stdio using the host-local analysis path

Current layout:

- `Analysis/` contains the analysis client abstraction, stdio RPC compatibility server, and transport-based migration bridge
- `Frontend/Deno/Hosting/` contains Deno worker startup, runtime asset resolution, and option parsing
- `Frontend/Deno/Protocol/` and `Frontend/Deno/Worker/` contain the frontend worker protocol and worker entrypoint
- `Hosting/` contains host lifecycle entry abstractions
- `Jazor/Core/` contains parsing, markup/import extraction, and fallback artifact generation for `.jazor`
- `Jazor/Projection/` contains projection generation used by LSP and tooling
- `LanguageServers/` contains external language-server discovery and projected lane hosts for Volar, TypeScript, and optional external Roslyn
- `Lsp/` contains document services, stdio LSP transport, result aggregation, lane coordination, and routing
- `Protocol/Contracts/` contains shared DTOs, RPC method names, document snapshots, and JSON serialization
- `Roslyn/InProc/` contains the current in-proc Roslyn code-service implementation
- `Razor/InProc/` contains the current in-proc Razor-to-C# design-time projection service
- `Razor/Toolset/` contains SDK-bits resolver/host logic used to discover self-contained Razor toolset assets
- `Rpc/` contains the stdio VueHost RPC server, dispatcher, and payload processing
- `Services/` contains `VueHostService`, frontend-context derivation, hot-update planning, and the null analysis client
- `VirtualDocuments/` contains virtual document models, registry state, and projection maps
- `Workspace/` contains the in-memory workspace store and the shared resolver for nearby lookup and bounded workspace scans
- `Program.cs` selects the runtime mode and composes the host

Shared RPC envelope DTOs live in `Protocol/Contracts/RpcMessages.cs`.
Bootstrap host info DTOs live in `Protocol/Contracts/HostInfo.cs`.
Shared host RPC method names live in `Protocol/Contracts/VueHostRpcMethodNames.cs`.
Shared protocol JSON serialization lives in `Protocol/Contracts/ProtocolJsonSerializer.cs`.
Related architecture context lives in `../../docs/architecture/jazor-vuehost-single-project.md`, `../../docs/architecture/vuehost-capabilities.md`, and `../../docs/architecture/vuehost-document-map.md`.

Current stdio envelope:

```json
{"id":"1","method":"vuehost/getOpenDocuments","payloadJson":null}
```

```json
{"id":"1","success":true,"payloadJson":"[]","error":null}
```

Bootstrap RPC methods:

- `vuehost/ping`
- `vuehost/getHostInfo`

Workspace RPC methods:

- `vuehost/openDocument`
- `vuehost/updateDocument`
- `vuehost/closeDocument`
- `vuehost/getOpenDocuments`

Frontend context RPC methods:

- `vuehost/getFrontendContext`

Analysis RPC methods:

- `vuehost/analyzeJazor`

Artifact RPC methods:

- `vuehost/getVirtualArtifact`

Hot-update RPC methods:

- `vuehost/getHotUpdatePlan`
