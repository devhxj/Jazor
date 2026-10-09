namespace Jazor.CLR;

/// <summary>Blazor drag payload item projected onto the browser's native DataTransferItem.</summary>
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Web.DataTransferItem", "DataTransferItem")]
public static class DataTransferItemModule
{
    // Browser-owned items are read-only views. Native file/string access is exposed
    // by the typed extension surface rather than by materializing a Blazor DTO.
    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.DataTransferItem.Kind.get", "__arg1.kind")]
    public extern static string GetKind(DataTransferItem instance);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransferItem.Kind.set")]
    public extern static void SetKind(DataTransferItem instance, string value);

    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Web.DataTransferItem.Type.get", "__arg1.type")]
    public extern static string GetType(DataTransferItem instance);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransferItem.Type.set")]
    public extern static void SetType(DataTransferItem instance, string value);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Web.DataTransferItem.DataTransferItem()")]
    public extern static DataTransferItem Create();
}
