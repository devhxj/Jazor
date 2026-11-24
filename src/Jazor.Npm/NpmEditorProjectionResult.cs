namespace Jazor.Npm;

// Private response shared by the packaged apphost and the C# editor generator.
internal sealed record NpmEditorProjectionResult(string? Source, NpmEditorProjectionError[] Errors);
internal sealed record NpmEditorProjectionError(string Namespace, string? Member, string Message);
