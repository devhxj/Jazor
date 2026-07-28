namespace JazorAdmin;

[ECMAScript]
[Description("@#")]
public sealed record ReleaseTableColumn : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#title")]
    public string? Title { get; init; }

    [Description("@#width")]
    public string? Width { get; init; }

    [Description("@#sortable")]
    public bool? Sortable { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record ReleaseTableCell : VueProps
{
    [Description("@#columnKey")]
    public string ColumnKey { get; init; } = string.Empty;

    [Description("@#text")]
    public string? Text { get; init; }
}

[ECMAScript]
[Description("@#")]
public sealed record ReleaseTableRow : VueProps
{
    [Description("@#key")]
    public string Key { get; init; } = string.Empty;

    [Description("@#cells")]
    public ReleaseTableCells? Cells { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(ReleaseTableColumnsCollectionBuilder), nameof(ReleaseTableColumnsCollectionBuilder.Create))]
public readonly union ReleaseTableColumns(ReleaseTableColumn[]) : IEnumerable<ReleaseTableColumn>
{
    public ReleaseTableColumn[]? AsArray => Value as ReleaseTableColumn[];

    IEnumerator<ReleaseTableColumn> IEnumerable<ReleaseTableColumn>.GetEnumerator()
        => ((IEnumerable<ReleaseTableColumn>)(AsArray ?? Array.Empty<ReleaseTableColumn>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ReleaseTableColumn>)this).GetEnumerator();
}

public static class ReleaseTableColumnsCollectionBuilder
{
    public static ReleaseTableColumns Create(ReadOnlySpan<ReleaseTableColumn> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(ReleaseTableRowsCollectionBuilder), nameof(ReleaseTableRowsCollectionBuilder.Create))]
public readonly union ReleaseTableRows(ReleaseTableRow[]) : IEnumerable<ReleaseTableRow>
{
    public ReleaseTableRow[]? AsArray => Value as ReleaseTableRow[];

    IEnumerator<ReleaseTableRow> IEnumerable<ReleaseTableRow>.GetEnumerator()
        => ((IEnumerable<ReleaseTableRow>)(AsArray ?? Array.Empty<ReleaseTableRow>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ReleaseTableRow>)this).GetEnumerator();
}

public static class ReleaseTableRowsCollectionBuilder
{
    public static ReleaseTableRows Create(ReadOnlySpan<ReleaseTableRow> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(ReleaseTableCellsCollectionBuilder), nameof(ReleaseTableCellsCollectionBuilder.Create))]
public readonly union ReleaseTableCells(ReleaseTableCell[]) : IEnumerable<ReleaseTableCell>
{
    public ReleaseTableCell[]? AsArray => Value as ReleaseTableCell[];

    IEnumerator<ReleaseTableCell> IEnumerable<ReleaseTableCell>.GetEnumerator()
        => ((IEnumerable<ReleaseTableCell>)(AsArray ?? Array.Empty<ReleaseTableCell>())).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<ReleaseTableCell>)this).GetEnumerator();
}

public static class ReleaseTableCellsCollectionBuilder
{
    public static ReleaseTableCells Create(ReadOnlySpan<ReleaseTableCell> values)
        => values.ToArray();
}
