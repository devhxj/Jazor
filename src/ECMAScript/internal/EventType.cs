namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[Category("ignore")]
/// <summary>
/// Common browser event-name constants for authoring JavaScript event registrations.
/// 用于编写 JavaScript 事件注册的常用浏览器事件名称常量。
/// </summary>
public static class EventType
{
	/// <summary>
	/// Click after a pointing-device press and release. 点击事件（按下并释放鼠标按钮）。
	/// </summary>
	public const string Click = "click";

	/// <summary>
	/// Double-click event. 双击事件。
	/// </summary>
	public const string DblClick = "dblclick";

	/// <summary>
	/// Pointing-device button press. 鼠标按钮按下。
	/// </summary>
	public const string MouseDown = "mousedown";

	/// <summary>
	/// Pointing-device button release. 鼠标按钮释放。
	/// </summary>
	public const string MouseUp = "mouseup";

	/// <summary>
	/// Pointing-device movement. 鼠标移动。
	/// </summary>
	public const string MouseMove = "mousemove";

	/// <summary>
	/// Pointer enters an element or descendant. 鼠标移入元素。
	/// </summary>
	public const string MouseOver = "mouseover";

	/// <summary>
	/// Pointer leaves an element or descendant. 鼠标移出元素。
	/// </summary>
	public const string MouseOut = "mouseout";

	/// <summary>
	/// Pointer enters an element without bubbling. 鼠标进入元素（不冒泡）。
	/// </summary>
	public const string MouseEnter = "mouseenter";

	/// <summary>
	/// Pointer leaves an element without bubbling. 鼠标离开元素（不冒泡）。
	/// </summary>
	public const string MouseLeave = "mouseleave";

	/// <summary>
	/// Context-menu request event. 右键菜单事件。
	/// </summary>
	public const string ContextMenu = "contextmenu";

	/// <summary>
	/// Keyboard key press. 按下键盘按键。
	/// </summary>
	public const string KeyDown = "keydown";

	/// <summary>
	/// Keyboard key release. 释放键盘按键。
	/// </summary>
	public const string KeyUp = "keyup";

	/// <summary>
	/// Deprecated keypress event. 已废弃的按键事件，不推荐使用。
	/// </summary>
	public const string KeyPress = "keypress";

	/// <summary>
	/// Form submission event. 表单提交。
	/// </summary>
	public const string Submit = "submit";

	/// <summary>
	/// Form reset event. 表单重置。
	/// </summary>
	public const string Reset = "reset";

	/// <summary>
	/// Committed form-control value change. 表单元素内容改变。
	/// </summary>
	public const string Change = "change";

	/// <summary>
	/// Live editable-value input event. 输入框输入时实时触发。
	/// </summary>
	public const string Input = "input";

	/// <summary>
	/// Element gains focus. 获取焦点。
	/// </summary>
	public const string Focus = "focus";

	/// <summary>
	/// Element loses focus. 失去焦点。
	/// </summary>
	public const string Blur = "blur";

	/// <summary>
	/// Constraint validation failure event. 表单验证不通过时触发。
	/// </summary>
	public const string Invalid = "invalid";

	/// <summary>
	/// Resource load completion. 资源加载完成。
	/// </summary>
	public const string Load = "load";

	/// <summary>
	/// Document unload during close or navigation. 文档卸载（关闭窗口或导航离开）。
	/// </summary>
	public const string Unload = "unload";

	/// <summary>
	/// Document is about to unload. 即将卸载文档。
	/// </summary>
	public const string BeforeUnload = "beforeunload";

	/// <summary>
	/// Viewport or window resize. 窗口大小改变。
	/// </summary>
	public const string Resize = "resize";

	/// <summary>
	/// Scrolling event. 滚动事件。
	/// </summary>
	public const string Scroll = "scroll";

	/// <summary>
	/// Initial HTML parsing is complete. 初始 HTML 文档完全加载和解析后触发。
	/// </summary>
	public const string DOMContentLoaded = "DOMContentLoaded";

	/// <summary>
	/// Clipboard copy event. 复制。
	/// </summary>
	public const string Copy = "copy";

	/// <summary>
	/// Clipboard cut event. 剪切。
	/// </summary>
	public const string Cut = "cut";

	/// <summary>
	/// Clipboard paste event. 粘贴。
	/// </summary>
	public const string Paste = "paste";

	/// <summary>
	/// Media playback begins. 播放开始。
	/// </summary>
	public const string Play = "play";

	/// <summary>
	/// Media playback pauses. 暂停。
	/// </summary>
	public const string Pause = "pause";

	/// <summary>
	/// Media playback ends. 播放结束。
	/// </summary>
	public const string Ended = "ended";

	/// <summary>
	/// Media playback position updates. 播放时间更新。
	/// </summary>
	public const string TimeUpdate = "timeupdate";

	/// <summary>
	/// Media volume changes. 音量改变。
	/// </summary>
	public const string VolumeChange = "volumechange";

