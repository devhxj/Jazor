using System.Text.Json;
using Jero.Core;
using Jero.Core.Service;

namespace Jero.Plugin.SaveLife.Service;

public interface ISaveLifeAgentMaterialModelInputService
{
    Task<SaveLifeAgentPreparedMaterialInput> BuildAsync(
        SaveLifeAgentMaterialAccessScope scope,
        IReadOnlyList<SaveLifeAgentMaterialReference> references,
        SaveLifeAgentOptions options,
        CancellationToken cancellationToken = default);
}

public interface ISaveLifeAgentMaterialFileReader
{
    Task<Stream> OpenReadAsync(
        long fileId,
        CancellationToken cancellationToken = default);
}

public sealed class SaveLifeAgentMaterialModelInputService(
    SqlSugarRepository<SaveLifeEnrollmentMaterial> materialRep,
    ISaveLifeAgentMaterialFileReader fileReader,
    ISaveLifeAgentMaterialRequestGate requestGate) : ISaveLifeAgentMaterialModelInputService, ITransient
{
    private readonly SqlSugarRepository<SaveLifeEnrollmentMaterial> _materialRep = materialRep;
    private readonly ISaveLifeAgentMaterialFileReader _fileReader = fileReader;
    private readonly ISaveLifeAgentMaterialRequestGate _requestGate = requestGate;

    public Task<SaveLifeAgentPreparedMaterialInput> BuildAsync(
        SaveLifeAgentMaterialAccessScope scope,
        IReadOnlyList<SaveLifeAgentMaterialReference> references,
        CancellationToken cancellationToken = default) =>
        BuildAsync(scope, references, new SaveLifeAgentOptions(), cancellationToken);

    public async Task<SaveLifeAgentPreparedMaterialInput> BuildAsync(
        SaveLifeAgentMaterialAccessScope scope,
        IReadOnlyList<SaveLifeAgentMaterialReference> references,
        SaveLifeAgentOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(references);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();
        if (references.Count == 0)
            return SaveLifeAgentPreparedMaterialInput.Empty;
        if (string.IsNullOrWhiteSpace(scope.ExternalUserId) || scope.PatientProfileId <= 0)
            throw MaterialFailure("scope_missing");

        var requestLease = await _requestGate.AcquireAsync(
            scope.ExternalUserId,
            cancellationToken);
        try
        {

            var requested = ParseMaterialRequests(references);
            var materialIds = requested.Select(item => item.MaterialId).ToList();
            var materials = await _materialRep.AsQueryable()
                .Where(material => materialIds.Contains(material.Id))
                .ToListAsync();
            cancellationToken.ThrowIfCancellationRequested();
            if (materials.Count != materialIds.Count)
                throw MaterialFailure("scope_mismatch");

            var materialById = materials.ToDictionary(material => material.Id);
            var plan = new SaveLifeAgentMaterialRetrievalPlan
            {
                CanSearch = true,
                ExternalUserId = scope.ExternalUserId.Trim(),
                PatientProfileId = scope.PatientProfileId,
                EnrollmentRecordId = scope.EnrollmentRecordId is > 0
                    ? scope.EnrollmentRecordId
                    : null,
                HasMaterialIntent = true
            };
            var prepared = requested
                .Select(item => ValidateAndPrepare(
                    materialById[item.MaterialId],
                    item.Reference,
                    plan))
                .ToList();

            var maxMediaCount = Math.Clamp(
                options.MaterialReviewMaxMediaCount,
                1,
                SaveLifeMaterialResourceBudget.MaterialReviewMediaCount);
            var maxMediaBytes = Math.Clamp(
                options.MaterialReviewMaxMediaBytes,
                1,
                SaveLifeMaterialResourceBudget.MaterialReviewMediaBytes);
            var maxDocumentCharacters = Math.Clamp(
                options.MaterialReviewMaxDocumentCharacters,
                1,
                SaveLifeMaterialResourceBudget.MaterialReviewDocumentCharacters);
            var maxSerializedRequestBytes = Math.Clamp(
                options.MaterialReviewMaxSerializedRequestBytes,
                1,
                SaveLifeMaterialResourceBudget.MaterialReviewSerializedRequestBytes);
            var content = new List<SaveLifeAgentModelContentPart>(prepared.Count);
            var selectedReferences = new List<SaveLifeAgentMaterialReference>(prepared.Count);
            var evidenceSources = new List<SaveLifeAgentPreparedMaterialEvidence>(prepared.Count);
            long rawMediaBytes = 0;
            var mediaCount = 0;
            var documentCharacters = 0;
            long estimatedSerializedBytes = SaveLifeMaterialResourceBudget.ModelRequestFixedOverheadBytes;
            foreach (var item in prepared)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.Kind == SaveLifeAgentModelContentTypes.Text)
                {
                    var documentText = BuildDocumentText(item.Material);
                    if (documentCharacters + documentText.Length >
                        maxDocumentCharacters)
                    {
                        continue;
                    }

                    var partBytes = JsonSerializer.SerializeToUtf8Bytes(documentText).LongLength + 64;
                    if (estimatedSerializedBytes + partBytes >
                        maxSerializedRequestBytes)
                    {
                        continue;
                    }

                    content.Add(SaveLifeAgentModelContentPart.CreateText(documentText));
                    selectedReferences.Add(item.Reference);
                    evidenceSources.Add(new SaveLifeAgentPreparedMaterialEvidence(
                        item.Material.Id,
                        item.Kind,
                        item.Material.ExtractedText?.Trim()));
                    documentCharacters += documentText.Length;
                    estimatedSerializedBytes += partBytes;
                    continue;
                }

                var declaredBytes = ResolveDeclaredMediaBytes(item.Material, maxMediaBytes);
                var encodedBytes = CalculateBase64Bytes(declaredBytes);
                if (mediaCount >= maxMediaCount ||
                    rawMediaBytes + declaredBytes > maxMediaBytes ||
                    estimatedSerializedBytes + encodedBytes >
                        maxSerializedRequestBytes)
                {
                    continue;
                }

                var media = await BuildDataUrlAsync(item, declaredBytes, cancellationToken);
                content.Add(item.Kind == SaveLifeAgentModelContentTypes.Video
                    ? SaveLifeAgentModelContentPart.CreateVideo(media.DataUrl)
                    : SaveLifeAgentModelContentPart.CreateImage(media.DataUrl));
                selectedReferences.Add(item.Reference);
                evidenceSources.Add(new SaveLifeAgentPreparedMaterialEvidence(
                    item.Material.Id,
                    item.Kind,
                    ExtractedText: null));
                rawMediaBytes += media.ActualBytes;
                mediaCount++;
                estimatedSerializedBytes += CalculateBase64Bytes(media.ActualBytes);
            }

            if (content.Count == 0 || selectedReferences.Count == 0)
            {
                await requestLease.DisposeAsync();
                return SaveLifeAgentPreparedMaterialInput.Empty;
            }

            return new SaveLifeAgentPreparedMaterialInput(
                content,
                selectedReferences,
                evidenceSources,
                rawMediaBytes,
                documentCharacters,
                estimatedSerializedBytes,
                requestLease);
        }
        catch
        {
            await requestLease.DisposeAsync();
            throw;
        }
    }

    private static List<RequestedMaterial> ParseMaterialRequests(
        IReadOnlyList<SaveLifeAgentMaterialReference> references)
    {
        var result = new List<RequestedMaterial>();
        var seen = new HashSet<long>();
        foreach (var reference in references)
        {
            var value = string.IsNullOrWhiteSpace(reference.MaterialId)
                ? reference.Id
                : reference.MaterialId;
            if (!long.TryParse(value, out var materialId) || materialId <= 0)
                throw MaterialFailure("reference_invalid");
            if (seen.Add(materialId))
                result.Add(new RequestedMaterial(materialId, reference));
        }

        return result;
    }

    private static PreparedMaterial ValidateAndPrepare(
        SaveLifeEnrollmentMaterial material,
        SaveLifeAgentMaterialReference reference,
        SaveLifeAgentMaterialRetrievalPlan plan)
    {
        if (!SaveLifeAgentMaterialRetrievalPolicy.IsEligibleForRetrieval(material, plan.ExternalUserId) ||
            !SaveLifeAgentMaterialRetrievalPolicy.IsInScope(material, plan))
        {
            var readiness = SaveLifeAgentMaterialRetrievalPolicy.GetReadinessStatus(material);
            if (readiness == SaveLifeAgentMaterialReadinessStatuses.Processing)
            {
                throw MaterialFailure(
                    "material_processing",
                    SaveLifeAgentMaterialReadinessMessages.Processing);
            }

            throw MaterialFailure(
                "material_not_eligible",
                readiness == SaveLifeAgentMaterialReadinessStatuses.Failed
                    ? SaveLifeAgentMaterialReadinessMessages.Failed
                    : null);
        }

        if (material.SizeBytes < 0 ||
            material.SizeBytes > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes ||
            material.SizeKb < 0 ||
            material.SizeKb > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes / 1024)
        {
            throw MaterialFailure(
                "material_too_large",
                "病例资料文件超过 20MB，暂时无法读取，请压缩后重新上传。",
                retryable: false);
        }

        var suffix = SaveLifeEnrollmentMaterialHelper.NormalizeExtension(
            material.FileSuffix ?? material.FileName);
        var contentType = SaveLifeEnrollmentMaterialHelper.ResolveContentType(
            suffix,
            material.ContentType);
        var kind = ResolveKind(suffix, contentType);
        if (kind == null)
        {
            throw MaterialFailure(
                "material_type_unsupported",
                "该病例资料格式暂不支持直接解读，请重新上传 JPG、PNG、PDF、DOC、DOCX、MP4 或 MOV 文件。",
                retryable: false);
        }

        if (kind != SaveLifeAgentModelContentTypes.Text && material.FileId <= 0)
            throw MaterialFailure("material_file_missing");
        if (kind == SaveLifeAgentModelContentTypes.Text && string.IsNullOrWhiteSpace(material.ExtractedText))
        {
            throw MaterialFailure(
                "material_text_missing",
                "病例文档暂未提取到可读文字；扫描件请改为清晰图片上传，或稍后重试。",
                retryable: true);
        }

        return new PreparedMaterial(material, reference, kind, contentType);
    }

    private static string? ResolveKind(string suffix, string contentType)
    {
        return (suffix, contentType.ToLowerInvariant()) switch
        {
            (".jpg", "image/jpeg") or (".png", "image/png") => SaveLifeAgentModelContentTypes.Image,
            (".mp4", "video/mp4") or (".mov", "video/quicktime") => SaveLifeAgentModelContentTypes.Video,
            (".pdf", "application/pdf") or
            (".doc", "application/msword") or
            (".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document") =>
                SaveLifeAgentModelContentTypes.Text,
            _ => null
        };
    }

    private async Task<PreparedMedia> BuildDataUrlAsync(
        PreparedMaterial item,
        long declaredBytes,
        CancellationToken cancellationToken)
    {
        var capacity = checked((int)declaredBytes);
        var buffer = GC.AllocateUninitializedArray<byte>(capacity);
        try
        {
            await using var stream = await _fileReader.OpenReadAsync(
                item.Material.FileId,
                cancellationToken);
            var total = 0;
            while (true)
            {
                if (total == capacity)
                {
                    var overflow = new byte[1];
                    if (await stream.ReadAsync(overflow, cancellationToken) != 0)
                        throw MaterialFailure("material_stream_too_large", retryable: false);
                    break;
                }

                var read = await stream.ReadAsync(
                    buffer.AsMemory(total, capacity - total),
                    cancellationToken);
                if (read == 0)
                    break;

                total = checked(total + read);
            }

            if (total == 0)
                throw MaterialFailure("material_file_empty");
            if (item.Material.SizeBytes > 0 && total != item.Material.SizeBytes)
                throw MaterialFailure("material_size_mismatch", retryable: false);

            return new PreparedMedia(
                $"data:{item.ContentType};base64,{Convert.ToBase64String(buffer, 0, total)}",
                total);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SaveLifeAgentUnavailableException)
        {
            throw;
        }
        catch
        {
            throw MaterialFailure("material_file_read_failed");
        }
    }

    private static string BuildDocumentText(SaveLifeEnrollmentMaterial material)
    {
        var text = material.ExtractedText!.Trim();
        if (text.Length > SaveLifeMaterialResourceBudget.MaterialReviewDocumentCharactersPerFile)
        {
            text = text[..SaveLifeMaterialResourceBudget.MaterialReviewDocumentCharactersPerFile];
        }

        var fileName = string.IsNullOrWhiteSpace(material.FileName)
            ? "病例文档"
            : material.FileName.Trim();
        return $"病例文档《{fileName}》提取文本：\n{text}";
    }

    private static long ResolveDeclaredMediaBytes(
        SaveLifeEnrollmentMaterial material,
        long maxMediaBytes)
    {
        var bytes = material.SizeBytes > 0
            ? material.SizeBytes
            : checked(material.SizeKb * 1024);
        if (bytes <= 0 || bytes > maxMediaBytes)
            throw MaterialFailure("material_size_invalid", retryable: false);
        return bytes;
    }

    private static long CalculateBase64Bytes(long rawBytes) =>
        4 * ((rawBytes + 2) / 3);

    private static SaveLifeAgentUnavailableException MaterialFailure(
        string reason,
        string? publicMessage = null,
        bool retryable = true)
    {
        return new SaveLifeAgentUnavailableException(
            SaveLifeAgentFailureCodes.MaterialNotReady,
            SaveLifeAgentFailureStages.Material,
            new InvalidOperationException($"Patient material input rejected: {reason}."),
            retryable,
            publicMessage: publicMessage);
    }

    private sealed record RequestedMaterial(
        long MaterialId,
        SaveLifeAgentMaterialReference Reference);

    private sealed record PreparedMaterial(
        SaveLifeEnrollmentMaterial Material,
        SaveLifeAgentMaterialReference Reference,
        string Kind,
        string ContentType);

    private sealed record PreparedMedia(string DataUrl, long ActualBytes);
}

