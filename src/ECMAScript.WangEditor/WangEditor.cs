using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// WangEditor 入口；提供编辑器与工具栏的 Vue 组件创建入口。
/// WangEditor entry; provides the Vue component entries for the editor and toolbar.
/// </summary>
/// <remarks>
/// 作者入口是官方 Vue 3 适配器 <c>@wangeditor/editor-for-vue</c>（<c>Editor</c> 与 <c>Toolbar</c>
/// 命名导出）；编辑器核心 <c>@wangeditor/editor</c> 由同一资源闭包提供，不在 C# 侧单独绑定。
/// 编辑器依赖真实 DOM、选区与剪贴板，必须在浏览器生命周期内挂载；SSR 期间不得创建实例。
/// 首期未绑定命令式 <c>createEditor</c>/<c>createToolbar</c>（组件已覆盖挂载/销毁与 value 双向绑定）。
/// The author entry is the official Vue 3 adapter <c>@wangeditor/editor-for-vue</c> (named exports
/// <c>Editor</c> and <c>Toolbar</c>); the editor core <c>@wangeditor/editor</c> is supplied by the
/// same resource closure rather than a separate C# binding. The editor needs a real DOM, selection,
/// and clipboard, so it must mount within the browser lifecycle; SSR must not create an instance.
/// The first slice does not bind the imperative <c>createEditor</c>/<c>createToolbar</c> (the
/// components already cover mount/destroy and two-way value binding).
/// </remarks>
[ECMAScript("@wangeditor/editor-for-vue")]
[Description("@#")]
public static partial class WangEditor
{
}
