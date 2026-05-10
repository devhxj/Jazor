# .NET 11 / C# 15 Union Migration Status

> Status: active migration snapshot, verification in progress
> Date: 2026-05-10

## Current Decisions

- Target SDK is `11.0.100-preview.3.26207.106` via `global.json`; prerelease SDK usage is intentional.
- Roslyn package alignment follows the SDK compiler surface: `CodeAnalysisVersion` is `5.7.0-1.26207.106`. The attempted `5.8.0-1.26257.103` line was rejected because SDK Roslyn `5.7.0.0` skipped newer analyzers with `CS9057`.
- Preview BCL still lacks the final union runtime contract in this SDK, so `ECMAScript` carries a temporary `System.Runtime.CompilerServices.UnionAttribute` / `IUnion` shim. Remove this only after the target SDK ships the official types.
- Official union projection is allowed only for safe erased-value host wrappers. Object, interface, delegate, nullable-boundary, or C# binding-hostile branches must keep explicit strong `From(...)` factories or overloads.
- In-memory Roslyn test compilation references are aligned to .NET 11 via `Basic.Reference.Assemblies.Net110`; new tests should not reintroduce `Net100.References.All`.
- Analyzer support must mirror proven compiler host surfaces narrowly. Do not suppress `JAZOR001` or widen arbitrary CLR indexers to make Vue/WebIDL authoring compile.

## Completed

- Solution/project target migration is on the `net11.0` preview path where needed.
- Compiler recognizes `[Union]` / `IUnion` as host-erased union markers without widening arbitrary object contracts.
- WebIDL generator emits `[Union] + IUnion + Value + public case constructors` for safe union branches and preserves explicit factories for unsafe branches.
- Generated WebIDL output was refreshed and passed ECMAScript build validation.
- Representative Vue union `VueNamesOrOptions` now uses the official-preview union contract while retaining collection-builder authoring.
- Roslyn operation-surface audit now explicitly rejects the new collection-expression placeholder operation and has a reflection guard for visitor coverage drift.
- Root `NuGet.config` is present for `nuget.org` plus the preview `dotnet-tools` feed.
- Roslyn packages are aligned to SDK compiler version `5.7.0-1.26207.106`, restoring analyzer execution instead of `CS9057` analyzer skips.
- Razor source-generator integration compatibility is matched by structural ABI, not IL hash. The hard gate is the expected SDK generator type, `IIncrementalGenerator`, and public instance `Initialize(IncrementalGeneratorInitializationContext): void`; the optional `Initialize` IL SHA-256 is retained only as diagnostic/probe metadata.
- RazorIr external-build tests now resolve SDK versions semantically and derive `TargetFramework` / `RazorLangVersion` from the resolved SDK major version. This prevents `10.0.300-preview...` from being selected ahead of `11.0.100-preview...` by string ordering.
- On CoreCLR 11, Lib.Harmony detouring is skipped by design and RazorVue uses the fallback Razor SG host-output generation path. The external build tests now validate both the old hook path on supported runtimes and the fallback trace/catalog/artifact path on CoreCLR 11.
- Razor 11 component-attribute binding differences are handled at the authored surface with explicit strong expressions, not by weakening component APIs. Static values for non-`string` parameters such as `VuetifyTextValue`, `[String]` enums, and other host value wrappers must be written as typed C# expressions when Razor cannot legally bind an HTML-style literal.
- Compiler and analyzer now share the object-literal host predicate through `Jazor.Compiler.Util.IsObjectLiteralHostType`.
- `Jazor.Analyzer` accepts only the supported indexer slice:
  - object/collection initializer indexer assignments on object-literal host types such as `VueDictionary<TValue>` and `VueEventHandlers<TEvent>`
  - ECMAScript record-proxy single-parameter indexer reads and simple assignments already supported by compiler lowering
- Ordinary unmarked record indexer reads and assignments still report `JAZOR001`.
- Focused analyzer regressions were added for Vue dictionary initializers, Vue event-handler initializers, ECMAScript record-proxy reads/assignments, and unmarked record indexer reads/assignments.
- In-memory reference assembly consumers were moved from `Basic.Reference.Assemblies.Net100` to `Basic.Reference.Assemblies.Net110`.

