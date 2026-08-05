# ECMAScript.ElementPlus

> Purpose: standalone Element Plus binding and RazorVue authoring surface.

Conservative first-slice bindings for `element-plus`.

Scope of this package:

- Root plugin/runtime host: `ElementPlus`
- Stable admin-shell component proxies: config provider, container layout, menu, button, card, link, space, divider
- Strongly typed RazorVue authoring surface without `object` catch-all props

Current package contract intentionally avoids weak catch-all choice wrappers. Rich content is exposed through verified slots, while high-frequency scalar props stay strongly typed.

Public authoring types follow Element Plus component naming: use `El*` (`ElUploadFile`, `ElButtonType`, `ElComponents`). Only the package root host remains `ElementPlus`.

## Boundary

This package defines host bindings and component contracts. Razor SG integration, render-function lowering, and output materialization remain owned by `Jazor.Vue`, `Jazor.RazorVue`, and `Jazor.Emit` respectively.
