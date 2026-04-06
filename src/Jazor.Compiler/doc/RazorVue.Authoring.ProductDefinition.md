# RazorVue Authoring Product Definition

> Status: active reference
> Positioning: Product-definition reference for the active RazorVue authoring lane.

## 1. Purpose

This document defines the authoring product direction for RazorVue.

The goal is to make RazorVue feel natural to C# developers while still compiling to Vue-first runtime output.

## 2. Positioning

RazorVue v1 is:

Blazor-like authoring, Vue-first runtime.

That means:

- authors stay in Razor and C#
- runtime output stays standard Vue ESM
- Vue ecosystem libraries are consumed through C#-friendly wrappers
- the authoring model remains familiar to Blazor users

RazorVue v1 is not:

- a Vue SFC replacement
- a Volar parity project
- a generic multi-framework UI abstraction
- a full Vue Composition API surface for business authors

## 3. Target Users

RazorVue v1 targets developers who:

- work primarily in C# and Razor
- want to keep `.razor + .razor.cs` as the main authoring surface
- want Vue runtime and ecosystem access without switching to TS/SFC workflows
- prefer Blazor-style component authoring semantics

## 4. Core Promise

RazorVue v1 should let authors:

- write components with `[Parameter]`, `EventCallback`, and `RenderFragment`
- use `@bind-*` in a Blazor-like way
- consume selected Vue libraries through C# component wrappers
- generate standard Vue artifacts without manual JS/TS wrapper code

## 5. Authoring Model

The primary authoring model remains:

- `.razor + .razor.cs`
- `[Parameter]`
- `EventCallback` / `EventCallback<T>`
- `RenderFragment` / `RenderFragment<T>`
- `@bind-*`
- familiar lifecycle methods such as `OnInitialized*`, `OnParametersSet*`, and `OnAfterRender*`

Business authors should not need to think in terms of:

- `defineComponent`
- `setup`
- `h`
- raw import specifiers
- host plugin installation details

## 6. Ecosystem Strategy

Vue ecosystem packages should integrate through a stable C#-friendly contract:

`C# stub + descriptor + host requirement`

This means:

- the author sees a normal C# component
- the compiler sees a stable library descriptor
- the host sees explicit runtime requirements

The first ecosystem target for this strategy is Vuetify.

## 7. Experience Goals

RazorVue v1 should prioritize:

- component tag completion
- parameter completion
- parameter type checking
- navigation to component and parameter definitions
- bind target validation
- slot name validation
- slot context validation
- diagnostics phrased from a Razor/C# point of view

## 8. Non-goals

RazorVue v1 does not attempt to:

- match Vue SFC + Volar semantics
- expose the full Vue Composition API to business authors
- support all Razor and Vue syntax and runtime combinations
- solve full style-layer authoring semantics
- deeply integrate multiple ecosystem packages at once

## 9. Runtime Boundary

RazorVue compiler layers are responsible for:

- semantic extraction
- descriptor construction
- render-tree lowering
- Vue artifact generation
- host-facing dependency and plugin requirement declaration

The host is responsible for:

- dependency resolution
- plugin installation
- final bundling
- runtime assembly

## 10. Success Criteria

RazorVue v1 is successful if:

- C# developers can build typical UI flows in Razor
- at least one Vue UI library closes the authoring-to-runtime loop
- generated output remains standard Vue ESM
- business authors do not need to write custom JS/TS wrappers
- design-time diagnostics are early, useful, and C#-oriented
