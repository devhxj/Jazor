namespace ECMAScript;

/// <summary>
/// Excludes a member from Jazor ECMAScript lowering and host-member discovery.
/// 标记成员应被 Jazor ECMAScript lowering 与宿主成员发现逻辑忽略。
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public sealed class ECMAScriptIgnoreAttribute : Attribute
{

}
