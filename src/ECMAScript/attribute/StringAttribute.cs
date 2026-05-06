using System.Runtime.Versioning;

namespace ECMAScript;

[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
[SupportedOSPlatform("browser")]
public sealed class StringAttribute : Attribute
{
}
