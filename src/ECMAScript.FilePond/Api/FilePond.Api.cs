using System.ComponentModel;

namespace ECMAScript;

public static partial class FilePond
{
    /// <summary>
    /// 在元素上创建 FilePond 实例。
    /// Creates a FilePond instance on the element.
    /// </summary>
    /// <param name="element">容器元素。The container element.</param>
    /// <param name="options">实例选项。The instance options.</param>
    /// <returns>FilePond 实例。The FilePond instance.</returns>
    [Description("@#create")]
    public extern static FilePondInstance Create(Element? element = null, FilePondOptions? options = null);

    /// <summary>
    /// 销毁元素上已附加的 FilePond 实例。
    /// Destroys the FilePond instance attached to the element.
    /// </summary>
    /// <param name="element">容器元素。The container element.</param>
    [Description("@#destroy")]
    public extern static void Destroy(Element element);

    /// <summary>
    /// 查找元素上已附加的 FilePond 实例；未附加时返回 null。
    /// Finds the FilePond instance attached to the element, or null when there is none.
    /// </summary>
    /// <param name="element">容器元素。The container element.</param>
    [Description("@#find")]
    public extern static FilePondInstance? Find(Element element);

    /// <summary>
    /// 读取实例或全局选项。
    /// Reads instance or global options.
    /// </summary>
    /// <param name="element">容器元素。The container element.</param>
    [Description("@#getOptions")]
    public extern static FilePondOptions GetOptions(Element element);

    /// <summary>
    /// 修改全局默认选项；影响之后创建的实例。
    /// Changes the global default options; it affects instances created afterwards.
    /// </summary>
    /// <param name="options">全局选项。The global options.</param>
    [Description("@#setOptions")]
    public extern static void SetOptions(FilePondOptions options);

    /// <summary>
    /// 注册一个或多个插件；插件是不透明的上游对象，由各 <c>@pqina/*</c> 包提供。
    /// Registers one or more plugins; a plugin is an opaque upstream object supplied by the
    /// individual <c>@pqina/*</c> packages.
    /// </summary>
    /// <param name="plugins">插件对象。The plugin objects.</param>
    [Description("@#registerPlugin")]
    public extern static void RegisterPlugin(params FilePondPlugin[] plugins);

    /// <summary>
    /// 判断当前浏览器是否满足 FilePond 的运行要求。
    /// Determines whether the current browser meets the FilePond runtime requirements.
    /// </summary>
    [Description("@#supported")]
    public extern static bool Supported();
}
