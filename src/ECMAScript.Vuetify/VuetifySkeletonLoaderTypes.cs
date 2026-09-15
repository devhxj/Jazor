using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 骨架加载器预设类型。
/// Vuetify skeleton loader preset types.
/// </summary>
[String]
public enum VuetifySkeletonLoaderType
{
    /// <summary>
    /// 操作按钮区域的骨架；上游取值为 “actions”。
    /// </summary>
    [Description("@#actions")]
    Actions,

    /// <summary>
    /// 文章内容区域的骨架；上游取值为 “article”。
    /// </summary>
    [Description("@#article")]
    Article,

    /// <summary>
    /// 头像形式；上游取值为 “avatar”。
    /// </summary>
    [Description("@#avatar")]
    Avatar,

    /// <summary>
    /// 普通按钮，不触发表单提交；上游取值为 “button”。
    /// </summary>
    [Description("@#button")]
    Button,

    /// <summary>
    /// 卡片样式；上游取值为 “card”。
    /// </summary>
    [Description("@#card")]
    Card,

    /// <summary>
    /// 带头像的卡片骨架；上游取值为 “card-avatar”。
    /// </summary>
    [Description("@#card-avatar")]
    CardAvatar,

    /// <summary>
    /// 标签片形式；上游取值为 “chip”。
    /// </summary>
    [Description("@#chip")]
    Chip,

    /// <summary>
    /// 日期选择器的骨架；上游取值为 “date-picker”。
    /// </summary>
    [Description("@#date-picker")]
    DatePicker,

    /// <summary>
    /// 日期选择器选项区域的骨架；上游取值为 “date-picker-options”。
    /// </summary>
    [Description("@#date-picker-options")]
    DatePickerOptions,

    /// <summary>
    /// 日期选择器日期网格的骨架；上游取值为 “date-picker-days”。
    /// </summary>
    [Description("@#date-picker-days")]
    DatePickerDays,

    /// <summary>
    /// 分隔线骨架；上游取值为 “divider”。
    /// </summary>
    [Description("@#divider")]
    Divider,

    /// <summary>
    /// 标题骨架；上游取值为 “heading”。
    /// </summary>
    [Description("@#heading")]
    Heading,

    /// <summary>
    /// 图片占位骨架；上游取值为 “image”。
    /// </summary>
    [Description("@#image")]
    Image,

    /// <summary>
    /// 单行列表项骨架；上游取值为 “list-item”。
    /// </summary>
    [Description("@#list-item")]
    ListItem,

    /// <summary>
    /// 带头像的单行列表项骨架；上游取值为 “list-item-avatar”。
    /// </summary>
    [Description("@#list-item-avatar")]
    ListItemAvatar,

    /// <summary>
    /// 双行列表项骨架；上游取值为 “list-item-two-line”。
    /// </summary>
    [Description("@#list-item-two-line")]
    ListItemTwoLine,

    /// <summary>
    /// 带头像的双行列表项骨架；上游取值为 “list-item-avatar-two-line”。
    /// </summary>
    [Description("@#list-item-avatar-two-line")]
    ListItemAvatarTwoLine,

    /// <summary>
    /// 三行列表项骨架；上游取值为 “list-item-three-line”。
    /// </summary>
    [Description("@#list-item-three-line")]
    ListItemThreeLine,

    /// <summary>
    /// 带头像的三行列表项骨架；上游取值为 “list-item-avatar-three-line”。
    /// </summary>
    [Description("@#list-item-avatar-three-line")]
    ListItemAvatarThreeLine,

    /// <summary>
    /// 基础骨架块；上游取值为 “ossein”。
    /// </summary>
    [Description("@#ossein")]
    Ossein,

    /// <summary>
    /// 段落骨架；上游取值为 “paragraph”。
    /// </summary>
    [Description("@#paragraph")]
    Paragraph,

    /// <summary>
    /// 多行句子骨架；上游取值为 “sentences”。
    /// </summary>
    [Description("@#sentences")]
    Sentences,

    /// <summary>
    /// 副标题骨架；上游取值为 “subtitle”。
    /// </summary>
    [Description("@#subtitle")]
    Subtitle,

    /// <summary>
    /// 完整表格骨架；上游取值为 “table”。
    /// </summary>
    [Description("@#table")]
    Table,

    /// <summary>
    /// 表格标题骨架；上游取值为 “table-heading”。
    /// </summary>
    [Description("@#table-heading")]
    TableHeading,

    /// <summary>
    /// 表格头部骨架；上游取值为 “table-thead”。
    /// </summary>
    [Description("@#table-thead")]
    TableThead,

    /// <summary>
    /// 表格正文骨架；上游取值为 “table-tbody”。
    /// </summary>
    [Description("@#table-tbody")]
    TableTbody,

    /// <summary>
    /// 带分隔线的表格行骨架；上游取值为 “table-row-divider”。
    /// </summary>
    [Description("@#table-row-divider")]
    TableRowDivider,

    /// <summary>
    /// 表格行骨架；上游取值为 “table-row”。
    /// </summary>
    [Description("@#table-row")]
    TableRow,

    /// <summary>
    /// 表格底部骨架；上游取值为 “table-tfoot”。
    /// </summary>
    [Description("@#table-tfoot")]
    TableTfoot,

