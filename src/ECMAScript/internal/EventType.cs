namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[Category("ignore")]
public static class EventType
{
	/// <summary>
	/// 点击事件（按下并释放鼠标按钮）
	/// </summary>
	public const string Click = "click";

	/// <summary>
	/// 双击事件
	/// </summary>
	public const string DblClick = "dblclick";

	/// <summary>
	/// 鼠标按钮按下
	/// </summary>
	public const string MouseDown = "mousedown";

	/// <summary>
	/// 鼠标按钮释放
	/// </summary>
	public const string MouseUp = "mouseup";

	/// <summary>
	/// 鼠标移动
	/// </summary>
	public const string MouseMove = "mousemove";

	/// <summary>
	/// 鼠标移入元素
	/// </summary>
	public const string MouseOver = "mouseover";

	/// <summary>
	/// 鼠标移出元素
	/// </summary>
	public const string MouseOut = "mouseout";

	/// <summary>
	/// 鼠标进入元素（不冒泡）
	/// </summary>
	public const string MouseEnter = "mouseenter";

	/// <summary>
	/// 鼠标离开元素（不冒泡）
	/// </summary>
	public const string MouseLeave = "mouseleave";

	/// <summary>
	/// 右键菜单事件
	/// </summary>
	public const string ContextMenu = "contextmenu";

	/// <summary>
	/// 按下键盘按键
	/// </summary>
	public const string KeyDown = "keydown";

	/// <summary>
	/// 释放键盘按键
	/// </summary>
	public const string KeyUp = "keyup";

	/// <summary>
	/// 按下并释放键盘按键（已废弃，不推荐使用）
	/// </summary>
	public const string KeyPress = "keypress";

	/// <summary>
	/// 表单提交
	/// </summary>
	public const string Submit = "submit";

	/// <summary>
	/// 表单重置
	/// </summary>
	public const string Reset = "reset";

	/// <summary>
	/// 表单元素内容改变
	/// </summary>
	public const string Change = "change";

	/// <summary>
	/// 输入框输入时实时触发
	/// </summary>
	public const string Input = "input";

	/// <summary>
	/// 获取焦点
	/// </summary>
	public const string Focus = "focus";

	/// <summary>
	/// 失去焦点
	/// </summary>
	public const string Blur = "blur";

	/// <summary>
	/// 表单验证不通过时触发
	/// </summary>
	public const string Invalid = "invalid";

	/// <summary>
	/// 资源加载完成
	/// </summary>
	public const string Load = "load";

	/// <summary>
	/// 文档卸载（关闭窗口或导航离开）
	/// </summary>
	public const string Unload = "unload";

	/// <summary>
	/// 即将卸载文档
	/// </summary>
	public const string BeforeUnload = "beforeunload";

	/// <summary>
	/// 窗口大小改变
	/// </summary>
	public const string Resize = "resize";

	/// <summary>
	/// 滚动事件
	/// </summary>
	public const string Scroll = "scroll";

	/// <summary>
	/// 初始HTML文档完全加载和解析后触发
	/// </summary>
	public const string DOMContentLoaded = "DOMContentLoaded";

	/// <summary>
	/// 复制
	/// </summary>
	public const string Copy = "copy";

	/// <summary>
	/// 剪切
	/// </summary>
	public const string Cut = "cut";

	/// <summary>
	/// 粘贴
	/// </summary>
	public const string Paste = "paste";

	/// <summary>
	/// 播放开始
	/// </summary>
	public const string Play = "play";

	/// <summary>
	/// 暂停
	/// </summary>
	public const string Pause = "pause";

	/// <summary>
	/// 播放结束
	/// </summary>
	public const string Ended = "ended";

	/// <summary>
	/// 播放时间更新
	/// </summary>
	public const string TimeUpdate = "timeupdate";

	/// <summary>
	/// 音量改变
	/// </summary>
	public const string VolumeChange = "volumechange";

