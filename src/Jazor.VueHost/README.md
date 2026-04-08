# Jazor.VueHost

> Status: working baseline
> Positioning: standalone .NET RPC host for workspace state, analysis coordination, and `.jazor` frontend-intelligence brokering.

`Jazor.VueHost` is the development-time service boundary for the new Jazor Vue architecture.

Authoring model:

- `.jazor` is intended to follow Razor syntax rather than a custom `<template>`-first dialect.
- `Jazor.VueHost` extends Razor authoring with cross-file awareness for nearby `.vue`, `.css`, `.js`, and `.ts` assets.
- runtime/build-time virtual artifacts still exist, but editor intelligence should treat them as implementation details rather than the source authoring surface.

Current scope in this skeleton:

- executable host project targeting `net10.0`
- host lifecycle abstraction with current line-based stdio RPC transport
- minimal LSP surface over stdio for `.jazor` authoring
- workspace store abstraction for `.jazor` / `.vue` / `.js` / `.ts` document snapshots
- analysis client abstraction with host-local protocol contracts
- pluggable analysis-client path with transport support plus local runtime fallback for virtual artifact generation
- minimal RPC-facing host service that coordinates workspace state and analysis calls
- line-oriented stdio RPC loop for the current request/response envelope
- bootstrap RPC methods for host discovery and liveness checks
- virtual artifact RPC for `.jazor -> vue-sfc` loading
- open/update/close/get-open-documents RPC for persistent frontend sessions

Out of scope for this skeleton:

- Bun / Vite process orchestration
- frontend indexing implementation
- Roslyn semantics or `.jazor` compilation logic

Compile-time boundary:

- no direct dependency on `Jazor.Vue` or `Jazor.VueContracts`
- host-local fallback analysis/compiler is used for virtual artifacts
- do not reference `Jazor.Vue.Analysis` directly

Minimal layout:

- `Analysis/IVueAnalysisClient.cs`
- `Analysis/IAnalysisRpcTransport.cs`
- `Analysis/DelegateAnalysisRpcTransport.cs`
- `Analysis/ProcessAnalysisRpcTransport.cs`
- `Analysis/RpcVueAnalysisClient.cs`
- `Analysis/VueAnalysisClientFactory.cs`
- `Analysis/VueAnalysisClientMode.cs`
- `Analysis/VueAnalysisClientOptions.cs`
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

LSP mode:

- start with `dotnet run --project src/Jazor.VueHost/Jazor.VueHost.csproj -- --lsp`
- current LSP surface: `initialize`, `textDocument/didOpen`, `textDocument/didChange`, `textDocument/didClose`, `textDocument/hover`, `textDocument/completion`, `textDocument/definition`, `textDocument/references`, `textDocument/rename`, `textDocument/codeAction`, `shutdown`, `exit`
- current focus: `.jazor` diagnostics plus Razor-markup/component-oriented hover, completion, definition, references, rename, and code actions

Analysis client bootstrap:

- default: local null fallback
- `--analysis-client=transport --analysis-command=<command>`
