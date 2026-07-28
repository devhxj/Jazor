# Jazor.Css

Deterministic CSS-in-JS authoring for Jazor applications.

`Jazor.Css` is an independent, framework-neutral package. It compiles structured C# style declarations into the standard `Jazor.Css/runtime.mjs` module, generates stable names from normalized content, and manages one owned browser stylesheet. The return value of `Css.Class` is an ordinary `string`, so plain ECMAScript modules, direct Vue authoring, and RazorVue components consume it without an adapter.

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor.Css" Version="0.1.31" />
</ItemGroup>
```

The package depends on the exact same version of `Jazor`. It does not install a Razor Hook, Vue integration, or CSS-specific MSBuild targets.

## Class Rules

```csharp
using Jazor.Css;

var buttonClass = Css.Class(new CssRule
{
    Display = "inline-flex",
    AlignItems = "center",
    Gap = "0.5rem",
    Color = "white",
    BackgroundColor = "#1769aa",
    ["--button-shadow"] = "0 0.25rem 0.75rem rgb(0 0 0 / 18%)",
    Children =
    [
        new(CssChildKind.Selector, "&:hover, &:focus-visible", new CssRule
        {
            BackgroundColor = "#125486"
        }),
        new(CssChildKind.Media, "(width < 40rem)", new CssRule
        {
            Width = "100%"
        })
    ]
});
```

Standard declarations are generated from the repository Webref inventory. The indexer accepts custom properties and other valid declaration names. Use `Additional` when duplicate declarations or fallback order are semantically significant:

```csharp
var layoutClass = Css.Class(new CssRule
{
    Additional =
    [
        new("display", "-webkit-box"),
        new("display", "flex", Important: true)
    ]
});
```

Named properties are normalized by CSS name, independent of C# initializer order. `Children` and `Additional` preserve author order because order is observable in the cascade.

## Keyframes And Global Rules

```csharp
var fadeIn = Css.Keyframes(
    new("from", new CssDeclarations { Opacity = "0" }),
    new("to", new CssDeclarations { Opacity = "1" }));

Css.Global("html, body", new CssRule
{
    Margin = "0",
    MinHeight = "100%"
});
```

`Css.Keyframes` returns a deterministic animation name. `Css.Global` registers a selector rule and deduplicates identical content. The first phase supports selector children, `@media`, and `@supports`; it does not accept arbitrary raw CSS or other at-rules.

## Browser Configuration

```csharp
Css.Configure(new CssOptions
{
    StyleId = "application-styles",
    Nonce = cspNonce
});
```

Call `Css.Configure` before the first class, keyframe, or global registration. In a browser, the runtime creates or adopts one marked `<style>` element, validates its ownership and nonce, and uses length-delimited entries to remain idempotent across module reloads. Without a DOM, registration and extraction continue in memory.

`Css.Extract()` returns the current CSS text without clearing the registry. It is suitable for diagnostics and controlled rendering flows, but the first phase does not provide per-request SSR isolation.

## Build Output

`Jazor.Css` uses the standard Jazor output contract without additional properties:

- `JazorMode=none` writes no artifacts.
- `JazorMode=debug` materializes `Jazor.Css/runtime.mjs`, its source map, and the manifest entry.
- `JazorMode=release` includes the runtime and its consumers in `bundle.js`.

Release mode remains runtime CSS-in-JS. It does not emit a separate `.css` file or perform build-time extraction, PostCSS processing, prefixing, or style reclamation.

## Runtime Guidance

- Use rule objects for finite visual states that benefit from deterministic names and deduplication.
- Use inline styles or CSS custom properties for high-cardinality continuous values.
- Treat declaration values as authored CSS source. Apply application-level constraints before inserting untrusted values.
- Keep global rules stable. Theme switching should normally update custom properties or theme classes.

## Verification

```powershell
dotnet run --file scripts/csharp/generate-jazor-css-properties.cs -- --check
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css-browser
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter JazorCss
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter JazorCss
```

The browser smoke uses the generated runtime module and verifies computed style, CSP nonce propagation, one-style ownership, Unicode entry framing, and HMR-style module adoption.
