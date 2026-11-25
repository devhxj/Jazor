namespace Jero.Core.Service;

public class DbColumnInput
{
    public string ConfigId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string DbColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int Length { get; set; }

    public string ColumnDescription { get; set; } = string.Empty;
    public int IsNullable { get; set; }

    public int IsIdentity { get; set; }

    public int IsPrimarykey { get; set; }

    public int DecimalDigits { get; set; }

    public string DefaultValue { get; set; } = string.Empty;
}

public class UpdateDbColumnInput
{
    public string ConfigId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string OldColumnName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
}

public class MoveDbColumnInput
{
    /// <summary>
    /// 数据库配置ID
    /// </summary>
    public string ConfigId { get; set; } = string.Empty;
    /// <summary>
    /// 目标表名
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    /// <summary>
    ///要移动的列名
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// 移动到该列后方（为空时移动到首列）
    /// </summary>
    public string AfterColumnName { get; set; } = string.Empty;
}

public class DeleteDbColumnInput
{
    public string ConfigId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string DbColumnName { get; set; } = string.Empty;
}