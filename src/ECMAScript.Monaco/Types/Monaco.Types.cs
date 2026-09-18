using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 标记（marker）严重级别；数值与 Monaco <c>MarkerSeverity</c> 一致。
/// The marker severity; values match Monaco's <c>MarkerSeverity</c>.
/// </summary>
public enum MonacoMarkerSeverity
{
    /// <summary>提示。A hint.</summary>
    Hint = 1,

    /// <summary>信息。Information.</summary>
    Info = 2,

    /// <summary>警告。A warning.</summary>
    Warning = 4,

    /// <summary>错误。An error.</summary>
    Error = 8
}

/// <summary>
/// 标记（marker）数据；对应 Monaco <c>IMarkerData</c>。
/// Marker data; mirrors Monaco's <c>IMarkerData</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoMarker
{
    /// <summary>起始行号（从 1 开始）。The start line number, 1-based.</summary>
    [Description("@#startLineNumber")]
    public Number StartLineNumber { get; init; } = default!;

    /// <summary>起始列号（从 1 开始）。The start column, 1-based.</summary>
    [Description("@#startColumn")]
    public Number StartColumn { get; init; } = default!;

    /// <summary>结束行号（从 1 开始）。The end line number, 1-based.</summary>
    [Description("@#endLineNumber")]
    public Number EndLineNumber { get; init; } = default!;

    /// <summary>结束列号（从 1 开始）。The end column, 1-based.</summary>
    [Description("@#endColumn")]
    public Number EndColumn { get; init; } = default!;

    /// <summary>标记消息。The marker message.</summary>
    [Description("@#message")]
    public string Message { get; init; } = default!;

    /// <summary>严重级别。The severity.</summary>
    [Description("@#severity")]
    public MonacoMarkerSeverity Severity { get; init; } = default!;
}

/// <summary>
/// 位置（行列坐标，均从 1 开始）；对应 Monaco <c>Position</c>。
/// A position (line and column, both 1-based); mirrors Monaco's <c>Position</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoPosition
{
    /// <summary>行号。The line number.</summary>
    [Description("@#lineNumber")]
    public Number LineNumber { get; init; } = default!;

    /// <summary>列号。The column.</summary>
    [Description("@#column")]
    public Number Column { get; init; } = default!;
}

/// <summary>
/// 选区（起点与终点）；对应 Monaco <c>Selection</c> 的行列投影。
/// A selection (anchor and position); the line/column projection of Monaco's <c>Selection</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoSelection
{
    /// <summary>起始行号。The start line number.</summary>
    [Description("@#startLineNumber")]
    public Number StartLineNumber { get; init; } = default!;

    /// <summary>起始列号。The start column.</summary>
    [Description("@#startColumn")]
    public Number StartColumn { get; init; } = default!;

    /// <summary>结束行号。The end line number.</summary>
    [Description("@#endLineNumber")]
    public Number EndLineNumber { get; init; } = default!;

    /// <summary>结束列号。The end column.</summary>
    [Description("@#endColumn")]
    public Number EndColumn { get; init; } = default!;
}

/// <summary>
/// 模型的语言与内容更新选项；对应 <c>ITextModelUpdateOptions</c> 的常用项。
/// Language and content update options for a model; the frequently used <c>ITextModelUpdateOptions</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoModelUpdateOptions
{
    /// <summary>是否追加而非替换内容。Whether the value is appended instead of replacing.</summary>
    [Description("@#append")]
    public bool? Append { get; init; }

    /// <summary>被替换的原始内容（用于撤销历史）。The previous value this edit replaces, used for undo history.</summary>
    [Description("@#valuePrevious")]
    public string? ValuePrevious { get; init; }
}

