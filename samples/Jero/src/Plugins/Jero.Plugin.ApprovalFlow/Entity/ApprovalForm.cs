namespace Jero.Plugin.ApprovalFlow;

/// <summary>
/// 审批流表单
/// </summary>
[SugarTable(null, "审批流表单")]
public class ApprovalForm : EntityBaseOrg
{
    /// <summary>
    /// 编号
    /// </summary>
    [SugarColumn(ColumnDescription = "编号", Length = 32)]
    public string? Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 32)]
    public string? Name { get; set; }

    /// <summary>
    /// 表单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "表单名称", Length = 32)]
    public string? FormName { get; set; }

    /// <summary>
    /// 表单属性
    /// </summary>
    [SugarColumn(ColumnDescription = "表单属性", Length = 32)]
    public string? FormType { get; set; }

    /// <summary>
    /// 表单状态
    /// </summary>
    [SugarColumn(ColumnDescription = "表单状态")]
    public int? FormStatus { get; set; }

    /// <summary>
    /// 表单结果
    /// </summary>
    [SugarColumn(ColumnDescription = "表单结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormResult { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public int? Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 255)]
    public string? Remark { get; set; }
}