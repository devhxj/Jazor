# Jolt in ASP.NET Core

Open `Jolt.AspNetCore.csproj` in Visual Studio, choose the **Jolt.AspNetCore** launch profile, and press **F5**. From a terminal with the .NET 11 SDK, start the same profile from the repository root.

```sh
cd samples/Jolt.AspNetCore
dotnet run
```

Open `http://localhost:5168/portal/frontend/index.html`. Increment the counter and edit the heading in `Counter.jazor`. The open page updates through the ASP.NET WebSocket proxy and keeps the current count. The sample uses the repository's Jolt CLI and writes generated modules under `.jazor`. When the host starts Vite, stopping the host also stops Vite.

Stop the development host, then publish and serve the application from the same sample directory.

```sh
dotnet publish -c Release -o bin/publish
cd bin/publish
dotnet Jolt.AspNetCore.dll --environment Production --urls http://localhost:5168
```

Open the same URL. Publishing bundles the frontend from the project root and copies the hashed assets from `.jazor/dist` into `wwwroot`. The production host serves those files directly. [Build targets](Jolt.AspNetCore.csproj)
