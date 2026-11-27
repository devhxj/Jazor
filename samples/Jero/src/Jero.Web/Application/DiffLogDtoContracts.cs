namespace Jero.Web.Application;

public sealed record DiffLogView
{
    public required string Id { get; init; }
    public required string TenantId { get; init; }
    public required string DiffData { get; init; }
    public required string Sql { get; init; }
    public required string Parameters { get; init; }
    public required string BusinessData { get; init; }
    public required string DiffType { get; init; }
    public required string Elapsed { get; init; }
    public required string CreateTime { get; init; }
    public DiffLogTableView[] TableItems { get; init; } = [];
    public DiffLogParameterView[] ParameterItems { get; init; } = [];
}

public sealed record DiffLogTableView
{
    public required string TableName { get; init; }
    public required string TableDescription { get; init; }
    public DiffLogColumnView[] Columns { get; init; } = [];
}

public sealed record DiffLogColumnView
{
    public required string ColumnName { get; init; }
    public required string ColumnDescription { get; init; }
    public required bool IsPrimaryKey { get; init; }
    public required string BeforeValue { get; init; }
    public required string AfterValue { get; init; }
}

public sealed record DiffLogParameterView
{
    public required string ParameterName { get; init; }
    public required string TypeName { get; init; }
    public required string Value { get; init; }
}
