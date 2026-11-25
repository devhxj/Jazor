namespace Jero.Core;

/// <summary>
/// 系统打印模板表
/// </summary>
[SugarTable(null, "系统打印模板表")]
[SysTable]
[SugarIndex("index_{table}_N", nameof(Name), OrderByType.Asc)]
public partial class SysPrint : EntityBaseTenant
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 64)]
    [Required, MaxLength(64)]
    public virtual string Name { get; set; } = string.Empty;
    /// <summary>
    /// 打印模板
    /// </summary>
    [SugarColumn(ColumnDescription = "打印模板", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [Required]
    public virtual string Template { get; set; } = string.Empty;
    /// <summary>
    /// 打印类型
    /// </summary>
    [SugarColumn(ColumnDescription = "打印类型")]
    [Required]
    public virtual PrintTypeEnum? PrintType { get; set; }

    /// <summary>
    /// 客户端服务地址
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端服务地址", Length = 128)]
    [MaxLength(128)]
    public virtual string? ClientServiceAddress { get; set; }

    /// <summary>
    /// 打印参数
    /// </summary>
    [SugarColumn(ColumnDescription = "打印参数", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public virtual string? PrintParam { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 128)]
    [MaxLength(128)]
    public string? Remark { get; set; }

    /// <summary>
    /// 打印预览测试数据
    /// </summary>
    [SugarColumn(ColumnDescription = "打印预览测试数据", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? PrintDataDemo { get; set; }
}