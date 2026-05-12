namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 时间线对齐方式。
/// Vuetify timeline alignment.
/// </summary>
[String]
public enum VuetifyTimelineAlign
{
    [Description("@#center")]
    Center,

    [Description("@#start")]
    Start
}

/// <summary>
/// Vuetify 时间线方向。
/// Vuetify timeline direction.
/// </summary>
[String]
public enum VuetifyTimelineDirection
{
    [Description("@#vertical")]
    Vertical,

    [Description("@#horizontal")]
    Horizontal
}

/// <summary>
/// Vuetify 时间线对齐策略。
/// Vuetify timeline justify strategy.
/// </summary>
[String]
public enum VuetifyTimelineJustify
{
    [Description("@#auto")]
    Auto,

    [Description("@#center")]
    Center
}

/// <summary>
/// Vuetify 时间线侧边位置。
/// Vuetify timeline side position.
/// </summary>
[String]
public enum VuetifyTimelineSide
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End
}

/// <summary>
/// Vuetify 时间线截断线位置。
/// Vuetify timeline truncate line position.
/// </summary>
[String]
public enum VuetifyTimelineTruncateLine
{
    [Description("@#start")]
    Start,

    [Description("@#end")]
    End,

    [Description("@#both")]
    Both
}
