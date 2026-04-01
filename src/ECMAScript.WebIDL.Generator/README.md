# ECMAScript.WebIDL.Generator

This project is the new .NET host for the WebIDL generation pipeline.

Current responsibilities:

- run the Deno collection worker through `DenoHost`
- collect a stable JSON inventory from `webref` and `webidl2`
- persist inventory artifacts under `src/ECMAScript/generate/.webidl`
- generate preview C# bindings for `typedef`, `enum`, `callback`, `callback interface`, and `dictionary`

Current non-goals:

- replacing the existing `app.ts` emitter in one step
- migrating interface, dictionary, and method emission completely yet

Typical command:

```powershell
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --out src/ECMAScript/generate/.webidl
```

The next migration step is to move binding rules from the legacy TypeScript emitter into C# on top of the generated inventory.
