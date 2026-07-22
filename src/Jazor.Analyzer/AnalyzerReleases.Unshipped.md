; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
JAZOR001 | Security | Error | CodeAnalyzer
JAZOR002 | Security | Error | Ambiguous runtime type filter
JAZOR003 | Security | Error | Invalid SpreadAttribute usage
JAZOR004 | Security | Error | SpreadAttribute conflicts with explicit property name
JAZORVUE001 | RazorVue | Error | Invalid RazorVue component inheritance
JAZORVUE002 | RazorVue | Error | Direct ComponentBase entry is not allowed
JAZORVUE004 | RazorVue | Warning | StateHasChanged has no effect in RazorVue (ignored, not an error)
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
JAZORVUE015 | RazorVue | Error | RazorVue child content parameter value is missing
JAZORVUE016 | RazorVue | Error | RazorVue component declaration is invalid
JAZORVUE018 | RazorVue | Error | RazorVue container inject declaration is invalid
JAZORVGA020 | Jazor.RazorVue.Analysis | Error | RazorVue Razor SG tail output failed
