# ECMAScript.Lucide

Complete generated Blazor/RazorVue bindings for `lucide-vue-next`. The generator reads the upstream TypeScript declarations and emits one strongly typed component for every named icon export. Each icon remains a named export from the upstream npm ESM package, so Deno and Vite perform normal tree shaking.

Use the binding directly in a Razor component:

```razor
<LogIn Size="24px" aria-label="Sign in" />
```

`Size`, `StrokeWidth`, and `AbsoluteStrokeWidth` are typed component parameters. Standard SVG attributes use Blazor attribute splatting through `AdditionalAttributes`; the generated component still resolves to the ordinary `lucide-vue-next` ESM export. No runtime icon catalog or generated wrapper module is required.

The generated XML documentation links each icon to its official Lucide page and records the upstream named export. Keep the generated source and XML together when packaging so IDE completion and Razor authoring retain the same guidance as the runtime binding.

Regenerate with dotnet run --file scripts/csharp/generate-lucide.cs -- --version 1.0.0.
