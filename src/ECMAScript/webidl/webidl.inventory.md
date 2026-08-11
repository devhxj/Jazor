# WebIDL Inventory

- Generated: `2026-08-11T15:10:06.4890000+00:00`
- Files: `330`
- Declarations: `3578`
- Event targets: `149`

## Sources

- Parser: `webidl2@24.5.0`
- WebRef IDL: `@webref/idl@3.82.0`
- WebRef CSS: `@webref/css@8.7.1`
- WebRef Events: `@webref/events@1.24.2`
- WebRef XRef: `@webref/xref@1.2.11`

## Declaration Kinds

- `callback`: `75`
- `callback interface`: `3`
- `dictionary`: `1063`
- `enum`: `394`
- `includes`: `270`
- `interface`: `1480`
- `interface mixin`: `126`
- `namespace`: `19`
- `typedef`: `148`

## Next Step

This inventory is the stable interchange format between the Deno collection layer and the C# binding emitter.
The preview emitter writes typedef, enum, callback, callback interface, dictionary, interface, and namespace bindings under `webidl/`.
When WebRef XRef can match a declaration, member, or argument to a specification definition, the inventory also carries the source anchor, heading, source-authored prose, and available specification usage expressions for XML documentation emission.
