namespace System.Runtime.CompilerServices;

/// <summary>Local marker used when generated source needs a module initializer without another framework dependency.</summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute
{
}
