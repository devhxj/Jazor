# Release Notes

## 2026-08-05

- Vuetify RazorVue bindings now favor ordinary C# and Razor contracts: `X` plus `XChanged` supplies two-way binding, `OnX` supplies ordinary listeners, and `ChildContent` / `DefaultContent`, `XContent`, and PascalCase named fragments supply Vue slots. Bare `Save`, `Load`, `Next`, `Prev`, `AfterEnter`, `AfterLeave`, and `Submit` callback parameters were renamed to their `OnX` forms. Only Vue names that C# cannot express, such as colon events and dot slots, retain explicit metadata.
- `VuetifyGridSpanValue` now uses the native C# union contract while retaining its boolean, number, string, and numeric assignment authoring forms.
- `VCalendar` date, allowed-date, and interval-format values now use native C# unions. Their `AsX` projections and scalar/array convenience conversions remain, while the redundant JavaScript `From(...)` factories are removed.
- `ECMAScript.Style` now models `box-shadow` as typed C# data: compose one or more `CssShadow` records with `shadows(...)`, including optional blur, spread, color, inset, variables, `none`, and CSS-wide values. JazorAdmin themes and components now use the same typed shadow surface instead of raw shadow strings.
- JazorAdmin now validates the production application route in one ASP.NET Core host: RazorVue UI, Web API, Identity, OpenIddict SSO, organization and membership management, role-based resource-operation grants, platform accounts, and OpenID client/scope configuration. Its TDesign-inspired icon rail and scoped secondary navigation are authored with Razor and `ECMAScript.Style`, with no application-owned JavaScript, CSS, static `index.html`, or Blazor registration.
- CSS-in-JS keyframes now preserve `params` frames as one JavaScript array, and global selector validation accepts all legal CSS whitespace, including line breaks in readable selector lists.
- Jazor 0.1.48 build targets now exclude native runtime DLL assets before invoking the managed emit tool, so RazorVue builds work with dependencies such as SQLite that ship native `.dll` files.
- Jazor 0.1.47 allows external ECMAScript host proxies to consume a module's `default` export while keeping Jazor-authored module declarations on deterministic named exports.
- Generated WebIDL bindings now represent `ByteString` browser text as `string`, including Fetch, Headers, navigation preload, and XMLHttpRequest contracts.
- WebIDL bindings now distinguish WebCrypto's `BigInteger` byte-array typedef from the JavaScript `bigint` primitive, which maps to `System.Numerics.BigInteger` and retains the concise `AsBigInteger` union projection.
- Jazor 0.1.46 packages now ship the Acornima analyzer assemblies that match the compiler's 1.7.0 ABI, preventing runtime `MethodNotFoundException` failures during Razor compilation.
- Bound extension method groups now retain their receiver when used as delegates, including identifier receivers; generated callbacks preserve the original call target instead of losing instance context.
- Compound assignment, unsigned right shift, implicit derived constructors, property initialization, interpolation format intrinsics, and host-bound member dispatch now have focused Roslyn-operation regressions for their evaluation and runtime-shape contracts.
- Whitelist generation now rejects incomplete alias declarations at generation time, preventing a catalog entry with no usable runtime name.
- Compiler packages now use Acornima 1.7.0 while preserving the existing ESTree emission and parsing contracts.
- Imports whose public name collides with a declared or reserved module binding now receive a stable generated alias, and inherited generic static members retain their concrete runtime host.
- Interpolating `dynamic` values now reports the stable text-contract diagnostic instead of exposing an internal compiler exception.
- The compiler quality gate now verifies 10,297 genuine Roslyn `IOperation` scenarios at 98.94% line coverage and 96.01% branch coverage, satisfying its 10,000 / 98% / 96% release gate.

## 2026-08-04

