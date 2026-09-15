using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

// Defines VSnackbarQueue message values, options, and scoped-slot contexts.
// 定义 VSnackbarQueue 的消息值、选项和作用域插槽上下文；可擦除的消息值使用原生 union。

/// <summary>
/// 消息条队列消息列表的擦除值联合类型。
/// Erased value union for snackbar-queue message lists.
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySnackbarQueueMessagesCollectionBuilder), nameof(VuetifySnackbarQueueMessagesCollectionBuilder.Create))]
public readonly union VuetifySnackbarQueueMessages(VuetifySnackbarQueueMessage[]) : IEnumerable<VuetifySnackbarQueueMessage>
{
    /// <summary>
    /// 读取当前值的 VuetifySnackbarQueueMessage[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySnackbarQueueMessage[]? AsArray => Value as VuetifySnackbarQueueMessage[];

    /// <summary>
    /// 将 VuetifySnackbarQueueMessage[] 值转换为 VuetifySnackbarQueueMessages，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySnackbarQueueMessages(VuetifySnackbarQueueMessage[] items)
        => new(items);

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySnackbarQueueMessages(string[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySnackbarQueueMessage)item));

    /// <summary>
    /// 将 VuetifySnackbarQueueMessageOptions[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySnackbarQueueMessages(VuetifySnackbarQueueMessageOptions[] items)
        => new(Array.ConvertAll(items, static item => (VuetifySnackbarQueueMessage)item));

    IEnumerator<VuetifySnackbarQueueMessage> IEnumerable<VuetifySnackbarQueueMessage>.GetEnumerator()
        => ((IEnumerable<VuetifySnackbarQueueMessage>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySnackbarQueueMessage>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySnackbarQueueMessagesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="items">按期望顺序排列的元素。</param>
    public static VuetifySnackbarQueueMessages Create(ReadOnlySpan<VuetifySnackbarQueueMessage> items)
        => items.ToArray();
}

/// <summary>
/// 单条消息条队列消息的擦除值联合类型。
/// Erased value union for a single snackbar-queue message.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySnackbarQueueMessage(string, VuetifySnackbarQueueMessageOptions)
{
    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsText => Value as string;

    /// <summary>
    /// 读取当前值的 VuetifySnackbarQueueMessageOptions 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySnackbarQueueMessageOptions? AsOptions => Value as VuetifySnackbarQueueMessageOptions;

    /// <summary>
    /// 将 string 值转换为 VuetifySnackbarQueueMessage，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySnackbarQueueMessage(string value)
        => new(value);

    /// <summary>
    /// 将 VuetifySnackbarQueueMessageOptions 值转换为 VuetifySnackbarQueueMessage，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
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
    /// <summary>
    /// 提示消息的正文文本。
    /// </summary>
    [Description("@#text")]
    public string? Text { get; init; }

    /// <summary>
    /// 当前项的主题颜色名或 CSS 颜色值。
    /// </summary>
    [Description("@#color")]
    public string? Color { get; init; }

    /// <summary>
    /// 消息自动关闭前等待的毫秒数；特殊值按 Snackbar 的 timeout 规则处理。
    /// </summary>
    [Description("@#timeout")]
    public VueStringNumberValue? Timeout { get; init; }

    /// <summary>
    /// 是否显示消息自动关闭的倒计时提示。
    /// </summary>
    [Description("@#timer")]
    public VuetifyBooleanStringValue? Timer { get; init; }

    /// <summary>
    /// 消息相对于显示区域的位置。
    /// </summary>
    [Description("@#location")]
    public VuetifyLocation? Location { get; init; }

    /// <summary>
    /// 消息容器的圆角大小或预设。
    /// </summary>
    [Description("@#rounded")]
    public VuetifyRoundedValue? Rounded { get; init; }

    /// <summary>
    /// 消息容器的视觉样式。
    /// </summary>
    [Description("@#variant")]
    public VuetifyVariant? Variant { get; init; }

    /// <summary>
    /// 使用适合多行文本的消息布局。
    /// </summary>
    [Description("@#multiLine")]
    public bool? MultiLine { get; init; }

    /// <summary>
    /// 将消息正文与操作按钮按垂直方向排列。
    /// </summary>
    [Description("@#vertical")]
    public bool? Vertical { get; init; }

    /// <summary>
    /// 是否显示消息关闭操作，可提供相应的图标值。
    /// </summary>
    [Description("@#closable")]
    public VuetifyBooleanStringValue? Closable { get; init; }

    /// <summary>
    /// 关闭消息操作的可访问性文本。
    /// </summary>
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
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
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
    /// <summary>
    /// 当前渲染或操作的数据项。
    /// </summary>
    [Description("@#item")]
    public VuetifySnackbarQueueMessage? Item { get; init; }

    /// <summary>
    /// 供自定义渲染转发给目标元素或组件的属性，包含上游提供的事件和可访问性绑定。
    /// </summary>
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
    /// <summary>
    /// 自定义渲染时应转发的点击处理函数，用于执行组件默认交互。
    /// </summary>
    [Description("@#onClick")]
    public Action? OnClick { get; init; }
}