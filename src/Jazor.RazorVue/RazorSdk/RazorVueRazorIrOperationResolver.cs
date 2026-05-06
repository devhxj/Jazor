using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Descriptor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorSdk;

internal sealed class RazorVueRazorIrOperationResolver
{
    private readonly RazorVueSemanticSnapshot _snapshot;
    private readonly SyntaxNode _generatedRoot;
    private readonly SemanticModel _semanticModel;
    private readonly SourceText _generatedText;
    private readonly ImmutableArray<SourceMapping> _sourceMappings;

    public RazorVueRazorIrOperationResolver(
        Jazor.RazorVue.RazorVueCompilationContext context,
        RazorVueSemanticSnapshot snapshot,
        RazorVueRazorCodeDocumentHandle handle)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (snapshot is null)
            throw new ArgumentNullException(nameof(snapshot));
        if (handle is null)
            throw new ArgumentNullException(nameof(handle));

        _snapshot = snapshot;
        var buildRenderTreeSyntax = GetBuildRenderTreeSyntax(snapshot);
        _generatedRoot = buildRenderTreeSyntax.SyntaxTree.GetRoot();
        _semanticModel = context.Compilation.GetSemanticModel(buildRenderTreeSyntax.SyntaxTree);
        _generatedText = buildRenderTreeSyntax.SyntaxTree.GetText();
        _sourceMappings = handle.SourceMappings;

        if (!_generatedText.ContentEquals(handle.CSharpDocument.Text))
        {
            throw new InvalidOperationException(
                $"The compiled Razor generated source for component '{snapshot.Descriptor.FullName}' diverged from RazorCodeDocument C# output. " +
                "RazorVue IR expression resolution currently requires SDK-aligned generated source.");
        }
    }

    public IOperation ResolveRequiredOperation(SourceSpan? sourceSpan, string detail)
    {
        if (TryResolveOperation(sourceSpan, out var operation))
            return operation;

        throw CreateUnsupportedMappingException(sourceSpan, detail);
    }

    public bool TryResolveOperation(SourceSpan? sourceSpan, out IOperation operation)
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

    private bool TryMapToGeneratedSpan(SourceSpan sourceSpan, out TextSpan generatedSpan)
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

    private IOperation? GetBestOperation(SyntaxNode syntax)
    {
        for (var current = syntax; current is not null; current = current.Parent)
        {
            var operation = _semanticModel.GetOperation(current);
            if (operation is IArgumentOperation argumentOperation)
                operation = argumentOperation.Value;

            operation = Jazor.RazorVue.RazorVueOperationNormalizer.Unwrap(operation);
            if (operation is not null)
                return operation;
        }

        return null;
    }

    private RazorVueCompilationIssueException CreateUnsupportedMappingException(SourceSpan? sourceSpan, string detail)
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

    private static StringComparer PathComparer
        => Path.DirectorySeparatorChar == '\\'
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
