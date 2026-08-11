namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VFileUpload 浏览插槽属性。
/// Browse slot props exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadBrowseProps
{
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
    [Description("@#file")]
    public Files? File { get; init; }

    [Description("@#props")]
    public VFileUploadItemSlotProps? Props { get; init; }
}
