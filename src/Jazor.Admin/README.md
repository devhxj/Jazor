# Jazor.Admin

Admin-shell contracts and native RazorVue admin-shell components for `Jazor`.

## Install

```xml
<ItemGroup>
  <PackageReference Include="Jazor" Version="0.1.31" />
  <PackageReference Include="Jazor.Vue" Version="0.1.31" PrivateAssets="all" />
  <PackageReference Include="Jazor.Admin" Version="0.1.31" />
</ItemGroup>
```

Scope of this package:

- Core admin-shell models: navigation, breadcrumb, page actions, layout modes
- Shared authoring base types for admin-shell components
- Native admin-shell components built on shared Vue3 authoring contracts
- Container-contract shell components that can be compile-time injected to concrete implementations
- Stable public contracts that do not leak specific UI-library props

Authoring notes:

- Native Admin shell components stay on the RazorVue user-component path: `ComponentBase + IVueComponent + [ECMAScriptModule(...)]`
- Public shell components also act as container contracts through `IVueContainerComponent`
- Concrete replacements implement `IVueContainerImplementation<TContainer>` and are selected through assembly-level `[VueInject]`
- Shared metadata such as prop/slot overrides belongs to the general Vue authoring layer via `VueProp` / `VueSlot`
- The shared `Logo` slot follows layout semantics in the native shell: `Top` mode feeds the default header, while `Sidebar` / `Mixed` modes feed the default sidebar and do not duplicate the same branding fragment into the default header
- Navigation destinations use two explicit, non-overlapping properties: `Href` emits a raw anchor, while the strongly typed `ECMAScript.VueRoute.RouteLocationRaw` `RouteTarget` emits native `<router-link :to="...">` navigation
- `RouteTarget` takes precedence when both properties are supplied. Non-empty `Href` values are trimmed before emission, and whitespace-only `Href` values are ignored rather than emitted as navigable links
- Sidebar navigation keys are normalized too: blank `AdminNavItem.Key` values are ignored as non-renderable menu entries, and padded keys are trimmed before selected/expanded-state matching, DOM `data-key` emission, and `SelectedKeyChanged` / `ExpandedKeysChanged` callbacks
- Sidebar also builds an effective unique-key tree before render and interaction logic: normalized duplicate keys are first-win, and later conflicting items are dropped rather than producing ambiguous selected/expanded state or duplicate DOM keys
- Native display text is normalized as well: header/page titles, breadcrumb titles, page-action text, and sidebar nav titles are trimmed before render, while whitespace-only values continue to count as absent
- Disabled sidebar branches stay non-expandable even when callers pass their keys through explicit `ExpandedKeys`; native shell state cannot force illegal expanded DOM back onto disabled navigation
- Sidebar expanded-state callbacks are normalized against the current navigation tree, so disabled branches and stale unknown keys are not echoed back through `ExpandedKeysChanged`
- The current native sidebar treats only non-blank `AdminNavItem.Title` values as renderable menu content, so whitespace-only nav items do not produce empty links, buttons, or sidebar regions
- `PageContainer` ignores breadcrumb items with blank `Title` and action items with blank `Text`, so dirty data does not create empty header regions, empty links, or empty action buttons
- `ApplicationFrame` carries controlled application-wide theme, language, and grayscale state across shell and standalone routes without owning application preference storage or localization policy
- Native sidebar layouts expose an accessible, label-customizable shell command that writes through `CollapsedChanged`; collapse remains application-controlled and does not introduce framework-owned persistence
- `Top` layout reuses the same normalized navigation tree and controlled selection/expansion callbacks inside the header navigation slot, so changing layout mode does not remove routing access or fork navigation semantics
- Tables, forms, notices, authentication fields, error pages, and other concrete page content are intentionally outside this package; applications compose those features inside the shell/page mechanisms
- Native shell slots are normalized by effective content, not just non-null delegates: empty, whitespace-only, or comment-only `Logo` / `Header` / `Sidebar` / `Actions` / `UserRegion` / `Extra` fragments are treated as absent, so they do not create empty wrappers

Verified baseline:

- Default registry resolution keeps `AdminLayout`, `SidebarMenu`, `HeaderBar`, and `PageContainer` on the native `Jazor.Admin` implementation path
- `[VueInject]` replacement is regression-covered for all four public shell components
- Injected runtime shape lowering is verified on the current RazorVue `.mjs` pipeline by `src/JazorAdmin/InjectSmoke`: the packaged build replaces `PageContainer`, emits a default implementation import, applies implementation prop/slot names, and mounts the result in a real browser
- Multi-shell composition is regression-covered on the current Vue render-function `.mjs` pipeline, including cross-container import aggregation, slot/prop/model mapping, and runtime stability
- Native route-target semantics are regression-covered at the source/component contract level and by JazorAdmin's current render-function `.mjs` artifacts; the real-browser lane exercises nested Vue Router navigation, deep links, history traversal, and recovery navigation
- Resolver normalization also regression-covers leading/trailing whitespace on non-empty `href/path/name/hash` values, so native shell output does not leak padded DOM `href` values or padded route-object fields
- Sidebar key normalization is regression-covered for blank-key filtering, selected/expanded-state matching against trimmed keys, DOM `data-key` trimming, and normalized `SelectedKeyChanged` / `ExpandedKeysChanged` payloads
- Sidebar duplicate-key handling is regression-covered for first-win effective-tree construction, duplicate DOM-key suppression, selection/expansion isolation, and blocked callbacks from dropped duplicate items
- Display-text normalization is regression-covered for trimmed header titles/subtitles, page titles/subtitles, breadcrumb/action text, and sidebar nav-item titles
- Disabled sidebar branch behavior is regression-covered for both derived navigability and explicit-expanded-state suppression
- Sidebar expanded-state writeback is regression-covered for invalid explicit keys and current-tree normalization
- Sidebar/admin-layout empty-content semantics are regression-covered for null nav items and whitespace-only nav-item titles
- Page-container header semantics are regression-covered for null entries, whitespace-only breadcrumb/action entries, and navigable/disabled target branches
- The JazorAdmin dogfood smoke consumes the packaged application frame and strongly typed admin models while implementing its concrete TDesign shell locally; its independent InjectSmoke companion verifies replacement against the native `PageContainer` contract.
- Empty-shell slot normalization is regression-covered for zero-frame, whitespace-only, and comment-only `RenderFragment` inputs, and visible slot content is captured once then replayed so presence checks do not double-execute user fragments
- Container compatibility failures are regression-covered for missing props, prop type mismatch, emit payload mismatch, default-slot mismatch, `CaptureUnmatchedValues` mismatch, duplicate registrations, and mismatched `IVueContainerImplementation<TContainer>` contracts

This package is the semantic and implementation core. Third-party UI-library cooperation belongs in samples or app-level composition, not in `Jazor.Admin.*` adapter packages.