/// <summary>
/// 编辑器构造选项；覆盖 Monaco <c>IStandaloneEditorConstructionOptions</c> 的常用项。
/// Editor construction options; the frequently used subset of Monaco's <c>IStandaloneEditorConstructionOptions</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoEditorConstructionOptions
{
    /// <summary>初始文本内容；未提供 model 时自动创建模型。The initial value; a model is created automatically when none is supplied.</summary>
    [Description("@#value")]
    public string? Value { get; init; }

    /// <summary>初始语言 id，例如 <c>javascript</c>、<c>csharp</c>、<c>json</c>。The initial language id, for example <c>javascript</c>, <c>csharp</c>, or <c>json</c>.</summary>
    [Description("@#language")]
    public string? Language { get; init; }

    /// <summary>初始主题名。The initial theme name.</summary>
    [Description("@#theme")]
    public string? Theme { get; init; }

    /// <summary>是否只读。Whether the editor is read-only.</summary>
    [Description("@#readOnly")]
    public bool? ReadOnly { get; init; }

    /// <summary>行高（像素）。The line height in pixels.</summary>
    [Description("@#lineHeight")]
    public Number? LineHeight { get; init; }

    /// <summary>字号（像素）。The font size in pixels.</summary>
    [Description("@#fontSize")]
    public Number? FontSize { get; init; }

    /// <summary>是否显示小地图。Whether the minimap is shown.</summary>
    [Description("@#minimap")]
    public MonacoMinimapOptions? Minimap { get; init; }

    /// <summary>是否自动换行：<c>off</c>/<c>on</c>/<c>wordWrapColumn</c>/<c>bounded</c>。Word wrapping: <c>off</c>/<c>on</c>/<c>wordWrapColumn</c>/<c>bounded</c>.</summary>
    [Description("@#wordWrap")]
    public string? WordWrap { get; init; }

    /// <summary>是否显示行号。Whether line numbers are shown.</summary>
    [Description("@#lineNumbers")]
    public bool? LineNumbers { get; init; }

    /// <summary>是否启用滚动超越末尾。Whether scrolling past the last line is allowed.</summary>
    [Description("@#scrollBeyondLastLine")]
    public bool? ScrollBeyondLastLine { get; init; }

    /// <summary>是否自动布局（随容器尺寸变化）。Whether the editor re-layouts automatically with its container.</summary>
    [Description("@#automaticLayout")]
    public bool? AutomaticLayout { get; init; }

    /// <summary>Tab 对应的空格数。The number of spaces a tab equals.</summary>
    [Description("@#tabSize")]
    public Number? TabSize { get; init; }

    /// <summary>是否把 Tab 转换为空格。Whether tabs are inserted as spaces.</summary>
    [Description("@#insertSpaces")]
    public bool? InsertSpaces { get; init; }
}

/// <summary>
/// 小地图选项。
/// Minimap options.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoMinimapOptions
{
    /// <summary>是否启用小地图。Whether the minimap is enabled.</summary>
    [Description("@#enabled")]
    public bool? Enabled { get; init; }
}

/// <summary>
/// 可释放句柄；调用 <c>dispose()</c> 取消注册。对应 Monaco <c>IDisposable</c>。
/// A disposable handle; calling <c>dispose()</c> cancels the registration. Mirrors Monaco's <c>IDisposable</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class MonacoDisposable
{
    private MonacoDisposable()
    {
    }
}

