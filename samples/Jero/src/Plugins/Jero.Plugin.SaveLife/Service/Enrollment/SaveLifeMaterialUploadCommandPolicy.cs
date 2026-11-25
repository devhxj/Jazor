using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Buffers;

namespace Jero.Plugin.SaveLife.Service;

public sealed record SaveLifeMaterialUploadPayload(
    long PatientProfileId,
    long EnrollmentRecordId,
    string ContentSha256,
    long ActualSizeBytes,
    string FileName,
    string FileSuffix,
    string ContentType,
    string MaterialType,
    string? Remark);

public static class SaveLifeMaterialUploadCommandPolicy
{
    public const string PayloadHashAlgorithm = "sha256-v1";

    public static SaveLifeAgentPayloadHash ComputePayloadHash(SaveLifeMaterialUploadPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var canonical = JsonSerializer.SerializeToUtf8Bytes(payload);
        return new SaveLifeAgentPayloadHash(
            PayloadHashAlgorithm,
            Convert.ToHexString(SHA256.HashData(canonical)));
    }

    public static async Task<SaveLifeMaterialContentFingerprint> ComputeContentFingerprintAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length <= 0)
            throw Oops.Oh("材料文件不能为空").StatusCode(400);
        if (file.Length > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes)
            throw Oops.Oh("材料文件不能超过20MB").StatusCode(413);

        await using var stream = file.OpenReadStream();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        long actualSize = 0;
        try
        {
            while (true)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (read == 0)
                    break;
                actualSize = checked(actualSize + read);
                if (actualSize > SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes)
                    throw Oops.Oh("材料文件实际内容不能超过20MB").StatusCode(413);
                hash.AppendData(buffer, 0, read);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }

        if (actualSize != file.Length)
            throw Oops.Oh("材料文件声明大小与实际内容不一致").StatusCode(400);
        return new SaveLifeMaterialContentFingerprint(
            Convert.ToHexString(hash.GetHashAndReset()),
            actualSize);
    }
}

public sealed record SaveLifeMaterialContentFingerprint(string ContentSha256, long ActualSizeBytes);
