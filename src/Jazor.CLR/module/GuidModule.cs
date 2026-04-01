namespace Jazor.CLR;

/// <summary>
/// System.Guid 与 UUID 字符串的映射。
/// </summary>
[Jazor(Op.Alias, "System.Guid", "String")]
public static class GuidModule
{
	[Jazor(Op.Inline, "System.Guid.Guid()", "crypto.randomUUID()")]
	public extern static string _0e58e51018e846d2();
}
