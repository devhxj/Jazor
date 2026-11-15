# WebIDL Inventory

- Generated: `2026-09-11T01:50:42.8860000+00:00`
- Files: `332`
- Declarations: `3639`
- Event targets: `152`

## Sources

- Parser: `webidl2@24.5.0`
- WebRef IDL: `@webref/idl@3.83.1`
- WebRef CSS: `@webref/css@8.7.4`
- WebRef Events: `@webref/events@1.25.1`
- WebRef XRef: `@webref/xref@1.2.16`

## Declaration Kinds

- `callback`: `75`
- `callback interface`: `3`
- `dictionary`: `1105`
- `enum`: `397`
- `includes`: `271`
- `interface`: `1495`
- `interface mixin`: `126`
- `namespace`: `19`
- `typedef`: `148`

## Next Step

This inventory is the stable interchange format between the NodeApi collection layer and the C# binding emitter.
The preview emitter writes typedef, enum, callback, callback interface, dictionary, interface, and namespace bindings under `webidl/`.
When WebRef XRef can match a declaration, member, or argument to a specification definition, the inventory also carries the source anchor, heading, source-authored prose, and available specification usage expressions for XML documentation emission.
