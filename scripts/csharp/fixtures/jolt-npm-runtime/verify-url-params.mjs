import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>', { url: 'https://jolt.example/' });
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement', 'Event'])
        globalThis[name] = window[name];
    const { createApp, nextTick } = await import('vue');
    const { default: UrlParams } = await import('./.jolt/UrlParams.mjs');
    const root = window.document.querySelector('main');
    const app = createApp(UrlParams);
    app.mount(root);
    for (const [query, expected] of [['', 'absent'], ['?tag=one', 'one'], ['?tag=one&tag=two', 'one/two'], ['', 'absent']]) {
        window.history.replaceState(null, '', '/' + query);
        window.dispatchEvent(new window.PopStateEvent('popstate'));
        await nextTick();
        assert.equal(root.querySelector('output').textContent, expected);
    }
    for (const [button, query, expected] of [
        ['single', '?tag=one', 'one'],
        ['repeated', '?tag=one&tag=two', 'one/two'],
        ['change', '?tag=one&tag=three', 'one/three'],
        ['delete', '', 'absent'],
    ]) {
        root.querySelector(`#${button}`).click();
        await nextTick();
        await nextTick();
        assert.equal(window.location.search, query);
        assert.equal(root.querySelector('output').textContent, expected);
    }
    app.unmount();

} finally {
    window.close();
}

export default 'Verified sparse URL parameter reads, writes, repeated values, and deletion.';
