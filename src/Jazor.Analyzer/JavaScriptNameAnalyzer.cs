using System.Collections.Immutable;
using System.Threading;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Jazor.Analyzer;

/// <summary>Validates authored names even in binding libraries with no module entry point.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class JavaScriptNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor EmptyName = new(
        "JAZOR009", "Empty JavaScript name", "JavaScript name cannot be empty",
        "Jazor.Compiler", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [EmptyName];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(ctx =>
        {
            if (AnalyzeAttribute(ctx.SemanticModel, (AttributeSyntax)ctx.Node, ctx.CancellationToken) is { } diagnostic)
                ctx.ReportDiagnostic(diagnostic);
        }, SyntaxKind.Attribute);
    }

    /// <summary>Checks a single attribute in a Roslyn or Jolt authoring projection.</summary>
    internal static Diagnostic? AnalyzeAttribute(SemanticModel model, AttributeSyntax attribute, CancellationToken cancellationToken)
    {
        if (attribute.ArgumentList is not { Arguments.Count: > 0 } list)
            return null;
        var argument = list.Arguments[0];
        if (argument.NameEquals is not null)
            return null;

        var constant = model.GetConstantValue(argument.Expression, cancellationToken);
        if (!constant.HasValue || constant.Value is not (null or string))
            return null;

        var value = (string?)constant.Value;
        var emptyName = string.IsNullOrWhiteSpace(value);
        var emptyDescription = value?.Trim() == "@#";
        if (!emptyName && !emptyDescription)
            return null;

        var constructor = model.GetSymbolInfo(attribute, cancellationToken).Symbol as IMethodSymbol;
        switch (constructor?.ContainingType.ToDisplayString())
        {
            case "ECMAScript.ECMAScriptNameAttribute" when emptyName:
                // An explicitly empty string on a string enum field is a wire value.
                if (value == string.Empty && attribute.Parent?.Parent is EnumMemberDeclarationSyntax member &&
                    Util.IsStringEnumType(model.GetDeclaredSymbol(member, cancellationToken)?.ContainingType))
                    return null;
                break;
            case "System.ComponentModel.DescriptionAttribute" when emptyDescription:
                break;
            default:
                return null;
        }

        return Diagnostic.Create(EmptyName, attribute.GetLocation());
    }
}
