# Repository Guidelines

## Project Structure & Module Organization

`Jazor.slnx` is the entry point for the .NET solution. The transformation branch has one active Razor-to-Vue direction:

| Line | Mode | Key Projects | Description |
|------|------|-------------|-------------|
| **Razor-to-Vue transformation** | Active | `Jazor.RazorVue`, `Jazor.Analyzer`, `Jazor.Compiler`, `Jazor.Emit` | Official Razor SG generated C# -> Roslyn `IOperation` -> Vue render-function `.mjs` |

Shared infrastructure used by the active line:

| Project | Role |
|---------|------|
| `src/Jazor.Compiler` | Core C#-to-JS compiler (`IOperation` -> Acornima ESTree), targets `netstandard2.0` |
| `src/Jazor.CLR` | CLR runtime shims with `[WhiteList]` declarations and JavaScript implementations |
| `src/Jazor.CLR/doc/` | Module docs co-located with CLR runtime source |
| `src/Jazor.Analyzer` | Static analyzer enforcing whitelist usage at compile time |
| `src/Jazor.Compiler.Generator` | Source generator scanning `[WhiteList]` attributes |
| `src/Jazor.CLR.Generator` | Type mapping and binding code generator |
| `src/Jazor.Emit` | Emit pipeline, bundle materialization, and SourceMap output |
| `src/Jazor.Common` | Shared contracts, naming, and symbol formatting utilities |
| `src/ECMAScript` | Core ECMAScript AST implementation |
| `src/ECMAScript.Contract` | ECMAScript contract definitions and attributes |
| `src/ECMAScript.WebIDL.Generator` | WebIDL spec-to-C# binding generator |
| `src/Jazor` | NuGet package bundling runtime, analyzer, generators, emit, and MSBuild integration |

ECMAScript ecosystem layer:

| Project | Role |
|---------|------|
| `src/ECMAScript.Vue3` | Vue 3 core type bindings |
| `src/ECMAScript.VueContract` | Vue component contracts, descriptors, and slot metadata attributes |
| `src/ECMAScript.VueRoute` | Vue Router type bindings |
| `src/ECMAScript.Vuetify` | Vuetify component wrappers (props, events, slots, value types) |
| `src/ECMAScript.Pinia` | Pinia state management bindings |
| `src/ECMAScript.Style` | Strongly typed, deterministic CSS-in-JS authoring and runtime module |

ASP.NET Core integration layer:

| Project | Role |
|---------|------|
| `src/Jazor.AspNetCore` | ASP.NET Core runtime integration |
| `src/Jazor.AspNetCore.Dev` | Development-time integration (HMR, DevServer bridging) |

Test projects live under `src/Jazor.CompilerTest`, `src/Jazor.CLR.Test`, `src/Jazor.RazorVue.Sg.Test`, `src/Jazor.EmitTest`, `src/ECMAScript.Style.Test`, `src/ECMAScript.WebIDL.GeneratorTest`, `src/ECMAScript.VueRoute.Test`, `src/ECMAScript.Pinia.Test`, and `src/ECMAScript.Pinia.Testing.Test`. Auxiliary tooling outside the main solution includes `src/Wiki` and `samples/`.

Documentation is organized under `docs/` in five categories:
- `docs/01-overview/` — product scope, reading map, and system overview
- `docs/02-architecture/` — current architecture, module ownership, and stable boundaries
- `docs/03-guides/` — installation, configuration, authoring, development, and testing
- `docs/04-roadmap/` — current direction, status, and reproducible quality gates
- `docs/05-history/` — concise background for retired routes and major evolution

Documentation interpretation rule:
- Historical exploration, old audits, and fixed test snapshots belong only to `docs/05-history/evolution.md`; Git history retains the detailed record.
- For current compiler semantics, lowering direction, and support boundaries, prefer `src/Jazor.Compiler/ImplementationPrinciples.md`, `docs/02-architecture/compiler.md`, `docs/04-roadmap/current-status.md`, and the current `src/Jazor.Compiler/README.md` / `src/Jazor.CompilerTest/README.md`.

