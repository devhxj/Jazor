namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMaterialTextExtractionLimits
{
    public const long MaximumInputBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;

    public const int TotalTimeoutSeconds = 30;

    public const int MaximumExtractedTextCharacters = 12_000;

    public const int MaximumDocxEntries = 256;

    public const int MaximumDocxEntryNameCharacters = 256;

    public const long MaximumDocxXmlEntryBytes = 4 * 1024 * 1024;

    public const long MaximumDocxXmlTotalBytes = 8 * 1024 * 1024;

    public const int MaximumDocxCompressionRatio = 100;

    public const int MaximumXmlDepth = 64;

    public const int MaximumPdfPages = 200;

    public const int MaximumPdfLetters = 250_000;

    public const int MaximumPdfStackDepth = 64;
}
