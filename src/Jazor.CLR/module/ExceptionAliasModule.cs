namespace Jazor.CLR;

/// <summary>
/// 衍生异常类型别名映射。
/// </summary>
[Jazor(Op.Alias, "System.InvalidOperationException", "Error")]
public static class InvalidOperationExceptionModule
{
}

/// <summary>
/// ArgumentNullException 映射到 JavaScript TypeError。
/// </summary>
[Jazor(Op.Alias, "System.ArgumentNullException", "TypeError")]
public static class ArgumentNullExceptionModule
{
}

/// <summary>
/// DivideByZeroException 映射到 JavaScript Error。
/// </summary>
[Jazor(Op.Alias, "System.DivideByZeroException", "Error")]
public static class DivideByZeroExceptionModule
{
}