- Standard interpolated strings now preserve C# null-to-empty conversion, `Boolean` text casing, numeric formatting, constant alignment, source-defined `ToString` dispatch, and single evaluation through compiler-owned ESTree lowering. Values without a stable runtime text contract now fail explicitly instead of inheriting JavaScript stringification.
- Source maps now keep an absolute source path when that source exactly equals the configured source root, avoiding a relative path that escapes the root directory.
- Compiler whitelist generation now publishes refreshed type and member mappings as one complete process-local catalog snapshot before CLR runtime modules are generated.
- The compiler quality gate now verifies 10,101 genuine Roslyn `IOperation` scenarios at 98.42% line coverage and 94.00% branch coverage. The RazorVue gate verifies 4,484 official Razor SG scenarios at 93.44% line coverage and 83.66% branch coverage.
- `DateTime` and `DateTimeOffset` now support their Gregorian-calendar constructor families, preserving calendar-null argument precedence, `DateTimeKind`, microseconds, and offset validation through the shared date carrier.
- `StringBuilder` now supports capacity-aware construction, `Capacity`, `MaxCapacity`, `EnsureCapacity`, string append and append-line paths, and content-based builder equality. Capacity growth preserves the .NET behavior where an allocation may briefly exceed `MaxCapacity` while already allocated space remains usable.
- `string.Intern` now runs through the string carrier contract, including null argument behavior; intern-table inspection remains an explicit boundary.
- Expression-tree and `IQueryable` lambda conversions now fail explicitly instead of being lowered as executable delegates, preserving the distinction from supported `Enumerable` callbacks.
- Generated runtime catalog assertions now track the shared char code-unit, comparer NaN ordering, and BigInt rotation helper contracts.
- Native `ECMAScript.Array`, `Set`, and `Map` now accept standard C# collection initializers; `Map` indexer and two-argument entries retain its typed `set` runtime behavior.
- Read-only collection, dictionary, and set construction now preserve their live-view and write-guard contracts instead of falling back to writable or snapshot carriers.
- Closures created inside a C# `for` loop now retain the loop control variable's single C# lifetime instead of inheriting JavaScript's per-iteration `let` binding behavior.
- `Nullable<T>.GetValueOrDefault(defaultValue)` now evaluates its receiver and explicit default argument eagerly from left to right before selecting the result, preserving fallback side effects even when the nullable contains a value.
- `Enumerable.Zip` now supports its three-source tuple overload alongside the existing two-source and result-selector forms, preserving source-order iterator creation and advancement, shortest-source termination, and reverse iterator closure.
- `Enumerable.CountBy` and both `AggregateBy` seed overloads now preserve comparer-aware grouping, first key representatives, insertion order, Int32 count bounds, and two-slot `KeyValuePair<TKey, TValue>` entries.
- Field-like instance events on generated non-record runtime member classes now preserve C# multicast subscription and removal semantics, invocation-list snapshots, method-group receiver identity, and conditional `Invoke` argument short-circuiting. Static, custom-accessor, virtual/override, by-reference, delegate-equality, and delegate-combination event forms remain explicit boundaries.
- Module methods, runtime member methods, and local functions using `yield` now generate JavaScript iterators; `async IAsyncEnumerable<T>` methods generate `async function*` while nested callback bodies remain isolated from the outer iterator shape.
- UTF-8 string literals now emit exact decoded UTF-8 byte sequences through the existing read-only span carrier, including escaped, raw, BMP, and supplementary-plane text.
- Lambda parameters with C# optional defaults now preserve omitted-call behavior at the generated JavaScript function boundary. By-reference lambda returns remain an explicit runtime boundary.
- Named arguments now retain C# source evaluation order while invoking Roslyn-bound parameter slots. `ref` and `out` array or member locations evaluate once and use the shared write-back protocol without reading an `out` location's prior value.

## 2026-08-03

- LINQ mappings now cover `Cast`, `OfType`, `TryGetNonEnumeratedCount`, comparer-aware `ToDictionary` and `ToHashSet`, and a broader set of selector, grouping, ordering, aggregation, and set operations through shared runtime helpers.
- CLR support now includes fixed-width integer and floating conversions, checked arithmetic, UTF-8 numeric parsing, Unicode character classification, deterministic scalar hash codes, and comparer-backed dictionary and set behavior.
- String span trimming and concatenation, line-ending replacement, joins and padding, one-dimensional `Array` parameter-index access, `ConditionalWeakTable` factory and clear operations, and `Exception` cause, `HelpLink`, and `Source` metadata now execute through generated runtime modules.
- `StringBuilder` fixed `float` and `double` append/insert overloads now reuse their corresponding numeric `ToString()` carrier semantics. Object/generic formatting, capacity, live views, CLR enumerators, and type/reflection protocols remain deliberate support boundaries.
- Read-only collection constructors and factories that require live views remain explicit support boundaries instead of returning writable or snapshot carriers.
- The compiler quality gate now verifies 8,265 scenarios at 96.26% line coverage and 90.02% branch coverage. The RazorVue quality gate verifies 4,482 official Razor SG scenarios at 93.44% line coverage and 83.68% branch coverage.

## 2026-08-02

- Nullable values and nested list patterns now preserve their C# null and single-evaluation semantics in generated JavaScript, including a stable failure for `Nullable<T>.Value` on an empty carrier.
- Official Razor components now retain static source-map paths, support optional `EventCallback` parameters with or without a listener, asynchronous `@bind:after` updates, and synchronous or asynchronous `@bind:set` method groups; their final render catalog generation remains stable across concurrent generator-driver use.
- Razor authoring errors reported by the official source generator now remain the sole diagnostic: RazorVue skips render-catalog generation for that invalid compilation instead of adding a secondary conversion failure.
- JazorAdmin now provides an expanded native RazorVue reference application with dashboard, release, audit, workspace, and settings flows, including controlled release-table selection and bulk actions.
- The compiler quality gate now verifies 8,158 scenarios at 96.43% line coverage and 90.71% branch coverage. The RazorVue quality gate verifies 4,472 official Razor SG scenarios at 93.44% line coverage and 83.67% branch coverage.

## 2026-08-01

- Razor-to-Vue now keeps ASP.NET Components catalog declarations and Razor-specific lowering inside the `Jazor.Vue` product boundary while retaining explicit, typed compiler extension contracts for product integrations.
- Official Razor source-generator output continues to produce direct Vue render-function `.mjs` artifacts; SFC, render-context marker protocols, and generated-builder fallbacks are not part of the supported output path.
- Dynamic `@attributes` on Razor components now maps descriptor-owned C# parameter names for plain objects, `Map` values, and KeyValuePair-shaped sequences before Vue VNode props are created, while explicit `@bind` values retain precedence.
- The JazorAdmin reference application now verifies local package consumption, native and `VueInject` builds, generated artifacts, and browser mounting through the packaged Deno host.
- The compiler quality gate now verifies 8,113 focused regression scenarios with 96.42% line coverage and 90.78% branch coverage, above its 8,000 / 95% / 90% release thresholds.

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
