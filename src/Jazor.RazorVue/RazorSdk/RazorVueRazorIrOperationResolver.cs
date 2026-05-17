using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorIrOperationResolver
{
    internal readonly record struct SourceRange(
        string FilePath,
        int Start,
        int End)
    {
        public int Length => End - Start;
    }

    internal readonly record struct ResolvedConditional(
        IConditionalOperation Operation,
        SourceRange StatementRange,
        SourceRange WhenTrueRange,
        SourceRange? WhenFalseRange);

    internal readonly record struct ResolvedForEach(
        IForEachLoopOperation Operation,
        SourceRange StatementRange,
        SourceRange BodyRange);

    internal readonly record struct ResolvedFor(
        IForLoopOperation Operation,
        SourceRange StatementRange,
        SourceRange BodyRange);

    private readonly RazorVueSemanticSnapshot _snapshot;
    private readonly Compilation _compilation;
    private readonly SyntaxTree _generatedTree;
    private readonly SyntaxNode _generatedRoot;
    private readonly SemanticModel _semanticModel;
    private readonly SourceText _generatedText;
    private readonly ImmutableArray<RazorVueRazorSourceMapping> _sourceMappings;

    public RazorVueRazorIrOperationResolver(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorSourceGeneratorDocument document)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        _snapshot = snapshot;
        _compilation = context.Compilation;
        _generatedTree = GetGeneratedRazorSyntaxTree(context, snapshot, document);
        _generatedRoot = _generatedTree.GetRoot();
        _semanticModel = _compilation.ContainsSyntaxTree(_generatedTree)
            ? _compilation.GetSemanticModel(_generatedTree)
            : _compilation.AddSyntaxTrees(_generatedTree).GetSemanticModel(_generatedTree);
        _generatedText = _generatedTree.GetText();
        _sourceMappings = document.SourceMappings;
    }

    public IOperation ResolveRequiredOperation(RazorVueRazorSourceSpan? sourceSpan, string detail)
    {
        if (TryResolveOperation(sourceSpan, out var operation))
            return operation;

        throw CreateUnsupportedMappingException(sourceSpan, detail);
    }

    public bool TryResolveOperation(RazorVueRazorSourceSpan? sourceSpan, out IOperation operation)
    {
        operation = default!;
        if (sourceSpan is null)
            return false;

        if (!TryMapToGeneratedSpan(sourceSpan.Value, out var generatedSpan))
            return false;

        var syntax = FindBestSyntaxNode(generatedSpan);
        if (syntax is null)
            return false;

        operation = GetBestOperation(syntax)!;
        return operation is not null;
    }

    public bool TryResolveGeneratedExpression(
        string expressionText,
        RazorVueRazorSourceSpan? sourceSpan,
        out IOperation operation)
    {
        operation = default!;
        if (string.IsNullOrWhiteSpace(expressionText))
            return false;

        var normalizedTarget = NormalizeComparableCode(expressionText);
        if (normalizedTarget.Length == 0)
            return false;

        TextSpan? preferredSpan = null;
        if (sourceSpan is not null &&
            TryMapToGeneratedSpan(sourceSpan.Value, out var generatedSpan))
        {
            preferredSpan = generatedSpan;
        }

        var candidates = _generatedRoot.DescendantNodes()
            .OfType<ExpressionSyntax>()
            .Where(candidate =>
                string.Equals(
                    NormalizeComparableCode(candidate.ToString()),
                    normalizedTarget,
                    StringComparison.Ordinal))
            .Select(candidate => new
            {
                Candidate = candidate,
                Operation = GetBestOperation(candidate),
                Score = ScoreGeneratedExpressionCandidate(candidate, preferredSpan)
            })
            .Where(item => item.Operation is not null)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Candidate.Span.Length)
            .ThenBy(item => item.Candidate.SpanStart)
            .ToArray();

        foreach (var candidate in candidates)
        {
            operation = candidate.Operation!;
            return true;
        }

        return false;
    }

    public bool TryResolveRewrittenSourceExpression(
        string expressionText,
        RazorVueRazorSourceSpan? sourceSpan,
        out IOperation operation)
    {
        operation = default!;
        if (sourceSpan is null || string.IsNullOrWhiteSpace(expressionText))
            return false;

        if (!TryMapToGeneratedSpan(sourceSpan.Value, out var generatedSpan))
            return false;

        ExpressionSyntax? replacementTarget = null;
        if (TryResolveOperation(sourceSpan, out var mappedOperation))
        {
            replacementTarget = mappedOperation.Syntax as ExpressionSyntax
                ?? mappedOperation.Syntax.AncestorsAndSelf().OfType<ExpressionSyntax>().FirstOrDefault();
        }

        replacementTarget ??= FindBestSyntaxNode(generatedSpan)?
            .AncestorsAndSelf()
            .OfType<ExpressionSyntax>()
            .FirstOrDefault();
        if (replacementTarget is null)
            return false;

        var updatedSource = _generatedText.ToString().Remove(replacementTarget.Span.Start, replacementTarget.Span.Length)
            .Insert(replacementTarget.Span.Start, expressionText);
        var updatedTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
            updatedSource,
            options: _generatedTree.Options as Microsoft.CodeAnalysis.CSharp.CSharpParseOptions,
            path: _generatedTree.FilePath,
            encoding: _generatedText.Encoding);
        var updatedCompilation = _compilation.ContainsSyntaxTree(_generatedTree)
            ? _compilation.ReplaceSyntaxTree(_generatedTree, updatedTree)
            : _compilation.AddSyntaxTrees(updatedTree);
        var updatedModel = updatedCompilation.GetSemanticModel(updatedTree);
        var updatedRoot = updatedTree.GetRoot();
        var replacementSpan = new TextSpan(replacementTarget.Span.Start, expressionText.Length);

        operation = updatedRoot.DescendantNodes()
            .OfType<ExpressionSyntax>()
            .Where(candidate =>
                string.Equals(
                    NormalizeComparableCode(candidate.ToString()),
                    NormalizeComparableCode(expressionText),
                    StringComparison.Ordinal))
            .Where(candidate =>
                Contains(candidate.FullSpan, replacementSpan) ||
                Contains(candidate.Span, replacementSpan) ||
                Contains(replacementSpan, candidate.FullSpan) ||
                Contains(replacementSpan, candidate.Span) ||
                Overlaps(candidate.FullSpan, replacementSpan) ||
                Overlaps(candidate.Span, replacementSpan))
            .OrderBy(static candidate => candidate.Span.Length)
            .ThenBy(candidate => Math.Abs(candidate.SpanStart - replacementSpan.Start))
            .Select(candidate => GetBestOperation(updatedModel, candidate))
            .FirstOrDefault(static candidate => candidate is not null)!;

        return operation is not null;
    }

    public bool TryResolveRewrittenBuilderAttributeValue(
        string methodName,
        string attributeName,
        int ordinal,
        string expressionText,
        out IOperation operation)
    {
        operation = default!;
        if (string.IsNullOrWhiteSpace(methodName) ||
            string.IsNullOrWhiteSpace(attributeName) ||
            ordinal < 0 ||
            string.IsNullOrWhiteSpace(expressionText))
        {
            return false;
        }

        var candidates = _generatedRoot.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(candidate => string.Equals(GetInvocationMethodName(candidate), methodName, StringComparison.Ordinal))
            .Where(candidate => candidate.ArgumentList.Arguments.Count >= 3)
            .Where(candidate =>
            {
                var attributeArgument = candidate.ArgumentList.Arguments[1].Expression;
                return attributeArgument is LiteralExpressionSyntax literal &&
                       literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression) &&
                       string.Equals(literal.Token.ValueText, attributeName, StringComparison.Ordinal);
            })
            .OrderBy(static candidate => candidate.SpanStart)
            .ToArray();
        if (ordinal >= candidates.Length)
            return false;

        var replacementTarget = candidates[ordinal].ArgumentList.Arguments[2].Expression;
        return TryResolveOperationFromRewrittenExpression(expressionText, replacementTarget, out operation);
    }

    public bool TryResolveConditional(RazorVueRazorSourceSpan? sourceSpan, out ResolvedConditional conditional)
    {
        conditional = default;
        if (!TryResolveBestIfStatement(sourceSpan, out var syntax))
            return false;

        if (syntax is null || GetBestOperation(syntax) is not IConditionalOperation operation)
            return false;

        if (!TryMapGeneratedSpanToSourceRange(syntax.Span, out var statementRange) ||
            !TryMapGeneratedSpanToSourceRange(syntax.Statement.Span, out var whenTrueRange))
        {
            return false;
        }

        SourceRange? whenFalseRange = null;
        if (syntax.Else is not null)
        {
            if (!TryMapGeneratedSpanToSourceRange(syntax.Else.Statement.Span, out var resolvedWhenFalseRange))
                return false;

            whenFalseRange = resolvedWhenFalseRange;
        }

        conditional = new ResolvedConditional(operation, statementRange, whenTrueRange, whenFalseRange);
        return true;
    }

    public bool TryResolveForEach(RazorVueRazorSourceSpan? sourceSpan, out ResolvedForEach loop)
    {
        loop = default;
        if (!TryResolveStatement(sourceSpan, out ForEachStatementSyntax? syntax))
            return false;

        if (syntax is null || GetBestOperation(syntax) is not IForEachLoopOperation operation)
            return false;

        if (!TryMapGeneratedSpanToSourceRange(syntax.Span, out var statementRange) ||
            !TryMapGeneratedSpanToSourceRange(syntax.Statement.Span, out var bodyRange))
        {
            return false;
        }

        loop = new ResolvedForEach(operation, statementRange, bodyRange);
        return true;
    }

    public bool TryResolveFor(RazorVueRazorSourceSpan? sourceSpan, out ResolvedFor loop)
    {
        loop = default;
        if (!TryResolveStatement(sourceSpan, out ForStatementSyntax? syntax))
            return false;

        if (syntax is null || GetBestOperation(syntax) is not IForLoopOperation operation)
            return false;

        if (!TryMapGeneratedSpanToSourceRange(syntax.Span, out var statementRange) ||
            !TryMapGeneratedSpanToSourceRange(syntax.Statement.Span, out var bodyRange))
        {
            return false;
        }

        loop = new ResolvedFor(operation, statementRange, bodyRange);
        return true;
    }

    private static SyntaxTree GetGeneratedRazorSyntaxTree(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorSourceGeneratorDocument document)
    {
        if (snapshot.BuildRenderTreeMethod is not null)
        {
            foreach (var syntaxReference in snapshot.BuildRenderTreeMethod.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is not MethodDeclarationSyntax methodDeclaration)
                    continue;

                var syntaxTree = methodDeclaration.SyntaxTree;
                var compiledText = syntaxTree.GetText();
                if (!compiledText.ContentEquals(document.CSharpText))
                {
                    throw new InvalidOperationException(
                        $"The compiled Razor generated source for component '{snapshot.Descriptor.FullName}' diverged from RazorCodeDocument C# output. " +
                        "RazorVue IR expression resolution requires SDK-aligned generated source.");
                }

                return syntaxTree;
            }
        }

        var parseOptions = context.Compilation.SyntaxTrees
            .Select(static tree => tree.Options)
            .OfType<Microsoft.CodeAnalysis.CSharp.CSharpParseOptions>()
            .FirstOrDefault();
        return Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
            document.CSharpText,
            options: parseOptions,
            path: string.IsNullOrWhiteSpace(document.HintName)
                ? snapshot.Descriptor.Name + ".razor.g.cs"
                : document.HintName);
    }

    private static MethodDeclarationSyntax GetBuildRenderTreeSyntax(RazorVueSemanticSnapshot snapshot)
    {
        if (snapshot.BuildRenderTreeMethod is null)
        {
            throw new InvalidOperationException(
                $"RazorVue Razor IR expression resolution requires BuildRenderTree to be present for component '{snapshot.Descriptor.FullName}'.");
        }

        foreach (var syntaxReference in snapshot.BuildRenderTreeMethod.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is MethodDeclarationSyntax methodDeclaration)
                return methodDeclaration;
        }

        throw new InvalidOperationException(
            $"BuildRenderTree syntax could not be located for component '{snapshot.Descriptor.FullName}'.");
    }

    private bool TryMapToGeneratedSpan(RazorVueRazorSourceSpan sourceSpan, out TextSpan generatedSpan)
    {
        generatedSpan = default;
        if (string.IsNullOrWhiteSpace(sourceSpan.FilePath) || _sourceMappings.IsDefaultOrEmpty)
            return false;

        var sourceStart = sourceSpan.AbsoluteIndex;
        var sourceEnd = sourceSpan.AbsoluteIndex + sourceSpan.Length;
        var mappings = _sourceMappings
            .Where(mapping => PathsEqual(mapping.OriginalSpan.FilePath, sourceSpan.FilePath))
            .Where(mapping => Overlaps(mapping.OriginalSpan.AbsoluteIndex, mapping.OriginalSpan.Length, sourceStart, sourceSpan.Length) ||
                              Contains(mapping.OriginalSpan.AbsoluteIndex, mapping.OriginalSpan.Length, sourceStart, sourceSpan.Length) ||
                              Contains(sourceStart, sourceSpan.Length, mapping.OriginalSpan.AbsoluteIndex, mapping.OriginalSpan.Length))
            .ToArray();
        if (mappings.Length == 0)
            return false;

        var generatedStart = mappings.Min(static mapping => mapping.GeneratedSpan.AbsoluteIndex);
        var generatedEnd = mappings.Max(static mapping => mapping.GeneratedSpan.AbsoluteIndex + mapping.GeneratedSpan.Length);
        if (generatedStart < 0 || generatedEnd <= generatedStart || generatedEnd > _generatedText.Length)
            return false;

        if (sourceSpan.Length > 0)
        {
            var mappedStart = mappings.Min(static mapping => mapping.OriginalSpan.AbsoluteIndex);
            var mappedEnd = mappings.Max(static mapping => mapping.OriginalSpan.AbsoluteIndex + mapping.OriginalSpan.Length);
            if (mappedStart > sourceStart || mappedEnd < sourceEnd)
                return false;
        }

        generatedSpan = TextSpan.FromBounds(generatedStart, generatedEnd);
        return true;
    }

    private bool TryMapGeneratedSpanToSourceRange(TextSpan generatedSpan, out SourceRange sourceRange)
    {
        sourceRange = default;
        if (_sourceMappings.IsDefaultOrEmpty)
            return false;

        var mappings = _sourceMappings
            .Where(mapping => Overlaps(mapping.GeneratedSpan.AbsoluteIndex, mapping.GeneratedSpan.Length, generatedSpan.Start, generatedSpan.Length) ||
                              Contains(mapping.GeneratedSpan.AbsoluteIndex, mapping.GeneratedSpan.Length, generatedSpan.Start, generatedSpan.Length) ||
                              Contains(generatedSpan.Start, generatedSpan.Length, mapping.GeneratedSpan.AbsoluteIndex, mapping.GeneratedSpan.Length))
            .ToArray();
        if (mappings.Length == 0)
            return false;

        var originalFilePath = mappings
            .Select(static mapping => mapping.OriginalSpan.FilePath)
            .FirstOrDefault(static path => !string.IsNullOrWhiteSpace(path));
        if (string.IsNullOrWhiteSpace(originalFilePath))
            return false;

        if (mappings.Any(mapping => !PathsEqual(mapping.OriginalSpan.FilePath, originalFilePath)))
            return false;

        var originalStart = mappings.Min(static mapping => mapping.OriginalSpan.AbsoluteIndex);
        var originalEnd = mappings.Max(static mapping => mapping.OriginalSpan.AbsoluteIndex + mapping.OriginalSpan.Length);
        if (originalStart < 0 || originalEnd <= originalStart)
            return false;

        sourceRange = new SourceRange(
            NormalizeComparablePath(originalFilePath),
            originalStart,
            originalEnd);
        return true;
    }

    private SyntaxNode? FindBestSyntaxNode(TextSpan generatedSpan)
    {
        if (_generatedText.Length == 0)
            return null;

        var start = generatedSpan.Start >= _generatedText.Length
            ? _generatedText.Length - 1
            : generatedSpan.Start;
        if (start < 0)
            return null;

        var token = _generatedRoot.FindToken(start);
        var current = token.Parent;
        while (current is not null && !Contains(current.FullSpan, generatedSpan))
            current = current.Parent;

        if (current is null)
            return null;

        while (true)
        {
            var next = current.ChildNodes().FirstOrDefault(child => Contains(child.FullSpan, generatedSpan));
            if (next is null)
                return current;

            current = next;
        }
    }

    private IOperation? GetBestOperation(SemanticModel semanticModel, SyntaxNode syntax)
    {
        for (var current = syntax; current is not null; current = current.Parent)
        {
            var operation = semanticModel.GetOperation(current);
            if (operation is IArgumentOperation argumentOperation)
                operation = argumentOperation.Value;

            operation = Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(operation);
            if (operation is not null)
                return operation;
        }

        return null;
    }

    private IOperation? GetBestOperation(SyntaxNode syntax)
        => GetBestOperation(_semanticModel, syntax);

    private bool TryResolveOperationFromRewrittenExpression(
        string expressionText,
        ExpressionSyntax replacementTarget,
        out IOperation operation)
    {
        operation = default!;
        var updatedSource = _generatedText.ToString().Remove(replacementTarget.Span.Start, replacementTarget.Span.Length)
            .Insert(replacementTarget.Span.Start, expressionText);
        var updatedTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
            updatedSource,
            options: _generatedTree.Options as Microsoft.CodeAnalysis.CSharp.CSharpParseOptions,
            path: _generatedTree.FilePath,
            encoding: _generatedText.Encoding);
        var updatedCompilation = _compilation.ContainsSyntaxTree(_generatedTree)
            ? _compilation.ReplaceSyntaxTree(_generatedTree, updatedTree)
            : _compilation.AddSyntaxTrees(updatedTree);
        var updatedModel = updatedCompilation.GetSemanticModel(updatedTree);
        var updatedRoot = updatedTree.GetRoot();
        var replacementSpan = new TextSpan(replacementTarget.Span.Start, expressionText.Length);
        var updatedExpression = updatedRoot.DescendantNodes()
            .OfType<ExpressionSyntax>()
            .Where(candidate =>
                string.Equals(
                    NormalizeComparableCode(candidate.ToString()),
                    NormalizeComparableCode(expressionText),
                    StringComparison.Ordinal))
            .Where(candidate =>
                Contains(candidate.FullSpan, replacementSpan) ||
                Contains(candidate.Span, replacementSpan) ||
                Contains(replacementSpan, candidate.FullSpan) ||
                Contains(replacementSpan, candidate.Span) ||
                Overlaps(candidate.FullSpan, replacementSpan) ||
                Overlaps(candidate.Span, replacementSpan))
            .OrderBy(static candidate => candidate.Span.Length)
            .ThenBy(candidate => Math.Abs(candidate.SpanStart - replacementSpan.Start))
            .FirstOrDefault();
        if (updatedExpression is null)
            return false;

        var resolvedOperation = GetBestOperation(updatedModel, updatedExpression);
        if (resolvedOperation is null)
            return false;

        operation = resolvedOperation;
        return true;
    }

    private static string? GetInvocationMethodName(InvocationExpressionSyntax invocation)
        => invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            GenericNameSyntax genericName => genericName.Identifier.ValueText,
            _ => null
        };

    private static int ScoreGeneratedExpressionCandidate(ExpressionSyntax candidate, TextSpan? preferredSpan)
    {
        if (preferredSpan is null)
            return 0;

        var span = preferredSpan.Value;
        if (Contains(candidate.FullSpan, span) || Contains(candidate.Span, span))
            return 4;

        if (Contains(span, candidate.FullSpan) || Contains(span, candidate.Span))
            return 3;

        if (Overlaps(candidate.FullSpan, span) || Overlaps(candidate.Span, span))
            return 2;

        return 0;
    }

    private bool TryResolveStatement<TStatementSyntax>(RazorVueRazorSourceSpan? sourceSpan, out TStatementSyntax? syntax)
        where TStatementSyntax : SyntaxNode
    {
        syntax = null;
        if (sourceSpan is null)
            return false;

        if (!TryMapToGeneratedSpan(sourceSpan.Value, out var generatedSpan))
            return false;

        var node = FindBestSyntaxNode(generatedSpan);
        if (node is null)
            return false;

        syntax = node.AncestorsAndSelf().OfType<TStatementSyntax>().FirstOrDefault();
        return syntax is not null;
    }

    private bool TryResolveBestIfStatement(RazorVueRazorSourceSpan? sourceSpan, out IfStatementSyntax? syntax)
    {
        syntax = null;
        if (sourceSpan is null)
            return false;

        if (!TryMapToGeneratedSpan(sourceSpan.Value, out var generatedSpan))
            return false;

        var candidates = _generatedRoot
            .DescendantNodes()
            .OfType<IfStatementSyntax>()
            .Where(candidate => Contains(candidate.FullSpan, generatedSpan) ||
                                Contains(candidate.Span, generatedSpan) ||
                                Overlaps(candidate.FullSpan, generatedSpan) ||
                                Overlaps(candidate.Span, generatedSpan))
            .OrderBy(static candidate => candidate.Span.Length)
            .ThenBy(candidate => Math.Abs(candidate.IfKeyword.SpanStart - generatedSpan.Start))
            .ToArray();
        if (candidates.Length == 0)
            return false;

        syntax = candidates[0];
        return true;
    }

    private RazorVueCompilationIssueException CreateUnsupportedMappingException(RazorVueRazorSourceSpan? sourceSpan, string detail)
    {
        var issue = new RazorVueCompilationIssue(
            RazorVueIssueCode.CanonicalizationFailed,
            RazorVueIssueSeverity.Error,
            $"RazorVue Razor IR frontend could not map {detail} back to a Roslyn operation in component '{_snapshot.Descriptor.FullName}'.",
            ImmutableArray<string>.Empty);
        return new RazorVueCompilationIssueException(
            issue,
            _snapshot.Descriptor.FullName,
            RazorVueRazorIrTemplateFrontend.CreateSourceOrigin(sourceSpan, RazorVueOriginKind.Template));
    }

    private static bool Overlaps(int leftStart, int leftLength, int rightStart, int rightLength)
    {
        var leftEnd = leftStart + leftLength;
        var rightEnd = rightStart + rightLength;
        return leftStart < rightEnd && rightStart < leftEnd;
    }

    private static bool Contains(int outerStart, int outerLength, int innerStart, int innerLength)
    {
        var outerEnd = outerStart + outerLength;
        var innerEnd = innerStart + innerLength;
        return outerStart <= innerStart && outerEnd >= innerEnd;
    }

    private static bool Contains(TextSpan outer, TextSpan inner)
        => outer.Start <= inner.Start && outer.End >= inner.End;

    private static bool Overlaps(TextSpan left, TextSpan right)
        => left.Start < right.End && right.Start < left.End;

    private static bool PathsEqual(string? left, string? right)
        => PathComparer.Equals(NormalizeComparablePath(left), NormalizeComparablePath(right));

    private static string NormalizeComparablePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        try
        {
            return Path.GetFullPath(path).Replace('\\', '/');
        }
        catch (ArgumentException)
        {
            return path!.Replace('\\', '/');
        }
        catch (PathTooLongException)
        {
            return path!.Replace('\\', '/');
        }
        catch (NotSupportedException)
        {
            return path!.Replace('\\', '/');
        }
        catch (IOException)
        {
            return path!.Replace('\\', '/');
        }
    }

    private static string NormalizeComparableCode(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return new string(text.Where(static character => !char.IsWhiteSpace(character)).ToArray());
    }

    private static StringComparer PathComparer
        => Path.DirectorySeparatorChar == '\\'
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
