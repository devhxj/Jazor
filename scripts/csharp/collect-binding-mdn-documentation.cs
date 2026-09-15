#!/usr/bin/env dotnet run
#:package AngleSharp@1.3.0
#:package Microsoft.CodeAnalysis.CSharp@5.11.0-1.26425.128
#:property PublishAot=false

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// MDN's rendered original prose includes resolved spec macros and parameter definitions.
// Cache responses locally, and freeze only excerpts actually attached to binding symbols.
var repo = Directory.GetCurrentDirectory();
var cache = Path.Combine(repo, ".tmp", "mdn-documentation");
Directory.CreateDirectory(cache);
var inventory = JsonNode.Parse(File.ReadAllText("src/ECMAScript/webidl/webidl.inventory.json"))!;
var requests = new Dictionary<string, string>(StringComparer.Ordinal);
foreach (var file in inventory["files"]!.AsArray())
foreach (var declaration in file!["declarations"]!.AsArray())
{
    var owner = declaration!["name"]?.GetValue<string>();
    if (owner is null) continue;
    if (declaration["documentation"]?["prose"] is null) requests.TryAdd(owner, "Web/API/" + owner);
    var payload = declaration["payload"]!;
    if (payload["members"] is not JsonArray members) continue;
    var docs = declaration["memberDocumentation"]?.AsArray();
    for (var i = 0; i < members.Count; i++)
    {
        var member = members[i]!;
        if (docs?.Any(doc => doc!["memberIndex"]!.GetValue<int>() == i && doc["documentation"]?["prose"] is not null) == true) continue;
        var type = member["type"]?.GetValue<string>();
        var name = member["name"]?.GetValue<string>();
        if (type == "constructor") name = owner;
        if (string.IsNullOrWhiteSpace(name) || type is "const" or "field") continue;
        if (owner == "CSSStyleProperties" || file["fileName"]?.GetValue<string>() == "jazor.webref-css-properties.idl")
        {
            var property = Regex.Replace(name, "[A-Z]", match => "-" + match.Value.ToLowerInvariant());
            requests.TryAdd(owner + "." + name, "Web/CSS/" + property);
        }
        else requests.TryAdd(owner + "." + name, "Web/API/" + owner + "/" + name);
    }
}
requests["WebGL_API.Constants"] = "Web/API/WebGL_API/Constants";
// Handwritten ECMAScript primitives use the same source, including constructor option domains.
foreach(var name in new[]{"Array","ArrayBuffer","SharedArrayBuffer","BigInt","Boolean","Date","Error","EvalError","RangeError","ReferenceError","SyntaxError","TypeError","URIError","AggregateError","JSON","Map","Set","WeakMap","WeakSet","Promise","Reflect","RegExp","String","Symbol","Number","Math","Intl"})
    requests.TryAdd(name, "Web/JavaScript/Reference/Global_Objects/" + name);
