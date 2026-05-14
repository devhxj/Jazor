# ECMAScript.ElementPlus

Conservative first-slice bindings for `element-plus`.

Scope of this package:

- Root plugin/runtime host: `ElementPlus`
- Stable admin-shell component proxies: config provider, container layout, menu, button, card, link, space, divider
- Strongly typed RazorVue authoring surface without `object` catch-all props

Current package contract intentionally avoids weak catch-all choice wrappers. Rich content is exposed through verified slots, while high-frequency scalar props stay strongly typed.
