# Jazor.Vue

> Status: experimental reference
> Positioning: Core `.jazor -> .jazor.vue` document model and compiler lane.

`Jazor.Vue` is the new project that hosts the C#-first Vue bridge described in the
Jazor v1 architecture proposal.

It currently owns:

- `.jazor` import / template / code document model
- virtual external symbol contracts (`VESL`)
- a minimal `.jazor` parser
- a prototype compiler that emits standard Vue-facing bridge artifacts

It intentionally does not own:

- Roslyn generator wiring
- Vite host integration
- downstream bundling/runtime behavior
