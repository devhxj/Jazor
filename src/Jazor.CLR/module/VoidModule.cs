namespace Jazor.CLR;

/// <summary>
/// 声明 void 在 Jazor 类型分析中的允许映射。
/// </summary>
/// <remarks>
/// void 是编译期返回类型，不产生 JavaScript runtime 对象；该 record 只作为白名单类型声明
/// 存在，不能被当作可实例化的 CLR value 使用。
/// </remarks>
[Jazor(Op.Allowed, "void")]
public record VoidModule;