/// <summary>
/// 文本模型；对应 Monaco <c>ITextModel</c>。模型绑定语言与内容，可被多个编辑器共享。
/// A text model; mirrors Monaco's <c>ITextModel</c>. A model binds language and content and can be shared by editors.
/// </summary>
[ECMAScript]
[Description("@#")]
public abstract class MonacoModel
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected MonacoModel()
    {
    }

    /// <summary>模型标识。The model identifier.</summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>模型 URI。The model URI.</summary>
    [Description("@#uri")]
    public extern MonacoUri Uri { get; }

    /// <summary>当前语言 id。The current language id.</summary>
    [Description("@#getLanguageId")]
    public extern string GetLanguageId();

    /// <summary>当前文本内容。The current text content.</summary>
    [Description("@#getValue")]
    public extern string GetValue();

    /// <summary>替换文本内容。Replaces the text content.</summary>
    /// <param name="newValue">新内容。The new value.</param>
    /// <param name="options">更新选项。The update options.</param>
    [Description("@#setValue")]
    public extern void SetValue(string newValue, MonacoModelUpdateOptions? options = null);

    /// <summary>行数。The line count.</summary>
    [Description("@#getLineCount")]
    public extern Number GetLineCount();

    /// <summary>按行号读取整行文本（行号从 1 开始）。Reads a whole line by its 1-based line number.</summary>
    /// <param name="lineNumber">行号。The line number.</param>
    [Description("@#getLineContent")]
    public extern string GetLineContent(Number lineNumber);

    /// <summary>模型内容版本号；每次编辑递增。The model content version id, incremented on every edit.</summary>
    [Description("@#getVersionId")]
    public extern Number GetVersionId();

    /// <summary>模型是否已被释放。Whether the model has been disposed.</summary>
    [Description("@#isDisposed")]
    public extern bool IsDisposed();

    /// <summary>释放模型。Disposes the model.</summary>
    [Description("@#dispose")]
    public extern void Dispose();

    /// <summary>内容变化时的订阅；返回句柄用于取消。Subscription for content changes; the handle cancels it.</summary>
    /// <param name="listener">变化回调。The change listener.</param>
    [Description("@#onDidChangeContent")]
    public extern MonacoDisposable OnDidChangeContent(Action<MonacoContentChangeEvent> listener);

    /// <summary>语言变化时的订阅；返回句柄用于取消。Subscription for language changes; the handle cancels it.</summary>
    /// <param name="listener">变化回调。The change listener.</param>
    [Description("@#onDidChangeLanguage")]
    public extern MonacoDisposable OnDidChangeLanguage(Action<MonacoLanguageChangeEvent> listener);

    /// <summary>模型即将被释放时的订阅；返回句柄用于取消。Subscription fired before the model is disposed; the handle cancels it.</summary>
    /// <param name="listener">释放回调。The dispose listener.</param>
    [Description("@#onWillDispose")]
    public extern MonacoDisposable OnWillDispose(Action listener);
}

/// <summary>
/// 模型 URI；对应 Monaco <c>Uri</c>。
/// A model URI; mirrors Monaco's <c>Uri</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class MonacoUri
{
    private MonacoUri()
    {
    }
}

/// <summary>
/// 模型内容变化事件。
/// The model content-change event.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoContentChangeEvent
{
    /// <summary>变化后的版本号。The version id after the change.</summary>
    [Description("@#versionId")]
    public Number VersionId { get; init; } = default!;

    /// <summary>是否为撤销或重做操作。Whether the change came from an undo or redo.</summary>
    [Description("@#isUndoing")]
    public bool IsUndoing { get; init; }

    /// <summary>是否为重做操作。Whether the change is a redo.</summary>
    [Description("@#isRedoing")]
    public bool IsRedoing { get; init; }
}

/// <summary>
/// 模型语言变化事件。
/// The model language-change event.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoLanguageChangeEvent
{
    /// <summary>变化后的语言 id。The language id after the change.</summary>
    [Description("@#newLanguage")]
    public string NewLanguage { get; init; } = default!;

    /// <summary>变化前的语言 id。The language id before the change.</summary>
    [Description("@#oldLanguage")]
    public string OldLanguage { get; init; } = default!;
}

/// <summary>
/// 编辑器布局信息；对应 <c>EditorLayoutInfo</c> 的常用项。
/// Editor layout information; the frequently used <c>EditorLayoutInfo</c> fields.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoLayoutInfo
{
    /// <summary>编辑器宽度（像素）。The editor width in pixels.</summary>
    [Description("@#width")]
    public Number Width { get; init; } = default!;

    /// <summary>编辑器高度（像素）。The editor height in pixels.</summary>
    [Description("@#height")]
    public Number Height { get; init; } = default!;
}

