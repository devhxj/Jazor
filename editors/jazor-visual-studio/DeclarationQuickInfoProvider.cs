using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Jazor.VisualStudio;

[Export(typeof(IAsyncQuickInfoSourceProvider))]
[Name("Jazor TypeScript declaration navigation")]
[ContentType("CSharp")]
internal sealed class DeclarationQuickInfoProvider : IAsyncQuickInfoSourceProvider
{
    [Import]
    public VisualStudioWorkspace Workspace { get; set; } = null!;

    public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer buffer) => new Source(Workspace, buffer);

    private sealed class Source : IAsyncQuickInfoSource
    {
        private readonly VisualStudioWorkspace _workspace;
        private readonly ITextBuffer _buffer;

        public Source(VisualStudioWorkspace workspace, ITextBuffer buffer)
        {
            _workspace = workspace;
            _buffer = buffer;
        }

        public void Dispose() { }

        public async Task<QuickInfoItem?> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var snapshot = _buffer.CurrentSnapshot;
            var trigger = session.GetTriggerPoint(snapshot);
            if (trigger is null)
                return null;

            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document is null || document.Project.Solution.Workspace != _workspace ||
                !document.Project.AnalyzerReferences.Any(reference =>
                    string.Equals(Path.GetFileName(reference.FullPath), "Jazor.Npm.Editor.dll", StringComparison.OrdinalIgnoreCase)))
                return null;

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null || trigger.Value.Position >= root.FullSpan.End)
                return null;

            var token = root.FindToken(trigger.Value.Position);
            if (!(token.Parent is SimpleNameSyntax name))
                return null;

            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = model?.GetSymbolInfo(name, cancellationToken).Symbol;
            if (symbol is null || symbol.ContainingType?.Name != "Exports" ||
                !symbol.Locations.Any(location => Path.GetFileName(location.SourceTree?.FilePath) == "Jazor.Npm.Editor.g.cs"))
                return null;

            var xml = symbol.GetDocumentationCommentXml(cancellationToken: cancellationToken);
            if (string.IsNullOrEmpty(xml))
                return null;

            var href = XElement.Parse(xml).Descendants("see")
                .FirstOrDefault(link => link.Value == "Open TypeScript declaration")?.Attribute("href")?.Value;
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !uri.IsFile)
                return null;

            var position = uri.Fragment.StartsWith("#L", StringComparison.Ordinal)
                ? uri.Fragment.Substring(2).Split(',') : Array.Empty<string>();
            if (position.Length != 2 || !int.TryParse(position[0], out int line) || !int.TryParse(position[1], out int column) ||
                line < 1 || column < 1 || !File.Exists(uri.LocalPath))
                return null;

            var span = snapshot.CreateTrackingSpan(token.SpanStart, token.Span.Length, SpanTrackingMode.EdgeInclusive);
            var link = new ClassifiedTextRun("text", $"Open {Path.GetFileName(uri.LocalPath)} at line {line}",
                () => NavigateAsync(uri.LocalPath, line, column).Forget(), uri.LocalPath);
            return new QuickInfoItem(span, new ClassifiedTextElement(link));
        }

        private static async Task NavigateAsync(string path, int line, int column)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.OpenDocument(ServiceProvider.GlobalProvider, path, Guid.Empty,
                out _, out _, out var frame, out var view);
            frame?.Show();
            view?.SetCaretPos(line - 1, column - 1);
            view?.CenterLines(line - 1, 1);
        }
    }
}
