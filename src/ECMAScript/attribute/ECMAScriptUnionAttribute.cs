using System.Runtime.Versioning;

namespace ECMAScript;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class ECMAScriptUnionAttribute : Attribute
{
}
