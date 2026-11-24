#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Diagnostics;
using Jazor.NodeHost;

if (args.Length is < 1 or > 3)
    throw new ArgumentException("Usage: verify-zero-jolt.cs <JoltApp> [library | --build <Jolt CLI apphost>]");

var project = Path.GetFullPath(args[0]);
var scratch = Directory.CreateDirectory(Path.Combine(project, ".jazor", $"verify-zero-jolt-{Guid.NewGuid():N}"));
var probe = Path.Combine(project, $".verify-zero-jolt-{Guid.NewGuid():N}.mjs");
try
{
    var library = Path.Combine(project, "dist");
    if (args is [_, "--build", var cli])
    {
        library = Path.Combine(scratch.FullName, "library");
        var start = new ProcessStartInfo(Path.GetFullPath(cli)) { WorkingDirectory = project, UseShellExecute = false };
        foreach (var argument in new[] { "library", project, "--output", library })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            return process.ExitCode;
    }
    else if (args is [_, var output])
        library = Path.GetFullPath(output);
    else if (args.Length != 1)
        throw new ArgumentException("Expected a library directory or --build with a Jolt CLI apphost.");

    if (!File.Exists(Path.Combine(library, "index.mjs")))
        throw new FileNotFoundException("Jolt library entry was not found.", Path.Combine(library, "index.mjs"));

    // Reuse the DOM fixture pinned by Jazor's npm runtime verification.
    var domFixture = Path.GetFullPath("fixtures/jolt-npm-runtime/package.json", ScriptDirectory());
    var store = Path.GetFullPath(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
        ?? Path.Combine(ScriptDirectory(), "..", "..", ".tmp", "nodeapi-store"));
    await NodeToolchain.RestoreAsync(Path.GetDirectoryName(domFixture)!, store, CancellationToken.None, Console.Out);
    await File.WriteAllTextAsync(probe, """
        import assert from 'node:assert/strict';
        import { createRequire, registerHooks } from 'node:module';
        import { join } from 'node:path';
        import { pathToFileURL } from 'node:url';

        const [library, domFixture] = globalThis.__jazorArguments;
        const { JSDOM } = createRequire(domFixture)('jsdom');
        // Resolve library peers from the app so the probe and components share Vue.
        // Styles remain browser assets while Node executes the packaged modules.
        const hook = registerHooks({
            resolve(specifier, context, next) {
                if (specifier === 'vue' || specifier === 'element-plus' || specifier.startsWith('element-plus/'))
                    return next(specifier, { ...context, parentURL: import.meta.url });
                return next(specifier, context);
            },
            load(url, context, next) {
                return url.endsWith('.css')
                    ? { shortCircuit: true, format: 'module', source: '' }
                    : next(url, context);
            },
        });
        const dom = new JSDOM('<!doctype html><base href="/portal/"><main></main>', {
            url: 'https://zero.test/portal/received-notice?filter=urgent#top',
            pretendToBeVisual: true,
        });
        globalThis.__jazorAfterAll(() => {
            dom.window.close();
            hook.deregister();
        });
        for (const name of ['window', 'document', 'HTMLElement', 'HTMLInputElement', 'SVGElement', 'Element', 'Node', 'Event',
            'MouseEvent', 'CustomEvent', 'FocusEvent', 'KeyboardEvent', 'MutationObserver', 'getComputedStyle', 'requestAnimationFrame', 'cancelAnimationFrame'])
            globalThis[name] = name === 'window' ? dom.window : dom.window[name];
        const { createApp, createSSRApp, h, nextTick, reactive, ref } = await import('vue');
        const { renderToString } = await import('vue/server-renderer');
        const { AboutPage, ErrorPage, ZeroQueryCard, ReceivedNoticePage } =
            await import(pathToFileURL(join(library, 'index.mjs')).href);
        const root = document.querySelector('main');
        const required = selector => {
            const element = root.querySelector(selector);
            assert.ok(element, selector);
            return element;
        };
        const button = label => {
            const match = [...root.querySelectorAll('button')].find(item => item.textContent.trim() === label);
            assert.ok(match, label);
            return match;
        };
        async function mount(render, test) {
            const app = createApp({ render });
            app.config.errorHandler = error => { throw error; };
            app.mount(root);
            try {
                await nextTick();
                await test();
            } finally {
                app.unmount();
            }
        }

        const about = await renderToString(createSSRApp(AboutPage));
        assert.match(about, /关于项目/);
        assert.match(about, /生产环境依赖/);
        assert.match(about, /开发环境依赖/);
        assert.match(about, /element-plus/);
        assert.match(about, /vite-plugin-pwa/);

        const calls = [];
        let dialogClosed;
        const status = ref('403');
        await mount(() => h(ErrorPage, {
            Status: status.value,
            GoHome: () => calls.push('home'),
            Refresh: () => calls.push('refresh'),
        }), async () => {
            for (const [value, heading, label] of [
                ['403', '无权访问', '返回首页'], ['404', '页面不存在', '返回首页'],
                ['500', '服务暂时不可用', '重新加载'], ['offline', '网络连接已断开', '重新加载'],
                ['coming-soon', '功能建设中', '返回首页'],
            ]) {
                status.value = value;
                await nextTick();
                assert.equal(required('h1').textContent, heading);
                button(label).click();
            }
        });
        assert.deepEqual(calls, ['home', 'home', 'refresh', 'refresh', 'home']);

        const key = 'zero-query-open:received-notice';
        const query = () => h(ZeroQueryCard, { Title: '自定义查询' }, {
            default: () => h('span', { id: 'filters' }, '查询字段'),
        });
        await mount(query, async () => {
            const toggle = required('.zero-query-card__head');
            assert.equal(required('.zero-query-card__title').textContent, '自定义查询');
            assert.equal(toggle.getAttribute('aria-expanded'), 'false');
            assert.equal(root.querySelector('#filters'), null);
            toggle.click();
            await nextTick();
            assert.equal(toggle.getAttribute('aria-expanded'), 'true');
            assert.equal(required('#filters').textContent, '查询字段');
            assert.equal(window.localStorage.getItem(key), '1');
        });
        await mount(query, async () => {
            const toggle = required('.zero-query-card__head');
            assert.equal(required('.zero-query-card__title').textContent, '自定义查询');
            assert.equal(toggle.getAttribute('aria-expanded'), 'true');
            toggle.click();
            await nextTick();
            assert.equal(root.querySelector('#filters'), null);
            assert.equal(window.localStorage.getItem(key), '0');
        });

        const notice = reactive({
            Rows: [{ id: 'n-7', readStatus: 0, title: '验收消息', contentSummary: '消息摘要',
                type: 1, publicUserName: '发布人', publicTime: '2026-11-23' }],
            TypeOptions: [{ value: 1, label: '通知', tagType: 'success' }],
            CurrentPage: 1, PageSize: 10, Total: 30,
            QueryTitle: '', QueryType: 0, Loading: true, OptionsLoading: false, MarkingRead: false,
            Error: '验收错误', DetailDialogVisible: false, DetailTitle: '详情标题',
            DetailContent: '<strong id="notice-detail">消息正文</strong>',
            DismissError() { calls.push('dismiss'); notice.Error = null; },
            SetTitle(value) { calls.push(['title', value]); notice.QueryTitle = value; },
            SetType(value) { calls.push(['type', value]); notice.QueryType = value; },
            Search() { calls.push(['search', notice.QueryTitle]); },
            ResetSearch() { calls.push('reset'); },
            Refresh() { calls.push('refresh'); },
            ChangePage(value) { calls.push(['page', value]); notice.CurrentPage = value; },
            ChangePageSize(value) { calls.push(['size', value]); notice.PageSize = value; },
            OpenDetail(id) {
                calls.push(['detail', id]);
                notice.Rows[0].readStatus = 1;
                notice.DetailDialogVisible = true;
            },
            SetDialogVisible(value) {
                calls.push(['dialog', value]);
                notice.DetailDialogVisible = value;
                if (!value) dialogClosed?.();
            },
        });
        calls.length = 0;
        window.localStorage.setItem(key, '1');
        await mount(() => h(ReceivedNoticePage, { ...notice }), async () => {
            assert.match(root.textContent, /正在加载站内信/);
            notice.Loading = false;
            await nextTick();
            assert.match(root.textContent, /验收消息/);
            assert.match(root.textContent, /消息摘要/);
            assert.match(root.textContent, /发布人/);
            assert.match(root.textContent, /未读/);
            required('.el-alert__close-btn').dispatchEvent(new MouseEvent('click', { bubbles: true }));
            const input = required('input[placeholder="搜索站内信标题"]');
            input.value = '验收';
            input.dispatchEvent(new Event('input', { bubbles: true }));
            await nextTick();
            button('查询').click();
            button('重置').click();
            button('刷新').click();
            required('.el-pagination .btn-next').click();
            button('详情').click();
            await nextTick();
            assert.match(root.textContent, /已读/);
            assert.equal(document.querySelector('#notice-detail')?.textContent, '消息正文');
            const close = document.querySelector('.el-dialog__headerbtn');
            assert.ok(close, 'dialog close button');
            // Element Plus emits update:modelValue after the leave transition.
            const closed = new Promise(resolve => { dialogClosed = resolve; });
            close.click();
            const timeout = AbortSignal.timeout(1500);
            await Promise.race([closed, new Promise((_, reject) => {
                timeout.addEventListener('abort', () => reject(new Error('Dialog close callback did not fire.')), { once: true });
            })]);
            assert.equal(notice.DetailDialogVisible, false);
            assert.deepEqual(calls, ['dismiss', ['title', '验收'], ['search', '验收'], 'reset', 'refresh',
                ['page', 2], ['detail', 'n-7'], ['dialog', false]]);
        });
        console.log('Verified About SSR, ErrorPage status actions, QueryCard slot persistence, and ReceivedNotice interactions.');
        """);

    Console.WriteLine(await NodeModuleRunner.RunAsync(project, probe, [library, domFixture]));
    return 0;
}
finally
{
    File.Delete(probe);
    scratch.Delete(recursive: true);
}

static string ScriptDirectory([System.Runtime.CompilerServices.CallerFilePath] string source = "") =>
    Path.GetDirectoryName(source)!;
