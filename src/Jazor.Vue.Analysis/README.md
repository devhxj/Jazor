# Jazor.Vue.Analysis

> Status: experimental reference
> Positioning: Thin Roslyn-facing shallow analysis layer for `.jazor`.

`Jazor.Vue.Analysis` is intentionally narrow.

It hosts shallow semantic glue that:

- scans `.jazor` additional files
- parses and compiles them through `Jazor.Vue`
- projects generated bridge artifacts into source output
- exposes deterministic Roslyn-backed diagnostics that do not need IDE session state

It should not become:

- the project/workspace host
- the Bun/Vite process manager
- the IDE/session orchestrator
- the long-lived cache coordinator

Those responsibilities belong to the future `Jazor.LanguageServer`.

Reference architecture:

- [docs/architecture/jazor-ls-bun-vite.md](D:/repository/own/jazor/Jazor/docs/architecture/jazor-ls-bun-vite.md)
