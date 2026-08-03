namespace Jazor.CLR;

[ECMAScriptModule("System/RangeModule.js")]
[Jazor(Op.Alias, "System.Range", "Object")]
public static class RangeModule
{
	private static bool EqualsCore(RuntimeModule.JIndex left, RuntimeModule.JIndex right)
		=> left.Value == right.Value && left.IsFromEnd == right.IsFromEnd;

	private static Number GetIndexHashCode(RuntimeModule.JIndex instance)
		=> (instance.Value * 2) + (instance.IsFromEnd ? 1 : 0);

	private static string GetIndexText(RuntimeModule.JIndex instance)
		=> instance.IsFromEnd ? "^" + instance.Value : "" + instance.Value;

	[Jazor(Op.Import, "System.Range.Range()")]
	public static RuntimeModule.JRange _d5659647559c2c27() => new(new(0, false), new(0, false));

	[Jazor(Op.Import, "System.Range.Start.get")]
	public static RuntimeModule.JIndex _ff879b9ef9597efb(RuntimeModule.JRange instance) => instance.Start;

	[Jazor(Op.Import, "System.Range.End.get")]
	public static RuntimeModule.JIndex _0be235222ad447c5(RuntimeModule.JRange instance) => instance.End;

	///<summary>Instantiates a new <see cref="T:System.Range" /> instance with the specified starting and ending indexes.</summary>
	[Jazor(Op.Import, "System.Range.Range(System.Index, System.Index)")]
	public static RuntimeModule.JRange _fc3dfc5dbaa397eb(RuntimeModule.JIndex start, RuntimeModule.JIndex end) => new(start, end);

	///<summary>Returns a value that indicates whether the current instance is equal to a specified object.</summary>
	[Jazor(Op.Import, "override System.Range.Equals(object)")]
	public static bool _31b6c9a4877f04c4(RuntimeModule.JRange instance, object? value)
	{
		var other = value as RuntimeModule.JRange;
		return other != null
			&& EqualsCore(instance.Start, other.Start)
			&& EqualsCore(instance.End, other.End);
	}

	///<summary>Returns a value that indicates whether the current instance is equal to another <see cref="T:System.Range" /> object.</summary>
	[Jazor(Op.Import, "System.Range.Equals(System.Range)")]
	public static bool _f858c453f3829489(RuntimeModule.JRange instance, RuntimeModule.JRange other)
		=> EqualsCore(instance.Start, other.Start) && EqualsCore(instance.End, other.End);

	///<summary>Returns the hash code for this instance.</summary>
	[Jazor(Op.Import, "override System.Range.GetHashCode()")]
	public static Number _7fc0f3cc7ec542d3(RuntimeModule.JRange instance)
		=> (GetIndexHashCode(instance.Start) * 397) ^ GetIndexHashCode(instance.End);

	///<summary>Returns the string representation of the current <see cref="T:System.Range" /> object.</summary>
	[Jazor(Op.Import, "override System.Range.ToString()")]
	public static string _1c286146a6526629(RuntimeModule.JRange instance)
		=> GetIndexText(instance.Start) + ".." + GetIndexText(instance.End);

	///<summary>Returns a new <see cref="T:System.Range" /> instance starting from a specified start index to the end of the collection.</summary>
	[Jazor(Op.Import, "static System.Range.StartAt(System.Index)")]
	public static RuntimeModule.JRange _2cc8d1f98d9f4b16(RuntimeModule.JIndex start) => new(start, new(0, true));

	///<summary>Creates a <see cref="T:System.Range" /> object starting from the first element in the collection to a specified end index.</summary>
	[Jazor(Op.Import, "static System.Range.EndAt(System.Index)")]
	public static RuntimeModule.JRange _1df4ded30f6797b5(RuntimeModule.JIndex end) => new(new(0, false), end);

	[Jazor(Op.Import, "static System.Range.All.get")]
	public static RuntimeModule.JRange _9fb8edf805e88967() => new(new(0, false), new(0, true));

	///<summary>Calculates the start offset and length of the range object using a collection length.</summary>
	[Jazor(Op.Import, "System.Range.GetOffsetAndLength(int)")]
	public static (Number Offset, Number Length) _1c7a1e658ed790ff(RuntimeModule.JRange instance, Number length)
		=> instance.GetOffsetAndLength(length);
}
