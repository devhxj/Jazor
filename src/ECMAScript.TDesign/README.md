# ECMAScript.TDesign

> Purpose: standalone TDesign Vue Next binding and RazorVue authoring surface.

The published package is self-contained: it ships the browser ESM, CSS, license, and
`manifest.json` for `tdesign-vue-next` 1.20.5. Consumers restore NuGet only; they do
not install Node.js packages or resolve resources from a CDN.

The frozen ESM has one bare runtime dependency, `vue`. The package manifest declares
that dependency as `vue3: ^3.5.0`, so Jazor materializes the TDesign and Vue resources
from the restored NuGet packages into the generated application's local `vendor/`
directory. No `node_modules` lookup is part of the consumer path.

The component binding input is driven by the versioned upstream snapshot under
`../ECMAScript.Vue.Generator/upstream/tdesign-vue-next/1.20.5`. `components.json` lists the 120 documented entries
whose exports exist in the current browser ESM, while `bindings.json` maps each entry to
its real module, named export, and TypeScript Props declaration. `contracts.json` resolves
each documented prop to its current TypeScript type and declaration source, including the
small set of Vue-mixin properties represented only in upstream `web-types`. Documentation-
only tags without a current runtime export are not binding inputs.

Update the upstream input with:

```text
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

These are package-maintainer tools. They use .NET to snapshot the locked upstream
version and Tree-sitter to read its TypeScript syntax, then generate audited binding and
Props-contract catalogs. They are never invoked by application build or publish, and do
not create a consumer dependency on Node.js, npm, or Tree-sitter.

`ECMAScript.Vue.Generator tdesign components` is a full-coverage verification gate. It emits the
118 current runtime components only when every declared prop has a concrete C# type;
the generated catalog and `--report` must remain `118/118`. It never substitutes
`object`, `VueValue`, or placeholder contracts to preserve component coverage.

Component contracts remain strongly typed: `object` is only permitted for Razor's
`AdditionalAttributes` sink. Rich TDesign content is represented by verified slots and
named value/callback contracts. A source type that cannot be expressed directly must
receive a documented, dedicated C# contract; it must not be silently reduced to
`object` or `VueValue` to make generation pass.

Public authoring types follow TDesign component naming: use `T*` (`TMenuValue`, `TButtonThemeValue`, `TComponents`). Only the package root host remains `TDesign`.

String-literal domains are emitted as `[String]` enums. Values such as `TButtonThemeValue.Primary` therefore lower to the authored TDesign literal (`"primary"`), never to a numeric enum ordinal.

## Boundary

This package defines host bindings and component contracts. Razor SG integration, render-function lowering, and output materialization remain owned by `Jazor.Vue`, `Jazor.RazorVue`, and `Jazor.Emit` respectively.
