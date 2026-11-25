using Jero.Core;
using Jero.Core.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeSysFileStorageConsistencyTests
{
    [Fact]
    public async Task UploadAsync_WhenMetadataPersistenceFails_DeletesStoredFileAndRethrowsOriginalError()
    {
        var calls = new List<string>();
        var file = new SysFile { Id = 701 };
        var databaseError = new InvalidOperationException("database unavailable");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SysFileStorageConsistency.UploadAsync(
                () =>
                {
                    calls.Add("store");
                    return Task.FromResult(file);
                },
                _ =>
                {
                    calls.Add("persist");
                    return Task.FromException(databaseError);
                },
                _ =>
                {
                    calls.Add("cleanup");
                    return Task.CompletedTask;
                }));

        Assert.Same(databaseError, error);
        Assert.Equal(["store", "persist", "cleanup"], calls);
    }

    [Fact]
    public async Task UploadAsync_WhenPersistenceAndCompensationFail_ReportsBothFailuresAndStoredFile()
    {
        var file = new SysFile { Id = 702 };
        var databaseError = new InvalidOperationException("database unavailable");
        var storageError = new IOException("storage unavailable");

        var error = await Assert.ThrowsAsync<SysFileUploadCompensationException>(() =>
            SysFileStorageConsistency.UploadAsync(
                () => Task.FromResult(file),
                _ => Task.FromException(databaseError),
                _ => Task.FromException(storageError)));

        Assert.Same(file, error.File);
        Assert.Same(databaseError, error.PersistenceError);
        Assert.Same(storageError, error.CompensationError);
    }

    [Fact]
    public async Task DeleteAsync_WhenStorageDeletionFails_DoesNotDeleteMetadata()
    {
        var calls = new List<string>();
        var file = new SysFile { Id = 703 };

        await Assert.ThrowsAsync<IOException>(() => SysFileStorageConsistency.DeleteAsync(
            file,
            _ =>
            {
                calls.Add("storage");
                return Task.FromException(new IOException("storage unavailable"));
            },
            _ =>
            {
                calls.Add("metadata");
                return Task.CompletedTask;
            }));

        Assert.Equal(["storage"], calls);
    }

    [Fact]
    public async Task DeleteAsync_RemovesStorageBeforeMetadata()
    {
        var calls = new List<string>();
        var file = new SysFile { Id = 704 };

        await SysFileStorageConsistency.DeleteAsync(
            file,
            _ =>
            {
                calls.Add("storage");
                return Task.CompletedTask;
            },
            _ =>
            {
                calls.Add("metadata");
                return Task.CompletedTask;
            });

        Assert.Equal(["storage", "metadata"], calls);
    }

    [Fact]
    public async Task DeleteIfExistsAsync_WhenMetadataWasAlreadyRemoved_IsSuccessfulNoOp()
    {
        var storageDeleteCount = 0;
        var metadataDeleteCount = 0;

        var deleted = await SysFileStorageConsistency.DeleteIfExistsAsync(
            () => Task.FromResult<SysFile?>(null),
            _ =>
            {
                storageDeleteCount++;
                return Task.CompletedTask;
            },
            _ =>
            {
                metadataDeleteCount++;
                return Task.CompletedTask;
            });

        Assert.False(deleted);
        Assert.Equal(0, storageDeleteCount);
        Assert.Equal(0, metadataDeleteCount);
    }
}
