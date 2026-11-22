import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>');
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement'])
        globalThis[name] = window[name];
    const copiedText = [];
    window.document.execCommand = command => {
        assert.equal(command, 'copy');
        copiedText.push(window.document.querySelector('textarea').value);
        return true;
    };
    const { createApp, nextTick } = await import('vue');
    const { default: Clipboard } = await import('./.jolt/Clipboard.mjs');
    const mount = window.document.querySelector('main');
    const app = createApp(Clipboard);
    app.mount(mount);
    const text = () => mount.querySelector('output').textContent;
    assert.equal(text(), '/idle/ready/none');
    mount.querySelector('#copy-text').click();
    await nextTick();
    assert.deepEqual(copiedText, ['plain']);
    assert.equal(text(), 'plain/copied/ready/none');
    mount.querySelector('#copy-promise').click();
    assert.deepEqual(copiedText, ['plain']);
    await Promise.resolve();
    await nextTick();
    assert.deepEqual(copiedText, ['plain', 'promised']);
    assert.equal(text(), 'promised/copied/ready/observed');
    app.unmount();
    console.log('VueUse clipboard copied text and promise results and exposed pending, copied, and text state.');
} finally {
    window.close();
}
