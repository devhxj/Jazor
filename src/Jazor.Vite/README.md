# Jazor.Vite

> Status: thin orchestration skeleton
> Positioning: C#-side Bun/Vite launcher plus Bun-first TS plugin shell.

`Jazor.Vite` is intentionally thin.

Current surfaces:

- `Jazor.Vite.csproj`: C# launcher and `Jazor.VueHost` bootstrap/probe client
- `src/*.ts`: Bun-first Vite plugin shell that resolves `.jazor` through `Jazor.VueHost`

It should:

- probe `Jazor.VueHost` over RPC
- launch Bun/Vite processes
- pass host bootstrap settings into the frontend runtime
- resolve `.jazor` virtual modules without duplicating compiler semantics
- stay replaceable if the frontend toolchain changes

Current implementation notes:

- the C# launcher sets `JAZOR_VUEHOST_COMMAND`, `JAZOR_VUEHOST_ARGS`, and `JAZOR_VUEHOST_RPC_MODE`
- the TS plugin shell currently resolves `.jazor` through `vuehost/analyzeJazor`
- current host communication is process-per-request stdio while the persistent transport is still pending

It should not:

- define `.jazor` semantics
- duplicate Roslyn or template analysis
- become a second workspace host
- own `.jazor` compile policy
