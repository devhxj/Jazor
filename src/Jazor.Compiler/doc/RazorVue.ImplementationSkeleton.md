# RazorVue Implementation Skeleton

> Status: Active phase-one implementation skeleton.
> Positioning: Repository-level skeleton for RazorVue phase-one implementation.
> Note: This is a phase artifact for establishing ownership, types, and landing order, not a claim that the full pipeline is already implemented.

This document maps the RazorVue design into concrete repository-level implementation slices.

It does not redefine the architecture.
It exists to answer four practical questions before coding starts:

1. which project owns each stage
2. which files and types should exist first
3. which diagnostics should be introduced first
4. which tests should be written first

Related documents:

- [RazorVue.DecisionSummary.md](./RazorVue.DecisionSummary.md)
- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)
- [RazorVue.DenoHostContract.md](./RazorVue.DenoHostContract.md)
- [RazorVue.HardRules.md](./RazorVue.HardRules.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)

## 1. Project Ownership

Phase one should split ownership across existing projects as follows.

### 1.1 `src/Jazor.Analyzer`

Owns:

- RazorVue entry discovery diagnostics
- RazorVue misuse diagnostics
- generated-code analysis enablement
- compilation symbol caching for RazorVue-relevant types

Does not own:

- render-tree extraction
- Vue lowering
- host-facing catalog/materialization

### 1.2 `src/Jazor.Compiler`

Owns:

- neutral RazorVue symbol/context models
- semantic snapshot extraction
- component descriptor extraction
- logic extraction
- render-tree extraction
- Vue lowering
- RazorVue catalog/materialization models

Does not own:

- `ComponentBase`-derived authoring base classes
- final host dependency resolution
- bundling

### 1.3 `src/Jazor.RazorVue`

Owns:

- `VueComponent`
- Vue-first authoring surface
- net10-only dependency boundary for Vue-facing entry types

Does not own:

- `JazorComponent`
- static-module lowering
- generic compiler infrastructure already owned by `src/Jazor.Compiler`

### 1.4 `src/Jazor.Razor`

Owns:

- `JazorComponent`
- Razor/AspNetCore-facing substrate for frontend component entry
- net10-only dependency boundary for Razor entry types

Does not own:

- Vue-first authoring APIs
- static-module lowering

### 1.5 `src/Jazor.RazorVue.Analysis`

Owns:

- RazorVue generator/analyzer-facing entry
- Razor-specific generated-code discovery for the RazorVue route
- wiring from `[ECMAScriptModule]` Razor components into compiler-owned lowering/artifact models

Does not own:

- `JazorComponent`
- `VueComponent`
- generic compiler-core contracts already owned by `src/Jazor.Compiler`

### 1.6 `src/Jazor.Emit`

Owns:

- catalog reading for emitted compiler-owned carriers
- manifest persistence format updates
- transition support for `ModuleCatalog` plus `RazorVueCatalog`
- `DenoHost` handoff

### 1.7 `src/Jazor.CompilerTest`

Owns:

- discovery/diagnostic tests
- extraction tests
- lowering tests
- artifact/catalog tests

Phase one should keep most verification here instead of creating a new test project.

## 2. Recommended Directory Shape

Phase one should avoid scattering RazorVue types across unrelated files.

Recommended compiler-side layout:

```text
src/Jazor.Compiler/
  RazorVue/
    RazorVueCompilationSymbols.cs
    RazorVueCompilationContext.cs
    RazorVueComponentCandidate.cs
    RazorVuePipeline.cs
    Discovery/
      RazorVueEntryClassifier.cs
    Descriptor/
      VueComponentDescriptorFactory.cs
      VueComponentRegistryBuilder.cs
    Logic/
      RazorVueLogicExtractor.cs
    RenderTree/
      RazorRenderTreeExtractor.cs
      RazorRenderTreeBuilderPatterns.cs
    Lowering/
      RazorVueLoweringContext.cs
      VueComponentLowerer.cs
      VueRenderFunctionEmitter.cs
    Artifacts/
      RazorVueSemanticSnapshot.cs
      VueCompiledArtifact.cs
      RazorVueCatalog.cs
      RazorVueManifest.cs
      RazorVueSourceOrigin.cs

src/Jazor.RazorVue/
  VueComponent.cs

src/Jazor.Razor/
  JazorComponent.cs

src/Jazor.RazorVue.Analysis/
  RazorVueGenerator.cs
```

