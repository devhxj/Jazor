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

LSP mode:

- start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --lsp`
- current LSP surface: `initialize`, `textDocument/didOpen`, `textDocument/didChange`, `textDocument/didClose`, `textDocument/hover`, `textDocument/completion`, `textDocument/definition`, `textDocument/references`, `textDocument/rename`, `textDocument/codeAction`, `shutdown`, `exit`
- current focus: `.jazor` diagnostics plus workspace-aware `.jazor <-> .vue` hover, completion, definition, references, rename, and code actions
- current IntelliSense contract: nearby/open `.vue` can suppress unresolved component diagnostics for open `.jazor`, and `.vue -> .jazor` rename/reference stays markup-only

Frontend runtime path:

- Deno is the only frontend/runtime host path
- `Jazor.Vite`, Bun, and the old split-host route are migration leftovers

Analysis compatibility mode:

- start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --analysis-stdio`
- this exposes `vueanalysis/analyzeJazor` over stdio using the host-local analysis path

Minimal layout:

- `Analysis/IVueAnalysisClient.cs`
- `Analysis/IAnalysisRpcTransport.cs`
- `Analysis/IVueAnalysisRpcService.cs`
- `Analysis/IVueAnalysisRpcProcessor.cs`
- `Analysis/DelegateAnalysisRpcTransport.cs`
- `Analysis/JazorVueAnalysisService.cs`
- `Analysis/ProcessAnalysisRpcTransport.cs`
- `Analysis/RpcVueAnalysisClient.cs`
- `Analysis/StdioVueAnalysisRpcServer.cs`
- `Analysis/VueAnalysisClientFactory.cs`
- `Analysis/VueAnalysisClientMode.cs`
- `Analysis/VueAnalysisClientOptions.cs`
- `Analysis/VueAnalysisRpcException.cs`
- `Analysis/VueAnalysisRpcProcessor.cs`
- `Analysis/VueAnalysisRpcSerializer.cs`
- `Frontend/IFrontendContextProvider.cs`
- `Lsp/JazorLspDocumentService.cs`
- `Lsp/LspModels.cs`
- `Lsp/LspSession.cs`
- `Lsp/StdioLspServer.cs`
- `Workspace/IVueHostWorkspaceStore.cs`
- `Workspace/InMemoryWorkspaceStore.cs`
- `Hosting/IVueHostService.cs`
- `Hosting/VueHostServiceEntry.cs`
- `Rpc/IVueHostRpcService.cs`
- `Rpc/IVueHostRpcProcessor.cs`
- `Rpc/IVueHostRpcDispatcher.cs`
- `Rpc/VueHostRpcDispatcher.cs`
- `Rpc/VueHostRpcProcessor.cs`
- `Rpc/VueHostRpcSerializer.cs`
- `Rpc/StdioVueHostRpcServer.cs`
- `Services/VueHostService.cs`
- `Services/NullVueAnalysisClient.cs`
- `Program.cs`

Shared RPC envelope DTOs live in `Protocol/Contracts/RpcMessages.cs`.
Bootstrap host info DTOs live in `Protocol/Contracts/HostInfo.cs`.
Shared host RPC method names live in `Protocol/Contracts/VueHostRpcMethodNames.cs`.
Shared protocol JSON serialization lives in `Protocol/Contracts/ProtocolJsonSerializer.cs`.

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

Artifact RPC methods:

- `vuehost/getVirtualArtifact`

Workspace RPC methods:

- `vuehost/openDocument`
- `vuehost/updateDocument`
- `vuehost/closeDocument`
- `vuehost/getOpenDocuments`
