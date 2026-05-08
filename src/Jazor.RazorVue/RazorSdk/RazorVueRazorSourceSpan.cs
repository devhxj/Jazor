namespace Jazor.RazorVue.RazorSdk;

internal readonly record struct RazorVueRazorSourceSpan(
    string? FilePath,
    int AbsoluteIndex,
    int Length,
    int LineIndex,
    int CharacterIndex);

internal readonly record struct RazorVueRazorSourceMapping(
    RazorVueRazorSourceSpan OriginalSpan,
    RazorVueRazorSourceSpan GeneratedSpan);
