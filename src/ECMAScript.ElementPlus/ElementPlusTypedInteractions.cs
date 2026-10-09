namespace ECMAScript.ElementPlus;

/// <summary>Default TableColumn slot payload for dictionary-shaped rows.</summary>
[ECMAScript]
public sealed record ElTableSlotContext : VueProps
{
    /// <summary>The row represented by this cell.</summary>
    [ECMAScriptName("row")]
    public VueDictionary Row { get; init; } = default!;

    /// <summary>The current column metadata.</summary>
    [ECMAScriptName("column")]
    public ElTableColumnContext? Column { get; init; }

    /// <summary>The zero-based row index supplied by Element Plus.</summary>
    [ECMAScriptName("$index")]
    public Number Index { get; init; }
}

/// <summary>Default TableColumn slot payload preserving the authored row type.</summary>
/// <typeparam name="TRow">The table's authored row type.</typeparam>
[ECMAScript]
public sealed record ElTableSlotContext<TRow> : VueProps
{
    /// <summary>The exact row supplied by the table.</summary>
    [ECMAScriptName("row")]
    public TRow Row { get; init; } = default!;

    /// <summary>The current column metadata.</summary>
    [ECMAScriptName("column")]
    public ElTableColumnContext? Column { get; init; }

    /// <summary>The zero-based row index supplied by Element Plus.</summary>
    [ECMAScriptName("$index")]
    public Number Index { get; init; }
}

/// <summary>Determines whether a row's selection checkbox is enabled.</summary>
/// <typeparam name="TRow">The authored row type.</typeparam>
[ECMAScript]
public delegate bool ElTableColumnSelectableCallback<TRow>(TRow row, Number index);

/// <summary>Compares two authored rows for local sorting.</summary>
/// <typeparam name="TRow">The authored row type.</typeparam>
[ECMAScript]
public delegate Number ElTableColumnSortMethodCallback<TRow>(TRow left, TRow right);

/// <summary>Formats a cell while preserving the authored row type.</summary>
/// <typeparam name="TRow">The authored row type.</typeparam>
[ECMAScript]
public delegate VueStringNumberVNodeValue ElTableColumnFormatterCallback<TRow>(
    TRow row, ElTableColumnContext column, VueValue? cellValue, Number index);

/// <summary>Determines whether an authored row matches a column filter.</summary>
/// <typeparam name="TRow">The authored row type.</typeparam>
[ECMAScript]
public delegate bool ElTableColumnFilterMethodCallback<TRow>(string value, TRow row, ElTableColumnContext column);
