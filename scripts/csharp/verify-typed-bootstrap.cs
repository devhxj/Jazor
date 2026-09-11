#!/usr/bin/env dotnet run
#:sdk Microsoft.NET.Sdk.Web

/*
 Verifies the application-owned typed bootstrap/optimistic-version contract
 described in docs/03-guides/installation-and-configuration.md.

 This fixture deliberately does not register Jazor services. Business freshness,
 validation, and draft retention belong to the application's endpoint.
*/

using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var port = AllocateLoopbackPort();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    EnvironmentName = Environments.Development
});

builder.WebHost.UseUrls($"http://127.0.0.1:{port}");

var service = new EditorService();
var app = builder.Build();

app.MapGet(
    "/api/editor/bootstrap",
    () => Results.Ok(service.Read()));

app.MapPost(
    "/api/editor/commit",
    (EditorCommand command) =>
    {
        if (command.Rows.Any(row => string.IsNullOrWhiteSpace(row.Name)))
        {
            return Results.UnprocessableEntity(new
            {
                errors = new[] { "Name is required." }
            });
        }

        return service.TryCommit(command, out var current)
            ? Results.Ok(current)
            : Results.Conflict(current);
    });

await app.StartAsync();

try
{
    using var client = new HttpClient
    {
        BaseAddress = new Uri($"http://127.0.0.1:{port}")
    };

    var initial = await GetBootstrapAsync(client);

    Ensure(
        initial.Version == 1 && initial.Rows.Count == 1,
        "GET must return the typed initial bootstrap.");

    var clientDraft = "client draft survives conflict and validation";

    using var staleResponse = await client.PostAsJsonAsync(
        "/api/editor/commit",
        new EditorCommand(
            initial.Version - 1,
            [new EditorRow("1", "stale write")]));

    Ensure(
        staleResponse.StatusCode == HttpStatusCode.Conflict,
        "A stale business version must return HTTP 409.");

    var conflict = await ReadRequiredAsync<EditorBootstrap>(staleResponse);

    Ensure(
        conflict.Version == initial.Version,
        "A conflict response must include the current bootstrap version.");

    Ensure(
        clientDraft == "client draft survives conflict and validation",
        "The client draft must survive a 409 refresh.");

    using var invalidResponse = await client.PostAsJsonAsync(
        "/api/editor/commit",
        new EditorCommand(
            conflict.Version,
            [new EditorRow("1", " ")]));

    Ensure(
        invalidResponse.StatusCode == HttpStatusCode.UnprocessableEntity,
        "Validation failures must return HTTP 422.");

    Ensure(
        clientDraft == "client draft survives conflict and validation",
        "The client draft must survive a validation failure.");

    using var committedResponse = await client.PostAsJsonAsync(
        "/api/editor/commit",
        new EditorCommand(
            conflict.Version,
            [new EditorRow("1", "saved")]));

    Ensure(
        committedResponse.IsSuccessStatusCode,
        "A current, valid command must commit successfully.");

    var committed = await ReadRequiredAsync<EditorBootstrap>(committedResponse);

    Ensure(
        committed.Version == 2 &&
        committed.Rows.Count == 1 &&
        committed.Rows[0].Name == "saved",
        "A successful commit must advance the business version.");

    Console.WriteLine("Typed bootstrap consumer verification passed.");
    Console.WriteLine(
        "GET bootstrap, stale 409 refresh, validation 422, draft retention, and successful commit were verified.");
}
finally
{
    await app.StopAsync();
    await app.DisposeAsync();
}

static async Task<EditorBootstrap> GetBootstrapAsync(HttpClient client)
{
    var value = await client.GetFromJsonAsync<EditorBootstrap>(
        "/api/editor/bootstrap");

    return value
        ?? throw new InvalidOperationException(
            "Bootstrap endpoint returned an empty payload.");
}

static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response)
{
    var value = await response.Content.ReadFromJsonAsync<T>();

    return value
        ?? throw new InvalidOperationException(
            $"HTTP {(int)response.StatusCode} returned an empty JSON payload.");
}

static int AllocateLoopbackPort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();

    return ((IPEndPoint)listener.LocalEndpoint).Port;
}

static void Ensure(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed class EditorService
{
    private EditorBootstrap current =
        new(1, [new EditorRow("1", "initial")]);

    public EditorBootstrap Read() => current;

    public bool TryCommit(
        EditorCommand command,
        out EditorBootstrap snapshot)
    {
        if (command.Version != current.Version)
        {
            snapshot = current;
            return false;
        }

        current = new EditorBootstrap(
            current.Version + 1,
            command.Rows);

        snapshot = current;
        return true;
    }
}

sealed record EditorBootstrap(
    int Version,
    IReadOnlyList<EditorRow> Rows);

sealed record EditorRow(
    string Id,
    string Name);

sealed record EditorCommand(
    int Version,
    IReadOnlyList<EditorRow> Rows);

