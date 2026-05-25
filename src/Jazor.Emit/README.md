# Jazor.Emit

> Status: active reference
> Positioning: host-facing materialization, bundle, and RazorVue diff layer.

`Jazor.Emit` 负责把编译阶段已经生成好的 catalog、SourceMap carrier 和 RazorVue artifact 真正写成文件系统中的产物。它不拥有 lowering 语义，只拥有物化、清理、打包与差分输出。

## Responsibilities

- 载入 root assembly 和被引用程序集。
- 收集 ECMAScript module catalog 与 RazorVue catalog。
- 物化 `.mjs` 或 `.vue`、统一 manifest 与 `.map` / origins 文件。
- 通过 `DenoHost` 执行 bundle。
- 将 RazorVue `.vue` SFC 编译为 Jazor authored-module 可消费的 named-export bridge modules。
- 生成 RazorVue consumer entry modules，标准化 manifest 解析、组件选择、mixed H/SFC 组件消费、SFC bridge 调用和 browser/SSR 入口拼接。
- 基于统一 `jazor-manifest.json` 中的 RazorVue component projection 生成 diff / update plan。

## Boundaries

- `Jazor.Compiler` 负责 AST、文本、catalog 和 source-origin/source-map carriers。
- `Jazor.RazorVue.Emit` 与 `Jazor.Common.SourceMaps` 提供跨模块共享模型。
- `Jazor.Emit` 只负责 host-facing 文件输出与 bundle orchestration。

## Key Files

- `Program.cs`: CLI 入口。
- `ModuleCollector.cs`: 汇总程序集中的发射 catalog。
- `ModuleWriter.cs`: 写出 ECMAScript 模块和 manifest。
- `RazorVueCatalogReader.cs`: 读取 RazorVue catalog。
- `RazorVueModuleWriter.cs`: 写出 legacy RazorVue 模块、统一 manifest component metadata 和 `.map`。
- `RazorVueSfcCatalogReader.cs`: 读取 `VueSfcArtifact` catalog。
- `RazorVueSfcModuleWriter.cs`: 写出 RazorVue `.vue` artifact、统一 manifest component metadata 和 sidecar metadata。
- `RazorVueSfcBridgeCompiler.cs` / `Deno/razorvue-sfc-bridge.ts`: 编译 `.vue` SFC 并输出 named-export `.mjs` bridge modules。
- `RazorVueConsumerEntryCompiler.cs`: 生成 consumer-facing browser/SSR entry modules。
- `ModuleBundler.cs`: bundle 编排。
- `RazorVueUpdatePlanWriter.cs`: 生成 RazorVue diff/update plan。

## CLI

发射：

```powershell
dotnet run --project src/Jazor.Emit -- --root <root.dll> --assembly <ref.dll> --out <dir> --write-manifest <manifest.json>
```

打包：

```powershell
dotnet run --project src/Jazor.Emit -- bundle --in <dir> --manifest <manifest.json> --out <bundle.mjs>
```

RazorVue diff：

```powershell
dotnet run --project src/Jazor.Emit -- razorvue-diff --previous <old.json> --current <new.json> --out <plan.json>
```

`--out` 是当前推荐参数名；`--write-plan` 作为兼容别名继续接受。

RazorVue SFC named-export bridge：

```powershell
dotnet run --project src/Jazor.Emit -- razorvue-sfc-bridge --host-root <jazor-dir> --manifest <jazor-manifest.json> --out <generated-dir> --mode browser
dotnet run --project src/Jazor.Emit -- razorvue-sfc-bridge --host-root <jazor-dir> --manifest <jazor-manifest.json> --out <generated-dir> --mode ssr
```

该 bridge 保持 Jazor 编译器“不支持 default import/export”的边界不变：`.vue` SFC 的 default component 会在 build-time 转换为 manifest 中 `ComponentName` 对应的 named export，组件间相对 `.vue` default import 也会重写为 named `.mjs` import。

RazorVue consumer entry：

```powershell
dotnet run --project src/Jazor.Emit -- razorvue-consumer-entry --host-root <jazor-dir> --out <build-dir> --client-runtime <runtime-client.js> --ssr-runtime <runtime-ssr.js> --client-runtime-export mountRazorVueConsumer --ssr-runtime-export runRazorVueConsumerSsr --component App=id:My.App.RootComponent
```

