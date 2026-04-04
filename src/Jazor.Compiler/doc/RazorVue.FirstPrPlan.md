# RazorVue First PR Plan

> Status: Historical planning artifact.
> Positioning: Archived first-PR planning slice for the early RazorVue rollout.
> Note: Keep this document as planning context and sequencing history; use newer implementation status documents for current progress.

This document turns the RazorVue implementation skeleton into a first delivery plan.

It is intentionally narrow.
It focuses on the first implementation lane that should land without destabilizing the existing static-module path.

Related documents:

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationSkeleton.md](./RazorVue.ImplementationSkeleton.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

## 1. Delivery Goal

The first PR series should prove only this:

1. the repository can recognize RazorVue entries
2. RazorVue diagnostics do not break the current static-module path
3. the implementation has a stable place to grow from

It should not attempt render-tree extraction or Vue code generation yet.

## 2. PR Strategy

The first delivery lane should be split into three small PRs.

### PR1. Base types and analyzer mode split

Goal:

- introduce the minimum type system and diagnostics shell
- split static-module analysis from RazorVue analysis

### PR2. Entry classification and descriptor shell

Goal:

- prove RazorVue components can be discovered and classified
- introduce descriptor/snapshot placeholders without lowering

### PR3. Emit transition shell

Goal:

- add host-facing carrier placeholders so later Vue artifacts have a real destination

## 3. PR1 Scope

PR1 should include only the following production files.

### 3.1 `src/Jazor.Razor`

- `JazorComponent.cs`

### 3.2 `src/Jazor.RazorVue`

- `VueComponent.cs`
 
### 3.3 `src/Jazor.Compiler`
- `RazorVue/RazorVueCompilationSymbols.cs`
- `RazorVue/RazorVueEntryKind.cs`

### 3.4 `src/Jazor.Analyzer`

- `RazorVue/RazorVueDiagnosticDescriptors.cs`
- `RazorVue/RazorVueKnownSymbols.cs`
- `RazorVue/RazorVueEntryAnalyzer.cs`
- `RazorVue/RazorVueMisuseAnalyzer.cs`

### 3.5 Existing files expected to change

- `src/Jazor.Razor/Jazor.Razor.csproj`
- `src/Jazor.RazorVue/Jazor.RazorVue.csproj`
- `src/Jazor.RazorVue.Analysis/Jazor.RazorVue.Analysis.csproj`
- `src/Jazor.Analyzer/Analyzer.cs`
- `src/Jazor.Analyzer/AnalyzerReleases.Unshipped.md`
- optionally `Jazor.slnx`

## 4. PR1 Non-goals

PR1 must not include:

- `BuildRenderTree` extraction
- `VueComponentDescriptorFactory`
- lowering models
- `DenoHost` integration changes
- `.razor` fixture-heavy end-to-end tests

If PR1 starts pulling those in, it is too large.

## 5. PR1 Concrete Tasks

### 5.1 Add base component types

Create:

- `JazorComponent : ComponentBase`
- `VueComponent : JazorComponent`

Rules:

- `JazorComponent` lives in `src/Jazor.Razor`
- `VueComponent` lives in `src/Jazor.RazorVue`
- `JazorComponent` stays thin
- `VueComponent` may be almost empty in PR1
- phase-one helper APIs do not need full implementation yet

### 5.2 Split analyzer mode

Refactor `Analyzer.cs` so the existing ECMAScript analyzer path does not automatically own RazorVue classes.

Required behavior:

- `[ECMAScriptModule] static class` continues through the legacy rule path
- `[ECMAScriptModule]` + `JazorComponent` descendant goes through RazorVue rule path
- direct `ComponentBase` entry produces a RazorVue diagnostic

### 5.3 Enable generated code analysis for RazorVue-specific checks

PR1 does not need full generated Razor extraction,
but the analyzer surface must stop structurally blocking that future path.

Required outcome:

- generated code analysis is enabled where RazorVue requires it
- the implementation is structured so future generated-code-based discovery does not require another analyzer rewrite

### 5.4 Add first diagnostic set

PR1 should land only these IDs:

- `JAZORVUE001` invalid RazorVue entry inheritance
- `JAZORVUE002` direct `ComponentBase` entry is not allowed
- `JAZORVUE004` `StateHasChanged` is not part of RazorVue semantics
- `JAZORVUE005` `ShouldRender` is not part of RazorVue semantics
- `JAZORVUE006` `SetParametersAsync` is not part of RazorVue semantics

Do not add ambiguity or descriptor diagnostics in PR1.

## 6. PR1 Test Plan

PR1 should add one new test file:

- `src/Jazor.CompilerTest/RazorVueAnalyzerTests.cs`

Recommended first tests:

- `RazorVue_Entry_ValidVueComponent_IsAccepted`
- `RazorVue_Entry_ComponentBaseOnly_ReportsJAZORVUE002`
- `RazorVue_Entry_StaticModule_RemainsOnLegacyPath`
- `RazorVue_Misuse_StateHasChanged_ReportsJAZORVUE004`
- `RazorVue_Misuse_ShouldRender_ReportsJAZORVUE005`
- `RazorVue_Misuse_SetParametersAsync_ReportsJAZORVUE006`

Recommended fixture style:

- keep them inline-C# compilation tests first
- do not introduce `.razor` files into PR1 tests unless absolutely needed

## 7. PR1 Acceptance Gate

PR1 is complete only when all of the following are true:

1. `JazorComponent` and `VueComponent` compile
2. existing static-module analyzer behavior still works
3. RazorVue entry diagnostics are reported through dedicated IDs
4. misuse diagnostics use dedicated RazorVue IDs
5. `RazorVueAnalyzerTests.cs` covers the entry split and misuse shell

## 8. PR2 Scope

PR2 can start only after PR1 is green.

Production files to introduce:

- `src/Jazor.RazorVue.Analysis/RazorVue/RazorVueCompilationContext.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/RazorVueComponentCandidate.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VueComponentDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VuePropDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VueEmitDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Descriptor/VueSlotDescriptor.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueSemanticSnapshot.cs`

PR2 should still stop before render-tree extraction.

PR2 test files:

- `RazorVueDescriptorExtractionTests.cs`

PR2 proof:

- descriptor shell exists
- entry candidates can become compiler-owned snapshots

## 9. PR3 Scope

PR3 should introduce only the host transition shell, not final lowering.

Production files to introduce:

- `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/VueCompiledArtifact.cs`
- `src/Jazor.RazorVue.Analysis/RazorVue/Artifacts/RazorVueCatalog.cs`
- `src/Jazor.Emit/RazorVueCatalogReader.cs`
- `src/Jazor.Emit/RazorVueManifestModel.cs`

PR3 proof:

- the new carrier has a home in `Jazor.Emit`
- transition with the current `ModuleCatalog` is explicit

## 10. Suggested Command-Level Verification

PR1 verification target:

```powershell
$env:DOTNET_CLI_HOME='D:\repository\own\jazor\Jazor\.dotnet'; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; dotnet test 'src/Jazor.CompilerTest/Jazor.CompilerTest.csproj' --filter 'FullyQualifiedName~RazorVueAnalyzerTests' -v minimal
```

PR2 verification target:

```powershell
$env:DOTNET_CLI_HOME='D:\repository\own\jazor\Jazor\.dotnet'; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; dotnet test 'src/Jazor.CompilerTest/Jazor.CompilerTest.csproj' --filter 'FullyQualifiedName~RazorVueAnalyzerTests|FullyQualifiedName~RazorVueDescriptorExtractionTests' -v minimal
```

PR3 verification target:

```powershell
$env:DOTNET_CLI_HOME='D:\repository\own\jazor\Jazor\.dotnet'; $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; dotnet test 'src/Jazor.CompilerTest/Jazor.CompilerTest.csproj' --filter 'FullyQualifiedName~RazorVue' -v minimal
```

## 11. Review Checklist for the First PR Series

Every PR in the first series should be reviewed against these questions:

1. does it preserve the static-module path
2. does it keep RazorVue types on a separate implementation lane
3. does it avoid prematurely touching render-tree/lowering work
4. does it add only the diagnostics needed for that PR
5. does it leave a cleaner next landing point than before

## 12. Conclusion

The first PR series should prove that RazorVue can enter the repository safely.

It should not try to prove the whole compiler.
The right first win is:

- entry split
- dedicated diagnostics
- explicit type skeleton
- explicit host transition lane
