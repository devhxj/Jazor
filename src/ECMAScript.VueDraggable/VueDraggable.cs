using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// vue-draggable-plus 入口；提供拖拽排序组合式函数、组件与指令。
/// vue-draggable-plus entry; provides the drag-sort composable, component, and directive.
/// </summary>
/// <remarks>
/// 上游 ESM 入口自包含 sortablejs，只 peer 依赖 Vue（由 <c>ECMAScript.Vue</c> 资源库提供）。
/// 拖拽依赖真实 DOM 与指针事件，必须在浏览器生命周期内使用；SSR 期间不得创建 Sortable 实例。
/// 首期未绑定 <c>vDraggable</c> 指令（Razor 指令编排尚未建模），组件与组合式函数已可用。
/// The upstream ESM entry vendors sortablejs and only peer-depends on Vue (supplied by the
/// <c>ECMAScript.Vue</c> library). Dragging needs a real DOM and pointer events, so it must run
/// within the browser lifecycle; SSR must not create a Sortable instance. The first slice does not
/// bind the <c>vDraggable</c> directive (Razor directive orchestration is not modelled yet); the
/// component and composable are supported.
/// </remarks>
[ECMAScript("vue-draggable-plus")]
[Description("@#")]
public static partial class VueDraggable
{
}
