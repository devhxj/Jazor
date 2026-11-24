using Jazor.Npm;
using System.Text.Json;
using Jolt.Compiler;
using Jolt.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Jolt.LanguageServer;

internal sealed partial class LanguageFeatures
{
    internal async Task<object[]> FormatAsync(TextSpan requested, JsonElement options, CancellationToken cancellationToken)
    {
        using var roslyn = new AdhocWorkspace(Host.Value);
        var document = await CreateRoslynDocumentAsync(roslyn, cancellationToken);
        var formattedOptions = roslyn.Options
            .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, !options.GetProperty("insertSpaces").GetBoolean())
            .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, Math.Max(1, options.GetProperty("tabSize").GetInt32()))
            .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, Math.Max(1, options.GetProperty("tabSize").GetInt32()))
            .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, NewLine());
        var mapped = projection.Spans.Where(span => span.Source.Length > 0 && requested.Contains(span.Source))
            .ToArray();
        var edits = new List<TextChange>();
        if (mapped.Length > 0)
        {
            var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
            var formatted = Formatter.GetFormattedTextChanges(root, mapped.Select(span => span.Generated), roslyn, formattedOptions, cancellationToken);
            foreach (var change in formatted)
            {
                // A boundary insertion belongs to the generated wrapper. Keep it
                // outside the edit set, even when its position touches author code.
                if (mapped.Any(span => span.Generated.Start < change.Span.Start && change.Span.End < span.Generated.End) &&
                    ToSource(change.Span) is { } source)
                    edits.Add(new(source, change.NewText ?? ""));
            }
        }
        var indent = options.GetProperty("insertSpaces").GetBoolean()
            ? new string(' ', Math.Max(1, options.GetProperty("tabSize").GetInt32()))
            : "\t";
        foreach (var element in JoltTree.Elements(projection.Document))
        {
            var tagLine = _text.Lines.GetLineFromPosition(element.Span.Start);
            var baseIndent = _text.ToString(TextSpan.FromBounds(tagLine.Start, element.Span.Start));
            if (!string.IsNullOrWhiteSpace(baseIndent))
                continue;
            foreach (var attribute in element.Attributes)
            {
                var line = _text.Lines.GetLineFromPosition(attribute.NameSpan.Start);
                var leading = TextSpan.FromBounds(line.Start, attribute.NameSpan.Start);
                if (line.LineNumber != tagLine.LineNumber && requested.Contains(leading) &&
                    string.IsNullOrWhiteSpace(_text.ToString(leading)) && _text.ToString(leading) != baseIndent + indent)
                    edits.Add(new(leading, baseIndent + indent));
            }
        }
        return edits.OrderBy(edit => edit.Span.Start).Select(ProtocolEdit).ToArray();
    }

    internal object[] CodeActions(JsonElement args, TextSpan requested, int? version, CancellationToken cancellationToken)
    {
        if (args.TryGetProperty("only", out var only) && !only.EnumerateArray().Any(kind => kind.GetString() == "quickfix"))
            return [];
        var actions = new List<object>();
        foreach (var diagnostic in args.GetProperty("diagnostics").EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!diagnostic.TryGetProperty("code", out var code) || code.ValueKind != JsonValueKind.String)
                continue;
            var start = Server.Offset(_text, diagnostic.GetProperty("range").GetProperty("start"));
            if (JoltTree.Elements(projection.Document).FirstOrDefault(element => element.NameSpan.Start == start) is not { } site ||
                !requested.IntersectsWith(site.NameSpan))
                continue;
            if (!analysis.Diagnostics.Any(item => item.Id == code.GetString() && item.Location.GetMappedLineSpan().Path == projection.Document.Path &&
                item.Location.SourceSpan == site.NameSpan))
                continue;
            if (code.GetString() == "JOLT1001")
            {
                var visible = projection.LocalComponents.Select(component => component.Name).Concat(
                    workspace.Options.NpmComponents?.Where(component => UsesPackage(component.ModuleSpecifier))
                        .Select(component => NpmNames.TagFor(component.ModuleSpecifier, component.ExportName)) ?? []);
                foreach (var candidate in ImportCandidates(site.Name, visible).Where(candidate => candidate.Name == site.Name))
                    Add("Add @using " + candidate.Namespace, ImportEdit(site.NameSpan, candidate.Namespace), diagnostic);
            }
            if (code.GetString() == "JOLT1005")
            {
                foreach (var fix in RequiredProperties(site, diagnostic.GetProperty("message").GetString(), cancellationToken))
                    Add(fix.Title, fix.Change, diagnostic);
            }
        }
        return actions.ToArray();

        void Add(string title, TextChange edit, JsonElement diagnostic) => actions.Add(new
        {
            title, kind = "quickfix", diagnostics = new[] { diagnostic }, edit = new
            {
                documentChanges = new[]
                {
                    new { textDocument = new { uri = new Uri(Path.GetFullPath(projection.Document.Path, projectDirectory)).AbsoluteUri, version },
                        edits = new[] { ProtocolEdit(edit) } },
                },
            },
        });
    }

    private IEnumerable<(string Title, TextChange Change)> RequiredProperties(JoltElement site, string? message, CancellationToken cancellationToken)
    {
        var target = LocalComponent(site.Name);
        if (target is null || MarkupAt(site.NameSpan.Start)?.Symbol.ComponentPath != target.Path)
            yield break;
        var model = projection.SemanticModel;
        var render = model.SyntaxTree.GetRoot(cancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.ValueText == "__JoltRender" &&
                method.Parent is ClassDeclarationSyntax type && type.Identifier.ValueText.StartsWith("__State", StringComparison.Ordinal));
        if (render?.Body is null)
            yield break;
        var consumer = (IMethodSymbol)model.GetDeclaredSymbol(render, cancellationToken)!;
        foreach (var property in target.Properties.Where(property => property.Required && property.Source.HasValue &&
            message == $"Component '{site.Name}' requires prop '{property.Name}'."))
        {
            var selected = GeneratedSite(site.NameSpan.Start, new(new(target.Path, JoltMarkupKind.Property, property.Name), site.NameSpan));
            var targetModel = analysis.Documents.Single(document => document.Document.Path == target.Path).SemanticModel;
            if (selected is null || SymbolAt(targetModel, selected.Span.Start, cancellationToken) is not IPropertySymbol parameter)
                continue;
            if (!consumer.ContainingType.GetMembers(property.Name).Any(member =>
                (member is IPropertySymbol { GetMethod: not null } source && SymbolEqualityComparer.IncludeNullability.Equals(source.Type, parameter.Type)) ||
                (member is IFieldSymbol field && SymbolEqualityComparer.IncludeNullability.Equals(field.Type, parameter.Type))))
                continue;
            var change = new TextChange(new TextSpan(site.NameSpan.End, 0), $" {property.Name}=\"@(this.{property.Name})\"");
            yield return ($"Set {property.Name} from this.{property.Name}", change);
        }
    }
}
