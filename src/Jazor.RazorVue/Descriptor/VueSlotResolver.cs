using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal readonly record struct VueSlotResolution(
    VueSlotDescriptor Descriptor,
    string PublicName,
    string SlotName,
    bool IsPatternMatch);

internal static class VueSlotResolver
{
    private const string StringPlaceholder = "${string}";

    public static bool TryResolve(
        ImmutableArray<VueSlotDescriptor> slots,
        string publicName,
        out VueSlotResolution resolution)
    {
        foreach (var slot in slots)
        {
            if (!slot.PatternOnly &&
                string.Equals(slot.PublicName, publicName, StringComparison.Ordinal))
            {
                resolution = new VueSlotResolution(slot, publicName, slot.Name, IsPatternMatch: false);
                return true;
            }
        }

        VueSlotDescriptor? matchedSlot = null;
        foreach (var slot in slots)
        {
            if (!MatchesNamePattern(slot.NamePattern, publicName))
                continue;

            if (matchedSlot is not null)
            {
                throw new InvalidOperationException(
                    $"Slot name '{publicName}' matches multiple slot patterns: '{matchedSlot.NamePattern}' and '{slot.NamePattern}'.");
            }

            matchedSlot = slot;
        }

        if (matchedSlot is not null)
        {
            resolution = new VueSlotResolution(matchedSlot, publicName, publicName, IsPatternMatch: true);
            return true;
        }

        resolution = default;
        return false;
    }

    public static bool MatchesAny(ImmutableArray<VueSlotDescriptor> slots, string publicName)
        => TryResolve(slots, publicName, out _);

    private static bool MatchesNamePattern(string? namePattern, string slotName)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            return false;

        var pattern = namePattern!;
        var placeholderIndex = pattern.IndexOf(StringPlaceholder, StringComparison.Ordinal);
        if (placeholderIndex < 0 ||
            placeholderIndex != pattern.LastIndexOf(StringPlaceholder, StringComparison.Ordinal))
        {
            return false;
        }

        var prefix = pattern.Substring(0, placeholderIndex);
        var suffix = pattern.Substring(placeholderIndex + StringPlaceholder.Length);
        return slotName.Length >= prefix.Length + suffix.Length &&
               slotName.StartsWith(prefix, StringComparison.Ordinal) &&
               slotName.EndsWith(suffix, StringComparison.Ordinal);
    }
}
