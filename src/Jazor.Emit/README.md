# Jazor.Emit

> Status: active reference
> Positioning: host-facing materialization, bundle, and RazorVue diff layer.

`Jazor.Emit` 负责把编译阶段已经生成好的 catalog、SourceMap carrier 和 RazorVue artifact 真正写成文件系统中的产物。它不拥有 lowering 语义，只拥有物化、清理、打包与差分输出。

## Responsibilities

- 载入 root assembly 和被引用程序集。
- 收集 ECMAScript module catalog 与 RazorVue catalog。
- 物化 `.mjs` 或 `.vue`、manifest、RazorVue sidecar manifest 与 `.map` / origins 文件。
- 通过 `DenoHost` 执行 bundle。
- 将 RazorVue `.vue` SFC 编译为 Jazor authored-module 可消费的 named-export bridge modules。
- 生成 RazorVue consumer entry modules，标准化 manifest 解析、组件选择、SFC bridge 调用和 browser/SSR 入口拼接。
- 生成 RazorVue manifest diff / update plan。

## Boundaries

- `Jazor.Compiler` 负责 AST、文本、catalog 和 source-origin/source-map carriers。
- `Jazor.RazorVue.Emit` 与 `Jazor.Common.SourceMaps` 提供跨模块共享模型。
- `Jazor.Emit` 只负责 host-facing 文件输出与 bundle orchestration。

## Key Files

- `Program.cs`: CLI 入口。
- `ModuleCollector.cs`: 汇总程序集中的发射 catalog。
- `ModuleWriter.cs`: 写出 ECMAScript 模块和 manifest。
- `RazorVueCatalogReader.cs`: 读取 RazorVue catalog。
- `RazorVueModuleWriter.cs`: 写出 legacy RazorVue 模块、manifest 和 `.map`。
- `RazorVueSfcCatalogReader.cs`: 读取 `VueSfcArtifact` catalog。
- `RazorVueSfcModuleWriter.cs`: 写出 RazorVue `.vue` artifact、manifest 和 sidecar metadata。
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

RazorVue SFC named-export bridge：

```powershell
dotnet run --project src/Jazor.Emit -- razorvue-sfc-bridge --host-root <jazor-dir> --manifest <jazor-manifest-razorvue.json> --out <generated-dir> --mode browser
dotnet run --project src/Jazor.Emit -- razorvue-sfc-bridge --host-root <jazor-dir> --manifest <jazor-manifest-razorvue.json> --out <generated-dir> --mode ssr
```

该 bridge 保持 Jazor 编译器“不支持 default import/export”的边界不变：`.vue` SFC 的 default component 会在 build-time 转换为 manifest 中 `ComponentName` 对应的 named export，组件间相对 `.vue` default import 也会重写为 named `.mjs` import。

RazorVue consumer entry：

```powershell
dotnet run --project src/Jazor.Emit -- razorvue-consumer-entry --host-root <jazor-dir> --out <build-dir> --client-runtime <runtime-client.js> --ssr-runtime <runtime-ssr.js> --client-runtime-export mountRazorVueConsumer --ssr-runtime-export runRazorVueConsumerSsr --component App=id:My.App.RootComponent
```

该命令会读取 `jazor-manifest-razorvue.json`，调用 SFC bridge 生成 browser/SSR named-export modules，并写出：

- `client-entry.mjs`
- `ssr-entry.mjs`
- `vue-feature-flags.mjs`
- `razorvue-consumer-entry.json`

consumer runtime export 的稳定签名是：

```js
mountRazorVueConsumer(razorVueConsumerComponents, razorVueHostRequirements);
runRazorVueConsumerSsr(razorVueConsumerComponents, razorVueHostRequirements);
```

`razorVueConsumerComponents` 是按 `--component Alias=selector` 生成的 frozen object。selector 支持 `id:...`、`name:...`、`path:...`，不带前缀时会按 ComponentId、ComponentName、RelativeModulePath 匹配；若匹配不唯一，命令会失败并要求显式 selector。

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