Recommended analyzer-side layout:

```text
src/Jazor.Analyzer/
  RazorVue/
    RazorVueKnownSymbols.cs
    RazorVueEntryAnalyzer.cs
    RazorVueMisuseAnalyzer.cs
    RazorVueDiagnosticDescriptors.cs
```

Recommended emit-side layout:

```text
src/Jazor.Emit/
  RazorVueCatalogReader.cs
  RazorVueManifestModel.cs
```

Phase one does not require every file above on day one.
It does require the ownership boundaries they represent.

## 3. First Concrete Types

The following types are the minimum implementation skeleton worth defining first.

### 3.1 Compiler context

```csharp
public sealed record RazorVueCompilationSymbols(
    INamedTypeSymbol ECMAScriptModuleAttribute,
    INamedTypeSymbol JazorComponent,
    INamedTypeSymbol VueComponent,
    INamedTypeSymbol ComponentBase,
    INamedTypeSymbol ParameterAttribute);
```

```csharp
public sealed record RazorVueCompilationContext(
    Compilation Compilation,
    RazorVueCompilationSymbols Symbols);
```

### 3.2 Entry classification

```csharp
public enum RazorVueEntryKind
{
    None,
    StaticModule,
    RazorVueComponent,
    Invalid
}
```

```csharp
public sealed record RazorVueComponentCandidate(
    INamedTypeSymbol ComponentSymbol,
    IMethodSymbol? BuildRenderTreeMethod,
    RazorVueEntryKind EntryKind);
```

### 3.3 Snapshot and artifact types

```csharp
public sealed record RazorVueSemanticSnapshot(
    INamedTypeSymbol ComponentSymbol,
    VueComponentDescriptor Descriptor,
    RazorVueLogicModel Logic,
    RazorRenderTreeNode RenderTree,
    ImmutableArray<RazorVueSourceOrigin> Origins);
```

```csharp
public sealed record VueCompiledArtifact(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    VueArtifactIdentity Identity,
    VueRuntimeHints Hints);
```

```csharp
public sealed record RazorVueCatalog(
    string AssemblyName,
    ImmutableArray<VueCompiledArtifact> Artifacts);
```

## 4. First Compiler Entry Surface

Phase one should not start by modifying the current `AstConverter` or `SemanticWalker`.

Recommended first compiler entry type:

```csharp
public sealed class RazorVuePipeline
{
    public RazorVueCatalog Execute(RazorVueCompilationContext context);
}
```

Recommended first internal call order:

1. `RazorVueEntryClassifier`
2. `VueComponentDescriptorFactory`
3. `RazorVueLogicExtractor`
4. `RazorRenderTreeExtractor`
5. `VueComponentLowerer`
6. `VueRenderFunctionEmitter`
7. catalog/materialization

This keeps the new path separate from the current static-module path.

## 5. Analyzer Breakdown

The current analyzer is one large rule surface.
Phase one RazorVue should not add more unrelated logic into that same shape.

Recommended split:

### 5.1 `RazorVueKnownSymbols`

Responsibility:

- cache `ECMAScriptModuleAttribute`
- cache `JazorComponent`
- cache `VueComponent`
- cache `ComponentBase`
- cache common Razor/Blazor symbols used by diagnostics

### 5.2 `RazorVueEntryAnalyzer`

Responsibility:

- identify `[ECMAScriptModule]` Razor component candidates
- validate `JazorComponent` inheritance
- distinguish static-module entry from RazorVue entry

### 5.3 `RazorVueMisuseAnalyzer`

Responsibility:

- diagnose `StateHasChanged`
- diagnose `ShouldRender`
- diagnose `SetParametersAsync`
- diagnose ambiguity-prone invalid patterns that should fail before lowering

## 6. Diagnostic Plan

Phase one should reserve a dedicated diagnostic range instead of reusing the existing generic analyzer error.

Recommended IDs:

