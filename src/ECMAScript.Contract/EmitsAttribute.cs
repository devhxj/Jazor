namespace ECMAScript.Contract;

/// <summary>
/// 声明当前字符串数组成员在未显式提供时，应从同一对象字面量中的 setup 回调里自动推导事件名。
///
/// 当前约定只接受稳定字符串字面量事件名；如果 setup 中存在无法静态确定的 emit 名称，
/// 调用方应显式提供成员值而不是依赖推导。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class EmitsAttribute : Attribute
{
}
