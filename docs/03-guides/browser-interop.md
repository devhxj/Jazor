# Browser Interop

This guide describes the `1.0.0-preview.8` browser authoring surface. Install all
Jazor/ECMAScript packages at the same version; see [validation and scope](../04-roadmap/preview8-developer-feedback.md).

## WebIDL and extension mappings

Browser interfaces belong in the WebIDL generator. `Element.ClassList`, `Part`,
`RelList`, and related token properties expose the live `DOMTokenList` interface:
use `Add`, `Remove`, `Toggle`, `Length`, and `GetItem` to read and modify the native
collection. These properties previously exposed `List<string>`; migrate `Count` to
`Length`, use `GetItem(index)` for native `item(index)` access, and migrate
collection-specific code to the corresponding DOM methods.

Additional host mappings for an existing C# type use extension members, following
`internal/Math.cs` and `internal/Object.cs`. For example, the authored `IWindow`
contract retains its existing `Location` member and gains the extension property
**`IWindow.LiveLocation`**, mapped to the same native `location` object. `WindowProxy`
implements `IWindow`, so this extension is available on `Global.Window` as well.
The WebIDL-generated `WindowRef.Location` already
has type `LocationRef` and directly supports `Reload()`:

```csharp
using ECMAScript;

void ReloadBrowser(WindowRef window) => window.Location.Reload();
void ReloadAuthoredWindow(IWindow window) => window.LiveLocation.Reload();
void ReloadProxy(WindowProxy window) => window.LiveLocation.Reload();
```

Explicit `(long)number` and `(ulong)number` conversions enter the BigInt-backed C#
integer domain through JavaScript `BigInt(number)`. Fractional numbers throw; conversion
cannot restore precision already lost in a Number. For integer identifiers outside the
IEEE-754 safe range, retain decimal strings or exact BigInt values at the API boundary.

## Precision-safe JSON

JavaScript `BigInt` values cannot be passed directly to `JSON.stringify`: the runtime
throws a `TypeError`. Keep the C# model strongly typed and choose the conversion at the
serialization call site with the existing typed replacer overload. This keeps ordinary
numbers, `null`, arrays, and nested values on the native JSON path while converting only
`bigint` values to their exact decimal text.

```csharp
using ECMAScript;
using static ECMAScript.Global;
using static ECMAScript.Vue;

var payload = new VueDictionary<VueValue>
{
    ["Id"] = 9007199254740993L,
    ["UnsignedId"] = 18446744073709551615UL,
    ["Count"] = 42,
    ["Nested"] = new VueDictionary<VueValue> { ["Id"] = -9007199254740993L },
    ["Items"] = new VueValue[] { 9007199254740993L, long.MaxValue },
    ["Values"] = new VueValue?[] { null, 7, "unchanged", true }
};

var json = JSON.Stringify(payload,
    (key, value) => TypeOf(value) == "bigint"
        ? value!.ToString()
        : value);
```

This policy is local to the call. It does not mutate `BigInt.prototype`, add an
`object` catch-all API, or change serialization for unrelated modules. It preserves IDs
larger than `2^53` (including nested properties and array elements) as decimal strings.
Use ordinary `double`/number fields only when the API contract explicitly permits the
IEEE-754 safe integer range.

Use a supported structural record or `VueDictionary<VueValue>` for plain JSON object
data. Native `JSON.stringify` enumerates the emitted object's own properties; a local
runtime class retains its JavaScript class/storage shape and does not simulate a CLR
DTO serializer or automatically project C# properties. The BigInt replacer handles
the values it sees, not class-to-DTO conversion.

When a request needs to inspect the generated payload, keep the rejection path explicit:
chain the existing promise `Then` rejection callback (or `Catch`) so a serialization
failure cannot leave a loading state active.

## Razor markup boundaries

The official Razor Source Generator parses markup before RazorVue lowers the generated
C#. The current SDK accepts sibling tags on the same line and interpolated expressions
such as `@($"...")`; feedback reproductions cover both forms. Naked text in a code block
uses `@:` or a `<text>` block. Razor parsing errors may appear as `RZ****` or generated-C#
diagnostics such as `CS1056`; fix the `.razor` source before investigating JavaScript.