- `JAZORVUE001` invalid RazorVue entry inheritance
- `JAZORVUE002` direct `ComponentBase` entry is not allowed
- `JAZORVUE003` generated-code analysis unavailable for RazorVue extraction
- `JAZORVUE004` `StateHasChanged` is not part of RazorVue semantics
- `JAZORVUE005` `ShouldRender` is not part of RazorVue semantics
- `JAZORVUE006` `SetParametersAsync` is not part of RazorVue semantics
- `JAZORVUE007` ambiguous component name
- `JAZORVUE008` intrinsic component name collision
- `JAZORVUE009` unknown component prop
- `JAZORVUE010` unsupported phase-one RazorVue syntax shape

Phase one should keep the initial set small.
Do not design 30 diagnostics before the first loop closes.

## 7. First Test Files

Recommended first test files in `src/Jazor.CompilerTest/`:

- `RazorVueAnalyzerTests.cs`
- `RazorVueDescriptorExtractionTests.cs`
- `RazorVueLogicExtractionTests.cs`
- `RazorVueRenderTreeExtractionTests.cs`
- `RazorVueLoweringTests.cs`
- `RazorVueArtifactEmissionTests.cs`
- `RazorVueHostIntegrationTests.cs`

Recommended first test names:

- `RazorVue_Entry_ValidVueComponent_IsDiscovered`
- `RazorVue_Entry_ComponentBaseOnly_ReportsDiagnostic`
- `RazorVue_Entry_StaticModule_RemainsOnLegacyPath`
- `RazorVue_Descriptor_ParameterProperty_BecomesProp`
- `RazorVue_Descriptor_EventCallback_BecomesEmit`
- `RazorVue_Descriptor_ChildContent_BecomesDefaultSlot`
- `RazorVue_Resolution_AmbiguousShortName_ReportsDiagnostic`
- `RazorVue_Resolution_FullyQualifiedComponent_ResolvesSuccessfully`
- `RazorVue_RenderTree_OpenElementAddContent_ProducesElementNode`
- `RazorVue_RenderTree_IfBlock_ProducesConditionalNode`
- `RazorVue_RenderTree_Foreach_ProducesLoopNode`
- `RazorVue_Lowering_HtmlElement_LowersToHCall`
- `RazorVue_Lowering_ComponentNode_LowersToComponentHCall`
- `RazorVue_Artifact_IdentityHashes_AreSplit`
- `RazorVue_Host_Manifest_CanBeMaterialized`

## 8. First PR Sequence

Phase one should not land as one giant PR.

Recommended sequence:

### PR1. Base and diagnostics shell

Includes:

- `JazorComponent`
- `VueComponent`
- analyzer mode split
- RazorVue diagnostics shell

### PR2. Descriptor extraction

Includes:

- descriptor models
- descriptor extraction
- descriptor tests

### PR3. Render-tree extraction

Includes:

- minimal render-tree model
- builder-pattern extractor
- render-tree tests

### PR4. Lowering and artifact emission

Includes:

- snapshot/artifact models
- lowering
- artifact tests

### PR5. Host transition

Includes:

- `RazorVueCatalog` consumption
- manifest evolution
- `DenoHost` handoff

## 9. Explicit Non-goals for the Skeleton

This skeleton should not start with:

- Vuetify-specific lowering
- Router/Pinia integration
- `.vue` SFC generation
- SSR/hydration planning
- alias-qualified component tag syntax
- broad Razor syntax support

## 10. Implementation Start Gate

Real implementation should begin only when all of the following are true:

1. the team accepts the proposed project/file ownership
2. the team accepts the first diagnostic ID range
3. the team accepts fully-qualified component names as the only phase-one ambiguity escape
4. the team accepts `RazorSourceMap -> GeneratedSyntaxLocation -> GeneratedFallback` provenance tiers
5. the team accepts a transition plan where RazorVue coexists with the current `ModuleCatalog` path

## 11. Conclusion

The goal of this document is not to predict every future file.

It is to ensure phase one starts with:

- concrete ownership
- stable stage boundaries
- explicit diagnostics
- explicit tests
- an implementation order that keeps the current compiler path working
