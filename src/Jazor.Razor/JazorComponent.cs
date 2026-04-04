using Microsoft.AspNetCore.Components;

namespace Jazor.Razor;

public abstract class JazorComponent : ComponentBase
{
    // Keep this type intentionally thin: it marks entry into the Jazor Razor
    // substrate without mixing author-facing APIs with compiler analysis logic.
}
