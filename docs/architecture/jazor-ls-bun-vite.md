# Obsolete: Bun/Vite Split-Host Note

This file is kept only to preserve old links.

It does not describe the current architecture target.

Current direction:

- `Jazor.VueHost` is the only long-term project boundary.
- Deno is the only frontend runtime boundary.
- `.jazor` is authored as Razor.
- Virtual `.vue` / `.cs` artifacts are internal bridge projections.
- `Jazor.Vite`, Bun, and the old split-host arrangement are migration leftovers and should not be used as design guidance.

Use [jazor-vuehost-single-project.md](D:/repository/own/jazor/Jazor/docs/architecture/jazor-vuehost-single-project.md) as the active architecture reference.
