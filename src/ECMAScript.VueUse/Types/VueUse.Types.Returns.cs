using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// <c>useNetwork()</c> 的返回值：响应式网络连接状态。
/// The <c>useNetwork()</c> return value: reactive network connection state.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseNetworkReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseNetworkReturn()
    {
    }

    /// <summary>当前环境是否支持该 API。Whether the current environment supports this API.</summary>
    [Description("@#isSupported")]
    public extern Vue.VueComputedRef<bool> IsSupported { get; }

    /// <summary>当前是否在线。Whether the browser is currently online.</summary>
    [Description("@#isOnline")]
    public extern Vue.VueReadonlyRef<bool> IsOnline { get; }

    /// <summary>最近一次离线的时间戳；从未离线时为 undefined。The timestamp of the most recent offline transition, undefined when never offline.</summary>
    [Description("@#offlineAt")]
    public extern Vue.VueReadonlyRef<Number?> OfflineAt { get; }

    /// <summary>最近一次在线的时间戳；未知时为 undefined。The timestamp of the most recent online transition, undefined when unknown.</summary>
    [Description("@#onlineAt")]
    public extern Vue.VueReadonlyRef<Number?> OnlineAt { get; }

    /// <summary>下行带宽估计值（兆比特每秒）。The estimated downlink bandwidth in megabits per second.</summary>
    [Description("@#downlink")]
    public extern Vue.VueReadonlyRef<Number?> Downlink { get; }

    /// <summary>下行带宽上限估计值。The estimated maximum downlink bandwidth.</summary>
    [Description("@#downlinkMax")]
    public extern Vue.VueReadonlyRef<Number?> DownlinkMax { get; }

    /// <summary>有效连接类型。The effective connection type.</summary>
    [Description("@#effectiveType")]
    public extern Vue.VueReadonlyRef<VueUseNetworkEffectiveType?> EffectiveType { get; }

    /// <summary>往返时延估计值（毫秒）。The estimated round-trip time in milliseconds.</summary>
    [Description("@#rtt")]
    public extern Vue.VueReadonlyRef<Number?> Rtt { get; }

    /// <summary>连接类型标识。The connection type identifier.</summary>
    [Description("@#type")]
    public extern Vue.VueReadonlyRef<string?> Type { get; }
}

/// <summary>
/// <c>useBattery()</c> 的返回值：响应式电池状态。
/// The <c>useBattery()</c> return value: reactive battery state.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseBatteryReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseBatteryReturn()
    {
    }

    /// <summary>当前环境是否支持该 API。Whether the current environment supports this API.</summary>
    [Description("@#isSupported")]
    public extern Vue.VueComputedRef<bool> IsSupported { get; }

    /// <summary>是否正在充电。Whether the battery is charging.</summary>
    [Description("@#charging")]
    public extern Vue.VueShallowRef<bool> Charging { get; }

    /// <summary>距离充满的剩余秒数。The remaining seconds until fully charged.</summary>
    [Description("@#chargingTime")]
    public extern Vue.VueShallowRef<Number> ChargingTime { get; }

    /// <summary>距离耗尽的剩余秒数。The remaining seconds until fully discharged.</summary>
    [Description("@#dischargingTime")]
    public extern Vue.VueShallowRef<Number> DischargingTime { get; }

    /// <summary>当前电量比例（0 到 1）。The current battery level between 0 and 1.</summary>
    [Description("@#level")]
    public extern Vue.VueShallowRef<Number> Level { get; }
}

/// <summary>
/// <c>useIdle()</c> 的返回值。
/// The <c>useIdle()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseIdleReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseIdleReturn()
    {
    }

    /// <summary>用户是否处于空闲状态。Whether the user is idle.</summary>
    [Description("@#idle")]
    public extern Vue.VueShallowRef<bool> Idle { get; }

    /// <summary>最近一次活跃的时间戳。The timestamp of the most recent activity.</summary>
    [Description("@#lastActive")]
    public extern Vue.VueShallowRef<Number> LastActive { get; }

    /// <summary>重置空闲计时。Resets the idle timer.</summary>
    [Description("@#reset")]
    public extern void Reset();
}

