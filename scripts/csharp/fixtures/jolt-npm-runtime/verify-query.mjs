import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

// Vue resolves document the first time it loads, before either app is created.
const { window } = new JSDOM('<main></main>');
for (const name of ['window', 'document', 'Element', 'SVGElement'])
    globalThis[name] = name === 'window' ? window : window[name];
const { useQueryClient } = await import('@tanstack/vue-query');
const { createApp, createSSRApp, nextTick } = await import('vue');
const { renderToString } = await import('vue/server-renderer');
const { default: Query, Install } = await import('./.jolt/Query.mjs');

// Infinite GC avoids a package timer keeping the embedded Node worker alive after disposal.
const options = { queries: { retry: false, gcTime: Infinity }, mutations: { gcTime: Infinity } };
const app = createSSRApp(Query);
function controlledRequest() {
    const requests = [];
    return {
        requests,
        run: () => new Promise((resolve, reject) => requests.push({ resolve, reject })),
    };
}
const queryRequests = controlledRequest();
const mutationRequests = controlledRequest();
const client = createApp(Query, {
    FetchValue: queryRequests.run,
    SaveValue: mutationRequests.run,
});
Install(app);
Install(client);
const queryClient = app.runWithContext(() => useQueryClient());
const clientQueryClient = client.runWithContext(() => useQueryClient());
queryClient.setDefaultOptions(options);
clientQueryClient.setDefaultOptions(options);
try {
    app.config.errorHandler = error => { throw error; };
    const html = await renderToString(app);
    assert.match(html, /<output id="query-data">fetched<\/output>/);
    assert.match(html, /<output id="mutation-data">fixture-saved<\/output>/);
    assert.match(html, /<output id="scalar-data">null-then-zero-and-false<\/output>/);
    assert.match(html, /<output id="infinite-data">first\|second\|0\|1\|stopped\|selected\|null-then-false<\/output>/);
    assert.deepEqual(queryClient.getQueryData(['infinite-fixture', 0, false, { Page: 3 }]).pageParams, [0, 1]);
    const key = ['fixture', 0, false, { Page: 3 }];
    assert.equal(Object.values(queryClient.getQueryData(key))[0], 'fetched');
    assert.equal(queryClient.getQueryData(['fixture', '0', 'false', { Page: 3 }]), undefined);

    client.config.errorHandler = error => { throw error; };
    const mount = window.document.querySelector('main');
    const timeout = AbortSignal.timeout(5000);
    const rendered = new Promise((resolve, reject) => {
        const observer = new window.MutationObserver(() => {
            if (mount.querySelector('#infinite-data')?.textContent === 'first|second|0|1|stopped|selected|null-then-false') {
                observer.disconnect();
                resolve();
            }
        });
        observer.observe(mount, { childList: true, subtree: true, characterData: true });
        timeout.addEventListener('abort', () => {
            observer.disconnect();
            reject(new Error('Vue Query mutation never rendered in the client mount'));
        }, { once: true });
    });
    client.mount(mount);
    await rendered;
    await nextTick();
    assert.equal(mount.querySelector('#query-data')?.textContent, 'fetched');
    assert.equal(mount.querySelector('#mutation-data')?.textContent, 'fixture-saved');
    assert.equal(mount.querySelector('#scalar-data')?.textContent, 'null-then-zero-and-false');
    assert.equal(mount.querySelector('#cache-data')?.textContent, 'missing|zero|fetched|removed|unrelated-false');
    assert.equal(clientQueryClient.getQueryData(['cache-roundtrip', 0]), undefined);
    assert.equal(clientQueryClient.getQueryData(['cache-roundtrip', 0, 'sibling']), false);
    assert.deepEqual(clientQueryClient.getQueryData(['infinite-fixture', 0, false, { Page: 3 }]).pageParams, [0, 1]);
    assert.equal(Object.values(clientQueryClient.getQueryData(key))[0], 'fetched');
    const flags = id => mount.querySelector('#' + id).textContent;
    const click = id => mount.querySelector('#' + id).click();
    async function waitForFlags(id, expected) {
        if (flags(id) === expected)
            return;
        await new Promise((resolve, reject) => {
            const observer = new window.MutationObserver(() => {
                if (flags(id) !== expected)
                    return;
                observer.disconnect();
                clearTimeout(timer);
                resolve();
            });
            const timer = setTimeout(() => {
                observer.disconnect();
                reject(new Error(`Expected ${id} to render ${expected}, received ${flags(id)}`));
            }, 5000);
            observer.observe(mount, { childList: true, subtree: true, characterData: true });
        });
    }

    assert.equal(flags('query-status'), 'pending|-|-|pending|');
    assert.equal(flags('mutation-status'), '-|-|-|idle|');
    click('query-observer');
    queryRequests.requests[0].resolve(0);
    await waitForFlags('query-status', '-|success|-|success|');
    click('query-observer');
    queryRequests.requests[1].reject(new Error('query failed'));
    await waitForFlags('query-status', '-|-|error|error|query failed');

    click('mutation-observer');
    await waitForFlags('mutation-status', 'pending|-|-|pending|');
    mutationRequests.requests[0].resolve(false);
    await waitForFlags('mutation-status', '-|success|-|success|');
    click('mutation-observer');
    await waitForFlags('mutation-status', 'pending|-|-|pending|');
    mutationRequests.requests[1].reject(new Error('mutation failed'));
    await waitForFlags('mutation-status', '-|-|error|error|mutation failed');
    console.log('Vue Query fetched and mutated typed data in SSR and a Vue client mount and rendered live observer state transitions.');
} finally {
    client.unmount();
    clientQueryClient.clear();
    queryClient.clear();
    window.close();
}