	/// <summary>
	/// Media can begin playback, though buffering may continue. 可以开始播放（但可能需要缓冲）。
	/// </summary>
	public const string CanPlay = "canplay";

	/// <summary>
	/// Media can play through without buffering. 可以连续播放无需缓冲。
	/// </summary>
	public const string CanPlayThrough = "canplaythrough";

	/// <summary>
	/// Drag operation continues. 拖动元素过程中持续触发。
	/// </summary>
	public const string Drag = "drag";

	/// <summary>
	/// Drag operation begins. 拖动开始。
	/// </summary>
	public const string DragStart = "dragstart";

	/// <summary>
	/// Drag operation ends. 拖动结束。
	/// </summary>
	public const string DragEnd = "dragend";

	/// <summary>
	/// Dragged item enters a drop target. 被拖动的元素进入放置目标。
	/// </summary>
	public const string DragEnter = "dragenter";

	/// <summary>
	/// Dragged item moves over a drop target. 被拖动元素在放置目标上方移动时触发。
	/// </summary>
	public const string DragOver = "dragover";

	/// <summary>
	/// Dragged item leaves a drop target. 被拖动元素离开放置目标。
	/// </summary>
	public const string DragLeave = "dragleave";

	/// <summary>
	/// Dragged item is dropped on a target. 在放置目标上释放拖动元素。
	/// </summary>
	public const string Drop = "drop";

	/// <summary>
	/// CSS animation starts. CSS 动画开始。
	/// </summary>
	public const string AnimationStart = "animationstart";

	/// <summary>
	/// CSS animation ends. CSS 动画结束。
	/// </summary>
	public const string AnimationEnd = "animationend";

	/// <summary>
	/// CSS animation iteration repeats. CSS 动画重复执行。
	/// </summary>
	public const string AnimationIteration = "animationiteration";

	/// <summary>
	/// CSS transition starts. CSS 过渡开始。
	/// </summary>
	public const string TransitionStart = "transitionstart";

	/// <summary>
	/// CSS transition ends. CSS 过渡结束。
	/// </summary>
	public const string TransitionEnd = "transitionend";

	/// <summary>
	/// Touch contact starts. 开始触摸。
	/// </summary>
	public const string TouchStart = "touchstart";

	/// <summary>
	/// Touch contact ends. 结束触摸。
	/// </summary>
	public const string TouchEnd = "touchend";

	/// <summary>
	/// Touch contact moves. 触摸移动。
	/// </summary>
	public const string TouchMove = "touchmove";

	/// <summary>
	/// Touch contact is cancelled, such as by interruption. 触摸取消（如触摸被突然中断）。
	/// </summary>
	public const string TouchCancel = "touchcancel";

	/// <summary>
	/// Pointer presses. 指针按下。
	/// </summary>
	public const string PointerDown = "pointerdown";

	/// <summary>
	/// Pointer releases. 指针释放。
	/// </summary>
	public const string PointerUp = "pointerup";

	/// <summary>
	/// Pointer moves. 指针移动。
	/// </summary>
	public const string PointerMove = "pointermove";

	/// <summary>
	/// Pointer enters an element or descendant. 指针移入。
	/// </summary>
	public const string PointerOver = "pointerover";

	/// <summary>
	/// Pointer leaves an element or descendant. 指针移出。
	/// </summary>
	public const string PointerOut = "pointerout";

	/// <summary>
	/// Pointer enters an element without bubbling. 指针进入（不冒泡）。
	/// </summary>
	public const string PointerEnter = "pointerenter";

	/// <summary>
	/// Pointer leaves an element without bubbling. 指针离开（不冒泡）。
	/// </summary>
	public const string PointerLeave = "pointerleave";

	/// <summary>
	/// Pointer interaction is cancelled. 指针取消。
	/// </summary>
	public const string PointerCancel = "pointercancel";

	/// <summary>
	/// Browser network connectivity becomes available. 网络恢复。
	/// </summary>
	public const string Online = "online";

	/// <summary>
	/// Browser network connectivity becomes unavailable. 网络断开。
	/// </summary>
	public const string Offline = "offline";

	/// <summary>
	/// Ready state changes, for example on XMLHttpRequest. 就绪状态改变（如 XMLHttpRequest）。
	/// </summary>
	public const string ReadyStateChange = "readystatechange";

	/// <summary>
	/// Error occurs, such as a failed resource load. 发生错误（如资源加载失败）。
	/// </summary>
	public const string Error = "error";

	/// <summary>
	/// Message is received, such as from workers, WebSocket, or cross-document messaging. 接收消息（如 Web Workers、WebSocket、跨文档消息）。
	/// </summary>
	public const string Message = "message";

	/// <summary>
	/// Same-origin localStorage or sessionStorage changes. localStorage 或 sessionStorage 发生改变（同源页面之间）。
	/// </summary>
	public const string Storage = "storage";

	/// <summary>
	/// Document visibility changes, such as moving to a background tab. 文档的可见性改变（如切换到后台标签页）。
	/// </summary>
	public const string VisibilityChange = "visibilitychange";
}
