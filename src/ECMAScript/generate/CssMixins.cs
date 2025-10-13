namespace ECMAScript;

/// <summary>
/// FunctionParameter
/// </summary>
[ECMAScript]
[Description("@#FunctionParameter")]
public record FunctionParameter(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#type")]string? Type = default,
	[property: Description("@#defaultValue")]string? DefaultValue = default);

/// <summary>
/// CSSFunctionRule
/// </summary>
[ECMAScript]
[Description("@#CSSFunctionRule")]
public class CSSFunctionRule : CSSGroupingRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// getParameters
    /// </summary>
    [Description("@#getParameters")]
    public extern FunctionParameter[] GetParameters();

    /// <summary>
    /// returnType
    /// </summary>
    [Description("@#returnType")]
    public extern string ReturnType { get; }
}

/// <summary>
/// CSSFunctionDescriptors
/// </summary>
[ECMAScript]
[Description("@#CSSFunctionDescriptors")]
public class CSSFunctionDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// result
    /// </summary>
    [Description("@#result")]
    public extern string Result { get; set; }
}

/// <summary>
/// CSSFunctionDeclarations
/// </summary>
[ECMAScript]
[Description("@#CSSFunctionDeclarations")]
public class CSSFunctionDeclarations : CSSRule
{
    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSFunctionDescriptors Style { get; }
}