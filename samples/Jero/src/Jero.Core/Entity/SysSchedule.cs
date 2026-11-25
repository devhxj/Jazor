namespace Jero.Core;

/// <summary>
/// 系统日程表
/// </summary>
[SugarTable(null, "系统日程表")]
[SysTable]
public class SysSchedule : EntityBaseTenant
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 日程日期
    /// </summary>
    [SugarColumn(ColumnDescription = "日程日期")]
    public DateTime? ScheduleTime { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间", Length = 10)]
    public string? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间", Length = 10)]
    public string? EndTime { get; set; }

    /// <summary>
    /// 日程内容
    /// </summary>
    [SugarColumn(ColumnDescription = "日程内容", Length = 256)]
    [Required, MaxLength(256)]
    public virtual string Content { get; set; } = string.Empty;
    /// <summary>
    /// 完成状态
    /// </summary>
    [SugarColumn(ColumnDescription = "完成状态")]
    public FinishStatusEnum Status { get; set; } = FinishStatusEnum.UnFinish;
}