namespace Jazor.Razor;

public interface IJazorComponent 
{
    // Keep this type intentionally thin: it marks entry into the Jazor Razor
    // substrate without mixing author-facing APIs with compiler analysis logic.
}
