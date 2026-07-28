# ECMAScript.TDesign

> Purpose: standalone TDesign Vue Next binding and RazorVue authoring surface.

Conservative first-slice bindings for `tdesign-vue-next`.

Scope of this package:

- Root plugin/runtime host: `TDesign`
- Stable admin-shell component proxies: layout, menu, button, card, space, divider, config provider
- Strongly typed RazorVue authoring surface without `object` catch-all props

Current package contract intentionally avoids weak `TNode`-as-prop abstractions. Rich content is surfaced through verified slots, while plain text use cases stay on string props.

## Boundary

This package defines host bindings and component contracts. Razor SG integration, render-function lowering, and output materialization remain owned by `Jazor.Vue`, `Jazor.RazorVue`, and `Jazor.Emit` respectively.
