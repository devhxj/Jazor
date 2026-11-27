import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>', { url: 'https://jolt.example/' });
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement', 'Event', 'StorageEvent', 'CustomEvent', 'Storage'])
        globalThis[name] = window[name];
    window.matchMedia = query => Object.assign(new window.EventTarget(), { media: query, matches: false });
    const { createApp, nextTick } = await import('vue');
    const { default: ColorMode } = await import('./.jazor/ColorMode.mjs');
    const mount = window.document.querySelector('main');
    const root = window.document.documentElement;
    const app = createApp(ColorMode);
    app.mount(mount);
    await nextTick();
    assert.equal(mount.querySelector('output').textContent, 'light/auto/light/light');
    assert.equal(root.getAttribute('data-theme'), 'light');

    mount.querySelector('button').click();
    await nextTick();
    assert.equal(mount.querySelector('output').textContent, 'dark/dark/light/dark');
    assert.equal(root.getAttribute('data-theme'), 'dark');
    app.unmount();
    console.log('VueUse color mode exposed the active value, stored choice, system preference, and resolved state, and updated the DOM.');
} finally {
    window.close();
}
