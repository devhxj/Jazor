using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// Monaco Editor 入口；提供编辑器/模型创建、语言与主题注册、标记与 worker 能力。
/// Monaco Editor entry; provides editor/model creation, language and theme registration, markers,
/// and web-worker capabilities.
/// </summary>
/// <remarks>
/// 资源闭包由 esbuild 从上游 ESM 树打包：编辑器入口、样式表与五个 worker
/// （editor/json/css/html/typescript）都是自包含 ESM 产物。上游 ESM 树本身不能被浏览器直接加载
/// （120 个模块 import 纯 <c>.css</c>），因此不直接 vendor 上游目录结构。
/// worker 通过 manifest 显式声明，由 Emit 随资源闭包物化。
/// 入口 specifier 经 monaco-editor@0.56.0 的 exports 通配（<c>"./*" → "./esm/vs/*"</c>）
/// 解析到真实的 <c>esm/vs/editor/editor.api.js</c>；不要退回裸包名，那只命中聚合
/// <c>esm/vs/index.js</c>，不是绑定声明的编辑器入口。
/// The closure is bundled by esbuild from the upstream ESM tree: the editor entry, the stylesheet,
/// and five workers (editor/json/css/html/typescript) are self-contained ESM artifacts. The upstream
/// ESM tree cannot be loaded by a browser directly (120 modules import plain <c>.css</c>), so its
/// directory shape is not vendored as-is. Workers are declared in the manifest so Emit materializes
/// them with the closure.
/// </remarks>
[ECMAScript("monaco-editor/editor/editor.api.js")]
[Description("@#")]
public static partial class Monaco
{
}