## Latest Verification

- `dotnet build src\Jazor.Compiler\Jazor.Compiler.csproj --no-restore -v minimal -p:UseSharedCompilation=false` passed.
- `dotnet build src\Jazor.Analyzer\Jazor.Analyzer.csproj --no-restore -v minimal -p:UseSharedCompilation=false` passed.
- `dotnet build src\Wiki\Wiki.csproj -v minimal -p:UseSharedCompilation=false -m:1` passed with 0 warnings and 0 errors. This confirms the original Vue dictionary/event-handler analyzer diagnostics are fixed in the consuming project.
- `dotnet build Jazor.slnx -v minimal -p:UseSharedCompilation=false -m:1` passed with 0 warnings and 0 errors after warning cleanup.
- Focused analyzer tests passed:
  - `Jazor_VueDictionaryIndexerInitializer_IsAccepted`
  - `Jazor_VueEventHandlersIndexerInitializer_IsAccepted`
  - `Jazor_ECMAScriptRecordProxyIndexerRead_IsAccepted`
  - `Jazor_ECMAScriptRecordProxyIndexerAssignment_IsAccepted`
  - `Jazor_UnmarkedRecordIndexerRead_ReportsJAZOR001`
  - `Jazor_UnmarkedRecordIndexerAssignment_ReportsJAZOR001`
- Focused compiler tests passed:
  - `Convert_ClassUsingVueObjectDictionarySurface_FlattensIntoObjectLiteralMembers`
  - `Convert_ClassUsingVueObjectEventHandlers_FlattensEventListeners`
  - `Visit_ECMAScriptRecordProxyIndexerAccess_AllowsJavaScriptComputedAccess`
  - `Visit_ECMAScriptRecordProxyIndexerAssignment_AllowsJavaScriptComputedAssignment`
  - `VisitObjectCreation_VueDictionaryIndexer_StaticNullLiteral_IsOmitted`
- Migration guard tests passed:
  - `SemanticWalkerNotSupportTest`
  - `Convert_ClassUsingSystemUnionMarkerProjection_GeneratesNativeValue`
  - `Convert_ClassUsingSystemUnionMarkerWithoutECMAScriptMarker_ThrowsUnsupportedExternalPropertyAccess`
  - `Convert_ClassUsingVueNamesOrOptionsValueProjection_GeneratesNativeValue`
