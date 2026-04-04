# ECMAScript Host Alignment

This document records the manual mapping rules used by `src/ECMAScript` outside generated bindings.

## Goal

The public C# host surface should stay as close as possible to the JavaScript runtime shape.
When a difference is unavoidable, prefer the smallest host-language escape hatch instead of inventing a new conceptual layer.

In practice, that means:

- Prefer JavaScript runtime names and object boundaries.
- Accept casing differences required by C# naming conventions.
- Use a trailing `_` only when C# name resolution would otherwise collide with an existing type or imported symbol.
- Prefer `IEnumerable<T>` for JavaScript iterable inputs, including locale lists such as `IEnumerable<string>`, unless runtime semantics require a more specific host shape.
- For entry-based inputs such as `Object.fromEntries(...)` or `new Map(...)`, it is acceptable to expose both `IEnumerable<Array<object?>>` and broader `IEnumerable<IEnumerable<object?>>` overloads so common C# sequence families still line up with JavaScript's iterable-of-entry model.
- When ECMA-402 APIs accept JavaScript mathematical values instead of only IEEE double inputs, prefer explicit unions such as `Either<Number, BigInt, string>` over prematurely narrowing the public surface to `Number`.
- When a JavaScript API allows an omitted leading argument but C# cannot express that omission naturally, prefer a direct overload rather than forcing callers to pass a CLR sentinel. A common case is Intl constructors that allow omitting `locales` while still supplying `options`.
- Apply the same rule to instance methods such as `toLocaleString(...)` or `localeCompare(...)` when JavaScript allows omitting the leading `locales` argument but still supplying later options.
- Preserve promise-like assimilation semantics in public signatures when JavaScript APIs await or adopt promise-like inputs.

## Global Host

`ECMAScript.Global` is the host projection of JavaScript `globalThis`.

Global functions and values that are truly exposed on `globalThis` stay there, for example:

- `parseInt`
- `parseFloat`
- `isNaN`
- `isFinite`
- `queueMicrotask`
- `structuredClone`

Constructor-like global functions that collide with C# type names use a trailing underscore:

- `Number_` -> JavaScript `Number`
- `String_` -> JavaScript `String`
- `Boolean_` -> JavaScript `Boolean`
- `BigInt_` -> JavaScript `BigInt`
- `Symbol_` -> JavaScript `Symbol`

This keeps the runtime shape recognizable while avoiding Roslyn ambiguity after `global using static ECMAScript.Global`.

When a JavaScript global constructor/function accepts arbitrary runtime values, the C# projection should not narrow it to a CLR-specific primitive shape unless the runtime really requires that narrower shape.
For example, `Symbol_` should accept `object?` because JavaScript stringifies any non-`undefined` description value at runtime.

## Object Host

`Object.*` static members and `Object.prototype.*` instance members are projected through:

- `Global.extension(object obj)`

This avoids creating an extra CLR-only host such as `JsObject`, which would increase the split between C# and JavaScript.

`IObject` is still kept as the narrow public shape for JavaScript object-like dynamic property access.
It is intentionally not replaced with plain `object`, because `object` is too broad to communicate "JavaScript object with property/index access" in public APIs.

When JavaScript exposes legacy-but-real `Object.prototype` members that C# can spell directly, prefer exposing them under the original runtime name instead of inventing CLR aliases.
Examples include:

- `__proto__`
- `__defineGetter__`
- `__defineSetter__`
- `__lookupGetter__`
- `__lookupSetter__`

Hidden protocol bridge interfaces may still exist for JavaScript symbol-based hooks such as `@@match` or `@@replace`.
Those bridges are implementation details for runtime alignment and should stay hidden unless exposing them directly is the only faithful option.

## Prototype And Inheritance

Prototype-oriented operations remain explicit JavaScript host members:

- `Object.GetPrototypeOf`
- `Object.SetPrototypeOf`
- `Object.Create`
- `Super(...)`

The mapping does not try to reinterpret JavaScript prototype inheritance as CLR inheritance.
Where JavaScript semantics are prototype-based, the public API should say so directly.

Static constructor hosts may also expose their `prototype` object directly when that helps keep the public surface aligned with JavaScript runtime structure.
This is preferable to forcing callers through reflection-like helper layers or omitting the host member entirely.

## Promise-Like Inputs

When a stable JavaScript API explicitly adopts or awaits promise-like values, the C# projection should model that shape directly.

- `Promise.resolve(...)` should preserve promise assimilation through `IPromise` / `IPromise<T>` overloads instead of collapsing everything to `object`.
- `Array.fromAsync(...)` should expose overloads for promise-like source items and async mapping callbacks, because JavaScript awaits both the input items and mapper results.
- Bridge-only overloads using `PromiseResult` may exist for compiler-lowered async code, but they stay hidden from normal editor completion.

## Weak Reference Hosts

Weak-reference-related APIs follow JavaScript runtime rules, not CLR reference-type rules.

- `WeakRef`, `WeakMap`, `WeakSet`, and `FinalizationRegistry` ultimately rely on the JavaScript `CanBeHeldWeakly` rule.
- C# constraints such as `where T : class` are only an approximation to block obvious non-JavaScript shapes such as value types.
- Final validity still belongs to the JavaScript runtime, including cases such as non-global symbols being allowed while ordinary CLR reference types like `string` are still invalid.

## Nullish Policy

The public C# layer exposes only `null` as the no-value surface.

- Public APIs do not model `undefined` as a second visible state.
- When JavaScript would return `undefined`, the public projection generally uses nullable C# types and maps that absence to `null`.
- Internal compiler/runtime layers may still use real JavaScript `undefined` where semantic fidelity requires it.
- For callback parameters such as `thisArg`, comments should describe the JavaScript runtime defaulting behavior without implying that public C# code can observe a separate `undefined` value.

## Constructor Host `prototype`

- When a hand-written mapping models a concrete JavaScript constructor host as a non-generic C# type, expose its `prototype` object directly on that host.
- Use a `Prototype` member with `[Description("@#prototype")]` so the public API still reads like the JavaScript runtime, subject only to normal C# casing rules.
- Do not force `prototype` onto generic CLR projections when that would falsely imply separate runtime constructors per closed generic type.

See [ECMAScript-nullish-semantics.md](./ECMAScript-nullish-semantics.md).

## Intentional Omissions

Some JavaScript members are intentionally not projected when C# cannot represent them faithfully enough:

- `Object.prototype.toString()` on `object`
  - C# instance dispatch on `object` would collide with CLR `object.ToString()` semantics.
- Callable `Object(...)`
  - JavaScript may return boxed wrapper objects whose public shape does not map cleanly to the current C# host model.

In these cases, omission is preferred over exposing a misleading CLR-shaped API.
