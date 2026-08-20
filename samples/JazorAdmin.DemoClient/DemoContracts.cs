namespace JazorAdmin.DemoClient;

public sealed record DemoSessionView(
    string Subject,
    string? Name,
    string? Email,
    string[] Roles,
    bool HasAccessToken);

public sealed record ProtectedOverviewView(
    int Accounts,
    int Applications,
    int Tokens,
    int AuditEvents,
    int TokenIssuances);
