# ECMAScript.WebIDL.Generator

This project is the new .NET host for the WebIDL generation pipeline.

Current responsibilities:

- run the Deno collection worker through `DenoHost`
- own the Deno worker/config for the WebIDL inventory pipeline
- collect a stable JSON inventory from `webref` and `webidl2`
- persist inventory artifacts under `src/ECMAScript/generate/.webidl`
- generate preview C# bindings for `typedef`, `enum`, `callback`, `callback interface`, `dictionary`, `interface`, and `namespace`

Current non-goals:

- replacing the existing `app.ts` emitter in one step
- assuming legacy TypeScript output is the source of truth when current `webref` inventory disagrees

Typical command:

```powershell
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --out src/ECMAScript/generate/.webidl
```

The Deno worker entrypoint and `deno.json` live under `src/ECMAScript.WebIDL.Generator/` so this project does not depend on `src/ECMAScript.WebIDL`.

The next migration step is to move binding rules from the legacy TypeScript emitter into C# on top of the generated inventory.
