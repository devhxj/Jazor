using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jolt.Compiler;

internal sealed partial class Projection
{
    internal IMethodSymbol? SetupMethod { get; private set; }

    internal void CheckSetup(SemanticModel model, INamedTypeSymbol state, List<Diagnostic> validationDiagnostics, bool prepareLowering)
    {
        IMethodSymbol? selected = null;
        var setupAttribute = model.Compilation.GetTypeByMetadataName("JoltAuthoring.SetupAttribute");
        var root = model.SyntaxTree.GetRoot();
        foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
        {
            if (!SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(attribute).Symbol?.ContainingType, setupAttribute))
                continue;

            if (attribute.Parent?.Parent is not MethodDeclarationSyntax declaration ||
                model.GetDeclaredSymbol(declaration) is not IMethodSymbol method ||
                !SymbolEqualityComparer.Default.Equals(method.ContainingType, state) ||
                method is not
                {
                    DeclaredAccessibility: Accessibility.Private, IsStatic: false, IsAsync: false,
                    IsGenericMethod: false, ReturnsVoid: true, Parameters.Length: 0
                } ||
                declaration is { Body: null, ExpressionBody: null })
            {
                ReportValidation(validationDiagnostics, "JOLT1038", Map(attribute.Span) ?? default, "[Setup] requires a synchronous private instance method with a body, no parameters or type parameters, and a void return type.");
                continue;
            }

            if (selected is null)
                selected = method;
            else
                ReportValidation(validationDiagnostics, "JOLT1038", Map(attribute.Span) ?? default, "A component can declare only one [Setup] method.");
        }
        if (prepareLowering)
            SetupMethod = selected;
    }
}