该命令会读取统一 `jazor-manifest.json` 中的 RazorVue component metadata，调用 SFC bridge 生成 browser/SSR named-export modules，并写出：

- `client-entry.mjs`
- `ssr-entry.mjs`
- `vue-feature-flags.mjs`
- `razorvue-consumer-entry.json`

混合组件模型下的 consumer 契约是：

- `component.model = "h"` 的 RazorVue H 组件不会经过 SFC bridge，而是从 host `jazor` 根中的 `.mjs` 直接 default import。
- `component.model = "sfc"` 的 RazorVue SFC 组件才会进入 `razorvue-sfc-bridge`，并以 named-export `.mjs` bridge 形式进入 consumer entry。
- 当只选择部分 SFC 组件时，bridge 只编译选中的 entry 及其相对 `.vue` 依赖闭包，不会因为 manifest 中未选中的坏 SFC 组件拖垮 consumer build。
- manifest 中的 component module path 与 sidecar path 必须是 manifest-relative path；统一 manifest 的 `relativePath` / `sourceMapPath` / `component.originMapPath`，以及 legacy RazorVue manifest 的 `RelativeModulePath` / `SourceMapPath` / `OriginMapPath`，遇到 rooted path、drive-qualified path 或 `..` 逃逸会在 projection 阶段直接判为 invalid。malformed module list、null entry、缺失必需 component identity 字段也会稳定判为 invalid，不会进入 bridge/consumer 文件解析。

consumer runtime export 的稳定签名是：

```js
mountRazorVueConsumer(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);
runRazorVueConsumerSsr(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);
```

`razorVueConsumerComponents` 是按 `--component Alias=selector` 生成的 frozen object。selector 支持 `id:...`、`name:...`、`path:...`，不带前缀时会按 ComponentId、ComponentName、RelativeModulePath 匹配；若匹配不唯一，命令会失败并要求显式 selector。

`razorVueConsumerRoutes` 是从统一 manifest 中 RazorVue component metadata 的 `routeTemplates` 投影出的 frozen array。当前只接受能稳定映射到 Vue Router path 的 Razor route 模板：

- 字面量路径，例如 `/`、`/catalog`
- 纯参数 segment，例如 `/examples/{id}`
- 可选参数 segment，例如 `/examples/{id?}`
- whole-segment default value，例如 `/examples/{id=42}`、`/examples/{id:int=42}`
- 带受控 constraint 的参数 segment，例如 `/examples/{id:int}`
- 不含 optional separator 的 mixed/composite segment，例如 `/examples/post-{id}`、`/examples/post-{id:int}`
- composite/mixed segment 内部 default value，例如 `/examples/post-{id=42}`；该参数在 Vue path 中保持 required，只把默认值写入 metadata，不做 URL elision
- 尾部 optional separator composite segment，例如 `/files/{filename}.{ext?}`；会展开为 `/files/:filename.:ext` 与 `/files/:filename` 两条稳定 Vue route
- catch-all segment，例如 `/examples/{*path}`

当前会显式拒绝：

- optional separator 参数不是 segment 尾部、没有紧邻前置 optional separator、或需要多层组合展开的 composite/mixed segment
- 无法诚实映射到 Vue Router regex path 的 route constraint 组合

对于 default value，consumer entry 会把默认值写入 `defaultParameterValues` metadata，并用 `elidableDefaultParameterNames` 明确标注哪些默认值允许在 href 生成时省略。whole-segment default value 会转换成 Vue Router 可接受的 optional path（例如 `:id?` / `:id(<regex>)?`），并标记为可省略；composite default value（例如 `post-{id=42}`）按 ASP.NET Core inbound 语义保持 required path，因此不会进入 `elidableDefaultParameterNames`。consumer runtime 会在 route match 读取与 href 生成时统一应用这两份 metadata，避免把 composite default 错误折叠成不存在的 URL。

当前 route constraint 只对“可稳定映射到 Vue Router path regex，或可由 generated metadata 二次校验”的受控子集开放。现已覆盖的典型约束包括：

