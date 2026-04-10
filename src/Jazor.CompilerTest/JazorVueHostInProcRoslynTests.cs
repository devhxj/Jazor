using System.Text.RegularExpressions;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Lsp;
using Jazor.VueHost.Razor.InProc;
using Jazor.VueHost.Roslyn.InProc;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostInProcRoslynTests
{
    private readonly InProcRoslynCodeService _service = new();

    [TestMethod]
    public async Task InProcRoslynCodeService_GetHoverAsync_ReturnsCodeSymbolHover()
    {
        var document = CreateDocument(
            """
            @code {
                private int count;

                private void Increment()
                {
                    count++;
                }
            }
            """);

        var hover = await _service.GetHoverAsync(
            document,
            GetPosition(document.Text, "count++;"),
            CancellationToken.None);

        Assert.IsNotNull(hover);
        StringAssert.Contains(hover.Contents.Value, "count");
        StringAssert.Contains(hover.Contents.Value, "Field");
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetCompletionItemsAsync_ReturnsCodeMembers()
    {
        var document = CreateDocument(
            """
            @code {
                private int count;

                private void Increment()
                {
                }

                private void Trigger()
                {
                    Incre
                }
            }
            """);

        var items = await _service.GetCompletionItemsAsync(
            document,
            GetPosition(document.Text, "Incre", advance: "Incre".Length),
            CancellationToken.None);

        CollectionAssert.Contains(items.Select(static item => item.Label).ToArray(), "Increment");
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetCompletionItemsAsync_WithNormalizedPath_ReturnsCodeMembers()
    {
        var document = CreateDocument(
            "D:/temp/Counter.jazor",
            """
            @page "/counter"

            @code {
                private int count = 1;

                public int Increment()
                {
                    cou
                    return count;
                }
            }
            """);

        var items = await _service.GetCompletionItemsAsync(
            document,
            GetPosition(document.Text, "cou", advance: "cou".Length),
            CancellationToken.None);

        CollectionAssert.Contains(items.Select(static item => item.Label).ToArray(), "count");
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetCompletionAndHoverAsync_WithDiskBackedCSharpDeclaration_UsesWorkspaceSemantics()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "jazor-vuehost-roslyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var consumer = new DocumentSnapshot(
                Path.Combine(tempDirectory, "CounterConsumer.cs"),
                DocumentKind.CSharp,
                """
                namespace Demo;

                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Cou
                    }
                }
                """,
                "1");

            var completionItems = await _service.GetCompletionItemsAsync(
                consumer,
                GetPosition(consumer.Text, "Cou", advance: "Cou".Length),
                CancellationToken.None);
            var hover = await _service.GetHoverAsync(
                new DocumentSnapshot(
                    consumer.DocumentPath,
                    consumer.DocumentKind,
                    consumer.Text.Replace("Cou", "Count"),
                    consumer.Version),
                GetPosition(consumer.Text.Replace("Cou", "Count"), "Count", advance: 1),
                CancellationToken.None);

            CollectionAssert.Contains(completionItems.Select(static item => item.Label).ToArray(), "Count");
            Assert.IsNotNull(hover);
            StringAssert.Contains(hover.Contents.Value, "Count");
            StringAssert.Contains(hover.Contents.Value, "Field");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetSignatureHelpAsync_TracksActiveParameterAcrossInvocationArguments()
    {
        var document = CreateDocument(
            """
            @code {
                private static string FormatValue(int count, string prefix, bool includeUnits)
                    => string.Empty;

                private void Render()
                {
                    FormatValue(1, "draft", true);
                }
            }
            """);

        var firstArgumentHelp = await _service.GetSignatureHelpAsync(
            document,
            GetPosition(document.Text, "FormatValue(1", advance: "FormatValue(".Length),
            CancellationToken.None);
        var secondArgumentHelp = await _service.GetSignatureHelpAsync(
            document,
            GetPosition(document.Text, "\"draft\"", advance: 1),
            CancellationToken.None);
        var thirdArgumentHelp = await _service.GetSignatureHelpAsync(
            document,
            GetPosition(document.Text, "true", advance: 1),
            CancellationToken.None);

        AssertSignatureHelp(firstArgumentHelp, expectedActiveParameter: 0);
        AssertSignatureHelp(secondArgumentHelp, expectedActiveParameter: 1);
        AssertSignatureHelp(thirdArgumentHelp, expectedActiveParameter: 2);
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetCompletionHoverAndSignatureHelpAsync_FromJazorCode_UseUnopenedDiskBackedCSharpDeclaration()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "jazor-vuehost-roslyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var declarationPath = Path.Combine(tempDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;

                    internal static string FormatValue(int count, string prefix, bool includeUnits)
                        => $"{prefix}:{count}:{includeUnits}";
                }
                """);

            var completionDocument = CreateDocument(
                Path.Combine(tempDirectory, "Counter.jazor"),
                """
                @using Demo

                @code {
                    private string Render()
                    {
                        return SharedState.Cou
                    }
                }
                """);

            var completionItems = await _service.GetCompletionItemsAsync(
                completionDocument,
                GetPosition(completionDocument.Text, "Cou", advance: "Cou".Length),
                CancellationToken.None);

            CollectionAssert.Contains(completionItems.Select(static item => item.Label).ToArray(), "Count");

            var hoverDocument = CreateDocument(
                Path.Combine(tempDirectory, "Counter.jazor"),
                """
                @using Demo

                @code {
                    private int Render()
                    {
                        return SharedState.Count;
                    }
                }
                """);

            var hover = await _service.GetHoverAsync(
                hoverDocument,
                GetPosition(hoverDocument.Text, "Count", advance: 1),
                CancellationToken.None);

            Assert.IsNotNull(hover);
            StringAssert.Contains(hover.Contents.Value, "Count");
            StringAssert.Contains(hover.Contents.Value, "Field");

            var signatureDocument = CreateDocument(
                Path.Combine(tempDirectory, "Counter.jazor"),
                """
                @using Demo

                @code {
                    private string Render()
                    {
                        return SharedState.FormatValue(1, "draft", true);
                    }
                }
                """);

            var firstArgumentHelp = await _service.GetSignatureHelpAsync(
                signatureDocument,
                GetPosition(signatureDocument.Text, "FormatValue(1", advance: "FormatValue(".Length),
                CancellationToken.None);
            var secondArgumentHelp = await _service.GetSignatureHelpAsync(
                signatureDocument,
                GetPosition(signatureDocument.Text, "\"draft\"", advance: 1),
                CancellationToken.None);
            var thirdArgumentHelp = await _service.GetSignatureHelpAsync(
                signatureDocument,
                GetPosition(signatureDocument.Text, "true", advance: 1),
                CancellationToken.None);

            AssertSignatureHelp(firstArgumentHelp, expectedActiveParameter: 0);
            AssertSignatureHelp(secondArgumentHelp, expectedActiveParameter: 1);
            AssertSignatureHelp(thirdArgumentHelp, expectedActiveParameter: 2);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDocumentSymbolsAsync_ReturnsTopLevelCodeMembersInSourceOrder()
    {
        var document = CreateDocument(
            """
            @code {
                private int count;
                private int Total => count;

                private void Increment()
                {
                    void Local() { }
                    count++;
                }
            }
            """);

        var symbols = await _service.GetDocumentSymbolsAsync(document, CancellationToken.None);

        CollectionAssert.AreEqual(
            new[] { "count", "Total", "Increment" },
            symbols.Select(static symbol => symbol.Name).ToArray());
        CollectionAssert.AreEqual(
            new[] { 8, 7, 6 },
            symbols.Select(static symbol => symbol.Kind).ToArray());
        CollectionAssert.AreEqual(
            new[] { 1, 2, 4 },
            symbols.Select(static symbol => symbol.SelectionRange.Start.Line).ToArray());
        CollectionAssert.DoesNotContain(
            symbols.Select(static symbol => symbol.Name).ToArray(),
            "Local");
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetSemanticTokensAsync_MapsCodeTokensBackToOriginalDocument()
    {
        var document = CreateDocument(
            """
            @code {
                private static readonly int count = 42;
                private string label = "items";

                private void Increment(int step)
                {
                    var next = count + step;
                }
            }
            """);

        var tokens = await _service.GetSemanticTokensAsync(document, CancellationToken.None);
        AssertHasSemanticToken(
            tokens,
            GetPosition(document.Text, "count = 42"),
            "count".Length,
            "variable",
            "declaration",
            "static",
            "readonly");
        AssertHasSemanticToken(
            tokens,
            GetPosition(document.Text, "\"items\""),
            "\"items\"".Length,
            "string");
        AssertHasSemanticToken(
            tokens,
            GetPosition(document.Text, "42"),
            "42".Length,
            "number");
        AssertHasSemanticToken(
            tokens,
            GetPosition(document.Text, "Increment(int"),
            "Increment".Length,
            "method",
            "declaration");
        AssertHasSemanticToken(
            tokens,
            GetPosition(document.Text, "step)"),
            "step".Length,
            "parameter",
            "declaration");
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDefinitionAsync_MapsToOriginalCodeRegion()
    {
        var document = CreateDocument(
            """
            @code {
                private void Increment()
                {
                }

                private void Trigger()
                {
                    Increment();
                }
            }
            """);

        var locations = await _service.GetDefinitionAsync(
            document,
            GetPosition(document.Text, "Increment();"),
            CancellationToken.None);

        Assert.AreEqual(1, locations.Count);
        Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(document.DocumentPath), locations[0].Uri);
        Assert.AreEqual(1, locations[0].Range.Start.Line);
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDefinitionAsync_WithCSharpDocument_ReturnsSameFileLocation()
    {
        var document = new DocumentSnapshot(
            "D:/temp/CounterLogic.cs",
            DocumentKind.CSharp,
            """
            internal static class CounterLogic
            {
                private static int count = 1;

                public static int Increment()
                {
                    count++;
                    return count;
                }
            }
            """,
            "1");

        var locations = await _service.GetDefinitionAsync(
            document,
            GetPosition(document.Text, "count++;", advance: 1),
            CancellationToken.None);

        Assert.AreEqual(1, locations.Count);
        Assert.AreEqual("file:///D:/temp/CounterLogic.cs", locations[0].Uri);
        Assert.AreEqual(2, locations[0].Range.Start.Line);
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDefinitionReferencesAndRenameAsync_WithDiskBackedCSharpDocument_IncludesUnopenedFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "JazorVueHostRoslyn_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var declarationPath = Path.Combine(tempDirectory, "CounterLogic.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                internal static class CounterLogic
                {
                    public static int Count = 1;
                }
                """);

            var consumer = new DocumentSnapshot(
                Path.Combine(tempDirectory, "CounterLogicConsumer.cs"),
                DocumentKind.CSharp,
                """
                internal static class CounterLogicConsumer
                {
                    public static int Read()
                    {
                        return CounterLogic.Count;
                    }
                }
                """,
                "1");
            var usagePosition = GetPosition(consumer.Text, "Count", advance: 1);

            var definition = await _service.GetDefinitionAsync(
                consumer,
                usagePosition,
                CancellationToken.None);
            var references = await _service.GetReferencesAsync(
                consumer,
                usagePosition,
                includeDeclaration: true,
                CancellationToken.None);
            var rename = await _service.GetRenameAsync(
                consumer,
                usagePosition,
                "TotalCount",
                CancellationToken.None);

            Assert.AreEqual(1, definition.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(declarationPath), definition[0].Uri);
            Assert.AreEqual(2, definition[0].Range.Start.Line);

            Assert.AreEqual(2, references.Count);
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(declarationPath)));
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(consumer.DocumentPath)));

            Assert.IsNotNull(rename);
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(declarationPath)));
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(consumer.DocumentPath)));
            Assert.IsTrue(rename.Changes.SelectMany(static pair => pair.Value).All(static edit => edit.NewText == "TotalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetReferencesAndRenameAsync_StayInOriginalDocument()
    {
        var document = CreateDocument(
            """
            @code {
                private int count;

                private void Increment()
                {
                    count++;
                }

                private void Trigger()
                {
                    count++;
                }
            }
            """);

        var references = await _service.GetReferencesAsync(
            document,
            GetPosition(document.Text, "count++;"),
            includeDeclaration: true,
            CancellationToken.None);
        var rename = await _service.GetRenameAsync(
            document,
            GetPosition(document.Text, "count++;"),
            "total",
            CancellationToken.None);

        Assert.AreEqual(3, references.Count);
        Assert.IsNotNull(rename);
        Assert.AreEqual(3, rename.Changes[LspProtocolHelpers.ToDocumentUri(document.DocumentPath)].Length);
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDefinitionAsync_WithOpenDocuments_PreservesPrimaryDocumentDefinition()
    {
        var primaryDocument = CreateDocument(
            @"D:\temp\PrimaryCounter.jazor",
            """
            @code {
                private void Increment()
                {
                }

                private void Trigger()
                {
                    Increment();
                }
            }
            """);
        var secondaryDocument = CreateDocument(
            @"D:\temp\SecondaryCounter.jazor",
            """
            @code {
                private int count;
            }
            """);

        var locations = await _service.GetDefinitionAsync(
            primaryDocument,
            GetPosition(primaryDocument.Text, "Increment();"),
            [primaryDocument, secondaryDocument],
            CancellationToken.None);

        Assert.AreEqual(1, locations.Count);
        Assert.AreEqual(
            LspProtocolHelpers.ToDocumentUri(primaryDocument.DocumentPath),
            locations[0].Uri);
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetReferencesAsync_AcrossOpenDocuments_ReturnsMappedLocationsFromAllDocuments()
    {
        var declarationDocument = CreateDocument(
            @"D:\temp\SharedSource.jazor",
            """
            @code {
                public static int Shared => 42;
            }
            """);
        var referencedTypeName = GetProjectedComponentTypeName(declarationDocument);
        var referenceText =
            "@code {\n" +
            "    private int Read()\n" +
            "    {\n" +
            $"        return {referencedTypeName}.Shared + {referencedTypeName}.Shared;\n" +
            "    }\n" +
            "}\n";
        var referenceDocument = CreateDocument(
            @"D:\temp\SharedConsumer.jazor",
            referenceText);
        var secondReferenceDocument = CreateDocument(
            @"D:\temp\SharedConsumer2.jazor",
            "@code {\n    private int ReadAgain() { return " + referencedTypeName + ".Shared; }\n}\n");

        var locations = await _service.GetReferencesAsync(
            referenceDocument,
            GetPosition(referenceDocument.Text, ".Shared"),
            includeDeclaration: true,
            [declarationDocument, referenceDocument, secondReferenceDocument],
            CancellationToken.None);

        Assert.IsTrue(locations.Count >= 2);
        Assert.IsTrue(locations.Any(static location => location.Uri.EndsWith("SharedConsumer.jazor", StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(locations.Any(static location => location.Uri.EndsWith("SharedConsumer2.jazor", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetRenameAsync_AcrossOpenDocuments_ReturnsWorkspaceEditsForBothDocuments()
    {
        var declarationDocument = CreateDocument(
            @"D:\temp\SharedSource.jazor",
            """
            @code {
                public static int Shared => 42;
            }
            """);
        var referencedTypeName = GetProjectedComponentTypeName(declarationDocument);
        var referenceText =
            "@code {\n" +
            "    private int Read()\n" +
            "    {\n" +
            $"        return {referencedTypeName}.Shared;\n" +
            "    }\n" +
            "}\n";
        var referenceDocument = CreateDocument(
            @"D:\temp\SharedConsumer.jazor",
            referenceText);
        var secondReferenceDocument = CreateDocument(
            @"D:\temp\SharedConsumer2.jazor",
            "@code {\n    private int ReadAgain() { return " + referencedTypeName + ".Shared; }\n}\n");

        var rename = await _service.GetRenameAsync(
            referenceDocument,
            GetPosition(referenceDocument.Text, ".Shared"),
            "Total",
            [declarationDocument, referenceDocument, secondReferenceDocument],
            CancellationToken.None);

        Assert.IsNotNull(rename);
        Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(referenceDocument.DocumentPath)));
        Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(secondReferenceDocument.DocumentPath)));
        Assert.IsTrue(rename.Changes.SelectMany(static pair => pair.Value).All(static edit => edit.NewText == "Total"));
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDefinitionReferencesAndRenameAsync_IncludeUnopenedDiskBackedCSharpDocuments()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "jazor-vuehost-roslyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var featuresDirectory = Path.Combine(tempDirectory, "Features");
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var dashboardsDirectory = Path.Combine(tempDirectory, "Dashboards");
            Directory.CreateDirectory(featuresDirectory);
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(dashboardsDirectory);

            var declarationPath = Path.Combine(sharedDirectory, "SharedState.cs");
            var declarationText =
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """;
            await File.WriteAllTextAsync(declarationPath, declarationText);

            var primaryPath = Path.Combine(featuresDirectory, "CounterConsumer.cs");
            var primaryText =
                """
                namespace Demo;

                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Count;
                    }
                }
                """;
            await File.WriteAllTextAsync(primaryPath, primaryText);

            var unopenedReferencePath = Path.Combine(dashboardsDirectory, "DashboardConsumer.cs");
            var unopenedReferenceText =
                """
                namespace Demo;

                internal static class DashboardConsumer
                {
                    internal static int Read()
                    {
                        return SharedState.Count + SharedState.Count;
                    }
                }
                """;
            await File.WriteAllTextAsync(unopenedReferencePath, unopenedReferenceText);

            var primaryDocument = new DocumentSnapshot(
                primaryPath,
                DocumentKind.CSharp,
                primaryText,
                "1");
            var usagePosition = GetPosition(primaryText, "SharedState.Count", advance: "SharedState.".Length + 1);

            var definition = await _service.GetDefinitionAsync(
                primaryDocument,
                usagePosition,
                CancellationToken.None);
            var references = await _service.GetReferencesAsync(
                primaryDocument,
                usagePosition,
                includeDeclaration: true,
                CancellationToken.None);
            var rename = await _service.GetRenameAsync(
                primaryDocument,
                usagePosition,
                "TotalCount",
                CancellationToken.None);

            Assert.AreEqual(1, definition.Count);
            Assert.AreEqual(
                LspProtocolHelpers.ToDocumentUri(declarationPath),
                definition[0].Uri);

            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(primaryPath)));
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(declarationPath)));
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(unopenedReferencePath)));

            Assert.IsNotNull(rename);
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(primaryPath)));
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(declarationPath)));
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(unopenedReferencePath)));
            Assert.IsTrue(rename.Changes.SelectMany(static pair => pair.Value).All(static edit => edit.NewText == "TotalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetReferencesAndRenameAsync_FromJazorDeclaration_IncludeUnopenedDiskBackedCSharpAndJazorDocuments()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "jazor-vuehost-roslyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var featuresDirectory = Path.Combine(tempDirectory, "Features");
            var dashboardsDirectory = Path.Combine(tempDirectory, "Dashboards");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(featuresDirectory);
            Directory.CreateDirectory(dashboardsDirectory);

            var declarationDocument = CreateDocument(
                Path.Combine(sharedDirectory, "SharedState.jazor"),
                """
                @code {
                    public static int Count => 1;
                }
                """);
            var referencedTypeName = GetProjectedComponentTypeName(declarationDocument);

            var unopenedCSharpPath = Path.Combine(featuresDirectory, "CounterConsumer.cs");
            await File.WriteAllTextAsync(
                unopenedCSharpPath,
                $$"""
                internal static class CounterConsumer
                {
                    internal static int Read()
                    {
                        return {{referencedTypeName}}.Count;
                    }
                }
                """);

            var unopenedJazorPath = Path.Combine(dashboardsDirectory, "DashboardPanel.jazor");
            await File.WriteAllTextAsync(
                unopenedJazorPath,
                $$"""
                @code {
                    private int Read()
                    {
                        return {{referencedTypeName}}.Count + {{referencedTypeName}}.Count;
                    }
                }
                """);

            var declarationPosition = GetPosition(declarationDocument.Text, "Count =>", advance: 1);

            var references = await _service.GetReferencesAsync(
                declarationDocument,
                declarationPosition,
                includeDeclaration: true,
                [declarationDocument],
                CancellationToken.None);
            var rename = await _service.GetRenameAsync(
                declarationDocument,
                declarationPosition,
                "TotalCount",
                [declarationDocument],
                CancellationToken.None);

            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(declarationDocument.DocumentPath)));
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(unopenedCSharpPath)));
            Assert.IsTrue(references.Any(location => location.Uri == LspProtocolHelpers.ToDocumentUri(unopenedJazorPath)));

            Assert.IsNotNull(rename);
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(declarationDocument.DocumentPath)));
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(unopenedCSharpPath)));
            Assert.IsTrue(rename.Changes.ContainsKey(LspProtocolHelpers.ToDocumentUri(unopenedJazorPath)));
            Assert.IsTrue(rename.Changes.SelectMany(static pair => pair.Value).All(static edit => edit.NewText == "TotalCount"));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public async Task InProcRoslynCodeService_GetDefinitionAsync_FromJazorCodeUsage_ResolvesUnopenedDiskBackedCSharpDeclaration()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "jazor-vuehost-roslyn-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            var sharedDirectory = Path.Combine(tempDirectory, "Shared");
            var featuresDirectory = Path.Combine(tempDirectory, "Features");
            Directory.CreateDirectory(sharedDirectory);
            Directory.CreateDirectory(featuresDirectory);

            var declarationPath = Path.Combine(sharedDirectory, "SharedState.cs");
            await File.WriteAllTextAsync(
                declarationPath,
                """
                namespace Demo;

                internal static class SharedState
                {
                    internal static int Count = 1;
                }
                """);

            var primaryDocument = CreateDocument(
                Path.Combine(featuresDirectory, "DashboardPanel.jazor"),
                """
                @using Demo

                @code {
                    private int Read()
                    {
                        return SharedState.Count;
                    }
                }
                """);
            var usagePosition = GetPosition(primaryDocument.Text, "SharedState.Count", advance: "SharedState.".Length + 1);

            var definition = await _service.GetDefinitionAsync(
                primaryDocument,
                usagePosition,
                [primaryDocument],
                CancellationToken.None);

            Assert.AreEqual(1, definition.Count);
            Assert.AreEqual(LspProtocolHelpers.ToDocumentUri(declarationPath), definition[0].Uri);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static string GetProjectedComponentTypeName(DocumentSnapshot document)
    {
        var projectionService = new RazorDesignTimeCodeProjectionService();
        Assert.IsTrue(projectionService.TryCreateProjection(document, out var projection));

        var namespaceMatches = Regex.Matches(
            projection.SourceText,
            @"namespace\s+(?<name>[A-Za-z0-9_.]+)",
            RegexOptions.CultureInvariant);
        var classMatches = Regex.Matches(
            projection.SourceText,
            @"public\s+partial\s+class\s+(?<name>[A-Za-z0-9_]+)",
            RegexOptions.CultureInvariant);
        Assert.IsTrue(namespaceMatches.Count > 0);
        Assert.IsTrue(classMatches.Count > 0);

        var sharedIndex = projection.SourceText.IndexOf("Shared", StringComparison.Ordinal);
        Assert.IsTrue(sharedIndex >= 0);
        var classMatch = classMatches
            .Where(match => match.Index <= sharedIndex)
            .LastOrDefault();
        Assert.IsNotNull(classMatch);

        var namespaceMatch = namespaceMatches
            .Where(match => match.Index <= sharedIndex)
            .LastOrDefault();
        Assert.IsNotNull(namespaceMatch);

        return $"global::{namespaceMatch.Groups["name"].Value}.{classMatch.Groups["name"].Value}";
    }

    private static DocumentSnapshot CreateDocument(string text)
        => CreateDocument(@"D:\temp\Counter.jazor", text);

    private static DocumentSnapshot CreateDocument(string path, string text)
        => new(
            path,
            DocumentKind.Jazor,
            text,
            "1");

    private static void AssertSignatureHelp(LspSignatureHelp? signatureHelp, int expectedActiveParameter)
    {
        Assert.IsNotNull(signatureHelp);
        var help = signatureHelp!;
        Assert.AreEqual(1, help.Signatures.Length);
        Assert.AreEqual(0, help.ActiveSignature);
        Assert.AreEqual(expectedActiveParameter, help.ActiveParameter);
        Assert.IsNotNull(help.Signatures[0].Parameters);
        var parameters = help.Signatures[0].Parameters!;
        Assert.AreEqual(3, parameters.Length);
        Assert.AreEqual("int count", parameters[0].Label);
        Assert.AreEqual("string prefix", parameters[1].Label);
        Assert.AreEqual("bool includeUnits", parameters[2].Label);
        StringAssert.Contains(help.Signatures[0].Label, "FormatValue");
        StringAssert.Contains(help.Signatures[0].Label, "int count");
        StringAssert.Contains(help.Signatures[0].Label, "string prefix");
        StringAssert.Contains(help.Signatures[0].Label, "bool includeUnits");
    }

    private static void AssertHasSemanticToken(
        IReadOnlyList<LspSemanticToken> tokens,
        LspPosition position,
        int length,
        string tokenType,
        params string[] modifiers)
    {
        var token = tokens.FirstOrDefault(candidate =>
            candidate.Line == position.Line
            && candidate.Character == position.Character
            && candidate.Length == length
            && string.Equals(candidate.TokenType, tokenType, StringComparison.Ordinal));
        Assert.IsNotNull(token, $"Expected semantic token '{tokenType}' at {position.Line}:{position.Character}.");

        CollectionAssert.AreEquivalent(
            modifiers,
            token.TokenModifiers,
            $"Expected modifiers '{string.Join(", ", modifiers)}' for semantic token '{tokenType}' at {position.Line}:{position.Character}.");
    }

    private static LspPosition GetPosition(string text, string marker, int advance = 0)
    {
        var offset = text.IndexOf(marker, StringComparison.Ordinal);
        Assert.IsTrue(offset >= 0, $"Expected marker '{marker}' to exist.");
        return LspProtocolHelpers.GetPosition(text, offset + advance);
    }
}
