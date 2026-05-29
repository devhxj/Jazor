# RazorVue Component Lowering 调整执行计划

> Status: Active
> Created: 2026-05-28
> Last updated: 2026-05-28
> Scope: 将当前 RazorVue 实现从“Razor IR 主前端”订正为“组件整体 lowering + Roslyn/`BuildRenderTree` 语义基线 + Razor IR SFC 增强”。

本文档是后续实现的任务队列和生产执行标准。后续 agent 必须先读取本文档，按 `Next Task Pointer` 选择任务，并在完成或阻塞时更新本文档。

相关文档：

- [src/Jazor.RazorVue/README.md](../../../../src/Jazor.RazorVue/README.md)
- [RazorVue.RazorIrMigrationPlan.md](./RazorVue.RazorIrMigrationPlan.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.RazorSg.MainlineIrInjection.DecisionRecord.md](./RazorVue.RazorSg.MainlineIrInjection.DecisionRecord.md)
- [RazorVue.RazorSg.TailInjection.Guidance.md](./RazorVue.RazorSg.TailInjection.Guidance.md)

## Next Task Pointer

Current next task: `RVCL-013` (blocked by current skip-platform-evidence decision; no unblocked automatic task remains)

Selection rule:

1. Pick the single task whose `Status` is `NEXT`.
2. If no task is marked `NEXT`, pick the first `TODO` task whose dependencies are all `DONE`, then mark it `NEXT`.
3. Before coding, update `Current next task` only if the previous `NEXT` is `DONE` or explicitly `BLOCKED`.
4. After each task, update:
   - task `Status`
   - `Last updated`
   - `Current next task`
   - `Progress Log`
   - any changed risk or follow-up task

Status values:

- `NEXT`: the only task a new implementation session should start.
- `TODO`: ready later, subject to dependencies.
- `IN_PROGRESS`: currently being implemented.
- `DONE`: accepted and verified.
- `BLOCKED`: cannot proceed without a documented external condition or user decision.

## Architecture Contract

The target architecture is:

```text
RazorVue component candidate
  -> merged INamedTypeSymbol / partial component class
  -> component semantic snapshot
       -> descriptor subset: props / emits / slots / model / style / plugin / container
       -> runtime subset: setup fields/properties/methods, lifecycle, helper methods/classes
       -> render subset: Roslyn-bound BuildRenderTree / IOperation
  -> canonical render model
  -> optional Razor IR SFC enhancement for .razor components
  -> SFC semantic model
  -> .vue / render-function artifact
```

Hard constraints:

- `BuildRenderTree` / Roslyn `IOperation` is the render semantic baseline for both `.razor` generated components and handwritten `.cs` components.
- Razor IR is an enhancement layer for `.razor` SFC fidelity, source mapping, directive intent, tag-helper metadata, mixed attributes, raw markup, and template shape recovery.
- `.razor` components are generated after official Razor SG and are triggered only by Razor SG tail output registered into the official generator pipeline.
- Pure handwritten `.cs` `BuildRenderTree` components do not pass through Razor SG tail; they are triggered by normal analyzer/source-generator output.
- Descriptor members must not be lowered twice as normal runtime code.
- RazorVue must not read `.razor` files from disk, privately run Razor SG in production, patch final `.vue` text, or hand-assemble JavaScript for C# semantics that belong to `Jazor.Compiler` / `SemanticWalker`.
- Missing official Razor SG tail output is a diagnostic condition, not a private fallback generation condition.

## Production Quality Bar

Every implementation task must satisfy all applicable requirements:

- Use Roslyn symbols and `IOperation` as semantic authority; do not infer C# behavior from text when Roslyn can provide it.
- Preserve evaluation order, side-effect count, source-origin quality, deterministic import/temp naming, descriptor identity, and HMR boundary semantics.
- Fail fast with actionable diagnostics for unsupported shapes; do not silently emit partial or fake artifacts.
- Add focused regression tests before or with behavior changes.
- Keep tasks small enough to finish and verify in one focused session.
- Do not rely on process-global mutable state, environment variables, shared temp directories, or shared ports in tests.
- Prefer existing helpers and pipeline types; introduce new abstractions only when they reduce real coupling.
- Update this document after each task and checkpoint.

## Implementation Phases

### Phase 0: Audit And Safety Rails

Goal: document current implementation drift, lock trigger behavior, and prevent further changes from deepening the old IR-primary route.

### Phase 1: Contract Split

Goal: make the code express three separate concepts: component semantic baseline, render baseline extraction, and optional IR enhancement.

### Phase 2: Pipeline Rewire

Goal: route `.razor` SFC generation through SG-after generated C# / Roslyn baseline first, then apply Razor IR enhancement.

### Phase 3: Robustness And Cleanup

Goal: harden diagnostics, tests, docs, and remove misleading naming or obsolete frontend assumptions.

## Task Queue

### RVCL-001: Audit Current Pipeline Against Target Contract

Status: DONE

Description:
Map current `Jazor.RazorVue` and `Jazor.Analyzer` code paths against the target architecture. The output is a short audit section appended to this document with concrete files/classes that currently encode the old IR-primary route, baseline-compatible code that can be reused, and uncertain areas requiring tests.

Acceptance criteria:

- [x] Identify current trigger routes for `.razor` SG tail output and handwritten `.cs` components.
- [ ] Identify current classes/methods that must be renamed, split, or rewired.
- [ ] Identify current tests that lock the old route and tests that can be adapted.

Verification:

- [ ] Run only read-only inspection commands.
- [ ] Update `Audit Notes` in this document.
- [ ] Promote `RVCL-002` to `NEXT` if no blocker remains.

Dependencies: None

Files likely touched:

- `docs/02-计划/jolt/razorvue-implementation/RazorVue.ComponentLoweringAdjustmentPlan.md`

Estimated scope: S

### RVCL-002: Lock Trigger Routing Tests

Status: DONE

Description:
Add focused tests proving route selection before the implementation rewire: handwritten `.cs` `BuildRenderTree` components are generated from the normal analyzer/source-generator path, while `.razor` components are generated from official Razor SG tail output after official generated C# is available.

Acceptance criteria:

- [x] Handwritten `.cs` component test proves no Razor SG document is required.
- [x] `.razor` component test proves the official SG tail document path is used.
- [x] Failure test proves missing generated render body is diagnostic, not silent artifact loss.
- [x] Private analyzer fallback behavior is rejected with diagnostics and cannot become the production source of Razor SG documents.

Verification:

- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVueGenerator"` (passes with no matching tests in this project)
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorSourceGenerator"`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueGeneratorRouteTests"`

Dependencies: `RVCL-001`

Files likely touched:

- `src/Jazor.RazorVue.Test/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: M

