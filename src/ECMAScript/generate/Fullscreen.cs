namespace ECMAScript;

/// <summary>
/// FullscreenOptions
/// </summary>
[ECMAScript]
[Description("@#FullscreenOptions")]
public record FullscreenOptions(
    [property: Description("@#navigationUI")]FullscreenNavigationUI NavigationUI = FullscreenNavigationUI.Auto,
	[property: Description("@#screen")]ScreenDetailed? Screen = default)
{
    [Category("optional")]
    public extern static FullscreenOptions OptionalNavigationUI(
        [Description("@#navigationUI")]FullscreenNavigationUI navigationUi = FullscreenNavigationUI.Auto);

    [Category("optional")]
    public extern static FullscreenOptions OptionalScreen(
        [Description("@#screen")]ScreenDetailed? Screen = default);
}