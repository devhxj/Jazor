using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 用户减少动效偏好；字符串值与 VueUse <c>ReducedMotionType</c> 值域一致。
/// The reduced-motion preference; values mirror the VueUse <c>ReducedMotionType</c> domain.
/// </summary>
[String]
public enum VueUseReducedMotion
{
    /// <summary>减少动效。Reduced motion.</summary>
    [Description("@#reduce")]
    Reduce,

    /// <summary>未声明偏好。No declared preference.</summary>
    [Description("@#no-preference")]
    NoPreference
}

/// <summary>
/// 键盘修饰键；字符串值与 VueUse <c>KeyModifier</c> 值域一致。
/// A keyboard modifier key; values mirror the VueUse <c>KeyModifier</c> domain.
/// </summary>
[String]
public enum VueUseKeyModifier
{
    /// <summary>Alt 键。The Alt key.</summary>
    [Description("@#Alt")]
    Alt,

    /// <summary>AltGraph 键。The AltGraph key.</summary>
    [Description("@#AltGraph")]
    AltGraph,

    /// <summary>CapsLock 键。The CapsLock key.</summary>
    [Description("@#CapsLock")]
    CapsLock,

    /// <summary>Control 键。The Control key.</summary>
    [Description("@#Control")]
    Control,

    /// <summary>Fn 键。The Fn key.</summary>
    [Description("@#Fn")]
    Fn,

    /// <summary>FnLock 键。The FnLock key.</summary>
    [Description("@#FnLock")]
    FnLock,

    /// <summary>Meta 键。The Meta key.</summary>
    [Description("@#Meta")]
    Meta,

    /// <summary>NumLock 键。The NumLock key.</summary>
    [Description("@#NumLock")]
    NumLock,

    /// <summary>ScrollLock 键。The ScrollLock key.</summary>
    [Description("@#ScrollLock")]
    ScrollLock,

    /// <summary>Shift 键。The Shift key.</summary>
    [Description("@#Shift")]
    Shift,

    /// <summary>Symbol 键。The Symbol key.</summary>
    [Description("@#Symbol")]
    Symbol,

    /// <summary>SymbolLock 键。The SymbolLock key.</summary>
    [Description("@#SymbolLock")]
    SymbolLock
}

/// <summary>
/// <c>useBase64()</c> 的选项对象。
/// Options for <c>useBase64()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseBase64Options
{
    /// <summary>输出为 data URL 而非裸 base64 文本。Whether the output is a data URL rather than bare base64 text.</summary>
    [Description("@#dataUrl")]
    public bool? DataUrl { get; init; }
}

/// <summary>
/// <c>useBase64()</c> 的返回值。
/// The <c>useBase64()</c> return value.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseBase64Return
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseBase64Return()
    {
    }

    /// <summary>当前 base64 文本。The current base64 text.</summary>
    [Description("@#base64")]
    public extern Vue.VueShallowRef<string> Base64 { get; }

    /// <summary>编码完成后的 promise。The promise that resolves the encoded value.</summary>
    [Description("@#promise")]
    public extern Vue.VueShallowRef<IPromise<string>> Promise { get; }

    /// <summary>执行编码。Executes the encoding.</summary>
    /// <returns>Promise 包装的编码结果。Promise-wrapped encoded value.</returns>
    [Description("@#execute")]
    public extern IPromise<string> Execute();
}

/// <summary>
/// <c>useCountdown()</c> 的选项对象。
/// Options for <c>useCountdown()</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VueUseCountdownOptions
{
    /// <summary>倒计时结束时的回调。Callback invoked when the countdown completes.</summary>
    [Description("@#onComplete")]
    public Action? OnComplete { get; init; }

    /// <summary>每次递减时的回调。Callback invoked on each tick.</summary>
    [Description("@#onTick")]
    public Action? OnTick { get; init; }
}

/// <summary>
/// <c>useCountdown()</c> 的返回值：剩余秒数与开始/停止/重置控制。
/// The <c>useCountdown()</c> return value: the remaining seconds and start/stop/reset control.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseCountdownReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseCountdownReturn()
    {
    }

    /// <summary>剩余秒数。The remaining seconds.</summary>
    [Description("@#remaining")]
    public extern Vue.VueShallowRef<Number> Remaining { get; }

    /// <summary>倒计时是否处于活跃状态。Whether the countdown is active.</summary>
    [Description("@#isActive")]
    public extern Vue.IVueRef<bool> IsActive { get; }

    /// <summary>暂停倒计时。Pauses the countdown.</summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>恢复倒计时。Resumes the countdown.</summary>
    [Description("@#resume")]
    public extern void Resume();

    /// <summary>重设倒计时秒数。Resets the countdown seconds.</summary>
    /// <param name="countdown">新的倒计时秒数；省略时复用初始值。The new countdown seconds, reusing the initial value when omitted.</param>
    [Description("@#reset")]
    public extern void Reset(Number? countdown = null);

    /// <summary>开始倒计时。Starts the countdown.</summary>
    /// <param name="countdown">起始秒数；省略时复用初始值。The starting seconds, reusing the initial value when omitted.</param>
    [Description("@#start")]
    public extern void Start(Number? countdown = null);

    /// <summary>停止倒计时。Stops the countdown.</summary>
    [Description("@#stop")]
    public extern void Stop();
}

/// <summary>
/// <c>useTextSelection()</c> 的返回值：当前文本选择、矩形与范围。
/// The <c>useTextSelection()</c> return value: the current text selection, rectangles, and ranges.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class VueUseTextSelectionReturn
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected VueUseTextSelectionReturn()
    {
    }

    /// <summary>当前选中的文本。The currently selected text.</summary>
    [Description("@#text")]
    public extern Vue.VueComputedRef<string> Text { get; }

    /// <summary>当前选择对象；无选择时为 null。The current selection object, null when there is none.</summary>
    [Description("@#selection")]
    public extern Vue.VueShallowRef<Selection?> Selection { get; }
}
