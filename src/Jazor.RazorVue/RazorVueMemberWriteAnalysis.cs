using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Jazor.RazorVue;

internal static class RazorVueMemberWriteAnalysis
{
    public static bool CanUseSourceStableMutableCarrierMember(ISymbol member)
        => member switch
        {
            IPropertySymbol propertySymbol when propertySymbol.SetMethod is not null
                => propertySymbol.SetMethod.DeclaredAccessibility == Accessibility.Private,
            IFieldSymbol fieldSymbol when !fieldSymbol.IsReadOnly
                => fieldSymbol.DeclaredAccessibility == Accessibility.Private,
            _ => false
        };

    public static bool HasObservableWritesOutsideDeclarationInitializer(
        Compilation compilation,
        ISymbol member)
    {
        if (compilation is null)
            throw new ArgumentNullException(nameof(compilation));
        if (member is null)
            throw new ArgumentNullException(nameof(member));

        var containingType = member.ContainingType;
        if (containingType is null)
            return true;

        foreach (var syntaxReference in containingType.DeclaringSyntaxReferences)
        {
            var syntax = syntaxReference.GetSyntax();
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);

            foreach (var descendant in syntax.DescendantNodes())
            {
                if (descendant is MemberDeclarationSyntax memberDeclaration &&
                    IsDeclaringMemberSyntax(member, memberDeclaration))
                {
                    continue;
                }

                if (semanticModel.GetOperation(descendant) is not IOperation operation)
                    continue;

                if (IsWriteToMember(operation, member))
                    return true;
            }
        }

        return false;
    }

    private static bool IsDeclaringMemberSyntax(ISymbol member, MemberDeclarationSyntax declarationSyntax)
    {
        foreach (var syntaxReference in member.DeclaringSyntaxReferences)
        {
            if (ReferenceEquals(syntaxReference.SyntaxTree, declarationSyntax.SyntaxTree) &&
                syntaxReference.Span.Equals(declarationSyntax.Span))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsWriteToMember(IOperation operation, ISymbol member)
        => operation switch
        {
            ISimpleAssignmentOperation assignment => TargetsMember(assignment.Target, member),
            ICompoundAssignmentOperation compoundAssignment => TargetsMember(compoundAssignment.Target, member),
            IIncrementOrDecrementOperation incrementOrDecrement => TargetsMember(incrementOrDecrement.Target, member),
            ICoalesceAssignmentOperation coalesceAssignment => TargetsMember(coalesceAssignment.Target, member),
            IArgumentOperation argument
                when argument.Parameter?.RefKind is RefKind.Ref or RefKind.Out
                     && TargetsMember(argument.Value, member) => true,
            _ => false
        };

    private static bool TargetsMember(IOperation? target, ISymbol member)
    {
        var current = RazorVueOperationNormalizer.Unwrap(target);
        return current switch
        {
            IPropertyReferenceOperation propertyReference
                when SymbolEqualityComparer.Default.Equals(propertyReference.Property, member) => true,
            IFieldReferenceOperation fieldReference
                when SymbolEqualityComparer.Default.Equals(fieldReference.Field, member) => true,
            _ => false
        };
    }
}