For an extern DOM value, use a plain null comparison when the intent is presence:

```csharp
if (Element != null)
    return Element.Id;
return "missing";
```

An empty property pattern (`Element is { } value`) asks the compiler for a runtime type
test. In the current extern DOM reproduction this produces `JAZORVGA026` for an unbound
`HTMLElement` identifier. Use the null comparison above for a presence check.

## Native DOM event and payload extensions

The same CLR-to-WebIDL projection applies to every supported DOM event family:
`ChangeEventArgs`, `InputFileChangeEventArgs`, `MouseEventArgs`, `KeyboardEventArgs`,
`FocusEventArgs`, `PointerEventArgs`, `WheelEventArgs`, `DragEventArgs`,
`ClipboardEventArgs`, `TouchEventArgs`, `ErrorEventArgs`, and `ProgressEventArgs`.
Import `ECMAScript` to expose their extension properties. Each `NativeEvent` returns
the same browser event with its precise generated WebIDL type; it creates no wrapper
or snapshot. Mouse-derived events inherit the common mouse extensions, while their
`NativeEvent` keeps the derived `PointerEvent`, `WheelEvent`, or `DragEvent` type.

| C# carrier | Extra native properties |
| --- | --- |
| All DOM event families | `Target`, `CurrentTarget`, `EventPhase`, `Bubbles`, `Cancelable`, `DefaultPrevented`, `Composed`, `IsTrusted`, `TimeStamp`, `NativeEvent` |
| Change, input-file change | `Type` |
| Mouse, keyboard, focus, touch | `View`, `SourceCapabilities`; keyboard/focus also gain `Detail` |
| Mouse and its derived events | `X`, `Y`, `RelatedTarget` |
| Pointer | `TangentialPressure`, `Twist`, `AltitudeAngle`, `AzimuthAngle`, `PersistentDeviceId` |
| Focus | `RelatedTarget` |
| Clipboard | Nullable native `ClipboardData` |
| Drag | Nullable `NativeDataTransfer` and `Files` |
| Touch event | `NativeTouches`, `NativeTargetTouches`, `NativeChangedTouches` as `TouchList` |
| Touch point | `Target`, `RadiusX`, `RadiusY`, `RotationAngle`, `Force`, `AltitudeAngle`, `AzimuthAngle`, `TouchType`, `NativeTouch` |
| CLR DataTransfer | `NativeFiles`, `NativeItems`, `NativeDataTransfer` |
| CLR DataTransferItem | `NativeItem`; CLR `Kind`/`Type` getters are mapped too |
| ElementReference | `NativeElement` as `HTMLElement` |

The CLR `DataTransfer.Files:string[]` and `Items:DataTransferItem[]` cannot be
overridden by extensions and have different semantics from browser-owned lists.
Use `NativeFiles`/`NativeItems`, or access the complete generated `NativeDataTransfer`
contract. The existing CLR touch array getters still copy the list with `Array.from`;
the new `NativeTouches` family keeps the browser `TouchList` itself.

`ProgressEventArgs.Loaded`/`Total` convert browser Number counters to the CLR `long`
BigInt carrier, so arithmetic such as `args.Loaded + 1` remains valid. The corresponding
`args.NativeEvent.Loaded`/`Total` use the generated WebIDL `double` contract. This
conversion cannot recover integer precision already lost in the browser Number.
Inline extension getters also preserve C# conditional access: `args?.Files` and
`args?.NativeEvent.Type` short-circuit the complete chain and evaluate the receiver once.

```csharp
using ECMAScript;
using Microsoft.AspNetCore.Components.Web;

void HandlePaste(ClipboardEventArgs args)
{
    var clipboard = args.ClipboardData;
    var text = clipboard?.GetData("text/plain");
    args.NativeEvent.PreventDefault();
}

void HandleMouse(MouseEventArgs args)
{
    var target = args.Target;
    var listener = args.CurrentTarget;
    var related = args.RelatedTarget;
    args.NativeEvent.StopPropagation();
}
```