    /// <summary>
    /// 文本样式；上游取值为 “text”。
    /// </summary>
    [Description("@#text")]
    Text
}

/// <summary>
/// C# 联合参数，允许 VuetifySkeletonLoaderType, string。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySkeletonLoaderTypeValue(VuetifySkeletonLoaderType, string)
{
    /// <summary>
    /// 读取当前值的 VuetifySkeletonLoaderType 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySkeletonLoaderType? AsType
        => Value is VuetifySkeletonLoaderType value ? value : default(VuetifySkeletonLoaderType?);

    /// <summary>
    /// 读取当前值的 string 分支；不属于该分支时返回 null。
    /// </summary>
    public string? AsCustomType => Value as string;

    /// <summary>
    /// 将 VuetifySkeletonLoaderType 值转换为 VuetifySkeletonLoaderTypeValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeValue(VuetifySkeletonLoaderType value)
        => new(value);

    /// <summary>
    /// 将 string 值转换为 VuetifySkeletonLoaderTypeValue，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeValue(string value)
        => new(value);
}

/// <summary>
/// C# 联合参数，允许 VuetifySkeletonLoaderTypeValue[]。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。
/// </summary>
[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySkeletonLoaderTypesCollectionBuilder), nameof(VuetifySkeletonLoaderTypesCollectionBuilder.Create))]
public readonly union VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderTypeValue[]) : IEnumerable<VuetifySkeletonLoaderTypeValue>
{
    /// <summary>
    /// 读取当前值的 VuetifySkeletonLoaderTypeValue[] 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySkeletonLoaderTypeValue[]? AsArray => Value as VuetifySkeletonLoaderTypeValue[];

    /// <summary>
    /// 将 VuetifySkeletonLoaderTypeValue[] 值转换为 VuetifySkeletonLoaderTypes，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderTypeValue[] values)
        => new(values);

    /// <summary>
    /// 将 VuetifySkeletonLoaderType[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderType[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySkeletonLoaderTypeValue)value));

    /// <summary>
    /// 将 string[] 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypes(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySkeletonLoaderTypeValue)value));

    IEnumerator<VuetifySkeletonLoaderTypeValue> IEnumerable<VuetifySkeletonLoaderTypeValue>.GetEnumerator()
        => ((IEnumerable<VuetifySkeletonLoaderTypeValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySkeletonLoaderTypeValue>)this).GetEnumerator();
}

/// <summary>
/// 供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySkeletonLoaderTypesCollectionBuilder
{
    /// <summary>
    /// 为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。
    /// </summary>
    /// <param name="values">按期望顺序排列的元素。</param>
    public static VuetifySkeletonLoaderTypes Create(ReadOnlySpan<VuetifySkeletonLoaderTypeValue> values)
        => values.ToArray();
}

/// <summary>
/// 用于 VSkeletonLoader.Type 的参数类型。
/// A string delimited list of skeleton components to create such as `type=&quot;text@3&quot;` or `type=&quot;card, list-item&quot;`. Will recursively generate a corresponding skeleton from the provided string. Also supports short-hand for multiple elements such as **article@3** and **paragraph@2** which will generate 3 _article_ skeletons and 2 _paragraph_ skeletons. Please see below for a list of available pre-defined options.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifySkeletonLoaderTypeSetting(
    VuetifySkeletonLoaderTypeValue,
    VuetifySkeletonLoaderTypes)
{
    /// <summary>
    /// 读取当前值的 VuetifySkeletonLoaderTypeValue 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySkeletonLoaderTypeValue? AsType
        => Value is VuetifySkeletonLoaderTypeValue value ? value : default(VuetifySkeletonLoaderTypeValue?);

    /// <summary>
    /// 读取当前值的 VuetifySkeletonLoaderTypes 分支；不属于该分支时返回 null。
    /// </summary>
    public VuetifySkeletonLoaderTypes? AsTypes
        => Value is VuetifySkeletonLoaderTypes value ? value : default(VuetifySkeletonLoaderTypes?);

    /// <summary>
    /// 将 VuetifySkeletonLoaderTypeValue 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypeValue value)
        => new(value);

    /// <summary>
    /// 将 VuetifySkeletonLoaderType 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderType value)
        => new((VuetifySkeletonLoaderTypeValue)value);

    /// <summary>
    /// 将 string 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(string value)
        => new((VuetifySkeletonLoaderTypeValue)value);

    /// <summary>
    /// 将 VuetifySkeletonLoaderTypes 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypes value)
        => new(value);

    /// <summary>
    /// 将 VuetifySkeletonLoaderTypeValue[] 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypeValue[] value)
        => new((VuetifySkeletonLoaderTypes)value);

    /// <summary>
    /// 将 VuetifySkeletonLoaderType[] 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderType[] value)
        => new((VuetifySkeletonLoaderTypes)value);

    /// <summary>
    /// 将 string[] 值转换为 VuetifySkeletonLoaderTypeSetting，保留输入值供 JavaScript API 使用。
    /// </summary>
    /// <param name="value">要传入的值，保持其声明的类型和数据。</param>
    /// <returns>转换后的强类型值。</returns>
    public static implicit operator VuetifySkeletonLoaderTypeSetting(string[] value)
        => new((VuetifySkeletonLoaderTypes)value);
}