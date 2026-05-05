using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// Historical compatibility marker for pre-named-union surfaces.
/// New public APIs should use explicit <c>[ECMAScriptUnion]</c> contracts instead.
/// </summary>
[ECMAScriptUnion]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IEither
{
}
