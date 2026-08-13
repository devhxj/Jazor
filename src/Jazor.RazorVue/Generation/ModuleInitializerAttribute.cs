namespace System.Runtime.CompilerServices;

/// <summary>
/// Local marker used when generated source needs a module initializer without another framework dependency.
/// 本地定义避免为生成期 bootstrap 引入额外运行时依赖。
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class ModuleInitializerAttribute : Attribute
{
}
