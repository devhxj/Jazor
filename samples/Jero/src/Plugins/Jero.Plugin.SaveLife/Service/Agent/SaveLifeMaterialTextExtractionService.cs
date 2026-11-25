using System.Buffers.Binary;
using System.IO.Compression;
using System.Xml;
using b2xtranslator.txt;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Exceptions;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeMaterialTextExtractionService
{
    Task<SaveLifeMaterialTextExtractionResult> ExtractAsync(
        SaveLifeEnrollmentMaterial material,
        Func<Task<Stream>> openFileStream,
        CancellationToken cancellationToken = default);
}

public sealed class SaveLifeMaterialTextExtractionService : ISaveLifeMaterialTextExtractionService, ITransient
{
    private const uint CentralDirectoryHeaderSignature = 0x02014b50;
    private const uint EndOfCentralDirectorySignature = 0x06054b50;
    private const int EndOfCentralDirectoryMinimumLength = 22;
    private const int MaximumZipCommentLength = ushort.MaxValue;

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly TimeSpan _totalTimeout;
    private readonly ISaveLifeDocumentParserWorkerClient _workerClient;

    public SaveLifeMaterialTextExtractionService(
        ISaveLifeDocumentParserWorkerClient workerClient)
        : this(
            workerClient,
            TimeSpan.FromSeconds(SaveLifeMaterialTextExtractionLimits.TotalTimeoutSeconds))
    {
    }

    internal SaveLifeMaterialTextExtractionService(TimeSpan totalTimeout)
        : this(new SaveLifeInProcessDocumentParserWorkerClient(), totalTimeout)
    {
    }

    internal SaveLifeMaterialTextExtractionService()
        : this(TimeSpan.FromSeconds(
            SaveLifeMaterialTextExtractionLimits.TotalTimeoutSeconds))
    {
    }

    internal SaveLifeMaterialTextExtractionService(
        ISaveLifeDocumentParserWorkerClient workerClient,
        TimeSpan totalTimeout)
    {
        _workerClient = workerClient;
        if (totalTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(totalTimeout));
        _totalTimeout = totalTimeout;
    }

    public async Task<SaveLifeMaterialTextExtractionResult> ExtractAsync(
        SaveLifeEnrollmentMaterial material,
        Func<Task<Stream>> openFileStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(openFileStream);

        var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(
            material.FileSuffix ?? material.FileName);
        var contentType = SaveLifeEnrollmentMaterialHelper.ResolveContentType(
            suffix,
            material.ContentType);
        if (SaveLifeEnrollmentMaterialHelper.IsImageOrVideo(suffix, contentType))
        {
            return new SaveLifeMaterialTextExtractionResult
            {
                Status = "synced",
                ExtractModel = "multimodal-embedding"
            };
        }

        if (suffix is not ".pdf" and not ".doc" and not ".docx")
            return Skipped("该材料格式暂未接入全文解析");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_totalTimeout);
        try
        {
            await using var source = await openFileStream().WaitAsync(timeout.Token);
            await using var input = await ReadBoundedInputAsync(source, timeout.Token);
            input.Position = 0;
            return suffix switch
            {
                ".pdf" or ".doc" => await _workerClient.ExtractAsync(
                    suffix,
                    input,
                    timeout.Token),
                ".docx" => await ExtractDocxResultAsync(input, timeout.Token),
                _ => Skipped("该材料格式暂未接入全文解析")
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return Failed("病例文档解析超时，请稍后重试", ResolveExtractModel(suffix));
        }
        catch (DocumentRejectedException ex)
        {
            return Failed(ex.PublicMessage, ResolveExtractModel(suffix));
        }
    }

    private static async Task<MemoryStream> ReadBoundedInputAsync(
        Stream source,
        CancellationToken cancellationToken)
    {
        var result = new MemoryStream();
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            var remaining = SaveLifeMaterialTextExtractionLimits.MaximumInputBytes + 1 - total;
            var read = await source.ReadAsync(
                buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                cancellationToken);
            if (read == 0)
                break;

            total += read;
            if (total > SaveLifeMaterialTextExtractionLimits.MaximumInputBytes)
            {
                await result.DisposeAsync();
                throw Rejected("病例文档实际内容超过 20MB，无法后台解析");
            }

            await result.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        if (total == 0)
        {
            await result.DisposeAsync();
            throw Rejected("病例资料文件为空，无法后台解析");
        }

        result.Position = 0;
        return result;
    }

