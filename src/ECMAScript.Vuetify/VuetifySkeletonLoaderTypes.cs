using System.Collections;
using System.Runtime.CompilerServices;

namespace ECMAScript.Vuetify;

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySkeletonLoaderTypeValue : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifySkeletonLoaderType? _type;
    private readonly string? _customType;

    private VuetifySkeletonLoaderTypeValue(VuetifySkeletonLoaderType value)
    {
        _kind = 1;
        _type = value;
        _customType = default;
    }

    private VuetifySkeletonLoaderTypeValue(string value)
    {
        _kind = 2;
        _type = default;
        _customType = value;
    }

    public VuetifySkeletonLoaderType? AsType => _kind == 1 ? _type : default;

    public string? AsCustomType => _kind == 2 ? _customType : default;

    public object? Value => _kind switch
    {
        1 => AsType,
        2 => AsCustomType,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySkeletonLoaderTypeValue From(VuetifySkeletonLoaderType value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySkeletonLoaderTypeValue From(string value);

    public static implicit operator VuetifySkeletonLoaderTypeValue(VuetifySkeletonLoaderType value)
        => new(value);

    public static implicit operator VuetifySkeletonLoaderTypeValue(string value)
        => new(value);
}

[ECMAScript]
[System.Runtime.CompilerServices.Union]
[Description("@#")]
[CollectionBuilder(typeof(VuetifySkeletonLoaderTypesCollectionBuilder), nameof(VuetifySkeletonLoaderTypesCollectionBuilder.Create))]
public readonly struct VuetifySkeletonLoaderTypes : System.Runtime.CompilerServices.IUnion, IEnumerable<VuetifySkeletonLoaderTypeValue>
{
    private readonly VuetifySkeletonLoaderTypeValue[]? _values;

    private VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderTypeValue[] values)
    {
        _values = values;
    }

    public VuetifySkeletonLoaderTypeValue[]? AsArray => _values;

    public object? Value => AsArray;

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySkeletonLoaderTypes From(VuetifySkeletonLoaderTypeValue[] values);

    public static implicit operator VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderTypeValue[] values)
        => new(values);

    public static implicit operator VuetifySkeletonLoaderTypes(VuetifySkeletonLoaderType[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySkeletonLoaderTypeValue)value));

    public static implicit operator VuetifySkeletonLoaderTypes(string[] values)
        => new(Array.ConvertAll(values, static value => (VuetifySkeletonLoaderTypeValue)value));

    IEnumerator<VuetifySkeletonLoaderTypeValue> IEnumerable<VuetifySkeletonLoaderTypeValue>.GetEnumerator()
        => ((IEnumerable<VuetifySkeletonLoaderTypeValue>)(_values ?? [])).GetEnumerator();

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
[System.Runtime.CompilerServices.Union]
[Description("@#")]
public readonly struct VuetifySkeletonLoaderTypeSetting : System.Runtime.CompilerServices.IUnion
{
    private readonly byte _kind;
    private readonly VuetifySkeletonLoaderTypeValue? _type;
    private readonly VuetifySkeletonLoaderTypes? _types;

    private VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypeValue value)
    {
        _kind = 1;
        _type = value;
        _types = default;
    }

    private VuetifySkeletonLoaderTypeSetting(VuetifySkeletonLoaderTypes value)
    {
        _kind = 2;
        _type = default;
        _types = value;
    }

    public VuetifySkeletonLoaderTypeValue? AsType => _kind == 1 ? _type : default;

    public VuetifySkeletonLoaderTypes? AsTypes => _kind == 2 ? _types : default;

    public object? Value => _kind switch
    {
        1 => AsType,
        2 => AsTypes,
        _ => default
    };

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySkeletonLoaderTypeSetting From(VuetifySkeletonLoaderTypeValue value);

    [ECMAScriptInline("__arg1")]
    public extern static VuetifySkeletonLoaderTypeSetting From(VuetifySkeletonLoaderTypes value);

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
