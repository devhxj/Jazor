namespace ECMAScript.Vuetify;

/// <summary>
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
/// Item slot context exposed by Vuetify VFileUpload.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VFileUploadItemSlotContext
{
    [Description("@#file")]
    public File? File { get; init; }

    [Description("@#props")]
    public VFileUploadItemSlotProps? Props { get; init; }
}
