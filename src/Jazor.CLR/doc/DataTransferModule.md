# DataTransferModule.cs

preview.8 保留 CLR `DataTransfer` 到 WebIDL `ECMAScript.DataTransfer` 的 Alias，
并在 `Jazor.Vue` 中提供 extension：`NativeDataTransfer` 暴露同一个对象的完整
WebIDL 契约，`NativeFiles` 返回 `FileList`，`NativeItems` 返回
`DataTransferItemList`。引入 `ECMAScript` 后即可使用这些属性。

既有 CLR `Files`（`string[]`）和 `Items`（DTO 数组）的 getter/setter 仍不支持；
原生集合不会冒充这些 CLR 数组。文件和文本 payload 的使用见
[Browser Interop](../../../docs/03-guides/browser-interop.md)。

> ⚠️ **注意**：签名= _+ SHA256Hash(成员)

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.get</br>
**签名**：_69d6126953ef76e6</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.set</br>
**签名**：_10e9a491fcf8c810</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.get</br>
**签名**：_30bc24b25bf7d9a2</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.set</br>
**签名**：_b719f7d9442296fd</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.Files.get</br>
**签名**：_a0fc8027e14f21ca</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.Files.set</br>
**签名**：_9592e4b9170f4557</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.Items.get</br>
**签名**：_380a3ba4cfe24381</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.Items.set</br>
**签名**：_25afb4e8576a1ada</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.Types.get</br>
**签名**：_c119fef09012b249</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.Types.set</br>
**签名**：_509a62c0c67e889b</br>

**成员**：Microsoft.AspNetCore.Components.Web.DataTransfer.DataTransfer()</br>
**签名**：_a4bc87e1b6bb8dea</br>
