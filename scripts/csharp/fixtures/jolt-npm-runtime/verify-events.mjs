import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<button></button><main></main>');
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement', 'MutationObserver'])
        globalThis[name] = window[name];
    const { createApp, nextTick } = await import('vue');
    const { default: Events } = await import('./.jazor/Events.mjs');
    const target = window.document.querySelector('button');
    const listeners = new Set();
    const add = target.addEventListener.bind(target);
    const remove = target.removeEventListener.bind(target);
    target.addEventListener = (type, listener, options) => {
        listeners.add(listener);
        add(type, listener, options);
    };
    target.removeEventListener = (type, listener, options) => {
        listeners.delete(listener);
        remove(type, listener, options);
    };
    const container = window.document.querySelector('main');
    const count = () => container.querySelector('output').textContent;
    const app = createApp(Events, { Target: target });
    app.mount(container);
    await nextTick();
    assert.equal(listeners.size, 3);
    const source = () => container.querySelector('#mouse-source').textContent;
    assert.equal(source(), 'none');
    window.dispatchEvent(new window.MouseEvent('mousemove', { clientX: 20 }));
    target.click();
    target.dispatchEvent(new window.KeyboardEvent('keydown', { key: 'Enter' }));
    target.dispatchEvent(new window.MouseEvent('dblclick', { clientX: 42 }));
    await nextTick();
    assert.equal(count(), '7');
    assert.equal(source(), 'mouse');
    app.unmount();
    assert.equal(listeners.size, 0);
    target.click();
    await nextTick();

    const manual = createApp(Events, { Target: target, Manual: true });
    manual.mount(container);
    await nextTick();
    target.click();
    target.dispatchEvent(new window.KeyboardEvent('keydown', { key: 'Enter' }));
    target.dispatchEvent(new window.MouseEvent('dblclick', { clientX: 42 }));
    await nextTick();
    assert.equal(count(), '0');
    manual.unmount();
    console.log('VueUse registered and dispatched DOM events, stopped on unmount, and exposed callable cleanup.');
} finally {
    window.close();
}
