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
    [Description("@#actions")]
    Actions,

    [Description("@#article")]
    Article,

    [Description("@#avatar")]
    Avatar,

    [Description("@#button")]
    Button,

    [Description("@#card")]
    Card,

    [Description("@#card-avatar")]
    CardAvatar,

    [Description("@#chip")]
    Chip,

    [Description("@#date-picker")]
    DatePicker,

    [Description("@#date-picker-options")]
    DatePickerOptions,

    [Description("@#date-picker-days")]
    DatePickerDays,

    [Description("@#divider")]
    Divider,

    [Description("@#heading")]
    Heading,

    [Description("@#image")]
    Image,

    [Description("@#list-item")]
    ListItem,

    [Description("@#list-item-avatar")]
    ListItemAvatar,

    [Description("@#list-item-two-line")]
    ListItemTwoLine,

    [Description("@#list-item-avatar-two-line")]
    ListItemAvatarTwoLine,

    [Description("@#list-item-three-line")]
    ListItemThreeLine,

    [Description("@#list-item-avatar-three-line")]
    ListItemAvatarThreeLine,

    [Description("@#ossein")]
    Ossein,

    [Description("@#paragraph")]
    Paragraph,

    [Description("@#sentences")]
    Sentences,

    [Description("@#subtitle")]
    Subtitle,

    [Description("@#table")]
    Table,

    [Description("@#table-heading")]
    TableHeading,

    [Description("@#table-thead")]
    TableThead,

    [Description("@#table-tbody")]
    TableTbody,

    [Description("@#table-row-divider")]
    TableRowDivider,

    [Description("@#table-row")]
    TableRow,

    [Description("@#table-tfoot")]
    TableTfoot,

    [Description("@#text")]
    Text
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifySkeletonLoaderTypeValue(VuetifySkeletonLoaderType, string)
{
    public VuetifySkeletonLoaderType? AsType
        => Value is VuetifySkeletonLoaderType value ? value : default(VuetifySkeletonLoaderType?);

    public string? AsCustomType => Value as string;

    public static implicit operator VuetifySkeletonLoaderTypeValue(VuetifySkeletonLoaderType value)
        => new(value);

    public static implicit operator VuetifySkeletonLoaderTypeValue(string value)
        => new(value);
}

[ECMAScript]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySkeletonLoaderTypesCollectionBuilder), nameof(VuetifySkeletonLoaderTypesCollectionBuilder.Create))]
public readonly union VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderTypeValue[]) : IEnumerable<VuetifySkeletonLoaderTypeValue>
{
    public VuetifySkeletonLoaderTypeValue[]? AsArray => Value as VuetifySkeletonLoaderTypeValue[];

    public static implicit operator VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderTypeValue[] values)
        => new(values);

    public static implicit operator VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderType[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySkeletonLoaderTypeValue)value));

    public static implicit operator VuetifySkeletonLoaderTypes(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySkeletonLoaderTypeValue)value));

    IEnumerator<VuetifySkeletonLoaderTypeValue> IEnumerable<VuetifySkeletonLoaderTypeValue>.GetEnumerator()
        => ((IEnumerable<VuetifySkeletonLoaderTypeValue>)(AsArray ?? [])).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<VuetifySkeletonLoaderTypeValue>)this).GetEnumerator();
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class VuetifySkeletonLoaderTypesCollectionBuilder
{
    public static VuetifySkeletonLoaderTypes Create(ReadOnlySpan<VuetifySkeletonLoaderTypeValue> values)
        => values.ToArray();
}

[ECMAScript]
[Description("@#")]
public readonly union VuetifySkeletonLoaderTypeSetting(
    VuetifySkeletonLoaderTypeValue,
    VuetifySkeletonLoaderTypes)
{
    public VuetifySkeletonLoaderTypeValue? AsType
        => Value is VuetifySkeletonLoaderTypeValue value ? value : default(VuetifySkeletonLoaderTypeValue?);

    public VuetifySkeletonLoaderTypes? AsTypes
        => Value is VuetifySkeletonLoaderTypes value ? value : default(VuetifySkeletonLoaderTypes?);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypeValue value)
        => new(value);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderType value)
        => new((VuetifySkeletonLoaderTypeValue)value);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(string value)
        => new((VuetifySkeletonLoaderTypeValue)value);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypes value)
        => new(value);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypeValue[] value)
        => new((VuetifySkeletonLoaderTypes)value);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderType[] value)
        => new((VuetifySkeletonLoaderTypes)value);

    public static implicit operator VuetifySkeletonLoaderTypeSetting(string[] value)
        => new((VuetifySkeletonLoaderTypes)value);
}
