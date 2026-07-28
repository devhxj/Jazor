namespace Jazor.Css;

public static partial class Css
{
    private const string RootMarker = "/*jazor-css:v1*/";
    private const string EntryMarkerPrefix = "/*jz:v1:";

    private static HTMLStyleElement? DomStyle;
    private static bool DomHydrated;

    private static void EnsureDomStyle()
    {
        if (DomHydrated)
            return;

        if (ECMAScript.Global.TypeOf(ECMAScript.Global.Document) == "undefined")
            return;

        var domDocument = ECMAScript.Global.Document;
        var existing = domDocument.GetElementById(StyleId);
        if (existing is null)
        {
            if (EntryIds.Length == 0)
                return;

            var head = domDocument.Head;
            if (head is null)
                Fail("Jazor.Css requires document.head to inject styles.");

            var style = (HTMLStyleElement)domDocument.CreateElement("style", new ElementCreationOptions());
            style.Id = StyleId;
            if (Nonce is not null)
                style.Nonce = Nonce;

            style.TextContent = RootMarker;
            head!.AppendChild(style);
            DomStyle = style;
            DomHydrated = true;

            for (var index = 0; index < EntryIds.Length; index++)
                AppendDomEntry(EntryIds[index], EntryBodies[index]);

            return;
        }

        if (existing.LocalName != "style")
            Fail("Jazor.Css StyleId '" + StyleId + "' is already used by a non-style element.");

        var existingStyle = (HTMLStyleElement)existing;
        var text = existingStyle.TextContent ?? "";
        if (!text.StartsWith(RootMarker))
            Fail("Jazor.Css StyleId '" + StyleId + "' is not owned by Jazor.Css.");

        if (Nonce is not null && existingStyle.Nonce != Nonce)
            Fail("Jazor.Css nonce does not match the existing style element.");

        DomStyle = existingStyle;
        AdoptDomEntries(domDocument, text);
        DomHydrated = true;
        HasRegistered = EntryIds.Length > 0;
    }

    private static void AdoptDomEntries(Document document, string text)
    {
        var memoryIds = EntryIds;
        var memoryBodies = EntryBodies;
        var adoptedIds = new Array<string>();
        var adoptedBodies = new Array<string>();
        var adoptedBodyById = new Map<string, string>();
        var offset = RootMarker.Length;

        while (offset < text.Length)
        {
            if (!text.Substring(offset).StartsWith(EntryMarkerPrefix))
                Fail("Jazor.Css style element contains a malformed entry marker.");

            var idStart = offset + EntryMarkerPrefix.Length;
            var idEnd = text.IndexOf(":", idStart);
            var headerEnd = idEnd < 0 ? -1 : text.IndexOf("*/", idEnd + 1);
            if (idEnd <= idStart || headerEnd < 0)
                Fail("Jazor.Css style element contains a malformed entry header.");

            var id = text.Substring(idStart, idEnd - idStart);
            var lengthText = text.Substring(idEnd + 1, headerEnd - idEnd - 1);
            var bodyLength = ParseEntryLength(lengthText);
            var bodyStart = headerEnd + 2;
            var bodyEnd = bodyStart + bodyLength;
            if (bodyEnd > text.Length)
                Fail("Jazor.Css style entry extends beyond the style text.");

            var body = text.Substring(bodyStart, bodyLength);
            if (adoptedBodyById.Has(id))
            {
                if (adoptedBodyById.Get(id) != body)
                    Fail("Jazor.Css style element contains one ID with different CSS bodies.");
            }
            else
            {
                adoptedBodyById.Set(id, body);
                adoptedIds.Push(id);
                adoptedBodies.Push(body);
            }

            offset = bodyEnd;
        }

        for (var index = 0; index < memoryIds.Length; index++)
        {
            var id = memoryIds[index];
            var body = memoryBodies[index];
            if (adoptedBodyById.Has(id))
            {
                if (adoptedBodyById.Get(id) != body)
                    Fail("A Jazor.Css hash collision was detected for '" + id + "'.");

                continue;
            }

            adoptedBodyById.Set(id, body);
            adoptedIds.Push(id);
            adoptedBodies.Push(body);
            AppendTextNode(document, FormatDomEntry(id, body));
        }

        EntryIds = adoptedIds;
        EntryBodies = adoptedBodies;
        BodyById.Clear();
        for (var index = 0; index < EntryIds.Length; index++)
            BodyById.Set(EntryIds[index], EntryBodies[index]);
    }

    private static int ParseEntryLength(string value)
    {
        if (value.Length == 0)
            Fail("Jazor.Css style entry length is empty.");

        var result = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var codeUnit = (int)value.CharCodeAt(index);
            if (codeUnit < 48 || codeUnit > 57)
                Fail("Jazor.Css style entry length is invalid.");

            result = result * 10 + codeUnit - 48;
        }

        return result;
    }

    private static void AppendDomEntry(string id, string body)
    {
        if (DomStyle is null)
            return;

        AppendTextNode(ECMAScript.Global.Document, FormatDomEntry(id, body));
    }

    private static string FormatDomEntry(string id, string body)
        => EntryMarkerPrefix + id + ":" + StringFn(body.Length) + "*/" + body;

    private static void AppendTextNode(Document document, string text)
    {
        if (DomStyle is null)
            return;

        DomStyle.AppendChild(document.CreateTextNode(text));
    }
}
