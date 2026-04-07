namespace ECMAScript.UI.Vue.Vuetify;

/// <summary>
/// Minimal typed activator context for the first scoped-slot Vuetify example.
/// </summary>
public sealed record VDialogActivatorContext(
    bool IsActive,
    string? AriaHasPopup = "dialog");