RazorVue artifact and lowering boundary rule:
- The production input is official Razor SG generated C#, and the output contract is a Vue render-function `.mjs` artifact. Razor DR/IR, generated SFC output, and Jolt protocols are not fallback paths.
- Razor/C# compiler already validates Razor-side unknown parameters, required parameters, and parameter type mismatches. RazorVue lowering should directly translate official SG generated C# and must not duplicate those checks.
- Do not introduce intermediate wrapper-JS marker protocols for RazorVue slot/template transport when the same behavior can be expressed as the final Vue render-function shape directly.
- When RazorVue lowering needs CLR-aware type mapping, import collection, symbol binding, reference stability, or other compiler-owned semantics, it must flow through `Jazor.Compiler` / `SemanticWalker` translation hooks rather than bypassing them with hand-assembled Acornima AST or ad hoc JavaScript string stitching.
- Direct AST/manual-JS construction in RazorVue is acceptable only for Vue artifact framing that `Jazor.Compiler` does not own (for example Vue runtime bridge code). It must not replace compiler translation for C# expression/function/member semantics.

## Build, Test, and Development Commands

Run from the repository root unless noted:

- `dotnet restore Jazor.slnx` restores NuGet packages for the solution projects.
- `dotnet build Jazor.slnx` builds the full solution targeting the current `net11.0` preview SDK.
- `dotnet run --file scripts/csharp/test-dotnet.cs` builds once and runs the current compiler, CLR, Pinia, Pinia.Testing, VueRoute, Razor SG, and emit suites.
- `dotnet run --file scripts/csharp/test-dotnet.cs -- --project razor-sg` runs the focused Razor SG suite.
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj` runs the compiler regression suite.
- `dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj` runs the Razor SG integration suite.
- `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj` runs the emit and bundle regression suite.
- `dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "SemanticWalkerPatternTest"` runs one compiler test class.
- In `src/ECMAScript.WebIDL`, run `npm install` once, then `npm run build` to compile the archived TypeScript generator, and `npm run clean` to clear its build artifacts.

Script policy:
- Repository automation scripts must use single-file C# entrypoints (`dotnet run --file ...`) rather than PowerShell.
- Do not add new `.ps1` files for build, test, publish, smoke, browser verification, packaging, release, or local diagnostic workflows.
- When migrating or extending automation, update the existing `.cs` script under `scripts/csharp/` or add a new single-file C# script there (or beside the sample it owns) instead of introducing a shell wrapper.
- Treat `.ps1` as prohibited for repository-owned automation except for external third-party artifacts outside repository ownership (for example package caches or generated dependency bins).

Local diagnostic scripting rule:
- Prefer .NET single-file C# apps for local probes that need reflection, Roslyn, metadata inspection, process orchestration, or complex quoting. Put reusable probes under `scripts/csharp/`, run them with `dotnet run --file scripts/csharp/<name>.cs`, and keep one-off scratch inputs under `.tmp/` only when needed.
- Do not create repository-owned PowerShell diagnostic scripts. If a probe becomes reusable, promote it to a checked-in `.cs` single-file script.

## Coding Style & Naming Conventions

Follow the existing code style: 4-space indentation in both C# and TypeScript, file-scoped namespaces in C#, `PascalCase` for public types and test classes, `camelCase` for locals and parameters, and leading underscores for private fields. Keep partial compiler logic grouped by concern using names like `SemanticWalker.cs.Pattern.cs`. Avoid hand-editing obviously generated files unless the generation source is part of the change.

Commenting rule:
- 代码注释可以根据上下文适当使用中文/英文混合表达；关键代码、容易误用或容易回归的代码必须补充简洁注释，优先说明设计原因、隐含约束、求值/生命周期顺序和副作用，而不是逐行翻译实现。
- 保持注释与代码行为同步。普通且自解释的代码不强制添加注释；复杂 lowering、协议边界、稳定性约束、错误处理分支和 workaround 应在代码附近留下中英文混合的 orienting comment，便于不同语言背景的维护者快速确认不能随意修改的原因。

Implementation scope rule:
- 只实现当前产品契约所必需的代码。绝对禁止添加未由 GOAL、SPEC 或现有失败场景要求的认证、权限、安全审计、对抗性校验、兼容 fallback 和预防性抽象。
- 只允许保留数据正确性、格式约束、确定性、错误传播以及验收必要的检查，绝对禁止过度兜底和防御性编程。

Whitelist generation rule:
- `src/Jazor.Compiler/WhiteList.cs.Generate.cs` is generated output and must not be edited manually.
- After changing CLR whitelist sources (for example `src/Jazor.CLR/module/*Module.cs` with `[Jazor(...)]` mappings), regenerate via `dotnet run --project src/Jazor.Compiler.Generator/Jazor.Compiler.Generator.csproj` and commit the regenerated whitelist file together with source changes.
- `Op.Discard` mappings stay in CLR module source declarations as unsupported/todo markers and are not emitted into `WhiteList.cs.Generate.cs`.

CLR carrier inference rule:
- Jazor.CLR static adapter signatures are the single source of truth for CLR-type-to-internal-carrier relationships. Infer those relationships by aligning the Roslyn symbol named by `[Jazor(...)]` with the adapter method's receiver, parameter, constructor-result, and return symbols.
- Internal carrier types such as `RuntimeModule.JDateTime` are implementation-only value wrappers. Do not add explicit carrier attributes, parallel registries, naming-based compiler special cases, hidden marker properties, or structural shape tests to identify them.
- A carrier relationship may support runtime value discrimination for `is`, declaration patterns, and `as`; it must not redirect CLR member dispatch away from the mapped module and must not be exposed as the `typeof(T)` / `System.Type` token for the mapped CLR type.
- Multiple CLR types may intentionally use the same internal carrier when the supported CLR slice has that representation. This is not a conflict and must not trigger invented one-to-one validation.

Whitelist key contract rule:
- Persisted whitelist keys have one canonical contract. If `[Jazor(...)]` explicitly provides the member/type string, store that authored string unchanged.
- If the member/type string is omitted and the generator derives the key from a Roslyn symbol, the derived key must be exactly `symbol.OriginalDefinition.ToDisplayString(Jazor.Common.Format.NameFormat)`.
- Do not add generator-only or lookup-only canonicalization to persisted keys. In particular, do not strip `extern` or silently rewrite other modifiers from symbol-derived keys.
- Lookup fallbacks such as static-extension display, override/virtual compatibility, const-field compatibility, or generic-parameter compatibility are consumer-side probes only. They must not change the canonical key written into `WhiteList.cs.Generate.cs`.
- If a new shared normalization/fallback rule is ever required, implement it as a shared utility under `src/Jazor.Common` and update both generator and compiler tests. Do not hide key rewrites inside `Jazor.Compiler.Generator`-private helpers.

CLR comparer mapping rule:
- For comparer families, treat concrete comparer and interface comparer as one contract surface. When introducing support for `System.Collections.Generic.EqualityComparer<T>` members, add matching callable coverage for `System.Collections.Generic.IEqualityComparer<T>` where the same usage path exists.
- For equality infrastructure, ship member support in coherent slices instead of one-off points: prefer `Default.get + Equals(T, T) + GetHashCode(T)` as a consistent set unless a narrower user requirement is explicitly requested.
- Keep runtime semantics centralized: if concrete and interface paths share behavior, route both through the same core helper logic to avoid drift in null handling, `NaN` handling, or identity/value comparison behavior.
- Every comparer-surface addition must include both:
  - whitelist metadata assertions in `src/Jazor.CLR.Test` (type alias + member Op/path)
  - compiler emission assertions in `src/Jazor.CompilerTest` for both direct concrete calls and interface-typed dispatch calls

CLR inline/import tradeoff rule:
- Keep `Inline` templates short and readable. Do not pack complex multi-branch semantics into long inline expressions.
- Prefer `Import` when behavior needs non-trivial control flow, repeated guards, or shared helper logic across modules.
- Do not switch to `Import` mechanically. If compiler-side semantic analysis already guarantees a condition (for example, a nullability/property invariant at call site), avoid duplicating that check in CLR runtime code unless runtime behavior requires it.
- For the same API surface, choose one strategy consistently (`Inline` or `Import`) so concrete and interface paths do not drift in behavior or readability.
- The acceptance bar is both robustness and maintainability: preserve observable semantics while keeping module code easy to read and review.

Compiler support boundary rule:
- Public host API constraints should be expressed in the C# surface first. Prefer explicit parameter/return types, closed union types, narrow interfaces, and shaped records over weakening the public contract to `object`, `object?`, or unconstrained generic fallbacks.
- Do not add `object`/`object?` catch-all parameters or open generic fallback overloads on ECMAScript/host-facing APIs merely to mirror JavaScript `any` flexibility when the intended usage set can be represented in C#.
- Type erasure in emitted JavaScript is not by itself a reason to weaken the authored C# API. If the goal is authoring-time strong typing, keep the stronger C# contract and let Roslyn overload/type checking enforce it.
- When a JavaScript value domain is known and intentionally shaped, prefer a closed, strongly typed C# union type as the primary public API surface. Union types are now the default representation for host value domains that previously required wrapper records plus `From(...)` factories.
- On `net11.0` / C# preview projects, the erased-value union priority order is: first the language-native `union` keyword, then a tagged fallback wrapper using `[System.Runtime.CompilerServices.Union]` + `IUnion` when native union cannot preserve semantics. Do not hand-author `[Union]` + `IUnion` wrappers when the same contract can be expressed as `public readonly union Name(T1, T2, ...)` while preserving required authoring ergonomics.
- Native erased-value unions are only safe when their branch types are mutually non-assignable. If any branch type is assignable to another branch type (for example `ScrollPositionElement : ScrollPositionCoordinates`), use a tagged fallback wrapper with `[System.Runtime.CompilerServices.Union]` + `IUnion` so `AsX` projections remain exact and cannot be widened by `Value as BaseType`. A bare `IUnion` implementation without `[System.Runtime.CompilerServices.Union]` is not an erased-value union contract.
- Preserve compatibility members such as `AsX` projections, collection builders, and strongly typed overload/initializer entry points when migrating an existing wrapper to the `union` keyword. The migration is only acceptable when normal assignment, implicit union construction, overload resolution, Razor SG binding, and compiler erased-value projection keep the same public behavior.
- Treat removed legacy union compatibility surfaces as fully retired. Do not add new public host APIs, generated WebIDL output, compatibility markers, or compiler erased-union recognition paths that depend on the retired generic-wrapper or marker model; use named native `union` contracts first, or a strongly typed `[System.Runtime.CompilerServices.Union]` + `IUnion` fallback when native union cannot preserve exact tagged projection semantics.
- If a class source type, scalar source type, or union branch can already be expressed cleanly through normal C# assignment, implicit conversion, overload resolution, or union construction, prefer that direct expression. Do not introduce `From(...)` helpers as a default habit when the language already models the contract cleanly.
- Use explicit overloads when they are clearer than a union for a small number of common call forms, when overload resolution materially improves C# authoring ergonomics, or when C# collection-initializer `Add(...)` binding needs a strong typed entry point.
- Treat explicit `From(...)` factories as a narrow compatibility or language-boundary bridge, not a second weak API surface and not the default authoring style: every factory overload must stay strongly typed, map to one intentional target contract, and exist only for scenarios union syntax, assignment, implicit conversion, or overload resolution cannot model ergonomically or legally.
- Only introduce analyzer rules, compiler special lowering, or JavaScript-side runtime guards when the required constraint cannot be expressed faithfully in the C# type system, or when runtime semantics explicitly require additional enforcement beyond C# authoring constraints.
- For JavaScript values with broad runtime domains, prefer dedicated host union/value types or explicit overload families over `object?` catch-alls. If a compiler/runtime special case is added, document why the C# surface alone was insufficient.
- RazorVue `.razor` component parameter surfaces must also remain valid for the official Razor Source Generator. Do not choose a parameter type shape that only RazorVue lowering can understand if the generated `.razor.g.cs` cannot compile under the SDK Razor SG. Static Razor attribute authoring, enum-like string domains, and union-valued props must be designed against both Razor SG binding rules and RazorVue lowering semantics.
- Generic arguments and array element types are treated as erased compile-time annotations unless the compiler is directly lowering that concrete type's runtime semantics.
- Do not reject `List<Unsupported>`, `Task<Unsupported>`, `Dictionary<TKey, Unsupported>`, `Unsupported[]`, or similar shapes only because their erased type arguments are unsupported.
- Reject unsupported external types only when the type itself is being materialized or lowered with runtime semantics, such as `new Unsupported()`, `default(Unsupported)` when a concrete lowering is required, runtime type checks, or direct static/instance member access on `Unsupported`.
- Validate methods, properties, and fields at the actual usage site. If an unsupported concrete type later participates in member access, invocation, property access, field access, or another runtime-sensitive lowering, the compiler must fail there.
- WhiteList/generic signature matching must be structural. Only declared generic parameters may be normalized; never treat concrete types as interchangeable with generic parameters.
- CLR module mapping follows "common-path first": prioritize high-frequency runtime APIs; long-tail methods/properties can stay `Op.Discard` or unsupported until there's concrete demand.

Analyzer diagnostics rule:
- `Jazor.Analyzer` is allowed to be stricter than `Jazor.Compiler` because analysis is cheaper and should diagnose problems earlier.
- The analyzer should eagerly report unsupported closed concrete external types that appear in erased positions such as generic arguments, array element types, collection-expression target types, local inferred types, and field/property/parameter/return signatures.
- The analyzer must not chase generic type-parameter provenance or dataflow. `T` itself is allowed; only concrete closed types are diagnosed early.
- Do not “fix” `Jazor.Compiler` by forcing it to reject every scenario that `Jazor.Analyzer` reports in erased positions. The current asymmetry is intentional: analyzer diagnostics are allowed to be earlier and stricter, while compiler acceptance/rejection is still decided at the actual runtime-sensitive lowering site.

Compiler implementation route rule:
- For `Jazor.Compiler` work, treat `src/Jazor.Compiler/ImplementationPrinciples.md` as the primary rationale document for lowering direction, support boundaries, and extension decisions.
- The first-class goals of compiler changes are: usage-site observable behavior, host semantic boundary clarity, and deterministic emission. Do not optimize for “looks more like hand-written JS” ahead of those three goals.
- When full CLR/runtime-shape equivalence is infeasible, preserve behavior in this order: evaluation order, side-effect count, final result, usage-site semantics, then runtime structure identity. Introducing synthetic temps, `SequenceExpression`, or IIFE wrappers is acceptable when needed to preserve that order.
- `SemanticWalker` is both the lowering layer and the final usage-site validation layer. `Jazor.Analyzer` may diagnose earlier, but lowering must still reject unsupported runtime-sensitive external types/members at the actual use site. Do not introduce silent raw-JS fallbacks for unsupported external members.
- Treat tuple lowering as erased value-composition lowering: preserve projection/deconstruction/comparison behavior, but do not preserve `System.ValueTuple` runtime identity. Treat `ref/out` and custom `Deconstruct` as explicit caller/callee protocol simulation, not as CLR address-model reconstruction.
- Treat enums as compile-time domain types that erase to underlying scalar constants at usage sites. Do not evolve them back into boxed CLR-like runtime enum objects, module-level enum declaration objects, or reverse-map JS enum artifacts by accident.
- Treat interfaces as contracts only: they may participate in analysis, host projection, and whitelist/implementation lookup, but they do not emit runtime declarations.
- For `is`/type-pattern checks against interface types that erase to `Object` at runtime, prefer Roslyn-driven compile-time folding when and only when the result is provable: emit `true`, `false`, or an explicit non-null check (`value !== null`) as appropriate. Preserve single-evaluation and side-effect order of the tested expression; if the result is not statically provable, keep an explicit unsupported failure rather than emitting unsound runtime heuristics, and make the diagnostic actionable by naming the source static type and target interface.
- For inheritance, the currently supported slice is same-module member-class inheritance with stable base-before-derived emission, `extends`, explicit `: base(...)` to `super(...)`, synthesized `super()` for derived classes without an explicit constructor, `base.Method(...)`/`base.Property`/`base.Property = value` lowering to `super`, base-method-group forwarding, and normal prototype dispatch. Keep explicit failures for base field access, `this(...)` constructor chaining, external base types, and any constructor-initializer protocol that has not been implemented end-to-end; never silently erase inheritance.
- For member-class constructor overloads, use one real JS `constructor` plus stable `$ctor_<hash>` helper methods and a selector supplied from the Roslyn-bound constructor symbol at every Jazor-compiled call site, including cross-module calls. Do not add an `arguments.length` fallback: same-arity overloads and overlapping optional-parameter ranges are valid because dispatch is symbol-bound, while external calls without a selector must fail explicitly. Bind optional defaults inside the selected branch, emit per-branch `super(...)` before helper execution for derived classes, keep dispatcher/helper insertion stable at the first explicit constructor slot, and explicitly reject `this(...)`, external-base constructor protocols, and `ref/out/in/params`-driven dispatch.
- Keep the output boundary layered: `AstConverter` owns module-level AST, writer/`ESGenerator` own JavaScript text plus module/source-map catalog carriers, and `Jazor.Emit` owns `.mjs` / `.mjs.map` / manifest / bundle materialization. Do not collapse these back into a vague “compiler directly writes files” model.
- Keep the import and compile mainline fixed: `SemanticWalker` collects `ImportSpecifier`s, `SenseArgument` flushes them, `AstConverter` merges them and emits module-header `ImportDeclaration`s with stable dedupe/order/alias behavior; consumer-side member dispatch remains `Compile -> Alias -> Inline -> Import -> normal lowering`, with `throw` from `Compile_*` treated as claimed-and-failed rather than silently falling back.
- Keep host semantics layered: `Alias` for simple name remaps, `Inline` for stable local expression templates, `Import` for explicit helper/module seams, and `Compile` for complex AST-level host semantics. If a host behavior needs context-sensitive AST construction or protocol-level lowering, prefer upgrading it to `Compile` rather than extending `Inline`.
- Stable temp naming, import alias stability, and source-origin/sourcemap anchoring are compiler contracts, not test-only conveniences. Do not reintroduce traversal-order-dependent names or per-call-site import alias drift.

## Testing Guidelines

Tests use `MSTest.Sdk` v4 with `coverlet.collector`. Add compiler regressions under `src/Jazor.CompilerTest`, CLR-specific tests under `src/Jazor.CLR.Test`, RazorVue SG regressions under `src/Jazor.RazorVue.Sg.Test`, emit regressions under `src/Jazor.EmitTest`, and WebIDL generator coverage under `src/ECMAScript.WebIDL.GeneratorTest`. Use explicit names like `Convert_ClassWithMethod_GeneratesFunctionDeclaration` or the existing `JazorVue*` naming in the relevant suite. Coverage settings target at least 85% line coverage and 80% branch coverage; keep new work covered before opening a PR.

Parallel test stability rules:
- Treat `[DoNotParallelize]` as a last-resort temporary escape hatch, not a normal fix. When parallel execution exposes a failure, prefer removing shared mutable state or isolating resources instead of disabling concurrency.
- Do not use process-global environment variables to steer per-test behavior when the same scenario can be passed through test-local settings, manifest/config payloads, dependency injection, or explicit method parameters.
- Test-spawned child processes must resolve to the current test/build output first. Do not silently fall back to repository-default `bin/Debug/...` artifacts in ways that can lock shared outputs or run a different binary than the one under test.
- Parallel-friendly tests must own their resources: use unique temp directories, unique file names, unique ports/pipes/process identities, and deterministic cleanup that short-circuits already-exited processes.
- When running multiple `dotnet test` lanes concurrently, isolate build outputs with distinct `BaseOutputPath` values. After a successful build, prefer focused `--no-build` reruns for verification.
- Keep targeted regression verification fast. As a default budget, the focused suite for a change should stay around 2 minutes or less unless the user explicitly asks for broader coverage.

## Release Publishing

Official NuGet publishing is performed only by `.github/workflows/nuget-publish-ref.yml`.
Push a `v*` tag or use `workflow_dispatch`; GitHub Actions owns trusted publishing
and any required credentials. Local `scripts/csharp/publish-nuget.cs` runs must use
`--skip-push` for package verification. Do not require or probe a local `NUGET_API_KEY`
when preparing or completing a repository release.

## Commit & Pull Request Guidelines

Recent history uses Conventional Commit-style prefixes, often with an emoji, for example `♻️ refactor(compiler): ...`, `✅ test(razorvue): ...`, and `🐛 fix(emit): ...`. Keep scopes specific (`compiler`, `razorvue`, `clr`, `emit`, `test`, `docs`) and subjects imperative. Pull requests should summarize the affected module, describe behavior changes, list the commands you ran, and include sample output or screenshots when the change affects generated code or developer-facing tooling.
