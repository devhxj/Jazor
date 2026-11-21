#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:package Microsoft.CodeAnalysis.CSharp@5.12.0-1.26529.118
#:project ../../src/Jolt.Compiler/Jolt.Compiler.csproj

using Jazor.NodeHost;
using Jolt.Compiler;
using Jolt.Npm;

var root = Path.GetFullPath(".");
var project = Path.GetFullPath(args is [var existing] ? existing :
    Path.Combine("scripts", "csharp", "fixtures", "jolt-binding-projection"));
var directory = Directory.CreateDirectory(Path.Combine(project, $".composable-runtime-{Guid.NewGuid():N}"));
try
{
    var provider = new NpmVueMetadataProvider(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(root, ".tmp", "nodeapi-store"));
    var document = Path.Combine(project, "Probe.jazor");
    var value = await provider.ReadValueAsync(document, "@tanstack/vue-query", "useQueryClient");
    var counts = await provider.ReadValuesAsync(document, "@tanstack/vue-query", ["useIsFetching", "useIsMutating"]);
    var composer = await provider.ReadValueAsync(document, "vue-i18n", "useI18n");
    var form = await provider.ReadValuesAsync(document, "vee-validate", ["useForm", "useFieldError", "useSetFieldError"]);
    if (value.Version != "5.103.1" ||
        value.Signatures.Single().ReturnType.Members?.Count(member =>
            member.Name is "isFetching" or "isMutating" or "clear" && member.Type.Kind == "method") != 3)
        throw new InvalidOperationException("Expected query-client methods from the locked Vue Query version.");
    if (value.Signatures.Single().ReturnType.Members?.Count(member =>
        member.Name is "invalidateQueries" or "refetchQueries" &&
        member.Type.Signatures is [{ Parameters.Count: 0, ReturnType: { Kind: "promise", ElementType.Kind: "void" } },
            { Parameters: [{ Type.Kind: "object" }] }, ..]) != 2)
        throw new InvalidOperationException("Expected query-client promises with and without a filter.");

    var contractProject = Directory.CreateDirectory(Path.Combine(directory.FullName, "contract"));
    var contractPackage = Directory.CreateDirectory(Path.Combine(contractProject.FullName, "fixture"));
    File.WriteAllText(Path.Combine(contractProject.FullName, "package.json"),
        """{"private":true,"dependencies":{"method-fixture":"file:./fixture"}}""");
    File.WriteAllText(Path.Combine(contractPackage.FullName, "package.json"),
        """{"name":"method-fixture","version":"1.0.0","type":"module","main":"./index.js","types":"./index.d.ts"}""");
    File.WriteAllText(Path.Combine(contractPackage.FullName, "index.js"), """
        class Worker {
            settle() { return Promise.resolve(); }
            reset() { return Promise.resolve(); }
            calculate() { return Promise.resolve(42); }
            imitate() { return { then() {} }; }
        }
        export function openWorker() { return new Worker(); }
        """);
    File.WriteAllText(Path.Combine(contractPackage.FullName, "index.d.ts"), """
        namespace Local {
            interface Promise<T> { then(complete: (value: T) => void): void; }
        }
        declare class Worker {
            settle<T extends readonly unknown[] = readonly unknown[]>(options?: {
                labels: T;
                strict?: boolean;
                writable?: unknown[];
            }): Promise<void>;
            reset(configuration?: { required: unknown }): Promise<void>;
            calculate(): Promise<number>;
            imitate(): Local.Promise<void>;
        }
        export function openWorker(): Worker;
        """);
    var contract = await provider.ReadValueAsync(Path.Combine(contractProject.FullName, "Probe.jazor"), "method-fixture", "openWorker");
    if (contract.Signatures.Single().ReturnType.Members is not
        [{ Name: "settle", Type.Signatures: [
            { Parameters.Count: 0, ReturnType: { Kind: "promise", ElementType.Kind: "void" } },
            { Parameters: [{ Type.Members: [{ Name: "labels", Type: { ReadOnly: true, ElementType.Variants: [
                { Kind: "boolean" }, { Kind: "number" }, { Kind: "string" }, { Kind: "plainRecord" }] } }, { Name: "strict" }] }] }] },
        { Name: "reset", Type.Signatures: [{ Parameters.Count: 0 }] }])
        throw new InvalidOperationException("Independent class filters require a constructible input.");
    const string source = """
        @using Query = Npm.Tanstack.VueQuery.Exports
        @using I18n = Npm.VueI18n.Exports
        @using Vee = Npm.VeeValidate.Exports
        <p>@Locale/@Error/@Count</p>
        @code {
            private double Count { get; set; }
            private string Locale { get; set; } = "";
            private string Error { get; set; } = "";
            [Setup]
            private void Initialize()
            {
                var client = Query.UseQueryClient();
                Action clear = client.Clear;
                clear();
                Count = client.IsFetching() + client.IsMutating();
                var composer = I18n.UseI18n();
                Locale = composer.Locale.Value + "/" + composer.T("greeting");
                composer.Locale.Value = "en";
                Locale += "/" + composer.T("greeting");
                Locale += "/" + composer.T("items", 1) + "/" + composer.T("items", 2);
                var form = Vee.UseForm();
                var error = Vee.UseFieldError("email");
                var report = Vee.UseSetFieldError("email");
                report("invalid");
                Error = error.Value;
                report(new string[] { "required", "email" });
                if (!form.IsSubmitting.Value && form.SubmitCount.Value == 0)
                    Error += "/" + error.Value;
            }
        }
        """;
    var options = new JoltCompileOptions { NpmValues = [value, composer, ..form] };
    var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
    if (result.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
    var code = result.Modules.Single().Content;
    File.WriteAllText(Path.Combine(directory.FullName, "App.mjs"), code);
    const string reload = """
        @using Query = Npm.Tanstack.VueQuery.Exports
        <p>reload</p>
        @code {
            private sealed record PageFilter(double Page) : ECMAScript.Vue.VueProps;

            public static ECMAScript.IPromise RefreshAll()
            {
                var client = Query.UseQueryClient();
                return client.InvalidateQueries().Then(() => client.RefetchQueries());
            }

            public static ECMAScript.IPromise RefreshPage(double page) =>
                Query.UseQueryClient().RefetchQueries(new() { QueryKey = ["page", page], Exact = true });

            [Setup]
            private void Initialize()
            {
                var client = Query.UseQueryClient();
                ECMAScript.IPromise invalidation = client.InvalidateQueries(new()
                {
                    QueryKey = ["active", 0, false, new PageFilter(3)],
                    Exact = true,
                });
                var prefix = invalidation.Then(() => client.InvalidateQueries(new()
                {
                    QueryKey = ["active", 0],
                }));
                prefix.Then(() => client.RefetchQueries(new()
                {
                    QueryKey = ["active", 0, false, new PageFilter(3)],
                    Exact = true,
                }));
            }
        }
        """;
    var reloaded = await JoltProjectCompiler.CompileAsync([new("Reload.jazor", reload)], options);
    if (reloaded.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, reloaded.Diagnostics));
    File.WriteAllText(Path.Combine(directory.FullName, "Reload.mjs"), reloaded.Modules.Single().Content);
    var counted = await JoltProjectCompiler.CompileAsync([new("Counts.jazor", """
        @using Query = Npm.Tanstack.VueQuery.Exports
        <p>@_counts</p>
        @code {
            private string _counts = "";

            [Setup]
            private void Initialize()
            {
                var fetching = Query.UseIsFetching(new() { QueryKey = ["active", 0, false], Exact = true });
                var mutating = Query.UseIsMutating(new() { MutationKey = ["active", 0, false], Exact = true });
                _counts = $"{fetching.Value}/{Query.UseIsFetching().Value}/{mutating.Value}/{Query.UseIsMutating().Value}";
            }
        }
        """)], new() { NpmValues = counts });
    if (counted.Diagnostics.Count != 0)
        throw new InvalidOperationException(string.Join(Environment.NewLine, counted.Diagnostics));
    File.WriteAllText(Path.Combine(directory.FullName, "Counts.mjs"), counted.Modules.Single().Content);
    var verify = Path.Combine(directory.FullName, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from 'node:assert/strict';
        import { createSSRApp } from 'vue';
        import { renderToString } from 'vue/server-renderer';
        import { QueryClient, VueQueryPlugin } from '@tanstack/vue-query';
        import { QueryObserver } from '@tanstack/query-core';
        import { createI18n } from 'vue-i18n';
        import App from './App.mjs';
        import Reload, { RefreshAll, RefreshPage } from './Reload.mjs';
        import Counts from './Counts.mjs';
        const countClient = new QueryClient();
        const pending = [];
        let finish;
        const hold = new Promise(resolve => { finish = resolve; });
        try {
            for (const key of [['active', 0, false], ['active', 0, 'false']]) {
                pending.push(countClient.fetchQuery({ queryKey: key, queryFn: () => hold }));
                pending.push(countClient.getMutationCache().build(countClient, {
                    mutationKey: key, mutationFn: () => hold,
                }).execute(undefined));
            }
            const counts = createSSRApp(Counts);
            counts.use(VueQueryPlugin, { queryClient: countClient });
            assert.equal(await renderToString(counts), '<p>1/2/1/2</p>');
        } finally {
            finish(42);
            await Promise.allSettled(pending);
            countClient.clear();
        }
        const client = new QueryClient();
        client.setQueryData(['probe'], 42);
        const app = createSSRApp(App);
        app.config.errorHandler = error => { throw error; };
        app.use(VueQueryPlugin, { queryClient: client });
        app.use(createI18n({ legacy: false, locale: 'fr', messages: {
            fr: { greeting: 'Salut' }, en: { greeting: 'Hello', items: 'one item | {count} items' },
        } }));
        assert.equal(await renderToString(app), '<p>fr/Salut/Hello/one item/2 items/invalid/required/0</p>');
        assert.equal(client.getQueryData(['probe']), undefined);
        const tracked = [
            { key: ['active', 0, false, { Page: 3 }], calls: [] },
            { key: ['active', 0, true, { Page: 3 }], calls: [] },
            { key: ['active', '0', false, { Page: 3 }], calls: [] },
        ];
        const reloaded = Promise.withResolvers();
        const stop = tracked.map(({ key, calls }) => {
            client.setQueryData(key, 42);
            const observer = new QueryObserver(client, {
                queryKey: key, staleTime: Infinity,
                queryFn: async () => {
                    calls.push(client.getQueryState(key).isInvalidated);
                    return 42 + calls.length;
                },
            });
            return observer.subscribe(() => {
                if (client.getQueryData(tracked[0].key) === 45 && client.getQueryData(tracked[1].key) === 43)
                    reloaded.resolve();
            });
        });
        const timeout = setTimeout(() => reloaded.reject(new Error('Exact and prefix fetches did not complete')), 3000);
        try {
            const reload = createSSRApp(Reload);
            reload.config.errorHandler = error => { throw error; };
            reload.use(VueQueryPlugin, { queryClient: client });
            await renderToString(reload);
            await reloaded.promise;
            assert.deepEqual(tracked.map(query => query.calls), [[true, true, false], [true], []]);
            assert.deepEqual(tracked.map(query => client.getQueryData(query.key)), [45, 43, 42]);
            await reload.runWithContext(RefreshAll);
            assert.deepEqual(tracked.map(query => query.calls.slice(-2)), [[true, false], [true, false], [true, false]]);
            assert.deepEqual(tracked.map(query => client.getQueryData(query.key)), [47, 45, 44]);

            // Resolve a superseded request last. Its result must reach neither
            // the newer request for the same page nor the other page's cache.
            const requests = [];
            for (const page of [1, 2]) {
                const key = ['page', page];
                client.setQueryData(key, 'initial');
                const observer = new QueryObserver(client, {
                    queryKey: key, staleTime: Infinity,
                    queryFn: ({ signal }) => {
                        const pending = Promise.withResolvers();
                        requests.push({ page, signal, ...pending });
                        return pending.promise;
                    },
                });
                stop.push(observer.subscribe(() => {}));
            }
            const refreshPage = page => reload.runWithContext(() => RefreshPage(page));
            const previous = refreshPage(1);
            const otherPage = refreshPage(2);
            const latest = refreshPage(1);
            assert.deepEqual(requests.map(request => request.page), [1, 2, 1]);
            assert.equal(requests[0].signal.aborted, true);
            requests[2].resolve('latest');
            requests[1].resolve('other');
            await Promise.all([latest, otherPage]);
            requests[0].resolve('superseded');
            await previous;
            assert.deepEqual([1, 2].map(page => client.getQueryData(['page', page])), ['latest', 'other']);
        } finally {
            clearTimeout(timeout);
            stop.forEach(unsubscribe => unsubscribe());
            client.clear();
        }
        console.log('Vue SSR exercised translations, field errors, mixed-key invalidation, refetch and out-of-order page requests.');
        """);
    Console.WriteLine(await NodeModuleRunner.RunAsync(project, verify));
}
finally
{
    directory.Delete(recursive: true);
}
