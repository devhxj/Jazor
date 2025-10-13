namespace ECMAScript;

/// <summary>
/// URLPatternInit
/// </summary>
[ECMAScript]
[Description("@#URLPatternInit")]
public record URLPatternInit(
    [property: Description("@#protocol")]string? Protocol = default,
	[property: Description("@#username")]string? Username = default,
	[property: Description("@#password")]string? Password = default,
	[property: Description("@#hostname")]string? Hostname = default,
	[property: Description("@#port")]string? Port = default,
	[property: Description("@#pathname")]string? Pathname = default,
	[property: Description("@#search")]string? Search = default,
	[property: Description("@#hash")]string? Hash = default,
	[property: Description("@#baseURL")]string? BaseURL = default);

/// <summary>
/// URLPatternOptions
/// </summary>
[ECMAScript]
[Description("@#URLPatternOptions")]
public record URLPatternOptions(
    [property: Description("@#ignoreCase")]bool IgnoreCase = false);

/// <summary>
/// URLPatternResult
/// </summary>
[ECMAScript]
[Description("@#URLPatternResult")]
public record URLPatternResult(
    [property: Description("@#inputs")]URLPatternInput[]? Inputs = default,
	[property: Description("@#protocol")]URLPatternComponentResult? Protocol = default,
	[property: Description("@#username")]URLPatternComponentResult? Username = default,
	[property: Description("@#password")]URLPatternComponentResult? Password = default,
	[property: Description("@#hostname")]URLPatternComponentResult? Hostname = default,
	[property: Description("@#port")]URLPatternComponentResult? Port = default,
	[property: Description("@#pathname")]URLPatternComponentResult? Pathname = default,
	[property: Description("@#search")]URLPatternComponentResult? Search = default,
	[property: Description("@#hash")]URLPatternComponentResult? Hash = default);

/// <summary>
/// URLPatternComponentResult
/// </summary>
[ECMAScript]
[Description("@#URLPatternComponentResult")]
public record URLPatternComponentResult(
    [property: Description("@#input")]string? Input = default,
	[property: Description("@#groups")]Dictionary<string, Either<string, object>>? Groups = default);

/// <summary>
/// URLPattern
/// </summary>
[ECMAScript]
[Description("@#URLPattern")]
public class URLPattern
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="baseUrl">baseURL</param>
    /// <param name="options">options</param>
    public extern URLPattern(URLPatternInput input, string baseUrl, URLPatternOptions options);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="options">options</param>
    public extern URLPattern(URLPatternInput input, URLPatternOptions options);

    /// <summary>
    /// test
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="baseUrl">baseURL</param>
    [Description("@#test")]
    public extern bool Test(URLPatternInput? input = default, string? baseUrl = default);

    /// <summary>
    /// exec
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="baseUrl">baseURL</param>
    [Description("@#exec")]
    public extern URLPatternResult? Exec(URLPatternInput? input = default, string? baseUrl = default);

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; }

    /// <summary>
    /// username
    /// </summary>
    [Description("@#username")]
    public extern string Username { get; }

    /// <summary>
    /// password
    /// </summary>
    [Description("@#password")]
    public extern string Password { get; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; }

    /// <summary>
    /// hasRegExpGroups
    /// </summary>
    [Description("@#hasRegExpGroups")]
    public extern bool HasRegExpGroups { get; }
}