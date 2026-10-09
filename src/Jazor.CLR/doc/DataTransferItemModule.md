# DataTransferItemModule

`Microsoft.AspNetCore.Components.Web.DataTransferItem` 通过 CLR `Alias` 映射到
WebIDL 生成的 `ECMAScript.DataTransferItem`。`Kind` 和 `Type` getter 分别以
Inline 读取原生 `kind` 和 `type`；构造器和 setter 保持 `Op.Discard`，不创建
或修改 Blazor DTO。

preview.8 在 `Jazor.Vue` 的作者程序集提供 `NativeItem` extension 属性。
引入 `ECMAScript` 后，`item.NativeItem` 返回同一个浏览器对象并暴露完整
WebIDL 契约，例如 `GetAsFile()` 返回 `FileRef?`，`GetAsString(...)` 读取文本。
此投影不复制对象，也不添加 CLR 文件或 Stream 包装。

| CLR 成员 | 映射 | 实现 |
|---|---|---|
| `DataTransferItem` | `Alias` | `DataTransferItem` |
| `Kind.get` | `Inline` | `__arg1.kind` |
| `Type.get` | `Inline` | `__arg1.type` |
| `Kind.set`、`Type.set`、构造器 | `Discard` | 不支持 |

原生拖放数据的使用与生命周期见 [Browser Interop](../../../docs/03-guides/browser-interop.md)。
