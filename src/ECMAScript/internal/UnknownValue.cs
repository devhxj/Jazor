namespace ECMAScript;

/// <summary>A JavaScript value whose declared type requires checked reads.</summary>
[ECMAScript]
public sealed class UnknownValue
{
    private UnknownValue() { }

    /// <summary>Returns the original string or <see langword="null"/>.</summary>
    public extern string? AsString
    {
        [ECMAScriptInline("(value => typeof value === 'string' ? value : null)(__arg1)")]
        get;
    }

    /// <summary>Returns the original number or <see langword="null"/>.</summary>
    public extern double? AsNumber
    {
        [ECMAScriptInline("(value => typeof value === 'number' ? value : null)(__arg1)")]
        get;
    }

    /// <summary>Returns the original boolean or <see langword="null"/>.</summary>
    public extern bool? AsBoolean
    {
        [ECMAScriptInline("(value => typeof value === 'boolean' ? value : null)(__arg1)")]
        get;
    }

    /// <summary>Returns the original error object if it has the JavaScript Error brand.</summary>
    public extern Error? AsError
    {
        [ECMAScriptInline("(value => Error.isError(value) ? value : null)(__arg1)")]
        get;
    }

    /// <summary>Reads an own field of a non-array object. Missing and nullish values return <see langword="null"/>.</summary>
    // `in` tracks missing keys through Vue proxies before the own-property check.
    [ECMAScriptInline("((value, key) => value !== null && typeof value === 'object' && !Array.isArray(value) && key in value && Object.hasOwn(value, key) ? value[key] ?? null : null)(__arg1, __arg2)")]
    public extern UnknownValue? GetOwn(string key);
}
