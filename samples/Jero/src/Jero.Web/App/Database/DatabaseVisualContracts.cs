namespace Jero.Web.App.Database;

internal sealed record DatabaseDiagram(DatabaseDiagramNode[] Nodes, DatabaseDiagramRelation[] Relations);

public sealed record DatabaseDiagramNode
{
    public required string id { get; init; }
    public required string text { get; init; }
    public required string html { get; init; }
    public required double x { get; init; }
    public required double y { get; init; }
}

public sealed record DatabaseDiagramRelation
{
    public required string id { get; init; }
    public required string from { get; init; }
    public required string to { get; init; }
    public required string color { get; init; }
}

public sealed record DatabaseColumnView
{
    public string tableName { get; init; } = string.Empty;
    public int tableId { get; init; }
    public required string dbColumnName { get; init; }
    public required string propertyName { get; init; }
    public required string dataType { get; init; }
    public required int length { get; init; }
    public required string columnDescription { get; init; }
    public required string defaultValue { get; init; }
    public required bool isNullable { get; init; }
    public required bool isIdentity { get; init; }
    public required bool isPrimarykey { get; init; }
    public required int decimalDigits { get; init; }
    public int scale { get; init; }
    public bool isArray { get; init; }
    public bool isJson { get; init; }
    public bool isUnsigned { get; init; }
    public int createTableFieldSort { get; init; }
}

public sealed record DatabaseColumnDraft
{
    public required int key { get; init; }
    public required string dbColumnName { get; init; }
    public required string dataType { get; init; }
    public required int length { get; init; }
    public required string columnDescription { get; init; }
    public int isNullable { get; init; }
    public int isIdentity { get; init; }
    public int isPrimarykey { get; init; }
    public required int decimalDigits { get; init; }
    public required string defaultValue { get; init; }
}

[global::ECMAScript.ECMAScript]
public sealed class DatabaseGraphActions
{
    [System.ComponentModel.Description("@#mount")]
    public extern void mount(DatabaseDiagramNode[] nodes, DatabaseDiagramRelation[] relations);
    [System.ComponentModel.Description("@#update")]
    public extern void update(DatabaseDiagramNode[] nodes, DatabaseDiagramRelation[] relations);
    [System.ComponentModel.Description("@#dispose")]
    public extern void dispose();
    [System.ComponentModel.Description("@#zoom")]
    public extern void zoom(double offset);
    [System.ComponentModel.Description("@#center")]
    public extern void center();
    [System.ComponentModel.Description("@#fit")]
    public extern void fit();
    [System.ComponentModel.Description("@#reset")]
    public extern void reset();
}
