using SqlSugar;

namespace Jero.Application.Modules.ApprovalFlow;

/// <summary>
/// 审批流流程记录
/// </summary>
[SugarTable(null, "审批流流程记录")]
public class ApprovalFlowRecord : EntityBaseOrg
{
    /// <summary>
    /// 表单名称
    /// </summary>
    [SugarColumn(ColumnDescription = "表单名称", Length = 255)]
    public string? FormName { get; set; }

    /// <summary>
    /// 表单状态
    /// </summary>
    [SugarColumn(ColumnDescription = "表单状态", Length = 32)]
    public string? FormStatus { get; set; }

    /// <summary>
    /// 表单触发
    /// </summary>
    [SugarColumn(ColumnDescription = "表单触发", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormJson { get; set; }

    /// <summary>
    /// 表单结果
    /// </summary>
    [SugarColumn(ColumnDescription = "表单结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormResult { get; set; }

    /// <summary>
    /// 流程结构
    /// </summary>
    [SugarColumn(ColumnDescription = "流程结构", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FlowJson { get; set; }

    /// <summary>
    /// 流程结果
    /// </summary>
    [SugarColumn(ColumnDescription = "流程结果", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FlowResult { get; set; }
}