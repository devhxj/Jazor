


using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;
namespace Jero.Test.SaveLife;

public class SaveLifeMaterialTextExtractionServiceTests
{
    [Fact]
    public async Task ExtractAsync_PdfWithTextLayer_ReturnsExtractedText()
    {
        // Arrange
        var service = new SaveLifeMaterialTextExtractionService();
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "report.pdf",
            FileSuffix = ".pdf",
            ContentType = "application/pdf"
        };
        var pdfBytes = CreatePdf("lung cancer report");

        // Act
        var result = await service.ExtractAsync(material, () => Task.FromResult<Stream>(new MemoryStream(pdfBytes)), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("synced", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Contains("lung cancer report", result.ExtractedText);
    }

    [Fact]
    public async Task ExtractAsync_PdfWithoutTextLayer_ReturnsOcrFallbackSkipped()
    {
        // Arrange
        var service = new SaveLifeMaterialTextExtractionService();
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "scan.pdf",
            FileSuffix = ".pdf",
            ContentType = "application/pdf"
        };
        var pdfBytes = CreateBlankPdf();

        // Act
        var result = await service.ExtractAsync(material, () => Task.FromResult<Stream>(new MemoryStream(pdfBytes)), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("skipped", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Contains("OCR/视觉解析", result.Error);
        Assert.Null(result.ExtractedText);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithText_ReturnsExtractedText()
    {
        // Arrange
        var service = new SaveLifeMaterialTextExtractionService();
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "report.docx",
            FileSuffix = ".docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
        var docxBytes = CreateDocx("肺癌诊断报告");

        // Act
        var result = await service.ExtractAsync(material, () => Task.FromResult<Stream>(new MemoryStream(docxBytes)), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("synced", result.Status);
        Assert.Equal("docx-openxml", result.ExtractModel);
        Assert.Contains("肺癌诊断报告", result.ExtractedText);
    }

    [Fact]
    public async Task ExtractAsync_InvalidLegacyDoc_ReturnsFailedWithoutThrowing()
    {
        // Arrange
        var service = new SaveLifeMaterialTextExtractionService();
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "legacy.doc",
            FileSuffix = ".doc",
            ContentType = "application/msword"
        };
        var invalidDocBytes = Encoding.UTF8.GetBytes("not a legacy word binary document");

        // Act
        var result = await service.ExtractAsync(material, () => Task.FromResult<Stream>(new MemoryStream(invalidDocBytes)), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("failed", result.Status);
        Assert.Equal("doc-b2xtranslator", result.ExtractModel);
        Assert.Null(result.ExtractedText);
    }

    [Theory]
    [InlineData(".pdf", "application/pdf")]
    [InlineData(".doc", "application/msword")]
    [InlineData(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public async Task ExtractAsync_DocumentActualStreamExceedsLimit_FailsAtBoundary(
        string suffix,
        string contentType)
    {
        var service = new SaveLifeMaterialTextExtractionService();
        var stream = new CountingGeneratedStream(
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1);
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = $"synthetic{suffix}",
            FileSuffix = suffix,
            ContentType = contentType
        };

        var result = await service.ExtractAsync(
            material,
            () => Task.FromResult<Stream>(stream),
            TestContext.Current.CancellationToken);

        Assert.Equal("failed", result.Status);
        Assert.Contains("超过 20MB", result.Error, StringComparison.Ordinal);
        Assert.Equal(
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1,
            stream.BytesRead);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithTooManyEntries_FailsClosed()
    {
        var entries = Enumerable.Range(0, 257)
            .Select(index => ($"custom/item-{index}.txt", "x"))
            .Append(("word/document.xml", CreateDocumentXml("正文")))
            .ToArray();

        var result = await ExtractDocxAsync(CreateDocx(entries));

        Assert.Equal("failed", result.Status);
        Assert.Contains("条目过多", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithEntryNameBeyondLimit_FailsClosed()
    {
        var longName = "word/" + new string(
            'a',
            SaveLifeMaterialTextExtractionLimits.MaximumDocxEntryNameCharacters);
        var bytes = CreateDocx(
            ("word/document.xml", CreateDocumentXml("正文")),
            (longName, "synthetic"));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("名称或路径无效", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithXmlEntryBeyondExpandedLimit_FailsClosed()
    {
        var oversizedXml = new string(
            'A',
            checked((int)SaveLifeMaterialTextExtractionLimits.MaximumDocxXmlEntryBytes + 1));
        var bytes = CreateDocx(
            CompressionLevel.NoCompression,
            ("word/document.xml", oversizedXml));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("单项展开量", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithAggregateXmlBeyondLimit_FailsClosed()
    {
        var entryLength = checked(
            (int)(SaveLifeMaterialTextExtractionLimits.MaximumDocxXmlTotalBytes / 3) + 1);
        var xml = new string('A', entryLength);
        var bytes = CreateDocx(
            CompressionLevel.NoCompression,
            ("word/document.xml", xml),
            ("word/header1.xml", xml),
            ("word/header2.xml", xml));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("累计展开量", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithHighCompressionXml_FailsClosed()
    {
        var repeated = new string('A', 512 * 1024);
        var bytes = CreateDocx(("word/document.xml", CreateDocumentXml(repeated)));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("压缩比", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithDuplicateEntries_FailsClosed()
    {
        var bytes = CreateDocx(
            ("word/document.xml", CreateDocumentXml("正文一")),
            ("word/document.xml", CreateDocumentXml("正文二")));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("重复条目", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithDtdExternalEntity_FailsClosed()
    {
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE document [<!ENTITY external SYSTEM "file:///synthetic.invalid">]>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body><w:p><w:r><w:t>&external;</w:t></w:r></w:p></w:body>
            </w:document>
            """;

        var result = await ExtractDocxAsync(CreateDocx(("word/document.xml", xml)));

        Assert.Equal("failed", result.Status);
        Assert.Contains("DTD", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithExcessiveXmlDepth_FailsClosed()
    {
        var nestedStart = string.Concat(Enumerable.Repeat("<w:sdt>", 70));
        var nestedEnd = string.Concat(Enumerable.Repeat("</w:sdt>", 70));
        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body>{nestedStart}<w:p><w:r><w:t>正文</w:t></w:r></w:p>{nestedEnd}</w:body>
            </w:document>
            """;

        var result = await ExtractDocxAsync(CreateDocx(("word/document.xml", xml)));

        Assert.Equal("failed", result.Status);
        Assert.Contains("XML 深度", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithExternalRelationship_FailsClosed()
    {
        var relationships = """
            <?xml version="1.0" encoding="UTF-8"?>
            <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
              <Relationship Id="rId9" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="https://example.invalid/image.png" TargetMode="External"/>
            </Relationships>
            """;
        var bytes = CreateDocx(
            ("word/document.xml", CreateDocumentXml("正文")),
            ("word/_rels/document.xml.rels", relationships));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("外部关系", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithNestedPackage_FailsClosed()
    {
        var bytes = CreateDocx(
            ("word/document.xml", CreateDocumentXml("正文")),
            ("word/embeddings/payload.zip", "synthetic"));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("嵌套包", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithAltChunk_FailsClosed()
    {
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
              <w:body><w:altChunk r:id="rId9" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"/></w:body>
            </w:document>
            """;

        var result = await ExtractDocxAsync(CreateDocx(("word/document.xml", xml)));

        Assert.Equal("failed", result.Status);
        Assert.Contains("altChunk", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithInvalidEntryPath_FailsClosed()
    {
        var bytes = CreateDocx(
            ("word/document.xml", CreateDocumentXml("正文")),
            ("../payload.xml", "<payload/>"));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("路径无效", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithMalformedXml_FailsClosed()
    {
        var bytes = CreateDocx(("word/document.xml", "<w:document xmlns:w=\"urn:test\"><w:p>"));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("XML 结构异常", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_DocxTextBeyondLimit_StopsAtCharacterBudget()
    {
        var text = string.Concat(Enumerable.Range(0, 2500).Select(index => index.ToString("D8")));
        var bytes = CreateDocx(("word/document.xml", CreateDocumentXml(text)));

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("synced", result.Status);
        Assert.Equal(
            SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters,
            result.ExtractedText?.Length);
    }

    [Fact]
    public async Task ExtractAsync_DocxWithEncryptedFlag_FailsClosed()
    {
        var bytes = CreateDocx(("word/document.xml", CreateDocumentXml("正文")));
        SetZipEncryptionFlags(bytes);

        var result = await ExtractDocxAsync(bytes);

        Assert.Equal("failed", result.Status);
        Assert.Contains("已加密", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_PdfBeyondPageLimit_FailsClosed()
    {
        var builder = new PdfDocumentBuilder();
        for (var index = 0;
             index <= SaveLifeMaterialTextExtractionLimits.MaximumPdfPages;
             index++)
        {
            builder.AddPage(PageSize.A4);
        }
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "synthetic.pdf",
            FileSuffix = ".pdf",
            ContentType = "application/pdf"
        };

        var result = await new SaveLifeMaterialTextExtractionService().ExtractAsync(
            material,
            () => Task.FromResult<Stream>(new MemoryStream(builder.Build(), writable: false)),
            TestContext.Current.CancellationToken);

        Assert.Equal("failed", result.Status);
        Assert.Contains("页数", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_WhenOpeningDocumentTimesOut_ReturnsStableFailure()
    {
        var service = new SaveLifeMaterialTextExtractionService(TimeSpan.FromMilliseconds(50));
        var neverCompletes = new TaskCompletionSource<Stream>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var result = await service.ExtractAsync(
            DocxMaterial(),
            () => neverCompletes.Task,
            TestContext.Current.CancellationToken);

        Assert.Equal("failed", result.Status);
        Assert.Contains("解析超时", result.Error, StringComparison.Ordinal);
        Assert.Equal("docx-openxml", result.ExtractModel);
    }

    [Fact]
    public async Task ExtractAsync_WhenCallerCancels_PropagatesCancellation()
    {
        var service = new SaveLifeMaterialTextExtractionService(TimeSpan.FromSeconds(5));
        var neverCompletes = new TaskCompletionSource<Stream>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken);
        var extraction = service.ExtractAsync(
            DocxMaterial(),
            () => neverCompletes.Task,
            cancellation.Token);

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => extraction);
    }

    [Fact]
    public async Task ExtractAsync_InvalidLegacyDoc_RemovesTemporaryWorkspace()
    {
        var before = Directory.GetFileSystemEntries(
            Path.GetTempPath(),
            "savelife-material-*",
            SearchOption.TopDirectoryOnly).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "synthetic.doc",
            FileSuffix = ".doc",
            ContentType = "application/msword"
        };

        var result = await new SaveLifeMaterialTextExtractionService().ExtractAsync(
            material,
            () => Task.FromResult<Stream>(new MemoryStream("invalid"u8.ToArray(), writable: false)),
            TestContext.Current.CancellationToken);

        var after = Directory.GetFileSystemEntries(
            Path.GetTempPath(),
            "savelife-material-*",
            SearchOption.TopDirectoryOnly).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Equal("failed", result.Status);
        Assert.True(before.SetEquals(after));
    }

    [Fact]
    public async Task ExtractAsync_WithProductionWorker_ParsesPdfInChildProcess()
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(PageSize.A4);
        page.AddText("worker pdf report", 12, new PdfPoint(50, 750), font);
        var material = new SaveLifeEnrollmentMaterial
        {
            FileName = "synthetic.pdf",
            FileSuffix = ".pdf",
            ContentType = "application/pdf"
        };
        var service = new SaveLifeMaterialTextExtractionService(
            new SaveLifeDocumentParserWorkerClient());

        var result = await service.ExtractAsync(
            material,
            () => Task.FromResult<Stream>(new MemoryStream(builder.Build(), writable: false)),
            TestContext.Current.CancellationToken);

        Assert.Equal("synced", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Contains("worker pdf report", result.ExtractedText);
    }

    [Fact]
    public async Task ExtractAsync_WithProductionWorkerAndBlankPdf_ReturnsSkippedWithModel()
    {
        var service = new SaveLifeMaterialTextExtractionService(
            new SaveLifeDocumentParserWorkerClient());

        var result = await service.ExtractAsync(
            new SaveLifeEnrollmentMaterial
            {
                FileName = "synthetic.pdf",
                FileSuffix = ".pdf",
                ContentType = "application/pdf"
            },
            () => Task.FromResult<Stream>(new MemoryStream(CreateBlankPdf(), writable: false)),
            TestContext.Current.CancellationToken);

        Assert.Equal("skipped", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Contains("OCR/视觉解析", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExtractAsync_WhenWorkerCannotStart_ReturnsSanitizedFailure()
    {
        var client = new SaveLifeDocumentParserWorkerClient(
            _ => throw new InvalidOperationException("private temporary path"));

        var result = await ExtractPdfWithWorkerAsync(client, "start failure probe");

        Assert.Equal("failed", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Equal("病例文档解析服务暂时不可用，请稍后重试", result.Error);
        Assert.DoesNotContain("private", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExtractAsync_WhenWorkerOmitsResult_ReturnsStableFailure()
    {
        var client = new SaveLifeDocumentParserWorkerClient(
            request => CreateExitProcessStartInfo(request, exitCode: 0));

        var result = await ExtractPdfWithWorkerAsync(client, "missing result probe");

        Assert.Equal("failed", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Equal("病例文档解析子进程返回无效结果，请稍后重试", result.Error);
    }

    [Fact]
    public async Task ExtractAsync_WhenWorkerReturnsInvalidJson_ReturnsStableFailure()
    {
        var client = new SaveLifeDocumentParserWorkerClient(
            CreateInvalidResultProcessStartInfo);

        var result = await ExtractPdfWithWorkerAsync(client, "invalid result probe");

        Assert.Equal("failed", result.Status);
        Assert.Equal("pdf-pdfpig", result.ExtractModel);
        Assert.Equal("病例文档解析子进程返回无效结果，请稍后重试", result.Error);
    }

    [Fact]
    public async Task ExtractAsync_WhenWorkerCrashes_ReturnsFailureAndNextWorkerStillRuns()
    {
        var crashingClient = new SaveLifeDocumentParserWorkerClient(
            request => CreateFaultProcessStartInfo(request, sleep: false));
        var failed = await ExtractPdfWithWorkerAsync(crashingClient, "crash probe");

        var recovered = await ExtractPdfWithWorkerAsync(
            new SaveLifeDocumentParserWorkerClient(),
            "recovery report");

        Assert.Equal("failed", failed.Status);
        Assert.Contains("异常退出", failed.Error, StringComparison.Ordinal);
        Assert.Equal("synced", recovered.Status);
        Assert.Contains("recovery report", recovered.ExtractedText);
    }

    [Fact]
    public async Task ExtractAsync_WhenWorkerTimesOut_KillsWorkerAndRemovesTemporaryDirectory()
    {
        var before = Directory.GetDirectories(
            Path.GetTempPath(),
            "savelife-document-worker-*",
            SearchOption.TopDirectoryOnly).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sleepingClient = new SaveLifeDocumentParserWorkerClient(
            request => CreateFaultProcessStartInfo(request, sleep: true));
        var service = new SaveLifeMaterialTextExtractionService(
            sleepingClient,
            TimeSpan.FromMilliseconds(150));

        var result = await ExtractPdfAsync(service, "timeout probe");

        var after = Directory.GetDirectories(
            Path.GetTempPath(),
            "savelife-document-worker-*",
            SearchOption.TopDirectoryOnly).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Equal("failed", result.Status);
        Assert.Contains("解析超时", result.Error, StringComparison.Ordinal);
        Assert.True(before.SetEquals(after));
    }

    private static async Task<SaveLifeMaterialTextExtractionResult> ExtractPdfWithWorkerAsync(
        ISaveLifeDocumentParserWorkerClient workerClient,
        string text)
    {
        return await ExtractPdfAsync(
            new SaveLifeMaterialTextExtractionService(workerClient),
            text);
    }

    private static async Task<SaveLifeMaterialTextExtractionResult> ExtractPdfAsync(
        SaveLifeMaterialTextExtractionService service,
        string text)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(PageSize.A4);
        page.AddText(text, 12, new PdfPoint(50, 750), font);
        return await service.ExtractAsync(
            new SaveLifeEnrollmentMaterial
            {
                FileName = "synthetic.pdf",
                FileSuffix = ".pdf",
                ContentType = "application/pdf"
            },
            () => Task.FromResult<Stream>(new MemoryStream(builder.Build(), writable: false)),
            TestContext.Current.CancellationToken);
    }

    private static ProcessStartInfo CreateFaultProcessStartInfo(
        SaveLifeDocumentParserWorkerClient.WorkerLaunchRequest request,
        bool sleep)
    {
        var info = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(request.InputPath)!
        };
        if (OperatingSystem.IsWindows())
        {
            if (sleep)
            {
                info.FileName = "powershell.exe";
                info.ArgumentList.Add("-NoProfile");
                info.ArgumentList.Add("-NonInteractive");
                info.ArgumentList.Add("-Command");
                info.ArgumentList.Add("Start-Sleep -Seconds 30");
            }
            else
            {
                info.FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
                info.ArgumentList.Add("/d");
                info.ArgumentList.Add("/c");
                info.ArgumentList.Add("exit 17");
            }
        }
        else
        {
            info.FileName = "/bin/sh";
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add(sleep ? "sleep 30" : "exit 17");
        }
        return info;
    }

    private static ProcessStartInfo CreateExitProcessStartInfo(
        SaveLifeDocumentParserWorkerClient.WorkerLaunchRequest request,
        int exitCode)
    {
        var info = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(request.InputPath)!
        };
        if (OperatingSystem.IsWindows())
        {
            info.FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe";
            info.ArgumentList.Add("/d");
            info.ArgumentList.Add("/c");
            info.ArgumentList.Add($"exit {exitCode}");
        }
        else
        {
            info.FileName = "/bin/sh";
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add($"exit {exitCode}");
        }
        return info;
    }

    private static ProcessStartInfo CreateInvalidResultProcessStartInfo(
        SaveLifeDocumentParserWorkerClient.WorkerLaunchRequest request)
    {
        var info = new ProcessStartInfo
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(request.InputPath)!
        };
        if (OperatingSystem.IsWindows())
        {
            info.FileName = "powershell.exe";
            info.ArgumentList.Add("-NoProfile");
            info.ArgumentList.Add("-NonInteractive");
            info.ArgumentList.Add("-Command");
            var outputPath = request.OutputPath.Replace("'", "''", StringComparison.Ordinal);
            info.ArgumentList.Add($"[IO.File]::WriteAllText('{outputPath}', [char]123)");
        }
        else
        {
            info.FileName = "/bin/sh";
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add("printf '{' > \"$1\"");
            info.ArgumentList.Add("worker");
            info.ArgumentList.Add(request.OutputPath);
        }
        return info;
    }

    private static async Task<SaveLifeMaterialTextExtractionResult> ExtractDocxAsync(byte[] bytes)
    {
        return await new SaveLifeMaterialTextExtractionService().ExtractAsync(
            DocxMaterial(),
            () => Task.FromResult<Stream>(new MemoryStream(bytes, writable: false)),
            TestContext.Current.CancellationToken);
    }

    private static SaveLifeEnrollmentMaterial DocxMaterial() => new()
    {
        FileName = "synthetic.docx",
        FileSuffix = ".docx",
        ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private static byte[] CreatePdf(string text)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(PageSize.A4);
        page.AddText(text, 12, new PdfPoint(50, 750), font);
        return builder.Build();
    }

    private static byte[] CreateBlankPdf()
    {
        var builder = new PdfDocumentBuilder();
        builder.AddPage(PageSize.A4);
        return builder.Build();
    }

    private static byte[] CreateDocx(string text)
    {
        return CreateDocx(("word/document.xml", CreateDocumentXml(text)));
    }

    private static byte[] CreateDocx(params (string Name, string Content)[] customEntries)
    {
        return CreateDocx(CompressionLevel.Optimal, customEntries);
    }

    private static byte[] CreateDocx(
        CompressionLevel compressionLevel,
        params (string Name, string Content)[] customEntries)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var entry in customEntries)
                AddEntry(archive, entry.Name, entry.Content, compressionLevel);
        }
        return stream.ToArray();
    }

    private static string CreateDocumentXml(string text)
    {
        var wordNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        return new XDocument(
            new XElement(
                XName.Get("document", wordNamespace),
                new XElement(
                    XName.Get("body", wordNamespace),
                    new XElement(
                        XName.Get("p", wordNamespace),
                        new XElement(
                            XName.Get("r", wordNamespace),
                            new XElement(XName.Get("t", wordNamespace), text))))))
            .ToString(SaveOptions.DisableFormatting);
    }

    private static void AddEntry(
        ZipArchive archive,
        string name,
        string content,
        CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        var entry = archive.CreateEntry(name, compressionLevel);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static void SetZipEncryptionFlags(byte[] bytes)
    {
        for (var offset = 0; offset <= bytes.Length - 10; offset++)
        {
            var signature = BitConverter.ToUInt32(bytes, offset);
            if (signature == 0x04034b50)
                bytes[offset + 6] |= 1;
            else if (signature == 0x02014b50)
                bytes[offset + 8] |= 1;
        }
    }

    private sealed class CountingGeneratedStream(long length) : Stream
    {
        private long _position;

        public long BytesRead { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = (int)Math.Min(count, length - _position);
            if (read <= 0)
                return 0;
            buffer.AsSpan(offset, read).Clear();
            _position += read;
            BytesRead += read;
            return read;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = (int)Math.Min(buffer.Length, length - _position);
            if (read <= 0)
                return ValueTask.FromResult(0);
            buffer.Span[..read].Clear();
            _position += read;
            BytesRead += read;
            return ValueTask.FromResult(read);
        }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
