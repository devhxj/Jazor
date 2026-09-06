namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueNavigationHistoryRuntimeTests
{
    [TestMethod]
    public async Task NavigationHost_HistoryCancellationRestoresUrlAndDisposeStopsEvents()
    {
        var module = File.ReadAllText(FindRepositoryRoot("src", "Jazor.Vue", "dist", "blazor-routing.mjs"))
            .Replace("import { routes } from \"@jazor/vue-runtime/routes.mjs\";", "const routes = [];", StringComparison.Ordinal);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "blazor-routing.mjs",
            module,
            "navigation-history-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { createNavigationHost } from "./blazor-routing.mjs";
            import { preventNavigation } from "Microsoft/AspNetCore/Components/Routing/LocationChangingContextModule.js";

            test("history handlers cancel after the browser moved and restore the accepted URL", async () => {
                const calls = [];
                const listeners = new Map();
                const location = {
                    origin: "https://example.test",
                    href: "https://example.test/app/start",
                    pathname: "/app/start", search: "", hash: "",
                };
                const setLocation = href => {
                    const value = new URL(href, location.href);
                    location.href = value.href;
                    location.pathname = value.pathname;
                    location.search = value.search;
                    location.hash = value.hash;
                };
                const history = {
                    state: "start",
                    replaceState(state, _title, href) {
                        this.state = state;
                        setLocation(href);
                        calls.push(["replace", href, state]);
                    },
                    pushState(state, _title, href) {
                        this.state = state;
                        setLocation(href);
                        calls.push(["push", href, state]);
                    },
                };
                globalThis.location = location;
                globalThis.history = history;
                globalThis.window = { location, history };
                globalThis.document = { querySelector() { return { getAttribute() { return "/app/"; } }; } };
                globalThis.addEventListener = (name, callback) => {
                    if (!listeners.has(name)) listeners.set(name, new Set());
                    listeners.get(name).add(callback);
                };
                globalThis.removeEventListener = (name, callback) => listeners.get(name)?.delete(callback);
                const emit = async name => {
                    for (const callback of [...(listeners.get(name) ?? [])]) callback();
                    await new Promise(resolve => setTimeout(resolve, 0));
                };

                let changes = 0;
                const host = createNavigationHost(() => changes++);
                host.navigation.registerLocationChangingHandler(context => {
                    if (context.targetLocation.endsWith("/blocked")) preventNavigation(context);
                });

                setLocation("/app/blocked");
                await emit("popstate");
                assert.equal(location.pathname, "/app/start");
                assert.deepEqual(calls, [["replace", "/app/start", "start"]]);
                assert.equal(changes, 0);

                setLocation("/app/allowed#details");
                history.state = "allowed";
                await emit("hashchange");
                assert.equal(host.navigation.uri, "https://example.test/app/allowed#details");
                assert.equal(changes, 1);
                await emit("popstate");
                assert.equal(changes, 1);

                host.dispose();
                setLocation("/app/after-dispose");
                await emit("popstate");
                assert.equal(changes, 1);
            });
            """,
            vueRuntimeSource: """
            const providers = new Map();
            export function reactive(value) { return value; }
            export function provide(key, value) { providers.set(key, value); }
            export function onUnmounted(_callback) {}
            """);
    }

    private static string FindRepositoryRoot(params string[] suffix)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine([directory.FullName, ..suffix]);
            if (File.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Could not locate the RazorVue navigation runtime module.");
    }
}
