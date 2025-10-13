namespace ECMAScript;

/// <summary>
/// SchedulerPostTaskOptions
/// </summary>
[ECMAScript]
[Description("@#SchedulerPostTaskOptions")]
public record SchedulerPostTaskOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default,
	[property: Description("@#priority")]TaskPriority? Priority = default,
	[property: Description("@#delay")]ulong Delay = 0);

/// <summary>
/// TaskPriorityChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#TaskPriorityChangeEventInit")]
public record TaskPriorityChangeEventInit(
    [property: Description("@#previousPriority")]TaskPriority? PreviousPriority = default): EventInit;

/// <summary>
/// TaskControllerInit
/// </summary>
[ECMAScript]
[Description("@#TaskControllerInit")]
public record TaskControllerInit(
    [property: Description("@#priority")]TaskPriority Priority = TaskPriority.UserVisible);

/// <summary>
/// TaskSignalAnyInit
/// </summary>
[ECMAScript]
[Description("@#TaskSignalAnyInit")]
public record TaskSignalAnyInit(
    [property: Description("@#priority")]Either<TaskPriority, TaskSignal>? Priority = default);

/// <summary>
/// Scheduler
/// </summary>
[ECMAScript]
[Description("@#Scheduler")]
public class Scheduler
{
    /// <summary>
    /// postTask
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    [Description("@#postTask")]
    public extern PromiseResult<object> PostTask(SchedulerPostTaskCallback callback, SchedulerPostTaskOptions? options = default);

    /// <summary>
    /// yield
    /// </summary>
    [Description("@#yield")]
    public extern PromiseResult<object> Yield();
}

/// <summary>
/// TaskPriorityChangeEvent
/// </summary>
[ECMAScript]
[Description("@#TaskPriorityChangeEvent")]
public class TaskPriorityChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="priorityChangeEventInitDict">priorityChangeEventInitDict</param>
    public extern TaskPriorityChangeEvent(string type, TaskPriorityChangeEventInit priorityChangeEventInitDict);

    /// <summary>
    /// previousPriority
    /// </summary>
    [Description("@#previousPriority")]
    public extern TaskPriority PreviousPriority { get; }
}

/// <summary>
/// TaskController
/// </summary>
[ECMAScript]
[Description("@#TaskController")]
public class TaskController : AbortController
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern TaskController(TaskControllerInit init);

    /// <summary>
    /// setPriority
    /// </summary>
    /// <param name="priority">priority</param>
    [Description("@#setPriority")]
    public extern void SetPriority(TaskPriority priority);
}

/// <summary>
/// TaskSignal
/// </summary>
[ECMAScript]
[Description("@#TaskSignal")]
public class TaskSignal : AbortSignal
{
    /// <summary>
    /// any
    /// </summary>
    /// <param name="signals">signals</param>
    /// <param name="init">init</param>
    [Description("@#any")]
    public extern static TaskSignal Any(AbortSignal[] signals, TaskSignalAnyInit? init = default);

    /// <summary>
    /// priority
    /// </summary>
    [Description("@#priority")]
    public extern TaskPriority Priority { get; }

    /// <summary>
    /// onprioritychange
    /// </summary>
    [Description("@#onprioritychange")]
    public extern EventHandler Onprioritychange { get; set; }
}