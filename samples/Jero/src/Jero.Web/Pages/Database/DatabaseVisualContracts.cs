namespace Jero.Web.Pages.Database;

internal sealed record DatabaseDiagram(DatabaseDiagramNode[] Nodes, DatabaseDiagramRelation[] Relations);

public sealed record DatabaseDiagramNode
{
    public required string Id { get; init; }
    public required string Text { get; init; }
    public required string Html { get; init; }
    public required double X { get; init; }
    public required double Y { get; init; }
}

public sealed record DatabaseDiagramRelation
{
    public required string Id { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
    public required string Color { get; init; }
}

public sealed record DatabaseColumnDraft
{
    public required int Key { get; init; }
    public required string DbColumnName { get; init; }
    public required string DataType { get; init; }
    public required int Length { get; init; }
    public required string ColumnDescription { get; init; }
    public int IsNullable { get; init; }
    public int IsIdentity { get; init; }
    public int IsPrimarykey { get; init; }
    public required int DecimalDigits { get; init; }
    public required string DefaultValue { get; init; }
}
