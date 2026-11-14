# Compiler syntax bug resolutions

Release: [Jazor 1.3.0](https://github.com/devhxj/Jazor/releases/tag/v1.3.0).

This ledger tracks the 35 `bug` issues from the C# syntax audit. A diagnostic resolution means the compiler deliberately rejects a runtime protocol it cannot preserve. It does not advertise support for that language feature. Ordinary source classes still compile without an `[ECMAScript]` permission marker.

| Issue | Construct | Resolution |
| --- | --- | --- |
| [#9](https://github.com/devhxj/Jazor/issues/9) | Dynamic binary operators | Reject operations requiring the C# runtime binder. |
| [#10](https://github.com/devhxj/Jazor/issues/10) | Dynamic conversions | Reject runtime-bound conversions; retain erased object/dynamic pass-through. |
| [#13](https://github.com/devhxj/Jazor/issues/13) | Local expression trees | Reject expression-tree conversions instead of emitting a callable function. |
| [#17](https://github.com/devhxj/Jazor/issues/17) | Pointer dereference | Report the missing memory-location protocol at the source operation. |
| [#18](https://github.com/devhxj/Jazor/issues/18) | Pointer indexing | Reject pointer indexing instead of emitting ordinary array access. |
| [#20](https://github.com/devhxj/Jazor/issues/20) | Fixed statements | Reject CLR pinning and fixed-address semantics. |
| [#28](https://github.com/devhxj/Jazor/issues/28) | Generic static state | Reject source generic static storage that would be shared across closed instantiations. Constants remain compile-time values. |
| [#29](https://github.com/devhxj/Jazor/issues/29) | Static abstract interface calls | Reject erased generic runtime implementation dispatch. |
| [#32](https://github.com/devhxj/Jazor/issues/32) | Checked integer arithmetic | Evaluate once, check the bound result width, and throw on overflow before write-back. |
| [#33](https://github.com/devhxj/Jazor/issues/33) | Checked integer casts | Truncate floating input, check bounds, and check BigInt narrowing before precision can be lost. |
| [#34](https://github.com/devhxj/Jazor/issues/34) | Unchecked integer arithmetic | Wrap results to their signed/unsigned width, including compound assignment and updates. |
| [#35](https://github.com/devhxj/Jazor/issues/35) | Integer division | Truncate toward zero; preserve operand evaluation order, zero-division failure, signed overflow, and integer positive zero. |
| [#36](https://github.com/devhxj/Jazor/issues/36) | Integer casts | Truncate floating fractions and narrow integer carriers correctly; use Roslyn's result for constant conversions. |
| [#37](https://github.com/devhxj/Jazor/issues/37) | Ref locals | Reject persistent storage aliases. |
| [#38](https://github.com/devhxj/Jazor/issues/38) | Ref reassignment | Reject reference-location reassignment. |
| [#39](https://github.com/devhxj/Jazor/issues/39) | Conditional ref | Reject conditional reference locations. |
| [#40](https://github.com/devhxj/Jazor/issues/40) | Ref returns | Reject source ref/ref-readonly returns, including local functions and lambdas. |
| [#42](https://github.com/devhxj/Jazor/issues/42) | Aliased ref/out arguments | Reject multi-reference calls without provably distinct locations. Preserve existing single-evaluation protocols for disjoint locations. |
| [#74](https://github.com/devhxj/Jazor/issues/74) | Delegate combination | Reject multicast combination/removal rather than emitting arithmetic. |
| [#76](https://github.com/devhxj/Jazor/issues/76) | Explicit interface implementation | Diagnose missing runtime implementation slots instead of dropping methods or emitting qualified accessor names. |
| [#77](https://github.com/devhxj/Jazor/issues/77) | Default interface methods | Reject calls to source interface bodies without an emitted runtime slot. |
| [#98](https://github.com/devhxj/Jazor/issues/98) | Record custom constructors | Reject executable bodies and constructor initializers instead of discarding their behavior. |
| [#99](https://github.com/devhxj/Jazor/issues/99) | Mutable record struct copying | Reject mutable record struct values until implicit value-copy semantics exist. Readonly structural records remain available. |
| [#102](https://github.com/devhxj/Jazor/issues/102) | Array defaults | Initialize every allocated element; allocate independent structured defaults and evaluate rectangular dimensions once. |
| [#103](https://github.com/devhxj/Jazor/issues/103) | Module field defaults | Reuse compiler-owned typed defaults for fields and backing storage. Initialize all module storage before executing nonconstant authored initializers, preserving forward reads through helper calls. RazorVue owns its existing setup/props phases. |
| [#104](https://github.com/devhxj/Jazor/issues/104) | Custom foreach enumerators | Reject source collection protocols without JS iterable semantics. |
| [#105](https://github.com/devhxj/Jazor/issues/105) | Custom async enumerators | Reject source async enumerator protocols without an implemented async iterable bridge. |
| [#106](https://github.com/devhxj/Jazor/issues/106) | Custom awaiters | Reject source C# awaiter protocols instead of treating arbitrary values as Promises. |
| [#107](https://github.com/devhxj/Jazor/issues/107) | Boxed numeric type checks | Fold provable CLR numeric identities while preserving evaluation; reject ambiguous erased identities. |
| [#108](https://github.com/devhxj/Jazor/issues/108) | Expression-bodied null suppression | Obtain the bound arrow-body operation when wrapper syntax has no direct operation. |
| [#130](https://github.com/devhxj/Jazor/issues/130) | Dynamic unary operators | Reject runtime binder semantics. |
| [#131](https://github.com/devhxj/Jazor/issues/131) | Dynamic updates | Reject dynamic increment/decrement. |
| [#132](https://github.com/devhxj/Jazor/issues/132) | Dynamic conditions | Reject runtime-bound truth/conversion operations. |
| [#136](https://github.com/devhxj/Jazor/issues/136) | Return in pattern switch | Keep returns in the authored function, with switch-scoped pattern bindings. |
| [#137](https://github.com/devhxj/Jazor/issues/137) | Await in pattern switch | Keep await and awaited guards in the authored async function; preserve finally and loop transfers. |

The 35 issue reproducers have individual regressions in `SyntaxAuditRegressionTests`. Runtime cases execute through the in-process NodeApi host. Numeric matrices and boundary cases compare CLR results/exceptions with the generated module; unsupported cases require deliberate diagnostics with source locations. Existing deterministic emission, import, source-origin, and source-map tests continue to cover the surrounding pipeline.

Local validation on November 15, 2026: the solution builds with no warnings or errors; the compiler gate passes **10,872 tests**, **99.38% line coverage**, and **97.02% branch coverage**. The CLR suite passes **5,092 tests**. The RazorVue gate passes **5,056 tests**, **97.54% line coverage**, and **94.01% branch coverage**. The complete Emit suite passes **292 tests**, including packaged browser consumers. Integration validation covers component parameter defaults, explicit disposal roots, DOM references, and module materialization; see the test projects for repeatable commands. These are local checks, not a published release or remote CI result.

These corrections do not add a CLR binder, boxing runtime, unmanaged memory model, multicast delegate representation, generic static registry, or custom enumerator/awaiter bridge. See the [compiler contract](../../src/Jazor.Compiler/README.md) and [implementation principles](../../src/Jazor.Compiler/ImplementationPrinciples.md) for the maintained support boundary.
