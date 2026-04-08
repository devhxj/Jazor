# Jazor.Vue.Analysis.Host

> Status: thin executable wrapper
> Positioning: process entry for `Jazor.Vue.Analysis.Runtime` stdio RPC.

`Jazor.Vue.Analysis.Host` is intentionally minimal.

It should:

- host the existing `Jazor.Vue.Analysis.Runtime` processor/server
- expose `vueanalysis/analyzeJazor` over line-based stdio
- stay thin enough that semantic logic remains in `Jazor.Vue.Analysis.Runtime`

It should not:

- own workspace/session state
- duplicate parsing or semantic logic from `Jazor.Vue.Analysis.Runtime`
- grow into a second host orchestration layer
