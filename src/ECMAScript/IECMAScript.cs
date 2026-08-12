namespace ECMAScript;

/// <summary>
/// Minimal marker interface for ECMAScript host bindings.
/// ECMAScript host binding 使用的最小标记接口。
/// </summary>
/// <remarks>
/// It only states that an authoring type belongs to the ECMAScript contract; it does not emit a JavaScript interface object or perform runtime checks.
/// 该接口只用于表达 authoring 类型属于 ECMAScript contract，不代表会在 JavaScript 中生成同名接口对象，
/// 也不提供运行时类型检查。
/// </remarks>
public interface IECMAScript
{

}
