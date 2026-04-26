; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
JAZOR001 | Security | Error | CodeAnalyzer
JAZOR002 | Security | Error | Ambiguous runtime type filter
JAZORVUE001 | RazorVue | Error | Invalid RazorVue component inheritance
JAZORVUE002 | RazorVue | Error | Direct ComponentBase entry is not allowed
JAZORVUE004 | RazorVue | Error | StateHasChanged is not part of RazorVue semantics
JAZORVUE005 | RazorVue | Error | ShouldRender is not part of RazorVue semantics
JAZORVUE006 | RazorVue | Error | SetParametersAsync is not part of RazorVue semantics
JAZORVUE007 | RazorVue | Error | RazorVue parameter is unknown
JAZORVUE008 | RazorVue | Error | RazorVue bind target is invalid
JAZORVUE009 | RazorVue | Error | RazorVue child content parameter is unknown
JAZORVUE010 | RazorVue | Error | RazorVue child content parameter context is invalid
JAZORVUE011 | RazorVue | Error | RazorVue child content parameter is assigned multiple times
JAZORVUE012 | RazorVue | Error | RazorVue library component declaration is invalid
JAZORVUE013 | RazorVue | Error | RazorVue library style dependency declaration is invalid
JAZORVUE014 | RazorVue | Error | RazorVue library plugin requirement declaration is invalid
