namespace ECMAScript;

/// <summary>
/// 用于标记属性或方法需要被 Jazor编译器进行针对性处理
/// </summary>
/// <param name="key"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method, Inherited = false)]
internal sealed class SpecialCompileAttribute() : Attribute
{
}
