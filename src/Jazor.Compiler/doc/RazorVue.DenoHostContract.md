# RazorVue DenoHost Contract

## 1. Purpose

This document defines the contract between RazorVue compiler output and `DenoHost`.

Its goal is to make build ownership explicit:

- RazorVue compiler owns semantic extraction and Vue artifact generation
- `DenoHost` owns dependency resolution, unified compilation, bundling, and later runtime-facing behaviors

This document is intentionally narrower than `RazorVue.Design.md`.
It only covers host-facing artifact and manifest expectations.

Related documents:

- [RazorVue.Design.md](./RazorVue.Design.md)
- [RazorVue.ImplementationChecklist.md](./RazorVue.ImplementationChecklist.md)
- [RazorVue.ComponentDescriptorSpec.md](./RazorVue.ComponentDescriptorSpec.md)

## 2. Core Boundary

RazorVue compiler is responsible for:

- discovering valid RazorVue components
- extracting contracts and logic
- lowering to Vue component artifacts
- declaring imports/styles/runtime hints
- emitting manifest-ready metadata

`DenoHost` is responsible for:

- resolving declared dependencies
- compiling and bundling modules
- orchestrating final output
- later HMR and sourcemap host behavior

This separation must remain stable.

## 3. Artifact Model

The compiler should not hand `DenoHost` only a raw JS string.

It should produce a structured artifact model such as:

```csharp
public sealed record VueCompiledArtifact(
    string ComponentName,
    string RelativeModulePath,
    string ModuleCode,
    ImmutableArray<VueImportRequirement> Imports,
    ImmutableArray<VueStyleRequirement> Styles,
    VueArtifactIdentity Identity,
    VueRuntimeHints Hints);
```

The exact type shape may evolve, but the categories above should remain stable.

Compiler-internal recommendation:

- `RazorVueSemanticSnapshot` is the semantic carrier before lowering
- `VueCompiledArtifact` is the lowering result
- `RazorVueCatalog` is the host-facing materialized carrier

Do not collapse these three concerns into one ad hoc string payload.

## 4. Required Artifact Fields

### 4.1 `ComponentName`

Stable human-readable component name.

Used for:

- diagnostics
- manifest readability
- tooling

### 4.2 `RelativeModulePath`

The compiler-owned relative output path for the component module.

This path must be:

- deterministic
- stable across equivalent builds
- independent from bundler-specific file naming

### 4.3 `ModuleCode`

The Vue ESM module content emitted by the compiler.

Phase one target:

- `defineComponent`
- `setup`
- render function
- standard ESM imports/exports

### 4.4 `Imports`

Declared module import requirements.

These are not resolved by the compiler.
They are declared for `DenoHost`.

Examples:

- `vue`
- `vuetify/components`
- `vue-router`
- project-local generated component paths

### 4.5 `Styles`

Declared style dependencies or style-related requirements.

Examples:

- CSS package dependency
- library style requirement
- theme-related package requirement

### 4.6 `Identity`

Reserved identity metadata for future HMR and stable change tracking.

Recommended fields:

```csharp
public sealed record VueArtifactIdentity(
    string ComponentId,
    string ModuleId,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    HmrBoundaryKind HmrBoundaryKind);
```

### 4.7 `Hints`

Runtime/build hints for host-side orchestration.

Recommended fields:

```csharp
public sealed record VueRuntimeHints(
    bool RequiresVueRuntime,
    bool RequiresHydration,
    bool SupportsSsr,
    bool UsesTeleport,
    bool UsesSuspense,
    bool UsesKeepAlive);
```

Phase one may keep hints small, but the category should exist.

### 4.8 `SourceOrigins` or `OriginMapPath`

Phase one does not require final sourcemap emit,
but artifacts must be able to carry or reference compiler-owned source-origin metadata.

Recommended options:

- `ImmutableArray<RazorVueSourceOrigin> SourceOrigins`
- `string? OriginMapPath`

Either approach is acceptable in phase one.
What is not acceptable is losing the origin chain before host handoff.

## 5. Manifest Model

`DenoHost` should consume a compiler-owned manifest derived from artifacts.

Recommended shape:

```csharp
public sealed record RazorVueManifest(
    string AssemblyName,
    ImmutableArray<RazorVueManifestEntry> Modules);

public sealed record RazorVueManifestEntry(
    string ComponentName,
    string RelativeModulePath,
    ImmutableArray<string> Imports,
    ImmutableArray<string> Styles,
    string DescriptorHash,
    string TemplateHash,
    string LogicHash,
    string ContentHash,
    HmrBoundaryKind HmrBoundaryKind,
    bool RequiresHydration,
    bool SupportsSsr);
```

