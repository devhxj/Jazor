namespace Jolt.Extensions;

internal interface IExtension
{
    ExtensionMetadata Metadata { get; }

    ValueTask InitializeAsync(
        ExtensionContext context,
        CancellationToken cancellationToken);

    ValueTask ActivateAsync(CancellationToken cancellationToken);

    ValueTask DeactivateAsync(CancellationToken cancellationToken);
}