/// <summary>
/// 主题定义的颜色表与规则。
/// The color table and rules of a theme definition.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoThemeData
{
    /// <summary>颜色键值表，例如 <c>{"editor.background":"#1e1e1e"}</c>。The color map, for example <c>{"editor.background":"#1e1e1e"}</c>.</summary>
    [Description("@#colors")]
    public Vue.VueDictionary? Colors { get; init; }

    /// <summary>基础主题：<c>vs</c>、<c>vs-dark</c> 或 <c>hc-black</c>。The base theme: <c>vs</c>, <c>vs-dark</c>, or <c>hc-black</c>.</summary>
    [Description("@#base")]
    public string? Base { get; init; }

    /// <summary>是否继承基础主题规则。Whether rules from the base theme are inherited.</summary>
    [Description("@#inherit")]
    public bool? Inherit { get; init; }
}

/// <summary>
/// 代码编辑器；对应 Monaco <c>IStandaloneCodeEditor</c> 的常用项。
/// The code editor; the frequently used surface of Monaco's <c>IStandaloneCodeEditor</c>.
/// </summary>
/// <remarks>
/// 编辑器依赖真实 DOM 与布局测量，必须在浏览器生命周期内创建；SSR 期间不得调用 <c>Create</c>。
/// The editor needs a real DOM and layout measurement, so it must be created within the browser
/// lifecycle; SSR must not call <c>Create</c>.
/// </remarks>
[ECMAScript]
[Description("@#")]
public abstract class MonacoEditor
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected MonacoEditor()
    {
    }

    /// <summary>当前关联的模型；未关联时为 null。The current model, or null when none is attached.</summary>
    [Description("@#getModel")]
    public extern MonacoModel? GetModel();

    /// <summary>切换关联的模型。Switches the attached model.</summary>
    /// <param name="model">目标模型。The target model.</param>
    [Description("@#setModel")]
    public extern void SetModel(MonacoModel? model);

    /// <summary>编辑器当前文本。The editor's current text.</summary>
    [Description("@#getValue")]
    public extern string GetValue();

    /// <summary>重新测量并布局编辑器；容器尺寸变化后需要调用（自动布局关闭时）。Re-measures and lays out the editor; call it after the container resizes when automatic layout is off.</summary>
    [Description("@#layout")]
    public extern void Layout();

    /// <summary>聚焦编辑器。Focuses the editor.</summary>
    [Description("@#focus")]
    public extern void Focus();

    /// <summary>当前布局信息。The current layout information.</summary>
    [Description("@#getLayoutInfo")]
    public extern MonacoLayoutInfo GetLayoutInfo();

    /// <summary>滚动位置（垂直偏移像素）。The scroll position as a vertical offset in pixels.</summary>
    [Description("@#getScrollTop")]
    public extern Number GetScrollTop();

    /// <summary>设置垂直滚动位置。Sets the vertical scroll position.</summary>
    /// <param name="scrollTop">垂直偏移像素。The vertical offset in pixels.</param>
    [Description("@#setScrollTop")]
    public extern void SetScrollTop(Number scrollTop);

    /// <summary>执行编辑器命令（按命令 id）。Executes an editor command by its command id.</summary>
    /// <param name="commandId">命令 id。The command id.</param>
    [Description("@#trigger")]
    public extern void Trigger(string commandId);

    /// <summary>更新编辑器选项。Updates the editor options.</summary>
    /// <param name="options">新选项。The new options.</param>
    [Description("@#updateOptions")]
    public extern void UpdateOptions(MonacoEditorConstructionOptions options);

    /// <summary>释放编辑器。Disposes the editor.</summary>
    [Description("@#dispose")]
    public extern void Dispose();

    /// <summary>光标位置变化时的订阅；返回句柄用于取消。Subscription for cursor position changes; the handle cancels it.</summary>
    /// <param name="listener">变化回调。The change listener.</param>
    [Description("@#onDidChangeCursorPosition")]
    public extern MonacoDisposable OnDidChangeCursorPosition(Action<MonacoCursorPositionEvent> listener);

    /// <summary>选区变化时的订阅；返回句柄用于取消。Subscription for selection changes; the handle cancels it.</summary>
    /// <param name="listener">变化回调。The change listener.</param>
    [Description("@#onDidChangeCursorSelection")]
    public extern MonacoDisposable OnDidChangeCursorSelection(Action<MonacoSelectionEvent> listener);

    /// <summary>容器布局变化时的订阅；返回句柄用于取消。Subscription for layout changes; the handle cancels it.</summary>
    /// <param name="listener">布局回调。The layout listener.</param>
    [Description("@#onDidLayoutChange")]
    public extern MonacoDisposable OnDidLayoutChange(Action<MonacoLayoutInfo> listener);

    /// <summary>编辑器聚焦文本时的订阅；返回句柄用于取消。Subscription for focus events; the handle cancels it.</summary>
    /// <param name="listener">聚焦回调。The focus listener.</param>
    [Description("@#onDidFocusEditorText")]
    public extern MonacoDisposable OnDidFocusEditorText(Action listener);

    /// <summary>编辑器失焦文本时的订阅；返回句柄用于取消。Subscription for blur events; the handle cancels it.</summary>
    /// <param name="listener">失焦回调。The blur listener.</param>
    [Description("@#onDidBlurEditorText")]
    public extern MonacoDisposable OnDidBlurEditorText(Action listener);
}

