using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

internal sealed class EmissionScopeContext
{
    private readonly Dictionary<string, string> _allocatedNames =
        new(System.StringComparer.Ordinal);

    private readonly HashSet<string> _localReservedNames;

    private readonly EmissionScopeContext? _parent;

    private EmissionScopeContext(
        UniqueNameSession session,
        EmissionScopeContext? parent,
        IOperation anchor,
        ScopeSite site,
        HashSet<string> localReservedNames)
    {
        Session = session;
        _parent = parent;
        Anchor = anchor;
        ScopeKey = session.CreateScopeKey(parent?.ScopeKey, site);
        _localReservedNames = localReservedNames;
    }

    public UniqueNameSession Session { get; }

    public IOperation Anchor { get; }

    public string ScopeKey { get; }

    public static EmissionScopeContext CreateRoot(UniqueNameSession session, IOperation anchor, ScopeSite site)
        => new(session, parent: null, anchor, site, new HashSet<string>(System.StringComparer.Ordinal));

    public EmissionScopeContext Enter(IOperation anchor, ScopeSite site)
        => new(Session, this, anchor, site, new HashSet<string>(System.StringComparer.Ordinal));

    public string Allocate(LoweringNameOwner owner, LoweringSite site)
    {
        var allocationKey = BuildAllocationKey(owner, site);
        if (_allocatedNames.TryGetValue(allocationKey, out var name))
            return name;

        var fallbackIndex = 0;
        while (true)
        {
            var salt = fallbackIndex == 0 ? "p" : "f" + fallbackIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var candidate = Session.CreateName(site, ScopeKey, owner, salt);
            if (IsReserved(candidate))
            {
                if (fallbackIndex == int.MaxValue)
                    throw new System.InvalidOperationException("Jazor 无法为当前作用域分配稳定唯一名称，因为稳定后备名称空间已耗尽。");

                fallbackIndex++;
                continue;
            }

            _allocatedNames.Add(allocationKey, candidate);
            _localReservedNames.Add(candidate);
            return candidate;
        }
    }

    private static string BuildAllocationKey(LoweringNameOwner owner, LoweringSite site)
    {
        return string.Concat(
            site.Kind.ToString(),
            "|",
            site.Slot,
            "|",
            owner.IdentityKey);
    }

    private bool IsReserved(string name)
        => _localReservedNames.Contains(name) || (_parent?.IsReserved(name) ?? false);
}
