namespace ECMAScript;

/// <summary>
/// LaunchParams
/// </summary>
[ECMAScript]
[Description("@#LaunchParams")]
public class LaunchParams
{
    /// <summary>
    /// targetURL
    /// </summary>
    [Description("@#targetURL")]
    public extern string? TargetURL { get; }

    /// <summary>
    /// files
    /// </summary>
    [Description("@#files")]
    public extern FrozenSet<FileSystemHandle> Files { get; }
}

/// <summary>
/// LaunchQueue
/// </summary>
[ECMAScript]
[Description("@#LaunchQueue")]
public class LaunchQueue
{
    /// <summary>
    /// setConsumer
    /// </summary>
    /// <param name="consumer">consumer</param>
    [Description("@#setConsumer")]
    public extern void SetConsumer(LaunchConsumer consumer);
}