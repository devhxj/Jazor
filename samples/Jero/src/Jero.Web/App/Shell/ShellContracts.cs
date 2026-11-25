using System;
using ECMAScript;
using static ECMAScript.Vue;

namespace Jero.Web.App.Shell;

public sealed partial record AppUnreadNotice
{
    public string id { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string content { get; init; } = string.Empty;
    public int type { get; init; }
    public string publicUserName { get; init; } = string.Empty;
    public string publicTime { get; init; } = string.Empty;
}


public sealed record LockFormHandlers(Action<EventRef> onSubmit, Action<EventRef> onKeydown) : VueProps;

public sealed record ShellTab(string Route, string Title, bool Active, bool IsAffix, Action Open, Action Close, ShellTabContextHandler ContextHandler);
public sealed record ShellTabContextHandler(Action<EventRef> onContextmenu) : VueProps;
public sealed record ShellContextAction(string Title, bool Disabled, Action Run);
public sealed record ShellSection(string Title, string Initial, bool Active, Action Select);
public sealed record ShellMenu(string Id, string Title, bool Active, bool IsExternal, bool IsSupported, Action Open);
public sealed record AppIframeDestination(string Title, string Url);
