namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialNativeFileEventRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorNativeCallbacks_ReadFileSelectionAndDropIntoMultipart()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/NativeFileEvents.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            @using Microsoft.AspNetCore.Components.Forms
            <input id="files" type="file" multiple onchange="@(EventCallback.Factory.Create<InputFileChangeEventArgs>(this, HandleSelection))" />
            <div id="drop" @ondrop="HandleDrop"
                 @ondrop:preventDefault @ondragover:preventDefault>Drop files</div>
            <span id="name">@Name</span>
            <span id="size">@Size</span>
            <span id="type">@ContentType</span>
            <span id="count">@Count</span>
            <span id="file-count">@SelectedFileCount</span>
            <button id="export" @onclick="CreateDownload">Export</button>
            <span id="download-name">@DownloadName</span>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components.Forms;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/native-file-events")]
            public partial class NativeFileEvents : ComponentBase, IVueComponent
            {
                private string Name { get; set; } = "none";
                private string Size { get; set; } = "0";
                private string ContentType { get; set; } = "none";
                private uint Count { get; set; }
                private int SelectedFileCount { get; set; }
                private string DownloadName { get; set; } = "none";

                private void CreateDownload()
                {
                    var blob = new Blob(["report"]);
                    var file = new FileRef([blob], "report.txt");
                    var url = URL.CreateObjectURL(file);
                    DownloadName = file.Name;
                    URL.RevokeObjectURL(url);
                }

                private void HandleSelection(InputFileChangeEventArgs args)
                {
                    SelectedFileCount = args.FileCount;
                    Capture(args.Files);
                }

                private void HandleDrop(DragEventArgs args)
                    => Capture(args.Files);

                private void Capture(FileList? files)
                {
                    Count = files?.Length ?? 0;
                    var first = files?.GetItem(0);
                    Name = first?.Name ?? "none";
                    Size = first?.Size.ToString() ?? "0";
                    ContentType = first?.Type ?? "none";
                    if ((files?.Length ?? 0) == 0)
                        return;

                    var form = new FormData();
                    for (uint index = 0; index < files.Length; index++)
                    {
                        var file = files.GetItem(index)!;
                        form.Append("files", file, file.Name);
                    }
                    Global.Window.Fetch("/upload", new RequestInit { Method = "POST", Body = new BodyInit((XMLHttpRequestBodyInit)form) });
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NativeFileEvents");

        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create<InputFileChangeEventArgs>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.DragEventArgs>", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("captureChangeEvent", StringComparison.Ordinal), observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "onChange", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onDrop", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "dataTransfer", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/native-file-events.js",
            observation.ModuleText,
            "official-native-file-events-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/native-file-events.js";

            function find(node, id) {
              if (Array.isArray(node)) {
                for (const child of node) {
                  const found = find(child, id);
                  if (found) return found;
                }
                return null;
              }
              if (!node || typeof node !== "object") return null;
              return node.props?.id === id ? node : find(node.children, id);
            }

            function fileList(files) {
              return { length: files.length, item: index => files[index] ?? null };
            }

            test("selection and drop preserve native File objects, metadata, multipart and cancellation", async () => {
              const uploads = [];
              globalThis.fetch = (url, options) => {
                uploads.push({ url, options });
                return Promise.resolve(new Response("{}"));
              };
              globalThis.window = globalThis;
              const first = new File(["abc"], "first.txt", { type: "text/plain" });
              const second = new File(["12345"], "second.csv", { type: "text/csv" });
              const render = component.setup({}, { slots: {} });
              const input = find(render(), "files");
              await Promise.resolve(input.props.onChange({ target: { files: fileList([first, second]) } }));
              let current = render();
              assert.equal(find(current, "name").children, "first.txt");
              assert.equal(find(current, "size").children, "3");
              assert.equal(find(current, "type").children, "text/plain");
              assert.deepEqual(find(current, "count").children, [2]);
              assert.deepEqual(find(current, "file-count").children, [2]);
              assert.equal(uploads.length, 1);
              assert.equal(uploads[0].url, "/upload");
              assert.equal(uploads[0].options.method, "POST");
              assert.ok(uploads[0].options.body instanceof FormData);
              assert.equal(uploads[0].options.headers, undefined);
              const posted = uploads[0].options.body.getAll("files");
              assert.deepEqual(posted.map(file => file.name), ["first.txt", "second.csv"]);
              assert.equal(await posted[0].text(), "abc");
              assert.equal(await posted[1].text(), "12345");

              let prevented = 0;
              const drop = find(current, "drop");
              drop.props.onDragover({ preventDefault() { prevented++; } });
              await Promise.resolve(drop.props.onDrop({
                dataTransfer: { files: fileList([second]) },
                preventDefault() { prevented++; }
              }));
              current = render();
              assert.equal(find(current, "name").children, "second.csv");
              assert.equal(find(current, "size").children, "5");
              assert.equal(find(current, "type").children, "text/csv");
              assert.deepEqual(find(current, "count").children, [1]);
              assert.equal(prevented, 2);
              assert.equal(uploads.length, 2);

              const created = [];
              const revoked = [];
              URL.createObjectURL = file => {
                assert.ok(file instanceof File);
                created.push(file);
                return "blob:export";
              };
              URL.revokeObjectURL = url => revoked.push(url);
              await Promise.resolve(find(current, "export").props.onClick());
              assert.equal(find(render(), "download-name").children, "report.txt");
              assert.equal(await created[0].text(), "report");
              assert.deepEqual(revoked, ["blob:export"]);

              await Promise.resolve(input.props.onChange({ target: { files: fileList([]) } }));
              await Promise.resolve(drop.props.onDrop({ dataTransfer: null, preventDefault() {} }));
              current = render();
              assert.equal(find(current, "name").children, "none");
              assert.equal(find(current, "size").children, "0");
              assert.deepEqual(find(current, "count").children, [0]);
              assert.deepEqual(find(current, "file-count").children, [0]);
              assert.equal(uploads.length, 2);
            });
            """);
    }
}
