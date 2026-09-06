namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorVueAuthenticationRuntimeTests
{
    [TestMethod]
    public async Task AuthenticationProvider_TransitionsAndNotifiesSubscribers()
    {
        var module = File.ReadAllText(FindRepositoryRoot("src", "Jazor.Vue", "dist", "authentication.mjs"));
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "authentication.mjs",
            module,
            "authentication-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { createAuthenticationProvider } from "./authentication.mjs";

            test("closed authentication states transition through the provider", async () => {
                const provider = createAuthenticationProvider({ status: "Anonymous" });
                const seen = [];
                const unsubscribe = provider.subscribe(state => seen.push(state.status));
                provider.setAuthenticated("user-42", { role: ["admin"] });
                provider.markForbidden();
                provider.markExpired();
                await provider.signOut();
                unsubscribe();
                provider.setAnonymous();
                assert.deepEqual(seen, ["Authenticated", "Forbidden", "Expired", "Anonymous"]);
                assert.equal(provider.status, "Anonymous");
                assert.deepEqual(provider.claims, {});
            });
            """,
            vueRuntimeSource: """
            export function reactive(value) { return value; }
            export function provide(_key, _value) {}
            export function inject(_key, fallback) { return fallback; }
            export function onUnmounted(_callback) {}
            """);
    }

    [TestMethod]
    public async Task AuthenticationProvider_RefreshIgnoresStaleEndpointCompletion()
    {
        var module = File.ReadAllText(FindRepositoryRoot("src", "Jazor.Vue", "dist", "authentication.mjs"));
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "authentication.mjs",
            module,
            "authentication-refresh-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { createAuthenticationProvider } from "./authentication.mjs";

            test("a newer refresh supersedes an older endpoint response", async () => {
                let releaseFirst;
                const first = new Promise(resolve => { releaseFirst = resolve; });
                let calls = 0;
                const provider = createAuthenticationProvider({ status: "Anonymous" }, {
                    refresh: async () => ++calls === 1
                        ? (await first, { schema: "jazor-auth-state", version: 1, state: { status: "Authenticated", subject: "stale" } })
                        : { schema: "jazor-auth-state", version: 1, state: { status: "Authenticated", subject: "new" } },
                });
                const firstRefresh = provider.refresh();
                const secondRefresh = provider.refresh();
                await secondRefresh;
                assert.equal(provider.subject, "new");
                releaseFirst();
                await firstRefresh;
                assert.equal(provider.subject, "new");
            });
            """);
    }

    [TestMethod]
    public async Task AuthenticationProvider_UsesExplicitSignInAndPreservesStateWhenEndpointFails()
    {
        var module = File.ReadAllText(FindRepositoryRoot("src", "Jazor.Vue", "dist", "authentication.mjs"));
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "authentication.mjs",
            module,
            "authentication-sign-in-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { createAuthenticationProvider, validateAuthenticationSnapshot } from "./authentication.mjs";

            test("application endpoint owns sign-in and endpoint failure remains observable", async () => {
                const provider = createAuthenticationProvider({
                    status: "Authenticated", subject: "ssr-user", claims: { role: ["reader"] },
                }, {
                    signIn: async (_current, credentials) => {
                        assert.deepEqual(credentials, { email: "new@example.test" });
                        return { schema: "jazor-auth-state", version: 1,
                            state: { status: "Authenticated", subject: "browser-user", claims: { role: ["admin"] } } };
                    },
                    refresh: async () => { throw new Error("endpoint unavailable"); },
                    signOut: async () => ({ schema: "jazor-auth-state", version: 1, state: { status: "Anonymous" } }),
                });
                assert.deepEqual(provider.snapshot(), validateAuthenticationSnapshot({
                    status: "Authenticated", subject: "ssr-user", claims: { role: ["reader"] },
                }));
                await provider.signIn({ email: "new@example.test" });
                assert.equal(provider.subject, "browser-user");
                await assert.rejects(provider.refresh(), /endpoint unavailable/);
                assert.equal(provider.status, "Authenticated");
                assert.match(provider.error.message, /endpoint unavailable/);
                provider.markExpired();
                provider.markForbidden();
                await provider.signOut();
                assert.equal(provider.status, "Anonymous");
                assert.equal(provider.error, null);
            });
            """,
            vueRuntimeSource: """
            export function reactive(value) { return value; }
            export function provide(_key, _value) {}
            export function inject(_key, fallback) { return fallback; }
            export function onUnmounted(_callback) {}
            """);
    }

    [TestMethod]
    public async Task AuthenticationProvider_ConsumesSsrSnapshotThroughSetupInjection()
    {
        var module = File.ReadAllText(FindRepositoryRoot("src", "Jazor.Vue", "dist", "authentication.mjs"));
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "authentication.mjs",
            module,
            "authentication-ssr-hydration-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { useAuthenticationProvider } from "./authentication.mjs";

            test("setup helper turns the SSR-provided snapshot into a browser provider", () => {
                globalThis.__providers = new Map([["jazor:auth-state", {
                    status: "Authenticated", subject: "hydrated-user", claims: { role: ["reader"] },
                }]]);
                const provider = useAuthenticationProvider();
                assert.equal(provider.status, "Authenticated");
                assert.equal(provider.subject, "hydrated-user");
                assert.deepEqual(provider.claims, { role: ["reader"] });
            });
            """,
            vueRuntimeSource: """
            export function reactive(value) { return value; }
            export function provide(key, value) { globalThis.__providers?.set(key, value); }
            export function inject(key, fallback) { return globalThis.__providers?.get(key) ?? fallback; }
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
        throw new DirectoryNotFoundException("Could not locate the RazorVue authentication runtime module.");
    }
}
