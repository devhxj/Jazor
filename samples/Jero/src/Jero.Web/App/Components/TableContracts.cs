using System;
using System.ComponentModel;
using static ECMAScript.Vue;

namespace Jero.Web.App.Components;

// Element Plus takes its generic row type from data. Keep the Vue event payloads
// and the column slot on that same row type when passing them through Jolt 2.7.
public sealed record TableData<T>(T[] data) : VueProps
{
    public Action<TableSortScope>? onSortChange { get; init; }
    public Action<T[]>? onSelectionChange { get; init; }
    public Action<T>? onRowClick { get; init; }
}

public sealed record TableSortScope(string? prop, string? order);
public sealed record TableSlot<T>(T row, [property: Description("@#$index")] int Index);
public sealed record TableTagProps(string type) : VueProps;
public sealed record ButtonTitle(string title) : VueProps;
public sealed record SelectableRows<T>(Func<T, double, bool> selectable) : VueProps;
