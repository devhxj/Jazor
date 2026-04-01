namespace ECMAScript.Console;

/// <summary>
/// console
/// </summary>
[ECMAScript]
[Description("@#console")]
public static class Console
{
    /// <summary>
    /// assert
    /// </summary>
    /// <param name="condition">condition</param>
    /// <param name="data">data</param>
    [Description("@#assert")]
    public static extern void Assert(bool condition = false, params object[] data);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public static extern void Clear();

    /// <summary>
    /// debug
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#debug")]
    public static extern void Debug(params object[] data);

    /// <summary>
    /// error
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#error")]
    public static extern void Error(params object[] data);

    /// <summary>
    /// info
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#info")]
    public static extern void Info(params object[] data);

    /// <summary>
    /// log
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#log")]
    public static extern void Log(params object[] data);

    /// <summary>
    /// table
    /// </summary>
    /// <param name="tabularData">tabularData</param>
    /// <param name="properties">properties</param>
    [Description("@#table")]
    public static extern void Table(object? tabularData = default, string[]? properties = default);

    /// <summary>
    /// trace
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#trace")]
    public static extern void Trace(params object[] data);

    /// <summary>
    /// warn
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#warn")]
    public static extern void Warn(params object[] data);

    /// <summary>
    /// dir
    /// </summary>
    /// <param name="item">item</param>
    /// <param name="options">options</param>
    [Description("@#dir")]
    public static extern void Dir(object? item = default, object? options = default);

    /// <summary>
    /// dirxml
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#dirxml")]
    public static extern void Dirxml(params object[] data);

    /// <summary>
    /// count
    /// </summary>
    /// <param name="label">label</param>
    [Description("@#count")]
    public static extern void Count(string label = "default");

    /// <summary>
    /// countReset
    /// </summary>
    /// <param name="label">label</param>
    [Description("@#countReset")]
    public static extern void CountReset(string label = "default");

    /// <summary>
    /// group
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#group")]
    public static extern void Group(params object[] data);

    /// <summary>
    /// groupCollapsed
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#groupCollapsed")]
    public static extern void GroupCollapsed(params object[] data);

    /// <summary>
    /// groupEnd
    /// </summary>
    [Description("@#groupEnd")]
    public static extern void GroupEnd();

    /// <summary>
    /// time
    /// </summary>
    /// <param name="label">label</param>
    [Description("@#time")]
    public static extern void Time(string label = "default");

    /// <summary>
    /// timeLog
    /// </summary>
    /// <param name="label">label</param>
    /// <param name="data">data</param>
    [Description("@#timeLog")]
    public static extern void TimeLog(string label = "default", params object[] data);

    /// <summary>
    /// timeEnd
    /// </summary>
    /// <param name="label">label</param>
    [Description("@#timeEnd")]
    public static extern void TimeEnd(string label = "default");
}
