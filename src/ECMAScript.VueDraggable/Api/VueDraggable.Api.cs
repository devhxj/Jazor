using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

public static partial class VueDraggable
{
    /// <summary>
    /// 创建拖拽排序实例；返回 Sortable 方法投影与生命周期控制。
    /// Creates a drag-sort instance, returning the Sortable method projections and lifecycle control.
    /// </summary>
    /// <param name="element">容器元素。The container element.</param>
    /// <param name="list">与容器绑定的列表引用。The list ref bound to the container.</param>
    /// <param name="options">拖拽选项。The drag options.</param>
    /// <typeparam name="TItem">列表元素类型。The list item type.</typeparam>
    [Description("@#useDraggable")]
    public extern static VueDraggableReturn UseDraggable<TItem>(
        Element element,
        Vue.IVueRef<TItem[]> list,
        VueDraggableOptions? options = null);
}
