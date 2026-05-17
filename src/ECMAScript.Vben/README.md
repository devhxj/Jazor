# ECMAScript.Vben

Admin-shell contracts and native RazorVue admin-shell components for `Jazor`.

Scope of this package:

- Core admin-shell models: navigation, breadcrumb, page actions, layout modes
- Shared authoring base types for admin-shell components
- Native admin-shell components built on shared Vue3 authoring contracts
- Container-contract shell components that can be compile-time injected to concrete implementations
- Stable public contracts that do not leak specific UI-library props

Authoring notes:

- Native Vben shell components stay on the RazorVue user-component path: `ComponentBase + IVueComponent + [ECMAScriptModule(...)]`
- Public shell components also act as container contracts through `IVueContainerComponent`
- Concrete replacements implement `IVueContainerImplementation<TContainer>` and are selected through assembly-level `[VueInject]`
- Shared metadata such as prop/slot overrides belongs to the general Vue authoring layer via `VueProp` / `VueSlot`
- The shared `Logo` slot follows layout semantics in the native shell: `Top` mode feeds the default header, while `Sidebar` / `Mixed` modes feed the default sidebar and do not duplicate the same branding fragment into the default header
- `VbenNavTarget` is intentionally split by semantics: `string` targets stay on raw anchor `href`, while `VbenRouteLocation` targets lower to native `<router-link :to="...">` navigation
- For `VbenRouteLocation`, the native path uses `Path` first, then `Name`, and always normalizes `Hash` to a leading `#`; non-empty `href/path/name/hash` values are trimmed before emission, and whitespace-only targets are ignored rather than emitted as navigable links
- Sidebar navigation keys are normalized too: blank `VbenNavItem.Key` values are ignored as non-renderable menu entries, and padded keys are trimmed before selected/expanded-state matching, DOM `data-key` emission, and `SelectedKeyChanged` / `ExpandedKeysChanged` callbacks
- Disabled sidebar branches stay non-expandable even when callers pass their keys through explicit `ExpandedKeys`; native shell state cannot force illegal expanded DOM back onto disabled navigation
- Sidebar expanded-state callbacks are normalized against the current navigation tree, so disabled branches and stale unknown keys are not echoed back through `ExpandedKeysChanged`
- The current native sidebar treats only non-blank `VbenNavItem.Title` values as renderable menu content, so whitespace-only nav items do not produce empty links, buttons, or sidebar regions
- `VbenPageContainer` ignores breadcrumb items with blank `Title` and action items with blank `Text`, so dirty data does not create empty header regions, empty links, or empty action buttons
- Native shell slots are normalized by effective content, not just non-null delegates: empty, whitespace-only, or comment-only `Logo` / `Header` / `Sidebar` / `Actions` / `UserRegion` / `Extra` fragments are treated as absent, so they do not create empty wrappers

Verified baseline:

- Default registry resolution keeps `VbenAdminLayout`, `VbenSidebarMenu`, `VbenHeaderBar`, and `VbenPageContainer` on the native `ECMAScript.Vben` implementation path
- `[VueInject]` replacement is regression-covered for all four public shell components
- Injected runtime shape lowering is regression-covered at both Vue SFC and pipeline artifact levels for all four public Vben shell components
- Multi-shell composition is regression-covered at both Vue SFC and pipeline artifact levels, including cross-container import aggregation, style/plugin dependency aggregation, and nested slot/prop/model mapping stability
- Native route-target semantics are regression-covered at three layers: render-tree behavior for `VbenSidebarMenu` / `VbenPageContainer`, resolver normalization for `Path` / `Name` / `Hash`, and direct `router-link` lowering probes at Vue SFC and pipeline artifact levels
- Resolver normalization also regression-covers leading/trailing whitespace on non-empty `href/path/name/hash` values, so native shell output does not leak padded DOM `href` values or padded route-object fields
- Sidebar key normalization is regression-covered for blank-key filtering, selected/expanded-state matching against trimmed keys, DOM `data-key` trimming, and normalized `SelectedKeyChanged` / `ExpandedKeysChanged` payloads
- Disabled sidebar branch behavior is regression-covered for both derived navigability and explicit-expanded-state suppression
- Sidebar expanded-state writeback is regression-covered for invalid explicit keys and current-tree normalization
- Sidebar/admin-layout empty-content semantics are regression-covered for null nav items and whitespace-only nav-item titles
- Page-container header semantics are regression-covered for null entries, whitespace-only breadcrumb/action entries, and navigable/disabled target branches
- Empty-shell slot normalization is regression-covered for zero-frame, whitespace-only, and comment-only `RenderFragment` inputs, and visible slot content is captured once then replayed so presence checks do not double-execute user fragments
- Container compatibility failures are regression-covered for missing props, prop type mismatch, emit payload mismatch, default-slot mismatch, `CaptureUnmatchedValues` mismatch, duplicate registrations, and mismatched `IVueContainerImplementation<TContainer>` contracts
- A real sample now exists under `samples/ECMAScript.Vben.ElementPlusInject/`, proving Razor-authored Vben shell composition, sample-local `Element Plus` container injection, official `razorvue-consumer-entry` bridge generation, and Deno-only SSR/browser/bundle smoke verification

This package is the semantic and implementation core. Third-party UI-library cooperation belongs in samples or app-level composition, not in `ECMAScript.Vben.*` adapter packages.
