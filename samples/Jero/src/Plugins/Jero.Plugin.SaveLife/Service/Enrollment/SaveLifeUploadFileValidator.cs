using System.Security.Cryptography;

namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeUploadFileValidator
{
    private static readonly Dictionary<string, string> StandardContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".png"] = "image/png",
        [".gif"] = "image/gif",
        [".pdf"] = "application/pdf",
        [".doc"] = "application/msword",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".mp4"] = "video/mp4",
        [".mov"] = "video/quicktime"
    };

    private static readonly Dictionary<string, HashSet<string>> CompatibleContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/jpg" },
        [".png"] = new(StringComparer.OrdinalIgnoreCase) { "image/png" },
        [".gif"] = new(StringComparer.OrdinalIgnoreCase) { "image/gif" },
        [".pdf"] = new(StringComparer.OrdinalIgnoreCase) { "application/pdf" },
        [".doc"] = new(StringComparer.OrdinalIgnoreCase) { "application/msword", "application/octet-stream" },
        [".docx"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/wps-office.docx"
        },
        [".mp4"] = new(StringComparer.OrdinalIgnoreCase) { "video/mp4" },
        [".mov"] = new(StringComparer.OrdinalIgnoreCase) { "video/quicktime", "video/mp4" }
    };

    public static string ValidateAndResolveContentType(IFormFile file, string extension)
    {
        if (!StandardContentTypes.TryGetValue(extension, out var contentType))
            throw Oops.Oh("文件格式不支持").StatusCode(400);

        if (!IsContentTypeCompatible(extension, file.ContentType))
            throw Oops.Oh("文件 Content-Type 与扩展名不匹配").StatusCode(400);

        if (!HasExpectedSignature(file, extension))
            throw Oops.Oh("文件内容与扩展名不匹配，请重新选择正确格式的文件").StatusCode(400);

        return contentType;
    }

    public static string ComputeStableHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool IsContentTypeCompatible(string extension, string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType) ||
            string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
            return true;

        return CompatibleContentTypes.TryGetValue(extension, out var allowed) &&
            allowed.Contains(contentType);
    }

    private static bool HasExpectedSignature(IFormFile file, string extension)
    {
        return extension switch
        {
            ".jpg" => StartsWith(file, 0xFF, 0xD8, 0xFF),
            ".png" => StartsWith(file, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
            ".gif" => StartsWithAscii(file, "GIF87a") || StartsWithAscii(file, "GIF89a"),
            ".pdf" => StartsWithAscii(file, "%PDF-"),
            ".doc" => StartsWith(file, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1),
            ".docx" => IsWordOpenXmlPackage(file),
            ".mp4" => IsIsoBaseMediaFile(file, ".mp4"),
            ".mov" => IsIsoBaseMediaFile(file, ".mov"),
            _ => false
        };
    }

    private static bool StartsWith(IFormFile file, params byte[] expected)
    {
        var header = ReadHeader(file, expected.Length);
        return header.Length >= expected.Length && expected.SequenceEqual(header.Take(expected.Length));
    }

    private static bool StartsWithAscii(IFormFile file, string expected)
    {
        return StartsWith(file, Encoding.ASCII.GetBytes(expected));
    }

    private static byte[] ReadHeader(IFormFile file, int length)
    {
        using var stream = file.OpenReadStream();
        var buffer = new byte[length];
        var read = stream.Read(buffer, 0, buffer.Length);
        if (read == buffer.Length)
            return buffer;

        return buffer.Take(read).ToArray();
    }

    private static bool IsWordOpenXmlPackage(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            return archive.GetEntry("[Content_Types].xml") != null &&
                archive.Entries.Any(u => u.FullName.StartsWith("word/", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private static bool IsIsoBaseMediaFile(IFormFile file, string extension)
    {
        var header = ReadHeader(file, 16);
        if (header.Length < 12)
            return false;

        var boxType = Encoding.ASCII.GetString(header, 4, 4);
        if (!string.Equals(boxType, "ftyp", StringComparison.Ordinal))
            return false;

        var brand = Encoding.ASCII.GetString(header, 8, 4);
        if (extension == ".mov")
            return string.Equals(brand, "qt  ", StringComparison.Ordinal) ||
                string.Equals(brand, "mp42", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(brand, "isom", StringComparison.OrdinalIgnoreCase);

        return brand is "isom" or "iso2" or "mp41" or "mp42" or "avc1" or "M4V ";
    }
}
