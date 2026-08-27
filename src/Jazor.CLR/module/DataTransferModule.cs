namespace Jazor.CLR;

[ECMAScriptModule("Microsoft/AspNetCore/Components/Web/DataTransferModule.js")]
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.DataTransfer", "DataTransfer")]
public static class DataTransferModule
{
	// These fields are exposed by the native DataTransfer object and are safe
	// to observe during a drag event without materializing a Blazor DTO.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.get", "__arg1.dropEffect")]
	public extern static string _69d6126953ef76e6(DataTransfer instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.DropEffect.set")]
	public extern static void _10e9a491fcf8c810(DataTransfer instance, string value);

	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.get", "__arg1.effectAllowed")]
	public extern static string? _30bc24b25bf7d9a2(DataTransfer instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.EffectAllowed.set")]
	public extern static void _b719f7d9442296fd(DataTransfer instance, string? value);

	// DataTransfer.types is a browser-owned read-only string sequence. The
	// generated CLR contract erases it to the existing Array<string> carrier.
	[Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.DataTransfer.Types.get", "__arg1.types")]
	public extern static Array<string> _c119fef09012b249(DataTransfer instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.Types.set")]
	public extern static void _509a62c0c67e889b(DataTransfer instance, Array<string> value);

	// File and item payloads are intentionally outside this slice. They require
	// separate File/IBrowserFile and DataTransferItem contracts.
	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.Files.get")]
	public extern static Array<string> _a0fc8027e14f21ca(DataTransfer instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.Files.set")]
	public extern static void _9592e4b9170f4557(DataTransfer instance, Array<string> value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.Items.get")]
	public extern static Array<DataTransferItem> _380a3ba4cfe24381(DataTransfer instance);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.Items.set")]
	public extern static void _25afb4e8576a1ada(DataTransfer instance, Array<DataTransferItem> value);

	[Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransfer.DataTransfer()")]
	public extern static DataTransfer _a4bc87e1b6bb8dea();
}