### RVCL-003: Define Component Semantic Baseline Contract

Status: DONE

Description:
Introduce or clarify internal contracts so the code has an explicit component semantic baseline distinct from Razor IR enhancement. This should avoid large rewrites while making the architecture visible in types and tests.

Acceptance criteria:

- [x] A named contract or documented internal flow represents merged component semantics.
- [x] Descriptor subset and runtime/render subsets are explicitly separated.
- [x] Existing lifecycle/setup/descriptor lowering continues to use the same semantic authority.

Verification:

- [x] `dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVue_Snapshot_ComponentBaseline|FullyQualifiedName~RazorVue_Snapshot_ContainsLifecycleAndLogicDescriptors|FullyQualifiedName~RazorVue_Candidate_ExtractsLifecycleAndLogicMethods" -v minimal`

Dependencies: `RVCL-002`

Files likely touched:

- `src/Jazor.RazorVue/Artifacts/*`
- `src/Jazor.RazorVue/Descriptor/*`
- `src/Jazor.RazorVue/Lowering/*`
- `src/Jazor.RazorVue.Test/*`

Estimated scope: M

Implementation notes:

- Added `RazorVueComponentSemanticBaseline` as the explicit internal component-level baseline contract.
- `RazorVueSemanticSnapshot.ComponentBaseline` now exposes descriptor, runtime, and render subsets without changing existing snapshot construction or pipeline behavior.
- Added a partial `.razor.cs` + generated `.razor.g.cs` regression proving descriptor props, runtime lifecycle/helper members, and render method all resolve through the same merged component symbol.

### RVCL-004: Split Render Baseline From IR Enhancement Interfaces

Status: DONE

Description:
Refactor the current frontend naming/dispatch so `BuildRenderTree` / Roslyn baseline extraction is not modeled as a fallback-only path, and Razor IR is not modeled as the primary semantic frontend. Keep the public behavior stable while making the internal route honest.

Acceptance criteria:

- [x] A baseline render extractor can run for both handwritten and SG-generated render bodies.
- [x] Razor IR enhancement is optional and layered after baseline model construction.
- [x] Existing `IRazorVueTemplateFrontend` usage is adapted, renamed, or wrapped without broad churn.

Verification:

- [x] `dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueRenderFrontendContractTests" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~LegacyIrFirstTemplateFrontend_FallsBackToBuildRenderTree_OnlyForHandwrittenBuildRenderTreeComponents|FullyQualifiedName~LegacyIrFirstTemplateFrontend_WithRazorGeneratedBuildRenderTreeButNoBoundRazorDocument_Throws|FullyQualifiedName~RazorVueRenderFrontendContractTests|FullyQualifiedName~RazorVueGeneratorRouteTests" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~CreateRenderTree_ForAddComponentParameterWrappedTypedSlotTemplateWithNestedComponentAndConditional_PreservesStructuredSubtree|FullyQualifiedName~RazorVue_Snapshot_ComponentBaseline" -v minimal`

Dependencies: `RVCL-003`

Files likely touched:

- `src/Jazor.RazorVue/Extensibility/*`
- `src/Jazor.RazorVue/RenderTree/*`
- `src/Jazor.RazorVue/RazorSdk/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: M

Implementation notes:

- Added `IRazorVueRenderBaselineExtractor` and `IRazorVueRenderEnhancement` beside the legacy `IRazorVueTemplateFrontend`.
- Added `RazorVueBaselineFirstTemplateFrontend` as the adapter from baseline-plus-enhancement contracts back to the current artifact factories.
- `BuildRenderTreeTemplateFrontend` now exposes the Roslyn/`BuildRenderTree` baseline role explicitly.
- `RazorVueRazorIrTemplateFrontend` now exposes an optional enhancement role while the legacy IR-first compatibility behavior remains isolated for old callers until cleanup can rename or remove it.
- Added `RazorVueRenderFrontendContractTests` proving the baseline extractor runs for handwritten and Razor-generated render bodies, and enhancement is invoked only after baseline extraction.

### RVCL-005: Rewire SFC Pipeline To Baseline-First For Razor Components

Status: DONE

Description:
Change `.razor` SFC generation so the pipeline builds render semantics from SG-generated `BuildRenderTree` / Roslyn operation first, then applies Razor IR enhancement where available. Do not regress handwritten `.cs` components.

Acceptance criteria:

- [x] `.razor` components use official generated render body as semantic baseline.
- [x] Handwritten `.cs` components keep normal analyzer/source-generator generation.
- [x] IR enhancement is applied only after baseline model creation and cannot replace compiler-owned C# expression semantics.

Verification:

- [x] `dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal`
- [x] `dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVuePipelineTests|FullyQualifiedName~RazorVueSfcArtifactFactoryTests" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueTemplateFrontendParityTests" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueRenderFrontendContractTests|FullyQualifiedName~RazorVueGeneratorRouteTests" -v minimal`

Dependencies: `RVCL-004`

Files likely touched:

- `src/Jazor.RazorVue/RazorVueSfcPipeline.cs`
- `src/Jazor.RazorVue/RazorSdk/*`
- `src/Jazor.RazorVue/RenderTree/*`
- `src/Jazor.RazorVue.Test/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: M

Implementation notes:

- Default analyzer SFC generation now uses `RazorVueBaselineFirstTemplateFrontend(BuildRenderTreeTemplateFrontend.Instance)` instead of the legacy IR-first compatibility frontend.
- Razor SG tail SFC output now binds generated Razor C# into the compilation before creating `RazorVueCompilationContext`, so `.razor` components have Roslyn `BuildRenderTree` syntax/operation available as the semantic baseline.
- Tail artifact tests assert generated artifacts come from the generated render body and not from raw Razor document text.
- The old preferred frontend route is no longer the default SFC production route and has been isolated as explicit legacy compatibility/parity coverage.

### RVCL-006: Implement Minimal No-Regression IR Enhancement Layer

Status: DONE

Description:
Convert current Razor IR frontend behavior into an enhancement layer with a conservative first slice: source-origin improvements, template fidelity metadata, and supported static/mixed attribute annotations. Unsupported IR shapes must not fake better output.

Acceptance criteria:

- [x] Enhancement layer can no-op without changing runtime semantics.
- [x] Supported enhancement cases are covered by tests.
- [x] Unsupported enhancement shape reports diagnostic or records absence without corrupting baseline output.

Verification:

- [x] `dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -v minimal`
- [x] `dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorIrEnhancement|FullyQualifiedName~RazorVueRenderFrontendContractTests" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueRenderFrontendContractTests|FullyQualifiedName~RazorVueGeneratorRouteTests|FullyQualifiedName~RazorVueTemplateFrontendParityTests" -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName~RazorVuePipelineTests|FullyQualifiedName~RazorVueSfcArtifactFactoryTests" -v minimal`

