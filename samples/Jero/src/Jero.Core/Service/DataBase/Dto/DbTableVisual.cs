namespace Jero.Core.Service;

public class VisualDb
{
    public string ConfigId { get; set; } = string.Empty;
    public string DbNickName { get; set; } = string.Empty;
}

/// <summary>
/// 库表可视化
/// </summary>
public class VisualDbTable
{
    public List<VisualTable> VisualTableList { get; set; } = [];
    public List<VisualColumn> VisualColumnList { get; set; } = [];
    public List<ColumnRelation> ColumnRelationList { get; set; } = [];
}

public class VisualTable
{
    public string TableName { get; set; } = string.Empty;
    public string TableComents { get; set; } = string.Empty;
    public int X { get; set; }

    public int Y { get; set; }
}

public class VisualColumn
{
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string DataLength { get; set; } = string.Empty;
    public string ColumnDescription { get; set; } = string.Empty;
}

public class ColumnRelation
{
    public string SourceTableName { get; set; } = string.Empty;
    public string SourceColumnName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TargetTableName { get; set; } = string.Empty;
    public string TargetColumnName { get; set; } = string.Empty;
}