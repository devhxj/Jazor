# Jazor.Vue.Analysis.Runtime

> Status: project skeleton
> Positioning: transport-neutral runtime lane for `.jazor` analysis RPC.

`Jazor.Vue.Analysis.Runtime` exists to keep runtime/process code out of the Roslyn analyzer assembly.

Current scope:

- execute shallow `.jazor` analysis through `Jazor.Vue`
- expose a transport-neutral RPC processor for `AnalyzeJazor`
- provide a simple stdio server that can be hosted by `Jazor.Vue.Analysis.Host`

Non-goals:

- Roslyn analyzer/generator packaging
- workspace ownership
- frontend toolchain orchestration
- IDE/session lifecycle
