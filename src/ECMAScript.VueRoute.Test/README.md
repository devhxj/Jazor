# ECMAScript.VueRoute.Test

`ECMAScript.VueRoute.Test` is the dedicated regression project for the `src/ECMAScript.VueRoute` host binding surface.

## Scope

- Module layout and project wiring guards for the standalone `ECMAScript.VueRoute` library.
- Reflection-based proxy surface checks for the exported Vue Router runtime bindings.
- Compiler-boundary coverage proving the binding types are consumable by `Jazor.Compiler`.
- Packaging and shared-test-entry wiring guards so `ECMAScript.VueRoute` remains part of the normal repo build/test flow.

## Current regression coverage

- `EcmaScriptVueRouteLayoutGuardTests`
- `EcmaScriptVueRouteProxyTests`
- `EcmaScriptVueRouteCompilerBoundaryTests`

重点覆盖的 authoring seam:

- delegate / lambda -> union 的显式 `From(...)` authoring
- 接口类型值进入 union 或字典宿主时的强类型入口
- maybe-ref authoring 的语言边界：`VueReadonlyRef<T>` 可直接赋值，`IVueRef<T>` 继续显式 `From(...)`
- 官方公开注入 key 与其值面 contract：避免把 router/view-depth 等公开导出弱化成 `object`
- 命名视图组件字典、`props` 字典、`beforeEnter` guard 数组、legacy `next(...)` callback 的集合初始化与对象初始化 authoring
- `Jazor.Compiler` 降级结果与宿主 API 公开形状之间的一致性

These tests intentionally live outside `Jazor.CompilerTest`. The compiler project keeps compiler semantics, while `ECMAScript.VueRoute.Test` owns the external library contract for the Vue Router binding surface.

## Run

```powershell
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj
```

Run with coverage settings:

```powershell
dotnet test src/ECMAScript.VueRoute.Test/ECMAScript.VueRoute.Test.csproj --settings src/ECMAScript.VueRoute.Test/coverlet.runsettings
```

Or use the shared repo entry point:

```powershell
pwsh ./scripts/test-dotnet.ps1 -Project vueroute
```

## Notes

- The tests read repository files directly to guard solution, package, and script wiring.
- Compiler-boundary tests validate current supported behavior instead of forcing `Jazor.CompilerTest` to carry Vue Router-specific fixtures.
