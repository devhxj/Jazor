using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Vue Devtools plugin 的顶层函数映射。调用会保留官方 bridge 行为，
/// runtime 资源由 Jazor 自带的 Vue manifest 提供，不在本包重复物化。
/// </summary>
public static partial class VueDevtools
{
    /// <summary>
    /// 向 Vue Devtools 注册一个 plugin。若希望在 Devtools 面板打开前记录 timeline，
    /// 请在 descriptor 上设置 <see cref="PluginDescriptor.EnableEarlyProxy"/>。
    /// </summary>
    /// <param name="descriptor">插件身份、所属 Vue app 与 settings 描述。</param>
    /// <param name="setup">Devtools 可用时接收 API handle 的 setup 回调。</param>
    [Description("@#setupDevToolsPlugin")]
    public extern static void SetupPlugin(PluginDescriptor descriptor, DevtoolsPluginSetupCallback setup);

    /// <summary>
    /// 向 Vue Devtools 注册带强类型 settings 投影的 plugin。
    /// </summary>
    /// <typeparam name="TSettings">业务侧读取 settings 时使用的结构化类型。</typeparam>
    /// <param name="descriptor">带 settings 类型标记的插件描述。</param>
    /// <param name="setup">接收强类型 API handle 的 setup 回调。</param>
    [Description("@#setupDevToolsPlugin")]
    public extern static void SetupPlugin<TSettings>(
        PluginDescriptor<TSettings> descriptor,
        DevtoolsPluginSetupCallback<TSettings> setup)
        where TSettings : Vue.VueProps;

    /// <summary>
    /// 添加一个顶层 custom tab。tab 直接由 Devtools client 渲染，不需要在应用中注册 Vue component。
    /// </summary>
    /// <param name="tab">tab 的名称、标题、分类和 view 定义。</param>
    [Description("@#addCustomTab")]
    public extern static void AddCustomTab(CustomTab tab);

    /// <summary>
    /// 添加一个可由 Devtools command palette 打开的 custom command。
    /// </summary>
    /// <param name="command">command 的唯一 id、显示文本和 URL action/children。</param>
    [Description("@#addCustomCommand")]
    public extern static void AddCustomCommand(CustomCommand command);

    /// <summary>
    /// 按唯一 id 移除先前注册的 custom command。
    /// </summary>
    /// <param name="commandId">需要移除的 command id。</param>
    [Description("@#removeCustomCommand")]
    public extern static void RemoveCustomCommand(string commandId);

    /// <summary>
    /// 在 Vue Devtools bridge 建立连接后执行回调。
    /// </summary>
    /// <param name="callback">连接完成时调用的回调。</param>
    /// <returns>表示注册/连接过程的 JavaScript Promise。</returns>
    [Description("@#onDevToolsConnected")]
    public extern static IPromise OnDevToolsConnected(DevtoolsConnectionCallback callback);

    /// <summary>
    /// 在 Devtools client 建立连接后执行回调。它与 browser extension 是否已经发现 app 的时机不同，
    /// 适用于需要等待 client UI 的集成。
    /// </summary>
    /// <param name="callback">client 连接完成时调用的回调。</param>
    /// <returns>表示注册/连接过程的 JavaScript Promise。</returns>
    [Description("@#onDevToolsClientConnected")]
    public extern static IPromise OnDevToolsClientConnected(DevtoolsConnectionCallback callback);
}
