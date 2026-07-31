# Release Notes

## 2026-07-31

- Nested structural record deconstruction now reads configured and inherited property keys directly, preserves nested `var` declarations, and no longer depends on record `Deconstruct` methods that are not emitted at runtime.
- Runtime member classes now preserve expression-bodied and block-bodied `init` accessors as JavaScript setters, including C# `field`-backed properties, while bodyless automatic `init` accessors retain their getter-only runtime shape.
- Private runtime member-class fields now use the same JavaScript private name in declarations and instance or static references, including compiler-generated property backing fields.
- Object literals keyed by compile-time negative ECMAScript numbers now emit valid computed-property syntax while preserving the numeric value.

## 2026-07-30

- CLR mappings now support `Half` through the Number carrier and `Int128` / `UInt128` through fixed-width BigInt semantics, including parsing, comparison, numeric helpers, bit counting, rotation, 128-bit arithmetic wraparound, and runtime-checked division and remainder overflow behavior.
- Generic whitelist compatibility lookup now reuses indexed key shapes, preventing ordinary generic method calls from repeatedly scanning the full CLR member catalog during compilation.
- Inline-backed CLR and host calls now preserve C# receiver and argument evaluation order, eager timing, and single-evaluation semantics when templates repeat, omit, reorder, conditionally consume, or capture placeholders in deferred functions.
- Composite property, tuple, and nested list patterns now evaluate each member input once, preventing repeated C# or JavaScript getter side effects while preserving pattern order and results.
- Nested object initializers now complete their values before assigning a property or indexer, preserve mapped setter dispatch, evaluate computed targets once, and prevent setters from observing partially initialized objects.
- Unsupported standalone `System.Index` and `System.Range` values now produce explicit, source-located diagnostics that direct authors to contextual `^` and `..` indexer or slice usage.
- Source-map source content now resolves through exact normalized syntax-tree paths, so files with the same name in different directories retain their own `sourcesContent` instead of relying on ambiguous filename fallback.
- Mapped compound assignment and increment/decrement now evaluate direct member receivers and index keys once before the right-hand side, including side-effecting fields, properties, and ECMAScript indexers.

## 2026-07-29

- `ECMAScript.Style` is the independent ECMAScript ecosystem package for framework-neutral CSS-in-JS. Its sole public facade is the lowercase static class `css`; consumers may use qualified calls such as `css.style(...)` and `css.px(...)`, or direct `style(...)` and `px(...)` calls through a static using.
- The 705 generated CSS properties now use native C# union domains and nominal values for lengths, percentages, colors, times, display values, tracks, transforms, and related syntax. Cross-domain and implicit string assignments fail at compile time, while `raw(...)` remains the explicit path for future or unmodeled CSS.
- Typed units, variables, colors, grid functions, transforms, keywords, and `calc(...)` operators compose as ordinary C# expressions. Mixed length-percentage arithmetic remains distinct from pure lengths.
- Debug materialization now publishes the single root entry `style.mjs` and its source map. The stable `jazor-css:v1` naming, class/keyframe hashes, DOM framing, isolated contexts, Shadow DOM ownership, snapshots, hydration, and release Bundle behavior remain unchanged.
- `ECMAScript.Style` remains an independent opt-in package and adds no Style-specific build configuration; it uses only `JazorMode`, `JazorDir`, and `JazorTool`.
- Dynamic Razor event modifiers now preserve their boolean conditions, including repeated `preventDefault` and `stopPropagation` modifiers, instead of being treated as unconditionally enabled.
- RenderTreeBuilder helpers can compose output after root-level local declarations while declarations inside an open element or component frame remain explicitly rejected.
- CLR runtime modules now annotate imported hashed JavaScript helper declarations with their authored CLR member names, making packaged runtime output easier to inspect without changing runtime behavior.
- Nullable `GetValueOrDefault()` calls now emit the correct default for the underlying value type, including booleans, characters, 64-bit integers, and enums.
- Compiler-generated temporary names are stable across repository relocations and parallel Git worktrees instead of depending on absolute source paths.
- BigInt-mapped increment and decrement operations now preserve BigInt operands for locals and mapped indexers, including `Int128` and `UInt128` values.
- Generated modules now publish and import the `System.Guid` runtime implementation for parsing, formatting, equality, and hash-code operations.
- Character-to-number and number-to-character conversions preserve UTF-16 code units, and nested conditional-access initializers emit valid ECMAScript expressions.
- Custom interpolated-string handler additions now report a source-located compiler diagnostic instead of leaking an internal range exception; handler creation, addition, and append protocols remain explicitly unsupported.

## 2026-07-28

- RazorVue now consumes the final Roslyn compilation produced by the Razor Source Generator. It no longer requires Razor host outputs or reparsing generated C#.
- Jazor output is configured through `JazorMode`: `none` produces no files, `debug` produces modules and a manifest, and `release` produces the production bundle.
- The default output root is `wwwroot/jazor`; release builds write only `bundle.js` and `bundle.js.map` to that directory.
- Public Vue binding packages are compatible with .NET 11 Preview 6.
- Razor-to-Vue generation is now supplied by the explicit `Jazor.Vue` package, while `Jazor` owns the shared analyzer and compiler dependencies so generators are loaded once.
- `Jazor.Admin` provides UI-library-neutral admin-shell contracts and native RazorVue shell components for layout, navigation, breadcrumbs, page actions, controlled collapse, routing targets, and application-wide display state. Forms, tables, authentication fields, and concrete pages remain application-owned.
- `ECMAScript.Style` adds an independent opt-in, framework-neutral CSS-in-JS runtime with 705 generated Webref properties, deterministic class and keyframe names, nested selectors, media/supports rules, global styles, nonce-aware DOM injection, HMR-safe adoption, and non-destructive extraction. It uses the existing `JazorMode` debug/release pipeline and introduces no style-specific build properties.
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
