namespace Jazor.CLR;

/// <summary>
/// System.Type 相关最小映射。
/// </summary>
[Jazor(Op.Allowed, "System.Type")]
public static class TypeModule
{
	[Jazor(Op.Alias, "abstract System.Reflection.MemberInfo.Name.get", "name")]
	public extern static string _51d0f6e5b66c4437(System.Type instance);

	[Jazor(Op.Alias, "virtual System.Reflection.MemberInfo.Name.get", "name")]
	public extern static string _f4438d3dc5cf4141(System.Type instance);

	[Jazor(Op.Alias, "virtual System.Type.Name.get", "name")]
	public extern static string _7d9a7a6686e842d3(System.Type instance);
}
