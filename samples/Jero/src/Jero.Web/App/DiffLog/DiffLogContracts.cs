namespace Jero.Web.App.DiffLog;

public sealed record DiffLogView
{
    public required string id { get; init; }
    public required string tenantId { get; init; }
    public required string diffData { get; init; }
    public required string sql { get; init; }
    public required string parameters { get; init; }
    public required string businessData { get; init; }
    public required string diffType { get; init; }
    public required string elapsed { get; init; }
    public required string createTime { get; init; }
    public DiffLogTableView[] tableItems { get; init; } = [];
    public DiffLogParameterView[] parameterItems { get; init; } = [];
}

public sealed record DiffLogTableView
{
    public required string tableName { get; init; }
    public required string tableDescription { get; init; }
    public DiffLogColumnView[] columns { get; init; } = [];
}

public sealed record DiffLogColumnView
{
    public required string columnName { get; init; }
    public required string columnDescription { get; init; }
    public required bool isPrimaryKey { get; init; }
    public required string beforeValue { get; init; }
    public required string afterValue { get; init; }
}

public sealed record DiffLogParameterView
{
    public required string parameterName { get; init; }
    public required string typeName { get; init; }
    public required string value { get; init; }
}
