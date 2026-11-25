namespace Jero.Core;

/// <summary>
/// 设置状态输入参数
/// </summary>
public class BaseStatusInput : BaseIdInput
{
    /// <summary>
    /// 状态
    /// </summary>
    [Enum]
    public StatusEnum Status { get; set; }
}