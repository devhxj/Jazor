; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
JAZOR001 | Security | Error | Unsupported types, members, property accessors, operators, and closed generic usage in the ECMAScript domain
JAZOR002 | Security | Error | Ambiguous runtime type filter, including as casts
JAZOR003 | Security | Error | Invalid SpreadAttribute usage
JAZOR004 | Security | Error | SpreadAttribute conflicts with explicit property name
JAZOR005 | Security | Error | Description and ECMAScriptName specify different JavaScript names
JAZOR006 | Security | Error | Duplicate JavaScript name in one emitted scope
