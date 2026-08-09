# ECMAScript.Style

Strongly typed, deterministic CSS-in-JS for ECMAScript modules authored in C#.

`ECMAScript.Style` is an independent opt-in module in the ECMAScript ecosystem. It turns structured C# values into the standard `style.mjs` runtime module while preserving ordinary ECMAScript imports, RazorVue interoperability, stable content-based names, isolated registries, Shadow DOM ownership, server-rendered snapshots, CSP nonces, and idempotent hydration.

## Installation

```xml
<ItemGroup>
  <PackageReference Include="ECMAScript.Style" Version="0.1.33" />
</ItemGroup>
```

The package depends on the exact same version of `Jazor` for compilation and artifact emission. It installs no Razor hook, CSS-specific MSBuild target, Vue adapter, or compiler branch. Merely referencing the package does not register styles; registration occurs only when a `css` API is executed.

## Authoring

The public facade is the lowercase static class `css`. Use it by qualification or import its members explicitly:

```csharp
using ECMAScript.Style;
using static ECMAScript.Style.css;

var actionClass = style(new CssRule
{
    Display = inlineFlex,
    AlignItems = keyword("center"),
    Gap = rem(0.5),
    Width = percent(100) - rem(2),
    Color = varOr("--action-color", color("white")),
    BackgroundColor = hex("1769aa"),
    Border = px(1) | solid | var("--action-border"),
    BackdropFilter = filters(blur(px(12)), saturate(1.15)),
    TransitionDuration = ms(180),
    Opacity = 0.9,
    ["--action-shadow"] = raw("0 0.25rem 0.75rem rgb(0 0 0 / 18%)"),
    Children =
    [
        new(CssChildKind.Selector, "&:hover, &:focus-visible", new CssRule
        {
            BackgroundColor = hex("125486")
        }),
        new(CssChildKind.Container, "toolbar (width < 40rem)", new CssRule
        {
            Width = percent(100)
        })
    ]
});
```

The qualified form remains available:

```csharp
var className = css.style(rule);
var width = css.px(24);
```

Package-owned global usings are not installed. Each consumer decides whether `using static ECMAScript.Style.css;` is appropriate.

## Typed Values

The generated catalog contains 705 writable `CSSStyleDeclaration` properties from the locked `@webref/css@6.12.7` grammar snapshot. Properties use native C# union domains instead of `string?`, including:

| Domain | Representative properties | Accepted values |
| --- | --- | --- |
| `CssLengthPercentageValue` | `Width`, `Margin`, `Gap` | `px(...)`, `rem(...)`, `percent(...)`, mixed `calc(...)`, variables, keywords |
| `CssColorValue` | `Color`, `BackgroundColor` | `color(...)`, `hex(...)`, `rgb(...)`, `rgba(...)`, variables, color keywords |
| `CssTimeValue` | `TransitionDuration`, `AnimationDelay` | `ms(...)`, `seconds(...)`, variables |
| `CssNumberPercentageValue` | `Opacity` | numeric values, `percent(...)`, variables |
| `CssDisplayValue` | `Display` | `block`, `flex`, `grid`, `inlineFlex`, `none`, variables |
| `CssTrackValue` | `GridTemplateColumns`, `GridAutoRows` | lengths, percentages, `fr(...)`, `minMax(...)`, `repeat(...)` |
| `CssBoxShadowValue` | `BoxShadow`, `WebkitBoxShadow` | `shadows(new CssShadow(...))`, `none`, variables, CSS-wide keywords |
| `CssBorderValue` | `Border`, `BorderTop`, `BorderInline` | `px(...) | solid | color`, `none`, variables, CSS-wide keywords |
| `CssFilterValue` | `Filter`, `BackdropFilter` | `filters(blur(...), saturate(...))`, `none`, variables, CSS-wide keywords |

Nominal token types prevent accidental cross-domain assignments. For example, `Width = deg(10)`, `Color = rem(1)`, and `Height = "10px"` do not compile. Mixed length-percentage arithmetic has its own `CssLengthPercentage` type, so it is accepted by `Width` but rejected by pure-length properties such as `ColumnWidth`.

The value API includes:

- units: `px`, `rem`, `em`, viewport units, physical units, `percent`, angles, times, frequencies, and resolutions;
- colors and text: `color`, `hex`, `rgb`, `rgba`, `hsl`, `hsla`, `url`, and `str`;
- variables and identifiers: `var`, `varOr`, `ident`, and `keyword`;
- grids and transforms: `fr`, `minMax`, `fitContent`, `repeat`, `translate`, `rotate`, `scale`, and `transform`;
- borders and filters: `px(1) | solid | var("--border")`, `thin | dashed | currentColor`, `blur`, `grayscale`, `saturate`, and `filters`;
- shadows: `shadows(new CssShadow(px(0), px(4), Blur: px(14), Color: rgba(31, 52, 78, 0.05)))`;
- numeric composition: typed operators produce `calc(...)`, with `min`, `max`, and `clamp` for length values.

Border shorthand token composition is intentionally type-directed: `CssLength`, colors, variables, raw escapes, named widths, and named styles implement `ICssBorderPart`; `|` produces `CssBorder`, which is accepted by border properties but not unrelated properties such as `Width`. Named line widths and styles are token types rather than C# enums because enums cannot define the `|` operator. Other closed keyword domains remain string enums where composition is not part of their CSS grammar.

