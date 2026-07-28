# Release Notes

## 2026-07-29

- `Jazor.Css` now supports isolated style contexts, request-local extraction, hydration snapshots, and nonce-aware style ownership for both `document` and `ShadowRoot` targets.
- Structured styling now covers `@container`, `@layer`, `@scope`, `@starting-style`, and declaration-block at-rules such as `@font-face`, `@property`, `@counter-style`, and nested `@page` rules without accepting raw CSS.
- Existing class names, keyframe names, global rules, DOM framing, RazorVue string consumption, and the standard `JazorMode` debug/release workflow remain compatible.

## 2026-07-28

- RazorVue now consumes the final Roslyn compilation produced by the Razor Source Generator. It no longer requires Razor host outputs or reparsing generated C#.
- Jazor output is configured through `JazorMode`: `none` produces no files, `debug` produces modules and a manifest, and `release` produces the production bundle.
- The default output root is `wwwroot/jazor`; release builds write only `bundle.js` and `bundle.js.map` to that directory.
- Public Vue binding packages are compatible with .NET 11 Preview 6.
- Razor-to-Vue generation is now supplied by the explicit `Jazor.Vue` package, while `Jazor` owns the shared analyzer and compiler dependencies so generators are loaded once.
- `Jazor.Admin` provides UI-library-neutral admin-shell contracts and native RazorVue shell components for layout, navigation, breadcrumbs, page actions, controlled collapse, routing targets, and application-wide display state. Forms, tables, authentication fields, and concrete pages remain application-owned.
- `Jazor.Css` adds an independent opt-in, framework-neutral CSS-in-JS runtime with 705 generated Webref properties, deterministic class and keyframe names, nested selectors, media/supports rules, global styles, nonce-aware DOM injection, HMR-safe adoption, and non-destructive extraction. It uses the existing `JazorMode` debug/release pipeline and introduces no CSS-specific build properties.
- RazorVue component lowering now keeps compiler semantics on the Acornima AST path. Import rebasing, string literals, and slot sequence normalization no longer serialize and reparse JavaScript text, and forwarded, named, scoped, typed, and conditional slots preserve zero, one, or many child nodes.
- RazorVue component events support both synchronous delegates and asynchronous `Func<Task>` / `Func<TValue, Task>` handlers.
- Switch expressions now preserve guarded discard-arm semantics: `_ when condition` falls through to later arms when its guard is false.
- The JazorAdmin sample uses `Jazor.Admin` for application framing, routing, controlled sidebar collapse, live sidebar/top navigation modes, breadcrumbs, and page containment. Sample-owned pages cover login, lock screen, localized 404/500 recovery, global theme/language/grayscale controls, asynchronous tables, typed forms, action feedback, and a responsive TDesign implementation through packaged real-browser smoke verification on desktop and mobile viewports. A separately built companion application verifies assembly-level `VueInject` replacement, implementation prop/slot names, default imports, slot rendering, and event-driven state updates on the current `.mjs` pipeline.

## 2026-07-27

- RazorVue render-context now covers the core generated component semantics for render surface, component props, DOM and component events, slots, bind, lifecycle, references, metadata, and browser DOM behavior.
- RazorVue now has a direct VNode emitter for linear element/content/attribute output, static component props/listeners, regions, static markup, non-generic and typed scoped slots, bulk attrs, element key/value updates, DOM bind modifiers, named event metadata, and ref captures, with render-context retained as oracle/transition coverage.
- Production bundling now supports explicit Deno and Netpack lanes over the same manifest contract.
- Import-backed `.vue` SFC assets now flow from explicit component references into the manifest and production bundles without source-root scanning or a separate frontend asset API.
- External package consumers can build RazorVue output and Netpack bundles from the local `Jazor` NuGet package path without relying on repository-local tool binaries.
- RazorVue now has a G2 performance benchmark entrypoint that records runtime throughput, browser heap, generated artifact size, build timing, and release performance reports.
