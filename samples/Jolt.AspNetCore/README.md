# Jolt in ASP.NET Core

From the repository root, pack the Jolt CLI from this checkout into a local feed. Give the candidate a unique prerelease version to keep it separate from released tools.

```powershell
$version = "2.7.0-local.$([DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))"
$feed = Join-Path (Get-Location) '.tmp/jolt-aspnet-feed'
dotnet pack src/Jolt.Cli/Jolt.Cli.csproj -p:MinVerVersionOverride=$version -o $feed
cd samples/Jolt.AspNetCore
dotnet new tool-manifest
dotnet tool install Jazor.Jolt --version $version --add-source $feed
dotnet run
```

If the local manifest already exists, use `dotnet tool update` with the same version and source. Cross-platform checkout builds can use the same commands in PowerShell. If the SDK cannot trust manifests on the checkout drive, use a checkout on a drive it trusts.

Open `http://localhost:5168/portal/frontend/index.html`. Increment the counter and edit the heading in `frontend/Counter.jazor`. The open page updates through the ASP.NET WebSocket proxy and keeps the current count.

For the desktop Visual Studio check, set this project as the startup project and use the checked-in launch profile. Set a breakpoint on `WebApplication.CreateBuilder(args)`, press F5, continue at the breakpoint and repeat the counter edit. Stop debugging and check both the host on port 5168 and the Jolt entry at `http://127.0.0.1:15658/portal/frontend/index.html`. When the sample starts Jolt, both should stop. To check attachment, start `dotnet tool run jolt dev frontend --port 15658` in a separate terminal before pressing F5. This time the external entry should still respond after the host stops.

On Windows, Visual Studio 2026 Insiders and the locally installed Jolt candidate `2.6.0-migration.20261124.11` passed the F5 check. Chromium displayed the edited heading and preserved the count. Shift+F5 stopped the host, its Jolt process and the tool launcher. The automated test also force-stops only the host process and checks that an attached external Jolt still serves the component. [Migration evidence](../../docs/04-roadmap/jazor-jolt-migration.md)

Stop the development host, then bundle and publish from the same sample directory.

```powershell
dotnet tool run jolt bundle frontend
dotnet publish -o bin/publish
cd bin/publish
dotnet Jolt.AspNetCore.dll --environment Production --urls http://localhost:5168
```

Open the same URL. The published host serves the built frontend directly.
