# Jazor.Emit

> 定位：面向宿主的 ECMAScript 模块物化、manifest 与浏览器 bundle 层。

`Jazor.Emit` 消费 compiler 生成的 ECMAScript catalog 与 source-map carrier，负责程序集读取、确定性文件输出、manifest、清理、本地库资源物化和 Netpack 浏览器打包；它不拥有 C# lowering 语义。

## 职责

- 读取根程序集与显式引用程序集。
- 收集 ECMAScript module catalog、`Jazor.Generated.VueRenderCatalog`、CLR runtime catalog 与 Razor-to-Vue runtime asset。
- 写入 `.mjs`、可选 `.mjs.map` 与 schema-v1 `jazor-manifest.json`。
- 在受控输出范围内清理过期模块和 source map。
- 通过 Netpack 打包应用模块，同时保留本地包提供的 library ESM 与 chained source map。

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
