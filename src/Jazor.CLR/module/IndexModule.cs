namespace Jazor.CLR;

[ECMAScriptModule("System/IndexModule.js")]
[Jazor(Op.Alias, "System.Index", "Object")]
public static class IndexModule
{
	[Jazor(Op.Import, "System.Index.Index()")]
	public static RuntimeModule.JIndex _94a150c0b38bdd79() => new(0, false);

	///<summary>Initializes a new <see cref="T:System.Index" /> with a specified index position and a value that indicates if the index is from the beginning or the end of a collection.</summary>
	[Jazor(Op.Import, "System.Index.Index(int, bool)")]
	public static RuntimeModule.JIndex _f406c4c734b11d38(Number value, bool fromEnd) => new(value, fromEnd);

	[Jazor(Op.Import, "static System.Index.Start.get")]
	public static RuntimeModule.JIndex _c6ec2b575aff2e24() => new(0, false);

	[Jazor(Op.Import, "static System.Index.End.get")]
	public static RuntimeModule.JIndex _0ba7c760bb17a58f() => new(0, true);

	///<summary>Creates an <see cref="T:System.Index" /> from the specified index at the start of a collection.</summary>
	[Jazor(Op.Import, "static System.Index.FromStart(int)")]
	public static RuntimeModule.JIndex _1b0e1c2ab6c4cd39(Number value) => new(value, false);

	///<summary>Creates an <see cref="T:System.Index" /> from the end of a collection at a specified index position.</summary>
	[Jazor(Op.Import, "static System.Index.FromEnd(int)")]
	public static RuntimeModule.JIndex _ce8b9229a41c8545(Number value) => new(value, true);

	[Jazor(Op.Import, "System.Index.Value.get")]
	public static Number _71953783d6b61ae1(RuntimeModule.JIndex instance) => instance.Value;

	[Jazor(Op.Import, "System.Index.IsFromEnd.get")]
	public static bool _b141712b3756cf57(RuntimeModule.JIndex instance) => instance.IsFromEnd;

	///<summary>Calculates the offset from the start of the collection using the specified collection length.</summary>
	[Jazor(Op.Import, "System.Index.GetOffset(int)")]
	public static Number _9b817e75f3f8f58f(RuntimeModule.JIndex instance, Number length) => instance.GetOffset(length);

	private static Number GetHashCodeCore(RuntimeModule.JIndex instance)
		=> (instance.Value * 2) + (instance.IsFromEnd ? 1 : 0);

	///<summary>Indicates whether the current Index object is equal to a specified object.</summary>
	[Jazor(Op.Import, "override System.Index.Equals(object)")]
	public static bool _2910b3afb47ad8b1(RuntimeModule.JIndex instance, object? value)
	{
		var other = value as RuntimeModule.JIndex;
		return other != null
			&& instance.Value == other.Value
			&& instance.IsFromEnd == other.IsFromEnd;
	}

	///<summary>Returns a value that indicates whether the current object is equal to another <see cref="T:System.Index" /> object.</summary>
	[Jazor(Op.Import, "System.Index.Equals(System.Index)")]
	public static bool _83db7aa629254762(RuntimeModule.JIndex instance, RuntimeModule.JIndex other)
		=> instance.Value == other.Value && instance.IsFromEnd == other.IsFromEnd;

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import, "override System.Index.GetHashCode()")]
	public static Number _1c7f7405a620c971(RuntimeModule.JIndex instance)
		=> GetHashCodeCore(instance);

	///<summary>Converts an integer number to an <see cref="T:System.Index" />.</summary>
	[Jazor(Op.Import, "static System.Index.implicit operator System.Index(int)")]
	public static RuntimeModule.JIndex _1e1b56e4e760a5d5(Number value) => new(value, false);

	///<summary>Returns the string representation of the current <see cref="T:System.Index" /> instance.</summary>
	[Jazor(Op.Import, "override System.Index.ToString()")]
	public static string _0fb768c390456f95(RuntimeModule.JIndex instance)
		=> instance.IsFromEnd ? "^" + instance.Value : "" + instance.Value;
}
