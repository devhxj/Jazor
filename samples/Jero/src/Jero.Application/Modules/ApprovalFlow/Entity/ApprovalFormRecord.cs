using SqlSugar;

namespace Jero.Application.Modules.ApprovalFlow;

/// <summary>
/// 审批流表单记录
/// </summary>
[SugarTable(null, "审批流表单记录")]
public class ApprovalFormRecord : EntityBaseOrg
{
    /// <summary>
    /// 流程Id
    /// </summary>
    [SugarColumn(ColumnDescription = "流程Id")]
    public long? FlowId { get; set; }

    /// <summary>
    /// 表单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "表单名称", Length = 32)]
    public string? FormName { get; set; }

    /// <summary>
    /// 表单类型
    /// </summary>
    [SugarColumn(ColumnDescription = "表单类型", Length = 32)]
    public string? FormType { get; set; }

    /// <summary>
    /// 表单状态
    /// </summary>
    [SugarColumn(ColumnDescription = "表单状态", Length = 11)]
    public string? FormStatus { get; set; }

    /// <summary>
    /// 修改前
    /// </summary>
    [SugarColumn(ColumnDescription = "修改前", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormBefore { get; set; }

    /// <summary>
    /// 修改后
    /// </summary>
    [SugarColumn(ColumnDescription = "修改后", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormAfter { get; set; }

    /// <summary>
    /// 表单结果
    /// </summary>
    [SugarColumn(ColumnDescription = "表单结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormResult { get; set; }
}