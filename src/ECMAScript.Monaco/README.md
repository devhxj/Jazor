# ECMAScript.Monaco

Monaco Editor 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游版本、`manifest.json`（schema 2）、打包后的 `dist/` 资源、样式、worker、许可证与 inventory。

Strongly typed C# bindings for Monaco Editor, shipped as a Jazor JS resource library with a locked upstream version, package-local `manifest.json` (schema 2), bundled `dist/` assets, stylesheet, workers, licenses, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| npm 包 | `monaco-editor` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 许可证 | MIT（`licenses/MONACO-LICENSE` 与 `licenses/THIRD-PARTY-NOTICES.txt`） |
| 作者入口 | `monaco-editor` |
| 构建输入 | `src/ECMAScript.Vue.Generator/monaco-runtime/`（package.json + lockfile 锁定版本） |

### 为什么需要打包

上游 `esm/vs/**` 树**不能被浏览器直接加载**：其中 120 个模块 `import` 纯 `.css` 文件，浏览器原生 ESM 会拒绝；上游 `min/` 是 AMD 格式。因此本包沿用仓库 TDesign 先例，用 esbuild 把上游 ESM 入口打成自包含 ESM，并由 esbuild 抽出样式。产物：

| 产物 | 说明 |
| --- | --- |
| `dist/monaco-editor/editor.api.mjs` | 编辑器入口（≈5 MB，含全部依赖） |
| `dist/monaco-editor/editor.api.css` | 编辑器样式（manifest `styles` 声明） |
| `editor.worker.start.mjs` | 基础 editor worker |
| `json.worker.mjs` / `css.worker.mjs` / `html.worker.mjs` / `ts.worker.mjs` | 语言服务 worker（manifest 模块依赖声明） |

## 首期范围 First slice

- 编辑器与模型：`create`、`createModel`、`getModels`、`getEditors`；`MonacoEditor`（`GetModel`/`SetModel`/`GetValue`/`Layout`/`Focus`/`GetLayoutInfo`/`ScrollTop`/`Trigger`/`UpdateOptions`/`Dispose`）；`MonacoModel`（`GetLanguageId`/`GetValue`/`SetValue`/`GetLineContent`/`GetVersionId`/`IsDisposed`/`Dispose`）
- 事件：模型内容/语言/释放，编辑器光标/选区/布局/聚焦/失焦（均返回 `MonacoDisposable`）
- 语言与主题：`setModelLanguage`、`defineTheme`、`setTheme`
- 标记：`setModelMarkers`、`removeAllMarkers`、`onDidChangeMarkers`
- worker：`createWebWorker<TWorker>`；worker URL 由应用显式提供

未绑定：diff editor、completion/suggest、语义 token、装饰（decorations）、大文件策略与自定义语言注册。这些属于计划中「评估」部分，按需扩充。

## SSR 边界 SSR boundary

编辑器依赖真实 DOM、布局测量与 `ResizeObserver`。`Create`、`Layout` 与全部 worker 入口必须在浏览器生命周期内使用；SSR 期间不得直接创建编辑器或 worker，也不提供服务器端等价实现。

## 使用示例 Authoring

```csharp
using static ECMAScript.Monaco;

var editor = Create(element, new MonacoEditorConstructionOptions
{
    Value = "fn main() {}",
    Language = "rust",
    Theme = "vs-dark",
    AutomaticLayout = true
});

var model = editor.GetModel()!;
var disposable = model.OnDidChangeContent(change => { /* versionId 递增 */ });
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-monaco.cs -- --version 0.56.0
```

生成器先用 `npm ci` 安装锁定的构建输入，再用 esbuild 打包编辑器与四个语言 worker，并执行浏览器安全闸门（`process.env` 必须带环境守卫）。

## 测试 Tests

`src/ECMAScript.Monaco.Test` 覆盖 manifest/inventory 元数据、全部产物哈希、样式与 worker 依赖声明、浏览器安全、上游导出 drift、契约形状与编译器 emission。