public sealed class SaveLifeAgentPreparedMaterialInput : IAsyncDisposable
{
    public static SaveLifeAgentPreparedMaterialInput Empty { get; } = new([], [], [], 0, 0, 0);

    private IAsyncDisposable? _requestLease;

    public SaveLifeAgentPreparedMaterialInput(
        IReadOnlyList<SaveLifeAgentModelContentPart> content,
        IReadOnlyList<SaveLifeAgentMaterialReference> references,
        long rawMediaBytes,
        int documentCharacters,
        long estimatedSerializedBytes,
        IAsyncDisposable? requestLease = null)
        : this(
            content,
            references,
            [],
            rawMediaBytes,
            documentCharacters,
            estimatedSerializedBytes,
            requestLease)
    {
    }

    internal SaveLifeAgentPreparedMaterialInput(
        IReadOnlyList<SaveLifeAgentModelContentPart> content,
        IReadOnlyList<SaveLifeAgentMaterialReference> references,
        IReadOnlyList<SaveLifeAgentPreparedMaterialEvidence> evidenceSources,
        long rawMediaBytes,
        int documentCharacters,
        long estimatedSerializedBytes,
        IAsyncDisposable? requestLease = null)
    {
        Content = content.ToArray();
        References = references.ToArray();
        EvidenceSources = evidenceSources.ToArray();
        RawMediaBytes = rawMediaBytes;
        DocumentCharacters = documentCharacters;
        EstimatedSerializedBytes = estimatedSerializedBytes;
        _requestLease = requestLease;
    }

