using System.Security.Cryptography;
using System.Text;

namespace Jazor.NodeHost;

/// <summary>Loads an assembly-owned CommonJS module for in-process NodeApi operations.</summary>
internal sealed class EmbeddedNodeModule
{
    private readonly string _source;
    private readonly string _relativePath;

    public EmbeddedNodeModule(Type owner, string filename)
    {
        using var source = owner.Assembly.GetManifestResourceStream(owner.Namespace + "." + filename)
            ?? throw new InvalidOperationException($"Missing embedded module '{filename}' in {owner.Assembly.GetName().Name}.");
        using var reader = new StreamReader(source);
        _source = reader.ReadToEnd();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_source)));
        _relativePath = Path.Combine(".jazor", "nodeapi", hash, filename);
    }

    public string Materialize(string projectDirectory)
    {
        var path = Path.Combine(projectDirectory, _relativePath);
        if (File.Exists(path))
            return path;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, _source);
            File.Move(temporary, path);
        }
        catch (IOException) when (File.Exists(path))
        {
            // Another compiler published the same complete content-addressed module.
        }
        finally
        {
            File.Delete(temporary);
        }
        return path;
    }

    // Deployed SSR workers can execute their module from a read-only directory.
    public Task<string> InvokeAsync(NodeEnvironment environment, string projectDirectory,
        string export, CancellationToken cancellationToken, params string[] arguments)
        => environment.InvokeSourceAsync(_source, Path.Combine(projectDirectory, _relativePath), export, cancellationToken, arguments);
}
