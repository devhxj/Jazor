# RazorVue Library Authoring Design

> Status: active reference
> Positioning: Design reference for the active library-authoring lane in RazorVue.

## 1. Purpose

This document defines how third-party Vue libraries enter RazorVue as first-class authoring surfaces for C# developers.

The design goal is to let a Vue ecosystem library appear as a normal Razor/C# component library while still compiling to standard Vue runtime usage.

## 2. Existing Foundation

RazorVue already provides:

- component descriptors
- component registry and resolution
- props/emits/slots extraction
- library component source kind
- Vue artifact lowering
- manifest emission

This design extends those mechanisms instead of replacing them.

## 3. Core Rule

A library component must have one authoring truth source:

the C# stub type.

The stub serves:

- Razor authoring
- IDE tooling
- descriptor extraction
- validation
- component resolution

Descriptor data must be derived from the stub and its metadata, not maintained as a separate manual truth source.

## 4. Library Component Model

Library components are represented by:

- `VueLibraryComponent`
- `VueLibraryComponentAttribute`
- `VueLibraryStyleAttribute`

A library component is a C# type that declares:

- where the Vue runtime component is imported from
- which export name to use
- which style dependencies are required

## 5. Descriptor Extraction Rules

If a component type has `VueLibraryComponentAttribute`, descriptor extraction must produce:

- `SourceKind = LibraryComponent`
- `ImportSpecifier` from the attribute
- `ExportName` from the attribute
- `StyleDependencies` from `VueLibraryStyleAttribute`

All prop/emit/slot extraction should still reuse the standard RazorVue rules:

- `[Parameter]` -> prop
- `EventCallback` -> emit
- `RenderFragment` -> slot
- `RenderFragment<T>` -> scoped slot parameter

## 6. Discovery Rules

Default registry creation must include:

- intrinsic components
- user components
- discovered library components from `Compilation`

No external registry file should be required for normal usage.

## 7. Resolution Rules

Library components follow the same resolution model as user components:

- fully-qualified name first
- intrinsic names remain reserved
- visibility is controlled by current namespace and `using`
- ambiguity must produce diagnostics, not heuristics

## 8. Binding and Slot Rules

Authoring remains C#-friendly:

- `EventCallback` for events
- `Xxx + XxxChanged` for model-style binding
- `RenderFragment` / `RenderFragment<T>` for scoped slots

Business authors should not write raw Vue event or slot payload shapes.

## 9. Manifest and Host Rules

Library integration must declare runtime requirements explicitly.

At minimum, the host-facing layer must be able to observe:

- imports
- styles
- plugin requirements

The compiler declares them.
The host consumes them.

## 10. Rejected Directions

The following directions are rejected for v1:

- separate manual descriptor truth
- library-specific lowering branches
- raw Vue runtime APIs as the main business authoring surface
- bypassing the standard registry and resolution rules
