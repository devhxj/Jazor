using System.Runtime.ExceptionServices;

namespace Jero.Core.Service;

internal static class SysFileStorageConsistency
{
    public static async Task<SysFile> UploadAsync(
        Func<Task<SysFile>> upload,
        Func<SysFile, Task> persistMetadata,
        Func<SysFile, Task> compensateStorage)
    {
        var file = await upload();
        try
        {
            await persistMetadata(file);
            return file;
        }
        catch (Exception persistenceError)
        {
            try
            {
                await compensateStorage(file);
            }
            catch (Exception compensationError)
            {
                throw new SysFileUploadCompensationException(
                    file,
                    persistenceError,
                    compensationError);
            }

            ExceptionDispatchInfo.Capture(persistenceError).Throw();
            throw;
        }
    }

    public static async Task DeleteAsync(
        SysFile file,
        Func<SysFile, Task> deleteStorage,
        Func<SysFile, Task> deleteMetadata)
    {
        await deleteStorage(file);
        await deleteMetadata(file);
    }

    public static async Task<bool> DeleteIfExistsAsync(
        Func<Task<SysFile?>> loadMetadata,
        Func<SysFile, Task> deleteStorage,
        Func<SysFile, Task> deleteMetadata)
    {
        var file = await loadMetadata();
        if (file == null)
            return false;

        await DeleteAsync(file, deleteStorage, deleteMetadata);
        return true;
    }
}

internal sealed class SysFileUploadCompensationException : Exception
{
    public SysFileUploadCompensationException(
        SysFile file,
        Exception persistenceError,
        Exception compensationError)
        : base(
            "File metadata persistence and storage compensation both failed",
            new AggregateException(persistenceError, compensationError))
    {
        File = file;
        PersistenceError = persistenceError;
        CompensationError = compensationError;
    }

    public SysFile File { get; }

    public Exception PersistenceError { get; }

    public Exception CompensationError { get; }
}
