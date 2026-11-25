namespace Jero.Core.Service;

public class ScheduleInput : BaseIdInput
{
    /// <summary>
    /// 状态
    /// </summary>
    public virtual FinishStatusEnum Status { get; set; }
}

public class ListScheduleInput
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 租户Id
    /// </summary>
    public long TenantId { get; set; }
}

public class AddScheduleInput : SysSchedule
{
    /// <summary>
    /// 日程内容
    /// </summary>
    [Required(ErrorMessage = "日程内容不能为空")]
    public override string Content { get; set; } = string.Empty;
}

public class UpdateScheduleInput : AddScheduleInput
{
}

public class DeleteScheduleInput : BaseIdInput
{
}