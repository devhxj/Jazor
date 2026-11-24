using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using System.Composition;

namespace Jazor.Npm.Editor;

[ExportCompletionProvider(nameof(NpmCompletionProvider), LanguageNames.CSharp)]
[Shared]
internal sealed partial class NpmCompletionProvider : CompletionProvider
{
    public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options)
        => trigger.Kind == CompletionTriggerKind.Invoke ||
           trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character == '.';

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        var document = context.Document;
        var cancellationToken = context.CancellationToken;
        if (document.SourceCodeKind != SourceCodeKind.Regular ||
            !string.Equals(Path.GetExtension(document.FilePath), ".cs", StringComparison.OrdinalIgnoreCase))
            return;

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var position = context.Position;
        if (root is null || position <= root.FullSpan.Start)
            return;

        var token = root.FindToken(position - 1);
        var access = token.Parent?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().FirstOrDefault(candidate =>
            candidate.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
            (token == candidate.OperatorToken && position == token.Span.End ||
             token == candidate.Name.Identifier && !token.IsMissing && position <= token.Span.End));
        var qualified = token.Parent?.AncestorsAndSelf().OfType<QualifiedNameSyntax>().FirstOrDefault(candidate =>
            (token == candidate.DotToken && position == token.Span.End ||
             token == candidate.Right.Identifier && !token.IsMissing && position <= token.Span.End) &&
             candidate.Ancestors().OfType<UsingDirectiveSyntax>().Any());
        if (access is null && qualified is null)
            return;

        var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (model?.Compilation is not CSharpCompilation compilation)
            return;

        if (qualified is not null)
            await AddPackageCompletionsAsync(context, model, qualified.Left).ConfigureAwait(false);
        if (access is null)
            return;

        var npmNamespace = NpmSourceRequests.FindCompletionNamespace(compilation, access.Expression, cancellationToken);
        if (npmNamespace is null)
            return;

        var names = await NpmEditorIndex.ReadAsync(document, npmNamespace, cancellationToken).ConfigureAwait(false);
        // Roslyn owns symbol completion for declarations already in the project.
        var existing = model.GetSymbolInfo(access.Expression, cancellationToken).Symbol as INamedTypeSymbol;
        foreach (var name in names)
            if (existing is null || existing.GetMembers(name).IsEmpty)
                context.AddItem(CompletionItem.Create(name));
    }
}
