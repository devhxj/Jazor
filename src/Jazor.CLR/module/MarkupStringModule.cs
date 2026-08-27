namespace Jazor.CLR;

// MarkupString construction and rendering are recognized by the direct render
// emitter. The type is admitted for that product boundary, not as a CLR object alias.
[Jazor(Op.Allowed, "Microsoft.AspNetCore.Components.MarkupString")]
public static class MarkupStringModule
{
	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.MarkupString.MarkupString(string)")]
	public extern static Object _21dcb9a52d94340d(string value);

	[Jazor(Op.Discard ,"Microsoft.AspNetCore.Components.MarkupString.Value.get")]
	public extern static string _eaeb000ac7f9a6f6(Object instance);

	[Jazor(Op.Discard ,"static Microsoft.AspNetCore.Components.MarkupString.explicit operator Microsoft.AspNetCore.Components.MarkupString(string)")]
	public extern static Object _34136c18a2219b97(string value);

	[Jazor(Op.Discard ,"override Microsoft.AspNetCore.Components.MarkupString.ToString()")]
	public extern static string _6d00a0d23b291c51(Object instance);
}
