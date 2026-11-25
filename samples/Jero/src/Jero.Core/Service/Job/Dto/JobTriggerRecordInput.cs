namespace Jero.Core.Service;

public class PageJobTriggerRecordInput : BasePageInput
{
    /// <summary>
    /// 作业Id
    /// </summary>
    public string JobId { get; set; } = string.Empty;
    /// <summary>
    /// 触发器Id
    /// </summary>
    public string TriggerId { get; set; } = string.Empty;
}