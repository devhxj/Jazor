using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace ECMAScript;

/// <summary>Browser file payloads for RazorVue's DOM-origin Blazor event carriers.</summary>
/// <remarks>Import ECMAScript to expose native FileList/File APIs without CLR stream wrappers.</remarks>
[ECMAScript]
public static class NativeFileEventExtensions
{
    extension(InputFileChangeEventArgs args)
    {
        /// <summary>Gets the file input's native FileList; read it during the callback before awaiting.</summary>
        public extern FileList Files
        {
            [ECMAScriptInline("__arg1.target.files")]
            get;
        }
    }

    extension(DragEventArgs args)
    {
        /// <summary>Gets native dropped files; read them during the drop callback before awaiting.</summary>
        public extern FileList? Files
        {
            [ECMAScriptInline("__arg1.dataTransfer?.files")]
            get;
        }
    }
}
