namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/ProgressEventArgsModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs", "ProgressEvent")]
public static class ProgressEventArgsModule
{
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.LengthComputable.get", "__arg1.lengthComputable")]
	public extern static bool _bae28343429bde7e(ProgressEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.LengthComputable.set")]
	public extern static void _22c2a4d44ec32b04(ProgressEvent instance, bool value);

	// ProgressEvent.loaded/total are WebIDL unsigned long long values, so they
	// retain the BigInt carrier. This is distinct from WebIDL long/int fields.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Loaded.get", "__arg1.loaded")]
	public extern static BigInt _474fa409d12984bc(ProgressEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Loaded.set")]
	public extern static void _64d355fb34da54b9(ProgressEvent instance, BigInt value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Total.get", "__arg1.total")]
	public extern static BigInt _e78be307199c1aa7(ProgressEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Total.set")]
	public extern static void _4dba6c365de468c5(ProgressEvent instance, BigInt value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Type.get", "__arg1.type")]
	public extern static string _7a6ec1e4bdffa1c5(ProgressEvent instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.Type.set")]
	public extern static void _0664569699e075cf(ProgressEvent instance, string value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.ProgressEventArgs.ProgressEventArgs()")]
	public extern static ProgressEvent _27278c163cb0f823();
}
