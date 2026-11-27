using System;
using System.ComponentModel;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Components;

/// <summary>以表格数据的元素类型约束勾选和点击回调，使事件中的行与列插槽中的行类型一致。</summary>
public sealed record TableData<T>([property: ECMAScriptName("data")] T[] Data) : VueProps
{
    [ECMAScriptName("onSortChange")]
    public Action<TableSortScope>? OnSortChange { get; init; }
    [ECMAScriptName("onSelectionChange")]
    public Action<T[]>? OnSelectionChange { get; init; }
    [ECMAScriptName("onRowClick")]
    public Action<T>? OnRowClick { get; init; }
}

public sealed record TableSortScope(
    [property: ECMAScriptName("prop")] string? Prop,
    [property: ECMAScriptName("order")] string? Order);
public sealed record TableSlot<T>([property: ECMAScriptName("row")] T Row, [property: Description("@#$index")] int Index);
public sealed record TableTagProps([property: ECMAScriptName("type")] string Type) : VueProps;
public sealed record ButtonTitle([property: ECMAScriptName("title")] string Title) : VueProps;
public sealed record SelectableRows<T>([property: ECMAScriptName("selectable")] Func<T, double, bool> Selectable) : VueProps;
