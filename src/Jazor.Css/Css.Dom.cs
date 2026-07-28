namespace Jazor.Css;

public static partial class Css
{
    private const string RootMarker = "/*jazor-css:v1*/";
    private const string EntryMarkerPrefix = "/*jz:v1:";

    private static void EnsureDomStyle(CssContext context)
    {
        if (context.Detached || context.DomHydrated)
            return;

        Document domDocument;
        Element? existing;
        Node insertionTarget;
        if (context.Target is null)
        {
            if (ECMAScript.Global.TypeOf(ECMAScript.Global.Document) == "undefined")
                return;

            domDocument = ECMAScript.Global.Document;
            existing = domDocument.GetElementById(context.StyleId);
            var head = domDocument.Head;
            if (head is null)
            {
                if (context.EntryIds.Length == 0)
                    return;

                Fail("Jazor.Css requires document.head to inject styles.");
            }

            insertionTarget = head!;
        }
        else
        {
            var ownerDocument = context.Target.OwnerDocument;
            if (ownerDocument is null)
                Fail("Jazor.Css target must belong to a document.");

            domDocument = ownerDocument!;
            existing = context.Target.GetElementById(context.StyleId);
            insertionTarget = context.Target;
        }

        if (existing is null)
        {
            if (context.EntryIds.Length == 0)
                return;

            var style = (HTMLStyleElement)domDocument.CreateElement("style", new ElementCreationOptions());
            style.Id = context.StyleId;
            if (context.Nonce is not null)
                style.Nonce = context.Nonce;

            style.TextContent = RootMarker;
            insertionTarget.AppendChild(style);
            context.DomStyle = style;
            context.DomDocument = domDocument;
            context.DomHydrated = true;

            for (var index = 0; index < context.EntryIds.Length; index++)
                AppendDomEntry(context, context.EntryIds[index], context.EntryBodies[index]);

            return;
        }

        if (existing.LocalName != "style")
            Fail("Jazor.Css StyleId '" + context.StyleId + "' is already used by a non-style element.");

        var existingStyle = (HTMLStyleElement)existing;
        var text = existingStyle.TextContent ?? "";
        if (!text.StartsWith(RootMarker))
            Fail("Jazor.Css StyleId '" + context.StyleId + "' is not owned by Jazor.Css.");

        if (context.Nonce is not null && existingStyle.Nonce != context.Nonce)
            Fail("Jazor.Css nonce does not match the existing style element.");

        context.DomStyle = existingStyle;
        context.DomDocument = domDocument;
        AdoptDomEntries(context, domDocument, text);
        context.DomHydrated = true;
        context.HasRegistered = context.EntryIds.Length > 0;
    }

    private static void AdoptDomEntries(CssContext context, Document document, string text)
    {
        var memoryIds = context.EntryIds;
        var memoryBodies = context.EntryBodies;
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
            AppendTextNode(context, document, FormatDomEntry(id, body));
        }

        context.EntryIds = adoptedIds;
        context.EntryBodies = adoptedBodies;
        context.BodyById.Clear();
        for (var index = 0; index < context.EntryIds.Length; index++)
            context.BodyById.Set(context.EntryIds[index], context.EntryBodies[index]);
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

    private static void AppendDomEntry(CssContext context, string id, string body)
    {
        if (context.DomStyle is null)
            return;

        var document = context.DomDocument;
        if (document is null)
            Fail("Jazor.Css style element must belong to a document.");

        AppendTextNode(context, document!, FormatDomEntry(id, body));
    }

    private static string FormatDomEntry(string id, string body)
        => EntryMarkerPrefix + id + ":" + StringFn(body.Length) + "*/" + body;

    private static string BuildHydrationText(CssContext context)
    {
        var output = new Array<string>();
        output.Push(RootMarker);
        for (var index = 0; index < context.EntryIds.Length; index++)
            output.Push(FormatDomEntry(context.EntryIds[index], context.EntryBodies[index]));

        return output.Join("");
    }

    private static void AppendTextNode(CssContext context, Document document, string text)
    {
        if (context.DomStyle is null)
            return;

        context.DomStyle.AppendChild(document.CreateTextNode(text));
    }
}
