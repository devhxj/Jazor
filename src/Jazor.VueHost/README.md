# Jazor.VueHost

> Status: active development-time boundary
> Positioning: standalone .NET host for `.jazor` authoring, workspace coordination, and Deno-backed Volar/TypeScript intelligence.

`Jazor.VueHost` is the only long-term host boundary in the current architecture.

Authoring model:

- `.jazor` follows Razor syntax.
- Editor intelligence is Razor-first and should resolve nearby `.vue`, `.css`, `.js`, and `.ts` files from the `.jazor` document graph.
- Legacy `@vueimport` / `@jsimport` are compatibility inputs only; they are not the target authoring model.
- Virtual `.vue` / `.cs` artifacts may still be produced for projection and tooling, but they are implementation details.

Implementation model:

- `.jazor` is the only authoring document and remains Razor-first.
- IntelliSense and build/materialization are separate stages.
- `.jazor` template IntelliSense runs on the source document plus VueHost-coordinated Razor/Roslyn bridge metadata; it does not materialize or depend on a projected `.g.vue` file.
- standalone `.cs` documents now route directly into the Roslyn lane through an identity projection, so `.jazor` code regions and real `.cs` files share the same in-proc Roslyn semantic path.
- Roslyn code-region IntelliSense and diagnostics now expand beyond open buffers through bounded workspace `.cs` / `.jazor` discovery, so unopened project files can participate in completion, hover, signature help, diagnostics, and source-level navigation without introducing a separate project system.
- the shared workspace resolver now treats tracked `.cs` files as workspace-root seeds alongside `.jazor` and `.vue`, which keeps bounded source discovery aligned across Roslyn and bridge paths.
- Razor/Roslyn semantics and Volar semantics stay native to their own lanes; VueHost only routes, maps, and aggregates them.
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
- workspace store abstraction for `.jazor` / `.cs` / `.vue` / `.js` / `.ts`
- Deno Volar worker integration for Vue/TS/CSS/HTML semantics
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
- Razor SDK toolset inspection: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --inspect-razor-toolset`
- in-proc Razor projection probe: start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --probe-inproc-razor=<absolute-or-relative-jazor-path>`

LSP mode:

- current LSP surface: `initialize`, `initialized`, `textDocument/didOpen`, `textDocument/didChange`, `textDocument/didClose`, `textDocument/hover`, `textDocument/completion`, `textDocument/documentSymbol`, `textDocument/semanticTokens/full`, `textDocument/definition`, `textDocument/references`, `textDocument/rename`, `textDocument/codeAction`, `shutdown`, `exit`
- current focus: `.jazor` diagnostics plus source-document `.jazor <-> .vue` markup navigation coordinated through Volar coordination metadata, direct `.cs` Roslyn semantics on the same host path, and self-contained Volar-lane completion/hover/definition/references/rename/document symbols/semantic tokens for real `.vue`, `.ts`, `.js`, `.css`, and `.html`
- current Roslyn scope: bounded workspace source discovery for `definition` / `references` / `rename` now includes tracked documents plus unopened disk-backed `.cs/.jazor`; this improves project-level source navigation, but it is still lighter than a full MSBuild-backed Roslyn project graph
- current IntelliSense contract: nearby/open `.vue` can suppress unresolved component diagnostics for open `.jazor`; `.vue -> .jazor` rename/reference stays markup-only; script-side `.ts` / `.vue <script>` imports preserve native Volar/tsserver definitions into `.vue`, then let VueHost bridge references/rename into nearby `.jazor` markup without touching `@code` identifiers; `.jazor`-origin definition/references/rename now also route through the same shared host bridge supplement instead of private lane-local fanout; rename/document symbols/semantic tokens still stay conservative and unsafe named-import rename requests still return no result instead of fabricating edits
- current bridge seam: `MarkupComponentBridgeService` owns the shared `MarkupBridgeSymbol` identity used to connect `.jazor` markup and Volar locations, while `MarkupBridgeFanoutCoordinator` owns the shared definition/references/rename supplement path above the lanes
- lane contract: `JazorLane` and `VolarLane` should return their own native/local results; cross-document `.jazor <-> .vue/.ts/.js` fan-out belongs in the shared coordinator path, not inside individual lanes
- the host always wires the Jazor lane, an in-proc Roslyn-backed code lane, and the bundled Deno Volar lane
- semantic request routing no longer fabricates lane-local fallback behind frontend/code lanes; native lane answers stay native, and only the shared host bridge supplement adds cross-lane `definition/references/rename` results when bridge identity can be resolved
- the self-contained Deno worker now serves real `.vue` / `.ts` / `.js` / `.css` / `.html` Volar/TypeScript semantics, while `.jazor` template requests stay on the source document and consume VueHost-coordinated Volar metadata instead of a projected `.g.vue`
- the bundled Deno worker is enabled by default, primes its dependency cache into `Frontend/Deno/Cache` at build time, resolves worker-local npm dependencies through `Frontend/Deno/Worker/deno.json`, starts with `--cached-only --allow-env --allow-read`, sets `DENO_DIR` to that bundled cache, and the Volar lane only supplements it with workspace-graph results while that worker is actually active

Volar/Deno runtime path:

- Deno is the only frontend/runtime host path
- the bundled Volar worker lives at `Frontend/Deno/Worker/frontend-worker.ts`
- bundled worker-local Deno config lives at `Frontend/Deno/Worker/deno.json`
- `Jazor.Vite`, Bun, and the old split-host route are migration leftovers

Analysis compatibility mode:

- this exposes `vueanalysis/analyzeJazor` over stdio using the host-local analysis path

Current layout:

- `Analysis/` contains the analysis client abstraction, stdio RPC compatibility server, and transport-based migration bridge
- `Frontend/Deno/Hosting/` contains Deno worker startup, runtime asset resolution, and option parsing
- `Frontend/Deno/Protocol/` and `Frontend/Deno/Worker/` contain the Volar worker protocol and worker entrypoint
- `Hosting/` contains host lifecycle entry abstractions
- `Jazor/Core/` contains parsing, markup/import extraction, and fallback artifact generation for `.jazor`
- `Jazor/Projection/` contains projection generation used by LSP and tooling
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

Volar bridge context RPC methods:

- `vuehost/getFrontendContext`

Analysis RPC methods:

- `vuehost/analyzeJazor`

Artifact RPC methods:

- `vuehost/getVirtualArtifact`

Hot-update RPC methods:

- `vuehost/getHotUpdatePlan`
