using Jazor.Npm;
using Acornima.Ast;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Jolt.Compiler;

internal sealed partial class VueLowering
{
    internal required INamedTypeSymbol StateType { get; init; }
    private bool _usesShallowState;
    private readonly Dictionary<IFieldSymbol, bool> _refStorage = new(SymbolEqualityComparer.Default);

    public override Expression? RewriteFieldReference(IFieldReferenceOperation operation, SenseArgument argument, Expression? instance)
    {
        if (instance is null || RewriteAutoPropertyBackingFieldReceiver(operation.Field, instance) is not { } receiver)
            return null;
        return Member(receiver, RuntimeClassPrivateStorageNames.GetProxySafeFieldName(operation.Field));
    }

    public override Expression? RewriteAutoPropertyBackingFieldReceiver(IFieldSymbol field, Expression receiver)
    {
        if (field.IsStatic || !SymbolEqualityComparer.Default.Equals(StateType, field.ContainingType))
            return null;
        if (!_refStorage.TryGetValue(field, out var storesRef))
        {
            storesRef = NpmValueProjection.ContainsVueRef(field.Type);
            _refStorage.Add(field, storesRef);
        }
        if (!storesRef)
            return null;

        // Both Vue views track the same raw target. Use the cached shallow view only for
        // ref-bearing storage so reads and replacements preserve the typed ref identity.
        _usesShallowState = true;
        return Call(Id("$joltShallowReactive"), Call(Id("$joltToRaw"), receiver));
    }
}
