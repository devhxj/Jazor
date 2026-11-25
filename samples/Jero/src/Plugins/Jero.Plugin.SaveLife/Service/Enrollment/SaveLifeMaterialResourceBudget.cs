namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeMaterialResourceBudget
{
    public const long MultipartOverheadBytes = 256 * 1024;

    public const long UploadRequestBytes =
        SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + MultipartOverheadBytes;

    public const int UploadTimeoutMilliseconds = 120 * 1000;

    public const int FormValueCountLimit = 8;

    public const int FormKeyLengthLimit = 128;

    public const int FormValueLengthLimit = 2048;

    public const int MultipartBoundaryLengthLimit = 128;

    public const int MultipartHeadersCountLimit = 16;

    public const int MultipartHeadersLengthLimit = 8192;

    public const int MaterialReviewMediaCount = 9;

    public const long MaterialReviewMediaBytes = SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes;

    public const int MaterialReviewDocumentCharacters =
        MaterialReviewDocumentCharactersPerFile * MaterialReviewMediaCount;

    public const int MaterialReviewDocumentCharactersPerFile = 16_000;

    public const long ModelRequestFixedOverheadBytes = 256 * 1024;

    public const long MaterialReviewEncodedMediaBytes =
        4 * ((MaterialReviewMediaBytes + 2) / 3);

    public const long MaterialReviewSerializedRequestBytes =
        MaterialReviewEncodedMediaBytes + ModelRequestFixedOverheadBytes;

    public const int MaterialReviewProcessConcurrency = 2;

    public const int MaterialReviewQueueLength = 8;

    public const int MaterialReviewQueueTimeoutMilliseconds = 5 * 1000;

    public const int MaterialReviewAccountRequestsPerMinute = 6;
}
