# ECMAScript.Vue.Generator

Maintains documentation for shared ECMAScript bindings from source declarations and pinned upstream prose.

The component binding generators are maintained in [RazorVue](https://github.com/devhxj/RazorVue/tree/main/src/ECMAScript.Vue.Generator).

Run from the repository root.

    dotnet run --project src/ECMAScript.Vue.Generator -- authoring --check
    dotnet run --project src/ECMAScript.Vue.Generator -- documentation --check
    dotnet run --project src/ECMAScript.Vue.Generator -- documentation snapshot <vue-3.5.42.tar.gz>

Run authoring to refresh type-derived hints, union projections, and numeric conversions after changing binding declarations. Run documentation to apply the reviewed descriptions. The Monaco generator uses the monaco-runtime tool inputs.
