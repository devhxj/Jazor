# ECMAScript.WebIDL.Generator

> 定位：WebIDL 生成管线的 .NET 宿主，负责采集 WebRef inventory 并生成预览 C# binding。

## 职责

- 通过 `DenoHost` 运行 Deno collection worker，并维护 worker 与 `deno.json`。
- 从 `webref`、`webidl2` 和 `@webref/xref` 收集稳定 JSON inventory，写入 `src/ECMAScript/webidl`。
- 为 `typedef`、`enum`、`callback`、`dictionary`、`interface` 和 `namespace` 生成预览 C# binding。
- 将 W3C/WHATWG 规范定义的精确锚点、章节、作者写作的正文和可用的规范内用法写入生成 XML 文档，便于 IDE 中直接理解 API 语义并定位规范。

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

生成器不会根据 WebIDL 类型形状编造诸如“Represents”或“Invokes”的摘要。`@webref/xref` 提供定义锚点，缺少 xref prose 时，worker 仅从该规范在 W3C/WHATWG GitHub 仓库中记录的源码中提取同一锚点或章节附近的作者正文；网络暂时不可用时不会伪造说明。若没有可验证的规范定义，输出将省略 XML 文档；已匹配项始终保留其规范链接。

## 相关文档

- [ECMAScript](../ECMAScript/README.md)
- [平台与绑定](../../docs/02-architecture/platform-and-bindings.md)
