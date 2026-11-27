import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>');
for (const name of ['window', 'document', 'Element', 'SVGElement'])
    globalThis[name] = name === 'window' ? window : window[name];

const { createApp, nextTick } = await import('vue');
const { default: I18nPlugin, MountClient } = await import('./.jazor/I18nPlugin.mjs');
const host = window.document.querySelector('main');
const app = createApp(I18nPlugin);
app.config.errorHandler = error => { throw error; };
const switchLocale = MountClient(app, host);
try {
    assert.match(host.textContent, /^Bonjour Ada \/ Bonjour Lin /);
    assert.equal(host.querySelector('#known-key').textContent, 'present/local');
    switchLocale();
    await nextTick();
    assert.match(host.textContent, /^Welcome Ada \/ Welcome Lin /);
    // The omitted locale searches the fallback chain; specifying English checks only English.
    assert.equal(host.querySelector('#known-key').textContent, 'present/missing');
} finally {
    app.unmount();
    window.close();
}
assert.equal(host.textContent, '');
console.log('Vue I18n mounted, re-rendered after a locale change, and unmounted.');
