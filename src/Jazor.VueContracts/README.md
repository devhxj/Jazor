# Jazor.VueContracts

> Status: experimental reference
> Positioning: Shared DTO and protocol contracts for `Jazor.VueHost` <-> `Jazor.VueAnalysis`.

`Jazor.VueContracts` is the compile-time sharing boundary for the distributed Vue architecture.

It exists to carry:

- request / response DTOs
- document and project snapshots
- semantic and artifact descriptors
- protocol-level records that may cross process boundaries
- transport-neutral RPC envelopes for host/client messaging
- shared protocol JSON serialization policy
- host bootstrapping DTOs for capability and liveness discovery
- host capability and server-info DTOs for IDE/Vite bootstrap
- shared VueHost RPC method names for cross-process invocation
- shared VueAnalysis RPC method names for cross-process invocation

It should not become:

- a semantic engine
- a host implementation
- a transport implementation
- a place for Roslyn or Vite-specific object graphs
