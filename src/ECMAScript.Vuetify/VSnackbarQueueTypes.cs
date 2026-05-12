using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// 消息条队列消息列表的擦除值联合类型。
/// Erased value union for snackbar-queue message lists.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySnackbarQueueMessagesCollectionBuilder), nameof(VuetifySnackbarQueueMessagesCollectionBuilder.Create))]
public readonly struct VuetifySnackbarQueueMessages : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifySnackbarQueueMessage>
{
    private readonly VuetifySnackbarQueueMessage[]? _items;

    private VuetifySnackbarQueueMessages(VuetifySnackbarQueueMessage[] items)
    {
        _items = items;
    }

    public VuetifySnackbarQueueMessage[]? AsArray => _items;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySnackbarQueueMessages From(VuetifySnackbarQueueMessage[] items);

    public static implicit operator VuetifySnackbarQueueMessages(VuetifySnackbarQueueMessage[] items)
        => new(items);

    public static implicit operator VuetifySnackbarQueueMessages(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySnackbarQueueMessage)item));

    public static implicit operator VuetifySnackbarQueueMessages(VuetifySnackbarQueueMessageOptions[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySnackbarQueueMessage)item));

    IEnumerator<VuetifySnackbarQueueMessage> IEnumerable<VuetifySnackbarQueueMessage>.GetEnumerator()
        => ((IEnumerable<VuetifySnackbarQueueMessage>)(_items ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySnackbarQueueMessage>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySnackbarQueueMessagesCollectionBuilder
{
    public static VuetifySnackbarQueueMessages Create(ReadOnlySpan<VuetifySnackbarQueueMessage> items)
        => items.ToArray();
}

/// <summary>
/// 单条消息条队列消息的擦除值联合类型。
/// Erased value union for a single snackbar-queue message.
/// </summary>
[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySnackbarQueueMessage : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly string? _text;
    private readonly VuetifySnackbarQueueMessageOptions? _options;

    private VuetifySnackbarQueueMessage(string value)
    {
        _kind = 1;
        _text = value;
        _options = default;
    }

    private VuetifySnackbarQueueMessage(VuetifySnackbarQueueMessageOptions value)
    {
        _kind = 2;
        _text = default;
        _options = value;
    }

    public string? AsText => _kind == 1 ? _text : default;

    public VuetifySnackbarQueueMessageOptions? AsOptions => _kind == 2 ? _options : default;

    public object? Value => _kind switch
    {
        1 => AsText,
        2 => AsOptions,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySnackbarQueueMessage From(string value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySnackbarQueueMessage From(VuetifySnackbarQueueMessageOptions value);

    public static implicit operator VuetifySnackbarQueueMessage(string value)
        => new(value);

    public static implicit operator VuetifySnackbarQueueMessage(VuetifySnackbarQueueMessageOptions value)
        => new(value);
}

/// <summary>
/// 消息条队列消息的选项配置记录。
/// Options record for a snackbar-queue message.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifySnackbarQueueMessageOptions : VueProps
{
    [Description("@#text")]
    public string? Text { get; init; }

    [Description("@#color")]
    public string? Color { get; init; }

    [Description("@#timeout")]
    public VueStringNumberValue? Timeout { get; init; }

    [Description("@#timer")]
    public VuetifyBooleanStringValue? Timer { get; init; }

    [Description("@#location")]
    public VuetifyLocation? Location { get; init; }

    [Description("@#rounded")]
    public VuetifyRoundedValue? Rounded { get; init; }

    [Description("@#variant")]
    public VuetifyVariant? Variant { get; init; }

    [Description("@#multiLine")]
    public bool? MultiLine { get; init; }

    [Description("@#vertical")]
    public bool? Vertical { get; init; }

    [Description("@#closable")]
    public VuetifyBooleanStringValue? Closable { get; init; }

    [Description("@#closeText")]
    public string? CloseText { get; init; }
}

/// <summary>
/// Vuetify VSnackbarQueue 默认和文本插槽所暴露的作用域插槽上下文。
/// Scoped slot context exposed by Vuetify VSnackbarQueue default and text slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSnackbarQueueSlotContext
{
    [Description("@#item")]
    public VuetifySnackbarQueueMessage? Item { get; init; }
}

/// <summary>
/// Vuetify VSnackbarQueue 操作插槽所暴露的作用域插槽上下文。
/// Scoped slot context exposed by Vuetify VSnackbarQueue actions slot.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSnackbarQueueActionsSlotContext
{
    [Description("@#item")]
    public VuetifySnackbarQueueMessage? Item { get; init; }

    [Description("@#props")]
    public VSnackbarQueueActionProps? Props { get; init; }
}

/// <summary>
/// 消息条队列操作按钮的属性对象。
/// Props object for snackbar-queue action buttons.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VSnackbarQueueActionProps : VueProps
{
    [Description("@#onClick")]
    public Action? OnClick { get; init; }
}