	/// <summary>
	/// 可以开始播放（但可能需要缓冲）
	/// </summary>
	public const string CanPlay = "canplay";

	/// <summary>
	/// 可以连续播放无需缓冲
	/// </summary>
	public const string CanPlayThrough = "canplaythrough";

	/// <summary>
	/// 拖动元素过程中持续触发
	/// </summary>
	public const string Drag = "drag";

	/// <summary>
	/// 拖动开始
	/// </summary>
	public const string DragStart = "dragstart";

	/// <summary>
	/// 拖动结束
	/// </summary>
	public const string DragEnd = "dragend";

	/// <summary>
	/// 被拖动的元素进入放置目标
	/// </summary>
	public const string DragEnter = "dragenter";

	/// <summary>
	/// 被拖动元素在放置目标上方移动时触发
	/// </summary>
	public const string DragOver = "dragover";

	/// <summary>
	/// 被拖动元素离开放置目标
	/// </summary>
	public const string DragLeave = "dragleave";

	/// <summary>
	/// 在放置目标上释放拖动元素
	/// </summary>
	public const string Drop = "drop";

	/// <summary>
	/// CSS动画开始
	/// </summary>
	public const string AnimationStart = "animationstart";

	/// <summary>
	/// CSS动画结束
	/// </summary>
	public const string AnimationEnd = "animationend";

	/// <summary>
	/// CSS动画重复执行
	/// </summary>
	public const string AnimationIteration = "animationiteration";

	/// <summary>
	/// 过渡开始
	/// </summary>
	public const string TransitionStart = "transitionstart";

	/// <summary>
	/// 过渡结束
	/// </summary>
	public const string TransitionEnd = "transitionend";

	/// <summary>
	/// 开始触摸
	/// </summary>
	public const string TouchStart = "touchstart";

	/// <summary>
	/// 结束触摸
	/// </summary>
	public const string TouchEnd = "touchend";

	/// <summary>
	/// 触摸移动
	/// </summary>
	public const string TouchMove = "touchmove";

	/// <summary>
	/// 触摸取消（如触摸被突然中断）
	/// </summary>
	public const string TouchCancel = "touchcancel";

	/// <summary>
	/// 指针按下
	/// </summary>
	public const string PointerDown = "pointerdown";

	/// <summary>
	/// 指针释放
	/// </summary>
	public const string PointerUp = "pointerup";

	/// <summary>
	/// 指针移动
	/// </summary>
	public const string PointerMove = "pointermove";

	/// <summary>
	/// 指针移入
	/// </summary>
	public const string PointerOver = "pointerover";

	/// <summary>
	/// 指针移出
	/// </summary>
	public const string PointerOut = "pointerout";

	/// <summary>
	/// 指针进入（不冒泡）
	/// </summary>
	public const string PointerEnter = "pointerenter";

	/// <summary>
	/// 指针离开（不冒泡）
	/// </summary>
	public const string PointerLeave = "pointerleave";

	/// <summary>
	/// 指针取消
	/// </summary>
	public const string PointerCancel = "pointercancel";

	/// <summary>
	/// 网络恢复
	/// </summary>
	public const string Online = "online";

	/// <summary>
	/// 网络断开
	/// </summary>
	public const string Offline = "offline";

	/// <summary>
	/// 就绪状态改变（如XMLHttpRequest）
	/// </summary>
	public const string ReadyStateChange = "readystatechange";

	/// <summary>
	/// 发生错误（如资源加载失败）
	/// </summary>
	public const string Error = "error";

	/// <summary>
	/// 接收消息（如Web Workers，WebSocket，跨文档消息）
	/// </summary>
	public const string Message = "message";

	/// <summary>
	/// localStorage或sessionStorage发生改变（同源页面之间）
	/// </summary>
	public const string Storage = "storage";

	/// <summary>
	/// 文档的可见性改变（如切换到后台标签页）
	/// </summary>
	public const string VisibilityChange = "visibilitychange";
}
