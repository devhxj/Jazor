# Jazor.Style

Deterministic, framework-neutral CSS-in-JS for Jazor applications.

`Jazor.Style` turns structured C# declarations into the standard `Jazor.Style/runtime.mjs` module. It provides generated CSS properties, stable content-based names, ordered nesting, structured at-rules, isolated registries, browser and Shadow DOM targets, SSR snapshots, CSP nonce handling, and idempotent hydration. `Css.Class` returns an ordinary `string`, so ECMAScript modules, Vue bindings, and RazorVue components use it without an adapter.

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor.Style" Version="0.1.32" />
</ItemGroup>
```

The package depends on the exact same version of `Jazor`. It installs no Razor Hook, Vue integration, CSS-specific analyzer, or additional MSBuild target.

## Class Rules

```csharp
using Jazor.Style;

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
        new(CssChildKind.Container, "toolbar (width < 40rem)", new CssRule
        {
            Width = "100%"
        }),
        new(CssChildKind.StartingStyle, null, new CssRule
        {
            Opacity = "0"
        })
    ]
});
```

The generated catalog exposes standard writable `CSSStyleDeclaration` properties from Webref. The string indexer covers custom properties, emerging properties, and at-rule descriptors that do not belong to `CSSStyleDeclaration`.

Named declarations are normalized by CSS name, independent of C# initializer order. `Additional` and `Children` retain author order because fallback and cascade order are observable:

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

## Nested Rules

`CssChildKind` supports the structured grouping forms used by component styles:

| Kind | Output |
| --- | --- |
| `Selector` | Nested selector; `&` refers to the current selector |
| `Media` | `@media <prelude> { ... }` |
| `Supports` | `@supports <prelude> { ... }` |
| `Container` | `@container <prelude> { ... }` |
| `Layer` | Named or anonymous `@layer` block |
| `Scope` | Named or anonymous `@scope` block |
| `StartingStyle` | `@starting-style` block without a prelude |

Selector lists are parsed with quote, escape, parenthesis, and attribute-selector awareness. Nested grouping rules preserve the active selector and sibling order.

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

Keyframes, classes, global selectors, and declaration at-rules use separate hash domains. Identical content is registered once while first-registration order remains stable.

## Declaration At-Rules

`Css.AtRule` represents block at-rules without accepting raw CSS:

```csharp
Css.AtRule(new CssAtRule(
    "font-face",
    new CssDeclarations
    {
        FontFamily = "Jazor Sans",
        ["src"] = "url('/fonts/jazor.woff2') format('woff2')",
        FontDisplay = "swap"
    }));

Css.AtRule(new CssAtRule(
    "page",
    new CssDeclarations { Margin = "12mm" },
    Prelude: ":first",
    Children:
    [
        new("top-left", new CssDeclarations
        {
            Content = "'Jazor'"
        })
    ]));
```

This model covers declaration-block rules such as `@font-face`, `@property`, `@counter-style`, `@page`, and nested page-margin rules. Runtime statement rules such as `@charset`, `@import`, and `@namespace` are intentionally excluded because their ordering and fetch semantics do not fit incremental style registration.

## Browser Configuration

The default registry targets `document.head`:

```csharp
Css.Configure(new CssOptions
{
    StyleId = "application-styles",
    Nonce = cspNonce
});
```

Call `Css.Configure` before the first default-context registration. The runtime creates or adopts one owned `<style>`, validates its ID and nonce, and stores entries in versioned UTF-16 length frames so module reloads do not duplicate rules.

For Shadow DOM, create a context bound to the shadow root:

```csharp
var shadowStyles = Css.CreateContext(new CssOptions
{
    Target = shadowRoot,
    StyleId = "widget-styles",
    Nonce = cspNonce
});

var className = Css.ClassIn(shadowStyles, widgetRule);
```

Contexts with different targets or IDs own independent style nodes. Contexts that point to the same target and ID adopt the same marked node.

## Isolated Rendering And Hydration

Create a detached context for request-local rendering or deterministic extraction:

```csharp
var requestStyles = Css.CreateContext(new CssOptions
{
    Detached = true,
    StyleId = "request-styles",
    Nonce = cspNonce
});

var className = Css.ClassIn(requestStyles, rule);
Css.GlobalIn(requestStyles, "body", globalRule);
var snapshot = Css.SnapshotFrom(requestStyles);
```

Context-aware operations are explicit:

| Default registry | Explicit context |
| --- | --- |
| `Css.Class` | `Css.ClassIn` |
| `Css.Keyframes` | `Css.KeyframesIn` |
| `Css.Global` | `Css.GlobalIn` |
| `Css.AtRule` | `Css.AtRuleIn` |
| `Css.Extract` | `Css.ExtractFrom` |
| `Css.Snapshot` | `Css.SnapshotFrom` |

`CssSnapshot.CssText` is plain CSS. `HydrationText` includes the ownership marker and framed entries expected by browser adoption. Render the hydration text as the text content of a `<style>` whose ID and nonce come from the same snapshot; a browser context configured with those values adopts it without rewriting or duplicating rules.

## Build Output

`Jazor.Style` uses the standard Jazor output contract:

- `JazorMode=none` writes no frontend artifacts.
- `JazorMode=debug` materializes `Jazor.Style/runtime.mjs`, its source map, and a manifest entry.
- `JazorMode=release` includes the runtime and its consumers in `bundle.js`.

No `Jazor.Style`-specific build property exists. Release remains runtime CSS-in-JS; it does not emit a separate `.css` file or invoke PostCSS, autoprefixer, or CSS Modules.

## Stable Boundaries

- The package does not wrap Goober or ship third-party JavaScript.
- It does not provide `styled(Component)` or a Vue/component-library adapter.
- It does not parse raw CSS blocks or tagged templates.
- It does not add compiler intrinsics, analyzer exceptions, or RazorVue lowering branches.
- It does not reclaim rules automatically. Use inline styles or custom properties for high-cardinality continuous values.
- Declaration values remain authored CSS source; Jazor.Style validates structure required for deterministic serialization, not the complete CSS value grammar.

## Verification

```powershell
dotnet run --file scripts/csharp/generate-jazor-style-properties.cs -- --check
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css
dotnet run --file scripts/csharp/test-dotnet.cs -- --project css-browser
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter JazorStyle
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter JazorStyle
```

The browser verification executes the generated runtime and checks computed styles, CSP nonce propagation, one-style ownership, Unicode framing, HMR adoption, Shadow DOM targeting, and detached-snapshot hydration.
