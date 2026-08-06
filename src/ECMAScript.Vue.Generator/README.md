# ECMAScript.Vue.Generator

Maintenance-only generator for the Element Plus, Vuetify, and TDesign binding
packages. It owns frozen upstream inputs and the tools that read them. Binding
packages retain only their authoring contracts, generated C#, `manifest.json`,
`dist/`, and `licenses/`; none references this project at application build or
runtime.

Run from the repository root:

```text
dotnet run --project src/ECMAScript.Vue.Generator -- elementplus
dotnet run --project src/ECMAScript.Vue.Generator -- elementplus --check
dotnet run --project src/ECMAScript.Vue.Generator -- vuetify
dotnet run --project src/ECMAScript.Vue.Generator -- vuetify --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign snapshot --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign bindings --check
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --report
dotnet run --project src/ECMAScript.Vue.Generator -- tdesign components --check
```

`upstream/element-plus/2.9.8` freezes only the Element Plus files consumed by
its generator. `upstream/tdesign-vue-next/1.20.5` preserves the declaration
snapshot and external type inputs required for reproducible TDesign contracts.

Vuetify has no frozen strong-prop source in this project. Its catalog command
uses Roslyn to derive `VuetifyCatalog.g.cs` from the current
`[VueLibraryComponent]` declarations. Component props remain authored contracts;
the generator does not present them as a complete upstream-derived surface.
