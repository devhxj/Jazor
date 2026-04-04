using Microsoft.AspNetCore.Components;

namespace Jazor.Compiler.Razor;

public abstract class JazorComponent : ComponentBase
{
    // Keep this type intentionally thin: it marks entry into the Razor pipeline
    // without collapsing Razor substrate and Vue authoring into one base class.
}
