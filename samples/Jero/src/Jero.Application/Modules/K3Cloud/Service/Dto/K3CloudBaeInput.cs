using System.ComponentModel.DataAnnotations;

namespace Jero.Application.Modules.K3Cloud.Service;

/// <summary>
/// ERP基础入参
/// </summary>
public class K3CloudBaeInput<T>
{
    /// <summary>
    /// 表单Id
    /// </summary>
    public string formid { get; set; } = string.Empty;

    /// <summary>
    /// 数据包
    /// </summary>
    [Required]
    public T? data { get; set; }
}
