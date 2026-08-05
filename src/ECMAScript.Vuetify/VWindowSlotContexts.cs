namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify VWindow show-arrows 接受的方向模式。
/// Direction mode accepted by Vuetify VWindow show-arrows.
/// </summary>
[String]
public enum VuetifyWindowShowArrowsMode
{
    [Description("@#hover")]
    Hover
}

/// <summary>
/// Vuetify VWindow show-arrows value, matching <c>boolean | "hover"</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyWindowShowArrowsValue(bool, VuetifyWindowShowArrowsMode)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VuetifyWindowShowArrowsMode? AsMode
        => Value is VuetifyWindowShowArrowsMode value ? value : default(VuetifyWindowShowArrowsMode?);

    public static implicit operator VuetifyWindowShowArrowsValue(bool value)
        => new(value);

    public static implicit operator VuetifyWindowShowArrowsValue(VuetifyWindowShowArrowsMode value)
        => new(value);
}

/// <summary>
/// Vuetify touch directive payload used by VWindow.
/// </summary>
[ECMAScript]
[Description("@#")]
public record VuetifyTouchData
{
    [Description("@#touchstartX")]
    public Number TouchstartX { get; init; }

    [Description("@#touchstartY")]
    public Number TouchstartY { get; init; }

    [Description("@#touchmoveX")]
    public Number TouchmoveX { get; init; }

    [Description("@#touchmoveY")]
    public Number TouchmoveY { get; init; }

    [Description("@#touchendX")]
    public Number TouchendX { get; init; }

    [Description("@#touchendY")]
    public Number TouchendY { get; init; }

    [Description("@#offsetX")]
    public Number OffsetX { get; init; }

    [Description("@#offsetY")]
    public Number OffsetY { get; init; }
}

/// <summary>
/// Vuetify touch event wrapper carrying the original DOM touch event.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTouchEventData : VuetifyTouchData
{
    [Description("@#originalEvent")]
    public TouchEvent? OriginalEvent { get; init; }
}

public delegate void VuetifyTouchEventHandler(VuetifyTouchEventData eventData);

public delegate void VuetifyTouchDirectionHandler(VuetifyTouchData touchData);

/// <summary>
/// Strongly typed Vuetify touch handler bag.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyTouchHandlers : VueProps
{
    [Description("@#start")]
    public VuetifyTouchEventHandler? Start { get; init; }

    [Description("@#end")]
    public VuetifyTouchEventHandler? End { get; init; }

    [Description("@#move")]
    public VuetifyTouchEventHandler? Move { get; init; }

    [Description("@#left")]
    public VuetifyTouchDirectionHandler? Left { get; init; }

    [Description("@#right")]
    public VuetifyTouchDirectionHandler? Right { get; init; }

    [Description("@#up")]
    public VuetifyTouchDirectionHandler? Up { get; init; }

    [Description("@#down")]
    public VuetifyTouchDirectionHandler? Down { get; init; }
}

/// <summary>
/// Vuetify VWindow touch prop value, matching <c>boolean | TouchHandlers</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public readonly union VuetifyTouchValue(bool, VuetifyTouchHandlers)
{
    public bool? AsBool => Value is bool value ? value : default(bool?);

    public VuetifyTouchHandlers? AsHandlers => Value as VuetifyTouchHandlers;

    public static implicit operator VuetifyTouchValue(bool value)
        => new(value);

    public static implicit operator VuetifyTouchValue(VuetifyTouchHandlers value)
        => new(value);
}

/// <summary>
/// Vuetify group item exposed through VWindow group slot context.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyWindowGroupItem
{
    [Description("@#id")]
    public string? Id { get; init; }

    [Description("@#value")]
    public VueValue? Value { get; init; }

    [Description("@#disabled")]
    public bool? Disabled { get; init; }
}

/// <summary>
/// Vuetify group contract exposed by VWindow default/additional slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyWindowGroupProvide
{
    [Description("@#select")]
    public VuetifyWindowGroupSelectCallback? Select { get; init; }

    [Description("@#selected")]
    public IVueRef<string[]>? Selected { get; init; }

    [Description("@#isSelected")]
    public VuetifyWindowGroupIsSelectedCallback? IsSelected { get; init; }

    [Description("@#prev")]
    public Action? Prev { get; init; }

    [Description("@#next")]
    public Action? Next { get; init; }

    [Description("@#selectedClass")]
    public IVueRef<string?>? SelectedClass { get; init; }

    [Description("@#items")]
    public VueComputedRef<VuetifyWindowGroupItem[]>? Items { get; init; }

    [Description("@#disabled")]
    public IVueRef<bool?>? Disabled { get; init; }

    [Description("@#getItemIndex")]
    public VuetifyWindowGroupItemIndexCallback? GetItemIndex { get; init; }
}

public delegate void VuetifyWindowGroupSelectCallback(string id, bool value);

public delegate bool VuetifyWindowGroupIsSelectedCallback(string id);

public delegate Number VuetifyWindowGroupItemIndexCallback(VueValue? value);

/// <summary>
/// Default and additional slot context exposed by Vuetify VWindow.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VWindowSlotContext
{
    [Description("@#group")]
    public VuetifyWindowGroupProvide? Group { get; init; }
}

/// <summary>
/// Props object for VWindow prev/next arrow slots.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VWindowControlProps
{
    [Description("@#icon")]
    public VuetifyIconValue? Icon { get; init; }

    [Description("@#class")]
    public string? Class { get; init; }

    [Description("@#onClick")]
    public Action? OnClick { get; init; }

    [Description("@#aria-label")]
    public string? AriaLabel { get; init; }
}

/// <summary>
/// Prev/next slot context exposed by Vuetify VWindow.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VWindowControlSlotContext
{
    [Description("@#props")]
    public VWindowControlProps? Props { get; init; }
}

/// <summary>
/// Payload emitted by Vuetify window item group:selected.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed record VuetifyGroupSelectedEvent
{
    [Description("@#value")]
    public bool Value { get; init; }
}
