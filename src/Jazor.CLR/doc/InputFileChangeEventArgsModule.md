# InputFileChangeEventArgsModule

`Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs` uses the native
DOM `EventRef` carrier. `FileCount` reads `event.target.files.length`; the CLR module
does not construct a Blazor event payload or register a runtime helper import.

preview.8 also exposes `NativeEvent` and the common DOM event properties through
extensions on the existing C# type. `NativeEvent` preserves the same WebIDL
`EventRef` identity. See [Browser Interop](../../../docs/03-guides/browser-interop.md)
for the event and native payload surface.

Import `ECMAScript` from the opt-in `Jazor.Vue` authoring payload for the extension
`args.Files`, which returns the native browser `FileList`. `DragEventArgs.Files`
projects `event.dataTransfer?.files` for drop callbacks. Read the needed `FileRef`
values before awaiting and then use normal `Name`/`Size`/`Type`, `ArrayBuffer` or
`FormData.Append` WebIDL bindings.

`File`, `GetMultipleFiles`, synthetic construction and `IBrowserFile`/CLR Stream
semantics remain unsupported. Use an explicit
`onchange="@(EventCallback.Factory.Create<InputFileChangeEventArgs>(this, Handler))"`
for a native file input; ordinary `@onchange` is bound by the official Razor SG to
`ChangeEventArgs` and its scalar `Value` projection.
