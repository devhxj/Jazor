using System;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.Pages.Shell;

public sealed record LockFormHandlers(
    [property: ECMAScriptName("onSubmit")] Action<EventRef> OnSubmit,
    [property: ECMAScriptName("onKeydown")] Action<EventRef> OnKeydown) : VueProps;

public sealed record ShellTab(string Route, string Title, string SectionTitle, bool Active, bool IsAffix, Action Open, Action Close, ShellTabContextHandler ContextHandler);
public sealed record ShellTabContextHandler([property: ECMAScriptName("onContextmenu")] Action<EventRef> OnContextmenu) : VueProps;
public sealed record ShellContextAction(string Title, bool Disabled, Action Run);
public sealed record ShellSection(string Title, string Initial, bool Active, Action Select);
public sealed record ShellMenu(string Id, string Title, bool Active, bool IsExternal, bool IsSupported, Action Open);
public sealed record AppIframeDestination(string Title, string Url);