The exact wire format can later be JSON, generated source, or another host-consumable carrier,
but the logical contract should remain this explicit.

## 6. Dependency Resolution Rule

The compiler declares dependencies.
`DenoHost` resolves them.

This means:

- compiler does not become a package resolver
- compiler does not decide final bundling topology
- compiler should not silently rewrite host dependency policy

`ImportSpecifier` and style requirements are host-facing declarations, not host-independent guarantees.

## 7. Physical Materialization Rule

Analyzer is the semantic extraction entry point,
but analyzer is not the final artifact writer.

Therefore the full chain should be interpreted as:

1. analyzer extracts semantics
2. compiler-owned lowering builds structured artifacts
3. a later build-facing emission stage materializes those artifacts
4. `DenoHost` consumes the materialized outputs

This distinction must remain explicit in implementation and documentation.

Recommended phase-one chain:

1. analyzer validates RazorVue entry and misuse patterns
2. compiler-owned extraction builds `RazorVueSemanticSnapshot`
3. lowering builds `VueCompiledArtifact`
4. emission materializes `RazorVueCatalog` plus manifest/sidecar outputs
5. `DenoHost` consumes those outputs

## 7.1 Migration compatibility with current `ModuleCatalog`

The repository already ships a plain module flow where generated source embeds module metadata into the target assembly.

Phase one RazorVue should define a compatible migration path instead of replacing that flow wholesale.

Recommended migration shape:

- keep `ModuleCatalog` for plain static modules
- add `RazorVueCatalog` for Vue component artifacts, or add a versioned superset contract
- update host/emission code to consume both shapes during transition

This reduces delivery risk and avoids forcing a full host rewrite before the first RazorVue path is proven.

## 8. HMR Contract Reservation

Phase one does not need complete HMR behavior,
but the host contract must already support it.

That requires:

- stable `ComponentId`
- stable `ModuleId`
- separate descriptor/template/logic hashes
- a declared `HmrBoundaryKind`

Recommended boundary enum:

```csharp
public enum HmrBoundaryKind
{
    Unknown,
    TemplateOnly,
    LogicSafe,
    FullReloadRequired
}
```

The compiler does not need to implement runtime HMR yet,
but it must hand `DenoHost` enough information to do so later.

## 9. SourceMap Contract Reservation

Phase one does not need complete sourcemap emission,
but the host contract must already preserve the path toward it.

That means artifacts should be able to carry or reference:

- source-origin metadata
- later source-map build outputs
- stable module identity for map association

The compiler must not force `DenoHost` to infer source origins from final JS text alone.

Recommended sidecar model:

```csharp
public sealed record RazorVueSourceOriginMap(
    string ComponentId,
    string ModuleId,
    ImmutableArray<RazorVueSourceOriginEntry> Entries);

public sealed record RazorVueSourceOriginEntry(
    RazorVueOriginKind OriginKind,
    string SourceFilePath,
    int SourceSpanStart,
    int SourceSpanLength,
    int StartLine,
    int StartColumn,
    string? GeneratedFilePath,
    int? GeneratedSpanStart,
    int? GeneratedSpanLength,
    RazorVueMappingQuality MappingQuality);
```

Phase one does not require this exact wire shape,
but it does require:

- path to the original `.razor` file when known
- stable spans or stable segment identity
- generated fallback location when exact source is unavailable
- explicit mapping quality

## 10. Validation Expectations

`DenoHost` should be able to assume the following from compiler output:

- component/module identity is deterministic
- imports/styles are explicitly declared
- contract-level validation already happened upstream
- manifest entries correspond to valid compiler-owned artifacts

`DenoHost` should not be required to:

- rediscover props/emits/slots
- reinterpret Razor template semantics
- reconstruct component contracts

## 11. Phase-one Minimum Contract

Phase one only requires:

- stable Vue ESM artifact emission
- explicit imports/styles
- deterministic relative module path
- split identity/hash data
- minimal runtime hints
- host-consumable manifest

Phase one does not require:

- complete HMR implementation
- complete sourcemap output
- advanced host-side optimization metadata

## 12. Conclusion

The RazorVue / `DenoHost` contract should be treated as a first-class design boundary.

If the compiler emits only ad hoc strings, or if `DenoHost` has to rediscover semantic information,
the system will immediately lose stability in:

- build ownership
- diagnostics
- HMR evolution
- sourcemap evolution

Structured artifacts and a compiler-owned manifest are the required contract surface.
