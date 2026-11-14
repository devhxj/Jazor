# C# and RazorVue syntax gap audit

> Audited on November 14, 2026, against commit `9f0bb79c` (after 1.2.0). This is an issue index and support assessment; no compiler capability was implemented or released by this audit.

The audit produced **133 separate English GitHub issues**, one per identified feature or authoring context. Each issue explains the current failure, why the implementation cannot preserve the semantics, whether support is worthwhile, and feature-specific acceptance criteria. No umbrella issue replaces these individual work items.

| Classification | Issues | Meaning |
| --- | ---: | --- |
| `bug` | 35 | Incorrect emitted behavior, invalid JavaScript, or an internal/inconsistent diagnostic; repair or explicitly reject the unsupported form. |
| `enhancement` | 82 | Evaluate a bounded implementation; the issue states whether to prioritize or defer support. |
| `documentation` | 16 | Retain an intentional boundary and make its rationale, diagnostics and alternatives clear. |

## Evidence and scope

The audit reviewed explicit rejection paths in `Jazor.Compiler` and `Jazor.RazorVue`, checked 160 core C# samples and 16 isolated render-body samples, and compared 35 selected emitted programs with their managed execution. Invalid C# samples were excluded from issue reproducers. Additional component activation and conditional-slot boundaries cite existing source/regression fixtures rather than claiming new runtime coverage.

Core probes called `AstConverter` after successful Roslyn binding; render probes called `RenderEmitter`. They intentionally isolate lowering from analyzer/MSBuild/package integration. A successful emit is not a claim that all production checks accept a program. JavaScript runtime probes used in-process NodeApi/LibNode on Windows with .NET SDK 11.0.100 and the repository's matching Roslyn preview.

Examples of observed mismatches include `ref` locals losing aliases, checked overflow not throwing, closed generic types sharing static fields, integral division returning a fraction, custom record constructors being skipped, and custom awaiters/enumerators being mistaken for native JS protocols. These are listed as bugs, not as supported behavior.

The inventory distinguishes language features from unavailable .NET APIs. It excludes VB-only operations, Roslyn CFG/internal placeholders, invalid C# input, unsupported external API inventories, and malformed RenderTreeBuilder frames. Ordinary third-party DLL implementation import is tracked independently in [issue #4](https://github.com/devhxj/Jazor/issues/4). This audit does not establish support for every combination in the C# language specification.

Successful controls included ordinary method overloads, basic array rank/index lowering, tuples, record `with`, ordinary `using`, `yield`, the current synchronous `lock` contract, the `field` keyword and ordinary class primary constructors with base arguments. Old comments claiming that all events or all multidimensional arrays are unsupported were not used as evidence.

## Suggested work order

1. Fix silent semantic changes and invalid output, or introduce deliberate usage-site rejection where full support is outside scope.
2. Implement high-frequency source/library syntax: constructor chains and arguments, source operators, abstract members, explicit/default interface dispatch, custom deconstruction and enumeration.
3. Complete Razor rendering composition through compiler hooks, especially switch, local computations and control-flow/cleanup around content.
4. Evaluate expensive representation changes separately: generic reification, value/location carriers, handlers, inline arrays and stack buffers. Keep dynamic binding, unmanaged addresses, finalizers and erased nominal identity within explicit boundaries unless requirements change.

## Individual issues

