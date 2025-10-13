namespace ECMAScript;

/// <summary>
/// TrustedTypePolicyOptions
/// </summary>
[ECMAScript]
[Description("@#TrustedTypePolicyOptions")]
public record TrustedTypePolicyOptions(
    [property: Description("@#createHTML")]CreateHTMLCallback? CreateHTML = default,
	[property: Description("@#createScript")]CreateScriptCallback? CreateScript = default,
	[property: Description("@#createScriptURL")]CreateScriptURLCallback? CreateScriptURL = default);

/// <summary>
/// TrustedHTML
/// </summary>
[ECMAScript]
[Description("@#TrustedHTML")]
public class TrustedHTML
{
    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern string ToJSON();
}

/// <summary>
/// TrustedScript
/// </summary>
[ECMAScript]
[Description("@#TrustedScript")]
public class TrustedScript
{
    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern string ToJSON();
}

/// <summary>
/// TrustedScriptURL
/// </summary>
[ECMAScript]
[Description("@#TrustedScriptURL")]
public class TrustedScriptURL
{
    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern string ToJSON();
}

/// <summary>
/// TrustedTypePolicyFactory
/// </summary>
[ECMAScript]
[Description("@#TrustedTypePolicyFactory")]
public class TrustedTypePolicyFactory
{
    /// <summary>
    /// createPolicy
    /// </summary>
    /// <param name="policyName">policyName</param>
    /// <param name="policyOptions">policyOptions</param>
    [Description("@#createPolicy")]
    public extern TrustedTypePolicy CreatePolicy(string policyName, TrustedTypePolicyOptions? policyOptions = default);

    /// <summary>
    /// isHTML
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#isHTML")]
    public extern bool IsHTML(object value);

    /// <summary>
    /// isScript
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#isScript")]
    public extern bool IsScript(object value);

    /// <summary>
    /// isScriptURL
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#isScriptURL")]
    public extern bool IsScriptURL(object value);

    /// <summary>
    /// emptyHTML
    /// </summary>
    [Description("@#emptyHTML")]
    public extern TrustedHTML EmptyHTML { get; }

    /// <summary>
    /// emptyScript
    /// </summary>
    [Description("@#emptyScript")]
    public extern TrustedScript EmptyScript { get; }

    /// <summary>
    /// getAttributeType
    /// </summary>
    /// <param name="tagName">tagName</param>
    /// <param name="attribute">attribute</param>
    /// <param name="elementNs">elementNs</param>
    /// <param name="attrNs">attrNs</param>
    [Description("@#getAttributeType")]
    public extern string? GetAttributeType(string tagName, string attribute, string? elementNs = "", string? attrNs = "");

    /// <summary>
    /// getPropertyType
    /// </summary>
    /// <param name="tagName">tagName</param>
    /// <param name="property">property</param>
    /// <param name="elementNs">elementNs</param>
    [Description("@#getPropertyType")]
    public extern string? GetPropertyType(string tagName, string property, string? elementNs = "");

    /// <summary>
    /// defaultPolicy
    /// </summary>
    [Description("@#defaultPolicy")]
    public extern TrustedTypePolicy? DefaultPolicy { get; }
}

/// <summary>
/// TrustedTypePolicy
/// </summary>
[ECMAScript]
[Description("@#TrustedTypePolicy")]
public class TrustedTypePolicy
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// createHTML
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="arguments">arguments</param>
    [Description("@#createHTML")]
    public extern TrustedHTML CreateHTML(string input, object arguments);

    /// <summary>
    /// createScript
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="arguments">arguments</param>
    [Description("@#createScript")]
    public extern TrustedScript CreateScript(string input, object arguments);

    /// <summary>
    /// createScriptURL
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="arguments">arguments</param>
    [Description("@#createScriptURL")]
    public extern TrustedScriptURL CreateScriptURL(string input, object arguments);
}