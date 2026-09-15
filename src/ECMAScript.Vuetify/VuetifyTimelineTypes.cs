namespace ECMAScript.Vuetify;

/// <summary>
/// Vuetify 时间线对齐方式。
/// Vuetify timeline alignment.
/// </summary>
[String]
public enum VuetifyTimelineAlign
{
    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
    [Description("@#center")]
    Center,

    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
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
    /// <summary>
    /// 垂直方向；上游取值为 “vertical”。
    /// </summary>
    [Description("@#vertical")]
    Vertical,

    /// <summary>
    /// 水平方向；上游取值为 “horizontal”。
    /// </summary>
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
    /// <summary>
    /// 自动选择；上游取值为 “auto”。
    /// </summary>
    [Description("@#auto")]
    Auto,

    /// <summary>
    /// 居中；上游取值为 “center”。
    /// </summary>
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
    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
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
    /// <summary>
    /// 逻辑起始端，方向随 RTL 布局变化；上游取值为 “start”。
    /// </summary>
    [Description("@#start")]
    Start,

    /// <summary>
    /// 逻辑结束端，方向随 RTL 布局变化；上游取值为 “end”。
    /// </summary>
    [Description("@#end")]
    End,

    /// <summary>
    /// 截断时间线起始和结束两端的线条；上游取值为 “both”。
    /// </summary>
    [Description("@#both")]
    Both
}