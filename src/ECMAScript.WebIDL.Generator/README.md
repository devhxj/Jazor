# ECMAScript.WebIDL.Generator

> 定位：WebIDL 生成管线的 .NET 宿主，负责采集 WebRef inventory 并生成预览 C# binding。

## 职责

- 通过 `DenoHost` 运行 Deno collection worker，并维护 worker 与 `deno.json`。
- 从 `webref` 和 `webidl2` 收集稳定 JSON inventory，写入 `src/ECMAScript/webidl`。
- 为 `typedef`、`enum`、`callback`、`dictionary`、`interface` 和 `namespace` 生成预览 C# binding。

## 运行

采集并生成：

```bash
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --out src/ECMAScript/webidl
```

只根据已提交 inventory 再生，不访问 WebRef：

```bash
dotnet run --project src/ECMAScript.WebIDL.Generator/ECMAScript.WebIDL.Generator.csproj -- --out src/ECMAScript/webidl --from-inventory src/ECMAScript/webidl/webidl.inventory.json
```

## 边界

`src/ECMAScript.WebIDL` 是归档的 legacy TypeScript generator，不参与当前构建。该项目不以旧 TypeScript 输出作为当前 WebIDL contract 的唯一依据。

## 相关文档

- [ECMAScript](../ECMAScript/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
