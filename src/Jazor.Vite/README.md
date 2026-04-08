# Jazor.Vite

> Status: working baseline
> Positioning: C#-side Bun/Vite launcher plus TS Vite plugin that streams `.jazor` through `Jazor.VueHost`.

`Jazor.Vite` is intentionally thin.

Current surfaces:

- `Jazor.Vite.csproj`: C# launcher and persistent `Jazor.VueHost` bootstrap/probe client
- `src/index.ts`: Vite plugin entry with persistent `Jazor.VueHost` stdio session
- `src/vue-host-session.ts`: long-lived stdio RPC client for `Jazor.VueHost`
- `src/rpc.ts`: compatibility transport wrapper over the persistent session model

It should:

- probe `Jazor.VueHost` over RPC
- launch Bun/Vite processes
- pass host bootstrap settings into the frontend runtime
- resolve `.jazor` virtual modules without duplicating compiler semantics
- keep tracked `.jazor` documents in sync for HMR and repeated loads
- stay replaceable if the frontend toolchain changes

Current implementation notes:

- the C# launcher sets `JAZOR_VUEHOST_COMMAND`, `JAZOR_VUEHOST_ARGS`, `JAZOR_VUEHOST_ARGS_JSON`, and `JAZOR_VUEHOST_RPC_MODE`
- the C# `ProcessVueHostRpcClient` now reuses one host process per client instance instead of spawning per RPC call
- the TS plugin resolves `.jazor` through `vuehost/getVirtualArtifact`
- the TS plugin maintains a persistent `process-stdio` session to avoid per-request host startup
- structured `JAZOR_VUEHOST_ARGS_JSON` is preferred over string splitting when available
- `buildStart`, `buildEnd`, `closeBundle`, `configureServer`, `load`, and HMR document refresh are implemented
- `load` returns a minimal consumable sourcemap derived from `SourceMapDescriptor[]`

It should not:

- define `.jazor` semantics
- duplicate Roslyn or template analysis
- become a second workspace host
- own `.jazor` compile policy
