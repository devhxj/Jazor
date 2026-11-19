# Browser Interop

This guide describes the current browser authoring surface. Install all Jazor/ECMAScript packages at the same version; see [installation](./installation-and-configuration.md) and [support scope](../04-roadmap/current-status.md).

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

## Native DOM events

Use the browser event and its WebIDL type in a Jolt handler. Read callback-local values such as
`CurrentTarget`, clipboard data, and drop data before the first `await`. A file input exposes
its selection through `HTMLInputElement.Files`, and a drop event exposes it through
`DragEvent.DataTransfer`. Capture the required `FileRef` values while the browser exposes them.

For a preview, create a URL with `URL.CreateObjectURL(blob)` and release it with
`URL.RevokeObjectURL(url)` when the preview is replaced or removed.
