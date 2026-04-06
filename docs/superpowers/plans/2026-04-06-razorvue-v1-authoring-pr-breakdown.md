# RazorVue v1 Authoring PR Breakdown

> Status: active plan
> Positioning: Execution-level PR slicing companion for the current RazorVue v1 authoring roadmap.

**Position:** Execution-level companion to the v1 authoring roadmap.

**Purpose:** Turn `PR1` through `PR7` into implementation-sized delivery units that can be executed directly against the current repository state.

Related documents:

- [RazorVue.Overview.md](../../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [RazorVue.Design.md](../../../src/Jazor.Compiler/doc/RazorVue.Design.md)
- [RazorVue.LibraryAuthoring.Design.md](../../../src/Jazor.Compiler/doc/RazorVue.LibraryAuthoring.Design.md)
- [RazorVue.Vuetify.FirstPackage.md](../../../src/Jazor.Compiler/doc/RazorVue.Vuetify.FirstPackage.md)
- [2026-04-06-razorvue-v1-authoring-roadmap.md](./2026-04-06-razorvue-v1-authoring-roadmap.md)

---

## 1. Execution Rules

Each PR in this lane should be reviewed against the same hard rules:

- keep RazorVue semantic ownership in `Jazor.RazorVue`
- keep `Jazor.RazorVue.Analysis` as Roslyn wiring plus diagnostic projection only
- keep the C# stub as the only authoring truth source
- keep library integration on the shared descriptor/registry/lowering path
- do not add Vuetify-specific lowering branches
- do not expand package breadth until the first authoring loop closes

Each PR should also satisfy all of the following:

- production scope is narrow enough to review in one pass
- tests prove the exact new contract surface
- the next PR can begin without refactoring the previous one

---

## 2. Sequence Overview

### PR1. Library metadata extraction

Output:
- a C# library stub can become a `LibraryComponent` descriptor

### PR2. Default library discovery

Output:
- referenced library stubs enter the default component registry automatically

### PR3. First Vuetify package

Output:
- `Jazor.RazorVue.Vuetify` exists as the first real authoring package

### PR4. Event and binding closure

Output:
- Blazor-shaped events and `@bind-*` close through library components

### PR5. Strong typed slot closure

Output:
- `RenderFragment<TContext>`-based scoped slots close through a real package example

### PR6. Host plugin requirement declaration

Output:
- host-facing artifacts and manifest can declare required Vue plugins

### PR7. Design-time diagnostics

Output:
- common authoring mistakes become explicit RazorVue diagnostics

---

## 3. PR1 Detail: Library Metadata Extraction

### Goal

Teach descriptor extraction to recognize library stubs without introducing a second metadata source.

### Production files

Add:

- `src/Jazor.RazorVue/VueLibraryComponent.cs`
- `src/Jazor.RazorVue/VueLibraryComponentAttribute.cs`
- `src/Jazor.RazorVue/VueLibraryStyleAttribute.cs`

Modify:

- `src/Jazor.RazorVue/RazorVue/RazorVueCompilationSymbols.cs`
- `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentDescriptorFactory.cs`

Tests:

- `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`

### Implementation tasks

1. Add `VueLibraryComponent : VueComponent` as the dedicated base type for library stubs.
2. Add `VueLibraryComponentAttribute(importSpecifier, exportName)` as the runtime import identity source.
3. Add `VueLibraryStyleAttribute(styleSpecifier)` as the style dependency declaration surface.
4. Extend `RazorVueCompilationSymbols` so descriptor extraction can resolve the new base type and attributes.
5. Update `VueComponentDescriptorFactory` to detect library stubs and emit:
   - `SourceKind = LibraryComponent`
   - `ImportSpecifier` from attribute
   - `ExportName` from attribute
   - `StyleDependencies` from style attributes
6. Keep prop/emit/slot extraction shared with normal components.
7. Split import normalization rules:
   - user components continue to normalize relative module paths
   - library import specifiers stay exactly as declared

### Acceptance gate

PR1 is complete only when all of the following are true:

- a library stub produces a `LibraryComponent` descriptor
- `vuetify/components` remains `vuetify/components`, not `vuetify/components.mjs`
- styles are carried in descriptor metadata
- `[Parameter]`, `EventCallback`, and `RenderFragment` extraction still use the shared path

### Non-goals

- no automatic discovery yet
- no new package project yet
- no manifest or plugin requirement changes yet

---

## 4. PR2 Detail: Default Library Discovery

### Goal

Make library stubs available through the standard registry/resolution path without manual registry injection.

### Production files

Modify:

- `src/Jazor.RazorVue/RazorVue/RazorVueCompilationContext.cs`
- `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentRegistry.cs`
- `src/Jazor.RazorVue/RazorVue/RazorVuePipeline.cs`

Tests:

- `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
- `src/Jazor.CompilerTest/RazorVueComponentRegistryTests.cs`
- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

### Implementation tasks

1. Add compilation-wide discovery for types deriving from `VueLibraryComponent`.
2. Reuse `VueComponentDescriptorFactory` for those discovered symbols.
3. Ensure discovered library descriptors enter the default registry created by `RazorVueCompilationContext`.
4. Keep user components and library components on different output lanes:
   - user components still become semantic snapshots and artifacts
   - library stubs should participate in resolution only
5. Confirm short-name, namespace, and fully-qualified resolution stay shared across user and library components.
6. Confirm intrinsic name reservation still wins over visible user/library collisions.

### Acceptance gate

PR2 is complete only when all of the following are true:

- referenced library stubs resolve from `using` scope without manual registry injection
- library stubs do not become generated Vue artifacts
- user component resolution behavior is unchanged
- lowering can resolve a library component through the default context path

### Non-goals

- no dedicated ecosystem package yet
- no package-level plugin requirement yet
- no diagnostics expansion yet

---

## 5. PR3 Detail: First Vuetify Package

### Goal

Introduce the first real authoring package and prove that external Vue UI components can be consumed through a C#-first surface.

### Production files

Add:

- `src/Jazor.RazorVue.Vuetify/Jazor.RazorVue.Vuetify.csproj`
- `src/Jazor.RazorVue.Vuetify/VBtn.cs`
- `src/Jazor.RazorVue.Vuetify/VTextField.cs`
- `src/Jazor.RazorVue.Vuetify/VCard.cs`
- `src/Jazor.RazorVue.Vuetify/VIcon.cs`

Modify:

- `Jazor.slnx`

Tests:

- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- `src/Jazor.CompilerTest/RazorVueComponentRegistryTests.cs`
- optionally `src/Jazor.CompilerTest/RazorVueLibraryIntegrationTests.cs` if package-consumer coverage becomes too large for existing files

### Implementation tasks

1. Create a dedicated authoring package project, not a runtime wrapper project.
2. Place package types under `ECMAScript.UI.Vue.Vuetify`.
3. Model each first-wave component as a thin stub:
   - inherits `VueLibraryComponent`
   - declares `VueLibraryComponentAttribute("vuetify/components", "...")`
   - declares `VueLibraryStyleAttribute("vuetify/styles")`
   - exposes a small, deliberate `[Parameter]` surface only
4. Start with low-risk components:
   - `VBtn`
   - `VTextField`
   - `VCard`
   - `VIcon`
5. Keep runtime behavior out of the stubs.
6. Add consumer-side tests that prove a RazorVue component can reference these stubs and lower to the expected import/style metadata.

### Acceptance gate

PR3 is complete only when all of the following are true:

- a consuming project can import `ECMAScript.UI.Vue.Vuetify`
- `VBtn`, `VTextField`, `VCard`, and `VIcon` resolve as library components
- generated artifacts reference `vuetify/components`
- generated artifacts carry `vuetify/styles`

### Non-goals

- no `VDialog` yet
- no plugin requirement metadata yet
- no attempt at broad Vuetify surface coverage

---

## 6. PR4 Detail: Event And Binding Closure

### Goal

Close the first authoring loop for library events and model binding without forking the lowering pipeline.

### Production files

Likely modify:

- `src/Jazor.RazorVue.Vuetify/VBtn.cs`
- `src/Jazor.RazorVue.Vuetify/VTextField.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentDescriptorFactory.cs`

Tests:

- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`

### Implementation tasks

1. Add first-wave event surfaces to Vuetify stubs:
   - `VBtn.OnClick`
   - `VTextField.ModelValue`
   - `VTextField.ModelValueChanged`
2. Verify descriptor extraction marks the model pair correctly.
3. Verify component attribute lowering maps:
   - `OnClick` to the expected Vue event listener name
   - `ModelValue` and `ModelValueChanged` to Vue model-style prop and update event names
4. Fix only generic lowering gaps if the current pipeline is insufficient.
5. Add proof cases for:
   - explicit event callback wiring
   - explicit `ModelValue` + `ModelValueChanged`
   - Blazor-shaped `@bind-*` flow if the generated Razor lane already surfaces it through the current extractor

### Acceptance gate

PR4 is complete only when all of the following are true:

- a consuming component can wire `VBtn.OnClick`
- a consuming component can wire `VTextField.ModelValue + ModelValueChanged`
- no Vuetify-specific branch is added to the lowering path
- the authoring surface still reads like normal C# component authoring

### Non-goals

- no scoped slot closure yet
- no new diagnostics IDs yet

### Mandatory review gate

Stop after PR4 and review the model against all of the following:

- the C# stub is still the single truth source
- the lowering path is still generic
- library semantics have not forked from user-component semantics
- the resulting API still feels native to a Blazor-style author

If any of these checks fail, repair the model before PR5.

---

## 7. PR5 Detail: Strong Typed Slot Closure

### Goal

Prove that scoped slots can be expressed as strong typed Razor fragments through a real component example.

### Production files

Add:

- `src/Jazor.RazorVue.Vuetify/VDialog.cs`
- `src/Jazor.RazorVue.Vuetify/VDialogActivatorContext.cs`

Likely modify:

- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- `src/Jazor.RazorVue/RazorVue/RenderTree/RazorVueRenderTreeExtractor.cs`

Tests:

- `src/Jazor.CompilerTest/RazorVueDescriptorExtractionTests.cs`
- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`

### Implementation tasks

1. Add `VDialog` as the first scoped-slot package example.
2. Model the activator slot as `RenderFragment<VDialogActivatorContext>`.
3. Introduce the minimum context type needed for authoring:
   - keep it small
   - keep it C#-friendly
   - avoid mirroring the full Vue runtime payload if a narrower authoring contract is sufficient
4. Verify descriptor extraction carries slot parameter type information.
5. Verify lowering still treats the slot generically as `RenderFragment<TContext>`.
6. Add both positive and negative tests:
   - slot lambda receives the typed context
   - non-callable slot values still do not produce broken JavaScript

### Acceptance gate

PR5 is complete only when all of the following are true:

- `VDialog.Activator` is described as a scoped slot
- the slot context has a named C# type
- consuming code can use the scoped slot without dropping to raw Vue payload handling
- slot lowering remains package-agnostic

### Non-goals

- no attempt to model all Vuetify slot contexts
- no theme, icon-set, or router integration

---

## 8. PR6 Detail: Host Plugin Requirement Declaration

### Goal

Make host-facing runtime requirements explicit so package consumption does not rely on hidden conventions.

### Production files

Likely add:

- `src/Jazor.RazorVue/VueLibraryPluginAttribute.cs`

Likely modify:

- `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentDescriptor.cs`
- `src/Jazor.RazorVue/RazorVue/Descriptor/VueComponentDescriptorFactory.cs`
- `src/Jazor.RazorVue/RazorVue/Artifacts/VueCompiledArtifact.cs`
- `src/Jazor.RazorVue/RazorVue/Artifacts/RazorVueCatalog.cs`
- `src/Jazor.RazorVue/RazorVue/Artifacts/RazorVueCatalogBuilder.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`
- `src/Jazor.Emit/RazorVueCatalogReader.cs`
- `src/Jazor.Emit/RazorVueManifestModel.cs`
- `src/Jazor.Emit/RazorVueModuleWriter.cs`

Tests:

- `src/Jazor.CompilerTest/RazorVueArtifactCatalogTests.cs`
- `src/Jazor.EmitTest/RazorVueCatalogReaderTests.cs`
- `src/Jazor.EmitTest/RazorVueEmitIntegrationTests.cs`

### Implementation tasks

1. Add a compiler-owned plugin requirement metadata shape.
2. Let library stubs declare plugin requirements explicitly.
3. Carry plugin requirements through:
   - descriptor
   - compiled artifact
   - generated catalog
   - catalog reader
   - manifest model
4. Keep the host contract declarative:
   - compiler declares requirements
   - host decides how to install and bundle them
5. Add the first Vuetify package-level requirement.

### Acceptance gate

PR6 is complete only when all of the following are true:

- the compiler can declare that Vuetify installation is required
- generated catalog data preserves that requirement
- manifest materialization preserves that requirement
- host-facing metadata remains explicit and compiler-owned

### Non-goals

- no actual host plugin installer logic
- no package manager integration

---

## 9. PR7 Detail: Design-Time Diagnostics

### Goal

Replace broad failures with author-facing diagnostics for the most common library authoring mistakes.

### Production files

Likely modify:

- `src/Jazor.RazorVue/RazorVue/Descriptor/RazorVueCompilationIssue.cs`
- `src/Jazor.RazorVue/RazorVue/Descriptor/RazorVueResolutionIssueFactory.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueExpressionEmitter.cs`
- `src/Jazor.RazorVue/RazorVue/Lowering/RazorVueArtifactFactory.cs`
- `src/Jazor.RazorVue.Analysis/RazorVueGenerator.cs`

Tests:

- `src/Jazor.CompilerTest/RazorVuePipelineTests.cs`
- `src/Jazor.CompilerTest/RazorVueAnalyzerTests.cs`
- add `src/Jazor.CompilerTest/RazorVueDiagnosticProjectionTests.cs` if the new matrix becomes too large for existing files

### Implementation tasks

1. Extend the compiler issue model beyond resolution and lifecycle failures.
2. Add explicit issue types for:
   - unknown parameter
   - invalid bind target
   - unknown slot
   - slot context misuse
3. Project them through `Jazor.RazorVue.Analysis` as dedicated diagnostics.
4. Prefer Razor/C# phrasing over raw Vue terminology.
5. Keep these diagnostics precise enough that the common failure no longer collapses into `JAZORVGA001`.

### Recommended diagnostic reservation

- `JAZORVGA006` unknown parameter
- `JAZORVGA007` invalid bind target
- `JAZORVGA008` unknown slot
- `JAZORVGA009` slot context misuse

### Acceptance gate

PR7 is complete only when all of the following are true:

- the main library authoring mistakes produce dedicated diagnostics
- error locations point to the Razor/C# authoring site
- the message phrasing stays C#-oriented

### Non-goals

- no attempt to cover every unsupported lowering shape
- no full Volar-like diagnostic matrix

---

## 10. Suggested Verification Order

After each PR:

1. run the narrow test file(s) introduced or expanded by that PR
2. run the broader `RazorVue` compiler test slice
3. for PR6, also run the `Jazor.EmitTest` RazorVue slice

Recommended broad verification targets:

- `src/Jazor.CompilerTest/Jazor.CompilerTest.csproj`
- `src/Jazor.EmitTest/Jazor.EmitTest.csproj`

---

## 11. Delivery Discipline

Do not start the next PR when any of the following are true:

- the current PR still relies on hidden conventions
- the current PR added package-specific branches to core lowering
- the new tests pass only because the test shape avoids the real contract
- the current PR widened the public API without a closed authoring scenario

The desired progression is:

- metadata
- discovery
- first package
- event/bind closure
- slot closure
- host requirement closure
- diagnostics closure

That order keeps the semantic model ahead of package breadth.
