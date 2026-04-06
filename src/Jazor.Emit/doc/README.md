# Jazor.Emit Docs

> Status: active reference
> Positioning: Local deep-doc entry for the emit and host-facing materialization lane.

## Purpose

This directory is the local deep-doc entry for `Jazor.Emit`.

Use it when you need more than the thin operational README and want a module-local explanation of:

- how emit loads compiler outputs
- how manifests and files are materialized
- how RazorVue artifacts continue into emit
- where SourceMap generation currently sits inside the emit lane

## Entry Documents

- [Emit.Pipeline.Overview.md](./Emit.Pipeline.Overview.md)
- [Emit.Materialization.Overview.md](./Emit.Materialization.Overview.md)
- [Emit.BundleAndSourceMap.Overview.md](./Emit.BundleAndSourceMap.Overview.md)

## Relationship To Other Docs

- `src/Jazor.Emit/README.md` stays the thin operational card.
- `docs/status/2026-04-06-emit-host-materialization-status.md` stays the repo-level status snapshot.
- `docs/plans/emit-materialization-execution-bridge.md` stays the repo-level execution bridge.

The local docs here should explain `Jazor.Emit` itself, not restate repo-level workstream navigation.
