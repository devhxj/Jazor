using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace Jero.Application.Modules.ApprovalFlow;

/// <summary>
/// 审批流程信息表
/// </summary>
[SugarTable(null, "审批流程信息表")]
public class ApprovalFlow : EntityBaseOrgDel
{
    /// <summary>
    /// 编号
    /// </summary>
    [SugarColumn(ColumnDescription = "编号", Length = 32)]
    [MaxLength(32)]
    public string? Code { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 32)]
    [MaxLength(32)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 表单
    /// </summary>
    [SugarColumn(ColumnDescription = "表单", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FormJson { get; set; }

    /// <summary>
    /// 流程
    /// </summary>
    [SugarColumn(ColumnDescription = "流程", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FlowJson { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public int? Status { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = 256)]
    [MaxLength(256)]
    public string? Remark { get; set; }
}