foreach(var name in new[]{"Collator","DateTimeFormat","NumberFormat","PluralRules","RelativeTimeFormat","ListFormat","Locale","Segmenter","DisplayNames","DurationFormat"})
{
    requests["Intl."+name] = "Web/JavaScript/Reference/Global_Objects/Intl/"+name;
    requests["Intl."+name+"."+name] = "Web/JavaScript/Reference/Global_Objects/Intl/"+name+"/"+name;
}
foreach(var path in Directory.EnumerateFiles("src/ECMAScript/internal","*.cs"))
{
    var root=CSharpSyntaxTree.ParseText(File.ReadAllText(path),CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)).GetRoot();
    foreach(var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
    {
        if(method.GetLeadingTrivia().ToString().Contains("<summary>"))continue;
        var owner=string.Join(".",method.Ancestors().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(x=>x.Identifier.ValueText));
        var name=method.Identifier.ValueText;
        var attr=method.AttributeLists.SelectMany(a=>a.Attributes).FirstOrDefault(a=>a.Name.ToString()=="Description");
        if(attr?.ArgumentList?.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax literal && literal.Token.ValueText.StartsWith("@#"))
            name=literal.Token.ValueText[2..];
        else name=char.ToLowerInvariant(name[0])+name[1..];
        requests.TryAdd(owner+"."+name,"Web/JavaScript/Reference/Global_Objects/"+owner.Replace('.','/')+"/"+name);
    }
}
var output = new ConcurrentDictionary<string, Documentation>(StringComparer.Ordinal);
using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
client.DefaultRequestHeaders.UserAgent.ParseAdd("Jazor-Documentation-Collector/1.0");
var processed = 0;
var failures = new ConcurrentBag<string>();
Console.WriteLine($"Collecting {requests.Count} MDN pages (cached responses reused).");
await Parallel.ForEachAsync(requests, new ParallelOptions { MaxDegreeOfParallelism = 12 }, async (request, token) =>
{
    var path = Path.Combine(cache, request.Value.Replace('/', '_') + ".json");
    try
    {
        string json;
        if (File.Exists(path)) json = await File.ReadAllTextAsync(path, token);
        else
        {
            using var response = await client.GetAsync("https://developer.mozilla.org/en-US/docs/" + request.Value + "/index.json", token);
            if (response.StatusCode == HttpStatusCode.NotFound) { await File.WriteAllTextAsync(path, "null", token); return; }
            response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync(token);
            await File.WriteAllTextAsync(path, json, token);
        }
        if (json == "null") return;
        var page = JsonNode.Parse(json)!;
        if(page["doc"]?["body"] is not JsonArray body) return;
        var parser = new HtmlParser();
        var first = body.FirstOrDefault(row => row!["type"]?.GetValue<string>() == "prose");
        var summary = Plain(first?["value"]?["content"]?.GetValue<string>(), parser);
        var parameters = new SortedDictionary<string,string>(StringComparer.Ordinal);
        string? returns = null;
        foreach(var section in body.Where(row=>row!["type"]?.GetValue<string>()=="prose"))
        {
            var id=section!["value"]?["id"]?.GetValue<string>();
            var html=section["value"]?["content"]?.GetValue<string>();
            if (id == "return_value") returns=Plain(html,parser);
            if (id == "parameters" && html is not null)
            {
                var dom = parser.ParseDocument(html);
                foreach(var dt in dom.QuerySelectorAll("dt"))
                    if(dt.NextElementSibling is {} dd && dd.TagName=="DD")
                        parameters[dt.QuerySelector("code")?.TextContent ?? dt.TextContent.Trim()] = Normalize(dd.TextContent);
            }
        }
        var href="https://developer.mozilla.org"+page["url"]!.GetValue<string>();
        if(request.Key.StartsWith("Intl.",StringComparison.Ordinal))
        foreach(var section in body.Where(row=>row!["type"]?.GetValue<string>()=="prose"))
        {
            var dom=parser.ParseDocument(section!["value"]!["content"]!.GetValue<string>());
            foreach(var dt in dom.QuerySelectorAll("dt"))
            {
                if(dt.NextElementSibling is not {} dd || dd.TagName!="DD")continue;
                var option=dt.QuerySelector("code")?.TextContent.Trim('"');
                if(option is null)continue;
                output.TryAdd(request.Key+"#"+option,new(Normalize(dd.TextContent),href,new(),null));
                foreach(var choice in dd.QuerySelectorAll("dt"))
                {
                    if(choice.NextElementSibling is not {} definition||definition.TagName!="DD")continue;
                    var literal=choice.QuerySelector("code")?.TextContent.Trim('"');
                    if(literal is not null)output.TryAdd(request.Key+"#"+option+"."+literal,new(Normalize(definition.TextContent),href,new(),null));
                }
            }
        }
        if(!string.IsNullOrWhiteSpace(summary))output[request.Key]=new(summary,href,parameters,returns);
        if(request.Key=="WebGL_API.Constants")
        foreach(var section in body.Where(row=>row!["type"]?.GetValue<string>()=="prose"))
        {
            var dom=parser.ParseDocument(section!["value"]!["content"]!.GetValue<string>());
            foreach(var row in dom.QuerySelectorAll("tr"))
            {
                var cells=row.QuerySelectorAll("td");
                if(cells.Length<3)continue;
                var name=cells[0].TextContent.Trim();
                if(!Regex.IsMatch(name,@"^[A-Z][A-Z0-9_]+$"))continue;
                var prose=Normalize(cells[2].TextContent);
                if(prose.Length>0)output["WebGL."+name]=new(prose,href,new(),null);
            }
        }
    }
    catch(Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
    { failures.Add(request.Key+": "+exception.Message); }
    finally { var count=Interlocked.Increment(ref processed);if(count%100==0)Console.WriteLine($"{count}/{requests.Count}; {output.Count} descriptions; {failures.Count} requests failed."); }
});
var target="src/ECMAScript.WebIDL.Generator/documentation/mdn.json";
Directory.CreateDirectory(Path.GetDirectoryName(target)!);
File.WriteAllText(target,JsonSerializer.Serialize(new{
    source="https://github.com/mdn/content",license="CC-BY-SA-2.5",retrievedOn=DateTime.UtcNow.ToString("yyyy-MM-dd"),
    descriptions=output.OrderBy(x=>x.Key,StringComparer.Ordinal).ToDictionary(x=>x.Key,x=>x.Value)
},new JsonSerializerOptions{WriteIndented=true,Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping})+"\n",new UTF8Encoding(false));
File.WriteAllLines(Path.Combine(cache,"failures.txt"),failures.Order(StringComparer.Ordinal));
Console.WriteLine($"Frozen {output.Count} descriptions in {target}; {failures.Count} network failures.");

static string? Plain(string? html,HtmlParser parser)
{
    if(html is null)return null;
    var document=parser.ParseDocument(html);
    foreach(var node in document.QuerySelectorAll("svg,script,interactive-example"))node.Remove();
    return Normalize(document.Body!.TextContent);
}
static string Normalize(string text)=>Regex.Replace(text,@"\s+"," ").Trim();
record Documentation(string Summary,string Href,SortedDictionary<string,string> Parameters,string? Returns);
