namespace Jazor.VueHost.Protocol.Documents;

public sealed record TextChange(TextSpan Span, string NewText);
