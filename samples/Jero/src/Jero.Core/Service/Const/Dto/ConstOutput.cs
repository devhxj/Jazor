namespace Jero.Core.Service;

public class ConstOutput
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// 编码
    /// </summary>
    public object? Code { get; set; }

    /// <summary>
    /// 扩展字段
    /// </summary>
    public object? Data { get; set; }
}
