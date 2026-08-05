// File: EmissionScopeContext.cs
// Purpose: Tracks lexical emission scopes and allocates collision-free stable temporary names.
// 合成名称由语义位点而非遍历顺序决定，既不能遮蔽父作用域，也不能因无关改动抖动。
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.Compiler;

/// <summary>
/// 表示 JavaScript 发射过程中的一个词法/语义作用域。
/// </summary>
/// <remarks>
/// 每个子作用域继承父作用域的名称保留集合，但拥有独立的分配缓存。
/// 这样既能避免合成名称遮蔽父级变量，又能让同一 lowering 位置重复请求时复用同一个名称。
/// </remarks>
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

        // Name generation is deterministic, but a generated hash can still equal a user binding
        // or a name allocated by an ancestor scope. Probe with deterministic salts rather than a
        // traversal counter, so unrelated lowering visits cannot rename this temporary.
        // 哈希不是“天然不冲突”的证明；后备序号同样必须是稳定输入的一部分。
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
