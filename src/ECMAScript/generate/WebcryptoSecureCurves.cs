namespace ECMAScript;

/// <summary>
/// Ed448Params
/// </summary>
[ECMAScript]
[Description("@#Ed448Params")]
public record Ed448Params(
    [property: Description("@#context")]IBufferSource? Context = default): Algorithm;