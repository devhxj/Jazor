// File: EventLowering.cs
// Purpose: Contains the shared lowering model for supported C# event operations.
// 将事件订阅/取消映射到明确的 JavaScript runtime seam，避免把 CLR event identity 误当作原生 JS 能力。
using Jazor.Common;
using Microsoft.CodeAnalysis;

namespace Jazor.Compiler;

/// <summary>
/// Defines the compiler-owned runtime protocol for field-like member-class events.
/// </summary>
/// <remarks>
/// C# events are multicast delegates rather than ordinary writable properties. The generated
/// names are deliberately outside the C# identifier domain, so source members cannot collide
/// with the storage and helper protocol. Both declaration emission and operation lowering use
/// this class to keep the protocol symbol-bound and deterministic.
/// </remarks>
internal static class EventLowering
{
    public static bool IsSupportedFieldLikeInstanceEvent(IEventSymbol eventSymbol, out string reason)
    {
        if (eventSymbol.ContainingType.TypeKind != TypeKind.Class || eventSymbol.ContainingType.IsRecord)
        {
            reason = "event storage is only supported on non-record runtime member classes.";
            return false;
        }

        if (eventSymbol.IsStatic)
        {
            reason = "static events are not supported by the member-class event protocol.";
            return false;
        }

        if (eventSymbol.IsAbstract)
        {
            reason = "abstract events do not provide a concrete multicast storage protocol.";
            return false;
        }

        // Virtual add/remove accessors dispatch independently from a field-like event's backing
        // storage. The protocol intentionally has one storage slot per declared event, so it must
        // reject virtual/override events instead of pretending a normal helper call preserves dispatch.
        if (eventSymbol.IsVirtual || eventSymbol.IsOverride)
        {
            reason = "virtual and override events require accessor dispatch that is not supported by the member-class event protocol.";
            return false;
        }

        // Roslyn creates add/remove accessors as one pair. IEventSymbol therefore always has an
        // add accessor; checking its declaredness distinguishes synthesized storage from custom accessors.
        if (!eventSymbol.AddMethod!.IsImplicitlyDeclared)
        {
            reason = "custom event accessors are not supported by the field-like event protocol.";
            return false;
        }

        // IEventSymbol is only produced for a delegate-typed C# event. Revalidating that language
        // invariant here would add an unreachable fallback rather than a product boundary.
        var invokeMethod = ((INamedTypeSymbol)eventSymbol.Type).DelegateInvokeMethod!;

        if (invokeMethod.Parameters.Any(static parameter => parameter.RefKind != RefKind.None))
        {
            reason = "delegate Invoke signatures with by-reference parameters are not supported by the multicast snapshot protocol.";
            return false;
        }

        if (invokeMethod.ReturnsByRef || invokeMethod.ReturnsByRefReadonly)
        {
            reason = "delegate Invoke signatures with by-reference returns are not supported by the multicast snapshot protocol.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public static IMethodSymbol GetInvokeMethod(IEventSymbol eventSymbol)
    {
        if (!IsSupportedFieldLikeInstanceEvent(eventSymbol, out var reason))
        {
            throw new NotSupportedException(
                $"Jazor member class event '{eventSymbol.Name}' cannot lower: {reason}");
        }

        return ((INamedTypeSymbol)eventSymbol.Type).DelegateInvokeMethod!;
    }

    public static string GetStorageName(IEventSymbol eventSymbol)
        => "$event_store_" + GetStableSuffix(eventSymbol);

    public static string GetAddMethodName(IEventSymbol eventSymbol)
        => "$event_add_" + GetStableSuffix(eventSymbol);

    public static string GetRemoveMethodName(IEventSymbol eventSymbol)
        => "$event_remove_" + GetStableSuffix(eventSymbol);

    public static string GetSnapshotMethodName(IEventSymbol eventSymbol)
        => "$event_snapshot_" + GetStableSuffix(eventSymbol);

    private static string GetStableSuffix(IEventSymbol eventSymbol)
        => Format.HashName(eventSymbol.OriginalDefinition.ToDisplayString(Format.NameFormat)).TrimStart('_');
}
