using System.Collections.Immutable;

namespace Jazor.RazorVue.Descriptor;

internal readonly record struct VuePropResolution(
    VuePropDescriptor Descriptor,
    string PublicName,
    string PropName,
    bool IsRuntimeNameMatch);

internal static class VuePropResolver
{
    public static bool TryResolve(
        ImmutableArray<VuePropDescriptor> props,
        string authoredName,
        out VuePropResolution resolution)
    {
        foreach (var prop in props)
        {
            if (string.Equals(prop.PublicName, authoredName, StringComparison.Ordinal))
            {
                resolution = new VuePropResolution(prop, prop.PublicName, prop.Name, IsRuntimeNameMatch: false);
                return true;
            }
        }

        foreach (var prop in props)
        {
            if (string.Equals(prop.Name, authoredName, StringComparison.Ordinal))
            {
                resolution = new VuePropResolution(prop, prop.PublicName, prop.Name, IsRuntimeNameMatch: true);
                return true;
            }
        }

        foreach (var prop in props)
        {
            if (string.Equals(prop.PublicName, authoredName, StringComparison.OrdinalIgnoreCase))
            {
                resolution = new VuePropResolution(prop, prop.PublicName, prop.Name, IsRuntimeNameMatch: false);
                return true;
            }
        }

        resolution = default;
        return false;
    }
}