    public IReadOnlyList<SaveLifeAgentModelContentPart> Content { get; }

    public IReadOnlyList<SaveLifeAgentMaterialReference> References { get; }

    internal IReadOnlyList<SaveLifeAgentPreparedMaterialEvidence> EvidenceSources { get; }

    public long RawMediaBytes { get; }

    public int DocumentCharacters { get; }

    public long EstimatedSerializedBytes { get; }

    public async ValueTask DisposeAsync()
    {
        var requestLease = Interlocked.Exchange(ref _requestLease, null);
        if (requestLease != null)
            await requestLease.DisposeAsync();
    }
}

internal sealed record SaveLifeAgentPreparedMaterialEvidence(
    long MaterialId,
    string Kind,
    string? ExtractedText);

public sealed class SaveLifeAgentMaterialFileReader(
    ISaveLifeFileAccessService fileAccessService) : ISaveLifeAgentMaterialFileReader, ITransient
{
    private readonly ISaveLifeFileAccessService _fileAccessService = fileAccessService;

    public async Task<Stream> OpenReadAsync(
        long fileId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stream = await _fileAccessService.OpenReadAsync(fileId);
        cancellationToken.ThrowIfCancellationRequested();
        return stream;
    }
}

public sealed record SaveLifeAgentMaterialAccessScope(
    string ExternalUserId,
    long PatientProfileId,
    long? EnrollmentRecordId);
