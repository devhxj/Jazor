# C# support boundaries and issue index

> Current scope: Jazor 1.3.0. The original audit's observations and probe limits are summarized in the [evolution record](../05-history/evolution.md). This page tracks individual feature boundaries and follow-up work.

The 35 audited bugs are resolved: 11 preserve supported runtime behavior, and 24 now produce explicit source-located unsupported diagnostics. A closed bug does not establish support for its complete CLR/runtime protocol. See the [resolution ledger](./compiler-syntax-bug-fixes.md).

The remaining enhancement and documentation issues require individual product decisions and acceptance evidence. GitHub is authoritative for their live status; an issue title or recommendation is not a support declaration. Ordinary DLL implementation import is tracked separately in [issue #4](https://github.com/devhxj/Jazor/issues/4).

## Scope and prioritization

The inventory separates C# syntax from unavailable .NET APIs and does not establish support for every language-feature combination. Source compilation, analyzer diagnostics and package integration have distinct validation layers. Use the [compiler contract](../../src/Jazor.Compiler/README.md) and targeted regressions to assess a concrete form.

Prioritize high-frequency source/library features with real reproducers, then rendering composition through compiler hooks. Representation changes such as generic reification, value/location carriers, handlers and stack buffers require separate protocol decisions. Keep dynamic binding, unmanaged addresses, finalizers and erased nominal identity within explicit boundaries unless requirements justify expansion.

## Individual issues

### Dynamic binding

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#5 — Define the boundary for constructor calls with dynamic arguments](https://github.com/devhxj/Jazor/issues/5) | `documentation` | Keep unsupported in the current typed compiler. |
| [#6 — Define the boundary for dynamic member reads and writes](https://github.com/devhxj/Jazor/issues/6) | `documentation` | Keep unsupported; typed host contracts cover the intended use cases. |
| [#7 — Define the boundary for dynamic method invocation](https://github.com/devhxj/Jazor/issues/7) | `documentation` | Keep unsupported until runtime C# binding has a justified product scope. |
| [#8 — Define the boundary for dynamic indexer access](https://github.com/devhxj/Jazor/issues/8) | `documentation` | Keep unsupported with a precise typed alternative. |
| [#9 — Reject or correctly bind dynamic binary operators instead of emitting JS coercion](https://github.com/devhxj/Jazor/issues/9) | `bug` — resolved in 1.3.0 | Reject operations requiring the C# runtime binder. |
| [#10 — Reject or implement runtime conversions from dynamic values](https://github.com/devhxj/Jazor/issues/10) | `bug` — resolved in 1.3.0 | Reject runtime-bound conversions; retain erased object/dynamic pass-through. |
| [#130 — Reject or correctly bind unary operators on dynamic operands](https://github.com/devhxj/Jazor/issues/130) | `bug` — resolved in 1.3.0 | Reject runtime binder semantics. |
| [#131 — Reject or correctly bind increment and decrement on dynamic values](https://github.com/devhxj/Jazor/issues/131) | `bug` — resolved in 1.3.0 | Reject dynamic increment/decrement. |
| [#132 — Reject or implement runtime boolean conversion in dynamic conditions](https://github.com/devhxj/Jazor/issues/132) | `bug` — resolved in 1.3.0 | Reject runtime-bound truth/conversion operations. |

### Control flow

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#11 — Assess support for goto label statements](https://github.com/devhxj/Jazor/issues/11) | `enhancement` | Defer general goto; consider a bounded structured lowering only when demanded by imported code. |
| [#12 — Assess support for goto case and goto default](https://github.com/devhxj/Jazor/issues/12) | `enhancement` | Support a bounded switch-local form before general goto if consumer demand warrants it. |
| [#91 — Preserve continue from pattern switch sections to an enclosing loop](https://github.com/devhxj/Jazor/issues/91) | `enhancement` | Worth supporting; this is ordinary structured C# control flow. |
| [#92 — Preserve labeled break and continue across pattern-switch lowering](https://github.com/devhxj/Jazor/issues/92) | `enhancement` | Useful for the selected SDK's labeled loop-control syntax. |
| [#136 — Preserve method returns inside pattern-matching switch statements](https://github.com/devhxj/Jazor/issues/136) | `bug` — resolved in 1.3.0 | Keep returns in the authored function, with switch-scoped pattern bindings. |
| [#137 — Await asynchronous pattern-switch lowering before subsequent statements](https://github.com/devhxj/Jazor/issues/137) | `bug` — resolved in 1.3.0 | Keep await and awaited guards in the authored async function; preserve finally and loop transfers. |

### Other declarations and language boundaries

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#13 — Keep expression-tree lambda materialization explicit across all member body forms](https://github.com/devhxj/Jazor/issues/13) | `bug` — resolved in 1.3.0 | Reject expression-tree conversions instead of emitting a callable function. |
| [#14 — Assess custom interpolated string handler conversions](https://github.com/devhxj/Jazor/issues/14) | `enhancement` | Consider a bounded source-defined handler protocol; ordinary string interpolation is already supported. |
| [#53 — Support nested static helper classes in ECMAScript modules](https://github.com/devhxj/Jazor/issues/53) | `enhancement` | Worth supporting as ordinary source organization. |
| [#54 — Support runtime classes nested inside runtime member classes](https://github.com/devhxj/Jazor/issues/54) | `enhancement` | Useful for library reuse and should follow stable flattening rules. |
| [#55 — Assess ordinary struct declarations and value semantics](https://github.com/devhxj/Jazor/issues/55) | `enhancement` | High value for ordinary libraries, but must be a coherent value-semantics slice. |
| [#57 — Support abstract method slots in source class hierarchies](https://github.com/devhxj/Jazor/issues/57) | `enhancement` | Useful for normal polymorphic source code. |
| [#58 — Support abstract properties in source class hierarchies](https://github.com/devhxj/Jazor/issues/58) | `enhancement` | Useful and should be consistent with abstract method dispatch. |
| [#59 — Erase nested delegate type declarations while preserving delegate calls](https://github.com/devhxj/Jazor/issues/59) | `enhancement` | Worth supporting as a small declaration-level improvement. |
| [#93 — Document the unsupported destructor/finalizer boundary](https://github.com/devhxj/Jazor/issues/93) | `documentation` | Keep CLR finalizers outside the JS artifact contract. |
| [#101 — Assess enum values inside interpolated strings](https://github.com/devhxj/Jazor/issues/101) | `enhancement` | Useful for diagnostics and UI text, through compile-time metadata rather than runtime enum objects. |
| [#102 — Initialize managed array elements with supported C# defaults](https://github.com/devhxj/Jazor/issues/102) | `bug` — resolved in 1.3.0 | Initialize every allocated element; allocate independent structured defaults and evaluate rectangular dimensions once. |
| [#103 — Initialize unassigned static module fields with C# defaults](https://github.com/devhxj/Jazor/issues/103) | `bug` — resolved in 1.3.0 | Reuse compiler-owned typed defaults for fields and backing storage. Initialize all module storage before executing nonconstant authored initializers, preserving forward reads through helper calls. |
| [#108 — Bind expression-bodied checked, null-suppression and ref wrappers without internal exceptions](https://github.com/devhxj/Jazor/issues/108) | `bug` — resolved in 1.3.0 | Obtain the bound arrow-body operation when wrapper syntax has no direct operation. |
| [#133 — Support instance extension methods declared in extension blocks](https://github.com/devhxj/Jazor/issues/133) | `enhancement` | Useful current C# authoring syntax, distinct from classic this-parameter methods. |
| [#135 — Assess assignment to static fields exported by another generated module](https://github.com/devhxj/Jazor/issues/135) | `enhancement` | Useful only with an owner-side mutation protocol; keep explicit rejection until then. |

### Pointers, layout and buffer storage

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#15 — Assess InlineArray element access and storage semantics](https://github.com/devhxj/Jazor/issues/15) | `enhancement` | Defer until a bounded value-storage model is available. |
| [#16 — Document the unsupported address-of operator boundary](https://github.com/devhxj/Jazor/issues/16) | `documentation` | Keep CLR address-of semantics outside the current JS target. |
| [#17 — Report pointer dereference as unsupported instead of throwing NullReferenceException](https://github.com/devhxj/Jazor/issues/17) | `bug` — resolved in 1.3.0 | Report the missing memory-location protocol at the source operation. |
| [#18 — Report unsafe pointer indexing as an explicit unsupported operation](https://github.com/devhxj/Jazor/issues/18) | `bug` — resolved in 1.3.0 | Reject pointer indexing instead of emitting ordinary array access. |
| [#19 — Document the delegate* invocation boundary](https://github.com/devhxj/Jazor/issues/19) | `documentation` | Keep function pointers unsupported; use executable delegates for JS callbacks. |
| [#20 — Give fixed statements a dedicated unsupported diagnostic](https://github.com/devhxj/Jazor/issues/20) | `bug` — resolved in 1.3.0 | Reject CLR pinning and fixed-address semantics. |
| [#21 — Assess stackalloc expressions for pointer and Span targets](https://github.com/devhxj/Jazor/issues/21) | `enhancement` | Consider only an explicitly bounded buffer subset; defer CLR stack identity and pointers. |
| [#22 — Define the sizeof boundary for generic unmanaged and user-defined layouts](https://github.com/devhxj/Jazor/issues/22) | `documentation` | Keep unsupported layouts explicit; support only sizes that can be proven at compile time. |

### Generic runtime operations

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#23 — Assess new T() with constructor-constrained type parameters](https://github.com/devhxj/Jazor/issues/23) | `enhancement` | Useful for reusable libraries, but requires an explicit specialization or constructor-token design. |
| [#24 — Assess default(T) for unconstrained and value-type generic parameters](https://github.com/devhxj/Jazor/issues/24) | `enhancement` | Useful for generic library reuse; support only when the default representation is known. |
| [#25 — Define typeof(T) support under erased generics](https://github.com/devhxj/Jazor/issues/25) | `enhancement` | Defer runtime support until a narrow type-token contract is designed. |
| [#26 — Assess is T and declaration patterns targeting erased type parameters](https://github.com/devhxj/Jazor/issues/26) | `enhancement` | Support only a provable or specialized subset; retain rejection otherwise. |
| [#27 — Assess as T for reference-constrained erased type parameters](https://github.com/devhxj/Jazor/issues/27) | `enhancement` | Allow only when runtime compatibility can be proven or specialized. |
| [#28 — Preserve static storage per closed generic type](https://github.com/devhxj/Jazor/issues/28) | `bug` — resolved in 1.3.0 | Reject source generic static storage that would be shared across closed instantiations. Constants remain compile-time values. |
| [#29 — Resolve or reject static abstract interface method and property dispatch](https://github.com/devhxj/Jazor/issues/29) | `bug` — resolved in 1.3.0 | Reject erased generic runtime implementation dispatch. |
| [#30 — Assess constrained generic operator dispatch for generic math](https://github.com/devhxj/Jazor/issues/30) | `enhancement` | Support a bounded symbol-resolved slice when imported libraries need it. |
| [#134 — Support properties declared in extension blocks](https://github.com/devhxj/Jazor/issues/134) | `enhancement` | Useful typed authoring feature; needs accessor helper lowering. |

### Numeric and boxed-value semantics

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#31 — Support checked numeric-to-char conversions with range validation](https://github.com/devhxj/Jazor/issues/31) | `enhancement` | Worth supporting as a small, explicit numeric semantics slice. |
| [#32 — Preserve overflow behavior of checked integral arithmetic](https://github.com/devhxj/Jazor/issues/32) | `bug` — resolved in 1.3.0 | Evaluate once, check the bound result width, and throw on overflow before write-back. |
| [#33 — Preserve overflow behavior of checked integral narrowing conversions](https://github.com/devhxj/Jazor/issues/33) | `bug` — resolved in 1.3.0 | Truncate floating input, check bounds, and check BigInt narrowing before precision can be lost. |
| [#34 — Preserve fixed-width wraparound in unchecked integral arithmetic](https://github.com/devhxj/Jazor/issues/34) | `bug` — resolved in 1.3.0 | Wrap results to their signed/unsigned width, including compound assignment and updates. |
| [#35 — Lower integral division with C# truncation semantics](https://github.com/devhxj/Jazor/issues/35) | `bug` — resolved in 1.3.0 | Truncate toward zero; preserve operand evaluation order, zero-division failure, signed overflow, and integer positive zero. |
| [#36 — Implement floating-point to integral cast semantics instead of erasing the conversion](https://github.com/devhxj/Jazor/issues/36) | `bug` — resolved in 1.3.0 | Truncate floating fractions and narrow integer carriers correctly; use Roslyn's result for constant conversions. |
| [#107 — Define runtime numeric type discrimination after boxing erasure](https://github.com/devhxj/Jazor/issues/107) | `bug` — resolved in 1.3.0 | Fold provable CLR numeric identities while preserving evaluation; reject ambiguous erased identities. Explicit host Number/BigInt patterns retain JavaScript value-domain checks, including erased host union projections. |

### References and aliasing

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#37 — Preserve aliasing for ref local declarations](https://github.com/devhxj/Jazor/issues/37) | `bug` — resolved in 1.3.0 | Reject persistent storage aliases. |
| [#38 — Preserve alias rebinding in ref assignments](https://github.com/devhxj/Jazor/issues/38) | `bug` — resolved in 1.3.0 | Reject reference-location reassignment. |
| [#39 — Preserve selected storage locations in conditional ref expressions](https://github.com/devhxj/Jazor/issues/39) | `bug` — resolved in 1.3.0 | Reject conditional reference locations. |
| [#40 — Preserve ref-return method and property locations](https://github.com/devhxj/Jazor/issues/40) | `bug` — resolved in 1.3.0 | Reject source ref/ref-readonly returns, including local functions and lambdas. |
| [#41 — Assess by-reference returns from lambdas and delegates](https://github.com/devhxj/Jazor/issues/41) | `enhancement` | Defer until ref-return storage semantics are implemented end to end. |
| [#42 — Preserve aliasing when multiple ref parameters reference the same location](https://github.com/devhxj/Jazor/issues/42) | `bug` — resolved in 1.3.0 | Reject multi-reference calls without provably distinct locations. Preserve existing single-evaluation protocols for disjoint locations. |
| [#45 — Support ref/out arguments in base constructor initializers](https://github.com/devhxj/Jazor/issues/45) | `enhancement` | Defer until the constructor sink protocol spans the inheritance boundary. |
| [#56 — Define support boundaries for ref struct declarations](https://github.com/devhxj/Jazor/issues/56) | `enhancement` | Defer general support until ordinary value and location semantics exist. |

### Construction, inheritance and initialization

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#43 — Support symbol-bound this(...) constructor chaining](https://github.com/devhxj/Jazor/issues/43) | `enhancement` | Useful for ordinary class/library authoring; prioritize with constructor protocol work. |
| [#44 — Support named arguments in base constructor initializers](https://github.com/devhxj/Jazor/issues/44) | `enhancement` | Worth supporting through the existing bound argument infrastructure. |
| [#46 — Assess constructor overload dispatch with ref parameters](https://github.com/devhxj/Jazor/issues/46) | `enhancement` | Useful but dependent on a complete caller/callee constructor protocol. |
| [#47 — Assess constructor overload dispatch with out parameters](https://github.com/devhxj/Jazor/issues/47) | `enhancement` | Useful but dependent on a complete caller/callee constructor protocol. |
| [#48 — Assess constructor overload dispatch with in parameters](https://github.com/devhxj/Jazor/issues/48) | `enhancement` | Useful but dependent on a complete caller/callee constructor protocol. |
| [#49 — Support params parameters in constructor overload dispatch](https://github.com/devhxj/Jazor/issues/49) | `enhancement` | Worth supporting as normal library authoring once argument packing is defined. |
| [#50 — Assess base-qualified field access](https://github.com/devhxj/Jazor/issues/50) | `enhancement` | Useful, but requires symbol-specific field storage rather than JS super property lookup. |
| [#51 — Define inheritance across generated module or host-library boundaries](https://github.com/devhxj/Jazor/issues/51) | `enhancement` | Support only explicit available constructor/instance protocols; ordinary DLL work is related to #4. |
| [#52 — Assess explicit static constructors on modules and runtime classes](https://github.com/devhxj/Jazor/issues/52) | `enhancement` | Useful for library reuse, but requires correct initialization timing. |

### Source-defined operators

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#60 — Support source-defined binary operator declarations and calls](https://github.com/devhxj/Jazor/issues/60) | `enhancement` | Useful for domain and numeric libraries; dispatch must remain symbol-bound. |
| [#61 — Support source-defined unary operator declarations and calls](https://github.com/devhxj/Jazor/issues/61) | `enhancement` | Useful after ordinary source operator emission is established. |
| [#62 — Support source-defined implicit and explicit conversion operators](https://github.com/devhxj/Jazor/issues/62) | `enhancement` | Useful for strongly typed domain models without weakening APIs. |
| [#63 — Support source-defined increment and decrement operators](https://github.com/devhxj/Jazor/issues/63) | `enhancement` | Useful with a complete read/modify/write protocol. |
| [#64 — Support source-defined compound assignments](https://github.com/devhxj/Jazor/issues/64) | `enhancement` | Useful, with bound operator and conversion semantics retained. |
| [#65 — Assess operator true and operator false in conditional expressions](https://github.com/devhxj/Jazor/issues/65) | `enhancement` | Defer until source operator and short-circuit semantics are implemented. |

### Events and delegates

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#66 — Support static field-like event declarations and subscription](https://github.com/devhxj/Jazor/issues/66) | `enhancement` | Useful for library compatibility with explicit ownership and lifecycle. |
| [#67 — Support custom event add/remove accessors](https://github.com/devhxj/Jazor/issues/67) | `enhancement` | Useful for authored observer wrappers and interface implementations. |
| [#68 — Assess virtual and override event accessor dispatch](https://github.com/devhxj/Jazor/issues/68) | `enhancement` | Useful for polymorphic libraries; requires accessor-level dispatch. |
| [#69 — Assess abstract event declarations as contract slots](https://github.com/devhxj/Jazor/issues/69) | `enhancement` | Useful alongside abstract member support; do not allocate fake storage. |
| [#70 — Assess multicast events whose delegates have ref/out/in parameters](https://github.com/devhxj/Jazor/issues/70) | `enhancement` | Defer until multicast invocation can share writable locations across handlers. |
| [#71 — Define the boundary for multicast events with by-reference returns](https://github.com/devhxj/Jazor/issues/71) | `documentation` | Keep unsupported pending a sound general ref-return carrier and a concrete use case. |
| [#72 — Assess delegate combination and removal expressions used as event handlers](https://github.com/devhxj/Jazor/issues/72) | `enhancement` | Useful, but requires full invocation-list identity rather than JS function arithmetic. |
| [#73 — Define delegate equality for field-like event values](https://github.com/devhxj/Jazor/issues/73) | `enhancement` | Support only with invocation-list equality; retain the current null-only boundary otherwise. |
| [#74 — Reject or implement ordinary delegate + and - expressions](https://github.com/devhxj/Jazor/issues/74) | `bug` — resolved in 1.3.0 | Reject multicast combination/removal rather than emitting arithmetic. |
| [#75 — Assess mapped external method groups as event handlers](https://github.com/devhxj/Jazor/issues/75) | `enhancement` | Worth supporting when stable callback/receiver identity can be established. |

### Interface dispatch and erased type identity

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#76 — Support explicit interface method and property implementations](https://github.com/devhxj/Jazor/issues/76) | `bug` — resolved in 1.3.0 | Diagnose missing runtime implementation slots instead of dropping methods or emitting qualified accessor names. |
| [#77 — Resolve default interface method and property implementations](https://github.com/devhxj/Jazor/issues/77) | `bug` — resolved in 1.3.0 | Reject calls to source interface bodies without an emitted runtime slot. |
| [#85 — Define unprovable runtime is checks against erased interfaces](https://github.com/devhxj/Jazor/issues/85) | `documentation` | Keep unresolved checks unsupported; preserve compiler-proven folds. |
| [#86 — Define as casts to erased interfaces when compatibility is not provable](https://github.com/devhxj/Jazor/issues/86) | `documentation` | Keep unresolved casts unsupported under the current contract. |
| [#89 — Define typeof for erased records, tuples, interfaces and internal CLR carriers](https://github.com/devhxj/Jazor/issues/89) | `documentation` | Keep tokens unavailable where the current representation cannot supply a truthful identity. |

### Enumeration, awaiting and cleanup

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#78 — Support using with explicit IDisposable.Dispose implementations](https://github.com/devhxj/Jazor/issues/78) | `enhancement` | Useful for ordinary resource wrappers. |
| [#79 — Support await using with explicit IAsyncDisposable implementations](https://github.com/devhxj/Jazor/issues/79) | `enhancement` | Useful with async resource lifetimes; share explicit-interface binding. |
| [#104 — Lower custom GetEnumerator patterns instead of assuming JS iterability](https://github.com/devhxj/Jazor/issues/104) | `bug` — resolved in 1.3.0 | Reject source collection protocols without JS iterable semantics. |
| [#105 — Lower custom asynchronous foreach enumerator patterns](https://github.com/devhxj/Jazor/issues/105) | `bug` — resolved in 1.3.0 | Reject source async enumerator protocols without an implemented async iterable bridge. |
| [#106 — Honor the C# awaiter pattern for custom awaitable types](https://github.com/devhxj/Jazor/issues/106) | `bug` — resolved in 1.3.0 | Reject source C# awaiter protocols instead of treating arbitrary values as Promises. |

### Deconstruction, slices and patterns

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#80 — Support extension Deconstruct methods in deconstruction assignment](https://github.com/devhxj/Jazor/issues/80) | `enhancement` | Useful and should reuse normal source extension call dispatch. |
| [#81 — Assess custom Deconstruct methods on ordinary structs](https://github.com/devhxj/Jazor/issues/81) | `enhancement` | Useful after a coherent ordinary struct implementation exists. |
| [#82 — Support foreach deconstruction through custom Deconstruct methods](https://github.com/devhxj/Jazor/issues/82) | `enhancement` | Worth supporting; the compiler already supports some body-level custom deconstruction. |
| [#83 — Support source-defined indexers whose parameter is System.Range](https://github.com/devhxj/Jazor/issues/83) | `enhancement` | Useful for shaped collections and library reuse. |
| [#84 — Support Range-based slice accessors in list patterns](https://github.com/devhxj/Jazor/issues/84) | `enhancement` | Useful to complete custom collection list-pattern behavior. |
| [#90 — Define positional patterns that depend on runtime ITuple](https://github.com/devhxj/Jazor/issues/90) | `documentation` | Keep the erased-tuple boundary explicit; use tuple-typed inputs or bound Deconstruct methods. |

### Structural records

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#87 — Document nominal is checks against structural records](https://github.com/devhxj/Jazor/issues/87) | `documentation` | Keep nominal checks unsupported; property and positional patterns are the supported model. |
| [#88 — Document as casts targeting structural records](https://github.com/devhxj/Jazor/issues/88) | `documentation` | Keep unsupported without an explicit runtime identity contract. |
| [#94 — Assess computed properties and custom accessors on structural records](https://github.com/devhxj/Jazor/issues/94) | `enhancement` | Defer arbitrary runtime members unless an explicit structural-helper design preserves authored behavior. |
| [#95 — Assess instance methods on structural records](https://github.com/devhxj/Jazor/issues/95) | `enhancement` | Defer general support until behavior can be expressed without changing the record representation. |
| [#96 — Assess static methods and fields declared on structural records](https://github.com/devhxj/Jazor/issues/96) | `enhancement` | Useful for record-local helpers; can be considered independently of nominal record identity. |
| [#97 — Define value equality for structural records](https://github.com/devhxj/Jazor/issues/97) | `enhancement` | Potentially useful, but requires a declared value comparison contract rather than JS identity. |
| [#98 — Preserve or reject custom record constructor bodies instead of dropping them](https://github.com/devhxj/Jazor/issues/98) | `bug` — resolved in 1.3.0 | Reject executable bodies and constructor initializers instead of discarding their behavior. |
| [#99 — Preserve record struct copy semantics during assignment](https://github.com/devhxj/Jazor/issues/99) | `bug` — resolved in 1.3.0 | Reject mutable record struct values until implicit value-copy semantics exist. Readonly structural records remain available. |
| [#100 — Assess computed string keys in structural object initializers](https://github.com/devhxj/Jazor/issues/100) | `enhancement` | Useful for explicit dictionary-like record contracts; define evaluation and key rules first. |
