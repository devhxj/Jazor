using System;

namespace Jazor.RazorVue;

internal static class RazorVueDomEventName
{
    public static bool TryNormalizeBlazorEventAttributeName(string attributeName, out string eventName)
    {
        eventName = string.Empty;
        if (string.IsNullOrWhiteSpace(attributeName) ||
            attributeName.Length <= 2 ||
            !attributeName.StartsWith("on", StringComparison.Ordinal))
        {
            return false;
        }

        eventName = attributeName.Substring(2);
        return !string.IsNullOrWhiteSpace(eventName);
    }

    public static string ToVueHandlerPropName(string eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return "on";

        return "on" + char.ToUpperInvariant(eventName[0]) + eventName.Substring(1);
    }
}
