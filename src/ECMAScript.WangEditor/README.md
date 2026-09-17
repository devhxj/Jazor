# ECMAScript.WangEditor

WangEditor 与其官方 Vue 3 适配器的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游多包版本、`manifest.json`（schema 2）、`dist/` 资源、默认样式、许可证与 inventory。

Strongly typed C# bindings for WangEditor and its official Vue 3 adapter, shipped as a Jazor JS resource library with locked upstream package versions, package-local `manifest.json` (schema 2), `dist/` runtime assets, the default stylesheet, license, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| 作者入口 | `@wangeditor/editor-for-vue`（官方 Vue 3 适配器） |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 闭包包 | `@wangeditor/editor-for-vue`（适配器）、`@wangeditor/editor`（编辑器核心） |
| 许可证 | MIT（核心包附带 `licenses/wangeditor-editor-LICENSE`；适配器包未附带 LICENSE 文件，其声明值记录在 `inventory.json` 的 `packageLicenses`） |
| 样式 | `dist/@wangeditor/editor/style.css`（manifest `styles` 声明） |
| peer 依赖 | `vue`（由 `ECMAScript.Vue` 资源库提供） |

> **版本选择注意**：`@wangeditor/editor-for-vue` 的 npm `latest` 是 **Vue 2** 版本（peer `vue ^2.6.14`），Jazor 必须使用 `5.1.x`（peer `vue ^3`）。生成器已锁定 5.1.12。

## 首期范围 First slice

- 组件代理：`WangEditorComponent`（`ModelValue` 双向 HTML、`Mode`、`DefaultHtml`、`DefaultConfig`、透传属性）与 `WangEditorToolbar`（`Editor`、`Mode`、`DefaultConfig`）
- 配置：`WangEditorConfig`（`Placeholder` 与 `OnChange`/`OnCreated`/`OnDestroyed`/`OnFocus`/`OnBlur` 生命周期回调）、`WangEditorToolbarConfig`
- 值域：`WangEditorMode`（`default`/`simple` 字符串枚举）

未绑定：命令式 `createEditor`/`createToolbar`（组件已覆盖挂载/销毁与 value 绑定）、slate 内容模型、菜单/上传扩展点。

## SSR 边界 SSR boundary

编辑器依赖真实 DOM、选区与剪贴板。`WangEditorComponent`/`WangEditorToolbar` 必须在浏览器生命周期内挂载；SSR 期间不得创建实例。

## 使用示例 Authoring

```razor
<WangEditorToolbar Editor="@editor" />
<WangEditorComponent @bind-ModelValue="@html" DefaultConfig="@config" />
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-wang-editor.cs -- --version 5.1.12
dotnet run --file scripts/csharp/generate-wang-editor.cs -- --source .tmp/p3c --version 5.1.12
```

核心版本按上游 peer 区间锁定在生成器内；生成器执行浏览器安全闸门（拒绝顶层 `process.env`），并在缺少 LICENSE 文件时如实记录声明值而不是伪造文本。

## 测试 Tests

`src/ECMAScript.WangEditor.Test` 覆盖 manifest/inventory 元数据、vendored 哈希、样式资源、上游导出 drift、组件代理契约、字符串枚举与编译器 emission。