Read `CurrentTarget`, clipboard and drag payloads in the callback before the first
`await`; the browser may reset these values after dispatch. The complete WebIDL
contract also supplies methods such as `GetModifierState`, pointer coalesced events,
and item `GetAsFile` through the native projections. Navigation, validation, and
other synthetic CLR `EventArgs` have no DOM carrier; these extensions apply only to
the explicitly mapped browser types.

## Native file and drop events

The CLR mapping aliases `InputFileChangeEventArgs` to the native `EventRef` and inlines
`FileCount` as `target.files.length`. `NativeFileEventExtensions` in the
`ECMAScript.Vue` assembly supplies the strongly typed `InputFileChangeEventArgs.Files`
and `DragEventArgs.Files` extensions through the opt-in `Jazor.Vue` authoring payload.
The input extension reads `target.files`; the drop extension reads
`dataTransfer?.files`. Browser `FileList` and `FileRef` retain their WebIDL contracts.
Import `ECMAScript` for the extensions and Razor web event directives for modifiers:

```razor
@using ECMAScript
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web

<input type="file" multiple
       onchange="@(EventCallback.Factory.Create<InputFileChangeEventArgs>(this, HandleSelection))" />
<div @ondrop="HandleDrop"
     @ondrop:preventDefault @ondragover:preventDefault>Drop files</div>
```

The handlers receive the native `FileList`, including file names, sizes, and MIME types:

```csharp
private void HandleSelection(InputFileChangeEventArgs args)
    => Upload(args.Files);

private void HandleDrop(DragEventArgs args)
    => Upload(args.Files);

private void Upload(FileList? files)
{
    if (files == null || files.Length == 0)
        return;

    var form = new FormData();
    for (uint index = 0; index < files.Length; index++)
    {
        var file = files.GetItem(index)!;
        form.Append("files", file, file.Name);
    }
    Global.Window.Fetch("/upload", new RequestInit
    {
        Method = "POST",
        Body = new BodyInit((XMLHttpRequestBodyInit)form)
    });
}
```

Use `onchange` with an explicit typed `EventCallback` as shown: ordinary `@onchange`
binds to Blazor `ChangeEventArgs`, whose `Value` contract excludes file selections.
`args.FileCount` lowers to the native list length. `args.Files.GetItem(index)` returns
`FileRef`, exposing `Name`, `Size`, `Type`, `Text()` and `ArrayBuffer()` through the
existing WebIDL API. Read `Files` and capture the needed `FileRef` values before awaiting,
especially for drop events whose data store is available during the callback.

Let the browser set multipart `Content-Type` with its boundary. A cleared selection or
a drop without files takes the empty branch. `InputFileChangeEventArgs.File`,
`GetMultipleFiles()` and `IBrowserFile.OpenReadStream()` keep their explicit unsupported
boundary; these CLR stream APIs are separate from the native `FileRef`/`FileList` slice.

For a preview or download, create a URL with `URL.CreateObjectURL(blob)` and revoke it
with `URL.RevokeObjectURL(url)` after the consumer finishes. For a persistent preview,
also revoke the previous URL when it is replaced and release the final URL on unmount.

The preview.8 mainline and release gates passed: compiler and RazorVue coverage,
public API compatibility, binding generation/inventory, XML documentation, and
23 lockstep package shapes. Packaged Windows SPA and SSR consumers verified real
Chrome interaction, `/docs` and `/todo` PathBase assets, self-contained Deno SSR,
and hydration. See [the validation record](../04-roadmap/preview8-developer-feedback.md)
for commands, counts, and scope.

The DOM event/payload extension regressions verify original event and payload
identity, inherited properties, single evaluation, native collections, and the CLR
DTO boundary.

The new runtime case dispatches mouse, focus, keyboard, pointer, drop, clipboard,
and touch callbacks through real Vue 3.5.42 and Happy DOM 20.10.6, checking event/payload
identity, targets, nullability, element focus and native-list/CLR-array behavior.
Happy DOM requires shaped FileList/TouchList payloads and some missing event initializer
fields; this is DOM-emulation evidence, not a real-browser smoke result.