- `dotnet test src\ECMAScript.WebIDL.GeneratorTest\ECMAScript.WebIDL.GeneratorTest.csproj --no-restore --filter 'FullyQualifiedName~PreviewBindingEmitterTests' -v minimal -p:UseSharedCompilation=false -m:1` passed: 25/25 tests.
- `dotnet build src\Jazor.EmitTest\Jazor.EmitTest.csproj -v minimal -p:UseSharedCompilation=false -m:1` passed with 0 warnings and 0 errors after removing the redundant `Microsoft.AspNetCore.Components` package reference covered by `Microsoft.AspNetCore.App`.
- `dotnet build src\Jazor.RazorVue.RazorIr.Test\Jazor.RazorVue.RazorIr.Test.csproj -v minimal -p:UseSharedCompilation=false -m:1` passed with 0 warnings and 0 errors after explicit nullable guards.
- `dotnet build src\Jazor.RazorVue.Test\Jazor.RazorVue.Test.csproj -v minimal -p:UseSharedCompilation=false -m:1` passed with 0 warnings and 0 errors after the same Razor source-document path guard.
- `dotnet build src\Jazor.Analyzer\Jazor.Analyzer.csproj --no-restore -v minimal -p:UseSharedCompilation=false -m:1` passed after making Razor SG IL fingerprint collection best-effort.
- `dotnet test src\Jazor.RazorVue.RazorIr.Test\Jazor.RazorVue.RazorIr.Test.csproj --filter 'FullyQualifiedName~RazorSourceGeneratorCompatibilityProbeTests' -v minimal -p:UseSharedCompilation=false -m:1` passed: 4/4 tests. This covers current SDK ABI probing, non-incremental-generator rejection, unknown `Initialize` IL hash acceptance, and unavailable IL fingerprint acceptance.
- `dotnet test src\Jazor.RazorVue.RazorIr.Test\Jazor.RazorVue.RazorIr.Test.csproj --no-build --filter 'FullyQualifiedName~ExternalBuild_RazorSgIntegrationDisabled_DoesNotEmitTailOutput' -v minimal -p:UseSharedCompilation=false -m:1` passed on CoreCLR 11.
- `dotnet test src\Jazor.RazorVue.RazorIr.Test\Jazor.RazorVue.RazorIr.Test.csproj --no-build --filter 'FullyQualifiedName~ExternalBuild_BootstrapHook_CanObserveOfficialRegisterHostOutput' -v minimal -p:UseSharedCompilation=false -m:1` passed on CoreCLR 11 via fallback trace and tail output generation.
- `dotnet test src\Jazor.RazorVue.RazorIr.Test\Jazor.RazorVue.RazorIr.Test.csproj --no-build --filter 'FullyQualifiedName~RazorSourceGeneratorCompatibilityProbeTests|FullyQualifiedName~RazorSourceGeneratorBootstrapPatchTests|FullyQualifiedName~RazorSdkToolsetProbeTests|FullyQualifiedName~RazorSourceGeneratorHostOutputTests|FullyQualifiedName~RazorSourceGeneratorCarrierBridgeTests' -v minimal -p:UseSharedCompilation=false -m:1` passed: 11/11 tests.
- `dotnet test src\Jazor.CompilerTest\Jazor.CompilerTest.csproj -v minimal -p:UseSharedCompilation=false -m:1` passed: 1878/1878 tests.
- `dotnet test src\Jazor.RazorVue.Test\Jazor.RazorVue.Test.csproj --no-build -v minimal -p:UseSharedCompilation=false -m:1` passed: 605/605 tests.
- `dotnet test src\Jazor.EmitTest\Jazor.EmitTest.csproj --filter 'FullyQualifiedName~Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace|FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts|FullyQualifiedName~Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace' -v minimal -p:UseSharedCompilation=false -m:1` passed: 3/3 tests after Razor 11 attribute-expression fixes and non-null sample text parameters.
- `dotnet test src\Jazor.EmitTest\Jazor.EmitTest.csproj -v minimal -p:UseSharedCompilation=false -m:1` passed: 110/110 tests after the Razor 11 attribute-expression fixes and non-null sample text-parameter cleanup.
- Direct `dotnet build samples\RazorVue.TodoList\Todo.Library\Todo.Library.csproj` is not a valid standalone verification in this checkout without a matching local `0.1.20` package feed. It currently fails restore with `NU1102`; the isolated local-package EmitTest flow is the authoritative sample verification.

## Original Analyzer Blocker

With Roslyn aligned to SDK `5.7`, analyzers loaded correctly and exposed real `Wiki` diagnostics for `VueDictionary<TValue>.this[string]` and `VueEventHandlers<TEvent>.this[string]` object initializer usage.

Resolution: these are plain-object host authoring surfaces, not arbitrary CLR indexer usage. `Jazor.Analyzer` now uses the same object-literal host predicate as compiler lowering and keeps ordinary runtime indexer reads/writes guarded.

## Next Work

1. Continue migrating remaining generated/manual union wrappers only when each target branch set is proven safe for the official-preview contract.
2. Keep object/interface/delegate/nullable-boundary union branches on explicit strong factories or overloads unless C# 15 preview semantics make normal assignment/overload binding sound.
3. Continue monitoring SDK preview changes. Remove the temporary union shim only when the target .NET 11 SDK exposes the official runtime union contract.
4. Re-check Roslyn `IOperation` shape after each SDK update; keep the reflection visitor guard green before accepting a new compiler package.
5. Re-run the full verification lane after the next SDK/Roslyn preview update, including compiler, RazorVue, RazorIr fallback, WebIDL generator, and Emit local-package sample coverage.
