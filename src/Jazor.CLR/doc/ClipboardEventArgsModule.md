# ClipboardEventArgsModule.cs

preview.8 在 `Jazor.Vue` 中为既有 `ClipboardEventArgs` 提供 extension。
引入 `ECMAScript` 后，`NativeEvent` 返回同一个 WebIDL `ClipboardEvent`，
`ClipboardData` 返回 `ECMAScript.DataTransfer?`，并可读取共同 Event 属性
如 `Target`、`CurrentTarget` 和 `DefaultPrevented`。CLR `Type` getter 的映射保留。
使用示例与事件生命周期见 [Browser Interop](../../../docs/03-guides/browser-interop.md)。

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.get</br>
**签名**：_fb72b7b890c36924</br>

**成员**：Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.Type.set</br>
**签名**：_90025c0225e61bd6</br>

**成员**：Microsoft.AspNetCore.Components.Web.ClipboardEventArgs.ClipboardEventArgs()</br>
**签名**：_7d238a713c8bf970</br>
