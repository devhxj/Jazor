using System.Text;

namespace Jazor.RazorVue.RazorSdk;

/// <summary>Produces stable Vue descriptor property names from runtime event names.</summary>
internal static class VueDescriptorNaming
{
    public static string ToListenerPropertyName(string eventName)
    {
        if (eventName.Length == 0 ||
            eventName.StartsWith("on", StringComparison.Ordinal) &&
            eventName.Length > 2 &&
            char.IsUpper(eventName[2]))
        {
            return eventName;
        }

        var result = new StringBuilder(eventName.Length + 2);
        result.Append("on");
        var capitalizeNext = true;
        foreach (var character in eventName)
        {
            if (character == '-')
            {
                capitalizeNext = true;
                continue;
            }

            result.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return result.ToString();
    }
}
