namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VFileUpload 浏览插槽属性。
/// Browse slot props exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadBrowseProps
{
    /// <summary>
    /// 自定义渲染时应转发的点击处理函数，用于执行组件默认交互。
    /// </summary>
    [Description("@#onClick")]
    public Action<MouseEvent>? OnClick { get; init; }
}

/// <summary>
/// Vuetify VFileUpload 浏览插槽上下文。
/// Browse slot context exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadBrowseSlotContext
{
    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VFileUploadBrowseProps? Props { get; init; }
}

/// <summary>
/// Vuetify VFileUpload 输入插槽上下文。
/// Input slot context exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadInputSlotContext
{
    /// <summary>
    /// Vuetify 已创建的原生输入节点，供自定义插槽组合使用。
    /// </summary>
    [Description("@#inputNode")]
    public IVNode? InputNode { get; init; }
}

/// <summary>
/// Vuetify VFileUpload 项目插槽属性。
/// Item slot props exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadItemSlotProps
{
    /// <summary>
    /// 请求移除当前上传文件。
    /// </summary>
    [Description("@#onClick:remove")]
    public Action? OnClickRemove { get; init; }
}

/// <summary>
/// Vuetify VFileUpload 项目插槽上下文。
/// Item slot context exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadItemSlotContext
{
    /// <summary>
    /// 当前上传条目的浏览器 File 对象。
    /// </summary>
    [Description("@#file")]
    public FileRef? File { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
    [Description("@#props")]
    public VFileUploadItemSlotProps? Props { get; init; }
}
