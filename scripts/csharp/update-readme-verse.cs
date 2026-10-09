#!/usr/bin/env dotnet run

using System.Text;
using System.Xml.Linq;

// GitHub's image proxy currently cannot fetch the provider. Keep the banner in the
// repository so both README languages display it, and refresh from the same source.
const string source = "https://v2.jinrishici.com/one.svg?font-size=20&spacing=2&color=Chocolate";
using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
var svg = await client.GetStringAsync(source);
var document = XDocument.Parse(svg);
if (document.Root?.Name != XName.Get("svg", "http://www.w3.org/2000/svg"))
    throw new InvalidOperationException("The verse provider did not return an SVG.");

var output = Path.GetFullPath("docs/assets/todays-verse.svg");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
await File.WriteAllTextAsync(output, svg.TrimEnd() + "\n", new UTF8Encoding(false));
Console.WriteLine($"Updated {output}");
