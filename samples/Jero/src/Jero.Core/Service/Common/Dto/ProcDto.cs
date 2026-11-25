


using Magicodes.ExporterAndImporter.Core.Filters;
using Magicodes.ExporterAndImporter.Core.Models;
namespace Jero.Core.Service;

/// <summary>
/// 基础存储过程输入类
/// </summary>
public class BaseProcInput
{
    /// <summary>
    /// ProcId
    /// </summary>
    public string ProcId { get; set; } = string.Empty;
    /// <summary>
    /// 数据库配置Id
    /// </summary>
    public string ConfigId { get; set; } = SqlSugarConst.MainConfigId;

    /// <summary>
    /// 存储过程输入参数
    /// </summary>
    /// <example>{"id":"351060822794565"}</example>
    public Dictionary<string, object> ProcParams { get; set; } = [];
}

/// <summary>
/// 带表头名称存储过程输入类
/// </summary>
public class ExportProcByTMPInput : BaseProcInput
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Template { get; set; } = string.Empty;
}

/// <summary>
/// 带表头名称存储过程输入类
/// </summary>
public class ExportProcInput : BaseProcInput
{
    public Dictionary<string, string> EHeader { get; set; } = [];
}

/// <summary>
/// 指定导出类名（有排序）存储过程输入类
/// </summary>
public class ExportProcInput2 : BaseProcInput
{
    public List<string> EHeader { get; set; } = [];
}

/// <summary>
/// 前端指定列
/// </summary>
public class ProcExporterHeaderFilter : IExporterHeaderFilter
{
    private Dictionary<string, Tuple<string, int>> _includeHeader;

    public ProcExporterHeaderFilter(Dictionary<string, Tuple<string, int>> includeHeader)
    {
        _includeHeader = includeHeader;
    }

    public ExporterHeaderInfo Filter(ExporterHeaderInfo exporterHeaderInfo)
    {
        if (_includeHeader != null && _includeHeader.Count > 0)
        {
            var key = exporterHeaderInfo.PropertyName.ToUpper();
            if (_includeHeader.ContainsKey(key))
            {
                exporterHeaderInfo.DisplayName = _includeHeader[key].Item1;
                return exporterHeaderInfo;
            }
            else
            {
                exporterHeaderInfo.ExporterHeaderAttribute.Hidden = true;
            }
        }
        return exporterHeaderInfo;
    }
}