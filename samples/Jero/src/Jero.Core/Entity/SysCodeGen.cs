namespace Jero.Core;

/// <summary>
/// 代码生成表
/// </summary>
[SugarTable(null, "代码生成表")]
[SysTable]
[SugarIndex("index_{table}_B", nameof(BusName), OrderByType.Asc)]
[SugarIndex("index_{table}_T", nameof(TableName), OrderByType.Asc)]
public partial class SysCodeGen : EntityBase
{
    /// <summary>
    /// 作者姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "作者姓名", Length = 32)]
    [MaxLength(32)]
    public string? AuthorName { get; set; }

    /// <summary>
    /// 是否移除表前缀
    /// </summary>
    [SugarColumn(ColumnDescription = "是否移除表前缀", Length = 8)]
    [MaxLength(8)]
    public string? TablePrefix { get; set; }

    /// <summary>
    /// 生成方式
    /// </summary>
    [SugarColumn(ColumnDescription = "生成方式", Length = 32)]
    [MaxLength(32)]
    public string? GenerateType { get; set; }

    /// <summary>
    /// 库定位器名
    /// </summary>
    [SugarColumn(ColumnDescription = "库定位器名", Length = 64)]
    [MaxLength(64)]
    public string? ConfigId { get; set; }

    /// <summary>
    /// 库名
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public string DbNickName
    {
        get
        {
            try
            {
                var dbOptions = App.GetConfig<DbConnectionOptions>("DbConnection", true);
                var config = dbOptions.ConnectionConfigs.FirstOrDefault(m => m.ConfigId.ToString() == ConfigId);
                return config?.DbNickName ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// 数据库名(保留字段)
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库库名", Length = 64)]
    [MaxLength(64)]
    public string? DbName { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型", Length = 64)]
    [MaxLength(64)]
    public string? DbType { get; set; }

    /// <summary>
    /// 数据库链接
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库链接", Length = 256)]
    [MaxLength(256)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 数据库表名
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库表名", Length = 128)]
    [MaxLength(128)]
    public string? TableName { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    [SugarColumn(ColumnDescription = "命名空间", Length = 128)]
    [MaxLength(128)]
    public string? NameSpace { get; set; }

    /// <summary>
    /// 业务名
    /// </summary>
    [SugarColumn(ColumnDescription = "业务名", Length = 128)]
    [MaxLength(128)]
    public string? BusName { get; set; }

    /// <summary>
    /// 表唯一字段配置
    /// </summary>
    [SugarColumn(ColumnDescription = "表唯一字段配置", Length = 512)]
    [MaxLength(128)]
    public string? TableUniqueConfig { get; set; }

    /// <summary>
    /// 是否生成菜单
    /// </summary>
    [SugarColumn(ColumnDescription = "是否生成菜单")]
    public bool GenerateMenu { get; set; } = true;

    /// <summary>
    /// 菜单图标
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单图标", Length = 32)]
    public string? MenuIcon { get; set; } = "ele-Menu";

    /// <summary>
    /// 菜单编码
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单编码")]
    public long? MenuPid { get; set; }

    /// <summary>
    /// 页面目录
    /// </summary>
    [SugarColumn(ColumnDescription = "页面目录", Length = 32)]
    public string? PagePath { get; set; }

    /// <summary>
    /// 支持打印类型
    /// </summary>
    [SugarColumn(ColumnDescription = "支持打印类型", Length = 32)]
    [MaxLength(32)]
    public string? PrintType { get; set; }

    /// <summary>
    /// 打印模版名称
    /// </summary>
    [SugarColumn(ColumnDescription = "打印模版名称", Length = 32)]
    [MaxLength(32)]
    public string? PrintName { get; set; }

    /// <summary>
    /// 表唯一字段列表
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public virtual List<TableUniqueConfigItem> TableUniqueList => string.IsNullOrWhiteSpace(TableUniqueConfig)
        ? []
        : JSON.Deserialize<List<TableUniqueConfigItem>>(TableUniqueConfig) ?? [];
}
