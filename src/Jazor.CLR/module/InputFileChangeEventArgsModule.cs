namespace Jazor.CLR;

/// <summary>Blazor authoring event projected onto the native file input change event.</summary>
[Jazor(Op.Alias, "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs", "EventRef")]
public static class InputFileChangeEventArgsModule
{
    // The event remains a native Event. File/IBrowserFile stream APIs have a different
    // runtime contract; use the opt-in Files extension to reach browser FileList/File.
    [Jazor(Op.Inline, "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.FileCount.get", "__arg1.target.files.length")]
    public extern static Number FileCount(EventRef instance);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.File.get")]
    public extern static object File(EventRef instance);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.GetMultipleFiles(int)")]
    public extern static Array<object> GetMultipleFiles(EventRef instance, Number maximumFileCount);

    [Jazor(Op.Discard, "Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs.InputFileChangeEventArgs(System.Collections.Generic.IReadOnlyList<Microsoft.AspNetCore.Components.Forms.IBrowserFile>)")]
    public extern static EventRef Create(Array<object> files);
}
