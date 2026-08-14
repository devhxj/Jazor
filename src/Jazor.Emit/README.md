# Jazor.Emit

> 定位：面向宿主的 ECMAScript 模块物化、manifest 与浏览器 bundle 层。

`Jazor.Emit` 消费 compiler 生成的 ECMAScript catalog 与 source-map carrier，负责程序集读取、确定性文件输出、manifest、清理、本地库资源物化和 Netpack 浏览器打包；它不拥有 C# lowering 语义。

## 职责

- 读取根程序集与显式引用程序集。
- 收集 ECMAScript module catalog、`Jazor.Generated.ArtifactCatalog`、CLR runtime catalog 与 adapter-owned runtime provider。
- 写入 `.mjs`、可选 `.mjs.map` 与 schema-v1 `jazor-manifest.json`。
- 在受控输出范围内清理过期模块和 source map。
- 通过 Netpack 打包应用模块，同时保留本地包提供的 library ESM 与 chained source map。

## Adapter 契约

Emit 只识别两个结构性数据 carrier，不引用 Vue、React 或其运行时程序集：

- `Jazor.Generated.ArtifactCatalog`：producer id、模块、source map、资产、package imports 与不透明 HMR payload。
- `Jazor.Artifacts.RuntimeProviderCatalog`：provider id、嵌入模块、静态依赖路径与 import-map contribution。

runtime provider 的模块仅在应用模块引用其入口时物化；provider 负责声明内部依赖闭包。`module-source` 资产以 producer 声明的 `ImportPath` 重写到其 artifact path，Emit 不再按框架类型分支处理 SFC、JSX 或其他源格式。

## Library Asset Contract

`PackageImports` selects the package ESM entries actually used by an application. A library manifest entry may declare mode-specific logical dependencies and relative `files`; Emit copies only the selected transitive entry closure, active library styles, and shared root metadata/license files. This keeps browser and SSR graphs explicit without probing `node_modules` or parsing third-party source at build time. SSR adds its runner-owned `vue` and `@vue/server-renderer` roots explicitly.

## 关键文件

- `Program.cs`：CLI 入口。
- `CatalogReader.cs`、`ModuleCollector.cs`：读取并稳定合并各程序集 catalog。
- `ModuleWriter.cs`、`ManifestModel.cs`：物化模块、source map 与 manifest。
- `LibraryMaterializer.cs`、`Toolchain.cs`、`NetpackBundler.cs`：本地资源与浏览器 bundle。

## CLI

物化模块：

```bash
dotnet run --project src/Jazor.Emit -- --root <root.dll> --assembly <ref.dll> --out <dir> --write-manifest <manifest.json>
```

打包模块：

```bash
dotnet run --project src/Jazor.Emit -- toolchain build --manifest <manifest.json> --artifacts <dir> --source-root <source-root> --out-root <output-root>
```

## 验证

```bash
dotnet test src/Jazor.EmitTest/Jazor.EmitTest.csproj
```

## 相关文档

- [Jazor.EmitTest](../Jazor.EmitTest/README.md)
- [产物管线](../../docs/02-architecture/artifact-pipeline.md)
