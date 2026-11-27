namespace Jero.Web.Application;

public sealed record DatabaseColumnView
{
    public string TableName { get; init; } = string.Empty;
    public int TableId { get; init; }
    public required string DbColumnName { get; init; }
    public required string PropertyName { get; init; }
    public required string DataType { get; init; }
    public required int Length { get; init; }
    public required string ColumnDescription { get; init; }
    public required string DefaultValue { get; init; }
    public required bool IsNullable { get; init; }
    public required bool IsIdentity { get; init; }
    public required bool IsPrimarykey { get; init; }
    public required int DecimalDigits { get; init; }
    public int Scale { get; init; }
    public bool IsArray { get; init; }
    public bool IsJson { get; init; }
    public bool IsUnsigned { get; init; }
    public int CreateTableFieldSort { get; init; }
}
