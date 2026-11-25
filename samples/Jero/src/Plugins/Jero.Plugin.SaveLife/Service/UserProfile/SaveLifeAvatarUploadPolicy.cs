namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAvatarUploadPolicy
{
    public const long MaxFileBytes = 5 * 1024 * 1024;

    public const long UploadRequestBytes = MaxFileBytes + 64 * 1024;

    public const int UploadTimeoutMilliseconds = 30 * 1000;

    public const int FormValueCountLimit = 2;

    public const int FormKeyLengthLimit = 64;

    public const int FormValueLengthLimit = 256;

    public const int MultipartBoundaryLengthLimit = 128;

    public const int MultipartHeadersCountLimit = 8;

    public const int MultipartHeadersLengthLimit = 4096;

    public static void Validate(IFormFile file)
    {
        if (file == null || file.Length <= 0)
            throw Oops.Oh("头像文件不能为空").StatusCode(400);

        if (file.Length > MaxFileBytes)
            throw Oops.Oh("头像文件不能超过5MB").StatusCode(413);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension == ".jpeg")
            extension = ".jpg";
        if (extension is not ".jpg" and not ".png")
            throw Oops.Oh("头像仅支持 JPEG 或 PNG 格式").StatusCode(400);

        SaveLifeUploadFileValidator.ValidateAndResolveContentType(file, extension);
    }
}