- `int`，生成整数 path regex，并通过 `parameterConstraints` metadata 在 consumer runtime 中按 Int32 边界校验
- `long`，生成整数 path regex，并通过 `parameterConstraints` metadata 在 consumer runtime 中按 Int64 边界校验
- `min(...)` / `max(...)` / `range(...)`，按 ASP.NET Core integer/long route constraint 语义生成整数 path regex，并通过 `parameterConstraints` metadata + runtime `BigInt` 校验边界
- `alpha`
- `bool`，按 ASP.NET Core route constraint 语义接受大小写混合的 `true` / `false`
- `guid`，覆盖 `N` / `D` / `B` / `P` / `X` 常见 GUID 文本形态，并兼容 browser pathname 中的 encoded wrapper 形态
- `decimal`，生成非空 path regex，并通过 `numberParse` metadata 在 consumer runtime 中校验 invariant decimal text，包括 `NumberStyles.Number` 的宽松 thousands separator、前置/尾随符号、29 位有效数字舍入和 96-bit decimal 最大值边界
- `double` / `float`，生成非空 path regex，并通过 `numberParse` metadata 在 consumer runtime 中校验 invariant floating-point text，包括 exponent、宽松 thousands separator、`NaN` 和 `Infinity`
- `datetime`，生成非空 path regex，并通过 `dateTimeParse` metadata 在 consumer runtime 中校验 ASP.NET Core `DateTimeRouteConstraint` 对应的 invariant `DateTime.TryParse(...)` 常见 URL 形态，包括 ISO-like numeric date/time、US numeric date、英文月份、day-of-week 校验、time-only、`GMT` / `Z` / numeric offset 后缀、闰年和 `24:00:00` rollover 边界
- `required`，作为 `lengthRange(min: 1)` metadata 参与约束求交
- `length(...)` / `minlength(...)` / `maxlength(...)`，支持单独生成 path regex，也支持与 `int` / `long` / `alpha` / `regex(...)` 等单一 path-regex 约束组合，通过 `parameterConstraints` metadata 在 consumer runtime 中执行长度求交校验
- `regex(...)`

`guid` 的 Vue Router path regex 会避开该 parser 对 custom-regex `)` 的单层转义限制；生成结果不是 C# 语义转译，只是最终 Vue route artifact 的可解析 path pattern。`int` / `long` / `min(...)` / `max(...)` / `range(...)` 的数值边界、`decimal` / `double` / `float` 的 parse 语义、`datetime` 的 DateTime parse 语义，以及 `length(...)` / `minlength(...)` / `maxlength(...)` 的长度边界不只靠 path regex，consumer runtime 会在 Vue Router match / `beforeEnter` / href 生成外层继续执行 generated metadata 校验，避免溢出值、不可解析数值文本或多 constraint 求交失败值被路由误接收。`date` 不是当前 ASP.NET Core 默认 `ConstraintMap` 中的内置 route constraint，仍保持 fail-fast；后续若要支持自定义 `date` constraint，需要先明确其服务端 constraint 类型和解析合同，不能按 `datetime` 近似放行。

consumer runtime 侧也已同步对齐：Playground 这类 consumer 不再维护独立的简化 path matcher / href 拼接规则，而是复用 `vue-router` matcher 语义处理 anchor interception 与 route href 生成，并在其外层叠加 generated route metadata 中的 default parameter 和 parameter constraint contract，避免 generated route metadata 与真实 router 语义漂移。

## Verification

```powershell
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

## Read Next

- [../Jazor.EmitTest/README.md](../Jazor.EmitTest/README.md)
- [../../docs/01-目标/compiler/emit/Emit.Pipeline.Overview.md](../../docs/01-目标/compiler/emit/Emit.Pipeline.Overview.md)
- [../../docs/01-目标/compiler/emit/Emit.Materialization.Overview.md](../../docs/01-目标/compiler/emit/Emit.Materialization.Overview.md)
- [../../docs/01-目标/compiler/emit/Emit.BundleAndSourceMap.Overview.md](../../docs/01-目标/compiler/emit/Emit.BundleAndSourceMap.Overview.md)
- [../../docs/03-完成/emit/status.md](../../docs/03-完成/emit/status.md)
