# Jazor.VueHost

> Status: project skeleton
> Positioning: standalone .NET host boundary for RPC transport, workspace state, and analysis coordination.

`Jazor.VueHost` is the development-time service boundary for the new Jazor Vue architecture.

Current scope in this skeleton:

- executable host project targeting `net10.0`
- host lifecycle abstraction for a future stdio / socket / named-pipe RPC transport
- workspace store abstraction for `.jazor` / `.vue` / `.js` / `.ts` document snapshots
- analysis client abstraction that depends only on `Jazor.VueContracts`
- pluggable analysis-client path with local null fallback and future RPC transport slot
- minimal RPC-facing host service that coordinates workspace state and analysis calls
- line-oriented stdio RPC loop for the current request/response envelope
- bootstrap RPC methods for host discovery and liveness checks

Out of scope for this skeleton:

- concrete RPC transport
- Bun / Vite process orchestration
- frontend indexing implementation
- Roslyn semantics or `.jazor` compilation logic

Compile-time boundary:

- reference `Jazor.VueContracts`
- do not reference `Jazor.VueAnalysis` directly

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

Shared RPC envelope DTOs live in `Jazor.VueContracts/Protocol/RpcMessages.cs`.
Bootstrap host info DTOs live in `Jazor.VueContracts/Protocol/HostInfo.cs`.
Shared host RPC method names live in `Jazor.VueContracts/Protocol/VueHostRpcMethodNames.cs`.
Shared protocol JSON serialization lives in `Jazor.VueContracts/Protocol/ProtocolJsonSerializer.cs`.

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

Analysis client bootstrap:

- default: local null fallback
- `--analysis-client=transport --analysis-command=<command>`