Use `raw(...)` when valid CSS cannot yet be represented by the typed surface:

```csharp
var rule = new CssRule
{
    Width = raw("anchor-size(--card inline, 20rem)"),
    Color = raw("oklch(from var(--brand) l c h)")
};
```

`raw(...)` is explicit so future syntax remains available without weakening every property back to an unrestricted string.

## Rules And Ordering

Named declarations are normalized by final CSS name and sorted ordinally, so C# initializer order does not affect the generated name. Order remains significant for `Additional`, `Children`, keyframe frames, and nested at-rules.

```csharp
var layoutClass = style(new CssRule
{
    Additional =
    [
        declaration("display", keyword("-webkit-box")),
        important("display", flex)
    ]
});
```

`!important` is available as a typed value modifier. Use `important(px(20))` (or another value accepted by the target property); the value's original CSS domain is preserved. The `important(name, value)` overload is reserved for an intentional additional declaration when a duplicate property is required.

`CssChildKind` supports `Selector`, `Media`, `Supports`, `Container`, `Layer`, `Scope`, and `StartingStyle`. Selector lists are processed with quote, escape, parenthesis, and attribute-selector awareness; nested grouping rules preserve the active selector and author order.

## Keyframes And At-Rules

```csharp
var fadeIn = keyframes(
    new("from", new CssDeclarations { Opacity = 0 }),
    new("to", new CssDeclarations { Opacity = 1 }));

global("html, body", new CssRule
{
    Margin = px(0),
    MinHeight = percent(100)
});

atRule(new CssAtRule(
    "font-face",
    new CssDeclarations
    {
        FontFamily = str("Example Sans"),
        ["src"] = raw("url('/fonts/example.woff2') format('woff2')"),
        FontDisplay = keyword("swap")
    }));
```

`CssAtRule` represents declaration blocks such as `@font-face`, `@property`, `@counter-style`, `@page`, and nested page-margin rules. Statement rules such as `@charset`, `@import`, and `@namespace` are excluded because their ordering and fetch semantics do not fit incremental registration.

## Contexts And Hydration

The default context targets `document.head`. Configure it before its first registration:

```csharp
configure(new CssOptions
{
    StyleId = "application-styles",
    Nonce = cspNonce
});
```

Create an explicit context for Shadow DOM or request-local rendering:

```csharp
var shadowStyles = context(new CssOptions
{
    Target = shadowRoot,
    StyleId = "widget-styles",
    Nonce = cspNonce
});

var widgetClass = style(shadowStyles, widgetRule);

var requestStyles = context(new CssOptions
{
    Detached = true,
    StyleId = "request-styles"
});

global(requestStyles, "body", globalRule);
var result = snapshot(requestStyles);
```

The same operation names are overloaded for the default and explicit contexts:

| Default context | Explicit context |
| --- | --- |
| `style(rule)` | `style(context, rule)` |
| `keyframes(frames)` | `keyframes(context, frames)` |
| `global(selector, rule)` | `global(context, selector, rule)` |
| `atRule(rule)` | `atRule(context, rule)` |
| `extract()` | `extract(context)` |
| `snapshot()` | `snapshot(context)` |

`CssSnapshot.CssText` contains plain CSS. `HydrationText` contains the stable ownership marker and UTF-16 length-framed entries used for browser adoption. A browser context with the same target, style ID, and nonce adopts the node without rewriting or duplicating rules.

## Build Output

`ECMAScript.Style` uses the standard Jazor build contract and adds no configuration properties:

| `JazorMode` | Output |
| --- | --- |
| `none` | No frontend artifacts |
| `debug` | `style.mjs`, `style.mjs.map`, consumer modules, and `jazor-manifest.json` |
| `release` | `bundle.js` and `bundle.js.map`; the Style runtime is included in the bundle |

The default output root is `$(MSBuildProjectDirectory)\wwwroot\jazor\`. Release remains runtime CSS-in-JS and does not produce a separate `.css` file or invoke PostCSS, autoprefixer, or CSS Modules.

## Stable Boundaries

- `ecmascript-style:v1`, `ecs-*`, `ecs-k-*`, and the default style ID remain stable protocol values.
- `ECMAScript.Style` does not wrap Goober or ship third-party JavaScript.
- It does not provide `styled(Component)`, a Vue wrapper, or a component-library adapter.
- It does not parse raw CSS blocks or tagged templates.
- It does not add CSS-specific compiler, analyzer, RazorVue, or MSBuild paths.
- Registered rules are not reclaimed automatically; use inline styles or custom properties for high-cardinality continuous values.

## Verification

```text
dotnet run --file scripts/csharp/generate-ecmascript-style-properties.cs -- --check
dotnet run --file scripts/csharp/test-dotnet.cs -- --project style
dotnet run --file scripts/csharp/test-dotnet.cs -- --project style-browser
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter EcmaScriptStyle
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter EcmaScriptStyle
```

Browser verification executes the generated module and checks computed styles, nonce propagation, one-node ownership, Unicode framing, module reload adoption, Shadow DOM targeting, and detached snapshot hydration.
