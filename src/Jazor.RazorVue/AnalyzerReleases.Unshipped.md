; Unshipped analyzer release
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
JAZORVGA020 | Jazor.RazorVue | Error | RazorVue Razor SG tail output failed
JAZORVCA001 | Jazor.RazorVue.Compatibility | Error | Injected DbContext is server-only in a RazorVue browser component
JAZORVCA002 | Jazor.RazorVue.Compatibility | Error | Injected ASP.NET server-only service is unavailable in a RazorVue browser component
JAZORVCA003 | Jazor.RazorVue.Compatibility | Error | ParameterView.TryGetValue is not materialized by the browser adapter
JAZORVCA004 | Jazor.RazorVue.Compatibility | Error | ParameterView enumeration is not materialized by the browser adapter
JAZORVCA005 | Jazor.RazorVue.Compatibility | Error | ParameterView.ToDictionary is not materialized by the browser adapter
JAZORVCA006 | Jazor.RazorVue.Compatibility | Error | Injected service properties must be writable auto-properties for browser activation
JAZORVCA007 | Jazor.RazorVue.Compatibility | Error | Known Blazor host service has no RazorVue browser adapter
JAZORVCA008 | Jazor.RazorVue.Compatibility | Error | CascadingParameter property is not a writable auto-property for the RazorVue browser adapter
JAZORVCA009 | Jazor.RazorVue.Compatibility | Error | Retained route-host descriptor; generated route catalogs no longer report it for supported `@page` authoring
JAZORVCA010 | Jazor.RazorVue.Compatibility | Error | Retained standard-component descriptor; registered adapters no longer report it for supported component tags
JAZORVCA011 | Jazor.RazorVue.Compatibility | Error | Persistent SSR state and server form handoff require an explicit versioned RazorVue host contract

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