    private static SaveLifeMaterialTextExtractionResult Skipped(
        string message,
        string? extractModel = null) => new()
        {
            Status = "skipped",
            Error = message,
            ExtractModel = extractModel
        };

    private static SaveLifeMaterialTextExtractionResult Failed(
        string message,
        string? extractModel = null) => new()
        {
            Status = "failed",
            Error = message,
            ExtractModel = extractModel
        };

    internal static async Task<SaveLifeMaterialTextExtractionResult> ExtractPdfResultAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        try
        {
            var extracted = await ExtractPdfTextAsync(stream, cancellationToken);
            return string.IsNullOrWhiteSpace(extracted)
                ? Skipped(
                    "PDF 未提取到文本层，可能是扫描件或图片版 PDF，需接入 OCR/视觉解析",
                    "pdf-pdfpig")
                : new SaveLifeMaterialTextExtractionResult
                {
                    Status = "synced",
                    ExtractedText = extracted,
                    ExtractModel = "pdf-pdfpig"
                };
        }
        catch (PdfDocumentEncryptedException)
        {
            return Failed("PDF 已加密或受密码保护，无法后台解析文本层", "pdf-pdfpig");
        }
        catch (PdfDocumentFormatException)
        {
            return Failed("PDF 文件结构异常，无法解析文本层", "pdf-pdfpig");
        }
        catch (DocumentRejectedException ex)
        {
            return Failed(ex.PublicMessage, "pdf-pdfpig");
        }
        catch (Exception ex) when (IsParserException(ex))
        {
            return Failed("PDF 文件无法解析文本层", "pdf-pdfpig");
        }
    }

    internal static async Task<SaveLifeMaterialTextExtractionResult> ExtractLegacyDocResultAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var workDirectory = Path.Combine(
            Path.GetTempPath(),
            $"savelife-material-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(workDirectory);
            TryRestrictDirectoryPermissions(workDirectory);
            var tempPath = Path.Combine(workDirectory, "input.doc");
            await using (var tempFile = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 64 * 1024,
                useAsync: true))
            {
                TryRestrictFilePermissions(tempPath);
                await stream.CopyToAsync(tempFile, cancellationToken);
            }

            return ExtractLegacyDocFileResult(tempPath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (IsParserException(ex))
        {
            return Failed("旧版 Word 文档结构异常，无法后台解析文本", "doc-b2xtranslator");
        }
        finally
        {
            DeleteDirectoryBestEffort(workDirectory);
        }
    }

    internal static SaveLifeMaterialTextExtractionResult ExtractLegacyDocFileResult(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var extracted = LimitText(DocTextExtractor.ExtractTextFromFile(path));
            cancellationToken.ThrowIfCancellationRequested();
            return string.IsNullOrWhiteSpace(extracted)
                ? Skipped(
                    "旧版 Word 文档未提取到可检索文本，建议转换为 docx 后重新上传",
                    "doc-b2xtranslator")
                : new SaveLifeMaterialTextExtractionResult
                {
                    Status = "synced",
                    ExtractedText = extracted,
                    ExtractModel = "doc-b2xtranslator"
                };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (IsParserException(ex))
        {
            return Failed("旧版 Word 文档结构异常，无法后台解析文本", "doc-b2xtranslator");
        }
    }

    private static async Task<SaveLifeMaterialTextExtractionResult> ExtractDocxResultAsync(
        MemoryStream stream,
        CancellationToken cancellationToken)
    {
        try
        {
            var extracted = await ExtractDocxTextAsync(stream, cancellationToken);
            return string.IsNullOrWhiteSpace(extracted)
                ? Skipped("Word 文档未提取到可检索文本", "docx-openxml")
                : new SaveLifeMaterialTextExtractionResult
                {
                    Status = "synced",
                    ExtractedText = extracted,
                    ExtractModel = "docx-openxml"
                };
        }
        catch (DocumentRejectedException ex)
        {
            return Failed(ex.PublicMessage, "docx-openxml");
        }
        catch (InvalidDataException)
        {
            return Failed("DOCX 已加密或文件结构异常，无法安全解析", "docx-openxml");
        }
        catch (XmlException ex) when (
            ex.Message.Contains("DTD", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("entity", StringComparison.OrdinalIgnoreCase))
        {
            return Failed("DOCX 包含 DTD 或外部实体，已拒绝解析", "docx-openxml");
        }
        catch (XmlException)
        {
            return Failed("DOCX XML 结构异常，无法安全解析", "docx-openxml");
        }
    }

    private static Task<string?> ExtractPdfTextAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var document = PdfDocument.Open(stream, new ParsingOptions
        {
            UseLenientParsing = false,
            SkipMissingFonts = true,
            MaxStackDepth = SaveLifeMaterialTextExtractionLimits.MaximumPdfStackDepth
        });
        if (document.NumberOfPages > SaveLifeMaterialTextExtractionLimits.MaximumPdfPages)
            throw Rejected("PDF 页数超过安全解析限制");

        var builder = new StringBuilder();
        var totalLetters = 0;
        for (var pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = document.GetPage(pageNumber);
            totalLetters = checked(totalLetters + page.Letters.Count);
            if (totalLetters > SaveLifeMaterialTextExtractionLimits.MaximumPdfLetters)
                throw Rejected("PDF 字形数量超过安全解析限制");

            var pageText = ContentOrderTextExtractor.GetText(page);
            if (string.IsNullOrWhiteSpace(pageText))
                continue;

            AppendBounded(builder, pageText.Trim());
            AppendBounded(builder, Environment.NewLine);
            if (builder.Length >= SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters)
                break;
        }

        return Task.FromResult(builder.Length == 0 ? null : LimitText(builder.ToString()));
    }

    private static async Task<string?> ExtractDocxTextAsync(
        MemoryStream stream,
        CancellationToken cancellationToken)
    {
        var buffer = stream.GetBuffer().AsSpan(0, checked((int)stream.Length));
        var manifest = ReadDocxManifest(buffer);
        stream.Position = 0;
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        if (archive.Entries.Count != manifest.EntryCount)
            throw Rejected("DOCX 中央目录与条目数量不一致");

        var entries = archive.Entries.ToDictionary(
            entry => entry.FullName,
            StringComparer.OrdinalIgnoreCase);
        foreach (var relationshipName in manifest.RelationshipEntryNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ValidateRelationshipsAsync(entries[relationshipName], cancellationToken);
        }

        var builder = new StringBuilder();
        foreach (var entryName in manifest.TextEntryNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExtractWordXmlAsync(
                entries[entryName],
                builder,
                cancellationToken);
            if (builder.Length >= SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters)
                break;
        }

        return builder.Length == 0 ? null : LimitText(builder.ToString().Trim());
    }

    private static DocxManifest ReadDocxManifest(ReadOnlySpan<byte> archiveBytes)
    {
        var endOffset = FindEndOfCentralDirectory(archiveBytes);
        var end = archiveBytes[endOffset..];
        var diskNumber = BinaryPrimitives.ReadUInt16LittleEndian(end[4..]);
        var centralDirectoryDisk = BinaryPrimitives.ReadUInt16LittleEndian(end[6..]);
        var entriesOnDisk = BinaryPrimitives.ReadUInt16LittleEndian(end[8..]);
        var entryCount = BinaryPrimitives.ReadUInt16LittleEndian(end[10..]);
        var centralDirectorySize = BinaryPrimitives.ReadUInt32LittleEndian(end[12..]);
        var centralDirectoryOffset = BinaryPrimitives.ReadUInt32LittleEndian(end[16..]);
        if (diskNumber != 0 || centralDirectoryDisk != 0 || entriesOnDisk != entryCount)
            throw Rejected("DOCX 不支持分卷 ZIP 包");
        if (entryCount == ushort.MaxValue ||
            centralDirectorySize == uint.MaxValue ||
            centralDirectoryOffset == uint.MaxValue)
        {
            throw Rejected("DOCX 不支持 ZIP64 包");
        }
        if (entryCount > SaveLifeMaterialTextExtractionLimits.MaximumDocxEntries)
            throw Rejected("DOCX 条目过多，无法安全解析");

        var centralEnd = checked((long)centralDirectoryOffset + centralDirectorySize);
        if (centralEnd > endOffset || centralDirectoryOffset > archiveBytes.Length)
            throw Rejected("DOCX 中央目录范围无效");

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var textEntries = new List<string>();
        var relationshipEntries = new List<string>();
        long totalXmlBytes = 0;
        var offset = checked((int)centralDirectoryOffset);
        for (var index = 0; index < entryCount; index++)
        {
            if (offset > archiveBytes.Length - 46 ||
                BinaryPrimitives.ReadUInt32LittleEndian(archiveBytes[offset..]) !=
                CentralDirectoryHeaderSignature)
            {
                throw Rejected("DOCX 中央目录条目无效");
            }

            var header = archiveBytes[offset..];
            var flags = BinaryPrimitives.ReadUInt16LittleEndian(header[8..]);
            var compressedBytes = BinaryPrimitives.ReadUInt32LittleEndian(header[20..]);
            var uncompressedBytes = BinaryPrimitives.ReadUInt32LittleEndian(header[24..]);
            var nameLength = BinaryPrimitives.ReadUInt16LittleEndian(header[28..]);
            var extraLength = BinaryPrimitives.ReadUInt16LittleEndian(header[30..]);
            var commentLength = BinaryPrimitives.ReadUInt16LittleEndian(header[32..]);
            var diskStart = BinaryPrimitives.ReadUInt16LittleEndian(header[34..]);
            var entryLength = checked(46 + nameLength + extraLength + commentLength);
            if (entryLength > archiveBytes.Length - offset)
                throw Rejected("DOCX 中央目录条目越界");
            if ((flags & 1) != 0)
                throw Rejected("DOCX 已加密或受密码保护，无法安全解析");
            if (diskStart != 0)
                throw Rejected("DOCX 不支持分卷 ZIP 包");

            var name = DecodeEntryName(
                archiveBytes.Slice(offset + 46, nameLength),
                utf8: (flags & (1 << 11)) != 0);
            ValidateEntryName(name);
            if (!names.Add(name))
                throw Rejected("DOCX 包含重复条目，无法安全解析");
            if (IsNestedPackage(name))
                throw Rejected("DOCX 包含嵌套包，无法安全解析");

            if (IsXmlEntry(name))
            {
                if (uncompressedBytes > SaveLifeMaterialTextExtractionLimits.MaximumDocxXmlEntryBytes)
                    throw Rejected("DOCX XML 单项展开量超过安全限制");
                totalXmlBytes = checked(totalXmlBytes + uncompressedBytes);
                if (totalXmlBytes > SaveLifeMaterialTextExtractionLimits.MaximumDocxXmlTotalBytes)
                    throw Rejected("DOCX XML 累计展开量超过安全限制");
                if (uncompressedBytes > 0 &&
                    uncompressedBytes / Math.Max(1u, compressedBytes) >
                    SaveLifeMaterialTextExtractionLimits.MaximumDocxCompressionRatio)
                {
                    throw Rejected("DOCX XML 压缩比超过安全限制");
                }
            }

            if (IsTextEntry(name))
                textEntries.Add(name);
            if (IsRelationshipEntry(name))
                relationshipEntries.Add(name);
            offset += entryLength;
        }

        if (offset != centralEnd)
            throw Rejected("DOCX 中央目录大小不一致");
        if (!names.Contains("word/document.xml"))
            throw Rejected("DOCX 缺少主文档 XML");

        textEntries.Sort((left, right) =>
            string.Equals(left, "word/document.xml", StringComparison.OrdinalIgnoreCase)
                ? -1
                : string.Equals(right, "word/document.xml", StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : StringComparer.OrdinalIgnoreCase.Compare(left, right));
        return new DocxManifest(entryCount, textEntries, relationshipEntries);
    }

    private static int FindEndOfCentralDirectory(ReadOnlySpan<byte> archiveBytes)
    {
        var minimumOffset = Math.Max(
            0,
            archiveBytes.Length - EndOfCentralDirectoryMinimumLength - MaximumZipCommentLength);
        for (var offset = archiveBytes.Length - EndOfCentralDirectoryMinimumLength;
             offset >= minimumOffset;
             offset--)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(archiveBytes[offset..]) !=
                EndOfCentralDirectorySignature)
            {
                continue;
            }

            var commentLength = BinaryPrimitives.ReadUInt16LittleEndian(archiveBytes[(offset + 20)..]);
            if (offset + EndOfCentralDirectoryMinimumLength + commentLength == archiveBytes.Length)
                return offset;
        }

        throw Rejected("DOCX 缺少有效中央目录");
    }

    private static string DecodeEntryName(ReadOnlySpan<byte> bytes, bool utf8)
    {
        try
        {
            if (utf8)
                return StrictUtf8.GetString(bytes);
            if (bytes.ContainsAnyExceptInRange((byte)0x20, (byte)0x7e))
                throw Rejected("DOCX 条目名称编码不受支持");
            return Encoding.ASCII.GetString(bytes);
        }
        catch (DecoderFallbackException ex)
        {
            throw Rejected("DOCX 条目名称编码无效", ex);
        }
    }

    private static void ValidateEntryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            name.Length > SaveLifeMaterialTextExtractionLimits.MaximumDocxEntryNameCharacters ||
            name.Contains('\\') ||
            name.Contains(':') ||
            name.StartsWith('/') ||
            name.Split('/').Any(segment => segment is "." or ".."))
        {
            throw Rejected("DOCX 条目名称或路径无效");
        }
    }

    private static bool IsXmlEntry(string name) =>
        name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
        name.EndsWith(".rels", StringComparison.OrdinalIgnoreCase);

    private static bool IsTextEntry(string name) =>
        name.Equals("word/document.xml", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("word/header", StringComparison.OrdinalIgnoreCase) &&
        name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("word/footer", StringComparison.OrdinalIgnoreCase) &&
        name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("word/footnotes.xml", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("word/endnotes.xml", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("word/comments.xml", StringComparison.OrdinalIgnoreCase);

    private static bool IsRelationshipEntry(string name) =>
        name.EndsWith(".rels", StringComparison.OrdinalIgnoreCase);

    private static bool IsNestedPackage(string name)
    {
        var extension = Path.GetExtension(name);
        return extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".docm", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".dotx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".dotm", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task ValidateRelationshipsAsync(
        ZipArchiveEntry entry,
        CancellationToken cancellationToken)
    {
        await using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, CreateXmlSettings());
        try
        {
            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureXmlDepth(reader);
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.LocalName.Equals("Relationship", StringComparison.OrdinalIgnoreCase) &&
                    reader.GetAttribute("TargetMode")?.Equals(
                        "External",
                        StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw Rejected("DOCX 包含外部关系资源，已拒绝解析");
                }
            }
        }
        catch (XmlException ex) when (IsDtdError(ex))
        {
            throw Rejected("DOCX 包含 DTD 或外部实体，已拒绝解析", ex);
        }
    }

    private static async Task ExtractWordXmlAsync(
        ZipArchiveEntry entry,
        StringBuilder builder,
        CancellationToken cancellationToken)
    {
        await using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, CreateXmlSettings());
        var textDepth = -1;
        var paragraphDepth = -1;
        var paragraphHasText = false;
        var valueBuffer = new char[1024];
        try
        {
            while (await reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                EnsureXmlDepth(reader);
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName.Equals("altChunk", StringComparison.OrdinalIgnoreCase))
                        throw Rejected("DOCX 包含 altChunk 外部内容，已拒绝解析");
                    if (reader.LocalName.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        paragraphDepth = reader.Depth;
                        paragraphHasText = false;
                    }
                    else if (reader.LocalName.Equals("t", StringComparison.OrdinalIgnoreCase))
                    {
                        textDepth = reader.Depth;
                    }
                    else if (paragraphDepth >= 0 &&
                        reader.LocalName is "tab" or "br" or "cr")
                    {
                        AppendBounded(builder, " ");
                    }
                }
                else if (reader.NodeType is XmlNodeType.Text or XmlNodeType.CDATA && textDepth >= 0)
                {
                    while (true)
                    {
                        var read = await reader.ReadValueChunkAsync(
                            valueBuffer,
                            0,
                            Math.Min(
                                valueBuffer.Length,
                                SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters -
                                builder.Length));
                        if (read == 0)
                            break;
                        builder.Append(valueBuffer, 0, read);
                        paragraphHasText = true;
                        if (builder.Length >=
                            SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters)
                        {
                            return;
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Depth == textDepth &&
                        reader.LocalName.Equals("t", StringComparison.OrdinalIgnoreCase))
                    {
                        textDepth = -1;
                    }
                    if (reader.Depth == paragraphDepth &&
                        reader.LocalName.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        if (paragraphHasText)
                            AppendBounded(builder, Environment.NewLine);
                        paragraphDepth = -1;
                    }
                }
            }
        }
        catch (XmlException ex) when (IsDtdError(ex))
        {
            throw Rejected("DOCX 包含 DTD 或外部实体，已拒绝解析", ex);
        }
    }

    private static XmlReaderSettings CreateXmlSettings() => new()
    {
        Async = true,
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        MaxCharactersFromEntities = 0,
        MaxCharactersInDocument = SaveLifeMaterialTextExtractionLimits.MaximumDocxXmlEntryBytes,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true
    };

    private static void EnsureXmlDepth(XmlReader reader)
    {
        if (reader.Depth > SaveLifeMaterialTextExtractionLimits.MaximumXmlDepth)
            throw Rejected("DOCX XML 深度超过安全限制");
    }

    private static bool IsDtdError(XmlException ex) =>
        ex.Message.Contains("DTD", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("entity", StringComparison.OrdinalIgnoreCase);

    private static void AppendBounded(StringBuilder builder, string value)
    {
        var remaining =
            SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters - builder.Length;
        if (remaining <= 0 || string.IsNullOrEmpty(value))
            return;
        builder.Append(value, 0, Math.Min(value.Length, remaining));
    }

    private static string? LimitText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        return text.Length > SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters
            ? text[..SaveLifeMaterialTextExtractionLimits.MaximumExtractedTextCharacters]
            : text;
    }

    private static void DeleteDirectoryBestEffort(string path)
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
                return;
            }
            catch
            {
                if (attempt < 2)
                    Thread.Sleep(10);
            }
        }
    }

    private static void TryRestrictDirectoryPermissions(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(
                path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    private static void TryRestrictFilePermissions(string path)
    {
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    private static string? ResolveExtractModel(string suffix) => suffix switch
    {
        ".pdf" => "pdf-pdfpig",
        ".doc" => "doc-b2xtranslator",
        ".docx" => "docx-openxml",
        _ => null
    };

    private static bool IsParserException(Exception ex) =>
        ex is not OperationCanceledException;

    private static DocumentRejectedException Rejected(
        string publicMessage,
        Exception? innerException = null) => new(publicMessage, innerException);

    private sealed record DocxManifest(
        int EntryCount,
        IReadOnlyList<string> TextEntryNames,
        IReadOnlyList<string> RelationshipEntryNames);

    private sealed class DocumentRejectedException(
        string publicMessage,
        Exception? innerException = null) : Exception(
            "Patient document was rejected by the extraction budget.",
            innerException)
    {
        public string PublicMessage { get; } = publicMessage;
    }
}

public sealed class SaveLifeMaterialTextExtractionResult
{
    public string Status { get; set; } = "synced";

    public string? Error { get; set; }

    public string? ExtractedText { get; set; }

    public string? ExtractModel { get; set; }
}