/// <summary>
/// <c>useColorMode()</c> 的返回值：当前模式、存储值与系统偏好。
/// The <c>useColorMode()</c> return value: the current mode, stored value, and system preference.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseColorModeReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseColorModeReturn()
    {
    }

    /// <summary>当前生效的配色模式。The currently applied color mode.</summary>
    [Description("@#value")]
    public extern VueUseColorMode Value { get; set; }

    /// <summary>持久化的模式值。The persisted mode value.</summary>
    [Description("@#store")]
    public extern Vue.IVueRef<VueUseColorMode> Store { get; }

    /// <summary>系统偏好模式。The system preference mode.</summary>
    [Description("@#system")]
    public extern Vue.VueComputedRef<VueUseColorMode> System { get; }

    /// <summary>解析自动模式后的当前状态模式。The resolved state mode after resolving the automatic mode.</summary>
    [Description("@#state")]
    public extern Vue.VueComputedRef<VueUseColorMode> State { get; }
}

/// <summary>
/// 可暂停/恢复的定时控制；由 <c>useIntervalFn</c>、<c>useTimeoutFn</c>、<c>useRafFn</c> 返回。
/// A pausable timing control returned by <c>useIntervalFn</c>, <c>useTimeoutFn</c>, and <c>useRafFn</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUsePausableControl
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUsePausableControl()
    {
    }

    /// <summary>定时是否处于活跃状态。Whether the timer is active.</summary>
    [Description("@#isActive")]
    public extern Vue.IVueRef<bool> IsActive { get; }

    /// <summary>暂停定时。Pauses the timer.</summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>恢复定时。Resumes the timer.</summary>
    [Description("@#resume")]
    public extern void Resume();
}

/// <summary>
/// VueUse 事件钩子；发布事件并注册/注销监听器。
/// A VueUse event hook that triggers events and manages listeners.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseEventHook<T>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseEventHook()
    {
    }

    /// <summary>注册事件监听器，返回注销函数。Registers a listener and returns a deregistration function.</summary>
    /// <param name="handler">事件处理回调。The event handler callback.</param>
    /// <returns>调用后注销该监听器的函数。A function that deregisters the listener when invoked.</returns>
    [Description("@#on")]
    public extern Action On(Action<T> handler);

    /// <summary>触发事件。Triggers the event.</summary>
    /// <param name="value">随事件传递的值。The value passed with the event.</param>
    [Description("@#trigger")]
    public extern void Trigger(T value);
}

/// <summary>
/// <c>useAsyncState()</c> 的选项对象。
/// Options for <c>useAsyncState()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseAsyncStateOptions
{
    /// <summary>是否立即执行。Whether to execute immediately.</summary>
    [Description("@#immediate")]
    public bool? Immediate { get; init; }

    /// <summary>执行前延迟的毫秒数。The delay in milliseconds before execution.</summary>
    [Description("@#delay")]
    public Number? Delay { get; init; }

    /// <summary>每次执行前是否重置状态。Whether to reset state before each execution.</summary>
    [Description("@#resetOnExecute")]
    public bool? ResetOnExecute { get; init; }

    /// <summary>是否使用浅层引用。Whether to use a shallow ref.</summary>
    [Description("@#shallow")]
    public bool? Shallow { get; init; }
}

/// <summary>
/// <c>useAsyncState()</c> 的返回值：状态引用与手动执行入口。
/// The <c>useAsyncState()</c> return value: state refs and manual execution entries.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseAsyncStateReturn<T>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseAsyncStateReturn()
    {
    }

    /// <summary>异步结果状态。The asynchronous result state.</summary>
    [Description("@#state")]
    public extern Vue.IVueRef<T> State { get; }

    /// <summary>是否已有可用结果。Whether a result is available.</summary>
    [Description("@#isReady")]
    public extern Vue.IVueRef<bool> IsReady { get; }

    /// <summary>是否正在加载。Whether execution is in progress.</summary>
    [Description("@#isLoading")]
    public extern Vue.IVueRef<bool> IsLoading { get; }

    /// <summary>最近一次执行的错误；无错误时为 undefined。The error from the most recent execution, undefined when none.</summary>
    [Description("@#error")]
    public extern Vue.IVueRef<Error?> Error { get; }

    /// <summary>执行异步操作。Executes the asynchronous operation.</summary>
    /// <param name="delay">可选延迟毫秒数。An optional delay in milliseconds.</param>
    /// <returns>Promise 包装的执行结果。Promise-wrapped execution result.</returns>
    [Description("@#execute")]
    public extern IPromise<T> Execute(Number? delay = null);
}
