# Jazor.Emit

> Status: active reference
> Positioning: host-facing materialization, bundle, and RazorVue diff layer.

`Jazor.Emit` 负责把编译阶段已经生成好的 catalog、SourceMap carrier 和 RazorVue artifact 真正写成文件系统中的产物。它不拥有 lowering 语义，只拥有物化、清理、打包与差分输出。

## Responsibilities

- 载入 root assembly 和被引用程序集。
- 收集 ECMAScript module catalog 与 RazorVue catalog。
- 物化 `.mjs`、manifest、RazorVue sidecar manifest 与 `.map` 文件。
- 通过 `DenoHost` 执行 bundle。
- 生成 RazorVue manifest diff / update plan。

## Boundaries

- `Jazor.Compiler` 负责 AST、文本、catalog 和 source-origin/source-map carriers。
- `Jazor.Common.Emit` 与 `Jazor.Common.SourceMaps` 提供跨模块共享模型。
- `Jazor.Emit` 只负责 host-facing 文件输出与 bundle orchestration。

## Key Files

- `Program.cs`: CLI 入口。
- `ModuleCollector.cs`: 汇总程序集中的发射 catalog。
- `ModuleWriter.cs`: 写出 ECMAScript 模块和 manifest。
- `RazorVueCatalogReader.cs`: 读取 RazorVue catalog。
- `RazorVueModuleWriter.cs`: 写出 RazorVue 模块、manifest 和 `.map`。
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
