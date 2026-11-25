namespace Jero.Core.Service;

/// <summary>
/// 接口/动态API输出
/// </summary>
public class ApiOutput
{
    /// <summary>
    /// 组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    /// <summary>
    /// 接口名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>
    /// 路由名称
    /// </summary>
    public string RouteName { get; set; } = string.Empty;
}