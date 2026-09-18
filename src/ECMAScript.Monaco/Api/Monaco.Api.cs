using System.ComponentModel;

namespace ECMAScript;

public static partial class Monaco
{
    // ---------- 编辑器与模型 Editor & model ----------

    /// <summary>
    /// 在容器元素上创建代码编辑器。<c>options.model</c> 为空时按 <c>value</c>/<c>language</c> 自动创建模型。
    /// Creates a code editor inside the container element. When <c>options.model</c> is omitted, a model
    /// is created automatically from <c>value</c>/<c>language</c>.
    /// </summary>
    /// <param name="domElement">容器元素。The container element.</param>
    /// <param name="options">构造选项。The construction options.</param>
    [Description("@#create")]
    public extern static MonacoEditor Create(Element domElement, MonacoEditorConstructionOptions? options = null);

    /// <summary>
    /// 创建文本模型；语言与内容随后续调用确定。
    /// Creates a text model; its language and content follow from later calls.
    /// </summary>
    /// <param name="value">初始内容。The initial value.</param>
    /// <param name="language">语言 id。The language id.</param>
    [Description("@#createModel")]
    public extern static MonacoModel CreateModel(string value, string? language = null);

    /// <summary>
    /// 返回所有已创建的模型。
    /// Returns every created model.
    /// </summary>
    [Description("@#getModels")]
    public extern static MonacoModel[] GetModels();

    /// <summary>
    /// 返回所有已创建的编辑器。
    /// Returns every created editor.
    /// </summary>
    [Description("@#getEditors")]
    public extern static MonacoEditor[] GetEditors();

    // ---------- 语言与主题 Language & theme ----------

    /// <summary>
    /// 设置模型的语言 id。
    /// Sets the model's language id.
    /// </summary>
    /// <param name="model">目标模型。The target model.</param>
    /// <param name="languageId">语言 id。The language id.</param>
    [Description("@#setModelLanguage")]
    public extern static void SetModelLanguage(MonacoModel model, string languageId);

    /// <summary>
    /// 注册或覆盖一个主题定义。
    /// Registers or overrides a theme definition.
    /// </summary>
    /// <param name="themeName">主题名。The theme name.</param>
    /// <param name="themeData">主题数据。The theme data.</param>
    [Description("@#defineTheme")]
    public extern static void DefineTheme(string themeName, MonacoThemeData themeData);

    /// <summary>
    /// 切换当前主题。
    /// Switches the current theme.
    /// </summary>
    /// <param name="themeName">主题名。The theme name.</param>
    [Description("@#setTheme")]
    public extern static void SetTheme(string themeName);

    // ---------- 标记 Markers ----------

    /// <summary>
    /// 为模型设置某个 owner 的标记集合。
    /// Sets the marker set owned by <paramref name="owner"/> for the model.
    /// </summary>
    /// <param name="model">目标模型。The target model.</param>
    /// <param name="owner">标记所有者标识。The marker owner identifier.</param>
    /// <param name="markers">标记集合。The markers.</param>
    [Description("@#setModelMarkers")]
    public extern static void SetModelMarkers(MonacoModel model, string owner, MonacoMarker[] markers);

    /// <summary>
    /// 移除某个 owner 的全部标记。
    /// Removes every marker owned by <paramref name="owner"/>.
    /// </summary>
    /// <param name="owner">标记所有者标识。The marker owner identifier.</param>
    [Description("@#removeAllMarkers")]
    public extern static void RemoveAllMarkers(string owner);

    /// <summary>
    /// 标记变化时的订阅；返回句柄用于取消。
    /// Subscription for marker changes; the handle cancels it.
    /// </summary>
    /// <param name="listener">变化回调。The change listener.</param>
    [Description("@#onDidChangeMarkers")]
    public extern static MonacoDisposable OnDidChangeMarkers(Action<MonacoUri[]> listener);

    // ---------- Worker ----------

    /// <summary>
    /// 为语言服务创建 web worker 代理；worker URL 由应用通过选项显式提供。
    /// Creates a web-worker proxy for a language service; the worker URL is supplied explicitly by the application.
    /// </summary>
    /// <param name="options">worker 选项。The worker options.</param>
    /// <typeparam name="TWorker">worker 代理的强类型契约。The strongly typed contract of the worker proxy.</typeparam>
    [Description("@#createWebWorker")]
    public extern static MonacoWebWorker<TWorker> CreateWebWorker<TWorker>(MonacoWebWorkerOptions options);
}