/// <summary>
/// 光标位置变化事件。
/// The cursor position-change event.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoCursorPositionEvent
{
    /// <summary>新的光标位置。The new cursor position.</summary>
    [Description("@#position")]
    public MonacoPosition Position { get; init; } = default!;
}

/// <summary>
/// 选区变化事件。
/// The selection-change event.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoSelectionEvent
{
    /// <summary>新的选区。The new selection.</summary>
    [Description("@#selection")]
    public MonacoSelection Selection { get; init; } = default!;

    /// <summary>选区文本是否为空。Whether the selection is empty.</summary>
    [Description("@#isEmpty")]
    public bool IsEmpty { get; init; }
}

/// <summary>
/// web worker 创建选项；对应 Monaco <c>IInternalWebWorkerOptions</c> 的公开子集。
/// Web-worker creation options; the public subset of Monaco's <c>IInternalWebWorkerOptions</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record MonacoWebWorkerOptions
{
    /// <summary>worker 模块的 URL；由应用按自身资源路径提供。The URL of the worker module, supplied by the application's own asset path.</summary>
    [Description("@#worker")]
    public string Worker { get; init; } = default!;

    /// <summary>暴露给 worker 的宿主值。The host values exposed to the worker.</summary>
    [Description("@#host")]
    public Vue.VueProps? Host { get; init; }

    /// <summary>初始化消息。The initialization message.</summary>
    [Description("@#keepIdleModels")]
    public bool? KeepIdleModels { get; init; }
}

/// <summary>
/// web worker 代理；对应 Monaco <c>MonacoWebWorker</c>。
/// A web-worker proxy; mirrors Monaco's <c>MonacoWebWorker</c>.
/// </summary>
/// <typeparam name="TWorker">worker 代理的强类型契约。The strongly typed contract of the worker proxy.</typeparam>
[ECMAScript]
[Description("@#")]
public abstract class MonacoWebWorker<TWorker>
{
    /// <summary>
    /// 供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。
    /// </summary>
    protected MonacoWebWorker()
    {
    }

    /// <summary>取得 worker 的代理对象。Gets the worker's proxy object.</summary>
    [Description("@#getProxy")]
    public extern TWorker GetProxy();

    /// <summary>把模型同步到 worker。Synchronizes a model to the worker.</summary>
    /// <param name="resources">要同步的模型。The models to synchronize.</param>
    [Description("@#withSyncedResources")]
    public extern void WithSyncedResources(MonacoModel[] resources);

    /// <summary>释放 worker 代理。Disposes the worker proxy.</summary>
    [Description("@#dispose")]
    public extern void Dispose();
}
