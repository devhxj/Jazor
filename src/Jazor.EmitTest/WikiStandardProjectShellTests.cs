using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Jazor.EmitTest;

[TestClass]
public sealed class WikiStandardProjectShellTests
{
    [TestMethod]
    [DataRow("Development", "/jazor/entry.js", true)]
    [DataRow("Production", "/jazor/dist/bundle.js", false)]
    public async Task Shell_UsesStandardEntryWithoutRuntimeManifest(string environment, string entry, bool hmr)
    {
        var root = Path.Combine(Path.GetTempPath(), "jazor-wiki-shell-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "host"));
        Directory.CreateDirectory(Path.Combine(root, "jazor", "dist"));
        // Both source and build output coexist in a standard project. Environment selects the entry.
        await File.WriteAllTextAsync(Path.Combine(root, "jazor", "entry.js"), "export {};");
        await File.WriteAllTextAsync(Path.Combine(root, "jazor", "dist", "bundle.js"), "export {};");
        var repository = new DirectoryInfo(AppContext.BaseDirectory);
        while (repository is not null && !File.Exists(Path.Combine(repository.FullName, "Jazor.slnx")))
            repository = repository.Parent;
        File.Copy(Path.Combine(repository!.FullName, "samples", "Wiki", "host", "index.template.html"),
            Path.Combine(root, "host", "index.template.html"));
        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = root,
                EnvironmentName = environment
            });
            builder.WebHost.UseTestServer();
            await using var app = builder.Build();
            app.UsePathBase("/docs");
            app.Run(context => Wiki.WikiHostShell.WriteHtmlAsync(context));
            await app.StartAsync();
            using var client = app.GetTestClient();
            var response = await client.GetAsync("/docs/");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            StringAssert.Contains(html, "src=\"/docs" + entry + "\"");
            Assert.AreEqual(hmr, html.Contains("/docs/jazor/@vite/client", StringComparison.Ordinal));
            Assert.IsFalse(html.Contains("importmap", StringComparison.Ordinal));
            Assert.IsFalse(html.Contains("__WIKI_", StringComparison.Ordinal));
            Assert.IsTrue(response.Headers.Contains("Content-Security-Policy"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