### Dynamic binding

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#5 — Define the boundary for constructor calls with dynamic arguments](https://github.com/devhxj/Jazor/issues/5) | `documentation` | Keep unsupported in the current typed compiler. |
| [#6 — Define the boundary for dynamic member reads and writes](https://github.com/devhxj/Jazor/issues/6) | `documentation` | Keep unsupported; typed host contracts cover the intended use cases. |
| [#7 — Define the boundary for dynamic method invocation](https://github.com/devhxj/Jazor/issues/7) | `documentation` | Keep unsupported until runtime C# binding has a justified product scope. |
| [#8 — Define the boundary for dynamic indexer access](https://github.com/devhxj/Jazor/issues/8) | `documentation` | Keep unsupported with a precise typed alternative. |
| [#9 — Reject or correctly bind dynamic binary operators instead of emitting JS coercion](https://github.com/devhxj/Jazor/issues/9) | `bug` | Fix the silent semantic mismatch; full dynamic support is not required. |
| [#10 — Reject or implement runtime conversions from dynamic values](https://github.com/devhxj/Jazor/issues/10) | `bug` | Fix by making the dynamic conversion boundary explicit. |
| [#130 — Reject or correctly bind unary operators on dynamic operands](https://github.com/devhxj/Jazor/issues/130) | `bug` | Fix silent dynamic coercion; full runtime binding remains optional. |
| [#131 — Reject or correctly bind increment and decrement on dynamic values](https://github.com/devhxj/Jazor/issues/131) | `bug` | Fix the accepted semantic mismatch. |
| [#132 — Reject or implement runtime boolean conversion in dynamic conditions](https://github.com/devhxj/Jazor/issues/132) | `bug` | Fix silent JS truthiness in place of C# dynamic boolean binding. |

### Control flow

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#11 — Assess support for goto label statements](https://github.com/devhxj/Jazor/issues/11) | `enhancement` | Defer general goto; consider a bounded structured lowering only when demanded by imported code. |
| [#12 — Assess support for goto case and goto default](https://github.com/devhxj/Jazor/issues/12) | `enhancement` | Support a bounded switch-local form before general goto if consumer demand warrants it. |
| [#91 — Preserve continue from pattern switch sections to an enclosing loop](https://github.com/devhxj/Jazor/issues/91) | `enhancement` | Worth supporting; this is ordinary structured C# control flow. |
| [#92 — Preserve labeled break and continue across pattern-switch lowering](https://github.com/devhxj/Jazor/issues/92) | `enhancement` | Useful for the selected SDK's labeled loop-control syntax. |
| [#136 — Preserve method returns inside pattern-matching switch statements](https://github.com/devhxj/Jazor/issues/136) | `bug` | Required correctness for ordinary accepted control flow. |
| [#137 — Await asynchronous pattern-switch lowering before subsequent statements](https://github.com/devhxj/Jazor/issues/137) | `bug` | Required correctness for accepted await and pattern-switch composition. |

### Other declarations and language boundaries

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#13 — Keep expression-tree lambda materialization explicit across all member body forms](https://github.com/devhxj/Jazor/issues/13) | `bug` | Keep symbolic expression trees unsupported, and close the inconsistent expression-body path. |
| [#14 — Assess custom interpolated string handler conversions](https://github.com/devhxj/Jazor/issues/14) | `enhancement` | Consider a bounded source-defined handler protocol; ordinary string interpolation is already supported. |
| [#53 — Support nested static helper classes in ECMAScript modules](https://github.com/devhxj/Jazor/issues/53) | `enhancement` | Worth supporting as ordinary source organization. |
| [#54 — Support runtime classes nested inside runtime member classes](https://github.com/devhxj/Jazor/issues/54) | `enhancement` | Useful for library reuse and should follow stable flattening rules. |
| [#55 — Assess ordinary struct declarations and value semantics](https://github.com/devhxj/Jazor/issues/55) | `enhancement` | High value for ordinary libraries, but must be a coherent value-semantics slice. |
| [#57 — Support abstract method slots in source class hierarchies](https://github.com/devhxj/Jazor/issues/57) | `enhancement` | Useful for normal polymorphic source code. |
| [#58 — Support abstract properties in source class hierarchies](https://github.com/devhxj/Jazor/issues/58) | `enhancement` | Useful and should be consistent with abstract method dispatch. |
| [#59 — Erase nested delegate type declarations while preserving delegate calls](https://github.com/devhxj/Jazor/issues/59) | `enhancement` | Worth supporting as a small declaration-level improvement. |
| [#93 — Document the unsupported destructor/finalizer boundary](https://github.com/devhxj/Jazor/issues/93) | `documentation` | Keep CLR finalizers outside the JS artifact contract. |
| [#101 — Assess enum values inside interpolated strings](https://github.com/devhxj/Jazor/issues/101) | `enhancement` | Useful for diagnostics and UI text, through compile-time metadata rather than runtime enum objects. |
| [#102 — Initialize managed array elements with supported C# defaults](https://github.com/devhxj/Jazor/issues/102) | `bug` | Required correctness for accepted array creation, including rectangular arrays. |
| [#103 — Initialize unassigned static module fields with C# defaults](https://github.com/devhxj/Jazor/issues/103) | `bug` | Required correctness for accepted module storage. |
| [#108 — Bind expression-bodied checked, null-suppression and ref wrappers without internal exceptions](https://github.com/devhxj/Jazor/issues/108) | `bug` | Fix semantic-root extraction; retain each operation's own support boundary. |
| [#133 — Support instance extension methods declared in extension blocks](https://github.com/devhxj/Jazor/issues/133) | `enhancement` | Useful current C# authoring syntax, distinct from classic this-parameter methods. |
| [#135 — Assess assignment to static fields exported by another generated module](https://github.com/devhxj/Jazor/issues/135) | `enhancement` | Useful only with an owner-side mutation protocol; keep explicit rejection until then. |

### Pointers, layout and buffer storage

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#15 — Assess InlineArray element access and storage semantics](https://github.com/devhxj/Jazor/issues/15) | `enhancement` | Defer until a bounded value-storage model is available. |
| [#16 — Document the unsupported address-of operator boundary](https://github.com/devhxj/Jazor/issues/16) | `documentation` | Keep CLR address-of semantics outside the current JS target. |
| [#17 — Report pointer dereference as unsupported instead of throwing NullReferenceException](https://github.com/devhxj/Jazor/issues/17) | `bug` | Keep pointer dereference unsupported and fix diagnostic correctness. |
| [#18 — Report unsafe pointer indexing as an explicit unsupported operation](https://github.com/devhxj/Jazor/issues/18) | `bug` | Keep native pointer indexing unsupported; replace the internal exception. |
| [#19 — Document the delegate* invocation boundary](https://github.com/devhxj/Jazor/issues/19) | `documentation` | Keep function pointers unsupported; use executable delegates for JS callbacks. |
| [#20 — Give fixed statements a dedicated unsupported diagnostic](https://github.com/devhxj/Jazor/issues/20) | `bug` | Keep pinning semantics unsupported and improve the diagnostic. |
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
| [#28 — Preserve static storage per closed generic type](https://github.com/devhxj/Jazor/issues/28) | `bug` | Required correctness for accepted generic source classes. |
| [#29 — Resolve or reject static abstract interface method and property dispatch](https://github.com/devhxj/Jazor/issues/29) | `bug` | Required for correctness; add a bounded implementation before advertising generic-math reuse. |
| [#30 — Assess constrained generic operator dispatch for generic math](https://github.com/devhxj/Jazor/issues/30) | `enhancement` | Support a bounded symbol-resolved slice when imported libraries need it. |
| [#134 — Support properties declared in extension blocks](https://github.com/devhxj/Jazor/issues/134) | `enhancement` | Useful typed authoring feature; needs accessor helper lowering. |

### Numeric and boxed-value semantics

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#31 — Support checked numeric-to-char conversions with range validation](https://github.com/devhxj/Jazor/issues/31) | `enhancement` | Worth supporting as a small, explicit numeric semantics slice. |
| [#32 — Preserve overflow behavior of checked integral arithmetic](https://github.com/devhxj/Jazor/issues/32) | `bug` | Required correctness for accepted checked arithmetic. |
| [#33 — Preserve overflow behavior of checked integral narrowing conversions](https://github.com/devhxj/Jazor/issues/33) | `bug` | Required correctness for accepted checked casts. |
| [#34 — Preserve fixed-width wraparound in unchecked integral arithmetic](https://github.com/devhxj/Jazor/issues/34) | `bug` | Required correctness for arithmetic already accepted by lowering. |
| [#35 — Lower integral division with C# truncation semantics](https://github.com/devhxj/Jazor/issues/35) | `bug` | Required high-priority correctness for basic integer expressions. |
| [#36 — Implement floating-point to integral cast semantics instead of erasing the conversion](https://github.com/devhxj/Jazor/issues/36) | `bug` | Required correctness for accepted numeric casts. |
| [#107 — Define runtime numeric type discrimination after boxing erasure](https://github.com/devhxj/Jazor/issues/107) | `bug` | Prevent false-positive CLR type checks when runtime identities have been erased. |

### References and aliasing

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#37 — Preserve aliasing for ref local declarations](https://github.com/devhxj/Jazor/issues/37) | `bug` | Implement a bounded storage-location protocol or reject the syntax before emission. |
| [#38 — Preserve alias rebinding in ref assignments](https://github.com/devhxj/Jazor/issues/38) | `bug` | Support only with a real compiler-owned location representation. |
| [#39 — Preserve selected storage locations in conditional ref expressions](https://github.com/devhxj/Jazor/issues/39) | `bug` | Implement a location-valued conditional or explicitly reject it. |
| [#40 — Preserve ref-return method and property locations](https://github.com/devhxj/Jazor/issues/40) | `bug` | A bounded location protocol is useful; reject unsupported returns rather than emitting invalid JS. |
| [#41 — Assess by-reference returns from lambdas and delegates](https://github.com/devhxj/Jazor/issues/41) | `enhancement` | Defer until ref-return storage semantics are implemented end to end. |
| [#42 — Preserve aliasing when multiple ref parameters reference the same location](https://github.com/devhxj/Jazor/issues/42) | `bug` | Required correctness within the advertised ref/out protocol, or explicitly bound the unsupported alias case. |
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
| [#74 — Reject or implement ordinary delegate + and - expressions](https://github.com/devhxj/Jazor/issues/74) | `bug` | Fix silent JS arithmetic; multicast support should use an explicit protocol. |
| [#75 — Assess mapped external method groups as event handlers](https://github.com/devhxj/Jazor/issues/75) | `enhancement` | Worth supporting when stable callback/receiver identity can be established. |

### Interface dispatch and erased type identity

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#76 — Support explicit interface method and property implementations](https://github.com/devhxj/Jazor/issues/76) | `bug` | Useful feature; at minimum reject every unsupported member shape deliberately. |
| [#77 — Resolve default interface method and property implementations](https://github.com/devhxj/Jazor/issues/77) | `bug` | Required correctness for accepted calls; a bounded source implementation slice is useful. |
| [#85 — Define unprovable runtime is checks against erased interfaces](https://github.com/devhxj/Jazor/issues/85) | `documentation` | Keep unresolved checks unsupported; preserve compiler-proven folds. |
| [#86 — Define as casts to erased interfaces when compatibility is not provable](https://github.com/devhxj/Jazor/issues/86) | `documentation` | Keep unresolved casts unsupported under the current contract. |
| [#89 — Define typeof for erased records, tuples, interfaces and internal CLR carriers](https://github.com/devhxj/Jazor/issues/89) | `documentation` | Keep tokens unavailable where the current representation cannot supply a truthful identity. |

### Enumeration, awaiting and cleanup

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#78 — Support using with explicit IDisposable.Dispose implementations](https://github.com/devhxj/Jazor/issues/78) | `enhancement` | Useful for ordinary resource wrappers. |
| [#79 — Support await using with explicit IAsyncDisposable implementations](https://github.com/devhxj/Jazor/issues/79) | `enhancement` | Useful with async resource lifetimes; share explicit-interface binding. |
| [#104 — Lower custom GetEnumerator patterns instead of assuming JS iterability](https://github.com/devhxj/Jazor/issues/104) | `bug` | Useful ordinary C# syntax; reject unsupported enumeration protocols until implemented. |
| [#105 — Lower custom asynchronous foreach enumerator patterns](https://github.com/devhxj/Jazor/issues/105) | `bug` | Useful after the selected async protocol is modeled; do not assume every C# async enumerable is a JS async iterable. |
| [#106 — Honor the C# awaiter pattern for custom awaitable types](https://github.com/devhxj/Jazor/issues/106) | `bug` | Useful for library interoperability; make unsupported awaiters explicit first. |

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
| [#98 — Preserve or reject custom record constructor bodies instead of dropping them](https://github.com/devhxj/Jazor/issues/98) | `bug` | Required correctness while keeping records structural. |
| [#99 — Preserve record struct copy semantics during assignment](https://github.com/devhxj/Jazor/issues/99) | `bug` | Required correctness for accepted mutable record structs. |
| [#100 — Assess computed string keys in structural object initializers](https://github.com/devhxj/Jazor/issues/100) | `enhancement` | Useful for explicit dictionary-like record contracts; define evaluation and key rules first. |

### RazorVue rendering and component activation

| Feature / issue | Classification | Recommendation |
| --- | --- | --- |
| [#109 — Support switch statements containing render content](https://github.com/devhxj/Jazor/issues/109) | `enhancement` | High-value authoring syntax; implement through the official SG operation path. |
| [#110 — Assess try/catch/finally around Razor render content](https://github.com/devhxj/Jazor/issues/110) | `enhancement` | Useful for authored render-time control flow if Vue framing remains sound. |
| [#111 — Preserve throw statements in Razor render bodies](https://github.com/devhxj/Jazor/issues/111) | `enhancement` | Worth supporting as ordinary explicit error propagation. |
| [#112 — Assess using scopes that contain render content](https://github.com/devhxj/Jazor/issues/112) | `enhancement` | Support a bounded synchronous scope if its cleanup timing can be preserved. |
| [#113 — Support runtime local declarations inside open render frames](https://github.com/devhxj/Jazor/issues/113) | `enhancement` | High-value normal Razor authoring pattern. |
| [#114 — Allow valid local declarations without initializers in render bodies](https://github.com/devhxj/Jazor/issues/114) | `enhancement` | Worth supporting; Roslyn already validates definite assignment. |
| [#115 — Support ordinary assignment and update statements in render bodies](https://github.com/devhxj/Jazor/issues/115) | `enhancement` | Useful for normal local render computation; document state-write implications separately. |
| [#116 — Support while/do conditions that require compiler temporaries](https://github.com/devhxj/Jazor/issues/116) | `enhancement` | Worth supporting to compose existing pattern/expression lowering. |
| [#117 — Support for control expressions that require compiler temporaries](https://github.com/devhxj/Jazor/issues/117) | `enhancement` | Worth supporting as a compiler-framing integration slice. |
| [#118 — Support side effects between complete render segments in a loop](https://github.com/devhxj/Jazor/issues/118) | `enhancement` | Useful for authored loops without changing evaluation order. |
| [#119 — Support computation-only loops inside render bodies](https://github.com/devhxj/Jazor/issues/119) | `enhancement` | Useful for ordinary render-local preparation. |
| [#120 — Allow for declarations with unused uninitialized control locals](https://github.com/devhxj/Jazor/issues/120) | `enhancement` | Small correctness/authoring improvement after local declarations are supported. |
| [#121 — Support statement-bodied RenderFragment factories with captured locals](https://github.com/devhxj/Jazor/issues/121) | `enhancement` | Useful for reusable typed render composition. |
| [#122 — Assess runtime element tag names in RenderTreeBuilder authoring](https://github.com/devhxj/Jazor/issues/122) | `enhancement` | Useful as an explicitly typed dynamic-element feature. |
| [#123 — Assess runtime attribute names in RenderTreeBuilder authoring](https://github.com/devhxj/Jazor/issues/123) | `enhancement` | Useful through an explicit dynamic-props contract; preserve typed component parameters. |
| [#124 — Support conditional parameters targeting different named RenderFragment slots](https://github.com/devhxj/Jazor/issues/124) | `enhancement` | Useful for conditional slot composition. |
| [#125 — Assess source component primary constructors](https://github.com/devhxj/Jazor/issues/125) | `enhancement` | Useful if parameters can be resolved through a clear activation contract. |
| [#126 — Define activation for multiple explicit component constructors](https://github.com/devhxj/Jazor/issues/126) | `enhancement` | Defer until a deterministic authoring-time selector is defined. |
| [#127 — Support base(...) arguments in source component activation](https://github.com/devhxj/Jazor/issues/127) | `enhancement` | Useful for source component hierarchies after activation inputs are specified. |
| [#128 — Assess this(...) chaining during component initialization](https://github.com/devhxj/Jazor/issues/128) | `enhancement` | Useful only with a coordinated component activation chain. |
| [#129 — Define ref/out/in/params component constructor parameter boundaries](https://github.com/devhxj/Jazor/issues/129) | `documentation` | Keep these activation parameter forms unsupported unless a concrete protocol need emerges. |

## Reproducibility limits

The posted issues contain their C# examples, observed results, assessment and immutable source links. One-off local probes and API publication receipts are under ignored `.tmp/syntax-audit/`; they are research inputs, not checked-in regression tests or release gates. Each implementation issue requires permanent focused regressions before its support status can change.
