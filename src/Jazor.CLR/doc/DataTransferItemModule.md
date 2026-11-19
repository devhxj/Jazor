# DataTransferItemModule

`Microsoft.AspNetCore.Components.Web.DataTransferItem` 通过 CLR `Alias` 映射到
WebIDL 生成的 `ECMAScript.DataTransferItem`。`Kind` 和 `Type` getter 分别以
Inline 读取原生 `kind` 和 `type`；构造器和 setter 保持 `Op.Discard`，不创建
或修改 Blazor DTO。

| CLR 成员 | 映射 | 实现 |
|---|---|---|
| `DataTransferItem` | `Alias` | `DataTransferItem` |
| `Kind.get` | `Inline` | `__arg1.kind` |
| `Type.get` | `Inline` | `__arg1.type` |
| `Kind.set`、`Type.set`、构造器 | `Discard` | 不支持 |

原生拖放数据的使用与生命周期见 [Browser Interop](../../../docs/03-guides/browser-interop.md)。
