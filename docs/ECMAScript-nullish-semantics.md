# ECMAScript Nullish Semantics

## Goal

The public C# projection hides JavaScript's `undefined` value from normal user code.
At the C# layer, `null` is the only exposed "no value" representation.

This reduces the semantic split between C# and JavaScript without changing the underlying JavaScript runtime behavior.

## Rule

1. Public C# APIs should not introduce an `Undefined` host value, wrapper type, or public constant.
2. When a JavaScript API returns `undefined` to mean "no value", the C# projection should normally surface that as `null`.
3. Documentation for nullable returns should say that JavaScript may produce `undefined`, and that the C# projection maps that absence to `null`.
4. Internal compiler/runtime layers may still emit or test real JavaScript `undefined` when JavaScript semantics require it.

## Where `undefined` Must Stay Internal

`undefined` is still required in the generated JavaScript for cases such as:

- omitted arguments that must trigger JavaScript default-parameter behavior
- internal placeholders for discarded or omitted values
- presence checks where JavaScript distinguishes `undefined` from an omitted binding
- bridge code that must preserve JavaScript runtime truth exactly

This is an implementation detail. It should not become a public C# host concept.

## API Design Guidance

- Prefer `T?`, `string?`, `object?`, or nullable host objects for JavaScript APIs whose "missing" result is `undefined`.
- Keep comments explicit when nullable is used to absorb JavaScript `undefined`.
- Do not model `undefined` as a second public nullish state next to `null`.
- When an existing compatibility surface must keep a non-nullable indexer, document that the indexer mirrors direct JavaScript property access and that APIs such as `At()` should be preferred for absence-aware reads.

## Presence-Sensitive APIs

Some JavaScript APIs encode "missing" through `undefined`, but also allow stored values that can be confused with a projected `null`.

Examples:

- `Map.get`
- `WeakMap.get`
- property lookup and existence checks

For these APIs, callers should pair value reads with explicit presence checks such as `Has`.
The projection should favor JavaScript's host shape, while the docs explain the C# null-projection behavior and its limits.

## Current Repository Policy

- Public ECMAScript host mappings expose only `null` as the C# no-value surface.
- Compiler and CLR bridge internals may still use real JavaScript `undefined`.
- If a future API needs a stronger distinction than `null` can express, prefer an accompanying protocol such as `Has` over exposing a public `undefined` value.
