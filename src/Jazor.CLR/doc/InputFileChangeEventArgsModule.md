# InputFileChangeEventArgsModule

`Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs` uses the native
DOM `EventRef` carrier. `FileCount` reads `event.target.files.length`; the CLR module
does not construct a Blazor event payload or register a runtime helper import.

`File`, `GetMultipleFiles`, and CLR stream access remain unsupported. For native files, use the
WebIDL interfaces described in [Browser Interop](../../../docs/03-guides/browser-interop.md).
