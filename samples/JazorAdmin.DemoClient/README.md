# JazorAdmin Demo Client

`JazorAdmin.DemoClient` is an independent confidential RazorVue client for the JazorAdmin identity center. It demonstrates the production boundary expected of a downstream application: authorization code + PKCE, a local downstream cookie, a bearer call to a protected platform API, and front-channel single logout. It consumes identity claims only; downstream business permissions do not become JazorAdmin resource operations.

## Local Configuration

The Development profiles use `https://localhost:49732` for JazorAdmin and `https://localhost:49734` for this client. Before starting either host, put one shared confidential-client secret in user secrets. Never put this secret in `appsettings*.json` or commit it.

```bash
dotnet user-secrets set --project samples/JazorAdmin/JazorAdmin.csproj "JazorAdmin:DemoClient:ClientSecret" "<shared-secret>"
dotnet user-secrets set --project samples/JazorAdmin.DemoClient/JazorAdmin.DemoClient.csproj "JazorAdminDemo:Authority" "https://localhost:49732"
dotnet user-secrets set --project samples/JazorAdmin.DemoClient/JazorAdmin.DemoClient.csproj "JazorAdminDemo:ClientSecret" "<shared-secret>"
```

The default client ID is `jazoradmin-demo-client`. For a custom origin, configure the following values in deployment configuration or user secrets before JazorAdmin first starts; the redirect values must exactly match the downstream host.

| Host | Configuration key | Required value |
| --- | --- | --- |
| JazorAdmin | `JazorAdmin:DemoClient:ClientId` | Same value as `JazorAdminDemo:ClientId` |
| JazorAdmin | `JazorAdmin:DemoClient:ClientSecret` | Shared confidential-client secret |
| JazorAdmin | `JazorAdmin:DemoClient:LaunchUri` | Downstream application origin |
| JazorAdmin | `JazorAdmin:DemoClient:RedirectUris:0` | `<downstream-origin>/signin-oidc` |
| JazorAdmin | `JazorAdmin:DemoClient:PostLogoutRedirectUris:0` | `<downstream-origin>/signout-callback-oidc` |
| DemoClient | `JazorAdminDemo:Authority` | JazorAdmin origin |
| DemoClient | `JazorAdminDemo:ClientId` | Registered client ID |
| DemoClient | `JazorAdminDemo:ClientSecret` | Shared confidential-client secret |

The client keeps OpenID correlation and nonce cookies `Secure` for HTTPS authorities. The HTTP Development loopback profile uses `SameAsRequest` only because a `Secure` cookie cannot complete an HTTP callback; do not use HTTP for a deployed identity center.

## Run

Start JazorAdmin first so it migrates the store and registers the confidential client:

```bash
dotnet run --project samples/JazorAdmin/JazorAdmin.csproj --launch-profile JazorAdmin
dotnet run --project samples/JazorAdmin.DemoClient/JazorAdmin.DemoClient.csproj --launch-profile JazorAdmin.DemoClient
```

Open `https://localhost:49734`, select **Start sign-in**, and authenticate with the local bootstrap administrator. The downstream page displays the identity projection and a live bearer-protected overview response. **Single logout** clears both the downstream cookie and the JazorAdmin identity session.

## Verify

The end-to-end smoke uses isolated ports, database, generated artifacts, and an ephemeral shared secret; it does not read or write user secrets.

```bash
dotnet run --no-launch-profile --file samples/JazorAdmin.DemoClient/verify-smoke.cs -- --configuration Release
```

It rebuilds both applications from locally packed source packages, performs CAPTCHA login, follows authorization code + PKCE, checks the protected bearer API and audit event, then verifies single logout.
