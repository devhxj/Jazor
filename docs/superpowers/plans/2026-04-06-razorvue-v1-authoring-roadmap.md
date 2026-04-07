# RazorVue v1 Authoring Roadmap

> Status: active plan
> Positioning: Execution-level roadmap for the current RazorVue authoring expansion lane.

**Goal:** Deliver RazorVue v1 as a C#-first authoring product with a Vue-first runtime target and a first validated ecosystem package through Vuetify.

**Architecture:** Keep the existing RazorVue main path intact. Add library component authoring as a thin layer on top of the current descriptor / registry / lowering flow. Use C# stubs as the authoring truth source, derive descriptors from the stubs, and keep host-facing plugin requirements explicit.

**Tech Stack:** C# 14, .NET 10, Razor, Roslyn, MSTest, Vue ESM artefacts

Related documents:

- [RazorVue.Overview.md](../../../src/Jazor.Compiler/doc/RazorVue.Overview.md)
- [RazorVue.Design.md](../../../src/Jazor.Compiler/doc/RazorVue.Design.md)
- [RazorVue.ComponentDescriptorSpec.md](../../../src/Jazor.Compiler/doc/RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](../../../src/Jazor.Compiler/doc/RazorVue.DenoHostContract.md)
- [RazorVue.ImplementationChecklist.md](../../../src/Jazor.Compiler/doc/RazorVue.ImplementationChecklist.md)
- [2026-04-06-razorvue-v1-authoring-pr-breakdown.md](./2026-04-06-razorvue-v1-authoring-pr-breakdown.md)
- [RazorVue 阶段评估（2026-04-06）](../../status/2026-04-06-razorvue-stage-assessment.md)

Historical context:

- [RazorVue.FirstPrPlan.md](../../../src/Jazor.Compiler/doc/RazorVue.FirstPrPlan.md)

---

## 1. Constraints

- Keep RazorVue core in `Jazor.RazorVue`
- Keep `Jazor.RazorVue.Analysis` thin
- Do not introduce a separate descriptor truth source
- Do not add library-specific lowering branches
- Do not chase full Vue SFC or Volar parity
- Prefer small PRs with clear verification

## 2. Delivery Plan

### PR1. Library metadata extraction

Goal:
- make real C# stubs produce `LibraryComponent` descriptors

Deliver:
- `VueLibraryComponent`
- `VueLibraryComponentAttribute`
- `VueLibraryStyleAttribute`
- descriptor extraction changes

Acceptance:
- a stub can supply import specifier, export name, and style dependencies
- props, emits, and slots still use the existing shared extraction logic

### PR2. Default library discovery

Goal:
- make library stubs visible in the default component registry

Deliver:
- library component discovery from `Compilation`
- default registry merge for intrinsic, user, and library components

Acceptance:
- referenced library stubs resolve without manual registry injection

### PR3. First Vuetify package

Goal:
- publish the first real authoring package

Deliver:
- `Jazor.RazorVue.Vuetify`
- `VBtn`
- `VTextField`
- `VCard`
- `VIcon`

Acceptance:
- Razor authors can import the package and use the first-wave components
- generated artefacts lower to `vuetify/components` and `vuetify/styles`

### PR4. Event and binding closure

Goal:
- prove the Blazor-shaped event and binding model works for library components

Deliver:
- `VBtn.OnClick`
- `VTextField.ModelValue + ModelValueChanged`

Acceptance:
- authoring stays C#-shaped
- lowering does not require Vuetify-specific special cases

### PR5. Strong typed slot closure

Goal:
- prove scoped slots can be authored as strong typed Razor fragments

Deliver:
- `VDialog`
- `VDialogActivatorContext`
- `RenderFragment<TContext>` slot mapping

Acceptance:
- `Activator` works as a scoped slot with a C# context type

### PR6. Host plugin requirement declaration

Goal:
- make Vuetify runtime requirements explicit in host-facing output

Deliver:
- plugin requirement model
- manifest support for plugin requirements
- Vuetify plugin declaration

Acceptance:
- the host can see that Vuetify installation is required

### PR7. Design-time diagnostics

Goal:
- improve authoring feedback for common mistakes

Deliver:
- unknown parameter diagnostics
- invalid bind target diagnostics
- unknown slot diagnostics
- slot context misuse diagnostics

Acceptance:
- the most common authoring errors are caught early with Razor / C#-oriented messages

## 3. Review Gate

Add a mandatory review gate after PR4.

Check:

- stub-as-truth-source is still holding
- lowering is still generic
- authoring still feels Blazor-like
- library integration has not forked the core semantic model

If any check fails, pause expansion and repair the model before continuing.

## 4. Completion Criteria

RazorVue v1 is complete when all of the following are true:

- library components are first-class in the core RazorVue model
- the Vuetify first package is usable from Razor
- event, bind, and slot authoring close through the shared pipeline
- host-facing plugin requirements are explicit
- common authoring errors produce useful diagnostics

## 5. Main Risks

- authoring truth drifting away from compiler truth
- library-specific lowering branches creeping into core
- exposing too much Vue runtime detail to business authors
- expanding package coverage before the first authoring loop closes