Dependencies: `RVCL-005`

Files likely touched:

- `src/Jazor.RazorVue/RazorSdk/*`
- `src/Jazor.RazorVue/Sfc/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: M

Implementation notes:

- Added `RazorVueRazorIrRenderEnhancer` as the conservative enhancement layer.
- `RazorVueRazorIrTemplateFrontend.TryEnhanceRenderTree(...)` now no-ops when IR input is absent, unsupported, or structurally incompatible with the Roslyn/BuildRenderTree baseline.
- The enhancement path can graft better Razor source origins onto a baseline render tree only after matching render node shape, attribute/slot structure, and operation/symbol identity. It never replaces baseline C# operations or render structure.
- Default analyzer SFC output and Razor SG tail output now attach `RazorVueRazorIrTemplateFrontend` as an optional enhancement after `BuildRenderTreeTemplateFrontend`.
- Route tests now expect `GeneratedMappingQuality.ExactSource` in `.razor` SFC artifacts while still asserting rendered expressions come from the generated render body.

### RVCL-007: Protect Descriptor And Runtime Subset Separation

Status: DONE

Description:
Add tests and implementation guards to ensure descriptor-owned members are not lowered twice as normal runtime code, while runtime setup/lifecycle/helper members remain available to compiler lowering.

Acceptance criteria:

- [x] Descriptor-only members affect descriptor/HMR identity but do not emit duplicate setup code.
- [x] Runtime helper methods/classes referenced by render/setup still lower through `Jazor.Compiler`.
- [x] `.razor.cs` partial members and `@code` members are both visible through merged component semantics.

Verification:

- [x] Focused descriptor/lifecycle/setup tests pass.
- [x] Add at least one `.razor` + `.razor.cs` regression.

Dependencies: `RVCL-005`

Files likely touched:

- `src/Jazor.RazorVue/Descriptor/*`
- `src/Jazor.RazorVue/Lowering/*`
- `src/Jazor.RazorVue.Test/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: M

Implementation notes:

- Added SFC artifact regressions that assert `[Parameter]` descriptor members contribute to props/emits/slots but are not rediscovered as runtime setup properties.
- Added a partial `.razor.cs` + `.razor.g.cs` regression where a render body calls a `.razor.cs` helper that depends on a generated `@code`-like helper, proving both partial sources are visible through the merged Roslyn component symbol.
- Verified helper lowering still flows through setup/script lowering and compiler-owned expression semantics, with render expressions lifted through the existing computed binding path.

### RVCL-008: Harden Diagnostics And Failure Modes

Status: DONE

Description:
Make failure behavior production-grade: no silent missing artifacts, no fake empty catalogs in required scenarios, and diagnostics must name the component, route, missing input, and suggested boundary.

Acceptance criteria:

- [x] Missing generated render body has actionable diagnostic.
- [x] Missing IR enhancement input does not silently claim enhanced output.
- [x] Unsupported C# semantics still fail from compiler/semantic lowering at usage site.

Verification:

- [x] Focused generator diagnostic tests pass.
- [x] Existing `JAZORVGA018` / `JAZORVGA019` / `JAZORVGA020` behavior remains stable unless explicitly updated.

Dependencies: `RVCL-006`

Files likely touched:

- `src/Jazor.Analyzer/RazorVue/Generation/*`
- `src/Jazor.RazorVue/RazorSdk/*`
- `src/Jazor.RazorVue.Test/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: M

Implementation notes:

- `BuildRenderTreeTemplateFrontend` / `RazorVueRenderTreeExtractor` now fail with a structured `CanonicalizationFailed` issue when a component snapshot cannot provide a Roslyn-bindable `BuildRenderTree` body.
- Normal SFC generation uses the baseline-first frontend by default: generated `.razor.g.cs` render bodies can produce SFC artifacts without requiring Razor IR/carrier input.
- Missing Razor IR/SG enhancement input is explicitly treated as no enhancement; generated artifacts do not claim `ExactSource` mapping quality unless enhancement input is present and compatible.
- `JAZORVGA001` now formats structured RazorVue compilation issues with both owner component and message, so missing baseline diagnostics no longer surface unresolved `{0}` / `{1}` placeholders.
- Razor SG tail output reports `JAZORVGA020` when received generator documents produce no SFC artifacts, including component and tail document summaries.

### RVCL-009: Provide Net11 Razor SG Tail Injection Backend

Status: DONE

Description:
Replace the current Harmony-only runtime patch dependency with a narrowly scoped Razor SG `Initialize(IncrementalGeneratorInitializationContext)` hook backend that works on the repository's `net11.0` preview SDK. This backend is not a general-purpose method detour library; it exists only to observe official Razor SG initialization, locate the official generated document output node, and register RazorVue tail output into the same incremental generator context.

Acceptance criteria:

- [x] `net11.0` external build can install the Razor SG tail hook without `Lib.Harmony`, `HarmonyX`, or `MonoMod.RuntimeDetour`.
- [x] The backend is constrained to the exact validated Razor SG `Initialize(IncrementalGeneratorInitializationContext)` shape and refuses ambiguous or unsupported shapes with `JAZORVGA019`.
- [x] Tail registration behavior remains identical: prefer implementation source output, fall back only to official host output, never run a private Razor SG.
- [x] The backend preserves single invocation, current-context registration tracking, exception isolation, and diagnostic trace behavior.
- [x] Existing Harmony route is removed; no `0Harmony.dll` package payload is required for the verified route.

Verification:

- [x] `dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorSourceGeneratorBootstrapPatchTests|FullyQualifiedName~RazorSourceGeneratorCompatibilityProbeTests|FullyQualifiedName~RazorVueGeneratorRouteTests" -v minimal`
- [x] External Razor SG integration build emits tail trace with `HostOutputHookInstalled = true` and `TailOutputRegisteredForCurrentContext = true` on CoreCLR 11.

Dependencies: `RVCL-008`

Files likely touched:

- `src/Jazor.Analyzer/RazorVue/Generation/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`

Estimated scope: L

Investigation notes:

- Official Razor SG source confirms that generated C# is emitted through `RegisterImplementationSourceOutput`, while `RazorCodeDocument` / Razor IR is carried only by `RegisterHostOutput` as `RazorGeneratorResult`. Official Razor does not persist IR to disk; `.razor.g.cs` alone is insufficient for RazorVue IR enhancement.
- Roslyn host outputs are exposed to the generator host through `GeneratorRunResult.HostOutputs`, not as a public generator-to-generator input. Wrapper/MSBuild post-task routes remain rejected because they either duplicate Razor SG execution or lose IR.
- The accepted production route is therefore a narrow hook of official `RazorSourceGenerator.Initialize(IncrementalGeneratorInitializationContext)` that registers RazorVue tail output into the same official incremental generator graph after Razor SG registers its output nodes.
- `Lib.Harmony 2.4.2` loads `lib/net10.0/0Harmony.dll` and fails on CoreCLR 11 with `System.PlatformNotSupportedException: CoreCLR version 11.0.0 is not supported`.
- `MonoMod.RuntimeDetour 25.3.4` ships up to `net10.0`; its package build target rejects `net11.0`, and even with `MonoMod_ReallySkipCheckTargetRuntime=true` it fails at runtime with `CoreCLR version 11.0.0 is not supported`.
- `MonoMod.RuntimeDetour 21.12.13.1` does not reject at build time but triggers a CoreCLR fatal error on the same minimal hook probe, so it is not a viable workaround.
- `HarmonyX 2.16.1` depends on MonoMod RuntimeDetour and is rejected by the same MonoMod target-runtime check on `net11.0`.
- `MonoDetour 0.7.13` is a HookGen/ILHook layer powered by MonoMod RuntimeDetour and only ships `net9.0`/`netstandard2.0` assets; it inherits the same runtime risk.
- `DH.DotNetDetour 10.0.2026.52200033` ships only `net10.0`/`net9.0`; a minimal `MethodBase` patch probe on CoreCLR 11 loaded but did not patch the target method.
- Older `DotNetDetour 1.0.3` and `Alex.CSharpDetour 2.5.0` restore through `net45` compatibility with `NU1701`; they are not production candidates for an analyzer running under modern CoreCLR.

Implementation notes:

- Added a self-owned native jump hook backend for Razor SG `Initialize` and removed the Harmony package/payload path.
- Split tail-output registration into `RazorSourceGeneratorTailOutputRegistration`, so the hook backend only owns interception and the existing output-node scanning/registration logic remains isolated.
- Added registration-version tracking in `RazorSourceGeneratorBootstrapState` so fallback diagnostics cannot be fooled by pooled Roslyn output-node builder object identity from previous runs.
- `RazorSourceGeneratorFallbackOutput` remains diagnostic-only and now no-ops when the current generator initialization has successfully registered RazorVue tail output.

### RVCL-010: Harden Windows Razor SG Hook Stability

Status: DONE

Description:
Productionize the self-owned Razor SG `Initialize` hook for the current supported Windows x64/CoreCLR 11 path and document deferred cross-platform certification separately. The hook is required because official Razor SG does not persist IR and exposes `RazorCodeDocument` only through host output memory.

Acceptance criteria:

- [x] Windows x64 remains verified by external Razor SG integration build.
- [x] Cross-platform hook evidence is explicitly deferred to `RVCL-013` and is not a blocker for Windows production stabilization.
- [x] Unsupported platforms fail before patching, do not run private Razor SG fallback, and include platform/architecture in diagnostics.
- [x] Hook install/uninstall logic preserves original Razor SG exceptions and cannot recursively re-enter the replacement.
- [x] Packaging tests prove no Harmony payload is included.

Verification:

- [x] `dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -v minimal`
- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorSourceGeneratorBootstrapPatchTests|FullyQualifiedName~RazorSourceGeneratorCompatibilityProbeTests|FullyQualifiedName~RazorVueGeneratorRouteTests" -v minimal`
- [x] `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~CreateLocalPackage_IncludesRazorVueAuthoringAssets" -v minimal`
- [x] Windows stability evidence and deferred platform certification boundary are recorded in this document.

Dependencies: `RVCL-009`

Files likely touched:

- `src/Jazor.Analyzer/RazorVue/Generation/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`
- `src/Jazor.EmitTest/*`
- CI or local verification notes

Estimated scope: M

Implementation notes:

- Wrapper/proxy/MSBuild replacement of the official Razor SG remains rejected. Historical evidence is recorded in `RazorVue.RazorSg.MainlineIrInjection.DecisionRecord.md` sections 9.2 and 10, and `RazorVue.RazorSg.TailInjection.Guidance.md` section 2. The only accepted production line is hooking the official Razor SG runtime path to reuse its in-memory IR and generated C# data flow.
- Added a self-owned native hook platform matrix with deterministic guard tests. Windows x64 is the current production-supported path for this milestone. Linux x64, macOS x64, macOS arm64, and Windows arm64 remain implementation candidates but require separate external certification in `RVCL-013`. Linux arm64 and non-x64/non-arm64 architectures are blocked before patching with platform/architecture in the failure text.
- Added a current-runtime hook self-test that patches and restores a simple method before touching the official Razor SG. If the backend cannot patch/restore on the current platform, bootstrap records `PatchUnavailable`, `.razor` component generation reports `JAZORVGA019`, and the diagnostic-only fallback output no-ops instead of reporting or running a private Razor SG fallback.
- Added a recursion guard around `InvokeOriginal(...)` and kept original `TargetInvocationException` unwrapping so official Razor SG exceptions are preserved.
- Current machine evidence:
  - OS/architecture: Windows x64, CoreCLR 11 preview.
  - WSL is unavailable on this machine (`Wsl/0x80070422`), and Docker CLI is not installed, so Linux x64 external integration verification cannot be completed here.
  - `dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -m:1 -p:UseSharedCompilation=false -v minimal` passed.
  - `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorSourceGeneratorBootstrapPatchTests|FullyQualifiedName~RazorSourceGeneratorCompatibilityProbeTests|FullyQualifiedName~RazorVueGeneratorRouteTests" -m:1 -p:UseSharedCompilation=false -v minimal` passed: 25/25.
  - `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~CreateLocalPackage_IncludesRazorVueAuthoringAssets" -m:1 -p:UseSharedCompilation=false -v minimal` passed: 1/1.

Deferred cross-platform work:

- `RVCL-013` will run or provision Linux x64, macOS x64, macOS arm64, and Windows arm64 verification, or deliberately change those guard entries to `JAZORVGA019` blocked with concrete reasons.

### RVCL-011: End-To-End SFC Consumer Verification

Status: DONE

Description:
Run and, if needed, repair the production consumer path: external package consumption, generated `.vue`, Deno build, SSR smoke, and browser smoke. This validates the architecture beyond unit-level parity.

Acceptance criteria:

- [x] Local package build includes analyzer and RazorVue payload without Razor Compiler payload.
- [x] External `.razor` consumer emits SFC artifacts.
- [x] Pure Deno consumer build and smoke paths pass.
- [x] Focused external consumer integration tests pass.

Verification:

- [x] `dotnet run --file ./samples/RazorVue.TodoList/build-local.cs`
- [x] `cd samples/RazorVue.TodoList/Todo.Host/consumer && dotnet run --file .\scripts\run-deno.cs -- task build`
- [x] `cd samples/RazorVue.TodoList/Todo.Host/consumer && dotnet run --file .\scripts\run-deno.cs -- task test`
- [x] `tar -tf .tmp/nupkg-sample/Jazor.0.1.27-alpha.0.21.nupkg | Select-String -Pattern 'Microsoft\.AspNetCore\.Razor|Microsoft\.CodeAnalysis\.Razor|Razor\.Language|Razor\.Compiler|Razor\.Utilities|Harmony|MonoMod|Detour'` returns no package entries.
- [x] Relevant external consumer tests pass:
  - `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts|FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace|FullyQualifiedName~Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace" -m:1 -p:UseSharedCompilation=false -v minimal`

Dependencies: `RVCL-010`

Files likely touched:

- `src/Jazor/*`
- `samples/RazorVue.TodoList/*`
- `src/Jazor.RazorVue.Test/*`

Estimated scope: M

### RVCL-012: Cleanup Naming, Docs, And Obsolete Route

Status: DONE

Description:
Remove or rename misleading IR-primary frontend names after the baseline-first path is verified. Update README and plan documents so future work follows the new route.

Acceptance criteria:

- [x] No active doc says Razor IR replaces Roslyn/`BuildRenderTree` as component semantic source.
- [x] Misleading internal type names are renamed or wrapped with clear comments if renaming is too disruptive.
- [x] This document marks all completed tasks and records remaining follow-up.

Verification:

- [x] `rg "Razor IR.*取代|替代.*BuildRenderTree|默认前端.*Razor IR" src docs`
  - Remaining active-plan hits are negative statements such as "not replace" / "must not be interpreted as replacing"; `docs/03-完成/` hits are historical archive material per repository documentation rules.
- [x] Focused RazorVue tests pass after cleanup:
  - `dotnet build src/Jazor.RazorVue/Jazor.RazorVue.csproj --no-restore -m:1 -p:UseSharedCompilation=false -v minimal`
  - `dotnet build src/Jazor.Analyzer/Jazor.Analyzer.csproj --no-restore -m:1 -p:UseSharedCompilation=false -v minimal`
  - `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorVueTemplateFrontendParityTests|FullyQualifiedName~RazorVueGeneratorRouteTests" -m:1 -p:UseSharedCompilation=false -v minimal`
  - `dotnet test src/Jazor.RazorVue.Test/Jazor.RazorVue.Test.csproj --filter "FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithGeneratedRazorComponentButNoCarrier_DefaultGenerator_WaitsForTailBridge|FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithAlignedRazorCarrier_DefaultGenerator_WaitsForRazorSgTail|FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithGeneratedRazorComponentButNoCarrier_SfcOutput_WaitsForTailBridge|FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithRazorCodeBehindPartialOnly_SfcOutput_WaitsForRazorSgTail|FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithGeneratedRazorComponentButNoCarrier_AndEnabledRazorSgIntegrationWithoutTailRegistration_ReportsJAZORVGA018|FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithGeneratedRazorComponentButNoCarrier_AndIncompatibleRazorSgAbi_ReportsJAZORVGA019|FullyQualifiedName=Jazor.RazorVue.Test.ESGeneratorTests.GenerateCatalog_WithRazorVuePartialOnly_AndUnavailableHookPlatform_ReportsJAZORVGA019" -m:1 -p:UseSharedCompilation=false -v minimal`

Dependencies: `RVCL-011`

Files likely touched:

- `src/Jazor.RazorVue/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`
- `docs/02-计划/jolt/razorvue-implementation/*`

Estimated scope: M

### RVCL-013: Cross-Platform Razor SG Hook Certification

Status: BLOCKED

Description:
Certify or explicitly block the self-owned Razor SG `Initialize` hook on non-Windows-x64 platforms. This task is intentionally deferred from the Windows stabilization milestone so the current production path can move forward without pretending unavailable platform evidence exists.

Blocking condition:

- Current user decision is to skip platform evidence for now and keep the milestone focused on Windows stability. Do not auto-start this task until cross-platform certification is explicitly resumed.

Acceptance criteria:

- [ ] Linux x64 hook is verified with an external Razor SG integration build, or blocked with `JAZORVGA019` and a concrete reason.
- [ ] macOS x64 and macOS arm64 behavior is verified, or blocked with `JAZORVGA019` and concrete reasons.
- [ ] Windows arm64 behavior is verified, or blocked with `JAZORVGA019` and a concrete reason.
- [ ] Platform support documentation and diagnostics agree with the verified matrix.

Verification:

- [ ] External Razor SG integration build on each verified platform.
- [ ] Focused bootstrap/route tests on each verified platform.
- [ ] Packaging smoke confirms no third-party detour payload is introduced.

Dependencies: `RVCL-010`

Files likely touched:

- `src/Jazor.Analyzer/RazorVue/Generation/*`
- `src/Jazor.RazorVue.RazorIr.Test/*`
- `docs/02-计划/jolt/razorvue-implementation/*`

Estimated scope: M

### RVCL-014: Remove Stale Detour Payload Assumptions From Windows Path

Status: DONE

Description:
Tighten the current Windows-supported Razor SG hook path so tests, package checks, and active guidance cannot accidentally reintroduce Harmony, MonoMod, DotNetDetour, or other third-party detour payload assumptions. This is a Windows stability follow-up and does not resume cross-platform certification.

Acceptance criteria:

- [x] External Razor SG bootstrap test payload setup no longer copies `0Harmony.dll` or any third-party detour payload.
- [x] Package authoring asset test fails if analyzer/lib payload contains Harmony, MonoMod, or Detour entries.
- [x] Active RazorVue guidance docs describe the self-owned native hook instead of Harmony patching.
- [x] Cross-platform certification remains blocked in `RVCL-013`.

Verification:

- [x] `dotnet test src/Jazor.RazorVue.RazorIr.Test/Jazor.RazorVue.RazorIr.Test.csproj --filter "FullyQualifiedName~RazorSourceGeneratorBootstrapPatchTests" -m:1 -p:UseSharedCompilation=false -v minimal`
- [x] `dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj --filter "FullyQualifiedName~CreateLocalPackage_IncludesRazorVueAuthoringAssets" -m:1 -p:UseSharedCompilation=false -v minimal`

Dependencies: `RVCL-012`

Files likely touched:

- `src/Jazor.RazorVue.RazorIr.Test/RazorSourceGeneratorBootstrapPatchTests.cs`
- `src/Jazor.EmitTest/SdkIntegrationTests.cs`
- `docs/02-计划/jolt/razorvue-implementation/*`
- `docs/02-计划/compiler/net11-csharp15-union-migration-status.md`

Estimated scope: S

Implementation notes:

- Removed stale `0Harmony.dll` copying from the external Razor SG bootstrap test fixture.
- Extended `CreateLocalPackage_IncludesRazorVueAuthoringAssets` so the package payload guard rejects Harmony, MonoMod, and Detour entries in addition to Razor Compiler / Razor Utilities payloads.
- Updated active Razor SG guidance and net11 migration notes to describe the self-owned native hook path and diagnostic-only missing-tail behavior. Historical detour library investigation remains recorded in `RVCL-009`.
- `CreateLocalPackage_IncludesRazorVueAuthoringAssets` timed out once at 184 seconds during the first concurrent verification attempt, then passed when rerun alone with a longer timeout.

## Checkpoints

### Checkpoint A: After RVCL-002

- [x] Trigger routing is protected by tests.
- [x] No production code route has been changed yet without tests.
- [x] `Current next task` points to `RVCL-003`.

### Checkpoint B: After RVCL-005

- [x] Baseline-first `.razor` and handwritten `.cs` paths both work.
- [x] No IR enhancement can bypass compiler-owned expression semantics.
- [x] Focused SFC pipeline tests pass.

### Checkpoint C: After RVCL-008

- [x] Failure modes are diagnostic and actionable.
- [x] Unsupported cases fail fast rather than emitting misleading artifacts.
- [x] Plan document has updated risks and current status.

### Checkpoint D: After RVCL-012

- [x] External consumer path is verified.
- [x] Docs and code naming agree with the architecture.
- [x] Remaining work is explicitly listed or the plan is marked complete.

## Risks And Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Generated `BuildRenderTree` operation is not directly available in the same compilation slice | High | Use official SG tail document C# text and current compilation binding path deliberately; add failure diagnostics instead of guessing. |
| Self-owned Razor SG hook is runtime/platform sensitive | High | Keep the hook narrow, validate the Razor SG ABI before patching, treat Windows x64 as the current production-supported path, certify non-Windows-x64 platforms in `RVCL-013`, and fail with diagnostics instead of fallback generation on unsupported platforms. |
| Private fallback route runs Razor SG inside the analyzer process | High | Keep fallback path diagnostic-only; tests must distinguish official SG tail output from forbidden private fallback. |
| IR enhancement changes runtime semantics | High | Require baseline/enhancement parity tests and apply IR only at canonical/SFC semantic layer. |
| Descriptor members emit duplicate runtime code | High | Add descriptor/runtime subset tests before broad rewiring. |
| Handwritten `.cs` path regresses while fixing `.razor` | High | Keep route tests separate and run focused handwritten BuildRenderTree tests. |
| Source-origin quality regresses | Medium | Compare origin provenance in parity tests and keep mapping quality explicit. |
| Cleanup renames create broad churn | Medium | Prefer compatibility wrappers first; rename only after behavior is verified. |

## Audit Notes

Status: `RVCL-001` complete.

### Current Trigger Route Map

- Normal analyzer/source-generator route starts in `Jazor.RazorVue.Analysis.RazorVueGenerator.Initialize`, collects `[ECMAScriptModule]` class candidates through `ForAttributeWithMetadataName`, then calls `EmitRazorVueCatalog`.
- With `JazorRazorVueEnableRazorSgIntegration=false`, `EmitRazorVueCatalog` runs the configured `RazorVueSfcPipeline` over `RazorVueRazorDocumentSemanticFrontend.Instance`. This is the current route for normal SFC generation and for handwritten `.cs` component generation outside Razor SG integration.
- With `JazorRazorVueEnableRazorSgIntegration=true`, `EmitRazorVueCatalog` first validates SG tail registration, then keeps only `GetIntegrationEligibleHandwrittenBuildRenderTreeSnapshots(...)` for the normal generator route. This intentionally leaves `.razor` components to official SG tail output and keeps pure handwritten `.cs BuildRenderTree` components in the analyzer/source-generator route.
- Razor SG tail route starts in `RazorSourceGeneratorTailOutput.Emit(...)`, converts host output documents to `RazorVueRazorSourceGeneratorDocumentInput`, then calls `RazorVueRazorSourceGeneratorTailBridge.ExecuteSfcPipeline(...)`.
- Tail bridge creates `RazorVueRazorSourceGeneratorSemanticFrontend` and `RazorVueBaselineFirstTemplateFrontend(BuildRenderTreeTemplateFrontend.Instance, RazorVueRazorIrTemplateFrontend)`; the semantic frontend filters snapshots to `RazorSourceGeneratorDocument != null`, so tail currently emits only `.razor`-bound snapshots.
- Razor SG fallback registration starts in `RazorSourceGeneratorFallbackOutput.Register(...)`; it must remain diagnostic-only and must not privately run the SDK Razor source generator inside the analyzer process. Missing official tail output reports `JAZORVGA020`.

### Current Type And Function Map

- `RazorVueCompilationContext.DiscoverComponentCandidates()` already treats a RazorVue component as the merged Roslyn class symbol: `[ECMAScriptModule]`, non-static, derives from `ComponentBase`, implements `IVueComponent`, and belongs to the current assembly. This is compatible with partial `.razor` + `.razor.cs` semantics.
- `RazorVueEntryClassifier` already separates static module classes from component classes and gathers lifecycle, setup logic members, fields, properties, and `BuildRenderTree`. This is the correct baseline discovery surface but the `BuildRenderTree` source-backed requirement needs rechecking for SG-generated syntax availability.
- `RazorVueSemanticSnapshot` already carries component symbol, render method, descriptor, lifecycle, logic, Razor IR carrier, and SG document. The target split can likely evolve this type instead of replacing it.
- `RazorVueSfcArtifactFactory.Lower(...)` is the main seam: it calls `_templateFrontend.CreateRenderTree(...)`, then canonical model, SFC semantic model, and artifact creation. Baseline-first lowering should be expressed at or before this seam.
- `RazorVueLegacyIrFirstTemplateFrontend` now carries the old explicit compatibility policy: SG document means `RazorVueRazorIrTemplateFrontend`; handwritten `BuildRenderTree` means `BuildRenderTreeTemplateFrontend`; generated `.razor.g.cs` without a bound Razor document throws. It is not the default SFC production route.
- `BuildRenderTreeTemplateFrontend` and `RazorVueRenderTreeExtractor` are the reusable Roslyn/`IOperation` baseline extractor. Today `Extract(...)` reads `snapshot.BuildRenderTreeMethod.DeclaringSyntaxReferences` from the current compilation; for SG tail documents, generated C# may need to be parsed/bound into a compilation slice before this path can serve as the baseline.
- `RazorVueRazorIrTemplateFrontend` is currently a full template frontend, not an enhancement layer. Its operation resolver already has logic for binding generated C# text and source mappings, so pieces of it can be reused for IR enhancement and generated-operation binding, but it must stop being the primary source of C# expression semantics.
- `RazorVueRazorDocumentSemanticFrontend` maps SG documents to component symbols by generated type name and carries imported namespaces. This is useful for `.razor` tail routing but must not imply legacy IR-first render extraction.

### Tests To Reuse

- `BuildRenderTreeTemplateFrontendTests` is the strongest reusable coverage for Roslyn/`BuildRenderTree` canonical render extraction, including slots, event modifiers, local declarations, imperative segments, and generated-style builder calls.
- `RazorVueSfcArtifactFactoryTests` already exercises SFC lowering from `BuildRenderTreeTemplateFrontend`, including a `.razor.g.cs`-style typed slot scenario. These are good baseline-first artifact tests.
- `RazorVuePipelineTests` contains broad handwritten `BuildRenderTree` behavior and identity/HMR coverage through `CreateBuildRenderTreePipeline()`.
- `RazorSourceGeneratorTailOutputTests`, `RazorSourceGeneratorBootstrapPatchTests`, and `RazorSourceGeneratorHostOutputTests` are the existing route and host-output test surface for official SG tail behavior and forbidden private fallback diagnostics.
- `RazorVueRazorIrTemplateFrontendTests`, `RazorVueRazorIrCompilerExpressionBridgeTests`, and `RazorVueTemplateFrontendParityTests` preserve valuable IR fidelity cases. Legacy IR-first parity remains only as compatibility coverage; production SFC behavior is baseline output plus optional enhancement.

### Tests To Rewrite Or Add

- Rewrite `RazorVueTemplateFrontendParityTests` assumptions that the legacy IR-first compatibility frontend should agree with `RazorVueRazorIrTemplateFrontend` as the preferred path. Future parity should compare baseline-only and enhanced output for runtime-equivalent cases, with source-origin/fidelity assertions isolated to enhancement.
- Add generator-route tests proving pure handwritten `.cs BuildRenderTree` components still emit from normal analyzer/source-generator output when SG integration is enabled and no Razor SG document exists.
- Add `.razor` route tests proving official SG tail output is required for `.razor` components and that missing generated render semantics produces an actionable diagnostic rather than an empty catalog.
- Add partial component tests where `.razor`, `.razor.cs`, and `@code`/generated members all contribute to the merged component symbol: descriptor members, setup/lifecycle/helper members, and render body must remain visible through the same snapshot.
- Add tests that mark private fallback as forbidden: fallback diagnostics may be emitted, but private Razor SG execution must not be documented or tested as a production route.

### Implementation Hazards

- `RazorVueRenderTreeExtractor` cannot baseline SG tail documents unless the generated `BuildRenderTree` method is present as Roslyn syntax/operation in the compilation used for lowering, or the pipeline deliberately creates a compilation slice that binds the SG generated C# with the component partials and references.
- The private fallback route violates the "no private Razor SG run in production" target. It must stay diagnostic-only even when tail injection fails.
- `RazorVueLegacyIrFirstTemplateFrontend` still blends old route selection, baseline extraction, and IR conversion behind one compatibility interface. It must not be used as the default production route.
- Descriptor subset and runtime subset are discovered from the same merged symbol. Rewire work must keep descriptor-owned members from being emitted again as setup/runtime logic while still allowing helper methods/classes to lower through `Jazor.Compiler`.
- Razor IR source mappings improve source origin quality, but if used before baseline semantics they can accidentally decide runtime behavior from template shape. Enhancement must operate after canonical render semantics or carry only fidelity metadata.
- Diagnostics currently allow empty catalog returns in several paths. Baseline-first production behavior needs route-specific diagnostics for missing generated render body, unreadable SG output, forbidden private fallback, and unsupported C# lowering.

## Progress Log

- 2026-05-28: Created adjustment execution plan. `RVCL-001` marked as `NEXT`.
- 2026-05-28: Completed `RVCL-001` read-only audit. Current implementation drift, trigger routes, reusable tests, rewrite targets, and hazards documented. `RVCL-002` marked as `NEXT`.
- 2026-05-28: Started `RVCL-002`; route-lock tests will be added before production pipeline rewiring.
- 2026-05-28: Completed `RVCL-002`. Added `RazorVueGeneratorRouteTests` to lock normal handwritten `.cs`, official Razor SG tail, missing SG document diagnostics, and forbidden private fallback diagnostics. Focused route and Razor SG tests pass. `RVCL-003` marked as `NEXT`.
- 2026-05-28: Started `RVCL-003`; component semantic baseline contract will be made explicit without rewiring behavior yet.
- 2026-05-28: Completed `RVCL-003`. Added `RazorVueComponentSemanticBaseline`, exposed `RazorVueSemanticSnapshot.ComponentBaseline`, and added a partial component regression for descriptor/runtime/render separation. Focused build and descriptor/lifecycle tests pass. `RVCL-004` marked as `NEXT`.
- 2026-05-28: Started `RVCL-004`; render baseline and Razor IR enhancement contracts will be split before rewiring default SFC behavior.
- 2026-05-28: Completed `RVCL-004`. Split baseline extractor and IR enhancement interfaces, added a baseline-first compatibility adapter, and covered handwritten plus Razor-generated baseline extraction. Focused build, route, frontend, and descriptor tests pass. `RVCL-005` marked as `NEXT`.
- 2026-05-28: Started `RVCL-005`; default SFC route will be rewired to use Roslyn/BuildRenderTree baseline first while keeping IR enhancement conservative until dedicated enhancement semantics exist.
- 2026-05-28: Completed `RVCL-005`. Default SFC generation and Razor SG tail output now bind and lower from Roslyn/BuildRenderTree baseline first; IR-primary frontend is retained only as an explicit compatibility path. Focused RazorVue build, analyzer build, SFC artifact/pipeline tests, route tests, frontend contract tests, and parity tests pass. `RVCL-006` marked as `NEXT`.
- 2026-05-28: Started `RVCL-006`; Razor IR enhancement will be constrained to conservative source-origin/fidelity enrichment and must no-op rather than replace baseline render semantics when compatibility is not proven.
- 2026-05-28: Completed `RVCL-006`. Added a conservative Razor IR render enhancer that preserves baseline structure and C# operations, grafts exact Razor source origins only for compatible render trees, and no-ops for absent/unsupported/incompatible IR. Default SFC routes now attach IR as optional enhancement after BuildRenderTree baseline. Focused build, route, parity, frontend, and SFC pipeline tests pass. `RVCL-007` marked as `NEXT`.
- 2026-05-28: Started `RVCL-007`; descriptor/runtime subset separation will be protected with focused regressions before changing discovery or lowering.
- 2026-05-28: Completed `RVCL-007`. Added descriptor/runtime separation and partial `.razor.cs` + generated `@code` helper regressions in `RazorVueSfcArtifactFactoryTests`. Focused descriptor/setup tests, full `RazorVueSfcArtifactFactoryTests`, and `Jazor.RazorVue` build pass. `RVCL-008` marked as `NEXT`.
- 2026-05-28: Started `RVCL-008`; generator and Razor SG route diagnostics will be hardened with focused regressions before changing failure behavior.
- 2026-05-28: Completed `RVCL-008`. Hardened missing render baseline diagnostics, prevented missing IR enhancement input from claiming exact source enhancement, fixed structured `JAZORVGA001` formatting, and added normal generator/tail no-artifact diagnostics. Focused `Jazor.RazorVue.Test` diagnostics, RazorIr route/frontend/tail tests, `Jazor.Analyzer` build, and `Jazor.RazorVue` build pass. `RVCL-009` marked as `NEXT`.
- 2026-05-28: Corrected trigger contract after net11 detour investigation. `.razor` generation is official Razor SG tail only; private fallback is diagnostic-only. `Lib.Harmony 2.4.2`, `MonoMod.RuntimeDetour 25.3.4/21.12.13.1`, `HarmonyX 2.16.1`, `MonoDetour 0.7.13`, `DH.DotNetDetour 10.0.2026.52200033`, `DotNetDetour 1.0.3`, and `Alex.CSharpDetour 2.5.0` are not viable CoreCLR 11 production backends. `RVCL-009` is now the dedicated net11 Razor SG tail injection backend task; external consumer verification moved to `RVCL-010`.
- 2026-05-28: Completed `RVCL-009` Windows x64/CoreCLR 11 slice. Replaced Harmony with a self-owned narrow Razor SG `Initialize` hook, split tail-output registration, preserved diagnostic-only fallback, and fixed current-run registration tracking with a registration version instead of pooled Roslyn output-node identity. Verified `Jazor.Analyzer` build and focused Razor SG bootstrap/compatibility/route tests. `RVCL-010` marked as `NEXT` for cross-platform hook matrix hardening.
- 2026-05-28: Started `RVCL-010`. Reconfirmed historical wrapper/proxy/MSBuild replacement routes are rejected; only the official Razor SG runtime hook route can access non-persisted IR without private SG fallback. Added platform guard tests, current-runtime native hook self-test, `PatchUnavailable` diagnostics, fallback no-op on unavailable hook backend, and hook recursion protection. Windows x64 external Razor SG integration remains verified; Linux/macOS/Windows arm64 external verification is still required, so `RVCL-010` remains `IN_PROGRESS`.
- 2026-05-28: Scoped `RVCL-010` to Windows production stability per current milestone decision. Cross-platform hook evidence is explicitly deferred to `RVCL-013`; Windows x64/CoreCLR 11 hook stability remains covered by the existing analyzer build, focused Razor SG bootstrap/compatibility/route tests, emit packaging test, current-runtime patch/restore self-test, recursion guard, and no-private-fallback diagnostics. `RVCL-010` marked `DONE`; `RVCL-011` marked as `NEXT`.
- 2026-05-28: Started `RVCL-011`. The previously documented `todo-consumer` path was not present in the current sample tree; verified the actual sample consumer layout before running the Deno consumer build.
- 2026-05-28: `RVCL-011` sample verification passed on the current Windows path. `build-local.cs` packed local `Jazor` / `ECMAScript.Vuetify`, rebuilt `Todo.Host`, generated 2 SFC artifacts, and produced browser assets through `Todo.Host/consumer` Deno `task build` and `task test`. The generated `Jazor` nupkg contains analyzer and `Jazor.RazorVue` payload and no Razor Compiler / Razor Utilities / Harmony / MonoMod / Detour package entries. Remaining work is focused external consumer integration tests.
- 2026-05-28: Completed `RVCL-011`. Focused external consumer integration tests passed: independent `.razor` SFC consumer emits artifacts, independent pure Deno pipeline passes, and TodoList sample pure Deno pipeline passes. `RVCL-012` marked as `NEXT`.
- 2026-05-28: Started `RVCL-012`. Cleanup targets are stale sample consumer documentation paths and the misleading preferred frontend compatibility type/test naming. The old IR-first route will remain only as explicit legacy compatibility/parity coverage, not as the default production route.
- 2026-05-28: Completed `RVCL-012`. Renamed `RazorVuePreferredTemplateFrontend` to `RazorVueLegacyIrFirstTemplateFrontend`, updated legacy compatibility tests and active docs, corrected current TodoList consumer paths to `Todo.Host/consumer`, and fixed the carrier-route regression test so a legacy Razor IR carrier without formal SG document input waits for official Razor SG tail output instead of producing an SFC from the normal generator route. Focused `Jazor.RazorVue`, `Jazor.Analyzer`, RazorIr parity/route, and exact `Jazor.RazorVue.Test` route tests pass. `RVCL-013` is marked `BLOCKED` by the current decision to skip cross-platform evidence for now.
- 2026-05-28: Added and completed `RVCL-014` as a Windows-only stability cleanup while `RVCL-013` remains blocked. Removed stale `0Harmony.dll` copying from the external Razor SG bootstrap fixture, extended package payload guards to reject Harmony / MonoMod / Detour entries, and updated active docs away from Harmony/fallback wording. Focused bootstrap tests passed 3/3; `CreateLocalPackage_IncludesRazorVueAuthoringAssets` timed out once at 184 seconds during concurrent verification and passed 1/1 when rerun alone with a longer timeout. Current next task returns to blocked `RVCL-013`; no unblocked automatic task remains in this plan.
